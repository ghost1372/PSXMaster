using Microsoft.EntityFrameworkCore;
using PSXMaster.Database;
using PSXMaster.Database.Tables;

namespace PSXMaster.Common;

public static partial class Queries
{
    public static readonly Func<PSXMasterDbContext, IAsyncEnumerable<QueueGames>> GetAllGamesQueryAsync = EF.CompileAsyncQuery((PSXMasterDbContext context) => context.QueueGames);
    public static readonly Func<PSXMasterDbContext, string, string, IAsyncEnumerable<QueueGames>> GetGameByIdAndTitleQueryAsync = EF.CompileAsyncQuery((PSXMasterDbContext context, string gameId, string titleId) => context.QueueGames.Where(x => x.GameId.ToLower() == gameId.ToLower() && x.Title.ToLower() == titleId.ToLower()));
    public static readonly Func<PSXMasterDbContext, int, IAsyncEnumerable<QueueGames>> GetGameByIdQueryAsync = EF.CompileAsyncQuery((PSXMasterDbContext context, int id) => context.QueueGames.Where(x => x.Id == id));
}
