using Engine;
using Random = Engine.Random;
namespace MineCraftService.Game;

public enum TreeType
{
    Oak,
    Birch,
    Spruce,
    TallSpruce,
    Mimosa,
    Poplar
}

public static class PlantsManager
{
    private static readonly List<TerrainBrush>[] TreeBrushesByType;

    private static readonly int[] TreeTrunksByType;

    private static readonly int[] TreeLeavesByType;

    static PlantsManager()
    {
        int treeTypeCount = EnumUtils.GetEnumValues(typeof(TreeType)).Max() + 1;
        TreeBrushesByType = new List<TerrainBrush>[treeTypeCount];

        // 定义各树种的树干方块 ID
        TreeTrunksByType =
        [
            (int)BlockType.OakWoodBlock, // Oak
            (int)BlockType.BirchWoodBlock, // Birch
            (int)BlockType.SpruceWoodBlock, // Spruce
            (int)BlockType.SpruceWoodBlock, // TallSpruce
            (int)BlockType.MimosaWoodBlock, // Mimosa
            (int)BlockType.PoplarWoodBlock // Poplar
        ];

        // 定义各树种的叶子方块值
        TreeLeavesByType =
        [
            (int)BlockType.OakLeavesBlock, // Oak
            (int)BlockType.BirchLeavesBlock, // Birch
            (int)BlockType.SpruceLeavesBlock, // Spruce
            (int)BlockType.TallSpruceLeavesBlock, // TallSpruce
            (int)BlockType.MimosaLeavesBlock, // Mimosa
            (int)BlockType.PoplarLeavesBlock // Poplar
        ];

        var random = new Random(33);

        // 初始化每种树的刷子集合
        TreeBrushesByType[(int)TreeType.Oak] = CreateOakBrushes(random);
        TreeBrushesByType[(int)TreeType.Birch] = CreateBirchBrushes(random);
        TreeBrushesByType[(int)TreeType.Spruce] = CreateSpruceBrushes(random);
        TreeBrushesByType[(int)TreeType.TallSpruce] = CreateTallSpruceBrushes(random);
        TreeBrushesByType[(int)TreeType.Mimosa] = CreateMimosaBrushes(random);
        TreeBrushesByType[(int)TreeType.Poplar] = CreatePoplarBrushes(random);
    }

    // 以下是每种树的封装构造函数，便于复用和维护
    private static List<TerrainBrush> CreateOakBrushes(Random random)
    {
        var list = new List<TerrainBrush>();
        int[] heights = { 5, 6, 7, 8, 9, 10, 11, 11, 12, 12, 13, 13, 14, 15, 16, 18 };
        for (int i = 0; i < heights.Length; i++)
        {
            int height = heights[i];
            int branches = (int)MathUtils.Lerp(10f, 22f, i / 16f);
            list.Add(CreateTreeBrush(random,
                GetTreeTrunkValue(TreeType.Oak),
                GetTreeLeavesValue(TreeType.Oak),
                height,
                branches,
                3,
                (y, _) => (y < 0.2f * height) ? 0f : 0.4f * 1.5f,
                y => (y < 0.3f * height || y > 0.9f * height)
                    ? 0f
                    : random.Float(0.33f, 1f) * ((y < 0.7f * height) ? 0.5f : 0.35f) * height));
        }

        return list;
    }

    private static List<TerrainBrush> CreateBirchBrushes(Random random)
    {
        var list = new List<TerrainBrush>();
        int[] heights = { 4, 5, 6, 7, 7, 8, 8, 9, 9, 9, 10, 10, 10, 11, 11, 12 };
        for (int i = 0; i < heights.Length; i++)
        {
            int height = heights[i];
            int branches = (int)MathUtils.Lerp(0f, 20f, i / 16f);
            list.Add(CreateTreeBrush(random,
                GetTreeTrunkValue(TreeType.Birch),
                GetTreeLeavesValue(TreeType.Birch),
                height,
                branches,
                3,
                (y, _) => (y < (height / 2) - 1) ? 0f : 0.66f * 1.5f,
                y => (y < height * 0.35f || y > height * 0.75f) ? 0f : random.Float(0f, 0.33f * height)));
        }

        return list;
    }

