using System.Diagnostics;
using Microsoft.Windows.Storage.Pickers;

namespace PSXMaster.ViewModels;
public partial class GeneralSettingViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string DatabaseFolderPath { get; set; } = Settings.DBPath;

    [ObservableProperty]
    public partial string LocalFileFolderPath { get; set; } = Settings.LocalFileDirectory;

    [RelayCommand]
    public async Task OnChooseLocalFileFolderPathAsync()
    {
        var picker = new FolderPicker(App.MainWindow.AppWindow.Id);
        var result = await picker.PickSingleFolderAsync();
        if (result is not null)
        {
            Settings.LocalFileDirectory = result.Path;
            LocalFileFolderPath = result.Path;
        }
    }

    [RelayCommand]
    public void OnLaunchLocalFileFolderPath()
    {
        Process.Start("explorer.exe", $"/select, {Settings.LocalFileDirectory}");
    }

    [RelayCommand]
    public async Task OnChooseDatabasePathAsync()
    {
        var picker = new FileOpenPicker(App.MainWindow.AppWindow.Id);
        picker.FileTypeFilter.Add(".db");
        var result = await picker.PickSingleFileAsync();
        if (result is not null)
        {
            Settings.DBPath = result.Path;
            DatabaseFolderPath = result.Path;
        }
    }

    [RelayCommand]
    public void OnLaunchDatabasePath()
    {
        Process.Start("explorer.exe", $"/select, {Settings.DBPath}");
    }
}
