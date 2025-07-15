using System.Collections.ObjectModel;
using System.Text;
using PSXMaster.Collection;
using PSXMaster.Database;
using PSXMaster.Database.Tables;
using Windows.ApplicationModel.DataTransfer;

namespace PSXMaster.ViewModels;
public partial class QueueGamesViewModel : ObservableRecipient
{
    private QueueGames _selectedItem => QueueGamesPage.Instance?.GetSelectedItem();
    private IList<QueueGames> _selectedItems => QueueGamesPage.Instance?.GetSelectedItems();

    [ObservableProperty]
    public partial ObservableCollection<QueueGames> Games { get; set; } = new ObservableCollection<QueueGames>();
    [ObservableProperty]
    public partial ObservableCollection<QueueGames> FilteredGames { get; set; } = new ObservableCollection<QueueGames>();

    public FilteringOperation<QueueGames> FilteringOption { get; set; }

    [ObservableProperty]
    public partial bool IsProcessActive { get; set; }

    [ObservableProperty]
    public partial string SearchQuery { get; set; }

    [RelayCommand]
    public void OnPageLoaded()
    {

        IsProcessActive = true;
        OnRefresh();

        FilteringOption = new FilteringOperation<QueueGames>();
        
    }

    [RelayCommand]
    public void OnSearchGames()
    {
        FilteringOption.CustomFilter = OnFilter;
        FilteringOption.IsEnabled = true;

        var result = FilteringOption.Execute(Games).ToList();
        FilteredGames = new(result);
    }

    private bool OnFilter(QueueGames game)
    {
        if (string.IsNullOrWhiteSpace(SearchQuery))
            return true;

        return (game.GameId?.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ?? false) ||
               (game.Title?.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ?? false) ||
               (game.Link?.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ?? false);
    }

    [RelayCommand]
    public void OnCopySelectedItem(object menuType)
    {
        string data = null;
        switch (menuType.ToString())
        {
            case "GameId":
                data = _selectedItem?.GameId;
                break;
            case "Title":
                data = _selectedItem?.Title;
                break;
            case "Link":
                data = _selectedItem?.Link;
                break;
            case "All":
                data = _selectedItem?.Link;
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine($"GameId: {_selectedItem?.GameId}");
                stringBuilder.AppendLine($"Title: {_selectedItem?.Title}");
                stringBuilder.AppendLine($"Link: {_selectedItem?.Link}");
                data = stringBuilder.ToString();
                break;
        }

        var package = new DataPackage();
        package.SetText(data);
        Clipboard.SetContent(package);
    }

    [RelayCommand]
    public async Task AddGame()
    {
        var dialog = new AddOrEditContentDialog(DialogType.Add);
        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            OnRefresh();
        }
    }

    [RelayCommand(CanExecute = nameof(CanEditGame))]
    public async Task EditGame()
    {
        var dialog = new AddOrEditContentDialog(DialogType.Edit);
        dialog.GameId = _selectedItem.GameId;
        dialog.GameTitle = _selectedItem.Title;
        dialog.Link = _selectedItem.Link;
        dialog.baseItemId = _selectedItem.Id;
        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            OnRefresh();
        }
    }

    private bool CanEditGame()
    {
        if (_selectedItems is not null && _selectedItems.Count == 1)
        {
            return true;
        }
        return false;
    }

    [RelayCommand(CanExecute = nameof(CanDeleteGame))]
    public async Task DeleteGame()
    {
        var dialog = new DeleteGameContentDialog();
        dialog._selectedItems = _selectedItems?.Select(item => item.Id).ToList();
        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            OnRefresh();
        }
    }

    [RelayCommand]
    private void OnRefresh()
    {
        using (var db = new PSXMasterDbContext())
        {
            Games = new(db.QueueGames);
        }
        FilteredGames = Games;
        IsProcessActive = false;
    }

    public void NotifyCanExecuteChanged()
    {
        EditGameCommand.NotifyCanExecuteChanged();
        DeleteGameCommand.NotifyCanExecuteChanged();
    }
    private bool CanDeleteGame()
    {
        if (_selectedItems is not null && _selectedItems.Count > 0)
        {
            return true;
        }
        return false;
    }
}
