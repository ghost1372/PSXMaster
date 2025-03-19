using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PSXMaster.Database.Tables;
public class QueueGames
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string Title { get; set; }
    public string Link { get; set; }
    public string GameId { get; set; }
    public Models.Console Console => GetConsole();

    private Models.Console GetConsole()
    {
        if (!string.IsNullOrEmpty(GameId) && GameId.StartsWith("PPSA", StringComparison.OrdinalIgnoreCase))
        {
            return Models.Console.PS5;
        }
        else
        {
            return Models.Console.PS4;
        }
    }
}

