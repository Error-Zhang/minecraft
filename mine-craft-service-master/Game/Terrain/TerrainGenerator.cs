using Engine;
using MineCraftService.Engine;
using Random = Engine.Random;

namespace MineCraftService.Game;

public class CavePoint
{
    public Vector3 Position;

    public Vector3 Direction;

    public int BrushType;

    public int Length;

    public int StepsTaken;
}

public class TerrainGenerator
{
    private readonly WorldSetting _worldSetting;

    private readonly int _seed;

    private readonly Vector2? _islandSize;

    private readonly Vector2 _oceanCorner;

    private readonly Vector2 _temperatureOffset;

    private readonly Vector2 _humidityOffset;

    private readonly Vector2 _mountainsOffset;

    private readonly Vector2 _riversOffset;

    private readonly float _biomeScaling;

    private readonly float _shoreFluctuations;

    private readonly float _shoreFluctuationsScaling;

    private readonly float _oceanSlope;

    private readonly float _oceanSlopeVariation;

    private readonly float _islandsFrequency;

    private readonly float _densityBias;

    private readonly float _heightBias;

    private readonly float _hillsPercentage;

    private readonly float _hillsStrength;

    private readonly int _hillsOctaves;

    private readonly float _hillsFrequency;

    private readonly float _hillsPersistence;

    private readonly float _mountainsStrength;

    private readonly float _mountainRangeFreq;

    private readonly float _mountainsPercentage;

    private static float _mountainsDetailFreq;

    private static int _mountainsDetailOctaves;

    private static float _mountainsDetailPersistence;

    private readonly float _riversStrength;

    private readonly float _turbulenceStrength;

    private readonly float _turbulenceFreq;

    private readonly int _turbulenceOctaves;

    private readonly float _turbulencePersistence;

    private readonly float _minTurbulence;

    private readonly float _turbulenceZero;

    private static float _surfaceMultiplier;

    private static readonly List<List<TerrainBrush>> CaveBrushesByType = new();

    private readonly Terrain _terrain;

    public TerrainGenerator(Terrain terrain, WorldSetting worldSetting)
    {
        _seed = worldSetting.GetSeedToInt();
        Random random = new Random(_seed);
        _terrain = terrain;
        _worldSetting = worldSetting;
        _islandSize = _worldSetting.WorldMode == WorldMode.Island ? new(400f, 400f) : null;
        float num = _islandSize.HasValue
            ? MathUtils.Min(_islandSize.Value.X, _islandSize.Value.Y)
            : float.MaxValue;
        _oceanCorner = new Vector2(-200f, -200f);
        _temperatureOffset = new Vector2(random.Float(-3000f, 3000f), random.Float(-3000f, 3000f));
        _humidityOffset = new Vector2(random.Float(-3000f, 3000f), random.Float(-3000f, 3000f));
        _mountainsOffset = new Vector2(random.Float(-3000f, 3000f), random.Float(-3000f, 3000f));
        _riversOffset = new Vector2(random.Float(-3000f, 3000f), random.Float(-3000f, 3000f));
        _biomeScaling = ((_worldSetting.WorldMode == WorldMode.Island) ? 1f : 1.75f) *
                        _worldSetting.BiomeSize;
        _shoreFluctuations = Math.Clamp(2f * num, 0f, 150f);
        _shoreFluctuationsScaling = Math.Clamp(0.04f * num, 0.5f, 3f);
        _oceanSlope = 0.006f;
        _oceanSlopeVariation = 0.004f;
        _islandsFrequency = 0.01f;
        _densityBias = 55f;
        _heightBias = 1f;
        _riversStrength = 1f;
        _mountainsStrength = 220f;
        _mountainRangeFreq = 0.0006f;
        _mountainsPercentage = 0.11f;
        _mountainsDetailFreq = 0.003f;
        _mountainsDetailOctaves = 3;
        _mountainsDetailPersistence = 0.53f;
        _hillsPercentage = 0.28f;
        _hillsStrength = 32f;
        _hillsOctaves = 1;
        _hillsFrequency = 0.014f;
        _hillsPersistence = 0.5f;
        _turbulenceStrength = 55f;
        _turbulenceFreq = 0.03f;
        _turbulenceOctaves = 1;
        _turbulencePersistence = 0.5f;
        _minTurbulence = 0.04f;
        _turbulenceZero = 0.84f;
        _surfaceMultiplier = 2f;

        SetGenerator();
    }

    public readonly List<ChunkGenerationStep> ChunkGenerationStep1 = new();
    public readonly List<ChunkGenerationStep> ChunkGenerationStep2 = new();
    public readonly List<ChunkGenerationStep> ChunkGenerationStep3 = new();

    // 注意:后续阶段都依赖第一阶段的数据，并且后续阶段会尝试访问相邻区块数据，所有必须先生成1阶段的全部数据
    private void SetGenerator()
    {
        // Step 1: 地表生成阶段
        // 第一步：生成表面参数，设置高度、地形等
        ChunkGenerationStep1.Add(
            new ChunkGenerationStep(100, GenerateSurfaceParameters));
        // 第二步：生成地形，生成块的形态、材料等
        ChunkGenerationStep1.Add(new ChunkGenerationStep(200, GenerateTerrain));
        ChunkGenerationStep1.Add(new ChunkGenerationStep(999, chunk => chunk.CurrentPhase++));

        // Step 2: 细节生成阶段
        // 第一步：生成洞穴
        ChunkGenerationStep2.Add(new ChunkGenerationStep(100, GenerateCaves));
        // 第二步：生成小空洞
        ChunkGenerationStep2.Add(new ChunkGenerationStep(200, GeneratePockets));
        // 第三步：生成矿物（如铁矿、金矿等）
        ChunkGenerationStep2.Add(new ChunkGenerationStep(300, GenerateMinerals));
        // 第四步：生成地面（如草地、泥土等）
        ChunkGenerationStep2.Add(new ChunkGenerationStep(400, GenerateSurface));
        // 第五步：处理流体流动
        ChunkGenerationStep3.Add(new ChunkGenerationStep(500, PropagateFluidsDownwards));
        ChunkGenerationStep2.Add(new ChunkGenerationStep(999, chunk => chunk.CurrentPhase++));

        // Step 3: 生物生成阶段
        // 第一步：生成草和植物
        ChunkGenerationStep3.Add(new ChunkGenerationStep(100, GenerateGrassAndPlants));
        // 第二步：生成树木的树干
        ChunkGenerationStep3.Add(new ChunkGenerationStep(200, GenerateLogs));
        // 第三步：生成完整的树木
        ChunkGenerationStep3.Add(new ChunkGenerationStep(300, GenerateTrees));
        // 第四步：生成仙人掌
        ChunkGenerationStep3.Add(new ChunkGenerationStep(400, GenerateCacti));
        ChunkGenerationStep3.Add(new ChunkGenerationStep(999, chunk => chunk.CurrentPhase++));


        // 对每个生成步骤按执行顺序进行排序
        ChunkGenerationStep1.Sort((a, b) => (a.GenerateOrder.CompareTo(b.GenerateOrder)));
        ChunkGenerationStep2.Sort((a, b) => (a.GenerateOrder.CompareTo(b.GenerateOrder)));
        ChunkGenerationStep3.Sort((a, b) => (a.GenerateOrder.CompareTo(b.GenerateOrder)));
    }

    private float CalculateHeight(ChunkSetting chunkSetting, float x, float z)
    {
        // ===== 控制山地高度缩放比例 =====
        const float mountainHeightScale = 0.4f;

        // ===== 基础坡度影响（远离大陆边缘时的基础上升趋势）=====
        float oceanSlopeNoise =
            SimplexNoise.OctaveNoise(x + _mountainsOffset.X, z + _mountainsOffset.Y, 0.01f, 1, 2f,
                0.5f);
        float oceanSlope = _oceanSlope +
                           _oceanSlopeVariation *
                           MathUtils.PowSign(2f * oceanSlopeNoise - 1f, 0.5f);

        float shoreDistance = CalculateOceanShoreDistance(x, z); // 越靠近陆地中心值越大
        float shoreFade = MathUtils.Saturate(2f - 0.05f * MathF.Abs(shoreDistance)); // 海岸线模糊区域
        float islandFactor =
            MathUtils.Saturate(MathF.Sin(_islandsFrequency * shoreDistance)); // 是否属于岛屿带

        // ===== 岛屿 / 远离海洋时的额外调整 =====
        float islandDrop =
            MathUtils.Saturate(MathUtils.Saturate(-oceanSlope * shoreDistance) -
                               0.85f * islandFactor);
        float oceanDrop =
            MathUtils.Saturate(MathUtils.Saturate(0.05f * (-shoreDistance - 10f)) - islandFactor);

        // ===== 获取地形类型参数 =====
        float mountainFactor = CalculateMountainRangeFactor(x, z); // 越靠近山地值越大

        // ===== Biome 噪声用于后续插值 =====
        float biomeNoise1 = (1f - shoreFade) *
                            SimplexNoise.OctaveNoise(x, z, 0.001f / _biomeScaling, 2, 2f, 0.5f);
        float biomeNoise2 = (1f - shoreFade) *
                            SimplexNoise.OctaveNoise(x, z, 0.0017f / _biomeScaling, 2, 4f, 0.7f);

        // ===== 丘陵/山地的参与程度 =====
        float hillsBlend = (1f - oceanDrop) * (1f - shoreFade) *
                           Squish(mountainFactor, 1f - _hillsPercentage, 1f - _mountainsPercentage);
        float mountainsBlend =
            (1f - oceanDrop) * Squish(mountainFactor, 1f - _mountainsPercentage, 1f);

        // ===== 丘陵噪声形状 =====
        float hillsShape = SimplexNoise.OctaveNoise(x, z, _hillsFrequency, _hillsOctaves, 1.93f,
            _hillsPersistence);

        // ===== 山地噪声形状 =====
        float mountainDetailPersistence = MathUtils.Lerp(0.75f * _mountainsDetailPersistence,
            1.33f * _mountainsDetailPersistence, biomeNoise1);
        float mountainShape = 1.5f * SimplexNoise.OctaveNoise(
                                  x, z, _mountainsDetailFreq, _mountainsDetailOctaves, 1.98f,
                                  mountainDetailPersistence) -
                              0.5f;

        // ===== 河流地形影响 =====
        float riverNoise = SimplexNoise.OctaveNoise(
            x + _riversOffset.X, z + _riversOffset.Y, 0.001f, 4, 2f, 0.5f);
        float riverStrength = MathUtils.Saturate(
            1.5f - MathUtils.Lerp(80f, 35f,
                MathUtils.Saturate(
                    1f * mountainsBlend + 0.5f * hillsBlend +
                    MathUtils.Saturate(1f - shoreDistance / 30f)
                )) * MathF.Abs(2f * riverNoise - 1f));

        // ===== 高度叠加各项地形影响 =====
        float baseHeight = -50f * islandDrop + _heightBias;
        float biomeHeightOffset =
            MathUtils.Lerp(0f, 8f, biomeNoise1) + MathUtils.Lerp(0f, -6f, biomeNoise2);
        float hillsHeight = _hillsStrength * hillsBlend * hillsShape;
        float mountainHeight =
            mountainHeightScale * _mountainsStrength * mountainsBlend * mountainShape;
        float totalHeight = baseHeight + biomeHeightOffset + mountainHeight + hillsHeight;

        // ===== 河流影响最终夹制高度（局部降低）=====
        float riverMinHeight = MathUtils.Lerp(totalHeight,
            MathUtils.Lerp(-2f, -4f, MathUtils.Saturate(mountainsBlend + 0.5f * hillsBlend)),
            _riversStrength * riverStrength);
        float finalHeight = MathUtils.Min(riverMinHeight, totalHeight);

        // ===== 返回结果，夹制在世界允许高度范围内 =====
        return Math.Clamp(
            chunkSetting.BaseHeight + finalHeight,
            chunkSetting.ChunkHeightUnit,
            chunkSetting.SurfaceMaxHeight);
    }


