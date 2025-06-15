using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using MineCraftService.GameDataBase;
using MineCraftService.GameHub;
using MineCraftService.Web;


var builder = WebApplication.CreateBuilder(args);

/*添加api文档 dotnet add package Microsoft.AspNetCore.OpenApi
Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
http://localhost:5110/openapi/v1.json*/
builder.Services.AddOpenApi();

// 注册自定义Json序列化
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    var targetNamespace = "MineCraftService.GameDataBase.Models"; // 自动注册所有模型的自定义Json解析
    var modelTypes = Assembly.GetExecutingAssembly()
        .GetTypes()
        .Where(t => t is { IsClass: true, IsAbstract: false } && t.Namespace == targetNamespace);
    
    foreach (var type in modelTypes)
    {
        var converterType = typeof(IgnoreForJsonConverter<>).MakeGenericType(type);
        var converter = (JsonConverter)Activator.CreateInstance(converterType)!;
        options.SerializerOptions.Converters.Add(converter);
    }
});

// 注册数据库
builder.Services.AddDbContext<GameDb>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("GameDb")));

// 注册服务
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<PlayerService>();
builder.Services.AddScoped<WorldService>();
builder.Services.AddScoped<ChunkService>();

// 注册实时通信服务
builder.Services.AddSignalR();

// 注册Cors
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .WithOrigins("http://localhost:4110");
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    // 使用 CORS 策略
    app.UseCors();
}

// 注册路由
app.MapUserRoutes();
app.MapWorldRoutes();
app.MapPlayerRoutes();
app.MapBlockRoutes();
app.MapChunkRoutes();

// 注册消息中心
app.MapHub<PlayerHub>("/playerHub");
app.MapHub<WorldHub>("/worldHub");

app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "Assets")),
    RequestPath = "/assets"
});
app.Run();