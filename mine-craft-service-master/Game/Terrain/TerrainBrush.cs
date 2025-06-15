using Engine;

namespace MineCraftService.Game;

public class Brush
{
    private readonly int? _value;
    private readonly Func<int?, int?> _valueHandler;
    private readonly Func<Point3, int?> _positionHandler;

    public static implicit operator Brush(int value) => new(value);
    public static implicit operator Brush(Func<int?, int?> handler) => new(handler);
    public static implicit operator Brush(Func<Point3, int?> handler) => new(handler);

    private Brush(int value) => _value = value;
    private Brush(Func<int?, int?> handler) => _valueHandler = handler;
    private Brush(Func<Point3, int?> handler) => _positionHandler = handler;

    public int? Paint(TerrainBrush terrainBrush, Point3 p)
    {
        if (_valueHandler is not null)
            return _valueHandler(terrainBrush.GetValue(p.X, p.Y, p.Z));

        if (_positionHandler is not null)
            return _positionHandler(p);

        return _value;
    }
}

public class Counter
{
    private readonly int? _expectedValue;
    private readonly Func<int?, int> _valueHandler;
    private readonly Func<Point3, int> _positionHandler;

    public static implicit operator Counter(int value) => new(value);
    public static implicit operator Counter(Func<int?, int> handler) => new(handler);
    public static implicit operator Counter(Func<Point3, int> handler) => new(handler);

    private Counter(int value) => _expectedValue = value;
    private Counter(Func<int?, int> handler) => _valueHandler = handler;
    private Counter(Func<Point3, int> handler) => _positionHandler = handler;

    public int Count(TerrainBrush terrainBrush, Point3 p)
    {
        if (_valueHandler is not null)
            return _valueHandler(terrainBrush.GetValue(p));

        if (_positionHandler is not null)
            return _positionHandler(p);

        return terrainBrush.GetValue(p) == _expectedValue ? 1 : 0;
    }
}

public class TerrainBrush
{
    public struct Cell : IComparable<Cell>
    {
        public sbyte X;

        public sbyte Y;

        public sbyte Z;

        public int Value;

        public int CompareTo(Cell other)
        {
            return Key(X, Y, Z) - Key(other.X, other.Y, other.Z);
        }
    }


    public Dictionary<int, Cell> m_cellsDictionary = [];

    public Cell[] m_cells;

    public Cell[] Cells => m_cells;

    public static int Key(int x, int y, int z)
    {
        return y + 128 + ((x + 128) << 8) + ((z + 128) << 16);
    }

    public void Compile()
    {
        m_cells = new Cell[m_cellsDictionary.Values.Count];
        int num = 0;
        foreach (Cell value in m_cellsDictionary.Values)
        {
            m_cells[num++] = value;
        }

        Array.Sort(m_cells);
        m_cellsDictionary = null;
    }

    public int CountNonDiagonalNeighbors(int x, int y, int z, Counter counter)
    {
        return 0 + counter.Count(this, new Point3(x - 1, y, z)) +
               counter.Count(this, new Point3(x + 1, y, z)) +
               counter.Count(this, new Point3(x, y - 1, z)) +
               counter.Count(this, new Point3(x, y + 1, z)) +
               counter.Count(this, new Point3(x, y, z - 1)) +
               counter.Count(this, new Point3(x, y, z + 1));
    }

    public void Replace(int oldValue, int newValue)
    {
        var dictionary = new Dictionary<int, Cell>();
        foreach (KeyValuePair<int, Cell> item in m_cellsDictionary)
        {
            Cell value = item.Value;
            if (value.Value == oldValue)
            {
                value.Value = newValue;
            }

            dictionary[item.Key] = value;
        }

        m_cellsDictionary = dictionary;
        m_cells = null;
    }

    public void CalculateBounds(out Point3 min, out Point3 max)
    {
        min = Point3.Zero;
        max = Point3.Zero;
        bool flag = true;
        foreach (Cell value in m_cellsDictionary.Values)
        {
            if (flag)
            {
                flag = false;
                min.X = max.X = value.X;
                min.Y = max.Y = value.Y;
                min.Z = max.Z = value.Z;
            }
            else
            {
                min.X = MathUtils.Min(min.X, value.X);
                min.Y = MathUtils.Min(min.Y, value.Y);
                min.Z = MathUtils.Min(min.Z, value.Z);
                max.X = MathUtils.Max(max.X, value.X);
                max.Y = MathUtils.Max(max.Y, value.Y);
                max.Z = MathUtils.Max(max.Z, value.Z);
            }
        }
    }

    public int? GetValue(Point3 p)
    {
        return GetValue(p.X, p.Y, p.Z);
    }

    public int? GetValue(int x, int y, int z)
    {
        int key = Key(x, y, z);
        if (m_cellsDictionary.TryGetValue(key, out Cell value))
        {
            return value.Value;
        }

        return null;
    }