    // 生成地表数据，包括温度和湿度
    private void GenerateSurfaceParameters(TerrainChunk chunk)
    {
        int chunkSize = chunk.ChunkSetting.ChunkSize;
        for (int x = 0; x < chunkSize; x++)
        {
            for (int z = 0; z < chunkSize; z++)
            {
                int worldX = x + chunk.Origin.X;
                int worldZ = z + chunk.Origin.Y;
                int temperature = CalculateTemperature(worldX, worldZ);
                int humidity = CalculateHumidity(worldX, worldZ);
                chunk.SetTemperatureFast(x, z, temperature);
                chunk.SetHumidityFast(x, z, humidity);
            }
        }
    }

    // 生成基础地形
    private void GenerateTerrain(TerrainChunk chunk)
    {
        // === 初始化参数 ===
        int chunkWidth = chunk.ChunkSetting.ChunkSize;
        int chunkHeight = chunk.ChunkSetting.ChunkHeight;
        int worldStartX = chunk.Origin.X;
        int worldStartZ = chunk.Origin.Y;

        // === 创建并填充海岸距离与山脉因子二维网格 ===
        Grid2D oceanShoreDistanceGrid = new Grid2D(chunkWidth, chunkWidth);
        Grid2D mountainRangeFactorGrid = new Grid2D(chunkWidth, chunkWidth);

        for (int zOffset = 0; zOffset < chunkWidth; zOffset++)
        {
            for (int xOffset = 0; xOffset < chunkWidth; xOffset++)
            {
                int worldX = xOffset + worldStartX;
                int worldZ = zOffset + worldStartZ;

                oceanShoreDistanceGrid.Set(xOffset, zOffset,
                    CalculateOceanShoreDistance(worldX, worldZ));
                mountainRangeFactorGrid.Set(xOffset, zOffset,
                    CalculateMountainRangeFactor(worldX, worldZ));
            }
        }

        // === 三维密度网格，用于生成体素数据 ===
        // 注意：每个体素块需要 8 个角点进行插值计算，因此密度网格维度需比块数多 1（即 +1）
        Grid3D terrainDensityGrid =
            new Grid3D(chunkWidth / 4 + 1, chunkHeight / 4 + 1, chunkWidth / 4 + 1);

        for (int blockX = 0; blockX < terrainDensityGrid.SizeX; blockX++)
        {
            for (int blockZ = 0; blockZ < terrainDensityGrid.SizeZ; blockZ++)
            {
                int worldX = blockX * 4 + worldStartX;
                int worldZ = blockZ * 4 + worldStartZ;

                float baseHeight = CalculateHeight(chunk.ChunkSetting, worldX, worldZ);
                float mountainFactor = CalculateMountainRangeFactor(worldX, worldZ);

                // 山地越多，扰动越强
                float turbulenceStrength =
                    MathUtils.Lerp(_minTurbulence, 1f, Squish(mountainFactor, _turbulenceZero, 1f));

                for (int densityLayer = 0; densityLayer < terrainDensityGrid.SizeY; densityLayer++)
                {
                    int layerHeight = densityLayer * 4;

                    // 利用 SimplexNoise 增加地形扰动
                    float turbulenceEffect = _turbulenceStrength * turbulenceStrength *
                                             MathUtils.Saturate(baseHeight - layerHeight) *
                                             (2f * SimplexNoise.OctaveNoise(worldX, layerHeight,
                                                  worldZ,
                                                  _turbulenceFreq, _turbulenceOctaves, 4f,
                                                  _turbulencePersistence) -
                                              1f);

                    float disturbedHeight = layerHeight + turbulenceEffect;
                    float densityValue = baseHeight - disturbedHeight;

                    // 增加偏移以确保底部更密实
                    densityValue += MathUtils.Max(4f * (_densityBias - layerHeight), 0f);

                    terrainDensityGrid.Set(blockX, densityLayer, blockZ, densityValue);
                }
            }
        }

        // === 获取海平面高度 ===
        int oceanLevel = chunk.ChunkSetting.BaseHeight;

        // === 进行体素插值并设置方块类型 ===
        for (int xBlock = 0; xBlock < terrainDensityGrid.SizeX - 1; xBlock++)
        {
            for (int zBlock = 0; zBlock < terrainDensityGrid.SizeZ - 1; zBlock++)
            {
                for (int yBlock = 0; yBlock < terrainDensityGrid.SizeY - 1; yBlock++)
                {
                    // 获取当前体素单元的 8 个角的密度值
                    terrainDensityGrid.Get8(xBlock, yBlock, zBlock,
                        out var d00, out var d10, out var d01, out var d11,
                        out var d00T, out var d10T, out var d01T, out var d11T);

                    float deltaXFront = (d10 - d00) / 4f;
                    float deltaXBack = (d11 - d01) / 4f;
                    float deltaZFront = (d00T - d00) / 4f;
                    float deltaZBack = (d01T - d01) / 4f;

                    float cur00 = d00;
                    float cur01 = d01;
                    float cur00T = d00T;
                    float cur01T = d01T;

                    for (int subX = 0; subX < 4; subX++)
                    {
                        float deltaYFront = (cur00T - cur00) / 4f;
                        float deltaYBack = (cur01T - cur01) / 4f;

                        float curFront = cur00;
                        float curBack = cur01;

                        for (int subZ = 0; subZ < 4; subZ++)
                        {
                            float deltaDensity = (curBack - curFront) / 4f;
                            float density = curFront;

                            int localX = subX + xBlock * 4;
                            int localZ = subZ + zBlock * 4;


                            float shoreDistance = oceanShoreDistanceGrid.Get(localX, localZ);
                            float mountainValue = mountainRangeFactorGrid.Get(localX, localZ);
                            int temperature = chunk.GetTemperatureFast(localX, localZ);
                            int humidity = chunk.GetHumidityFast(localX, localZ);

                            // 计算石头、泥土的密度阈值，随山地程度而调整
                            float adjustedMountainFactor = mountainValue - 0.01f * humidity;
                            float stoneThreshold = MathUtils.Lerp(100f, 0f, adjustedMountainFactor);
                            float dirtThreshold = MathUtils.Lerp(300f, 30f, adjustedMountainFactor);

                            // 判断是否为沙漠/干旱地带
                            bool isDesertLike = mountainValue < 0.97f &&
                                                (temperature > 8 && humidity < 8 ||
                                                 MathF.Abs(shoreDistance) < 16);

                            int baseIndex = chunk.GetCellIndex(localX, 0, localZ);

                            for (int layer = 0; layer < 4; layer++)
                            {
                                int heightLevel = layer + yBlock * 4;
                                BlockType blockType;

                                if (density < 0f)
                                {
                                    // 低密度 -> 空气或水（在海平面以下）
                                    blockType = heightLevel <= oceanLevel
                                        ? BlockType.WaterBlock
                                        : BlockType.AirBlock;
                                }
                                else
                                {
                                    if (isDesertLike)
                                    {
                                        // 沙漠地形
                                        blockType = density < stoneThreshold
                                            ? BlockType.SandStoneBlock
                                            : (density < dirtThreshold
                                                ? BlockType.GraniteStoneBlock
                                                : BlockType.BasaltStoneBlock);
                                    }
                                    else
                                    {
                                        // 普通地形
                                        blockType = density < dirtThreshold
                                            ? BlockType.GraniteStoneBlock
                                            : BlockType.BasaltStoneBlock;
                                    }
                                }

                                chunk.SetCellValueFast(baseIndex + heightLevel, (int)blockType);
                                density += deltaDensity;
                            }

                            curFront += deltaYFront;
                            curBack += deltaYBack;
                        }

                        cur00 += deltaXFront;
                        cur01 += deltaXBack;
                        cur00T += deltaZFront;
                        cur01T += deltaZBack;
                    }
                }
            }
        }
    }


