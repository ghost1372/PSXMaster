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

    private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        ViewModel.QuerySubmittedCommand.Execute(null);
    }

    private void webView2_NavigationCompleted(WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs args)
    {
        ViewModel.NavigationCompletedCommand.Execute(sender);
    }
}