    private static List<TerrainBrush> CreateSpruceBrushes(Random random)
    {
        var list = new List<TerrainBrush>();
        int[] heights = { 7, 8, 9, 10, 10, 11, 11, 12, 12, 13, 13, 14, 14, 15, 16, 17 };
        foreach (var height in heights)
        {
            int branches = height * 3;
            list.Add(CreateTreeBrush(random,
                GetTreeTrunkValue(TreeType.Spruce),
                GetTreeLeavesValue(TreeType.Spruce),
                height,
                branches,
                3,
                (y, _) =>
                {
                    if (y < 3) return 0f;
                    float r = MathUtils.Lerp(1.4f, 0.3f, y / (float)height);
                    return (y % 2 == 0) ? r * 0.3f : r;
                },
                y => (y < 3 || y > height * 0.8f || y % 2 == 0)
                    ? 0f
                    : MathUtils.Lerp(0.3f * height, 0f, MathUtils.Saturate(y / (float)height))));
        }

        return list;
    }

    private static List<TerrainBrush> CreateTallSpruceBrushes(Random random)
    {
        var list = new List<TerrainBrush>();
        int[] heights = { 20, 21, 22, 23, 24, 24, 25, 25, 26, 26, 27, 27, 28, 28, 29, 30 };
        for (int i = 0; i < heights.Length; i++)
        {
            int height = heights[i];
            int branches = height * 3;
            float start = (0.3f + (i % 4) * 0.05f) * height;
            list.Add(CreateTreeBrush(random,
                GetTreeTrunkValue(TreeType.TallSpruce),
                GetTreeLeavesValue(TreeType.TallSpruce),
                height,
                branches,
                3,
                (y, _) =>
                {
                    float t = y / (float)height;
                    float r = MathUtils.Lerp(1.5f, 0f, MathUtils.Saturate((t - 0.6f) / 0.4f));
                    if (y < start || (y % 3 != 0 && y < height - 4)) return 0f;
                    return (y % 3 != 0) ? r * 0.2f : r;
                },
                y =>
                {
                    float t = y / (float)height;
                    if (y % 3 != 0) return 0f;
                    if (y < start - 4) return 0f;
                    if (y < start) return 0.1f * height;
                    return MathUtils.Lerp(0.18f * height, 0f, MathUtils.Saturate((t - 0.6f) / 0.4f));
                }));
        }

        return list;
    }

    private static List<TerrainBrush> CreateMimosaBrushes(Random random)
    {
        var list = new List<TerrainBrush>();
        for (int i = 0; i < 16; i++)
        {
            float height = MathUtils.Lerp(6f, 9f, i / 15f);
            list.Add(CreateMimosaBrush(random, height));
        }

        return list;
    }

    private static List<TerrainBrush> CreatePoplarBrushes(Random random)
    {
        var list = new List<TerrainBrush>();
        int[] heights = { 10, 11, 11, 12, 12, 13, 13, 13, 14, 14, 14, 15, 15, 16, 16, 17 };
        foreach (var height in heights)
        {
            int branches = height * 3;
            list.Add(CreateTreeBrush(random,
                GetTreeTrunkValue(TreeType.Poplar),
                GetTreeLeavesValue(TreeType.Poplar),
                height,
                branches,
                2,
                (y, r) =>
                {
                    int bottom = height < 14 ? 1 : 2;
                    if (y < bottom || (y == bottom && r > 0)) return 0f;
                    return (r == 0) ? 1f : MathUtils.LinearStep(height - 1, bottom + 2, y);
                },
                y => 0f));
        }

        return list;
    }


