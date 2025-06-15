using System.Collections;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MineCraftService.Web;

public class IgnoreForJsonConverter<T> : JsonConverter<T> where T : class, new()
{
    private JsonSerializerOptions GetCleanOptions(JsonSerializerOptions options)
    {
        // 如果不去掉自己就会死亡递归
        var cleanOptions = new JsonSerializerOptions(options);
        var thisConverter =
            cleanOptions.Converters.FirstOrDefault(c =>
                c.GetType() == typeof(IgnoreForJsonConverter<T>));
        if (thisConverter != null)
        {
            cleanOptions.Converters.Remove(thisConverter);
        }
        return cleanOptions;
    }
    
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var cleanOptions = GetCleanOptions(options);
        // 使用 JsonSerializer 反序列化 JSON 为 T 类型的实例
        return JsonSerializer.Deserialize<T>(ref reader, cleanOptions);
    }


    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        
        var cleanOptions = GetCleanOptions(options);
        // 使用干净 options 序列化为 JsonNode
        var jsonObj = JsonSerializer.SerializeToNode(value, cleanOptions)?.AsObject();
        if (jsonObj == null)
        {
            writer.WriteNullValue();
            return;
        }

        // 移除需要忽略的属性
        foreach (var prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var ignore = prop.GetCustomAttribute<IgnoreForJsonAttribute>();
            if (ignore?.IgnoreInResponse == true)
            {
                var name = prop.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? prop.Name;
                jsonObj.Remove(name);
            }
        }

        jsonObj.WriteTo(writer, options);
    }
}