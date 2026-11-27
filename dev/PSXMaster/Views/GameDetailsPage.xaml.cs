using System.Numerics;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Input;
using Microsoft.Web.WebView2.Core;

namespace PSXMaster.Views;

public sealed partial class GameDetailsPage : Page
{
    // How much larger than its frame the cover is drawn. The extra area is what the parallax moves through.
    private const float ParallaxScale = 1.12f;
    private const float CoverCornerRadius = 8f;

    private Compositor _compositor;
    private Visual _hostVisual;
    private Visual _coverVisual;

    public GameDetailsViewModel ViewModel { get; }
    public static GameDetailsPage Instance { get; private set; }

    public GameDetailsPage()
    {
        ViewModel = App.GetService<GameDetailsViewModel>();
        this.InitializeComponent();
        Instance = this;
        SetupParallax();
        _ = webView2.EnsureCoreWebView2Async(null);
        webView2.Visibility = Visibility.Collapsed;
    }

    // Used by x:Bind: show the "look up a game" hint only when nothing else is on screen
    public static Visibility ShowHint(Visibility detailsVisibility, bool isBusy, bool infoOpen)
        => detailsVisibility == Visibility.Collapsed && !isBusy && !infoOpen
            ? Visibility.Visible
            : Visibility.Collapsed;

    public void SetSource(Uri source)
    {
        // Setting the same Source again does not navigate, which left the spinner running forever.
        if (webView2.CoreWebView2 != null && webView2.Source == source)
        {
            webView2.Reload();
        }
        else
        {
            webView2.Source = source;
        }
    }

    private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        ViewModel.QuerySubmittedCommand.Execute(null);
    }

    private void webView2_NavigationCompleted(WebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
    {
        if (!args.IsSuccess)
        {
            ViewModel.IsProcessActive = false;
            ViewModel.ShowInfo("Error", $"Could not load the page ({args.WebErrorStatus}).", InfoBarSeverity.Error);
            return;
        }

        // Ignore the initial blank page
        if (sender.Source == null || sender.Source.AbsoluteUri.StartsWith("about:", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        ViewModel.NavigationCompletedCommand.Execute(sender);
    }

    // ---- Cover banner: sizing, rounded clip, parallax ---------------------------------------

    private void SetupParallax()
    {
        _hostVisual = ElementCompositionPreview.GetElementVisual(CoverHost);
        _coverVisual = ElementCompositionPreview.GetElementVisual(CoverImage);
        _compositor = _coverVisual.Compositor;

        // Move the image with Translation so XAML layout is never disturbed
        ElementCompositionPreview.SetIsTranslationEnabled(CoverImage, true);
        _coverVisual.Properties.InsertVector3("Translation", Vector3.Zero);
        _coverVisual.Scale = new Vector3(ParallaxScale, ParallaxScale, 1f);

        // Smooth follow: every change of Translation eases to its new value
        var ease = _compositor.CreateVector3KeyFrameAnimation();
        ease.Target = "Translation";
        ease.InsertExpressionKeyFrame(1f, "this.FinalValue");
        ease.Duration = TimeSpan.FromMilliseconds(280);

        var implicitAnimations = _compositor.CreateImplicitAnimationCollection();
        implicitAnimations["Translation"] = ease;
        _coverVisual.ImplicitAnimations = implicitAnimations;
    }

    private void DetailsScroll_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        // Taller banner on wide windows so the art isn't cropped to a thin strip
        CoverHost.Height = Math.Clamp(e.NewSize.Width * 0.38, 240, 440);
    }

    private void CoverHost_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        var w = (float)e.NewSize.Width;
        var h = (float)e.NewSize.Height;
        if (w <= 0 || h <= 0)
        {
            return;
        }

        _coverVisual.CenterPoint = new Vector3(w / 2f, h / 2f, 0f);

        var radius = new Vector2(CoverCornerRadius);
        _hostVisual.Clip = _compositor.CreateRectangleClip(0, 0, w, h, radius, radius, radius, radius);
    }

    private void CoverHost_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        var w = CoverHost.ActualWidth;
        var h = CoverHost.ActualHeight;
        if (w <= 0 || h <= 0)
        {
            return;
        }

        var position = e.GetCurrentPoint(CoverHost).Position;

        // -1 .. 1 from the centre of the banner
        var nx = (float)((position.X / w - 0.5) * 2);
        var ny = (float)((position.Y / h - 0.5) * 2);

        // The most the image can shift before an edge shows
        var maxX = (float)(w * (ParallaxScale - 1) / 2);
        var maxY = (float)(h * (ParallaxScale - 1) / 2);

        // Opposite direction to the mouse gives the depth effect
        _coverVisual.Properties.InsertVector3("Translation", new Vector3(-nx * maxX, -ny * maxY, 0f));
    }

    private void CoverHost_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        _coverVisual.Properties.InsertVector3("Translation", Vector3.Zero);
    }
}
