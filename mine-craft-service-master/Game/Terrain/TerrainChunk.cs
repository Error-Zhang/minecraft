using Engine;

namespace MineCraftService.Game;

public class ChunkSetting
{
    public static int MaxHeight = 256;
    public static int MinHeight = 32;
    public static int MaxSize = 24;
    public static int MinSize = 8;

    // 区块宽度（X/Z方向的尺寸，单位：方块），能被根号2开方
    public int ChunkSize { get; private set; } = 16
        
        ;

    // 区块垂直高度（Y方向的最大高度，单位：方块），范围 32~256，必须为 16 的倍数
    public int ChunkHeight { get; private set; } = 128;

    // 基准高度
    public int BaseHeight => 64;

    // 可生成地表的最低高度
    public int SurfaceMinHeight => BaseHeight / 2;

    // 可生成地表的最高高度
    public int SurfaceMaxHeight => ChunkHeight - ChunkHeightUnit / 2;

    // 单位大小（用于划分子块等用途）
    public int ChunkSizeUnit => 4;

    public int ChunkHeightUnit => 8;

    // 默认构造函数（使用默认值）
    public ChunkSetting()
    {
    }

    // 构造函数（带参数校验）
    public ChunkSetting(int chunkSize, int chunkHeight)
    {
        // 检查是否为 4 的倍数
        if (chunkSize % ChunkSizeUnit != 0)
            throw new ArgumentException("ChunkSize must be a multiple of 4.");

        if (chunkHeight % ChunkHeightUnit != 0)
            throw new ArgumentException("ChunkHeight must be a multiple of 4.");

        // 检查是否在有效范围内
        if (chunkSize < MinSize || chunkSize > MaxSize)
            throw new ArgumentOutOfRangeException(nameof(chunkSize),
                "ChunkSize must be between 8 and 24.");

        if (chunkHeight < MinHeight || chunkHeight > MaxHeight)
            throw new ArgumentOutOfRangeException(nameof(chunkHeight),
                "ChunkHeight must be between 32 and 256.");

        ChunkSize = chunkSize;
        ChunkHeight = chunkHeight;
    }
}

public class TerrainChunkExport
{
    public int X { get; set; }
    public int Z { get; set; }
    public int[] Cells { get; set; }
    public int[] Shafts { get; set; }
}

public class TerrainChunk
{
    public int CurrentPhase { get; set; } = 0;

    public readonly Terrain Terrain;

    public Point2 Coords;
    public Point2 Origin;

    public readonly int[] Cells;
    public readonly int[] Shafts;

    public readonly ChunkSetting ChunkSetting;

    public TerrainChunk(Terrain terrain, ChunkSetting setting, int x, int z)
    {
        Terrain = terrain;
        ChunkSetting = setting;

        Coords = new Point2(x, z);
        Origin = new Point2(x * ChunkSetting.ChunkSize, z * ChunkSetting.ChunkSize);

        int totalCells = ChunkSetting.ChunkSize * ChunkSetting.ChunkSize * ChunkSetting.ChunkHeight;
        Cells = new int[totalCells];

        int shaftCount = ChunkSetting.ChunkSize * ChunkSetting.ChunkSize;
        Shafts = new int[shaftCount];
        terrain.ChunksStorage.TryAdd(this);
    }

    public void Reinitialize(int x, int z)
    {
        Coords = new Point2(x, z);
        Origin = new Point2(x * ChunkSetting.ChunkSize, z * ChunkSetting.ChunkSize);
    }

    public void Remove()
    {
        Terrain.ChunksStorage.TryRemove(Coords.X, Coords.Y);
    }

    public void Clear()
    {
        // 仅重置，不会释放内存
        Array.Clear(Cells, 0, Cells.Length);
        Array.Clear(Shafts, 0, Shafts.Length);
    }

    /**
     * 使用RLE(相同合并)算法进行压缩，压缩率约为92% 65536->5000
     */
    public int[] CompressRLE(int[] cells)
    {
        if (cells.Length == 0) return Array.Empty<int>();

        var result = new List<int>();
        int current = cells[0];
        int count = 1;

        for (int i = 1; i < cells.Length; i++)
        {
            if (cells[i] == current)
            {
                count++;
            }
            else
            {
                // 把上一段重复的值和计数添加进去
                result.Add(current);
                result.Add(count);

                current = cells[i];
                count = 1;
            }
        }

        // 最后一段数据
        result.Add(current);
        result.Add(count);

        return result.ToArray();
    }

    public TerrainChunkExport ToExportFormat()
    {
        var export = new TerrainChunkExport
        {
            X = Coords.X,
            Z = Coords.Y,
            Cells = CompressRLE(Cells),
            Shafts = CompressRLE(Shafts)
        };


        return export;
    }

    public int GetCellIndex(int x, int y, int z)
    {
        int size = ChunkSetting.ChunkSize;
        int height = ChunkSetting.ChunkHeight;
        return y + (x * height) + (z * height * size);
    }

    public virtual int GetCellValueFast(int index)
    {
        return Cells[index];
    }

    public virtual int GetCellValueFast(int x, int y, int z)
    {
        return Cells[GetCellIndex(x, y, z)];
    }

    public virtual void SetCellValueFast(int x, int y, int z, int value)
    {
        Cells[GetCellIndex(x, y, z)] = value;
    }

    public virtual void SetCellValueFast(int x, int y, int z, BlockType value)
    {
        Cells[GetCellIndex(x, y, z)] = (int)value;
    }

    public virtual void SetCellValueFast(int index, int value)
    {
        Cells[index] = value;
    }

    public virtual int GetCellContentsFast(int x, int y, int z)
    {
        return Terrain.ExtractContents(GetCellValueFast(x, y, z));
    }

    public virtual int GetShaftValueFast(int x, int z)
    {
        return Shafts[x + (z * ChunkSetting.ChunkSize)];
    }

    public virtual void SetShaftValueFast(int x, int z, int value)
    {
        Shafts[x + (z * ChunkSetting.ChunkSize)] = value;
    }

    public virtual int GetTemperatureFast(int x, int z)
    {
        return Terrain.ExtractTemperature(GetShaftValueFast(x, z));
    }

    public virtual void SetTemperatureFast(int x, int z, int temperature)
    {
        SetShaftValueFast(x, z, Terrain.ReplaceTemperature(GetShaftValueFast(x, z), temperature));
    }

    public virtual int GetHumidityFast(int x, int z)
    {
        return Terrain.ExtractHumidity(GetShaftValueFast(x, z));
    }

    public virtual void SetHumidityFast(int x, int z, int humidity)
    {
        SetShaftValueFast(x, z, Terrain.ReplaceHumidity(GetShaftValueFast(x, z), humidity));
    }

    public virtual int CalculateTopmostCellHeight(int x, int z)
    {
        int height = ChunkSetting.ChunkHeight;
        for (int y = height - 1; y >= 0; y--)
        {
            if (Terrain.ExtractContents(GetCellValueFast(x, y, z)) != 0)
            {
                return y;
            }
        }

        return 0;
    }

}