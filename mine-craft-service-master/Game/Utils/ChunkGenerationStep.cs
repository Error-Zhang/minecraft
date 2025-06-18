namespace MineCraftService.Game;

public class ChunkGenerationStep(int generateOrder, Action<TerrainChunk> action)
{
    public readonly int GenerateOrder = generateOrder;
    
    public Action<TerrainChunk> GenerateAction = action;
}