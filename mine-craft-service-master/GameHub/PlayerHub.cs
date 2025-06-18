using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using MineCraftService.Web;

namespace MineCraftService.GameHub;

public class PlayerHub : Hub
{
    private static readonly ConcurrentDictionary<string, (int worldId, int playerId)>
        ConnectionMap = new();
    private static readonly ConcurrentDictionary<int, PlayerMoveData> PlayerPositions = new();

    public async Task<ApiResponse> JoinWorld(int worldId, int playerId)
    {
        if (worldId <= 0 || playerId <= 0)
            return ApiResponse.RawFail(400, "参数错误,请检查worldId和playerId是否存在");
        
        // 防止重复加入：判断当前 playerId 是否已存在
        if (ConnectionMap.Values.Any(v => v.playerId == playerId && v.worldId == worldId))
        {
            return ApiResponse.RawFail(409, $"玩家 {playerId} 已经在世界 {worldId} 中");
        }
        ConnectionMap[Context.ConnectionId] = (worldId, playerId);
        await Groups.AddToGroupAsync(Context.ConnectionId, GetWorldGroup(worldId));
       
        // 向同一世界的其他玩家广播加入消息（不包含自己）
        await Clients.GroupExcept(GetWorldGroup(worldId), [Context.ConnectionId])
            .SendAsync("PlayerJoined", playerId);
        return ApiResponse.RawSuccess();
    }
    
    public Task<List<int>> GetPlayersInWorld()
    {
        var (worldId, playerId) = ConnectionMap[Context.ConnectionId];
        var players = ConnectionMap
            .Where(kvp => kvp.Value.worldId == worldId && kvp.Value.playerId != playerId)
            .Select(kvp => kvp.Value.playerId)
            .Distinct() 
            .ToList();

        return Task.FromResult(players);
    }
    
    public async Task PlayerMove(PlayerMoveData data)
    {
        if (ConnectionMap.TryGetValue(Context.ConnectionId, out var info))
        {
            // 更新该玩家当前位置
            PlayerPositions[info.playerId] = data;
            await Clients.GroupExcept(GetWorldGroup(info.worldId), [Context.ConnectionId])
                         .SendAsync("PlayerMove", data);
        }
    }
    public Task<ApiResponse> GetPlayerPosition(int playerId)
    {
        var (worldId, _) = ConnectionMap[Context.ConnectionId];
        if (!ConnectionMap.Values.Any(v => v.worldId == worldId && v.playerId == playerId))
        {
            return Task.FromResult(ApiResponse.RawFail(404, "该玩家未在指定世界中"));
        }

        if (PlayerPositions.TryGetValue(playerId, out var position))
        {
            return Task.FromResult(ApiResponse.RawSuccess(position));
        }
        return Task.FromResult(ApiResponse.RawSuccess(null,"找不到该玩家的位置"));
    }

    public async Task PlaceBlock(BlockActionData[] data)
    {
        if (ConnectionMap.TryGetValue(Context.ConnectionId, out var info))
        {
            foreach (var block in data)
            {
                block.PlayerId = info.playerId;
            }
            await Clients.GroupExcept(GetWorldGroup(info.worldId), [])
                         .SendAsync("PlaceBlock", data);
        }
    }

    public async Task SendChatMessage(ChatMessageData message)
    {
        if (ConnectionMap.TryGetValue(Context.ConnectionId, out var info))
        {
            await Clients.Group(GetWorldGroup(info.worldId))
                         .SendAsync("ChatMessage", message);
        }
    }
    

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (ConnectionMap.TryRemove(Context.ConnectionId, out var info))
        {
            await Clients.Group(GetWorldGroup(info.worldId))
                         .SendAsync("PlayerLeave", info.playerId);

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetWorldGroup(info.worldId));
        }

        await base.OnDisconnectedAsync(exception);
    }

    private static string GetWorldGroup(int worldId) => $"world:{worldId}";
}
