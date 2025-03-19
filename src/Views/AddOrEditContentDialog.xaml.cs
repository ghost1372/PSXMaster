using PSXMaster.Database;
using PSXMaster.Database.Tables;

namespace PSXMaster.Views;
public sealed partial class AddOrEditContentDialog : ContentDialog
{
    public string GameId
    {
        get { return (string)GetValue(GameIdProperty); }
        set { SetValue(GameIdProperty, value); }
    }

    public static readonly DependencyProperty GameIdProperty =
        DependencyProperty.Register(nameof(GameId), typeof(string), typeof(AddOrEditContentDialog), new PropertyMetadata(null));

    public string GameTitle
    {
        get { return (string)GetValue(GameTitleProperty); }
        set { SetValue(GameTitleProperty, value); }
    }

    public static readonly DependencyProperty GameTitleProperty =
        DependencyProperty.Register(nameof(GameTitle), typeof(string), typeof(AddOrEditContentDialog), new PropertyMetadata(null));

    public string Link
    {
        get { return (string)GetValue(LinkProperty); }
        set { SetValue(LinkProperty, value); }
    }

    public static readonly DependencyProperty LinkProperty =
        DependencyProperty.Register(nameof(Link), typeof(string), typeof(AddOrEditContentDialog), new PropertyMetadata(null));

    internal int baseItemId;
    internal DialogType _dialogType;
    public AddOrEditContentDialog(DialogType dialogType)
    {
        XamlRoot = App.MainWindow.Content.XamlRoot;

        this.InitializeComponent();

        this._dialogType = dialogType;
        var service = App.GetService<IThemeService>();
        RequestedTheme = service.GetElementTheme();
        if (dialogType == DialogType.Add)
        {
            Title = "Add New Game";
            PrimaryButtonText = "Save [Enter]";
        }
        else
        {
            Title = "Update Game";
            PrimaryButtonText = "Update [Enter]";
        }
    }

    private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        OnPrimaryButtonInvoked();
    }

    private void OnEnterInvoked(Microsoft.UI.Xaml.Input.KeyboardAccelerator sender, Microsoft.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
    {
        OnPrimaryButtonInvoked();
    }

    private async void OnPrimaryButtonInvoked()
    {
        if (string.IsNullOrEmpty(this.GameId) || string.IsNullOrEmpty(this.GameTitle))
        {
            Growl.ErrorGlobal("GameId or Title is null or empty");
            return;
        }

        try
        {
            if (_dialogType == DialogType.Add)
            {
                using (var db = new PSXMasterDbContext())
                {
                    if (!db.QueueGames.Any(x => x.GameId.ToLower() == this.GameId.ToLower() && x.Title.ToLower() == this.GameTitle.ToLower()))
                    {
                        var game = new QueueGames
                        {
                            GameId = this.GameId,
                            Title = this.GameTitle,
                            Link = this.Link
                        };

                        await db.QueueGames.AddAsync(game);
                        await db.SaveChangesAsync();
                        Growl.SuccessGlobal($"{GameId} Added");

                        GameId = string.Empty;
                        GameTitle = string.Empty;
                        Link = string.Empty;
                    }
                }
            }
            else
            {
                using (var db = new PSXMasterDbContext())
                {
                    var game = await db.QueueGames.FindAsync(baseItemId);
                    if (game != null)
                    {
                        game.Title = this.GameTitle;
                        game.Link = this.Link;
                        game.GameId = this.GameId;

                        await db.SaveChangesAsync();
                        this.Hide();
                        Growl.SuccessGlobal($"{GameId} Updated");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger?.Error(ex.Message);
            Growl.ErrorGlobal("Error", ex.Message);
        }
    }
}
public enum DialogType
{
    Add,
    Edit
}
