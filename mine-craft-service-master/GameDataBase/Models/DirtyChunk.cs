using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MineCraftService.GameDataBase.Models;

public class DirtyChunk
{
    public int ChunkX { get; set; }
    public int ChunkZ { get; set; }

    [ForeignKey("World")]
    public int WorldId { get; set; }
    public World World { get; set; }

    // 存储序列化后的 JSON（包含多个 block 的局部坐标与 blockId）
    public string BlockData { get; set; } = string.Empty;
}