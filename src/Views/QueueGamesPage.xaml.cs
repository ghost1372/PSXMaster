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

    public Database.Tables.QueueGames GetSelectedItem()
    {
        return (Database.Tables.QueueGames)MyListView.SelectedItem;
    }
    public IList<Database.Tables.QueueGames> GetSelectedItems()
    {
        var items = MyListView.SelectedItems.Cast<QueueGames>().ToList();
        return items;
    }
    private void MyListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ViewModel?.NotifyCanExecuteChanged();
    }

    private void DataRow_RightTapped(object sender, Microsoft.UI.Xaml.Input.RightTappedRoutedEventArgs e)
    {
        var element = e.OriginalSource as FrameworkElement;
        if (element != null)
        {
            var item = (element.DataContext as QueueGames);
            if (item != null)
            {
                MyListView.SelectedItem = item;
            }
        }
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        ViewModel.PageLoadedCommand.Execute(null);
    }

    private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        ViewModel.SearchGamesCommand.Execute(null);
    }
}
