// Window of the picker: loads the index, filters it and applies the chosen icon to the folder
using System.Diagnostics;
using Foldicon.App.Imaging;
using Foldicon.App.Interop;
using Foldicon.App.Models;
using Foldicon.Core.Folders;
using Foldicon.Core.Icons;
using Foldicon.Core.Indexing;
using Foldicon.Core.Search;
using Foldicon.Core.Storage;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.Windows.ApplicationModel.Resources;
using Windows.Graphics;
using Windows.System;

namespace Foldicon.App;

public sealed partial class MainWindow : Window
{
    // Size of the window in effective pixels
    private const int WindowWidth = 900;
    private const int WindowHeight = 640;

    // Smallest size of the window in effective pixels, which keeps the preview and the buttons usable
    private const int MinimumWidth = 640;
    private const int MinimumHeight = 500;

    // Share of the visible width that the arrows of the tabs scroll
    private const double TabsScrollStep = 0.75;

    private readonly string? folder;
    private readonly ResourceLoader strings = new();
    private readonly FolderIconService folders = FolderIconService.ForSystem();
    private readonly SynonymTable synonyms = SynonymTable.LoadEmbedded();
    private readonly Dictionary<IconReference, IconItem> items = [];
    private readonly Dictionary<IconReference, IconCatalogEntry> catalog =
        IconCatalog.LoadEmbedded().ToDictionary(entry => entry.Icon);
    private IconIndexStore? store;

    // IconResource the folder had before it was restored, for Undo
    private string? previousIcon;

    public MainWindow(string? folder)
    {
        this.folder = folder;
        InitializeComponent();

        // Draws the content under the title bar, over Mica, at a compact size
        Title = strings.GetString("AppDisplayName");
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(TitleBar);
        var scale = NativeDpi.GetDpiForWindow(WinRT.Interop.WindowNative.GetWindowHandle(this)) / 96.0;
        AppWindow.Resize(new SizeInt32((int)(WindowWidth * scale), (int)(WindowHeight * scale)));
        if (AppWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.PreferredMinimumWidth = (int)(MinimumWidth * scale);
            presenter.PreferredMinimumHeight = (int)(MinimumHeight * scale);
        }

        AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets", "Foldicon.ico"));

        // Adds one tab per library after "All"
        foreach (var library in SystemIconLibraries.All)
        {
            LibraryBar.Items.Add(new SelectorBarItem { Text = library, Tag = library });
        }

        // Shows the folder, or explains how to open the app when there is none
        FolderText.Text = folder ?? "";
        if (folder is null || !Directory.Exists(folder))
        {
            ShowResult(InfoBarSeverity.Error, folder is null ? "NoFolderMessage" : "FolderNotFoundMessage", canUndo: false);
            ApplyButton.IsEnabled = false;
            RestoreButton.IsEnabled = false;
        }

        // Listens to the wheel even after the scroll viewer handles it
        LibraryScroller.AddHandler(UIElement.PointerWheelChangedEvent, new PointerEventHandler(OnLibraryWheel), true);

        Root.Loaded += OnLoaded;
        Closed += (_, _) => store?.Dispose();
    }

