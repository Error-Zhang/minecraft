using MineCraftService.GameDataBase;
using MineCraftService.GameDataBase.Models;

namespace MineCraftService.Web;

public static class WorldRoutes
{
    // 扩展方法：由传入对象改为由对象调用通过在参数前增加this关键字
    public static IEndpointRouteBuilder MapWorldRoutes(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/world");

        // 获取世界列表
        group.MapGet("/{userId}", async (int userId, WorldService service) =>
        {
            var worlds = await service.GetWorlds(userId);
            return ApiResponse.Success(worlds);
        });

        // 创建世界
        group.MapPost("/",
            ( World world, WorldService service, UserService userService) =>
                MyValidation.ValidateAndRunAsync(world,
                    async _ =>
                    {
                        if (world.WorldName.Length > 16 || world.Seed.Length > 16)
                        {
                            return ApiResponse.Fail(401,"世界名称和种子必须小于16个字符");
                        }
                        var user = await userService.GetUser(world.UserId);
                        if (user == null) return ApiResponse.NotFound("User");
                        var result = await service.CreateWorld(world);
                        return ApiResponse.Success(result);
                    }));

        // 更改世界
        group.MapPut("/",async (World world,WorldService service) =>
        {
            var ok = await service.UpdateWorld(world);
            return ok ? ApiResponse.Success(world) : ApiResponse.Fail(402,"世界不存在或不属于当前用户");
        });
        
        // 删除世界
        group.MapDelete("/", async (int worldId, int userId,WorldService service) =>
        {
            var ok = await service.DeleteWorld(worldId,userId);
            return ok ? ApiResponse.Success() : ApiResponse.Fail(402,"世界不存在或不属于当前用户");
        });

        return routes;
    }
}
