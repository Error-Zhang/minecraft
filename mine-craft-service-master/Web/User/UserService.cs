using Microsoft.EntityFrameworkCore;
using MineCraftService.GameDataBase.Models;

namespace MineCraftService.Web;

using GameDataBase;

public class UserService(GameDb db)
{
    public async Task<User?> GetUser(int userId)
    {
        return await db.Users.FindAsync(userId);
    }

    public async Task<User?> GetUserId(string username, string? password)
    {
        return await db.Users.FirstOrDefaultAsync(u =>
            u.UserName == username && (password == null || u.PassWord == password));
    }

    public async Task<bool> CreateUser(User user)
    {
        var id = await GetUserId(user.UserName, null);
        if (id != null) return false;
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return true; // EF Core 保存后会自动填充自增主键
    }
}
