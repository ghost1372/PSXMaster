using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace PSXMaster.Core;

public delegate void DestroyDelegate(Client client);
public delegate void UpdataUrlLog(UrlInfo urlinfo);

public class PSXTools
{
    public static List<IPAddress> GetHostIp()
    {
        IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());

        return ipHost.AddressList
                .Where(p => p.AddressFamily == AddressFamily.InterNetwork)
                .ToList();
    }


    public static bool RegexUrl(string? urls)
    {
        string[]? rules = Settings.Rule!.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
        if (rules.Length <= 0 || string.IsNullOrEmpty(urls))
        {
            return false;
        }

        return
            rules.Select(rule => new Regex(rule.ToLower().Replace(".", @"\.").Replace("*", ".*?")))
                 .Any(regex => regex.Match(urls.ToLower()).Success);
    }
}
