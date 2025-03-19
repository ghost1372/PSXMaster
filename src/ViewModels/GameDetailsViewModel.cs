using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace PSXMaster.ViewModels;
public partial class GameDetailsViewModel : ObservableRecipient
{
    [ObservableProperty]
    public partial bool IsProcessActive { get; set; }

    [ObservableProperty]
    public partial Visibility GridVisibility { get; set; } = Visibility.Collapsed;

    [ObservableProperty]
    public partial string Title { get; set; }

    [ObservableProperty]
    public partial string TitleId { get; set; }

    [ObservableProperty]
    public partial string LastUpdate { get; set; }

    [ObservableProperty]
    public partial string Region { get; set; }

    [ObservableProperty]
    public partial string RequiredFirmware { get; set; }

    [ObservableProperty]
    public partial string DateAdded { get; set; }

    [ObservableProperty]
    public partial string PKGSize { get; set; }

    [ObservableProperty]
    public partial string Patch { get; set; }

    [ObservableProperty]
    public partial string BackgroundURL { get; set; }

    [ObservableProperty]
    public partial bool ShowBackground { get; set; } = true;

    [ObservableProperty]
    public partial Uri Source { get; set; }

    [ObservableProperty]
    public partial string SearchQuery { get; set; }

    [ObservableProperty]
    public partial string InfoBarTitle { get; set; }

    [ObservableProperty]
    public partial string InfoBarMessage { get; set; }

    [ObservableProperty]
    public partial InfoBarSeverity InfoBarSeverity { get; set; }

    [ObservableProperty]
    public partial bool ShowInfoBar { get; set; }

    [RelayCommand]
    public void OnQuerySubmitted()
    {
        IsProcessActive = true;
        GridVisibility = Visibility.Collapsed;
        ShowInfo("Please wait", "", InfoBarSeverity.Informational);
        if (!string.IsNullOrEmpty(SearchQuery) && SearchQuery.Contains("PPSA", StringComparison.OrdinalIgnoreCase))
        {
            Source = new Uri($"https://prosperopatches.com/{SearchQuery}");
        }
        else if (!string.IsNullOrEmpty(SearchQuery) && SearchQuery.Contains("CUSA", StringComparison.OrdinalIgnoreCase))
        {
            Source = new Uri($"https://orbispatches.com/{SearchQuery}");
        }

        if (GameDetailsPage.Instance != null)
        {
            GameDetailsPage.Instance.SetSource(Source);
        }
    }

    public void ShowInfo(string title, string message, InfoBarSeverity severity)
    {
        InfoBarTitle = title;
        InfoBarMessage = message;
        InfoBarSeverity = severity;
        ShowInfoBar = true;
    }

    [RelayCommand]
    public async Task OnNavigationCompleted(WebView2 sender)
    {
        try
        {
            // Execute JavaScript to get the inner HTML of the dynamic content
            string input = await sender.ExecuteScriptAsync("document.getElementById('dynpatch').innerHTML");
            string header = await sender.ExecuteScriptAsync("document.documentElement.outerHTML;");

            input = Regex.Unescape(input);
            header = Regex.Unescape(header);

            var doc = new HtmlDocument();
            doc.LoadHtml(header);

            var lastUpdatedNode = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'bd-badge lead')]");
            var titleNode = doc?.DocumentNode?.SelectSingleNode("//h1[contains(@class, 'bd-title mb-0 text-white')]");
            var regionNode = doc?.DocumentNode?.SelectSingleNode("//div[contains(@class, 'bd-badge ms-2')]");

            Title = titleNode?.InnerText?.Trim();
            TitleId = ExtractTitleId(input);
            Patch = ExtractPatch(input);
            PKGSize = ExtractPKGSize(input);
            RequiredFirmware = ExtractFirmware(input);
            DateAdded = ExtractDateAdded(input);
            LastUpdate = lastUpdatedNode?.InnerText?.Trim();
            Region = regionNode?.InnerText?.Trim();
            BackgroundURL = ExtractBackgroundUrl(header);
            IsProcessActive = false;

