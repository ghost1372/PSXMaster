using System.IO;
using Microsoft.Data.Sqlite;

namespace PSXMaster.Database;
public static partial class DbBootstrapper
{
    public static void EnsureDatabaseExists(string databaseFilePath)
    {
        bool isNew = !File.Exists(databaseFilePath);

        using var connection = new SqliteConnection($"Data Source={databaseFilePath}");
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
