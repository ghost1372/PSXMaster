using PSXMaster.Database;
using PSXMaster.Services;

namespace PSXMaster;

public partial class App : Application
{
    public static Window MainWindow = Window.Current;
    public new static App Current => (App)Application.Current;
    public IServiceProvider Services { get; }
    public IJsonNavigationService NavService => GetService<IJsonNavigationService>();
    public IThemeService ThemeService => GetService<IThemeService>();

    public static T GetService<T>() where T : class
    {
        if ((App.Current as App)!.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service;
    }

    public App()
    {
        Services = ConfigureServices();
        this.InitializeComponent();

        DbBootstrapper.EnsureDatabaseExists();

        if (!Directory.Exists(Settings.LocalFileDirectory))
        {
            Directory.CreateDirectory(Settings.LocalFileDirectory);
        }
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IThemeService, ThemeService>();
        services.AddSingleton<IJsonNavigationService, JsonNavigationService>();
        services.AddSingleton<IPSXMasterService, PSXMasterService>();

        services.AddTransient<GameDetailsViewModel>();
        services.AddTransient<GameTransferViewModel>();
        services.AddTransient<QueueGamesViewModel>();
        services.AddSingleton<ContextMenuService>();
        services.AddTransient<GeneralSettingViewModel>();
        services.AddTransient<AppUpdateSettingViewModel>();
        services.AddTransient<AboutUsSettingViewModel>();

        return services.BuildServiceProvider();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainWindow = new MainWindow();

        if (ThemeService != null)
        {
            ThemeService.AutoInitialize(MainWindow);
        }

        MainWindow.Title = MainWindow.AppWindow.Title = ProcessInfoHelper.ProductNameAndVersion;
        MainWindow.AppWindow.SetIcon("Assets/AppIcon.ico");

        MainWindow.Activate();

        InitializeApp();
    }

    private async void InitializeApp()
    {
        var menuService = GetService<ContextMenuService>();
        if (menuService != null && RuntimeHelper.IsPackaged())
        {
            ContextMenuItem menu = new ContextMenuItem
            {
                Title = "Open PSXMaster Here",
                Param = @"""{path}""",
                AcceptFileFlag = (int)FileMatchFlagEnum.ExtList,
                AcceptDirectoryFlag = (int)(DirectoryMatchFlagEnum.Directory | DirectoryMatchFlagEnum.Background | DirectoryMatchFlagEnum.Desktop),
                AcceptMultipleFilesFlag = (int)FilesMatchFlagEnum.Each,
                AcceptExts = ".pkg|.pup",
                Index = 0,
                Enabled = true,
                Icon = ProcessInfoHelper.GetFileVersionInfo().FileName,
                Exe = "PSXMaster.exe"
            };

            await menuService.SaveAsync(menu);
        }

        if (Settings.UseDeveloperMode)
        {
            ConfigureLogger();
        }

        UnhandledException += (s, e) => Logger?.Error(e.Exception, "UnhandledException");
    }
}

