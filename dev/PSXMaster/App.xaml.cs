using PSXMaster.Common;
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
        services.AddTransient<GeneralSettingViewModel>();
        services.AddTransient<AppUpdateSettingViewModel>();
        services.AddTransient<AboutUsSettingViewModel>();

        return services.BuildServiceProvider();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainWindow = new MainWindow();

        string filename = Constants.DatabaseFilePath;
        if (!string.IsNullOrEmpty(Settings.DBPath))
        {
            filename = $"{Settings.DBPath}";
        }

        DbBootstrapper.EnsureDatabaseExists(filename);

        PSXMasterDbContext.DatabasePath = filename;

        if (ThemeService != null)
        {
            ThemeService.Initialize(MainWindow);
        }

#if DEBUG
        ProcessInfoHelper.IsDebug = true;
#else
        ProcessInfoHelper.IsDebug = false;
#endif
        MainWindow.Title = MainWindow.AppWindow.Title = ProcessInfoHelper.ProductNameAndVersion;

        MainWindow.AppWindow.SetIcon("Assets/AppIcon.ico");
        MainWindow.AppWindow.SetTaskbarIcon("Assets/AppIcon.ico");

        MainWindow.Activate();

        InitializeApp();
    }

    private async void InitializeApp()
    {
        if (Settings.UseDeveloperMode)
        {
            ConfigureLogger();
        }

        UnhandledException += (s, e) => Logger?.Error(e.Exception, "UnhandledException");
    }
}

