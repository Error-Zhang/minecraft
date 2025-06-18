using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using MineCraftService.Web;

namespace MineCraftService.GameDataBase.Models;


public class World
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [IgnoreForJson(IgnoreInRequest = true)]
    public int Id { get; set; }

    [StrictRequired] public string WorldName { get; set; } = DefaultValue.String;
    [StrictRequired] public string Seed { get; set; } = string.Empty;

    [StrictRequired(typeof(GameMode))] 
    public GameMode GameMode{ get; set; } = (GameMode)DefaultValue.Enum;

    [StrictRequired(typeof(WorldMode))]
    public WorldMode WorldMode{ get; set; } = (WorldMode)DefaultValue.Enum;
    
    [StrictRequired(typeof(Season))]
    public Season Season { get; set; } = (Season)DefaultValue.Enum;

    [IgnoreForJson(IgnoreInRequest = true)]
    public DateTime CreatedAt { get; set; }
    
    public int IsPublic { get; set; }

    [ForeignKey("User")]
    [StrictRequired(bindId:true)]
    [IgnoreForJson(IgnoreInResponse = true)]public int UserId { get; set; } = DefaultValue.Id; // 声明外键指向User字段

    public User User { get; set; } // 导航属性
    
}