    // Opens the cache and brings it up to date in the background, then shows every icon
    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            store = await Task.Run(() =>
            {
                var opened = IconIndexStore.OpenDefault();
                IconIndexer.ForSystem(opened).Refresh(SystemIconLibraries.All);
                opened.ReplaceNames(IconCatalog.Terms(catalog.Values));
                return opened;
            });
            Filter();
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or Microsoft.Data.Sqlite.SqliteException)
        {
            Trace.TraceError($"Index failed: {exception}");
            ShowResult(InfoBarSeverity.Error, "IndexFailedMessage", canUndo: false);
        }
        finally
        {
            LoadingRing.IsActive = false;
        }

        SearchBox.Focus(FocusState.Programmatic);
    }

    // Shows the icons that match the search box and the selected tab
    private void Filter()
    {
        if (store is null)
        {
            return;
        }

        // Runs the search with the synonyms of each typed word
        var library = LibraryBar.SelectedItem?.Tag as string;
        var query = SearchQuery.Parse(SearchBox.Text, synonyms);
        var results = store.Search(query, library);

        // Browses the "All" tab in its own order while nothing is being searched
        if (library is null && query.Count == 0)
        {
            results = IconBrowseOrder.Sort(results, catalog);
        }

        // Reuses the items already built, so their images stay loaded
        var shown = results.Select(icon =>
        {
            if (!items.TryGetValue(icon, out var item))
            {
                // Names the icon in the language of the interface, or by its identifier when the catalog lacks it
                var name = catalog.TryGetValue(icon, out var entry)
                    ? entry.NameFor(System.Globalization.CultureInfo.CurrentUICulture)
                    : string.Format(System.Globalization.CultureInfo.CurrentCulture, strings.GetString("IconNameFormat"), icon.ResourceId);
                item = new IconItem(icon, name);
                items[icon] = item;
            }

            return item;
        }).ToList();

        IconGrid.ItemsSource = shown;
        EmptyText.Visibility = shown.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        if (shown.Count > 0)
        {
            IconGrid.SelectedIndex = 0;
        }
    }

    // Loads the image of a cell when the grid draws it for the first time
    private void OnContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
    {
        if (!args.InRecycleQueue && args.Item is IconItem item)
        {
            item.EnsureImage(Root.XamlRoot?.RasterizationScale ?? 1.0);
        }
    }

    // Shows the selected icon at every size of the preview
    private async void OnIconSelected(object sender, SelectionChangedEventArgs e)
    {
        if (IconGrid.SelectedItem is not IconItem item)
        {
            return;
        }

        SelectedName.Text = item.Name;
        SelectedReference.Text = item.Reference;
        var scale = Root.XamlRoot?.RasterizationScale ?? 1.0;
        foreach (var (image, size) in new[] { (Preview256, 256), (Preview48, 48), (Preview32, 32), (Preview24, 24), (Preview16, 16) })
        {
            var source = await IconImageLoader.LoadAsync(item.Icon, (int)Math.Round(size * scale));

            // Stops when another icon was selected while this one was loading
            if (IconGrid.SelectedItem != item)
            {
                return;
            }

            image.Source = source;
        }
    }

    // Applies the selected icon and closes the window, or stays open to show why it failed
    private void ApplySelected()
    {
        if (folder is null || !ApplyButton.IsEnabled || IconGrid.SelectedItem is not IconItem item)
        {
            return;
        }

        if (Run(() => folders.Apply(folder, item.Icon), successMessage: null, canUndo: false))
        {
            Close();
        }
    }

    // Puts back the default icon of the folder and offers to undo it
    private void OnRestoreClick(object sender, RoutedEventArgs e)
    {
        if (folder is null)
        {
            return;
        }

        Run(() =>
        {
            previousIcon = folders.GetCurrent(folder);
            folders.Restore(folder);
        }, "RestoredMessage", canUndo: true);
    }

    // Puts back the icon the folder had before the last change
    private void OnUndoClick(object sender, RoutedEventArgs e)
    {
        if (folder is null)
        {
            return;
        }

        Run(() => folders.Undo(folder, previousIcon), "UndoneMessage", canUndo: false);
    }

    // Runs a change of the folder, reports how it went when there is a message for it, and returns whether it worked
    private bool Run(Action change, string? successMessage, bool canUndo)
    {
        try
        {
            change();
            if (successMessage is not null)
            {
                ShowResult(InfoBarSeverity.Success, successMessage, canUndo);
            }

            return true;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or System.Runtime.InteropServices.COMException)
        {
            Trace.TraceError($"Folder change failed: {exception}");
            ShowResult(InfoBarSeverity.Error, "ApplyFailedMessage", canUndo: false);
            return false;
        }
    }

    // Opens the bar of results with a translated message
    private void ShowResult(InfoBarSeverity severity, string message, bool canUndo)
    {
        ResultBar.Severity = severity;
        ResultBar.Message = strings.GetString(message);
        UndoButton.Visibility = canUndo ? Visibility.Visible : Visibility.Collapsed;
        ResultBar.IsOpen = true;
    }

    private void OnApplyClick(object sender, RoutedEventArgs e) => ApplySelected();

    private void OnIconDoubleTapped(object sender, DoubleTappedRoutedEventArgs e) => ApplySelected();

    private void OnCancelClick(object sender, RoutedEventArgs e) => Close();

    private void OnSearchTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args) => Filter();

    // Shows the icons of the selected tab and brings the tab into view, also when it was reached with the keyboard
    private void OnLibraryChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
    {
        Filter();
        sender.SelectedItem?.StartBringIntoView();
    }

    // Turns the wheel into a sideways scroll over the tabs
    private void OnLibraryWheel(object sender, PointerRoutedEventArgs e)
    {
        var delta = e.GetCurrentPoint(LibraryScroller).Properties.MouseWheelDelta;
        LibraryScroller.ChangeView(LibraryScroller.HorizontalOffset - delta, null, null);
        e.Handled = true;
    }

    private void OnTabsBackClick(object sender, RoutedEventArgs e) => ScrollTabs(-1);

    private void OnTabsForwardClick(object sender, RoutedEventArgs e) => ScrollTabs(1);

    // Moves the tabs by most of the visible width
    private void ScrollTabs(int direction) =>
        LibraryScroller.ChangeView(LibraryScroller.HorizontalOffset + direction * LibraryScroller.ViewportWidth * TabsScrollStep, null, null);

    private void OnLibraryScrollChanged(object? sender, ScrollViewerViewChangedEventArgs e) => UpdateTabArrows();

    private void OnLibraryScrollerSizeChanged(object sender, SizeChangedEventArgs e) => UpdateTabArrows();

    // Shows the arrows only when the tabs do not fit, and enables each one while there is room in its direction
    private void UpdateTabArrows()
    {
        var overflow = LibraryScroller.ScrollableWidth > 0;
        var visibility = overflow ? Visibility.Visible : Visibility.Collapsed;
        TabsBackButton.Visibility = visibility;
        TabsForwardButton.Visibility = visibility;
        TabsBackButton.IsEnabled = LibraryScroller.HorizontalOffset > 0;
        TabsForwardButton.IsEnabled = LibraryScroller.HorizontalOffset < LibraryScroller.ScrollableWidth - 1;
    }

    // Moves to the grid when the search is submitted
    private void OnSearchSubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args) =>
        IconGrid.Focus(FocusState.Keyboard);

    // Moves to the grid with the down arrow
    private void OnSearchKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Down)
        {
            IconGrid.Focus(FocusState.Keyboard);
            e.Handled = true;
        }
    }

    // Applies the selected icon with Enter
    private void OnGridKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter)
        {
            ApplySelected();
            e.Handled = true;
        }
    }

    // Moves to the search box with Ctrl+F
    private void OnFindInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        SearchBox.Focus(FocusState.Keyboard);
        args.Handled = true;
    }

    // Closes the window with Esc
    private void OnEscapeInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        Close();
        args.Handled = true;
    }
}
