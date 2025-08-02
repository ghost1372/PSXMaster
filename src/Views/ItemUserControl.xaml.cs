using System.Diagnostics;
using Microsoft.UI.Input;
using PSXMaster.Database;
using PSXMaster.Database.Tables;
using Windows.ApplicationModel.DataTransfer;
using WinRT.Interop;

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

    private void OnCopyButton(object sender, RoutedEventArgs e)
    {
        CopyData(Game.Link);
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
                if (!db.QueueGames.Any(x => x.GameId.ToLower() == Game.TitleID.ToLower() && x.Title.ToLower() == Game.UrlLastPart.ToLower()))
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

    private void Shield_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        var element = sender as Shield;
        ToolTipService.SetToolTip(element, "Click to Copy");
        GeneralHelper.ChangeCursor(element, InputSystemCursor.Create(InputSystemCursorShape.Hand));
    }

    private void Shield_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        var element = sender as Shield;
        GeneralHelper.ChangeCursor(element, InputSystemCursor.Create(InputSystemCursorShape.Arrow));
    }

    private void Shield_Click(object sender, RoutedEventArgs e)
    {
        var element = sender as Shield;
        CopyData(element?.Status?.ToString());
        GeneralHelper.ChangeCursor(element, InputSystemCursor.Create(InputSystemCursorShape.Arrow));
        ToolTipService.SetToolTip(element, "Copied!");
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
