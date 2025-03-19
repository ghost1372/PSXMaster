namespace PSXMaster.Core;

public class UrlOperate
{
    private static readonly HashUrl HashurlOperate = HashUrl.Instance();

    public static string MatchFile(string url)
    {
        return HashurlOperate.PsnLocalPath(url);
    }

    public static string PsnLocalPath(string psnurl)
    {
        return HashurlOperate.PsnLocalPath(psnurl);
    }

    public static string GetUrlFileName(string psnurl)
    {
        return HashurlOperate.GetUrlFileName(psnurl);
    }
}
