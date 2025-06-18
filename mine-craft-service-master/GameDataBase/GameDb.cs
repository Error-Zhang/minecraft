using Microsoft.EntityFrameworkCore;
using MineCraftService.GameDataBase.Models;

namespace MineCraftService.GameDataBase;

public class GameDb(DbContextOptions<GameDb> options) : DbContext(options)
{
    /** 1.确保已安装如下包
     *  dotnet tool install --global dotnet-ef
     *  dotnet add package Microsoft.EntityFrameworkCore.Sqlite
     *  dotnet add package Microsoft.EntityFrameworkCore.Design
     */
    /** 2.按顺序运行如下命令
     *  删除数据库(选择性执行): dotnet ef database drop
     *  添加初始迁移: dotnet ef migrations add InitialCreate
     *  应用迁移，创建表结构: dotnet ef database update
     *  生成新的迁移(如果有爆红需要修改描述并重新执行)：dotnet ef migrations add (描述)
     */
    /**
     * 3.如果直接使用Model作为DTO和VO需要特别小心，非常容易发生循环引用的问题，注意ide的提示
     */
    public DbSet<World> Worlds => Set<World>();

    public DbSet<Player> Players => Set<Player>();
    public DbSet<User> Users => Set<User>();
    public DbSet<DirtyChunk> DirtyChunks => Set<DirtyChunk>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 存储枚举
        modelBuilder.Entity<World>()
            .Property(w => w.GameMode)
            .HasConversion<int>();

        modelBuilder.Entity<World>()
            .Property(w => w.WorldMode)
            .HasConversion<int>();

        modelBuilder.Entity<World>()
            .Property(w => w.Season)
            .HasConversion<int>();

        modelBuilder.Entity<DirtyChunk>()
            .HasKey(c => new { c.WorldId, c.ChunkX, c.ChunkZ }); // 复合主键
    }
}