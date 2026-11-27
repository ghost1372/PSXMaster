using PSXMaster.Database.Tables;

namespace PSXMaster.Views;

public sealed partial class QueueGamesPage : Page
{
    public QueueGamesViewModel ViewModel { get; set; }
    internal static QueueGamesPage Instance { get; set; }

    public QueueGamesPage()
    {
        ViewModel = App.GetService<QueueGamesViewModel>();
        this.InitializeComponent();
        Instance = this;
    }

    // Used by x:Bind in the empty-state panel
    public static Visibility ShowWhenEmpty(int count, bool isBusy)
        => count == 0 && !isBusy ? Visibility.Visible : Visibility.Collapsed;

    public QueueGames GetSelectedItem()
    {
        return (QueueGames)MyListView.SelectedItem;
    }

    public IList<QueueGames> GetSelectedItems()
    {
        return MyListView.SelectedItems.Cast<QueueGames>().ToList();
    }

    private void MyListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ViewModel?.NotifyCanExecuteChanged();
    }

    private void DataRow_RightTapped(object sender, Microsoft.UI.Xaml.Input.RightTappedRoutedEventArgs e)
    {
        if ((e.OriginalSource as FrameworkElement)?.DataContext is QueueGames item
            && !MyListView.SelectedItems.Contains(item)) // keep a multi-selection intact
        {
            MyListView.SelectedItem = item;
        }
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.OnPageLoaded();
    }

    private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            ViewModel.SearchGamesCommand.Execute(null);
        }
    }
}
