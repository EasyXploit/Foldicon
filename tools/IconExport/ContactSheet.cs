// Function to draw grids of icons with their identifier under each one, to look at them and name them
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using Foldicon.Core.Icons;

namespace Foldicon.Tools.IconExport;

internal static class ContactSheet
{
    // Layout of each sheet: 10 × 6 cells of 104 × 84 px under a header
    private const int Columns = 10;
    private const int Rows = 6;
    private const int CellWidth = 104;
    private const int CellHeight = 84;
    private const int Header = 32;

    // Draws as many sheets as the icons need and returns how many it drew
    public static int DrawAll(string library, IReadOnlyList<(IconReference Icon, byte[] Pixels)> icons, string folder)
    {
        Directory.CreateDirectory(folder);
        var perSheet = Columns * Rows;
        var count = (icons.Count + perSheet - 1) / perSheet;
        for (var sheet = 0; sheet < count; sheet++)
        {
            var page = icons.Skip(sheet * perSheet).Take(perSheet).ToList();
            using var bitmap = Draw($"{library} · {sheet + 1}/{count}", page);
            bitmap.Save(Path.Combine(folder, $"{Path.GetFileNameWithoutExtension(library)}-{sheet + 1:00}.png"), ImageFormat.Png);
        }

        return count;
    }

    // Draws one sheet on a light gray background, so both dark and white icons stand out
    private static Bitmap Draw(string title, List<(IconReference Icon, byte[] Pixels)> icons)
    {
        var bitmap = new Bitmap(Columns * CellWidth, Header + Rows * CellHeight);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.FromArgb(225, 225, 225));
        graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        using var titleFont = new Font("Segoe UI", 13, FontStyle.Bold, GraphicsUnit.Pixel);
        using var labelFont = new Font("Segoe UI", 14, FontStyle.Bold, GraphicsUnit.Pixel);
        graphics.DrawString(title, titleFont, Brushes.Black, 8, 7);

        // Draws each icon centered in its cell, with its identifier under it
        for (var i = 0; i < icons.Count; i++)
        {
            var x = i % Columns * CellWidth;
            var y = Header + i / Columns * CellHeight;
            using var icon = IconPngWriter.ToBitmap(icons[i].Pixels, IconPngWriter.Size);
            graphics.DrawRectangle(Pens.Silver, x, y, CellWidth - 1, CellHeight - 1);
            graphics.DrawImage(icon, x + (CellWidth - IconPngWriter.Size) / 2, y + 6);
            var label = $"-{icons[i].Icon.ResourceId}";
            var width = graphics.MeasureString(label, labelFont).Width;
            graphics.DrawString(label, labelFont, Brushes.Black, x + (CellWidth - width) / 2, y + 60);
        }

        return bitmap;
    }
}
