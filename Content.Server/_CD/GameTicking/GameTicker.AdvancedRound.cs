using System.Linq;
using System.Threading.Tasks;
using Content.Shared._CD.CCVars;

namespace Content.Server.GameTicking;

public sealed partial class GameTicker
{
    private static int _cacheDepth;

    public Queue<string> MapCache = new();

    private void IncrementAdvancedRoundNumber()
    {
        _cacheDepth = _cfg.GetCVar(CDCCVars.MapvoteCacheDepth);
        //var mapName = ;

        var task = Task.Run(async () =>
        {
            var server = await _dbEntryManager.ServerEntity;

            await _db.AddNewAdvancedRound(server, RoundId, _gameMapManager.GetSelectedMap()!.ID);

            UpdateMapQueue(_gameMapManager.GetSelectedMap()!.ID);
        });
    }

    private void UpdateMapQueue(string mapName)
    {
        var task = Task.Run(async () =>
        {
            if (MapCache.Count < _cacheDepth)
            {
                await _db.RetrieveMapQueue(MapCache, _cacheDepth);
                return;
            }

            MapCache.Dequeue();
            MapCache.Enqueue(mapName);
        });
    }
}
