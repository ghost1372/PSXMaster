namespace PSXMaster.Models;
public class Game
{
    public int ID { get; set; }

    public string? Title { get; set; }

    public string? TitleID { get; set; }

    public string? LocalPath { get; set; }

    public int Region { get; set; }

    public string? Version { get; set; }

    public Console? Console { get; set; }

    public string? Link { get; set; }
}

public enum Console
{
    PS4,
    PS5
}