    private void PropagateFluidsDownwards(TerrainChunk chunk)
    {
        int chunkSize = chunk.ChunkSetting.ChunkSize;
        int chunkHeight = chunk.ChunkSetting.ChunkHeight;

        for (int x = 0; x < chunkSize; x++)
        {
            for (int z = 0; z < chunkSize; z++)
            {
                int cellIndex = chunk.GetCellIndex(x, chunkHeight - 1, z);
                int lastNonEmptyContent = 0;

                for (int y = chunkHeight - 1; y >= 0; y--, cellIndex--)
                {
                    int currentContent = Terrain.ExtractContents(chunk.GetCellValueFast(cellIndex));

                    if (currentContent == 0 && lastNonEmptyContent != 0 && BlocksManager.IsFluidBlock(lastNonEmptyContent))
                    {
                        chunk.SetCellValueFast(cellIndex, lastNonEmptyContent);
                        currentContent = lastNonEmptyContent;
                    }

                    lastNonEmptyContent = currentContent;
                }
            }
        }
    }

    // 生成表层方块，如草、雪、泥土等
    private void GenerateSurface(TerrainChunk chunk)
    {
        int chunkSize = chunk.ChunkSetting.ChunkSize;
        int surfaceMaxHeight = chunk.ChunkSetting.SurfaceMaxHeight;
        int surfaceMinHeight = chunk.ChunkSetting.SurfaceMinHeight;
        int chunkHeightUnit = chunk.ChunkSetting.ChunkHeightUnit;
        int baseHeight = chunk.ChunkSetting.BaseHeight;

        Random random = new Random(_seed + chunk.Coords.X + 101 * chunk.Coords.Y);
        for (int i = 0; i < chunkSize; i++)
        {
            for (int j = 0; j < chunkSize; j++)
            {
                int worldX = i + chunk.Origin.X;
                int worldZ = j + chunk.Origin.Y;
                int y = surfaceMaxHeight;
                int cellIndex = chunk.GetCellIndex(i, y, j);

                while (y >= surfaceMinHeight)
                {
                    BlockType blockType =
                        (BlockType)Terrain.ExtractContents(chunk.GetCellValueFast(cellIndex));
                    if (!BlocksManager.IsTransparent((int)blockType))
                    {
                        // 找到第一个非透明方块
                        float mountainFactor = CalculateMountainRangeFactor(worldX, worldZ);
                        int temperature = _terrain.GetTemperature(worldX, worldZ);
                        int humidity = _terrain.GetHumidity(worldX, worldZ);

                        BlockType surfaceBlock;

                        if (blockType == BlockType.SandStoneBlock)
                        {
                            surfaceBlock = temperature is > 4 and < 7
                                ? BlockType.GravelStoneBlock
                                : BlockType.SandBlock;
                        }
                        else
                        {
                            int tLevel = temperature / 4;
                            BlockType aboveBlock = (y + 1 < surfaceMaxHeight)
                                ? (BlockType)chunk.GetCellContentsFast(i, y + 1, j)
                                : BlockType.AirBlock;

                            // 冰雪覆盖
                            if (y > surfaceMaxHeight - chunkHeightUnit &&
                                SubsystemWeather.IsPlaceFrozen(temperature, y))
                            {
                                surfaceBlock = BlockType.IceBlock;
                            }
                            // 石灰石条件
                            else if ((y < baseHeight + 2 ||
                                      y == baseHeight + chunkHeightUnit + 4 + tLevel ||
                                      y == baseHeight + (int)(chunkHeightUnit * 2.5f - 1) + tLevel)
                                     && humidity == 9 && temperature % 6 == 1)
                            {
                                surfaceBlock = BlockType.LimeStoneBlock;
                            }
                            // 粘土
                            else if (aboveBlock == BlockType.WaterBlock &&
                                     humidity > 8 && humidity % 2 == 0 &&
                                     temperature % 3 == 0)
                            {
                                surfaceBlock = BlockType.ClayBlock;
                            }
                            else
                            {
                                surfaceBlock = BlockType.DirtBlock;
                            }
                        }

                        // 计算表层厚度
                        int thickness;
                        if (surfaceBlock == BlockType.IceBlock)
                        {
                            thickness = (int)Math.Clamp(-temperature, 1f, 7f);
                        }
                        else
                        {
                            float heightFactor = MathUtils.Saturate((y - 100f) * 0.05f);
                            float f = MathUtils.Saturate(
                                MathUtils.Saturate((mountainFactor - 0.9f) / 0.1f) -
                                MathUtils.Saturate((humidity - 3f) / 12f) +
                                _surfaceMultiplier * heightFactor
                            );
                            int min = (int)MathUtils.Lerp(4f, 0f, f);
                            int max = (int)MathUtils.Lerp(7f, 0f, f);
                            thickness = MathUtils.Min(random.Int(min, max), y);
                        }

                        // 设置表层方块
                        int topIndex = chunk.GetCellIndex(i, y + 1, j);

                        for (int k = topIndex - thickness; k < topIndex; k++)
                        {
                            if (Terrain.ExtractContents(chunk.GetCellValueFast(k)) !=
                                (int)BlockType.AirBlock)
                            {
                                chunk.SetCellValueFast(k,
                                    Terrain.ReplaceContents((int)surfaceBlock));
                            }
                        }

                        break;
                    }

                    y--;
                    cellIndex--;
                }
            }
        }
    }


    // 生成矿物资源
    private void GenerateMinerals(TerrainChunk chunk)
    {
        int centerX = chunk.Coords.X;
        int centerZ = chunk.Coords.Y;

        int chunkSize = chunk.ChunkSetting.ChunkSize;
        int baseHeight = chunk.ChunkSetting.BaseHeight;
        int chunkHeightUnit = chunk.ChunkSetting.ChunkHeightUnit;
        int surfaceMaxHeight = chunk.ChunkSetting.SurfaceMaxHeight;

        // 遍历中心区块及其周围 3x3 范围内的所有区块
        for (int chunkX = centerX - 1; chunkX <= centerX + 1; chunkX++)
        {
            for (int chunkZ = centerZ - 1; chunkZ <= centerZ + 1; chunkZ++)
            {
                // 为每个区块生成独立的随机数发生器
                Random random = new Random(_seed + chunkX + 119 * chunkZ);

                // 控制噪声强度的山脉因子，用于模拟矿物分布
                float mountainFactor =
                    CalculateMountainRangeFactor(chunkX * chunkSize, chunkZ * chunkSize);

                // === 煤矿生成 ===
                int coalCount =
                    (int)(5f + 3f * mountainFactor *
                        SimplexNoise.OctaveNoise(chunkX, chunkZ, 0.33f, 1, 1f, 1f));
                GenerateMineral(chunk, chunkX, chunkZ, random, coalCount, 5, surfaceMaxHeight,
                    BlockType.GraniteStoneBlock,
                    CoalBrushes);

                // === 铜矿生成 ===
                int copperCount = (int)(6f + 2f * mountainFactor *
                    SimplexNoise.OctaveNoise(chunkX + 1211, chunkZ + 396, 0.33f, 1, 1f, 1f));
                GenerateMineral(chunk, chunkX, chunkZ, random, copperCount, baseHeight / 2 - 10,
                    baseHeight,
                    BlockType.GraniteStoneBlock, CopperBrushes);

                // === 铁矿生成 ===
                int ironCount = (int)(5f + 2f * mountainFactor *
                    SimplexNoise.OctaveNoise(chunkX + 713, chunkZ + 211, 0.33f, 1, 1f, 1f));
                GenerateMineral(chunk, chunkX, chunkZ, random, ironCount, 2,
                    baseHeight / 2 + chunkHeightUnit / 2,
                    BlockType.BasaltStoneBlock, IronBrushes);

                // === 硝石生成 ===
                int saltpeterCount = (int)(3f + 3f * mountainFactor *
                    SimplexNoise.OctaveNoise(chunkX + 915, chunkZ + 272, 0.33f, 1, 1f, 1f));
                GenerateMineral(chunk, chunkX, chunkZ, random, saltpeterCount,
                    baseHeight - chunkHeightUnit,
                    baseHeight + chunkHeightUnit + 10, BlockType.SandStoneBlock, SaltpeterBrushes);

                // === 硫磺生成 ===
                int sulphurCount = (int)(3f + 2f * mountainFactor *
                    SimplexNoise.OctaveNoise(chunkX + 711, chunkZ + 1194, 0.33f, 1, 1f, 1f));
                GenerateMineral(chunk, chunkX, chunkZ, random, sulphurCount, 2,
                    baseHeight / 2 + chunkHeightUnit / 2,
                    BlockType.BasaltStoneBlock, SulphurBrushes);

                // === 钻石生成 ===
                int diamondCount = (int)(0.5f + 2f * mountainFactor *
                    SimplexNoise.OctaveNoise(chunkX + 432, chunkZ + 907, 0.33f, 1, 1f, 1f));
                GenerateMineral(chunk, chunkX, chunkZ, random, diamondCount, 2, chunkHeightUnit,
                    BlockType.BasaltStoneBlock, DiamondBrushes);

                // === 锗矿生成 ===
                int germaniumCount = (int)(3f + 2f * mountainFactor *
                    SimplexNoise.OctaveNoise(chunkX + 799, chunkZ + 131, 0.33f, 1, 1f, 1f));
                GenerateMineral(chunk, chunkX, chunkZ, random, germaniumCount, 2,
                    baseHeight - chunkHeightUnit,
                    BlockType.BasaltStoneBlock, GermaniumBrushes);
            }
        }
    }

