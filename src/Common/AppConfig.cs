using Nucs.JsonSettings.Examples;
using Nucs.JsonSettings.Modulation;

namespace PSXMaster.Common;

[GenerateAutoSaveOnChange]
public partial class AppConfig : NotifiyingJsonSettings, IVersionable
{
    [EnforcedVersion("1.1.0.0")]
    public virtual Version Version { get; set; } = new Version(1, 1, 0, 0);

    private string fileName { get; set; } = Constants.AppConfigPath;

    private bool useDeveloperMode { get; set; }
    private string lastUpdateCheck { get; set; }

    private string? rule { get; set; } = "*.pkg|*.pup";

    private string? host { get; set; } = "";

    private bool isAutoFindFile { get; set; } = true;

    private string? localFileDirectory { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "PSXMaster");

    private int bufferSize { get; set; } = 2 * 1024 * 1024;

    private string dBPath { get; set; } = $"{AppContext.BaseDirectory}Games.db";
}
