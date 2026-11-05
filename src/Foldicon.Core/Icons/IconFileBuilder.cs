// Function to rebuild an .ico file from an icon group's resources
using System.Buffers.Binary;

namespace Foldicon.Core.Icons;

public static class IconFileBuilder
{
    // Header sizes: the common start, a group resource entry and an .ico file entry
    private const int HeaderSize = 6;
    private const int GroupEntrySize = 14;
    private const int FileEntrySize = 16;

    // Offset of the image identifier inside a group resource entry
    private const int GroupEntryIdOffset = 12;

    // Returns the .ico file of a group, or null when the group is malformed or one of its images is missing
    public static byte[]? Build(byte[] group, Func<int, byte[]?> imageOf)
    {
        ArgumentNullException.ThrowIfNull(group);
        ArgumentNullException.ThrowIfNull(imageOf);

        // Checks that the group holds as many entries as it says
        if (group.Length < HeaderSize)
        {
            return null;
        }

        var count = BinaryPrimitives.ReadUInt16LittleEndian(group.AsSpan(4));
        if (count == 0 || group.Length < HeaderSize + count * GroupEntrySize)
        {
            return null;
        }

        // Reads the image of each entry
        var images = new byte[count][];
        for (var i = 0; i < count; i++)
        {
            var id = BinaryPrimitives.ReadUInt16LittleEndian(group.AsSpan(HeaderSize + i * GroupEntrySize + GroupEntryIdOffset));
            var image = imageOf(id);
            if (image is null)
            {
                return null;
            }

            images[i] = image;
        }

        // Writes the header, which is the same in both formats
        var directorySize = HeaderSize + count * FileEntrySize;
        var file = new byte[directorySize + images.Sum(image => image.Length)];
        group.AsSpan(0, HeaderSize).CopyTo(file);

        // Writes each entry, whose identifier becomes the offset of its image
        var offset = directorySize;
        for (var i = 0; i < count; i++)
        {
            var entry = file.AsSpan(HeaderSize + i * FileEntrySize);
            group.AsSpan(HeaderSize + i * GroupEntrySize, GroupEntryIdOffset).CopyTo(entry);
            BinaryPrimitives.WriteUInt32LittleEndian(entry[GroupEntryIdOffset..], (uint)offset);
            images[i].CopyTo(file, offset);
            offset += images[i].Length;
        }

        return file;
    }
}
