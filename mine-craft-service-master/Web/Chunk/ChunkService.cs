using MineCraftService.Game;
using MineCraftService.GameHub.Managers;
using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MineCraftService.GameDataBase;
using MineCraftService.GameDataBase.Models;

namespace MineCraftService.Web;
using ChunkBlocks = ConcurrentDictionary<(int X, int Y, int Z), int>;
using DirtyChunkDict = ConcurrentDictionary<(int x, int z), ConcurrentDictionary<(int x, int y, int z), int>>;


public class ChunkService(GameDb db)
{
    private static readonly SemaphoreSlim ConcurrencyLimiter =
        new(Environment.ProcessorCount, Environment.ProcessorCount);

    private static readonly ConcurrentDictionary<int, SemaphoreSlim> WorldLocks = new();

    private SemaphoreSlim GetWorldLock(int worldId) =>
        WorldLocks.GetOrAdd(worldId, _ => new SemaphoreSlim(1, 1));

    public async Task<List<TerrainChunkExport>?> GenerateChunks(int worldId, List<(int x, int z)> chunkCoords)
    {
        var worldLock = GetWorldLock(worldId);
        await worldLock.WaitAsync();
        try
        {
            var worldContext = WorldManager.Instance.GetWorld(worldId);
            if (worldContext == null)
                return null;

            var result = new List<TerrainChunkExport>();
            var targetChunks = new List<TerrainChunk>();

            foreach (var (x, z) in chunkCoords)
            {
                var newChunk = worldContext.Terrain.ChunkPool.Rent(x, z);
                var chunk = newChunk ?? new TerrainChunk(worldContext.Terrain, worldContext.ChunkSetting, x, z);
                targetChunks.Add(chunk);
            }

            if (targetChunks.Count == 0)
                return result;

            await GenerateChunksInPhases(worldContext, targetChunks);
           
            // 合并数据
            foreach (var chunk in targetChunks)
            {
                var dirtyChunk = worldContext.DirtyChunkTracker.GetDirtyChunk(chunk.Coords.X, chunk.Coords.Y);
                if (dirtyChunk != null)
                {
                    foreach (var kvp in dirtyChunk)
                    {
                        chunk.SetCellValueFast(kvp.Key.x, kvp.Key.y, kvp.Key.z, kvp.Value);
                    }
                }
                result.Add(chunk.ToExportFormat());
            }

            return result;
        }
        finally
        {
            worldLock.Release();
        }
    }
    

    
    private async Task GenerateChunksInPhases(WorldContext context, List<TerrainChunk> targetChunks)
    {
        // 阶段 1：收集所有目标 Chunk 和其邻居
        var allFirstPhaseChunks = CollectChunksForPhase1(context, targetChunks);

        // 记录本次生成中向 storage 添加了哪些 Chunk，方便后续清理
        var insertedChunkCoords = new HashSet<(int x, int y)>();

        // 添加所有参与的 chunk 到 storage（供生成逻辑访问）
        foreach (var chunk in allFirstPhaseChunks)
        {
            var coords = (chunk.Coords.X, chunk.Coords.Y);
            if (context.Terrain.ChunksStorage.TryAdd(chunk))
            {
                insertedChunkCoords.Add(coords);
            }
        }

        try
        {
            // 阶段 1：生成邻居和自身初始数据
            await ProcessChunksInParallel(allFirstPhaseChunks, chunk =>
                ProcessChunkPhase(chunk, context.Generator.ChunkGenerationStep1));

            // 阶段 2：生成地表和洞穴
            await ProcessChunksInParallel(targetChunks, chunk =>
                ProcessChunkPhase(chunk, context.Generator.ChunkGenerationStep2));

            // 阶段 3：草和植被
            await ProcessChunksInParallel(targetChunks, chunk =>
                ProcessChunkPhase(chunk, context.Generator.ChunkGenerationStep3));
        }
        finally
        {
            // 清理本次生成中使用的 chunk，释放内存
            foreach (var (x, y) in insertedChunkCoords)
            {
                if (context.Terrain.TryGetChunkAtCoords(x, y, out var chunk))
                {
                    context.Terrain.ChunkPool.Return(chunk!);
                }
            }
        }
    }

