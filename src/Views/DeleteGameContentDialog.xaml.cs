
using PSXMaster.Database;

namespace PSXMaster.Views;
public sealed partial class DeleteGameContentDialog : ContentDialog
{
    internal List<int> _selectedItems;
    public DeleteGameContentDialog()
    {
        XamlRoot = App.MainWindow.Content.XamlRoot;

        this.InitializeComponent();

        var service = App.GetService<IThemeService>();
        RequestedTheme = service.GetElementTheme();
    }

    private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        OnPrimaryButtonInvoked();
    }

    private async void OnPrimaryButtonInvoked()
    {
        try
        {
            using (var db = new PSXMasterDbContext())
            {
                foreach (var item in _selectedItems)
                {
                    var delete = await db.QueueGames.FindAsync(item);
                    if (delete != null)
                    {
                        db.QueueGames.Remove(delete);
                    }
                }
                await db.SaveChangesAsync();
                Growl.SuccessGlobal("Selected Items Deleted");
            }
        }
        catch (Exception ex)
        {
            Logger?.Error(ex.Message);
            Growl.ErrorGlobal("Error", ex.Message);
        }
    }

    private void OnEnterInvoked(Microsoft.UI.Xaml.Input.KeyboardAccelerator sender, Microsoft.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
    {
        OnPrimaryButtonInvoked();
        this.Hide();
    }
}
