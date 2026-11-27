using Microsoft.Windows.Storage.Pickers;
using PSXMaster.Models;
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

    public static Visibility ToVisibility(bool value) => value ? Visibility.Visible : Visibility.Collapsed;

    public static Visibility ToInverseVisibility(bool value) => value ? Visibility.Collapsed : Visibility.Visible;

    public static string GetStatusText(bool isConnected) => isConnected ? "Connected" : "Disconnected";

    public static string GetEndpoint(object? address, double port)
    {
        var host = address as string;
        return string.IsNullOrWhiteSpace(host) ? "No address selected" : $"{host}:{(int)port}";
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
        var picker = new FolderPicker(App.MainWindow.AppWindow.Id);
        var result = await picker.PickSingleFolderAsync();
        if (result != null)
        {
            Settings.LocalFileDirectory = result.Path;
            UpdateWorkingDirectory(result.Path);
        }
    }

    // Shows the parent path in secondary text and the last folder name in bold.
    private void UpdateWorkingDirectory(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            DirectoryParentRun.Text = "No directory selected";
            DirectoryNameRun.Text = string.Empty;
            return;
        }

        var trimmed = path.TrimEnd('\\', '/');
        var name = Path.GetFileName(trimmed);

        if (string.IsNullOrEmpty(name))
        {
            // Drive root such as "C:\"
            DirectoryParentRun.Text = string.Empty;
            DirectoryNameRun.Text = path;
            return;
        }

        DirectoryParentRun.Text = trimmed[..^name.Length];
        DirectoryNameRun.Text = name;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        UpdateWorkingDirectory(Settings.LocalFileDirectory);
#if DEBUG
        MoreButton.Visibility = Visibility.Visible;
#else
        // The overflow menu currently only holds the debug action.
        MoreButton.Visibility = Visibility.Collapsed;
#endif
    }

    private void ComboBox_Loaded(object sender, RoutedEventArgs e)
    {
        var cmb = sender as ComboBox;
        if (cmb != null)
        {
            cmb.SelectedItem = cmb.Items.Where(x => ((BufferModel)x).Name.Equals(Settings.TransferBuffer.Name)).FirstOrDefault();
        }
    }
}
