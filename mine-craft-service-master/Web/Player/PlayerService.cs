using Microsoft.EntityFrameworkCore;
using MineCraftService.GameDataBase;
using MineCraftService.GameDataBase.Models;

namespace MineCraftService.Web;

public class PlayerService(GameDb db)
{
    public async Task<Player?> GetPlayer(int playerId)
    {
        return await db.Players
            .FirstOrDefaultAsync(p => p.Id == playerId);
    }

    public async Task<Player?> GetPlayer(int userId, int worldId)
    {
        return await db.Players
            .FirstOrDefaultAsync(p => p.UserId == userId && p.WorldId == worldId);
    }

    public async Task<List<Player>> GetPlayers(int worldId)
    {
        return await db.Players
            .Include(p => p.User) // 加载 User 导航属性
            .Where(p => p.WorldId == worldId)
            .ToListAsync();
    }

    public async Task<bool> HasPlayerInWorld(int userId, int worldId)
    {
        return await db.Players.AnyAsync(p => p.WorldId == worldId && p.UserId == userId);
    }

    public async Task<bool> AddPlayer(Player player)
    {
        var world = await db.Worlds.FindAsync(player.WorldId);
        if (world == null) return false;
        if (world.IsPublic==0 && world.UserId != player.UserId) return false;
        player.CreatedAt = TimeHelper.GetChinaLocalTime();
        db.Players.Add(player);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task DeletePlayers(int worldId)
    {
        await db.Players
            .Where(p => p.WorldId == worldId)
            .ExecuteDeleteAsync();
    }

    public async Task<bool> RemovePlayer(Player player)
    {
        db.Players.Remove(player);
        var affectedRows = await db.SaveChangesAsync();
        return affectedRows > 0;
    }


    public async Task<bool> PlayerBelongsToWorld(int playerId, int worldId)
    {
        var player = await db.Players.FindAsync(playerId);
        return player?.WorldId == worldId;
    }
}