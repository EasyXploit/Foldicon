// Icon shown in the grid, whose image is loaded only when its cell is first drawn
using System.ComponentModel;
using Foldicon.App.Imaging;
using Foldicon.Core.Icons;
using Microsoft.UI.Xaml.Media;

namespace Foldicon.App.Models;

public sealed partial class IconItem(IconReference icon, string name) : INotifyPropertyChanged
{
    // Size of the image of the grid in effective pixels
    public const int GridSize = 32;

    private ImageSource? image;
    private bool loading;

    public event PropertyChangedEventHandler? PropertyChanged;

    public IconReference Icon { get; } = icon;

    public string Name { get; } = name;

    // Text shown under the name, as in "imageres.dll,-3"
    public string Reference => Icon.ToString();

    // Text of the tooltip of the cell: the name over the reference
    public string ToolTip => $"{Name}{Environment.NewLine}{Reference}";

    public ImageSource? Image
    {
        get => image;
        private set
        {
            image = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Image)));
        }
    }

    // Loads the image once, at the scale of the screen
    public async void EnsureImage(double scale)
    {
        if (loading)
        {
            return;
        }

        loading = true;
        Image = await IconImageLoader.LoadAsync(Icon, (int)Math.Round(GridSize * scale));
    }
}
