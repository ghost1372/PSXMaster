using PSXMaster.Common;
using PSXMaster.Models;

namespace PSXMaster.Views;

public sealed partial class GeneralSettingPage : Page
{
    public GeneralSettingViewModel ViewModel { get; }
    public GeneralSettingPage()
    {
        ViewModel = App.GetService<GeneralSettingViewModel>();
        this.InitializeComponent();
    }

    private async void NavigateToLogPath_Click(object sender, RoutedEventArgs e)
    {
        string folderPath = (sender as HyperlinkButton).Content.ToString();
        if (Directory.Exists(folderPath))
        {
            Windows.Storage.StorageFolder folder = await Windows.Storage.StorageFolder.GetFolderFromPathAsync(folderPath);
            await Windows.System.Launcher.LaunchFolderAsync(folder);
        }
    }

    private void ComboBox_Loaded(object sender, RoutedEventArgs e)
    {
        var cmb = sender as ComboBox;
        if (cmb != null)
        {
            cmb.SelectedItem = cmb.Items.Where(x=> ((BufferModel)x).Name.Equals(Settings.TransferBuffer.Name)).FirstOrDefault();
        }
    }
}


