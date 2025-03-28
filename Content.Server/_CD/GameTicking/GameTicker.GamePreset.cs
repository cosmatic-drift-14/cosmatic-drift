using System.Linq;
using System.Threading.Tasks;
using Content.Shared.GameTicking;

namespace Content.Server.GameTicking;

public sealed partial class GameTicker
{
    private void IncrementAdvancedRoundNumber()
    {
        var playerIds = _playerGameStatuses.Keys.Select(player => player.UserId).ToArray();
        var mapName = _gameMapManager.GetSelectedMap()!.ID;

        var task = Task.Run(async () =>
        {
            var roundid = RoundId;
            var server = await _dbEntryManager.ServerEntity;
            return await _db.AddNewAdvancedRound(server, roundid, mapName, playerIds);
        });
    }
}
