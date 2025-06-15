using MineCraftService.GameDataBase.Models;

namespace MineCraftService.Web;

public class WorldDto
{
    public int Id { get; set; }
    public string WorldName { get; set; } = string.Empty;
    public string Seed { get; set; } = string.Empty;
    public GameMode GameMode { get; set; }
    public WorldMode WorldMode { get; set; }
    public Season Season { get; set; }
    public int IsPublic { get; set; }
    public DateTime CreatedAt { get; set; }

    public User User { get; set; }

    public List<Player> Players { get; set; } = [];
}
