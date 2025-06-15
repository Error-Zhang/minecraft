using Microsoft.AspNetCore.SignalR;
using MineCraftService.Game;
using MineCraftService.GameHub.Managers;
using MineCraftService.Web;
using System.Collections.Concurrent;
namespace MineCraftService.GameHub;

// 每次连接都会创建一个新的 Hub 实例
public class WorldHub(WorldService worldService,ChunkService chunkService) : Hub
{
    private static readonly WorldManager _worldManager = WorldManager.Instance;

    // 记录连接 ID 对应的世界 ID 和玩家 ID
    private static readonly ConcurrentDictionary<string, (int worldId, int playerId)>
        ConnectionMap = new();
    private async Task HandleDisconnect(string connectionId)
    {
        if (ConnectionMap.TryRemove(connectionId, out var entry))
        {
            int worldId = entry.worldId;
            var worldContext = _worldManager.GetWorld(worldId);

            if (worldContext != null)
            {
                bool hasOtherPlayers = ConnectionMap.Values.Any(p => p.worldId == worldId);
                if (!hasOtherPlayers)
                {
                    await chunkService.SaveDirtyChunksAsync(worldId, worldContext.DirtyChunkTracker.GetAll());
                    _worldManager.UnloadWorld(worldId);
                    Console.WriteLine($"UnloadWorld {worldId}");
                }
            }
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await HandleDisconnect(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
    public async Task<ApiResponse> JoinWorld(int worldId, int playerId)
    {
        if (worldId <= 0 || playerId <= 0)
            return ApiResponse.RawFail(400, "参数错误,请检查worldId和playerId是否存在");
        ConnectionMap[Context.ConnectionId] = (worldId, playerId);

        var worldContext = _worldManager.GetWorld(worldId);
        if (worldContext != null)
        {
            return ApiResponse.RawSuccess(worldContext.ChunkSetting);
        }

        var world = await worldService.GetWorld(worldId);
        if (world == null) return ApiResponse.RawNotFound("World");

        var worldSetting = new WorldSetting
        {
            Seed = world.Seed,
            Season = world.Season,
            WorldMode = world.WorldMode,
        };
        var chunkSetting = new ChunkSetting();
        worldContext = _worldManager.LoadWorld(worldId, worldSetting, chunkSetting);
        
        var dirtyChunkDict = await chunkService.LoadDirtyChunksAsync(worldId);
        worldContext.DirtyChunkTracker.Load(dirtyChunkDict);

        return ApiResponse.RawSuccess(chunkSetting);
    }
    
    public Task<ApiResponse> SetBlock(BlockActionData[] data)
    {
        var (worldId, _) = ConnectionMap[Context.ConnectionId];
        var worldContext = _worldManager.GetWorld(worldId)!;
        foreach (var block in data)
        {
            // 记录为脏数据
            worldContext.DirtyChunkTracker.MarkDirty(block.X, block.Y, block.Z,block.BlockId);
        }
        return Task.FromResult(ApiResponse.RawSuccess());
    }
}