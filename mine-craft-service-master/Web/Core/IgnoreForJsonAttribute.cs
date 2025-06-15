using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace MineCraftService.Web;

// TODO:请求验证器不会自动生效，需调用MyValidation进行验证，响应器注册在GameDatabase.Models命名空间下的类会自动处理
public class IgnoreForJsonAttribute(bool ignoreInRequest = false, bool ignoreInResponse = false) : ValidationAttribute
{
    public bool IgnoreInRequest { get; set; } = ignoreInRequest;
    public bool IgnoreInResponse { get; set; } = ignoreInResponse;
    
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        // 仅在请求时进行验证
        if (IgnoreInRequest && value != null)
        {
            var type = value.GetType();

            // 对于值类型，检查是否为默认值
            
            if (type.IsValueType)
            {
                var defaultValue = Activator.CreateInstance(type);
                // 命名规定CreatedAt字段自动赋值不可更改
                if (!Equals(value, defaultValue))
                    return new ValidationResult( $"{validationContext.DisplayName} 不允许在请求中指定");
            }
            // 对于引用类型，非 null 直接视为被指定
            else
            {
                return new ValidationResult($"{validationContext.DisplayName} 不允许在请求中指定");
            }
        }

        return ValidationResult.Success;
    }
}