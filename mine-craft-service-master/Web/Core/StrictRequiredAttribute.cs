namespace MineCraftService.Web;

using System.ComponentModel.DataAnnotations;

// TODO:该类须配合DefaultValue进行使用,或者使用内置的[Required]配合?进行使用
public class StrictRequiredAttribute : ValidationAttribute
{
    private Type? EnumType { get; set; }
    private bool BindId { get; set; }
    

    public StrictRequiredAttribute()
    {
        
    }

    public StrictRequiredAttribute(bool bindId)
    {
        BindId = bindId;
    }

    public StrictRequiredAttribute(Type enumType)
    {
        EnumType = enumType;
    }
    
    // 如果验证没有生效，优先检查一下是否是属性(含有get;set;方法)不能是字段
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return new ValidationResult(ErrorMessage);
        }

        if (value is string str && str == DefaultValue.String)
        {
            return new ValidationResult(ErrorMessage);
        }

        if (BindId && (int)value <= DefaultValue.Id)
        {
            return new ValidationResult(ErrorMessage);
        }
        
        if (EnumType != null && !Enum.IsDefined(EnumType, value))
        {
            return new ValidationResult(ErrorMessage);
        }

        return ValidationResult.Success;
    }
}