using System.Text.Json;
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

    private static readonly Regex TitleIdRegex =
        new(@"\b(PPSA|CUSA)\d{5}\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private CancellationTokenSource _loadCts;

    [RelayCommand]
    public void OnQuerySubmitted()
    {
        // Accepts "PPSA29660" as well as pasted text like "PPSA29660: Grand Theft Auto VI"
        var match = TitleIdRegex.Match(SearchQuery ?? string.Empty);
        if (!match.Success)
        {
            ShowInfo("Invalid title id", "Enter a title id like PPSA05684 or CUSA00341.", InfoBarSeverity.Warning);
            return;
        }

        var id = match.Value.ToUpperInvariant();
        var host = id.StartsWith("PPSA", StringComparison.Ordinal) ? "prosperopatches.com" : "orbispatches.com";

        _loadCts?.Cancel();
        ResetDetails();
        IsProcessActive = true;
        ShowInfo("Please wait", $"Loading {id}...", InfoBarSeverity.Informational);

        Source = new Uri($"https://{host}/{id}");
        GameDetailsPage.Instance?.SetSource(Source);
    }

    public void ShowInfo(string title, string message, InfoBarSeverity severity)
    {
        InfoBarTitle = title;
        InfoBarMessage = message;
        InfoBarSeverity = severity;
        ShowInfoBar = true;
    }

    private void ResetDetails()
    {
        GridVisibility = Visibility.Collapsed;
        Title = TitleId = LastUpdate = Region = RequiredFirmware = null;
        DateAdded = PKGSize = Patch = BackgroundURL = null;
    }

    // AllowConcurrentExecutions: a new navigation must be able to start (and cancel the old run)
    // while a previous run is still waiting for the page's JavaScript.
    [RelayCommand(AllowConcurrentExecutions = true)]
    public async Task OnNavigationCompleted(WebView2 sender)
    {
        _loadCts?.Cancel();
        var cts = _loadCts = new CancellationTokenSource();
        var ct = cts.Token;

        try
        {
            // The patch list is filled in by JavaScript after the page has "loaded".
            await WaitForPatchesAsync(sender, ct);

            var html = await GetStringAsync(sender, "document.documentElement.outerHTML", ct);
            var patchHtml = await GetStringAsync(sender, "document.getElementById('dynpatch')?.innerHTML ?? ''", ct);

            ct.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(html))
            {
                ShowInfo("Error", "The page returned no content.", InfoBarSeverity.Error);
                return;
            }

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var pageTitle = Clean(doc.DocumentNode.SelectSingleNode("//title")?.InnerText);
            if (pageTitle != null && pageTitle.Contains("Just a moment", StringComparison.OrdinalIgnoreCase))
            {
                ShowInfo("Blocked", "The site showed a Cloudflare check instead of the game page.", InfoBarSeverity.Error);
                return;
            }

            var titleNode = doc.DocumentNode.SelectSingleNode("//h1[contains(@class,'bd-title')]")
                            ?? doc.DocumentNode.SelectSingleNode("//h1");
            var regionNode = doc.DocumentNode.SelectSingleNode("//div[contains(@class,'bd-badge') and contains(@class,'ms-2')]");
            var updatedNode = doc.DocumentNode.SelectSingleNode("//div[contains(@class,'bd-badge') and contains(@class,'lead')]");

            Title = Clean(titleNode?.InnerText);
            Region = Clean(regionNode?.InnerText);
            LastUpdate = Clean(updatedNode?.InnerText);
            if (LastUpdate != null && LastUpdate.Contains("Loading", StringComparison.OrdinalIgnoreCase))
            {
                LastUpdate = null; // the badge shows a placeholder until the patches are loaded
            }
            BackgroundURL = ExtractBackgroundUrl(html);

            TitleId = ExtractTitleId(patchHtml) ?? TitleIdRegex.Match(SearchQuery ?? string.Empty).Value.ToUpperInvariant();
            Patch = ExtractPatch(patchHtml);
            PKGSize = ExtractLabelValue(patchHtml, "PKG Size");
            RequiredFirmware = ExtractLabelValue(patchHtml, "Required Firmware");
            DateAdded = ExtractDateAdded(patchHtml);

            if (string.IsNullOrEmpty(Title))
            {
                GridVisibility = Visibility.Collapsed;
                ShowInfo("Not found", $"No game page was found for {SearchQuery}.", InfoBarSeverity.Warning);
                return;
            }

            GridVisibility = Visibility.Visible;

            if (string.IsNullOrWhiteSpace(patchHtml))
            {
                // Title exists but has no patch entries yet (e.g. unreleased games)
                ShowInfo("No patches", "This title has no patch information on the site yet.", InfoBarSeverity.Informational);
                return;
            }

            var missing = new List<string>();
            if (string.IsNullOrEmpty(Patch)) missing.Add(nameof(Patch));
            if (string.IsNullOrEmpty(PKGSize)) missing.Add(nameof(PKGSize));
            if (string.IsNullOrEmpty(RequiredFirmware)) missing.Add(nameof(RequiredFirmware));
            if (string.IsNullOrEmpty(DateAdded)) missing.Add(nameof(DateAdded));
            if (string.IsNullOrEmpty(Region)) missing.Add(nameof(Region));
            if (string.IsNullOrEmpty(LastUpdate)) missing.Add(nameof(LastUpdate));

            if (missing.Count > 0)
            {
                ShowInfo("Warning", "No results found for: " + string.Join(", ", missing), InfoBarSeverity.Warning);
            }
            else
            {
                ShowInfoBar = false;
            }
        }
        catch (OperationCanceledException)
        {
            // A newer search replaced this one; it owns the UI state now.
            return;
        }
        catch (Exception ex)
        {
            ShowInfo("Error", ex.Message, InfoBarSeverity.Error);
        }
        finally
        {
            if (ReferenceEquals(_loadCts, cts))
            {
                IsProcessActive = false;
            }
        }
    }

    // ---- helpers -------------------------------------------------------------------------

    /// <summary>Runs JS and returns the result as a plain string (null if the script returned null).</summary>
    private static async Task<string> GetStringAsync(WebView2 webView, string script, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var json = await webView.ExecuteScriptAsync(script);
        if (string.IsNullOrEmpty(json) || json == "null")
        {
            return null;
        }

        // ExecuteScriptAsync returns JSON, so decode it properly instead of Regex.Unescape
        return JsonSerializer.Deserialize<string>(json);
    }

    /// <summary>Waits (max ~15 s) until the page no longer shows "Loading patches...".</summary>
    private static async Task WaitForPatchesAsync(WebView2 webView, CancellationToken ct)
    {
        const string script =
            "document.body ? document.body.innerText.includes('Loading patches') : true";

        for (var i = 0; i < 50; i++)
        {
            ct.ThrowIfCancellationRequested();
            var result = await webView.ExecuteScriptAsync(script);
            if (result == "false")
            {
                await Task.Delay(200, ct); // let the DOM settle
                return;
            }

            await Task.Delay(300, ct);
        }
    }

    private static string Clean(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        return HtmlEntity.DeEntitize(text).Trim();
    }

    private static string FirstGroup(string input, string pattern, RegexOptions options = RegexOptions.None)
    {
        if (string.IsNullOrEmpty(input))
        {
            return null;
        }

        var match = Regex.Match(input, pattern, options | RegexOptions.IgnoreCase);
        return match.Success ? Clean(match.Groups[1].Value) : null;
    }

    public static string ExtractBackgroundUrl(string html)
    {
        var url = FirstGroup(html, @"background:\s*url\(\s*['""]?(.*?)['""]?\s*\)");
        return url;
    }

    public static string ExtractTitleId(string patchHtml) =>
        FirstGroup(patchHtml, @"data-titleid=""([^""]+)""");

    public static string ExtractPatch(string patchHtml) =>
        FirstGroup(patchHtml, @"data-title=""([^""]+)""");

    /// <summary>Reads the value that follows a label, e.g. "PKG Size" ... &lt;div class="text-end"&gt;12.3 GB&lt;/div&gt;.</summary>
    public static string ExtractLabelValue(string patchHtml, string label) =>
        FirstGroup(patchHtml, Regex.Escape(label) + @".*?text-end[^>]*>\s*([^<]+?)\s*<", RegexOptions.Singleline);

    public static string ExtractDateAdded(string patchHtml) =>
        FirstGroup(patchHtml, @"(\d{4}-\d{2}-\d{2}(?: \d{2}:\d{2}:\d{2})?)");
}