    public static int GenerateRandomPlantValue(Random random, int groundValue, int temperature, int humidity, int y)
    {
        bool isFrozen = SubsystemWeather.IsPlaceFrozen(temperature, y);

        // 土壤/草地判断
        if (groundValue == (int)BlockType.DirtBlock || groundValue == (int)BlockType.GrassBlock)
        {
            // 湿润环境：可能生长草或植物
            if (humidity >= 6 && random.Float(0f, 1f) < humidity / 60f)
            {
                if (isFrozen)
                    return Terrain.MakeBlockValue((int)BlockType.TallGrassBlock, 
                        PlantBlock.SetSize(0, 1));

                float chance = random.Float(0f, 1f);
                if (chance < 0.04f)
                    return Terrain.MakeBlockValue((int)BlockType.RedFlowerBlock,PlantBlock.SetSize(0, 1));
                if (chance < 0.07f)
                    return Terrain.MakeBlockValue((int)BlockType.PurpleFlowerBlock,PlantBlock.SetSize(0, 1));
                if (chance < 0.09f)
                    return Terrain.MakeBlockValue((int)BlockType.WhiteFlowerBlock,PlantBlock.SetSize(0, 1));
                if (chance < 0.17f)
                    return Terrain.MakeBlockValue((int)BlockType.RyeBlock, 
                        PlantBlock.SetIsWild(PlantBlock.SetSize(0, 7), isWild: true));
                if (chance < 0.19f)
                    return Terrain.MakeBlockValue((int)BlockType.CottonBlock, 
                        PlantBlock.SetIsWild(PlantBlock.SetSize(0, 2), isWild: true));
                return Terrain.MakeBlockValue((int)BlockType.TallGrassBlock, 
                    PlantBlock.SetSize(0, 1));
            }

            // 干燥地区小概率长出干枯灌木
            if (random.Float(0f, 1f) < 0.025f)
            {
                return random.Float(0f, 1f) < 0.2f
                    ? (int)BlockType.LargeDryBushBlock
                    : (int)BlockType.DryBushBlock;
            }
        }

        // 沙地：湿度低时可生成灌木
        if (groundValue == (int)BlockType.SandBlock)
        {
            if (humidity < 8 && random.Float(0f, 1f) < 0.01f)
            {
                return random.Float(0f, 1f) < 0.05f
                    ? (int)BlockType.LargeDryBushBlock
                    : (int)BlockType.DryBushBlock;
            }
        }

        return (int)BlockType.AirBlock;
    }


    public static TreeType? GenerateRandomTreeType(Random random, int temperature, int humidity, int y,
        float densityMultiplier = 1f)
    {
        // 构建一个列表，包含所有候选树种及其带随机因子的概率值
        var treeScores = new Dictionary<TreeType, float>
        {
            { TreeType.Oak, random.Float() * CalculateTreeProbability(TreeType.Oak, temperature, humidity, y) },
            { TreeType.Birch, random.Float() * CalculateTreeProbability(TreeType.Birch, temperature, humidity, y) },
            { TreeType.Spruce, random.Float() * CalculateTreeProbability(TreeType.Spruce, temperature, humidity, y) },
            {
                TreeType.TallSpruce,
                random.Float() * CalculateTreeProbability(TreeType.TallSpruce, temperature, humidity, y)
            },
            { TreeType.Mimosa, random.Float() * CalculateTreeProbability(TreeType.Mimosa, temperature, humidity, y) },
            { TreeType.Poplar, random.Float() * CalculateTreeProbability(TreeType.Poplar, temperature, humidity, y) },
        };

        // 找到最大值对应的树种
        var bestTree = treeScores.OrderByDescending(kv => kv.Value).First();

        // 如果概率值大于 0，且通过密度判断，返回该树种
        if (bestTree.Value > 0f &&
            random.Bool(densityMultiplier * CalculateTreeDensity(bestTree.Key, temperature, humidity, y)))
        {
            return bestTree.Key;
        }

        return null;
    }


    private static float CalculateTreeDensity(TreeType treeType, int temperature, int humidity, int y)
    {
        switch (treeType)
        {
            case TreeType.Oak:
                return RangeProbability(humidity, 4f, 15f, 15f, 15f);
            case TreeType.Birch:
                return RangeProbability(humidity, 4f, 15f, 15f, 15f);
            case TreeType.Spruce:
                return RangeProbability(humidity, 4f, 15f, 15f, 15f);
            case TreeType.TallSpruce:
                return RangeProbability(humidity, 4f, 15f, 15f, 15f);
            case TreeType.Mimosa:
                return 0.04f;
            case TreeType.Poplar:
                return RangeProbability(temperature, 4f, 8f, 10f, 15f) * RangeProbability(humidity, 3f, 15f, 15f, 15f) *
                       RangeProbability(y, 0f, 0f, 85f, 92f);
            default:
                return 0f;
        }
    }

