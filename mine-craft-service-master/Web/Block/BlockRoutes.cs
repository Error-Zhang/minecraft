using MineCraftService.Game;

namespace MineCraftService.Web;

public static class BlockRoutes
{
    public static void MapBlockRoutes(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/block");

        // 获取所有 BlockType 枚举项
        group.MapGet("/types", () =>
        {
            var names = Enum.GetNames(typeof(BlockType));
            var values = Enum.GetValues(typeof(BlockType)).Cast<BlockType>();

            var byName = names
                .Zip(values, (name, val) => new { name, val })
                .ToDictionary(x => x.name, x => Convert.ToInt32(x.val));

            var byId = values
                .ToDictionary(val => Convert.ToInt32(val).ToString(), val => val.ToString());

            var result = new {
                byName,
                byId
            };

            return ApiResponse.Success(result);
        });
    }
}
