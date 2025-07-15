using Microsoft.Data.Sqlite;
using PSXMaster.Common;

namespace PSXMaster.Database;
public static partial class DbBootstrapper
{
    public static void EnsureDatabaseExists()
    {
        string filename = Constants.DatabaseFilePath;
        if (!string.IsNullOrEmpty(Settings.DBPath))
        {
            filename = $"{Settings.DBPath}";
        }

        bool isNew = !File.Exists(filename);

        using var connection = new SqliteConnection($"Data Source={filename}");
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS QueueGames (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Title TEXT NULL,
                Link TEXT NULL,
                GameId TEXT NULL
            );
        ";
        command.ExecuteNonQuery();
    }
}
