using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MineCraftService.Web;

namespace MineCraftService.GameDataBase.Models;


public class User
{
    // 强制自动生成(忽略设置的值)
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [IgnoreForJson(IgnoreInRequest = true)]
    public int Id { get; set; }

    [StrictRequired]
    public string UserName { get; set; } = DefaultValue.String;

    [StrictRequired]
    [IgnoreForJson(IgnoreInResponse = true)]
    public string PassWord { get; set; } = DefaultValue.String;
}