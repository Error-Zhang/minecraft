using System.Collections.Concurrent;
using MineCraftService.Game;

namespace MineCraftService.GameHub;

using DirtyChunkDict = ConcurrentDictionary<(int x, int z), ConcurrentDictionary<(int x, int y, int z), int>>;

public class DirtyChunkTracker(ChunkSetting chunkSetting)
{
    private DirtyChunkDict _dirtyChunks = new();
    private readonly int _chunkSize = chunkSetting.ChunkSize;

    public void MarkDirty(int x, int y, int z, int blockId)
    {
        int chunkX = (int)MathF.Floor((float)x / _chunkSize);
        int chunkZ = (int)MathF.Floor((float)z / _chunkSize);

        int localX = x - chunkX * _chunkSize;
        int localZ = z - chunkZ * _chunkSize;

        var key = (chunkX, chunkZ);
        var localBlockPos = (localX, y, localZ);
        var chunkDict = _dirtyChunks.GetOrAdd(key, _ => new ConcurrentDictionary<(int, int, int), int>());
        chunkDict[localBlockPos] = blockId;
    }

    public ConcurrentDictionary<(int x, int y, int z), int>? GetDirtyChunk(int x,int z)
    {
        return _dirtyChunks.TryGetValue((x, z), out var chunk) ? chunk : null;
    }

    public void Load(DirtyChunkDict dictionary)
    {
        _dirtyChunks = dictionary;
    }

    public DirtyChunkDict GetAll() => _dirtyChunks;

    public void Clear() => _dirtyChunks.Clear();
}