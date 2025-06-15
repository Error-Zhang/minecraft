using MineCraftService.GameDataBase;
using MineCraftService.GameDataBase.Models;

namespace MineCraftService.Web;

public static class PlayerRoutes
{
    public static IEndpointRouteBuilder MapPlayerRoutes(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/player");
        
        group.MapGet("/", async (int userId,int worldId,PlayerService playerService) =>
        {
            var players = await playerService.GetPlayer(userId,worldId);
            if(players == null) return ApiResponse.NotFound("Player");
            return ApiResponse.Success(players);
        });

        // 获取玩家列表
        group.MapGet("/list/{worldId}", async (int worldId,PlayerService playerService) =>
        {
            var players = await playerService.GetPlayers(worldId);
            return ApiResponse.Success(players);
        });

        // 添加玩家到世界
        group.MapPost("/", async (Player player, PlayerService playerService) =>
        {
            var exist = await playerService.HasPlayerInWorld(player.UserId, player.WorldId);
            if(exist) return ApiResponse.Fail(401,"玩家已存在");
            var success = await playerService.AddPlayer(player);
            return success
                ? ApiResponse.Success()
                : ApiResponse.Fail(402,"世界不存在或世界不属于当前用户");
        });
        
        // 删除玩家
        group.MapDelete("/", async (int userId, int worldId, int playerId, PlayerService playerService) =>
        {
            // 1. 获取玩家信息
            var player = await playerService.GetPlayer(playerId);
            if (player == null)
                return ApiResponse.NotFound("Player");

            // 2. 检查玩家是否属于指定世界
            var playerBelongsToWorld = await playerService.PlayerBelongsToWorld(playerId, worldId);
            if (!playerBelongsToWorld)
                return ApiResponse.Fail(401, "玩家不属于当前用户");

            // 3. 防止用户删除自己
            if (userId == player.UserId)
                return ApiResponse.Fail(401, "不能删除自己");

            // 4. 删除玩家
            await playerService.RemovePlayer(player);
            return ApiResponse.Success();
        });
        
        return routes;
    }
}