using System.Collections.ObjectModel;
using Nucs.JsonSettings;
using Nucs.JsonSettings.Fluent;
using Nucs.JsonSettings.Modulation;
using Nucs.JsonSettings.Modulation.Recovery;
using PSXMaster.Models;

namespace PSXMaster.Common;
public static partial class AppHelper
{
    public static AppConfig Settings = JsonSettings.Configure<AppConfig>()
                               .WithRecovery(RecoveryAction.RenameAndLoadDefault)
                               .WithVersioning(VersioningResultAction.RenameAndLoadDefault)
                               .LoadNow();

    public static ObservableCollection<BufferModel> Buffers = new ObservableCollection<BufferModel>()
    {
        new BufferModel { Size = 4 * 1024, Name = "4 KB" },
        new BufferModel { Size = 8 * 1024, Name = "8 KB" },
        new BufferModel { Size = 16 * 1024, Name = "16 KB" },
        new BufferModel { Size = 32 * 1024, Name = "32 KB" },
        new BufferModel { Size = 64 * 1024, Name = "64 KB" },
        new BufferModel { Size = 128 * 1024, Name = "128 KB" },
        new BufferModel { Size = 256 * 1024, Name = "256 KB" },
        new BufferModel { Size = 512 * 1024, Name = "512 KB" },
        new BufferModel { Size = 1 * 1024 * 1024, Name = "1 MB" },
        new BufferModel { Size = 2 * 1024 * 1024, Name = "2 MB" },
        new BufferModel { Size = 4 * 1024 * 1024, Name = "4 MB" },
        new BufferModel { Size = 8 * 1024 * 1024, Name = "8 MB" },
        new BufferModel { Size = 16 * 1024 * 1024, Name = "16 MB" },
        new BufferModel { Size = 32 * 1024 * 1024, Name = "32 MB" },
        new BufferModel { Size = 64 * 1024 * 1024, Name = "64 MB" }
    };

    public static Game GetGameDetails(string file)
    {
        if (string.IsNullOrEmpty(file))
        {
            return null;
        }
        string? titleID = Path.GetFileName(file).Split('-')[1].Replace("_00", "");

        string? filePath = Path.GetDirectoryName(file)!;

        if (IsValidUrl(file))
        {
            filePath = file;
        }

        string? title = Directory.GetParent(file)?.Name;
        var console = titleID.Contains("CUSA") ? Models.Console.PS4 : Models.Console.PS5;
        return new Game
        {
            TitleID = titleID,
            Title = title,
            LocalPath = filePath,
            Console = console
        };
    }

    public static bool IsValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult)
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}

