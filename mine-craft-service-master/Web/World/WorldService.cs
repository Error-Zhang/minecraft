using Microsoft.EntityFrameworkCore;
using MineCraftService.GameDataBase.Models;

namespace MineCraftService.Web;

using GameDataBase;

public class WorldService(GameDb db,PlayerService playerService,ChunkService chunkService)
{
    
    public async Task<List<WorldDto>> GetWorlds(int userId)
    {
        var worlds = await db.Worlds
            .Where(w => w.UserId == userId || w.IsPublic != 0)
            .Include(w => w.User)
            .ToListAsync();

        var worldIds = worlds.Select(w => w.Id).ToList();

        var players = await db.Players
            .Where(p => worldIds.Contains(p.WorldId))
            .Include(p => p.User)
            .ToListAsync();

        // 按 WorldId 进行分组
        var playersByWorld = players
            .GroupBy(p => p.WorldId)
            .ToDictionary(g => g.Key, g => g.ToList());

        return worlds.Select((w) => new WorldDto
        {
            Id = w.Id,
            WorldName = w.WorldName,
            Seed = w.Seed,
            GameMode = w.GameMode,
            WorldMode = w.WorldMode,
            Season = w.Season,
            IsPublic = w.IsPublic,
            CreatedAt = w.CreatedAt,
            User = w.User,
            Players = playersByWorld.TryGetValue(w.Id, out var list) ? list : new List<Player>() // 正确分配
        }).ToList();
    }


    
    public async Task<World?> GetWorld(int worldId)
    {
        return await db.Worlds.FindAsync(worldId);
    }

    public async Task<World> CreateWorld(World world)
    {
        world.CreatedAt = TimeHelper.GetChinaLocalTime();
        db.Worlds.Add(world);
        await db.SaveChangesAsync();
        return world;
    }
    
    public async Task<bool> UpdateWorld(World updatedWorld)
    {
        var existing = await GetWorld(updatedWorld.Id);
        if (existing == null) return false;
        if(existing.UserId != updatedWorld.UserId) return false;
        existing.WorldName = updatedWorld.WorldName;
        existing.GameMode = updatedWorld.GameMode;
        existing.IsPublic = updatedWorld.IsPublic;
        db.Worlds.Update(existing);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteWorld(int worldId, int userId)
    {
        var transaction = await db.Database.BeginTransactionAsync();

        try
        {
            var world = await db.Worlds.FirstOrDefaultAsync(w => w.Id == worldId && w.UserId == userId);
            if (world is null)
                return false;
            
            db.Worlds.Remove(world);
            await db.SaveChangesAsync(); // 世界删除要先提交（如有外键依赖）

            await playerService.DeletePlayers(worldId);        // 内部使用 ExecuteDeleteAsync（无需另 SaveChanges）
            await chunkService.DeleteDirtyChunks(worldId);     // 同上
            await transaction.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Console.WriteLine($"删除世界失败: {ex.Message}");
            return false;
        }
    }


}