    /// <summary>
    /// 通用矿物生成方法
    /// </summary>
    private void GenerateMineral(
        TerrainChunk chunk,
        int chunkX,
        int chunkZ,
        Random random,
        int count,
        int minY,
        int maxY,
        BlockType onlyInBlockType,
        List<TerrainBrush> brushes)
    {
        int chunkSize = chunk.ChunkSetting.ChunkSize;
        for (int i = 0; i < count; i++)
        {
            int x = chunkX * chunkSize + random.Int(0, chunkSize - 1);
            int y = random.Int(minY, maxY);
            int z = chunkZ * chunkSize + random.Int(0, chunkSize - 1);
            TerrainBrush brush = brushes[random.Int(0, brushes.Count - 1)];
            brush.PaintFastSelective(chunk, x, y, z, (int)onlyInBlockType);
        }
    }


    private void GeneratePockets(TerrainChunk chunk)
    {
        // 使用统一的随机数实例，以避免每次都创建新的随机数实例
        Random random = new Random(_seed + chunk.Coords.X + chunk.Coords.Y * 71);

        int chunkSize = chunk.ChunkSetting.ChunkSize;
        int baseHeight = chunk.ChunkSetting.BaseHeight;
        int chunkHeightUnit = chunk.ChunkSetting.ChunkHeightUnit;
        int surfaceMaxHeight = chunk.ChunkSetting.SurfaceMaxHeight;

        // 遍历周围的区域
        for (int offsetX = -1; offsetX <= 1; offsetX++)
        {
            for (int offsetY = -1; offsetY <= 1; offsetY++)
            {
                // 根据坐标偏移计算当前区块的位置
                int chunkX = offsetX + chunk.Coords.X;
                int chunkY = offsetY + chunk.Coords.Y;

                // 计算该区域的山脉影响因子
                float mountainRangeFactor =
                    CalculateMountainRangeFactor(chunkX * chunkSize, chunkY * chunkSize);

                // 生成不同类型的石头
                GeneratePocket(DirtPocketBrushes, chunk, chunkX, chunkY, 5,
                    baseHeight - chunkHeightUnit,
                    surfaceMaxHeight, BlockType.GraniteStoneBlock, random);
                GeneratePocket(SandPocketBrushes, chunk, chunkX, chunkY, 20, chunkHeightUnit,
                    surfaceMaxHeight,
                    BlockType.SandStoneBlock, random);
                GeneratePocket(BasaltPocketBrushes, chunk, chunkX, chunkY, 4, baseHeight / 2,
                    baseHeight,
                    BlockType.SandStoneBlock, random); // 玄武岩
                GeneratePocket(LimestonePocketBrushes, chunk, chunkX, chunkY, 5,
                    chunkHeightUnit / 2,
                    surfaceMaxHeight, BlockType.GraniteStoneBlock,
                    random); // 石灰石
                GeneratePocket(GravelPocketBrushes, chunk, chunkX, chunkY, 20, 4, surfaceMaxHeight,
                    BlockType.BasaltStoneBlock, random); // 碎石
                GeneratePocket(GranitePocketBrushes, chunk, chunkX, chunkY, 6, 4,
                    baseHeight - chunkHeightUnit,
                    BlockType.BasaltStoneBlock, random); // 花岗岩

                // 生成水
                if (random.Bool(0.02f + 0.01f * mountainRangeFactor))
                {
                    GenerateWaterPocket(WaterPocketBrushes, chunk, chunkX, chunkY, random);
                }

                // 生成熔岩
                if (random.Bool(0.04f + 0.02f * mountainRangeFactor))
                {
                    GenerateMagmaPocket(MagmaPocketBrushes, chunk, chunkX, chunkY, random);
                }
            }
        }
    }

    // 提取通用的矿物生成方法
    private void GeneratePocket(List<TerrainBrush> brushes, TerrainChunk chunk, int chunkX,
        int chunkY, int pocketCount,
        int minY, int maxY, BlockType onlyInBlockType, Random random)
    {
        int chunkSize = chunk.ChunkSetting.ChunkSize;
        for (int i = 0; i < pocketCount; i++)
        {
            // 随机选择生成位置
            int x = chunkX * chunkSize + random.Int(0, chunkSize - 1);
            int y = random.Int(minY, maxY);
            int z = chunkY * chunkSize + random.Int(0, chunkSize - 1);

            // 从矿物刷子列表中随机选择一个刷子进行快速绘制
            brushes[random.Int(0, brushes.Count - 1)]
                .PaintFastSelective(chunk, x, y, z, (int)onlyInBlockType);
        }
    }

    // 生成水口袋的方法
    private void GenerateWaterPocket(List<TerrainBrush> brushes, TerrainChunk chunk, int chunkX,
        int chunkY,
        Random random)
    {
        int chunkSize = chunk.ChunkSetting.ChunkSize;
        int baseHeight = chunk.ChunkSetting.BaseHeight;

        // 水口袋的随机生成位置
        int baseX = chunkX * chunkSize;
        int baseY = random.Int((int)(baseHeight * (1.0 / 2)),
            baseHeight-4); // 注意浮点运算
        int baseZ = chunkY * chunkSize;

        // 随机生成多个水口袋
        int pocketCount = random.Int(1, 3);
        for (int i = 0; i < pocketCount; i++)
        {
            // 使用随机向量来扰动位置
            Vector2 offset = random.Vector2((chunkSize / 2) - 1);
            int offsetX = chunkSize / 2 + (int)MathF.Round(offset.X);
            int offsetZ = chunkSize / 2 + (int)MathF.Round(offset.Y);

            // 随机选择水口袋刷子进行绘制
            brushes[random.Int(0, brushes.Count - 1)]
                .PaintFast(chunk, baseX + offsetX, baseY, baseZ + offsetZ);
        }
    }

    // 生成熔岩口袋的方法
    private void GenerateMagmaPocket(List<TerrainBrush> brushes, TerrainChunk chunk, int chunkX,
        int chunkY,
        Random random)
    {
        int chunkSize = chunk.ChunkSetting.ChunkSize;
        int baseHeight = chunk.ChunkSetting.BaseHeight;

        int baseX = chunkX * chunkSize;
        int baseY =
            random.Int((int)(baseHeight * (1.0 / 5)), (int)(baseHeight * (2.0 / 5))); // 注意浮点运算
        int baseZ = chunkY * chunkSize;

        // 随机生成多个熔岩口袋
        int pocketCount = random.Int(1, 2);
        for (int i = 0; i < pocketCount; i++)
        {
            // 使用随机向量来扰动位置
            Vector2 offset = random.Vector2(7f);
            int offsetX = chunkSize / 2 + (int)MathF.Round(offset.X);
            int offsetY = random.Int(0, 1);
            int offsetZ = chunkSize / 2 + (int)MathF.Round(offset.Y);

            // 随机选择熔岩口袋刷子进行绘制
            brushes[random.Int(0, brushes.Count - 1)]
                .PaintFast(chunk, baseX + offsetX, baseY + offsetY, baseZ + offsetZ);
        }
    }

