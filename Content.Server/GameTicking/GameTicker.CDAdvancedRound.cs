using System.Linq;
using System.Threading.Tasks;
using Content.Shared._CD.CCVars;
using Content.Shared.GameTicking;

namespace Content.Server.GameTicking;

public sealed partial class GameTicker : SharedGameTicker
{
    private static int _cacheDepth;

    public Queue<string> MapCache = new();

    private void IncrementAdvancedRoundNumber()
    {
        _cacheDepth = _cfg.GetCVar(CDCCVars.MapvoteCacheDepth);

        var task = Task.Run(async () =>
        {
            var server = await _dbEntryManager.ServerEntity;
            var map = _gameMapManager.GetSelectedMap()!.ID;

            await _db.AddNewAdvancedRound(server, RoundId, map);
            UpdateMapQueue(map);
        });
    }

    private void UpdateMapQueue(string mapName)
    {
        var task = Task.Run(async () =>
        {
            if (MapCache.Count < _cacheDepth)
            {
                await _db.RetrieveMapQueue(MapCache, _cacheDepth);
            }
            else
            {
                MapCache.Dequeue();
                MapCache.Enqueue(mapName);
            }
        });


    }
}
