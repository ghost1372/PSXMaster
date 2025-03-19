using System.Collections.ObjectModel;
using System.Text;
using CommunityToolkit.WinUI.Collections;
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
    public partial AdvancedCollectionView GamesACV { get; set; }

    [ObservableProperty]
    public partial bool IsProcessActive { get; set; }

    [ObservableProperty]
    public partial string SearchQuery { get; set; }

    [RelayCommand]
    public void OnPageLoaded()
    {
        IsProcessActive = true;
        OnRefresh();
    }

    [RelayCommand]
    public void OnSearchGames()
    {
        Search();
    }

    public void Search()
    {
        if (Games != null && Games.Any())
        {
            GamesACV.Filter = _ => true;
            GamesACV.Filter = GamesFilter;
        }
    }

    private bool GamesFilter(object item)
    {
        if (item == null)
            return false;

        var query = (QueueGames)item;

        if (query == null)
            return false;

        return query.GameId.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
            query.Title.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
            query.Link.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase);
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
        GamesACV = new AdvancedCollectionView(Games, true);
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
