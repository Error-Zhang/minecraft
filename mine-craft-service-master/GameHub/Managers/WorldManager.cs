using System.Collections.Concurrent;
using MineCraftService.Game;
using MineCraftService.Web;

namespace MineCraftService.GameHub.Managers;

public class WorldContext(Terrain terrain, TerrainGenerator generator, ChunkSetting chunkSetting, DirtyChunkTracker dirtyChunkTracker)
{
    public Terrain Terrain { get; } = terrain;
    public TerrainGenerator Generator { get; } = generator;
    public ChunkSetting ChunkSetting { get; } = chunkSetting;
    public DirtyChunkTracker DirtyChunkTracker { get; } = dirtyChunkTracker;
}

public class WorldManager
{
    private readonly Dictionary<int, WorldContext> _loadedWorlds = new();
    private readonly object _lock = new();
    
    private static readonly WorldManager _instance = new();
    public static WorldManager Instance => _instance;
    
    private WorldManager()
    {
        TerrainGenerator.CreateBrushes(); // 全局生成地图刷
    }

    public WorldContext LoadWorld(int worldId, WorldSetting worldSetting, ChunkSetting chunkSetting)
    {
        lock (_lock)
        {
            var chunkPool = new ChunkPool(24);
            var terrain = new Terrain(chunkPool,chunkSetting.ChunkSize);
            var generator = new TerrainGenerator(terrain, worldSetting);
            var dirtyChunkTracker = new DirtyChunkTracker(chunkSetting);
            var context = new WorldContext(terrain, generator, chunkSetting,dirtyChunkTracker);
            _loadedWorlds[worldId] = context;
            return context;
        }
    }

    public void UnloadWorld(int worldId)
    {
        lock (_lock)
        {
            if (_loadedWorlds.TryGetValue(worldId, out var context))
            {
                context.Terrain.ChunkPool.Release();
                _loadedWorlds.Remove(worldId);
            }
        }
    }

    public WorldContext? GetWorld(int worldId)
    {
        lock (_lock)
        {
            return _loadedWorlds.TryGetValue(worldId, out var context) ? context : null;
        }
    }
}