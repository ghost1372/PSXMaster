using System.Diagnostics;
using Microsoft.UI.Xaml.Media;
using PSXMaster.Common;
using PSXMaster.Database;
using PSXMaster.Database.Tables;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI;

namespace PSXMaster.Views;

public sealed partial class ItemUserControl : UserControl
{
    public Models.LogModel Game
    {
        get { return (Models.LogModel)GetValue(GameProperty); }
        set { SetValue(GameProperty, value); }
    }

    public static readonly DependencyProperty GameProperty =
        DependencyProperty.Register(nameof(Game), typeof(Models.LogModel), typeof(ItemUserControl), new PropertyMetadata(null));

    public ItemUserControl()
    {
        this.InitializeComponent();
    }

    // ---- x:Bind helpers ----

    private static bool FileExists(string? path) => !string.IsNullOrWhiteSpace(path) && File.Exists(path);

    public static Visibility FoundVisibility(string? path) => FileExists(path) ? Visibility.Visible : Visibility.Collapsed;

    public static Visibility MissingVisibility(string? path) => FileExists(path) ? Visibility.Collapsed : Visibility.Visible;

    public static string GetConsoleText(object? console) => console?.ToString() ?? string.Empty;

    public static Brush GetConsoleBrush(object? console) => GetConsoleText(console).ToUpperInvariant() switch
    {
        "PS5" => new SolidColorBrush(Color.FromArgb(255, 0x1D, 0x3B, 0x8F)),
        "PS4" => new SolidColorBrush(Color.FromArgb(255, 0x0B, 0x5C, 0x7A)),
        _ => new SolidColorBrush(Color.FromArgb(255, 0x45, 0x45, 0x45))
    };

    // ---- Handlers ----

    private void OnCopyButton(object sender, RoutedEventArgs e)
    {
        CopyData(Game.Link);
    }

    private void OnCopyTitleId(object sender, RoutedEventArgs e)
    {
        CopyData(Game.TitleID);
        ToolTipService.SetToolTip((DependencyObject)sender, "Copied!");
    }

    private void OnTitleIdPointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        ToolTipService.SetToolTip((DependencyObject)sender, "Click to copy");
    }

    private void OnShowInFolder(object sender, RoutedEventArgs e)
    {
        if (!FileExists(Game.FilePath))
        {
            return;
        }

        Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{Game.FilePath}\"") { UseShellExecute = true });
    }

    private async void OnFavoriteButton(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(Game.TitleID) || string.IsNullOrEmpty(Game.UrlLastPart))
        {
            await MessageBox.ShowAsync("GameId or Title is null or empty");
            return;
        }

        try
        {
            using (var db = new PSXMasterDbContext())
            {
                if (!await Queries.GetGameByIdAndTitleQueryAsync(db, Game.TitleID, Game.UrlLastPart).AnyAsync())
                {
                    var game = new QueueGames
                    {
                        GameId = Game.TitleID,
                        Link = Game.Link,
                        Title = Game.UrlLastPart
                    };

                    await db.QueueGames.AddAsync(game);
                    await db.SaveChangesAsync();
                }
            }
        }
        catch (Exception ex)
        {
            Logger?.Error(ex.Message);
            await MessageBox.ShowAsync(ex.Message);
        }
    }

    private void OnDownloadButton(object sender, RoutedEventArgs e)
    {
        string idm32 = @"C:\Program Files\Internet Download Manager\IDMan.exe";
        string idm64 = @"C:\Program Files (x86)\Internet Download Manager\IDMan.exe";
        if (System.Environment.Is64BitOperatingSystem)
        {
            Process.Start(idm64, $"/d {Game.Link} /a");
        }
        else
        {
            Process.Start(idm32, $"/d {Game.Link} /a");
        }
    }

    public void CopyData(string data)
    {
        if (string.IsNullOrEmpty(data))
        {
            return;
        }

        var dataPackage = new DataPackage();
        dataPackage.SetText(data);
        Clipboard.SetContent(dataPackage);
    }
}
