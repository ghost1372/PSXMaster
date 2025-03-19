namespace PSXMaster.Core;

public class HashUrl
{
    private static HashUrl? _instance;
    private static readonly object Lock = new();

    public static HashUrl Instance()
    {
        if (_instance == null)
        {
            lock (Lock)
            {
                _instance = new HashUrl();
            }
        }

        return _instance;
    }

    public string PsnLocalPath(string psnurl)
    {
        try
        {
            if (!Settings.IsAutoFindFile)
            {
                return string.Empty;
            }

            // Get the filename from the URL
            string filename = GetUrlFileName(psnurl);
            if (!string.IsNullOrEmpty(filename))
            {
                // Search for the file in the local directory and its subdirectories
                string foundFilePath = FindFileInDirectory(Settings.LocalFileDirectory, filename);
                return foundFilePath ?? string.Empty;
            }

            return string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    // Method to search for the file in the directory and its subdirectories using EnumerateFiles
    private string? FindFileInDirectory(string directory, string filename)
    {
        try
        {
            // Use EnumerateFiles to search for the file in all subdirectories
            var files = Directory.EnumerateFiles(directory, filename, SearchOption.AllDirectories);

            // Return the first matching file found
            return files.FirstOrDefault();
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
        catch (DirectoryNotFoundException)
        {
            return null;
        }
        catch (IOException)
        {
            return null;
        }
    }

    public string GetUrlFileName(string psnurl)
    {
        List<string>? rules = Settings.Rule!.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        if (rules.Count <= 0)
        {
            return string.Empty;
        }

        rules = rules.Select(r => r.Replace("*", "")).ToList();
        string filename = string.Empty;
        rules.ForEach(r =>
        {
            if (psnurl.IndexOf(r) > 0)
            {
                filename = psnurl[..(psnurl.IndexOf(r) + r.Length)];
                filename = filename[(filename.LastIndexOf("/") + 1)..];
            }
        });

        return filename;
    }
}
