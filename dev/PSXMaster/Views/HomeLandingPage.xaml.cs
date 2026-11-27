using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Media.Imaging;

namespace PSXMaster.Views;

public sealed partial class HomeLandingPage : Page
{
    private const double ParallaxStrength = 14;
    private readonly Windows.UI.ViewManagement.UISettings _uiSettings = new();
    private Microsoft.UI.Composition.Visual? _heroVisual;
    private Microsoft.UI.Composition.SpringVector3NaturalMotionAnimation? _heroSpring;
    private System.Numerics.Vector3 _heroTarget;

    public string NewGroupText { get; set; } = "Recently added";
    public string UpdatedGroupText { get; set; } = "Recently updated";
    public string PreviewGroupText { get; set; } = "Preview";

    public HomeLandingPage()
    {
        this.InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        ActualThemeChanged += (_, _) => DispatcherQueue.TryEnqueue(UpdateHeroImage);
    }

    public static Visibility ToVisibility(bool value) => value ? Visibility.Visible : Visibility.Collapsed;

    public static string GetGlyph(object key) => (key as string) switch
    {
        "New" => "\uE734",
        "Updated" => "\uE777",
        "Preview" => "\uE7B3",
        _ => "\uE71D"
    };

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        UpdateHeroImage();

        DataSource.Instance.ParsingCompleted -= OnParsingCompleted;
        DataSource.Instance.ParsingCompleted += OnParsingCompleted;

        Refresh();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        DataSource.Instance.ParsingCompleted -= OnParsingCompleted;
    }

    private void OnParsingCompleted(object? sender, EventArgs e)
    {
        if (DispatcherQueue.HasThreadAccess)
        {
            Refresh();
        }
        else
        {
            DispatcherQueue.TryEnqueue(Refresh);
        }
    }

    private void UpdateHeroImage()
    {
        if (HeroRoot.Tag is string path && !string.IsNullOrEmpty(path))
        {
            HeroBrush.ImageSource = new BitmapImage(new Uri(path)) { DecodePixelWidth = 1920 };
        }
    }

    private void Refresh()
    {
        var all = DataSource.Instance.Groups
            .Where(group => !group.HideGroup)
            .SelectMany(group => group.Items)
            .Where(item => !item.HideItem)
            .ToList();

        var tagged = all.Where(item => item.BadgeString != null).ToList();
        var sections = new ObservableCollection<GroupInfoList>();

        AddSection(sections, tagged, "New", NewGroupText);
        AddSection(sections, tagged, "Updated", UpdatedGroupText);
        AddSection(sections, tagged, "Preview", PreviewGroupText);

        LoadingPanel.Visibility = ToVisibility(all.Count == 0);
        SectionsRepeater.ItemsSource = sections;
    }

    private static void AddSection(ObservableCollection<GroupInfoList> sections, List<DataItem> source, string key, string title)
    {
        var items = source
            .Where(item => string.Equals(item.BadgeString, key, StringComparison.Ordinal))
            .OrderBy(item => item.Title)
            .ToList();

        if (items.Count > 0)
        {
            sections.Add(new GroupInfoList(items) { Key = key, Title = title });
        }
    }

    private void OnHeroSizeChanged(object sender, SizeChangedEventArgs e)
    {
        // Keep the oversized cover inside the banner.
        HeroRoot.Clip = new Microsoft.UI.Xaml.Media.RectangleGeometry
        {
            Rect = new Windows.Foundation.Rect(0, 0, e.NewSize.Width, e.NewSize.Height)
        };
    }

    private void EnsureParallax()
    {
        if (_heroVisual != null)
        {
            return;
        }

        // Animate on the compositor (off the UI thread) so mouse moves never block layout or rendering.
        Microsoft.UI.Xaml.Hosting.ElementCompositionPreview.SetIsTranslationEnabled(HeroImage, true);
        _heroVisual = Microsoft.UI.Xaml.Hosting.ElementCompositionPreview.GetElementVisual(HeroImage);
        _heroVisual.Properties.InsertVector3("Translation", System.Numerics.Vector3.Zero);

        _heroSpring = _heroVisual.Compositor.CreateSpringVector3Animation();
        _heroSpring.Target = "Translation";
        _heroSpring.DampingRatio = 1.0f;
        _heroSpring.Period = TimeSpan.FromMilliseconds(70);
    }

    private void MoveHeroTo(System.Numerics.Vector3 target)
    {
        EnsureParallax();

        // Ignore sub-pixel changes so we don't restart the animation on every tiny mouse move.
        if (System.Numerics.Vector3.DistanceSquared(target, _heroTarget) < 1f && target != System.Numerics.Vector3.Zero)
        {
            return;
        }

        _heroTarget = target;
        _heroSpring!.FinalValue = target;
        _heroVisual!.StartAnimation("Translation", _heroSpring);
    }

    private void OnHeroPointerMoved(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        if (!_uiSettings.AnimationsEnabled || HeroRoot.ActualWidth <= 0 || HeroRoot.ActualHeight <= 0)
        {
            return;
        }

        var point = e.GetCurrentPoint(HeroRoot).Position;
        var x = Math.Clamp(point.X / HeroRoot.ActualWidth - 0.5, -0.5, 0.5) * 2;
        var y = Math.Clamp(point.Y / HeroRoot.ActualHeight - 0.5, -0.5, 0.5) * 2;

        // Move opposite to the pointer for a subtle depth effect.
        MoveHeroTo(new System.Numerics.Vector3((float)(-x * ParallaxStrength), (float)(-y * ParallaxStrength), 0));
    }

    private void OnHeroPointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        MoveHeroTo(System.Numerics.Vector3.Zero);
    }

    private void OnPageSizeChanged(object sender, SizeChangedEventArgs e)
    {
        // Scale the banner with the window so the 16:9 cover keeps more of its height.
        HeroRoot.Height = Math.Clamp(e.NewSize.Width * 0.28, 280, 420);
    }

    private void OnExploreAllClick(object sender, RoutedEventArgs e)
    {
        ContentPanel.StartBringIntoView(new BringIntoViewOptions
        {
            AnimationDesired = true,
            VerticalAlignmentRatio = 0.0
        });
    }

    private void OnCardClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: DataItem item })
        {
            App.Current.NavService.NavigateTo(item.UniqueId, item);
        }
    }
}
