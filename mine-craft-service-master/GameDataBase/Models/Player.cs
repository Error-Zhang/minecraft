using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using MineCraftService.Web;

namespace MineCraftService.GameDataBase.Models;

public class Player
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [IgnoreForJson(IgnoreInRequest = true)]
    public int Id { get; set; }

    [StrictRequired] public string PlayerName { get; set; } = DefaultValue.String;

    [ForeignKey("User")]
    [IgnoreForJson(IgnoreInResponse = true)]
    public int UserId { get; set; }
    
    public int Sex { get; set; }

    [IgnoreForJson(IgnoreInRequest = true)]
    public User User { get; set; }

    [ForeignKey("World")][IgnoreForJson(IgnoreInResponse = true)] public int WorldId { get; set; }

    [JsonIgnore] public World World { get; set; }

    [IgnoreForJson(IgnoreInRequest = true)]
    public DateTime CreatedAt { get; set; }
}