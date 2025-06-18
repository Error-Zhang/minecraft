namespace MineCraftService.Web;

public static class ChunkRoutes
{
    public static void MapChunkRoutes(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/chunk");

        // 生成区块
        group.MapPost("/generate/{worldId}",
            async (int worldId, List<Coord> coords, ChunkService chunkService) =>
            {
                var coordinates = coords.Select(c => (c.X, c.Z)).ToList();
                var result = await chunkService.GenerateChunks(worldId, coordinates);
                if (result == null) return ApiResponse.Fail(400, "世界尚未初始化,请先建立websocket连接");
                return ApiResponse.Success(result);
            });
    }
}