    private List<TerrainChunk> CollectChunksForPhase1(WorldContext context, List<TerrainChunk> targetChunks)
    {
        var chunkMap = new Dictionary<(int x, int z), TerrainChunk>();

        foreach (var chunk in targetChunks)
        {
            var coords = chunk.Coords;
            chunkMap[(coords.X, coords.Y)] = chunk;

            for (int dx = -1; dx <= 1; dx++)
            for (int dz = -1; dz <= 1; dz++)
            {
                int nx = coords.X + dx;
                int nz = coords.Y + dz;

                var key = (nx, nz);
                if (chunkMap.ContainsKey(key))
                    continue;

                if (!context.Terrain.ChunksStorage.TryGet(nx, nz, out var neighbor) ||
                    neighbor!.CurrentPhase < 1)
                {
                    var newChunk = context.Terrain.ChunkPool.Rent(nx, nz);
                    neighbor = newChunk ?? new TerrainChunk(context.Terrain, context.ChunkSetting, nx, nz);
                }

                chunkMap[key] = neighbor;
            }
        }

        return chunkMap.Values.ToList();
    }

    private async Task ProcessChunksInParallel(List<TerrainChunk> chunks, Func<TerrainChunk, Task> process)
    {
        var tasks = chunks.Select(process);
        await Task.WhenAll(tasks);
    }

    private async Task ProcessChunkPhase(TerrainChunk chunk, IEnumerable<ChunkGenerationStep> steps)
    {
        await RunWithLimit(() =>
        {
            foreach (var step in steps)
                step.GenerateAction(chunk);

            chunk.CurrentPhase++;
        });
    }

    private Task RunWithLimit(Action action)
    {
        return Task.Run(async () =>
        {
            await ConcurrencyLimiter.WaitAsync();
            try { action(); }
            finally { ConcurrencyLimiter.Release(); }
        });
    }
    public async Task<DirtyChunkDict> LoadDirtyChunksAsync(int worldId)
    {
        var dirtyChunks = new DirtyChunkDict();
        var chunks = await db.DirtyChunks
            .Where(c => c.WorldId == worldId)
            .ToListAsync();

        foreach (var chunk in chunks)
        {
            var list = JsonSerializer.Deserialize<List<int[]>>(chunk.BlockData);
            if (list is null) continue;

            var dict = new ChunkBlocks();
            foreach (var entry in list)
            {
                // 手动映射数组数据到字典，前三个坐标，最后一个id
                dict[(entry[0], entry[1], entry[2])] = entry[3];
            }

            dirtyChunks[(chunk.ChunkX, chunk.ChunkZ)] = dict;
        }
        
        return dirtyChunks;
    }


    public async Task SaveDirtyChunksAsync(int worldId, DirtyChunkDict dirtyChunks)
    {
        foreach (var ((chunkX, chunkZ), dict) in dirtyChunks)
        {
            // 构造为数组形式 [x, y, z, id]
            var blockList = dict.Select(kv => new[] {
                kv.Key.x, kv.Key.y, kv.Key.z, kv.Value
            }).ToList();

            string json = JsonSerializer.Serialize(blockList);

            var existing = await db.DirtyChunks
                .FirstOrDefaultAsync(c => c.ChunkX == chunkX && c.ChunkZ == chunkZ && c.WorldId == worldId);

            if (existing is not null)
            {
                existing.BlockData = json;
                db.DirtyChunks.Update(existing);
            }
            else
            {
                db.DirtyChunks.Add(new DirtyChunk
                {
                    ChunkX = chunkX,
                    ChunkZ = chunkZ,
                    WorldId = worldId,
                    BlockData = json
                });
            }
        }

        await db.SaveChangesAsync();
    }
    public async Task DeleteDirtyChunks(int worldId)
    {
        await db.DirtyChunks
            .Where(c => c.WorldId == worldId)
            .ExecuteDeleteAsync();
    }

}