    public static float CalculateTreeProbability(TreeType treeType, int temperature, int humidity, int y)
    {
        switch (treeType)
        {
            case TreeType.Oak:
                return RangeProbability(temperature, 4f, 10f, 15f, 15f) * RangeProbability(humidity, 6f, 8f, 15f, 15f) *
                       RangeProbability(y, 0f, 0f, 82f, 87f);
            case TreeType.Birch:
                return RangeProbability(temperature, 5f, 9f, 11f, 15f) * RangeProbability(humidity, 3f, 15f, 15f, 15f) *
                       RangeProbability(y, 0f, 0f, 82f, 87f);
            case TreeType.Spruce:
                return RangeProbability(temperature, 0f, 0f, 6f, 10f) * RangeProbability(humidity, 3f, 10f, 11f, 12f);
            case TreeType.TallSpruce:
                return 0.25f * RangeProbability(temperature, 0f, 0f, 6f, 10f) *
                       RangeProbability(humidity, 9f, 11f, 15f, 15f) * RangeProbability(y, 0f, 0f, 95f, 100f);
            case TreeType.Mimosa:
                return RangeProbability(temperature, 2f, 4f, 12f, 14f) * RangeProbability(humidity, 0f, 0f, 4f, 6f);
            case TreeType.Poplar:
                return RangeProbability(temperature, 4f, 8f, 12f, 15f) * RangeProbability(humidity, 3f, 15f, 15f, 15f) *
                       RangeProbability(y, 0f, 0f, 85f, 92f);
            default:
                return 0f;
        }
    }


    private static TerrainBrush CreateTreeBrush(Random random, int woodIndex, int leavesIndex, int height,
        int branchesCount, int leavesRounds, Func<int, int, float> leavesProbability, Func<int, float> branchesLength)
    {
        var terrainBrush = new TerrainBrush();

        // 构建主树干
        terrainBrush.AddRay(0, -1, 0, 0, height, 0, 1, 1, 1, woodIndex);

        // 构建枝干
        for (int i = 0; i < branchesCount; i++)
        {
            int baseY = random.Int(0, height);
            float length = branchesLength(baseY);

            // 随机方向并归一化，再乘以枝干长度
            Vector3 dir = Vector3.Normalize(new Vector3(
                random.Float(-1f, 1f),
                random.Float(0f, 0.33f),
                random.Float(-1f, 1f)
            )) * length;

            int endX = (int)MathF.Round(dir.X);
            int endY = baseY + (int)MathF.Round(dir.Y);
            int endZ = (int)MathF.Round(dir.Z);

            // 确定 cutFace（最大变化方向）
            float absX = MathF.Abs(dir.X);
            float absY = MathF.Abs(dir.Y);
            float absZ = MathF.Abs(dir.Z);
            int cutFace = (absX >= absY && absX >= absZ) ? 1 : (absZ >= absY && absZ >= absX) ? 0 : 4;
         
            // 添加枝干射线（使用 cutFace 设置方向）
            terrainBrush.AddRay(0, baseY, 0, endX, endY, endZ, 1, 1, 1,
                (Func<int?, int?>)(v => v.HasValue
                    ? null
                    : Terrain.MakeBlockValue(woodIndex,  WoodBlock.SetCutFace(0, cutFace)))
            );
        }

        // 构建树叶
        for (int round = 0; round < leavesRounds; round++)
        {
            terrainBrush.CalculateBounds(out Point3 min, out Point3 max);

            for (int x = min.X - 1; x <= max.X + 1; x++)
            {
                for (int z = min.Z - 1; z <= max.Z + 1; z++)
                {
                    for (int y = 1; y <= max.Y + 1; y++)
                    {
                        if (random.Float(0f, 1f) >= leavesProbability(y, round))
                            continue;

                        if (terrainBrush.GetValue(x, y, z).HasValue)
                            continue;

                        int neighborLeaves = terrainBrush.CountNonDiagonalNeighbors(x, y, z, leavesIndex);
                        int neighborWood = terrainBrush.CountNonDiagonalNeighbors(x, y, z, (Func<int?, int>)(v =>
                                v.HasValue && Terrain.ExtractContents(v.Value) == woodIndex ? 1 : 0)
                        );

                        if (neighborLeaves > 0 || neighborWood > 0)
                            terrainBrush.AddCell(x, y, z, 0);
                    }
                }
            }

            terrainBrush.Replace(0, leavesIndex); // 将标记替换为树叶
        }

        // 树顶添加额外叶子块
        terrainBrush.AddCell(0, height, 0, leavesIndex);

        terrainBrush.Compile();
        return terrainBrush;
    }