    public void AddCell(int x, int y, int z, Brush brush)
    {
        int? num = brush.Paint(this, new Point3(x, y, z));
        if (num.HasValue)
        {
            int key = Key(x, y, z);
            m_cellsDictionary[key] = new Cell
            {
                X = (sbyte)x,
                Y = (sbyte)y,
                Z = (sbyte)z,
                Value = num.Value
            };
            m_cells = null;
        }
    }

    public void AddBox(int x, int y, int z, int sizeX, int sizeY, int sizeZ, Brush brush)
    {
        for (int i = x; i < x + sizeX; i++)
        {
            for (int j = y; j < y + sizeY; j++)
            {
                for (int k = z; k < z + sizeZ; k++)
                {
                    AddCell(i, j, k, brush);
                }
            }
        }
    }

    public void AddRay(int x1, int y1, int z1, int x2, int y2, int z2, int sizeX, int sizeY,
        int sizeZ, Brush brush)
    {
        Vector3 vector = new Vector3(x1, y1, z1) + new Vector3(0.5f);
        Vector3 vector2 = new Vector3(x2, y2, z2) + new Vector3(0.5f);
        Vector3 vector3 = 0.33f * Vector3.Normalize(vector2 - vector);
        int num = (int)MathF.Round(3f * Vector3.Distance(vector, vector2));
        Vector3 vector4 = vector;
        for (int i = 0; i < num; i++)
        {
            int x3 = Terrain.ToCell(vector4.X);
            int y3 = Terrain.ToCell(vector4.Y);
            int z3 = Terrain.ToCell(vector4.Z);
            AddBox(x3, y3, z3, sizeX, sizeY, sizeZ, brush);
            vector4 += vector3;
        }
    }

    public void PaintFastSelective(TerrainChunk chunk, int x, int y, int z, int onlyInValue)
    {
        x -= chunk.Origin.X;
        z -= chunk.Origin.Y;


        foreach (var cell in Cells)
        {
            int cx = cell.X + x;
            int cy = cell.Y + y;
            int cz = cell.Z + z;

            if (cx >= 0 && cx < chunk.ChunkSetting.ChunkSize &&
                cy >= 0 && cy < chunk.ChunkSetting.ChunkHeight &&
                cz >= 0 && cz < chunk.ChunkSetting.ChunkSize)
            {
                int index = chunk.GetCellIndex(cx, cy, cz);
                int cellValueFast = chunk.GetCellValueFast(index);
                if (onlyInValue == cellValueFast)
                {
                    chunk.SetCellValueFast(index, cell.Value);
                }
            }
        }
    }

    public void PaintFastAvoidWater(TerrainChunk chunk, int x, int y, int z)
    {
        Terrain terrain = chunk.Terrain;

        x -= chunk.Origin.X;
        z -= chunk.Origin.Y;

        var water = (int)BlockType.WaterBlock;

        foreach (var cell in Cells)
        {
            int cx = cell.X + x;
            int cy = cell.Y + y;
            int cz = cell.Z + z;

            // 确保在区块本地坐标范围内
            if (cx >= 0 && cx < chunk.ChunkSetting.ChunkSize && cy >= 0 &&
                cy < chunk.ChunkSetting.ChunkHeight && cz >= 0 && cz < chunk.ChunkSetting.ChunkSize)
            {
                // 转换为世界坐标
                int wx = cx + chunk.Origin.X;
                int wz = cz + chunk.Origin.Y;

                // 避免写入水块及其邻接
                if (chunk.GetCellContentsFast(cx, cy, cz) != water &&
                    terrain.GetCellContents(wx - 1, cy, wz) != water &&
                    terrain.GetCellContents(wx + 1, cy, wz) != water &&
                    terrain.GetCellContents(wx, cy, wz - 1) != water &&
                    terrain.GetCellContents(wx, cy, wz + 1) != water &&
                    chunk.GetCellContentsFast(cx, cy + 1, cz) != water)
                {
                    chunk.SetCellValueFast(cx, cy, cz, cell.Value);
                }
            }
        }
    }


    // 向区块中绘制方块
    public void PaintFast(TerrainChunk chunk, int x, int y, int z)
    {
        x -= chunk.Origin.X;
        z -= chunk.Origin.Y;

        foreach (var cell in Cells)
        {
            int cx = cell.X + x;
            int cy = cell.Y + y;
            int cz = cell.Z + z;

            if (cx >= 0 && cx < chunk.ChunkSetting.ChunkSize &&
                cy >= 0 && cy < chunk.ChunkSetting.ChunkHeight &&
                cz >= 0 && cz < chunk.ChunkSetting.ChunkSize)
            {
                chunk.SetCellValueFast(cx, cy, cz, cell.Value);
            }
        }
    }
}