    // 生成洞穴
    private void GenerateCaves(TerrainChunk chunk)
    {
        int chunkSize = chunk.ChunkSetting.ChunkSize;
        int chunkX = chunk.Coords.X;
        int chunkY = chunk.Coords.Y;

        // 遍历当前区块及其周围区块（共 5x5）
        for (int i = chunkX - 2; i <= chunkX + 2; i++)
        {
            for (int j = chunkY - 2; j <= chunkY + 2; j++)
            {
                List<CavePoint> cavePoints = new List<CavePoint>();
                Random random = new Random(_seed + i + 9973 * j);

                // 随机决定是否生成一个洞穴
                if (!random.Bool(0.5f)) continue;

                // 随机位置（i, j 区块内）
                int worldX = i * chunkSize + random.Int(0, chunkSize - 1);
                int worldZ = j * chunkSize + random.Int(0, chunkSize - 1);
                int heightCenter = (int)CalculateHeight(chunk.ChunkSetting, worldX, worldZ);
                int heightXOffset = (int)CalculateHeight(chunk.ChunkSetting, worldX + 3, worldZ);
                int heightZOffset = (int)CalculateHeight(chunk.ChunkSetting, worldX, worldZ + 3);

                // 洞穴起点及方向
                Vector3 startPos = new(worldX, heightCenter - 1, worldZ);
                Vector3 dirX = new(3f, heightXOffset - heightCenter, 0f);
                Vector3 dirZ = new(0f, heightZOffset - heightCenter, 3f);
                Vector3 direction = Vector3.Normalize(Vector3.Cross(dirX, dirZ));

                // 判断坡度（如果太陡则不生成）
                if (direction.Y > -0.6f)
                {
                    cavePoints.Add(new CavePoint
                    {
                        Position = startPos,
                        Direction = direction,
                        BrushType = 0,
                        Length = random.Int(80, 240)
                    });
                }

                var centerX = i * chunkSize + chunkSize / 2;
                var centerZ = j * chunkSize + chunkSize / 2;

                // 开始生成路径
                for (int c = 0; c < cavePoints.Count;)
                {
                    CavePoint cave = cavePoints[c];

                    // 使用刷子画洞
                    List<TerrainBrush> brushes = CaveBrushesByType[cave.BrushType];
                    TerrainBrush brush = brushes[random.Int(0, brushes.Count - 1)];
                    brush.PaintFastAvoidWater(chunk,
                        Terrain.ToCell(cave.Position.X),
                        Terrain.ToCell(cave.Position.Y),
                        Terrain.ToCell(cave.Position.Z));

                    // 位置推进
                    cave.Position += 2f * cave.Direction;
                    cave.StepsTaken += 2;

                    float dx = cave.Position.X - centerX;
                    float dz = cave.Position.Z - centerZ;

                    // 随机扰动方向
                    if (random.Bool(0.5f))
                    {
                        Vector3 randomDir = Vector3.Normalize(random.Vector3(1f));

                        float maxOffset = 25.5f; // 设置允许的最大偏移量

                        // 防止走出边界（X/Z）
                        if ((dx < -maxOffset && randomDir.X < 0f) ||
                            (dx > maxOffset && randomDir.X > 0f))
                            randomDir.X *= -1;
                        if ((dz < -maxOffset && randomDir.Z < 0f) ||
                            (dz > maxOffset && randomDir.Z > 0f))
                            randomDir.Z *= -1;

                        // 控制上下坡变化
                        if ((cave.Direction.Y < -0.5f && randomDir.Y < -10f) ||
                            (cave.Direction.Y > 0.1f && randomDir.Y > 0f)) randomDir.Y *= -1;

                        cave.Direction = Vector3.Normalize(cave.Direction + 0.5f * randomDir);
                    }

                    // 方向/类型变化（模拟分支或层级变化）
                    if (cave.StepsTaken > 20)
                    {
                        if (random.Bool(0.06f))
                        {
                            cave.Direction =
                                Vector3.Normalize(random.Vector3(1f) * new Vector3(1f, 0.33f, 1f));
                        }

                        if (random.Bool(0.05f))
                        {
                            cave.Direction.Y = 0f;
                            cave.BrushType = MathUtils.Min(cave.BrushType + 2,
                                CaveBrushesByType.Count - 1);
                        }
                    }

                    // 垂直向下洞穴
                    if (cave.StepsTaken > 30 && random.Bool(0.03f))
                    {
                        cave.Direction = new Vector3(0f, -1f, 0f);
                    }

                    // 向上洞穴（少见）
                    if (cave.StepsTaken > 30 && cave.Position.Y < 30f && random.Bool(0.02f))
                    {
                        cave.Direction = new Vector3(0f, 1f, 0f);
                    }

                    // 随机改变 Brush 类型（7次方分布）
                    if (random.Bool(0.33f))
                    {
                        cave.BrushType = (int)(MathF.Pow(random.Float(0f, 0.999f), 7f) *
                                               CaveBrushesByType.Count);
                    }

                    // 生成新分支洞穴（最多 12 条）
                    if (random.Bool(0.06f) && cavePoints.Count < 12 && cave.StepsTaken > 20 &&
                        cave.Position.Y < 58f)
                    {
                        cavePoints.Add(new CavePoint
                        {
                            Position = cave.Position,
                            Direction =
                                Vector3.Normalize(random.Vector3(1f, 1f) *
                                                  new Vector3(1f, 0.33f, 1f)),
                            BrushType = (int)(MathF.Pow(random.Float(0f, 0.999f), 7f) *
                                              CaveBrushesByType.Count),
                            Length = random.Int(40, 180)
                        });
                    }

                    // 停止条件（长度、边界、高度限制）
                    if (cave.StepsTaken >= cave.Length ||
                        MathF.Abs(dx) > 34f || MathF.Abs(dz) > 34f ||
                        cave.Position.Y < 5f || cave.Position.Y > 246f)
                    {
                        c++;
                    }
                    else if (cave.StepsTaken % 20 == 0)
                    {
                        // 每 20 步检测是否过高
                        float surface = CalculateHeight(chunk.ChunkSetting, cave.Position.X,
                            cave.Position.Z);
                        if (cave.Position.Y > surface + 1f) c++;
                    }
                }
            }
        }
    }

    // 生成横向倒地的树干
    private void GenerateLogs(TerrainChunk chunk)
    {
        int chunkSize = chunk.ChunkSetting.ChunkSize;
        int originX = chunk.Origin.X;
        int originY = chunk.Origin.Y;
        int endX = originX + chunkSize;
        int endY = originY + chunkSize;

        int chunkX = chunk.Coords.X;
        int chunkY = chunk.Coords.Y;

        // 仅对当前区块执行一次
        Random random = new Random(_seed + chunkX + 3943 * chunkY);
        int humidity = CalculateHumidity(chunkX * chunkSize, chunkY * chunkSize);
        int temperature = CalculateTemperature(chunkX * chunkSize, chunkY * chunkSize);

        // 使用噪声计算生成概率
        float logChance = MathUtils.Saturate(
            (SimplexNoise.OctaveNoise(chunkX, chunkY, 0.1f, 2, 2f, 0.5f) - 0.25f) / 0.2f +
            (random.Bool(0.25f) ? 0.5f : 0f)
        );

        int logCount = 0;
        int maxLogs = logChance > 0.7f ? random.Int(0, 1) : 0;

        for (int attempt = 0; attempt < 16 && logCount < maxLogs; attempt++)
        {
            int baseX = chunkX * chunkSize + random.Int(0, chunkSize - 1);
            int baseZ = chunkY * chunkSize + random.Int(0, chunkSize - 1);
            int topY = _terrain.CalculateTopmostCellHeight(baseX, baseZ);

            // 地形高度太低则不生成
            if (topY < chunk.ChunkSetting.BaseHeight)
                continue;

            int groundBlock = _terrain.GetCellContentsFast(baseX, topY, baseZ);
            if (groundBlock != (int)BlockType.DirtBlock && groundBlock != (int)BlockType.GrassBlock)
                continue;

            topY++; // 树干起始高度
            int logLength = random.Int(4, 7); // 树干长度

            Point3 direction = CellFace.FaceToPoint3[random.Int(0, 3)]; // 倒地方向

            // 边界检查，避免树干超出 chunk
            if ((direction.X < 0 && baseX - logLength + 1 < 0) ||
                (direction.X > 0 && baseX + logLength - 1 > chunkSize - 1))
                direction.X *= -1;
            if ((direction.Z < 0 && baseZ - logLength + 1 < 0) ||
                (direction.Z > 0 && baseZ + logLength - 1 > chunkSize - 1))
                direction.Z *= -1;

            bool isPathClear = true;
            bool hasSupportStart = false;
            bool hasSupportEnd = false;

            for (int l = 0; l < logLength; l++)
            {
                int x = baseX + direction.X * l;
                int z = baseZ + direction.Z * l;

                // 边界检查
                if (x < originX + 1 || x >= endX - 1 || z < originY + 1 || z >= endY - 1)
                {
                    isPathClear = false;
                    break;
                }

                if (BlocksManager.IsCollidable(_terrain.GetCellContentsFast(x, topY, z)))
                {
                    isPathClear = false;
                    break;
                }

                if (BlocksManager.IsCollidable(_terrain.GetCellContentsFast(x, topY - 1, z)))
                {
                    if (l <= MathUtils.Max(logLength / 2, 0)) hasSupportStart = true;
                    if (l >= MathUtils.Min(logLength / 2 + 1, logLength - 1)) hasSupportEnd = true;
                }
            }

            if (!(isPathClear && hasSupportStart && hasSupportEnd))
                continue;

            Point3 perpendicular = (direction.X != 0) ? new Point3(0, 0, 1) : new Point3(1, 0, 0);

            // 获取树类型
            TreeType? treeType = PlantsManager.GenerateRandomTreeType(
                random,
                temperature + SubsystemWeather.GetTemperatureAdjustmentAtHeight(topY),
                humidity,
                topY,
                2f
            );

            if (!treeType.HasValue)
                continue;

            int trunkValue = PlantsManager.GetTreeTrunkValue(treeType.Value);
            int cutFace = (direction.X != 0) ? 1 : 0;
            trunkValue =
                Terrain.ReplaceData(trunkValue,
                    WoodBlock.SetCutFace(Terrain.ExtractData(trunkValue), cutFace));

            int leavesValue = PlantsManager.GetTreeLeavesValue(treeType.Value);

            // 放置树干与少量附属结构
            for (int m = 0; m < logLength; m++)
            {
                int x = baseX + direction.X * m;
                int z = baseZ + direction.Z * m;

                _terrain.SetCellValueFast(x, topY, z, trunkValue);

                if (m > logLength / 2)
                {
                    // 放置叶子或树枝（周围）
                    TryPlace(random, x + perpendicular.X, topY, z + perpendicular.Z, leavesValue,
                        trunkValue);
                    TryPlace(random, x - perpendicular.X, topY, z - perpendicular.Z, leavesValue,
                        trunkValue);
                    TryPlace(random, x, topY + 1, z, leavesValue, trunkValue);
                }
            }

            logCount++;
        }
    }

    /// <summary>
    /// 随机尝试放置树叶或额外的树干
    /// </summary>
    private void TryPlace(Random random, int x, int y, int z, int leavesValue, int trunkValue)
    {
        if (!BlocksManager.IsCollidable(_terrain.GetCellContentsFast(x, y, z)))
        {
            if (random.Bool(0.5f))
                _terrain.SetCellValueFast(x, y, z, leavesValue);
            else if (random.Bool(0.05f))
                _terrain.SetCellValueFast(x, y, z, trunkValue);
        }
    }