    private static TerrainBrush CreateMimosaBrush(Random random, float size)
    {
        var terrainBrush = new TerrainBrush();
        int wood = TreeTrunksByType[4];
        int leaves = TreeLeavesByType[4];

        // 添加底部主干
        terrainBrush.AddRay(0, -1, 0, 0, 0, 0, 1, 1, 1, wood);

        var branchEnds = new List<Point3>();
        float startAngle = random.Float(0f, MathF.PI * 2f);

        // 生成 3 根放射状枝干
        for (int i = 0; i < 3; i++)
        {
            float angle = startAngle + i * MathUtils.DegToRad(120f);
            Vector3 dir = Vector3.Normalize(new Vector3(1f, random.Float(1f, 1.5f), 0f));
            dir = Vector3.Transform(dir, Matrix.CreateRotationY(angle));

            int branchLength = random.Int((int)(0.7f * size), (int)size);
            Vector3 endPos = Vector3.Round(dir * branchLength);
            Point3 end = new Point3((int)endPos.X, (int)endPos.Y, (int)endPos.Z);

            terrainBrush.AddRay(0, 0, 0, end.X, end.Y, end.Z, 1, 1, 1, wood);
            branchEnds.Add(end);
        }

        // 在每个枝干末端生成叶子团
        foreach (Point3 center in branchEnds)
        {
            float radius = random.Float(0.3f * size, 0.45f * size);
            int rCeil = (int)MathF.Ceiling(radius);

            for (int x = center.X - rCeil; x <= center.X + rCeil; x++)
            {
                for (int y = center.Y - rCeil; y <= center.Y + rCeil; y++)
                {
                    for (int z = center.Z - rCeil; z <= center.Z + rCeil; z++)
                    {
                        if (terrainBrush.GetValue(x, y, z).HasValue)
                            continue;

                        int manhattan = Math.Abs(x - center.X) + Math.Abs(y - center.Y) + Math.Abs(z - center.Z);
                        Vector3 offset = new Vector3(x - center.X, (y - center.Y) * 1.7f, z - center.Z);
                        float dist = offset.Length();

                        if (dist <= radius && (radius - dist > 1f || manhattan <= 2 || random.Bool(0.7f)))
                        {
                            terrainBrush.AddCell(x, y, z, leaves);
                        }
                    }
                }
            }
        }

        terrainBrush.Compile();
        return terrainBrush;
    }


    private static float RangeProbability(float v, float a, float b, float c, float d)
    {
        if (v < a)
            return 0f;

        if (v < b)
            return (v - a) / (b - a);

        if (v <= c)
            return 1f;

        if (v <= d)
            return 1f - ((v - c) / (d - c));

        return 0f;
    }


    public static int GetTreeTrunkValue(TreeType treeType)
    {
        return TreeTrunksByType[(int)treeType];
    }

    public static int GetTreeLeavesValue(TreeType treeType)
    {
        return TreeLeavesByType[(int)treeType];
    }


    public static ReadOnlyList<TerrainBrush> GetTreeBrushes(TreeType treeType)
    {
        return new ReadOnlyList<TerrainBrush>(TreeBrushesByType[(int)treeType]);
    }
}