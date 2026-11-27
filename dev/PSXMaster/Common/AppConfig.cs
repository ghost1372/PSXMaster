using Nucs.JsonSettings;
using Nucs.JsonSettings.Autosave;
using Nucs.JsonSettings.Modulation;
using PSXMaster.Models;

namespace PSXMaster.Common;

[Autosave]
public partial class AppConfig : NotifiyingJsonSettings, IVersionable
{
    [EnforcedVersion("1.5.0.0")]
    public virtual Version Version { get; set; } = new Version(1, 5, 0, 0);

    public override string FileName { get; set; } = Constants.AppConfigPath;

    public bool UseDeveloperMode { get; set; }
    public string LastUpdateCheck { get; set; }

    public string? Rule { get; set; } = "*.pkg|*.pup";

    public string? Host { get; set; } = "";

    public bool IsAutoFindFile { get; set; } = true;

    public string? LocalFileDirectory { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "PSXMaster");

    public BufferModel TransferBuffer { get; set; } = new BufferModel { Size = 2 * 1024 * 1024, Name = "2 MB" };

    public string DBPath { get; set; } = Constants.DatabaseFilePath;
}