    private void GenerateTrees(TerrainChunk chunk)
    {
        int worldX = chunk.Origin.X;
        int worldZ = chunk.Origin.Y;
        ChunkSetting chunkSetting = chunk.ChunkSetting;

        Random random = new Random(_seed + worldX + 3943 * worldZ);

        int humidity = CalculateHumidity(worldX, worldZ);
        int temperature = CalculateTemperature(worldX, worldZ);
        float forestDensity = CalculateForestDensity(worldX, worldZ);

        int targetTreeCount = random.Int(12, (int)(12 * forestDensity));
        int placedTrees = 0;

        List<(int x, int z)> treePositions = new(); // 用于记录已放置的树位置
        const int minDistance = 4; // 最小树间距

        for (int attempt = 0; attempt < 36 && placedTrees < targetTreeCount; attempt++)
        {
            int localX = worldX + random.Int(chunkSetting.ChunkSizeUnit,
                chunkSetting.ChunkSize - chunkSetting.ChunkSizeUnit);
            int localZ = worldZ + random.Int(chunkSetting.ChunkSizeUnit,
                chunkSetting.ChunkSize - chunkSetting.ChunkSizeUnit);

            int topY = _terrain.CalculateTopmostCellHeight(localX, localZ);
            if (topY < chunkSetting.BaseHeight)
                continue;

            int groundBlock = _terrain.GetCellContentsFast(localX, topY, localZ);
            if (groundBlock != (int)BlockType.GrassBlock && groundBlock != (int)BlockType.DirtBlock)
                continue;

            int placeY = topY + 1;

            if (BlocksManager.IsCollidable(
                    _terrain.GetCellContentsFast(localX + 1, placeY, localZ)) ||
                BlocksManager.IsCollidable(
                    _terrain.GetCellContentsFast(localX - 1, placeY, localZ)) ||
                BlocksManager.IsCollidable(
                    _terrain.GetCellContentsFast(localX, placeY, localZ + 1)) ||
                BlocksManager.IsCollidable(
                    _terrain.GetCellContentsFast(localX, placeY, localZ - 1)))
                continue;

            // 检查是否离其他树太近
            bool tooClose = treePositions.Any(pos =>
            {
                int dx = pos.x - localX;
                int dz = pos.z - localZ;
                return dx * dx + dz * dz < minDistance * minDistance;
            });
            if (tooClose)
                continue;

            TreeType? treeType = PlantsManager.GenerateRandomTreeType(
                random,
                temperature + SubsystemWeather.GetTemperatureAdjustmentAtHeight(placeY),
                humidity,
                placeY
            );

            if (treeType.HasValue)
            {
                var brushes = PlantsManager.GetTreeBrushes(treeType.Value);
                var selectedBrush = brushes[random.Int(brushes.Count)];

                selectedBrush.PaintFast(chunk, localX, placeY, localZ);
                treePositions.Add((localX, localZ)); // 记录位置
                placedTrees++;
            }
        }
    }


    // 生成草地和植株
    private void GenerateGrassAndPlants(TerrainChunk chunk)
    {
        // 基于区块坐标生成确定性随机种子
        Random random = new Random(_seed + chunk.Coords.X + 3943 * chunk.Coords.Y);

        ChunkSetting chunkSetting = chunk.ChunkSetting;

        for (int localX = 0; localX < chunkSetting.ChunkSize; localX++)
        {
            for (int localZ = 0; localZ < chunkSetting.ChunkSize; localZ++)
            {
                // 从地表向下搜索，找到第一个非空气的方块
                for (int y = chunkSetting.SurfaceMaxHeight; y >= chunkSetting.SurfaceMinHeight; y--)
                {
                    BlockType blockType = (BlockType)chunk.GetCellValueFast(localX, y, localZ);

                    if (blockType != BlockType.AirBlock) // 找到非空气方块
                    {
                        if (!BlocksManager.IsFluidBlock((int)blockType))
                        {
                            int temperature = chunk.GetTemperatureFast(localX, localZ);
                            int humidity = chunk.GetHumidityFast(localX, localZ);

                            // 根据温湿度在该位置上方生成草/植物（如果适合）
                            int plantValue = PlantsManager.GenerateRandomPlantValue(
                                random, (int)blockType, temperature, humidity, y + 1);

                            if (plantValue != (int)BlockType.AirBlock)
                            {
                                chunk.SetCellValueFast(localX, y + 1, localZ, plantValue);
                            }

                            // 如果原地是泥土，替换为草方块
                            if (blockType == BlockType.DirtBlock)
                            {
                                chunk.SetCellValueFast(localX, y, localZ,
                                    (int)BlockType.GrassBlock);
                            }
                        }

                        break; // 一旦找到非空气地面，无需继续向下
                    }
                }
            }
        }
    }

    // 生成仙人掌
    private void GenerateCacti(TerrainChunk chunk)
    {
        // 使用区块坐标 + 固定因子生成确定性随机种子
        Random random = new Random(_seed + chunk.Coords.X + 1991 * chunk.Coords.Y);

        // 以 50% 概率跳过该区块，加入自然变化
        if (!random.Bool(0.5f)) return;

        // 尝试生成仙人掌的次数（可配置）
        int cactusAttempts = random.Int(0, 1);

        // 获取区块配置
        ChunkSetting chunkSetting = chunk.ChunkSetting;

        for (int attemptIndex = 0; attemptIndex < cactusAttempts; attemptIndex++)
        {
            // 在区块中偏中心区域选择一个基准位置
            int baseX = random.Int(chunkSetting.ChunkSizeUnit - 1,
                chunkSetting.ChunkSize - chunkSetting.ChunkSizeUnit);
            int baseZ = random.Int(chunkSetting.ChunkSizeUnit - 1,
                chunkSetting.ChunkSize - chunkSetting.ChunkSizeUnit);

            int humidity = chunk.GetHumidityFast(baseX, baseZ);
            int temperature = chunk.GetTemperatureFast(baseX, baseZ);

            // 如果不满足沙漠条件（太湿或太冷），跳过
            if (humidity >= 6 || temperature <= 8)
                continue;
            // 在基准点附近最多尝试8次放置仙人掌
            for (int tryIndex = 0; tryIndex < 8; tryIndex++)
            {
                int localX = baseX + random.Int(-2, 2);
                int localZ = baseZ + random.Int(-2, 2);

                // 从上往下搜索，找沙地放置位置
                for (int y = chunkSetting.SurfaceMaxHeight; y >= chunkSetting.SurfaceMinHeight; y--)
                {
                    int blockType = chunk.GetCellValueFast(localX, y, localZ);

                    switch (blockType)
                    {
                        case (int)BlockType.SandBlock:
                        {
                            // 向上生成最多3格高的仙人掌，确保四周为空
                            for (int cactusY = y + 1;
                                 cactusY <= y + 3 &&
                                 cactusY < chunkSetting.ChunkHeight &&
                                 chunk.GetCellContentsFast(localX + 1, cactusY, localZ) == 0 &&
                                 chunk.GetCellContentsFast(localX - 1, cactusY, localZ) == 0 &&
                                 chunk.GetCellContentsFast(localX, cactusY, localZ + 1) == 0 &&
                                 chunk.GetCellContentsFast(localX, cactusY, localZ - 1) == 0;
                                 cactusY++)
                            {
                                chunk.SetCellValueFast(localX, cactusY, localZ,
                                    BlockType.CactusBlock);
                            }

                            break;
                        }

                        case (int)BlockType.AirBlock:
                            continue; // 继续向下找
                    }

                    break; // 找到可用地形后不再往下搜索
                }
            }
        }
    }

    private float CalculateOceanShoreDistance(float x, float z)
    {
        if (_islandSize.HasValue)
        {
            float num = CalculateOceanShoreX(z);
            float num2 = CalculateOceanShoreZ(x);
            float num3 = CalculateOceanShoreX(z + 1000f) + _islandSize.Value.X;
            float num4 = CalculateOceanShoreZ(x + 1000f) + _islandSize.Value.Y;
            return MathUtils.Min(x - num, z - num2, num3 - x, num4 - z);
        }

        float num5 = CalculateOceanShoreX(z);
        float num6 = CalculateOceanShoreZ(x);
        return MathUtils.Min(x - num5, z - num6);
    }

    private float CalculateMountainRangeFactor(float x, float z)
    {
        return SimplexNoise.OctaveNoise(x + _mountainsOffset.X, z + _mountainsOffset.Y,
            _mountainRangeFreq / _biomeScaling, 3, 1.91f, 0.75f, ridged: true);
    }

    private int CalculateTemperature(float x, float z)
    {
        return Math.Clamp(
            (int)(MathUtils.Saturate(
                3f * SimplexNoise.OctaveNoise(x + _temperatureOffset.X, z + _temperatureOffset.Y,
                    0.0015f / _biomeScaling, 5, 2f, 0.6f) - 1.1f) * 16f), // 从 -1.1f 改成 -1.05f
            0, 15);
    }

    private int CalculateHumidity(float x, float z)
    {
        return Math.Clamp(
            (int)(MathUtils.Saturate(
                3f * SimplexNoise.OctaveNoise(x + _humidityOffset.X, z + _humidityOffset.Y,
                    0.0012f / _biomeScaling, 5, 2f, 0.6f) - 1.1f) * 16f), // 从 -1.1f 改成 -1.15f
            0, 15);
    }


    private float Squish(float v, float zero, float one)
    {
        return MathUtils.Saturate((v - zero) / (one - zero));
    }

    private float CalculateOceanShoreX(float z)
    {
        return _oceanCorner.X + _shoreFluctuations *
            SimplexNoise.OctaveNoise(z, 0f, 0.005f / _shoreFluctuationsScaling, 4, 1.95f, 1f);
    }

    private float CalculateOceanShoreZ(float x)
    {
        return _oceanCorner.Y + _shoreFluctuations *
            SimplexNoise.OctaveNoise(0f, x, 0.005f / _shoreFluctuationsScaling, 4, 1.95f, 1f);
    }

    private float CalculateForestDensity(float x, float z)
    {
        Point2 point = _terrain.ToChunk(new Vector2(x, z));
        bool flag = MathUtils.Hash((uint)(point.X + 1000 * point.Y)) % 1000 < 300;
        return MathUtils.Saturate(
            (SimplexNoise.OctaveNoise(point.X, point.Y, 0.1f, 2, 2f, 0.5f) - 0.25f) / 0.2f +
            (flag ? 0.6f : 0f));
    }

