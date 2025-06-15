using System.Collections;
using System.Reflection;

namespace MineCraftService.Web;

using MiniValidation;

public static class MyValidation
{
    public static IResult ValidateAndRun<T>(T model, Func<T, IResult> action)
    {
        if (!MiniValidator.TryValidate(model, out var errors))
        {
            // 返回统一结构，包含详细错误信息
            return ApiResponse.Fail(400, "参数验证失败", errors);
        }

        return action(model);
    }

    public static async Task<IResult> ValidateAndRunAsync<T>(T model, Func<T, Task<IResult>> action)
    {
        if (!MiniValidator.TryValidate(model, out var errors))
        {
            return ApiResponse.Fail(400, "参数验证失败", errors);
        }
        
        return await action(model);
    }

    [Obsolete("该方法已不在使用")]
    private static void FilterIgnoredProperties<T>(T model, string context)
    {
        if (model == null) return;

        var properties = model.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            if (property.GetIndexParameters().Length > 0)
                continue;

            try
            {
                var ignoreAttribute = property.GetCustomAttribute<IgnoreForJsonAttribute>();
                var value = property.GetValue(model);

                // 集合类型优先处理
                if (value is IEnumerable enumerable && property.PropertyType != typeof(string))
                {
                    foreach (var item in enumerable)
                    {
                        if (item != null && item.GetType().IsClass && item.GetType() != typeof(string))
                        {
                            FilterIgnoredProperties(item, context);
                        }
                    }
                }
                else if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
                {
                    if (value != null)
                    {
                        FilterIgnoredProperties(value, context);
                    }
                }

                // 忽略属性处理
                if (ignoreAttribute != null && property.CanWrite)
                {
                    if ((context == "request" && ignoreAttribute.IgnoreInRequest) ||
                        (context == "response" && ignoreAttribute.IgnoreInResponse))
                    {
                        property.SetValue(model, null);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing property {property.Name}: {ex.Message}");
            }
        }
    }

}