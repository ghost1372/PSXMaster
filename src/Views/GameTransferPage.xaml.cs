using Windows.System;

namespace PSXMaster.Views;

public sealed partial class GameTransferPage : Page
{
    public GameTransferViewModel ViewModel { get; }
    public static GameTransferPage Instance { get; private set; }
    public GameTransferPage()
    {
        ViewModel = App.GetService<GameTransferViewModel>();
        this.InitializeComponent();
        Instance = this;
    }

    private async void OnLaunchFolder(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(Settings.LocalFileDirectory) || !Directory.Exists(Settings.LocalFileDirectory))
        {
            ShowMessage("directory not found", "Please select a valid directory", InfoBarSeverity.Error);
            return;
        }

        await Launcher.LaunchFolderPathAsync(Settings.LocalFileDirectory);
    }

    public void ShowMessage(string title, string message, InfoBarSeverity severity)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            MainStatus.Title = title;
            MainStatus.Message = message;
            MainStatus.Severity = severity;
            MainStatus.IsOpen = true;
        });
    }

    private async void BtnChangeDirectory_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FolderPicker(App.MainWindow);
        var result = await picker.PickSingleFolderAsync();
        if (result != null)
        {
            Settings.LocalFileDirectory = result.Path;
            WorkingDirectoryInfo.Message = result.Path;
        }
    }

    private void ComboBox_Loaded(object sender, RoutedEventArgs e)
    {
        var cmb = sender as ComboBox;
        cmb.SelectedValue = Settings.BufferSize;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
#if DEBUG
        DebugButton.Visibility = Visibility.Visible;
#else
        DebugButton.Visibility = Visibility.Collapsed;
#endif
    }
}