    private static readonly List<TerrainBrush> CoalBrushes = new();

    private static readonly List<TerrainBrush> IronBrushes = new();

    private static readonly List<TerrainBrush> CopperBrushes = new();

    private static readonly List<TerrainBrush> SaltpeterBrushes = new();

    private static readonly List<TerrainBrush> SulphurBrushes = new();

    private static readonly List<TerrainBrush> DiamondBrushes = new();

    private static readonly List<TerrainBrush> GermaniumBrushes = new();

    private static readonly List<TerrainBrush> DirtPocketBrushes = new();

    private static readonly List<TerrainBrush> GravelPocketBrushes = new();

    private static readonly List<TerrainBrush> LimestonePocketBrushes = new();

    private static readonly List<TerrainBrush> SandPocketBrushes = new();

    private static readonly List<TerrainBrush> BasaltPocketBrushes = new();

    private static readonly List<TerrainBrush> ClayPocketBrushes = new();

    private static readonly List<TerrainBrush> GranitePocketBrushes = new();

    private static readonly List<TerrainBrush> WaterPocketBrushes = new();

    private static readonly List<TerrainBrush> MagmaPocketBrushes = new();

    public static void CreateBrushes()
    {
        // 使用固定种子保证生成结果可重复
        var random = new Random(24);

        // 生成各类矿石笔刷
        GenerateCoalBrushes(random);
        GenerateIronBrushes(random);
        GenerateCopperBrushes(random);
        GenerateSaltpeterBrushes(random);
        GenerateSulphurBrushes(random);
        GenerateDiamondBrushes(random);
        GenerateGermaniumBrushes(random);

        // 生成资源矿脉
        GeneratePocketBrushes(random);

        // 水脉和岩浆需要特殊处理
        GenerateWaterPockets(random);
        GenerateMagmaPockets(random);

        // 生成洞穴系统
        GenerateCaveBrushes(random);
    }

    #region 矿石生成逻辑

    /// <summary>
    /// 生成煤矿笔刷
    /// 特征：中心单元格增强、允许较大高度变化
    /// </summary>
    private static void GenerateCoalBrushes(Random random)
    {
        const int brushCount = 16;

        for (int i = 0; i < brushCount; i++)
        {
            var brush = new TerrainBrush();
            int pathCount = random.Int(4, 12);

            GenerateMineralPaths(brush, random, pathCount,
                yRange: (-1f, 1f),
                segmentRange: (3, 8),
                (int)BlockType.CoalOreBlock);

            // 第一个笔刷添加中心强化单元格
            if (i == 0)
            {
                brush.AddCell(0, 0, 0, (int)BlockType.CoalBlock);
            }

            FinalizeBrush(brush, CoalBrushes);
        }
    }

    /// <summary>
    /// 生成铁矿笔刷
    /// 特征：较短的路径长度、中等高度变化
    /// </summary>
    private static void GenerateIronBrushes(Random random)
    {
        GenerateStandardMineralBrushes(
            random: random,
            pathCountRange: (3, 7),
            yRange: (-1f, 1f),
            segmentRange: (3, 6),
            blockType: (int)BlockType.IronOreBlock,
            targetList: IronBrushes
        );
    }

    /// <summary>
    /// 生成铜矿笔刷
    /// 特征：允许更大的垂直变化
    /// </summary>
    private static void GenerateCopperBrushes(Random random)
    {
        GenerateStandardMineralBrushes(
            random: random,
            pathCountRange: (4, 10),
            yRange: (-2f, 2f),
            segmentRange: (3, 6),
            blockType: (int)BlockType.CopperOreBlock,
            targetList: CopperBrushes
        );
    }

    /// <summary>
    /// 生成盐硝石笔刷
    /// 特征：水平方向为主，高度变化较小
    /// </summary>
    private static void GenerateSaltpeterBrushes(Random random)
    {
        GenerateStandardMineralBrushes(
            random: random,
            pathCountRange: (8, 16),
            yRange: (-0.25f, 0.25f),
            segmentRange: (4, 8),
            blockType: (int)BlockType.SaltpeterOreBlock,
            targetList: SaltpeterBrushes
        );
    }

    /// <summary>
    /// 生成硫磺笔刷
    /// 特征：中等规模分布
    /// </summary>
    private static void GenerateSulphurBrushes(Random random)
    {
        GenerateStandardMineralBrushes(
            random: random,
            pathCountRange: (4, 10),
            yRange: (-1f, 1f),
            segmentRange: (3, 6),
            blockType: (int)BlockType.SulphurOreBlock,
            targetList: SulphurBrushes
        );
    }

    /// <summary>
    /// 生成钻石笔刷
    /// 特征：稀有矿物，生成路径较少
    /// </summary>
    private static void GenerateDiamondBrushes(Random random)
    {
        GenerateStandardMineralBrushes(
            random: random,
            pathCountRange: (2, 6),
            yRange: (-1f, 1f),
            segmentRange: (3, 6),
            blockType: (int)BlockType.DiamondOreBlock,
            targetList: DiamondBrushes
        );
    }

    /// <summary>
    /// 生成锗矿笔刷 
    /// 特征：科技矿物，中等密度分布
    /// </summary>
    private static void GenerateGermaniumBrushes(Random random)
    {
        GenerateStandardMineralBrushes(
            random: random,
            pathCountRange: (4, 10),
            yRange: (-1f, 1f),
            segmentRange: (3, 6),
            blockType: (int)BlockType.GermaniumOreBlock,
            targetList: GermaniumBrushes
        );
    }

    #endregion

    #region 通用生成方法

    /// <summary>
    /// 标准矿石生成模板方法
    /// </summary>
    /// <param name="pathCountRange">路径数量范围 (min, max)</param>
    /// <param name="yRange">Y轴方向变化范围 (minY, maxY)</param>
    /// <param name="segmentRange">每条路径的段数范围 (minSegments, maxSegments)</param>
    /// <param name="blockType">要生成的方块类型ID</param>
    /// <param name="targetList">目标存储列表</param>
    private static void GenerateStandardMineralBrushes(
        Random random,
        (int Min, int Max) pathCountRange,
        (float Min, float Max) yRange,
        (int Min, int Max) segmentRange,
        int blockType,
        IList<TerrainBrush> targetList)
    {
        const int brushCount = 16;

        for (int i = 0; i < brushCount; i++)
        {
            var brush = new TerrainBrush();
            int pathCount = random.Int(pathCountRange.Min, pathCountRange.Max);

            GenerateMineralPaths(
                brush: brush,
                random: random,
                pathCount: pathCount,
                yRange: yRange,
                segmentRange: segmentRange,
                blockType: blockType
            );

            FinalizeBrush(brush, targetList);
        }
    }

    /// <summary>
    /// 生成矿物路径核心逻辑
    /// </summary>
    private static void GenerateMineralPaths(
        TerrainBrush brush,
        Random random,
        int pathCount,
        (float Min, float Max) yRange,
        (int Min, int Max) segmentRange,
        int blockType)
    {
        for (int p = 0; p < pathCount; p++)
        {
            // 生成随机方向向量（标准化后缩放）
            var direction = Vector3.Normalize(new Vector3(
                x: random.Float(-1f, 1f),
                y: random.Float(yRange.Min, yRange.Max),
                z: random.Float(-1f, 1f)
            ) * 0.5f);

            int segments = random.Int(segmentRange.Min, segmentRange.Max);
            var position = Vector3.Zero;

            // 沿路径生成方块
            for (int s = 0; s < segments; s++)
            {
                brush.AddBox(
                    (int)MathF.Floor(position.X),
                    (int)MathF.Floor(position.Y),
                    (int)MathF.Floor(position.Z),
                    1, 1, 1, blockType
                );
                position += direction;
            }
        }
    }

    /// <summary>
    /// 完成笔刷的最终处理
    /// </summary>
    private static void FinalizeBrush(TerrainBrush brush, ICollection<TerrainBrush> targetList)
    {
        brush.Compile();
        targetList.Add(brush);
    }

    #endregion

    #region 矿脉生成逻辑

    /// <summary>
    /// 生成各种资源矿脉（泥土、砂石等）
    /// </summary>
    private static void GeneratePocketBrushes(Random random)
    {
        // 泥土
        GeneratePocketType(
            random: random,
            blockType: (int)BlockType.DirtBlock,
            pathCountRange: (16, 32),
            yRange: (-0.75f, 0.75f),
            segmentRange: (6, 12),
            targetList: DirtPocketBrushes
        );

        // 砂石
        GeneratePocketType(
            random: random,
            blockType: (int)BlockType.GravelStoneBlock,
            pathCountRange: (16, 32),
            yRange: (-0.75f, 0.75f),
            segmentRange: (6, 12),
            targetList: GravelPocketBrushes
        );
        // 石灰岩
        GeneratePocketType(
            random: random,
            blockType: (int)BlockType.LimeStoneBlock,
            pathCountRange: (16, 32),
            yRange: (-0.75f, 0.75f),
            segmentRange: (6, 12),
            targetList: LimestonePocketBrushes
        );

        // 粘土
        GenerateVerticalRestrictedPocket(
            random: random,
            blockType: (int)BlockType.ClayBlock,
            verticalRange: (-0.1f, 0.1f),
            targetList: ClayPocketBrushes
        );

        // 砂矿
        GeneratePocketType(
            random: random,
            blockType: (int)BlockType.SandBlock,
            pathCountRange: (16, 32),
            yRange: (-0.75f, 0.75f),
            segmentRange: (6, 12),
            targetList: SandPocketBrushes
        );

        // 玄武岩
        GeneratePocketType(
            random: random,
            blockType: (int)BlockType.BasaltStoneBlock,
            pathCountRange: (16, 32),
            yRange: (-0.75f, 0.75f),
            segmentRange: (6, 12),
            targetList: BasaltPocketBrushes
        );

        // 花岗岩
        GenerateOmniDirectionPocket(
            random: random,
            blockType: (int)BlockType.GraniteStoneBlock,
            pathCountRange: (16, 32),
            segmentRange: (5, 10),
            targetList: GranitePocketBrushes
        );
    }

