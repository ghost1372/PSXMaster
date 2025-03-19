using System.Collections.ObjectModel;
using System.Net;
using Microsoft.UI.Dispatching;
using PSXMaster.Core;
using PSXMaster.Models;
using PSXMaster.Services;

namespace PSXMaster.ViewModels;

public partial class GameTransferViewModel : ObservableRecipient
{
    private readonly DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();

    [ObservableProperty]
    public partial List<IPAddress> AddressList { get; set; }

    [ObservableProperty]
    public partial int Port { get; set; } = 8080;

    [ObservableProperty]
    public partial object SelectedAddress { get; set; }

    [ObservableProperty]
    public partial bool IsConnected { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<LogModel> LogList { get; set; } = new ObservableCollection<LogModel>();

    private IPSXMasterService? MasterService;

    public GameTransferViewModel(IPSXMasterService masterService)
    {
        LogList.CollectionChanged += (s, e) => ClearLogCommand.NotifyCanExecuteChanged();
        this.MasterService = masterService;
        AddressList = PSXTools.GetHostIp();
        var selectedIPs = AddressList?.ToList();

        if (AddressList.Any())
        {
            var defaultIP = AddressList.Where(ip => ip.ToString().StartsWith("192.168.1.")).FirstOrDefault();
            if (defaultIP != null)
            {
                SelectedAddress = defaultIP;
            }
            else
            {
                SelectedAddress = AddressList?.LastOrDefault();
            }
        }

    }

    partial void OnSelectedAddressChanged(object value)
    {
        IsConnected = false;
        IsConnected = true;
    }

    async partial void OnIsConnectedChanged(bool value)
    {
        IPAddress.TryParse(SelectedAddress?.ToString(), out var address);
        await MasterService.Connect(address!, Port, IsConnected, OnListenerActivated);
    }

    [RelayCommand(CanExecute = nameof(CanExecuteClearLog))]
    public void OnClearLog()
    {
        LogList?.Clear();
        if (GameTransferPage.Instance != null)
        {
            GameTransferPage.Instance.ShowMessage("Logs Cleared", "", InfoBarSeverity.Success);
        }
    }

    public bool CanExecuteClearLog()
    {
        return LogList != null && LogList.Count > 0;
    }

    [RelayCommand]
    private void GenerateFakeData()
    {
        OnClearLog();
        List<UrlInfo> fakeData = new List<UrlInfo>();
        for (int i = 0; i < 23; i++)
        {
            fakeData.Add(new UrlInfo
            {
                PsnUrl = $"http://gst.prod.dl.playstation.net/gst/prod/00/PPSA01490_00/app/pkg/73/f_f08223e30d5b7c908a86298b5809ce9fdc1037743a0169e69d0d82ee21fa1d8d/EP0001-PPSA01490_00-GAME000000000000_{i}.pkg",
                ReplacePath = $"C:\\Users\\Mahdi\\Desktop\\PSXMaster\\Valhala\\EP0001-PPSA01490_00-GAME000000000000_{i}.pkg",
                Host = "store.playstation.com"
            });
        }

        foreach (var item in fakeData)
        {
            OnListenerActivated(item);
        }
    }

    private void OnListenerActivated(UrlInfo ui)
    {
        try
        {
            ui.PsnUrl = ui.PsnUrl?.Split('?')[0];
            if (PSXTools.RegexUrl(ui.PsnUrl))
            {
                Uri? uri = new(ui.PsnUrl!);
                string fileName = Path.GetFileName(uri.LocalPath);

                var matchedFile = UrlOperate.MatchFile(ui.PsnUrl);
                Game game = null;
                if (!string.IsNullOrEmpty(matchedFile))
                {
                    game = GetGameDetails(matchedFile);
                }
                else
                {
                    game = GetGameDetails(ui.PsnUrl);
                }

                if (game == null)
                {
                    return;
                }

                LogModel log = new()
                {
                    FilePath = File.Exists(Path.Combine(game?.LocalPath ?? "", fileName)) ? Path.Combine(game?.LocalPath ?? "", fileName) : "File Not Found",
                    Link = ui.PsnUrl,
                    TitleID = game.TitleID,
                    Title = Directory.GetParent(Path.Combine(game?.LocalPath ?? "", fileName))?.Name,
                    Console = game.Console
                };

                dispatcherQueue.TryEnqueue(() =>
                {
                    if (!LogList.Any(lg => lg.UrlLastPart.Equals(log.UrlLastPart, StringComparison.OrdinalIgnoreCase)))
                    {
                        LogList.Insert(0, log);
                    }
                });
            }
        }
        catch
        {

        }
    }
}
