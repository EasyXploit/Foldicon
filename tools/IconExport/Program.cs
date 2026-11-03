// Entry of the tool: exports the icons of every system library and draws the contact sheets used to name them
//
// Usage: dotnet run --project tools/IconExport -p:Platform=x64 [-- <output folder>]
// The default output folder is tools/IconExport/out, which git ignores.
using Foldicon.Core.Icons;
using Foldicon.Core.Indexing;
using Foldicon.Tools.IconExport;

var output = args.Length > 0 ? args[0] : Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "out");
output = Path.GetFullPath(output);
var windows = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
var reader = new NativeGroupIconReader();
var duplicates = new DuplicateFinder();

foreach (var library in SystemIconLibraries.All)
{
    // Lists the icon groups of the library
    var path = ResourceFileResolver.Resolve(library, windows, File.Exists);
    var icons = reader.ReadGroupIconIds(path).Select(id => new IconReference(library, id)).ToList();

    // Writes one PNG per icon and notes its pixels to find the repeated ones
    var exported = IconPngWriter.WriteAll(icons, Path.Combine(output, "icons", Path.GetFileNameWithoutExtension(library)));
    foreach (var (icon, pixels) in exported)
    {
        duplicates.Add(icon, pixels);
    }

    // Draws the contact sheets of the library
    var sheets = ContactSheet.DrawAll(library, exported, Path.Combine(output, "sheets"));
    Console.WriteLine($"{library}: {icons.Count} icons, {exported.Count} exported, {sheets} sheets");
}

// Lists the groups of icons that look exactly the same at 48 px
File.WriteAllLines(Path.Combine(output, "duplicates.txt"), duplicates.Groups());
Console.WriteLine($"Output in {output}");