    #region 特殊矿脉生成

    /// <summary>
    /// 生成垂直受限矿脉（如粘土）
    /// </summary>
    private static void GenerateVerticalRestrictedPocket(
        Random random,
        int blockType,
        (float Min, float Max) verticalRange,
        IList<TerrainBrush> targetList)
    {
        const int brushCount = 16;
        (int, int) pathCountRange = (16, 32);
        (int, int) segmentRange = (6, 12);

        for (int i = 0; i < brushCount; i++)
        {
            var brush = new TerrainBrush();
            int pathCount = random.Int(pathCountRange.Item1, pathCountRange.Item2);

            for (int p = 0; p < pathCount; p++)
            {
                var direction = Vector3.Normalize(new Vector3(
                    random.Float(-1f, 1f),
                    random.Float(verticalRange.Min, verticalRange.Max),
                    random.Float(-1f, 1f)
                ) * 0.5f);

                GeneratePathSegments(brush, direction, random, segmentRange, blockType);
            }

            FinalizeBrush(brush, targetList);
        }
    }

    /// <summary>
    /// 全方向矿脉生成（花岗岩）
    /// </summary>
    private static void GenerateOmniDirectionPocket(
        Random random,
        int blockType,
        (int Min, int Max) pathCountRange,
        (int Min, int Max) segmentRange,
        IList<TerrainBrush> targetList)
    {
        const int brushCount = 16;

        for (int i = 0; i < brushCount; i++)
        {
            var brush = new TerrainBrush();
            int pathCount = random.Int(pathCountRange.Min, pathCountRange.Max);

            for (int p = 0; p < pathCount; p++)
            {
                // 完全随机方向
                var direction = Vector3.Normalize(
                    new Vector3(
                        random.Float(-1f, 1f),
                        random.Float(-1f, 1f),
                        random.Float(-1f, 1f)
                    ) * 0.5f);

                GeneratePathSegments(brush, direction, random, segmentRange, blockType);
            }

            FinalizeBrush(brush, targetList);
        }
    }

    /// <summary>
    /// 生成路径段通用方法
    /// </summary>
    private static void GeneratePathSegments(
        TerrainBrush brush,
        Vector3 direction,
        Random random,
        (int Min, int Max) segmentRange,
        int blockType)
    {
        int segments = random.Int(segmentRange.Min, segmentRange.Max);
        Vector3 position = Vector3.Zero;

        for (int s = 0; s < segments; s++)
        {
            brush.AddBox(
                (int)MathF.Floor(position.X),
                (int)MathF.Floor(position.Y),
                (int)MathF.Floor(position.Z),
                1, 1, 1, blockType
            );
            position += direction;
        }
    }

    #endregion

    #region 水体与岩浆生成

    /// <summary>
    /// 生成地下水脉
    /// 原始逻辑：多层结构，包含地表和地下部分
    /// </summary>
    private static void GenerateWaterPockets(Random random)
    {
        int[] sizePresets = { 4, 6, 8 };
        const int variantsPerSize = 4;

        foreach (int baseSize in sizePresets)
        {
            for (int variant = 0; variant < variantsPerSize; variant++)
            {
                var brush = new TerrainBrush();
                int depth = variant % 2 + 1; // 计算深度层数
                float sizeModifier = (variant == 2) ? 0.5f : 1f;
                int pointCount = (variant == 1) ? baseSize * baseSize : 2 * baseSize * baseSize;

                GenerateLayeredPocket(
                    brush: brush,
                    random: random,
                    baseSize: baseSize,
                    depth: depth,
                    sizeModifier: sizeModifier,
                    pointCount: pointCount,
                    surfaceBlock: (int)BlockType.AirBlock, // 空气
                    undergroundBlock: (int)BlockType.WaterBlock // 水
                );

                FinalizeBrush(brush, WaterPocketBrushes);
            }
        }
    }

    /// <summary>
    /// 生成岩浆矿脉
    /// 特征：更大尺寸、多层结构
    /// </summary>
    private static void GenerateMagmaPockets(Random random)
    {
        int[] sizePresets = { 8, 12, 14, 16 };
        const int variantsPerSize = 4;

        foreach (int baseSize in sizePresets)
        {
            for (int variant = 0; variant < variantsPerSize; variant++)
            {
                var brush = new TerrainBrush();
                int totalDepth = baseSize + 2;
                float sizeModifier = (variant == 2) ? 0.5f : 1f;
                int pointCount = (variant == 1) ? baseSize * baseSize : 2 * baseSize * baseSize;

                GenerateLayeredPocket(
                    brush: brush,
                    random: random,
                    baseSize: baseSize,
                    depth: totalDepth,
                    sizeModifier: sizeModifier,
                    pointCount: pointCount,
                    surfaceBlock: (int)BlockType.AirBlock, // 空气
                    undergroundBlock: (int)BlockType.MagmaBlock // 岩浆
                );

                FinalizeBrush(brush, MagmaPocketBrushes);
            }
        }
    }

    /// <summary>
    /// 分层矿脉生成核心逻辑
    /// </summary>
    private static void GenerateLayeredPocket(
        TerrainBrush brush,
        Random random,
        int baseSize,
        int depth,
        float sizeModifier,
        int pointCount,
        int surfaceBlock,
        int undergroundBlock)
    {
        for (int i = 0; i < pointCount; i++)
        {
            Vector2 position = random.Vector2(0f, baseSize);
            float distanceFactor = position.Length() / baseSize;

            // 地表部分
            int surfaceWidth = random.Int(3, 4);
            int surfaceHeight = 1 + (int)MathUtils.Lerp(
                Math.Max(baseSize / 3f, 2.5f) * sizeModifier,
                0f,
                distanceFactor
            ) + random.Int(0, 1);

            brush.AddBox(
                (int)MathF.Floor(position.X), 0, (int)MathF.Floor(position.Y),
                surfaceWidth, surfaceHeight, surfaceWidth,
                surfaceBlock
            );

            // 地下部分
            int undergroundDepth = 1 + (int)MathUtils.Lerp(
                depth,
                0f,
                distanceFactor
            ) + random.Int(0, 1);

            brush.AddBox(
                (int)MathF.Floor(position.X), -undergroundDepth, (int)MathF.Floor(position.Y),
                surfaceWidth, undergroundDepth, surfaceWidth,
                undergroundBlock
            );
        }
    }

    #endregion

    /// <summary>
    /// 通用矿脉生成方法
    /// </summary>
    private static void GeneratePocketType(
        Random random,
        int blockType,
        (int Min, int Max) pathCountRange,
        (float Min, float Max) yRange,
        (int Min, int Max) segmentRange,
        IList<TerrainBrush> targetList)
    {
        const int brushCount = 16;

        for (int i = 0; i < brushCount; i++)
        {
            var brush = new TerrainBrush();
            int pathCount = random.Int(pathCountRange.Min, pathCountRange.Max);

            for (int p = 0; p < pathCount; p++)
            {
                var direction = Vector3.Normalize(new Vector3(
                    random.Float(-1f, 1f),
                    random.Float(yRange.Min, yRange.Max),
                    random.Float(-1f, 1f)
                )) * 0.5f;

                int segments = random.Int(segmentRange.Min, segmentRange.Max);
                var position = Vector3.Zero;

                for (int s = 0; s < segments; s++)
                {
                    brush.AddBox(
                        (int)MathF.Floor(position.X),
                        (int)MathF.Floor(position.Y),
                        (int)MathF.Floor(position.Z),
                        1, 1, 1, blockType
                    );
                    position += direction;
                }
            }

            FinalizeBrush(brush, targetList);
        }
    }

    #endregion

    #region 洞穴生成系统

    /// <summary>
    /// 生成洞穴笔刷（7种类型，每种3个变体）
    /// </summary>
    private static void GenerateCaveBrushes(Random random)
    {
        for (int type = 0; type < 7; type++)
        {
            CaveBrushesByType.Add(new List<TerrainBrush>());

            // 每个类型生成3个变体
            for (int variant = 0; variant < 3; variant++)
            {
                var brush = new TerrainBrush();
                int pathCount = 6 + 4 * type;
                int maxWidth = 3 + type / 3;
                int maxLength = 9 + type;

                GenerateCavePaths(brush, random, pathCount, maxWidth, maxLength);

                brush.Compile();
                CaveBrushesByType[type].Add(brush);
            }
        }
    }

    /// <summary>
    /// 生成单个洞穴路径
    /// </summary>
    private static void GenerateCavePaths(
        TerrainBrush brush,
        Random random,
        int pathCount,
        int maxWidth,
        int maxLength)
    {
        for (int p = 0; p < pathCount; p++)
        {
            int width = random.Int(2, maxWidth);
            int length = random.Int(8, maxLength) - 2 * width;
            var direction = 0.5f * new Vector3(
                random.Float(-1f, 1f),
                random.Float(0f, 1f),
                random.Float(-1f, 1f)
            );

            var position = Vector3.Zero;
            for (int s = 0; s < length; s++)
            {
                brush.AddBox(
                    (int)MathF.Floor(position.X) - width / 2,
                    (int)MathF.Floor(position.Y) - width / 2,
                    (int)MathF.Floor(position.Z) - width / 2,
                    width, width, width, 0
                );
                position += direction;
            }
        }
    }

    #endregion
}