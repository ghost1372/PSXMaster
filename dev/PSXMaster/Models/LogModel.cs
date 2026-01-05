using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace PSXMaster.Models;
public class LogModel
{
    public string? TitleID { get; set; }

    public string? Title { get; set; }

    public string? Link { get; set; }

    public string? FilePath { get; set; }

    public Models.Console? Console { get; set; }

    public string UrlLastPart => GetTextLastPart(Link);
    public ImageSource ConsoleLogo => GetConsoleLogo(Console);

    private string GetTextLastPart(string text)
    {
        string fixedText = text;
        if (!string.IsNullOrEmpty(text))
        {
            fixedText = text.Substring(text.LastIndexOf('/') + 1);
            if (text.Equals(fixedText))
            {
                fixedText = text.Substring(text.LastIndexOf('\\') + 1);
            }
        }

        return fixedText;
    }

    private ImageSource GetConsoleLogo(Console? console)
    {
        switch (console)
        {
            case Models.Console.PS4:
                return new BitmapImage(new Uri("ms-appx:///Assets/Fluent/PlayStationFourLogo.png"));
            case Models.Console.PS5:
                return new BitmapImage(new Uri("ms-appx:///Assets/Fluent/PlayStationFiveLogo.png"));
        }
        return null;
    }
}