            if (!string.IsNullOrEmpty(Title))
            {
                GridVisibility = Visibility.Visible;
            }

            ShowInfoBar = false;

            List<string> emptyMessages = new List<string>();
            if (string.IsNullOrEmpty(Title))
            {
                emptyMessages.Add($"No results found for {nameof(Title)}");
            }

            if (string.IsNullOrEmpty(TitleId))
            {
                emptyMessages.Add($"No results found for {nameof(TitleId)}");
            }

            if (string.IsNullOrEmpty(PKGSize))
            {
                emptyMessages.Add($"No results found for {nameof(PKGSize)}");
            }

            if (string.IsNullOrEmpty(Region))
            {
                emptyMessages.Add($"No results found for {nameof(Region)}");
            }

            if (string.IsNullOrEmpty(RequiredFirmware))
            {
                emptyMessages.Add($"No results found for {nameof(RequiredFirmware)}");
            }

            if (string.IsNullOrEmpty(DateAdded))
            {
                emptyMessages.Add($"No results found for {nameof(DateAdded)}");
            }

            if (string.IsNullOrEmpty(LastUpdate))
            {
                emptyMessages.Add($"No results found for {nameof(LastUpdate)}");
            }

            if (string.IsNullOrEmpty(Patch))
            {
                emptyMessages.Add($"No results found for {nameof(Patch)}");
            }

            if (emptyMessages.Count > 0)
            {
                ShowInfo("Warning", string.Join(", ", emptyMessages), InfoBarSeverity.Warning);
            }
        }
        catch (Exception ex)
        {
            ShowInfo("Error", ex.Message, InfoBarSeverity.Error);
        }
        finally
        {
            IsProcessActive = false;
        }
    }

    public string ExtractBackgroundUrl(string header)
    {
        if (string.IsNullOrEmpty(header))
        {
            return null;
        }

        var urlPattern = @"background:url\((.*?)\)";

        var urlMatch = Regex.Match(header, urlPattern);
        return urlMatch.Success ? urlMatch.Groups[1].Value : string.Empty;
    }
    public string ExtractPKGSize(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return null;
        }

        int sizeStart = input.IndexOf("PKG Size") + "PKG Size".Length;
        int sizeEnd = input.IndexOf("GB", sizeStart);
        return input?.Substring(input.IndexOf("text-end\">", sizeStart) + "text-end\">".Length, sizeEnd - (input.IndexOf("text-end\">", sizeStart) + "text-end\">".Length)) + "GB";
    }
    public string ExtractTitleId(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return null;
        }

        int titleIdStart = input.IndexOf("data-titleid=\"") + "data-titleid=\"".Length;
        int titleIdEnd = input.IndexOf("\"", titleIdStart);
        return input?.Substring(titleIdStart, titleIdEnd - titleIdStart);
    }
    public string ExtractPatch(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return null;
        }

        int patchStart = input.IndexOf("data-title=\"") + "data-title=\"".Length;
        int patchEnd = input.IndexOf("\"", patchStart);
        return input?.Substring(patchStart, patchEnd - patchStart);
    }
    public string ExtractFirmware(string input)
    {
        // Look for "Required Firmware" followed by its value in the next <div>
        string pattern = """Required Firmware</div><div class="col-auto text-end">(\d{1,2}\.\d{1,2}(\.\d{1,2}){0,2})</div>""";
        var regex = new Regex(pattern);
        var match = regex.Match(input);
        return match.Success ? match.Groups[1].Value : "Firmware not found";
    }
    public string ExtractDateAdded(string input)
    {
        string[] patterns = {
            @"\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}", // 2022-10-15 18:20:19
            @"\d{4}-\d{2}-\d{2}"                     // 2022-10-15
        };

        foreach (var pattern in patterns)
        {
            var regex = new Regex(pattern);
            var match = regex.Match(input);
            if (match.Success)
            {
                return match.Value;
            }
        }

        return "Date not found";
    }
}
