// SQLite cache of the icon groups of each library and of the words they can be searched by
using System.Globalization;
using Foldicon.Core.Icons;
using Foldicon.Core.Indexing;
using Microsoft.Data.Sqlite;

namespace Foldicon.Core.Storage;

public sealed class IconIndexStore : IDisposable
{
    // Version of the schema; a cache written with another one is dropped and rebuilt
    private const int SchemaVersion = 2;

    private readonly SqliteConnection connection;

    // Opens the cache at the given connection string and prepares its schema
    public IconIndexStore(string connectionString)
    {
        connection = new SqliteConnection(connectionString);
        connection.Open();
        EnsureSchema();
    }

    // Opens the cache file of the user
    public static IconIndexStore OpenDefault() => Open(IndexCachePath.Get());

    // Opens a cache file, whose path may hold any character
    public static IconIndexStore Open(string path) => new(new SqliteConnectionStringBuilder { DataSource = path }.ToString());

    // Creates the tables, or drops them first when they come from another version of the schema
    private void EnsureSchema()
    {
        // Reads the version stored in the file
        var version = Convert.ToInt32(Scalar("PRAGMA user_version"), CultureInfo.InvariantCulture);

        // Drops a cache of another version, since it can always be regenerated
        if (version != SchemaVersion)
        {
            Execute("DROP TABLE IF EXISTS names; DROP TABLE IF EXISTS icons; DROP TABLE IF EXISTS libraries;");
        }

        // Creates the tables when they are missing
        Execute($"""
            CREATE TABLE IF NOT EXISTS libraries (
                library TEXT PRIMARY KEY,
                length INTEGER NOT NULL,
                last_write INTEGER NOT NULL);
            CREATE TABLE IF NOT EXISTS icons (
                library TEXT NOT NULL,
                resource_id INTEGER NOT NULL,
                PRIMARY KEY (library, resource_id));
            CREATE TABLE IF NOT EXISTS names (
                library TEXT NOT NULL,
                resource_id INTEGER NOT NULL,
                term TEXT NOT NULL);
            CREATE INDEX IF NOT EXISTS names_by_icon ON names (library, resource_id, term);
            PRAGMA user_version = {SchemaVersion};
            """);
    }

    // Returns the stamp of the file a library was last read from, or null if it was never indexed
    public LibraryStamp? GetStamp(string library)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT length, last_write FROM libraries WHERE library = $library";
        command.Parameters.AddWithValue("$library", library);
        using var reader = command.ExecuteReader();
        return reader.Read() ? new LibraryStamp(reader.GetInt64(0), reader.GetInt64(1)) : null;
    }

    // Replaces the icons cached for a library in one transaction
    public void ReplaceLibrary(LibraryIcons icons)
    {
        ArgumentNullException.ThrowIfNull(icons);
        using var transaction = connection.BeginTransaction();

        // Drops the previous icons of the library
        Execute("DELETE FROM icons WHERE library = $library", ("$library", icons.Library));

        // Stores each icon group
        using (var insert = connection.CreateCommand())
        {
            insert.CommandText = "INSERT OR IGNORE INTO icons (library, resource_id) VALUES ($library, $id)";
            insert.Parameters.AddWithValue("$library", icons.Library);
            var id = insert.Parameters.Add("$id", SqliteType.Integer);
            foreach (var resourceId in icons.ResourceIds)
            {
                id.Value = resourceId;
                insert.ExecuteNonQuery();
            }
        }

        // Stores the stamp of the file
        Execute(
            "INSERT OR REPLACE INTO libraries (library, length, last_write) VALUES ($library, $length, $lastWrite)",
            ("$library", icons.Library),
            ("$length", icons.Stamp.Length),
            ("$lastWrite", icons.Stamp.LastWriteTicks));

        transaction.Commit();
    }

    // Replaces every search word of the icons with the given ones, already normalized
    public void ReplaceNames(IEnumerable<(IconReference Icon, string Term)> names)
    {
        ArgumentNullException.ThrowIfNull(names);
        using var transaction = connection.BeginTransaction();
        Execute("DELETE FROM names");

        // Stores each word of each icon
        using var insert = connection.CreateCommand();
        insert.CommandText = "INSERT INTO names (library, resource_id, term) VALUES ($library, $id, $term)";
        var library = insert.Parameters.Add("$library", SqliteType.Text);
        var id = insert.Parameters.Add("$id", SqliteType.Integer);
        var term = insert.Parameters.Add("$term", SqliteType.Text);
        foreach (var name in names)
        {
            library.Value = name.Icon.Library;
            id.Value = name.Icon.ResourceId;
            term.Value = name.Term;
            insert.ExecuteNonQuery();
        }

        transaction.Commit();
    }

    // Returns the icons of one library, or of all of them when it is null, matching every group of words: an icon
    // matches a group when one of its words, its library or its identifier starts with one of the words of the group
    public IReadOnlyList<IconReference> Search(IReadOnlyList<IReadOnlyList<string>> termGroups, string? library)
    {
        ArgumentNullException.ThrowIfNull(termGroups);
        using var command = connection.CreateCommand();
        var conditions = new List<string>();

        // Narrows the search to one library
        if (library is not null)
        {
            conditions.Add("i.library = $library");
            command.Parameters.AddWithValue("$library", library);
        }

        // Adds one condition per group of words, with one parameter per word
        for (var g = 0; g < termGroups.Count; g++)
        {
            var alternatives = new List<string>();
            for (var t = 0; t < termGroups[g].Count; t++)
            {
                var parameter = $"$t{g}_{t}";
                command.Parameters.AddWithValue(parameter, EscapeLike(termGroups[g][t]) + "%");
                alternatives.Add(
                    "EXISTS (SELECT 1 FROM names n WHERE n.library = i.library AND n.resource_id = i.resource_id " +
                    $"AND n.term LIKE {parameter} ESCAPE '\\') " +
                    $"OR i.library LIKE {parameter} ESCAPE '\\' " +
                    $"OR CAST(i.resource_id AS TEXT) LIKE {parameter} ESCAPE '\\'");
            }

            conditions.Add("(" + string.Join(" OR ", alternatives) + ")");
        }

        // Runs the query, ordered by the identifiers inside each library
        var where = conditions.Count == 0 ? "" : "WHERE " + string.Join(" AND ", conditions);
        command.CommandText = $"SELECT i.library, i.resource_id FROM icons i {where} ORDER BY i.resource_id";
        var results = new List<IconReference>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            results.Add(new IconReference(reader.GetString(0), reader.GetInt32(1)));
        }

        // Keeps the libraries in the order of the tabs, which a stable sort leaves inside each one by identifier
        return results.OrderBy(icon => IconLibraryTabs.Rank(icon.Library)).ToList();
    }

    // Escapes the wildcards of LIKE in a word typed by the user
    private static string EscapeLike(string value) =>
        value.Replace(@"\", @"\\", StringComparison.Ordinal)
            .Replace("%", @"\%", StringComparison.Ordinal)
            .Replace("_", @"\_", StringComparison.Ordinal);

    // Runs a statement without results
    private void Execute(string sql, params (string Name, object Value)[] parameters)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        foreach (var (name, value) in parameters)
        {
            command.Parameters.AddWithValue(name, value);
        }

        command.ExecuteNonQuery();
    }

    // Runs a query that returns one value
    private object? Scalar(string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        return command.ExecuteScalar();
    }

    // Closes the connection
    public void Dispose() => connection.Dispose();
}
