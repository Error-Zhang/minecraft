namespace MineCraftService.Game;

// 每个世界分配一定内存专门处理区块生成.减少GC压力
public class ChunkPool(int max)
{
    private readonly Stack<TerrainChunk> _pool = new();
    
    public TerrainChunk? Rent(int x, int z)
    {
        if (_pool.Count > 0 && _pool.Count <= max)
        {
            var chunk = _pool.Pop();
            chunk.Reinitialize(x, z);
            return chunk;
        }

        return null;
    }

    public void Return(TerrainChunk chunk)
    {
        chunk.Remove();
        if (_pool.Count < max)
        {
            chunk.Clear();
            _pool.Push(chunk);
        }
    }

    public void Release()
    {
        _pool.Clear();
    }
}
