namespace MineCraftService.Game;

// TODO: (special)复合型int数据，前端要进行解码，来获取附加信息
public enum BlockType
{
    AirBlock, // 空气
    WaterBlock, // 水
    MagmaBlock, // 岩浆
    
    DirtBlock, // 泥土
    GrassBlock, // 草方块
    ClayBlock, // 粘土块
    IceBlock, // 冰块
    
    TallGrassBlock, // 高草(special)
    RedFlowerBlock,
    PurpleFlowerBlock,
    WhiteFlowerBlock,
    RyeBlock, // 黑麦(special)
    CottonBlock, // 棉花(special)
    DryBushBlock, // 干枯灌木:一种在干旱或沙漠环境中生成的小型植物类装饰方块
    LargeDryBushBlock, // 大型干枯灌
    CactusBlock, // 仙人掌
    
    SandBlock,
    SandStoneBlock,
    GraniteStoneBlock, // 花岗岩
    BasaltStoneBlock, // 玄武岩
    GravelStoneBlock,// 碎石块
    LimeStoneBlock, // 石灰石
    
    CoalBlock, // 纯煤矿
    CoalOreBlock, // 煤矿
    IronOreBlock, // 铁矿
    CopperOreBlock,// 铜矿
    SaltpeterOreBlock,// 硝石矿
    SulphurOreBlock, // 硫磺矿
    DiamondOreBlock, // 钻石矿
    GermaniumOreBlock, // 锗矿
    
    OakWoodBlock, // 橡树木块
    BirchWoodBlock, // 桦树木块
    SpruceWoodBlock, // 云杉木块
    MimosaWoodBlock, // 金合欢木块
    PoplarWoodBlock, // 白杨木块
    
    OakLeavesBlock,
    BirchLeavesBlock,
    SpruceLeavesBlock,
    TallSpruceLeavesBlock,
    MimosaLeavesBlock,
    PoplarLeavesBlock,
}