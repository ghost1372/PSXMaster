namespace PSXMaster.Views;

public sealed partial class GameDetailsPage : Page
{
    public GameDetailsViewModel ViewModel { get; }
    public static GameDetailsPage Instance { get; private set; }
    public GameDetailsPage()
    {
        ViewModel = App.GetService<GameDetailsViewModel>();
        this.InitializeComponent();
        Instance = this;
        _ = webView2.EnsureCoreWebView2Async(null);
        webView2.Visibility = Visibility.Collapsed;
    }

    public void SetSource(Uri source)
    {
        webView2.Source = source;
    }
}
