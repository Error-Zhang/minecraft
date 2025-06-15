namespace MineCraftService.Game;

public static class BlocksManager
{
    private static readonly List<BlockType> TransparentBlocks =
    [
        BlockType.AirBlock,
        BlockType.WaterBlock,
        BlockType.MagmaBlock,
    ];

    private static readonly List<BlockType> NotCollidableBlocks =
    [
        BlockType.AirBlock,
        BlockType.WaterBlock,
        BlockType.MagmaBlock,
    ];
    
    private static readonly List<BlockType> FluidBlocks =
    [
        BlockType.WaterBlock,
        BlockType.MagmaBlock,
    ];
    
    public static bool IsTransparent(int blockType)
    {
        return TransparentBlocks.Contains((BlockType)blockType);
    }

    public static bool IsCollidable(int blockType)
    {
        return !NotCollidableBlocks.Contains((BlockType)blockType);
    }

    public static bool IsFluidBlock(int blockType)
    {
        return FluidBlocks.Contains((BlockType)blockType);
    }
}