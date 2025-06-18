using System.Collections.Concurrent;
using Engine;

namespace MineCraftService.Game;

public class ChunksStorage
{
    private readonly ConcurrentDictionary<(int x, int y), TerrainChunk> _map = new();

    public TerrainChunk Get(int x, int y) => _map[(x, y)];

    public bool TryGet(int x, int y, out TerrainChunk? chunk) =>
        _map.TryGetValue((x, y), out chunk);

    public bool TryAdd(TerrainChunk chunk) => _map.TryAdd((chunk.Coords.X, chunk.Coords.Y), chunk);

    public void TryRemove(int x, int y) => _map.TryRemove((x, y), out _);

    public bool Contains(int x, int y) => _map.ContainsKey((x, y));
}

public class Terrain(ChunkPool chunkPool, int chunkSize)
{
    public readonly ChunksStorage ChunksStorage = new();
    public ChunkPool ChunkPool { get; set; } = chunkPool;

    public int ChunkSize { get; } = chunkSize;
    private int ChunkShift => (int)MathF.Log2(ChunkSize);
    private int ChunkMask => ChunkSize - 1;

    public virtual bool TryGetChunkAtCoords(int chunkX, int chunkZ, out TerrainChunk? chunk)
    {
        return ChunksStorage.TryGet(chunkX, chunkZ, out chunk);
    }

    public virtual TerrainChunk GetChunkAtCoords(int chunkX, int chunkZ)
    {
        return ChunksStorage.Get(chunkX, chunkZ);
    }

    public virtual TerrainChunk GetChunkAtCell(int x, int z)
    {
        return GetChunkAtCoords(x >> ChunkShift, z >> ChunkShift);
    }

    public Point2 ToChunk(Vector2 p)
    {
        return ToChunk(ToCell(p.X), ToCell(p.Y));
    }

    public Point2 ToChunk(int x, int z)
    {
        return new Point2(x >> ChunkShift, z >> ChunkShift);
    }

    public static int ToCell(float x)
    {
        return (int)MathF.Floor(x);
    }

    public virtual bool IsCellValid(int x, int y, int z)
    {
        return y >= 0 && y < 256;
    }

    public virtual int GetCellValue(int x, int y, int z)
    {
        return !IsCellValid(x, y, z) ? 0 : GetCellValueFast(x, y, z);
    }

    public virtual int GetCellContents(int x, int y, int z)
    {
        return !IsCellValid(x, y, z) ? 0 : GetCellContentsFast(x, y, z);
    }

    public virtual int GetCellValueFast(int x, int y, int z)
    {
        return GetChunkAtCell(x, z)?.GetCellValueFast(x & ChunkMask, y, z & ChunkMask) ?? 0;
    }

    public virtual int GetCellValueFastChunkExists(int x, int y, int z)
    {
        return GetChunkAtCell(x, z).GetCellValueFast(x & ChunkMask, y, z & ChunkMask);
    }

    public virtual int GetCellContentsFast(int x, int y, int z)
    {
        return ExtractContents(GetCellValueFast(x, y, z));
    }

    public virtual void SetCellValueFast(int x, int y, int z, int value)
    {
        GetChunkAtCell(x, z)?.SetCellValueFast(x & ChunkMask, y, z & ChunkMask, value);
    }

    public virtual int CalculateTopmostCellHeight(int x, int z)
    {
        return GetChunkAtCell(x, z)?.CalculateTopmostCellHeight(x & ChunkMask, z & ChunkMask) ?? 0;
    }

    public virtual int GetShaftValue(int x, int z)
    {
        return GetChunkAtCell(x, z)?.GetShaftValueFast(x & ChunkMask, z & ChunkMask) ?? 0;
    }

    public virtual void SetShaftValue(int x, int z, int value)
    {
        GetChunkAtCell(x, z)?.SetShaftValueFast(x & ChunkMask, z & ChunkMask, value);
    }

    public virtual int GetTemperature(int x, int z)
    {
        return ExtractTemperature(GetShaftValue(x, z));
    }

    public virtual int GetHumidity(int x, int z)
    {
        return ExtractHumidity(GetShaftValue(x, z));
    }

    public static int MakeBlockValue(int contents, int data)
    {
        return (contents & 0xFFF) | ((data & 0xF) << 12);
    }

    public static int ExtractContents(int value)
    {
        return value & 0xFFF;
    }

    public static int ExtractData(int value)
    {
        return (value >> 12) & 0xF;
    }

    public static int ExtractTopHeight(int value)
    {
        return value & 0xFF;
    }

    public static int ExtractBottomHeight(int value)
    {
        return (value & 0xFF0000) >> 16;
    }

    public static int ExtractSunlightHeight(int value)
    {
        return (value >>> 24);
    }

    public static int ExtractHumidity(int value)
    {
        return (value & 0xF000) >> 12;
    }

    public static int ExtractTemperature(int value)
    {
        return (value & 0xF00) >> 8;
    }

    public static int ReplaceContents(int value, int contents)
    {
        return (value & ~0xFFF) | (contents & 0xFFF);
    }

    public static int ReplaceContents(int contents)
    {
        return contents & 0xFFF;
    }

    public static int ReplaceData(int value, int data)
    {
        return (value & ~(0xF << 12)) | ((data & 0xF) << 12);
    }

    public static int ReplaceHumidity(int value, int humidity)
    {
        return value ^ ((value ^ (humidity << 12)) & 0xF000);
    }

    public static int ReplaceTemperature(int value, int temperature)
    {
        return value ^ ((value ^ (temperature << 8)) & 0xF00);
    }
}
