using Abstractions.Models;
using Microsoft.EntityFrameworkCore;
using Stl.Fusion.EntityFramework;
using Stl.Fusion.EntityFramework.Operations;

namespace Services;

public class AppDbContext : DbContextBase
{
    public DbSet<UserData> UsersData { get; protected set; } = null!;
    public DbSet<DbOperation> Operations { get; protected set; } = null!;
    public AppDbContext(DbContextOptions options) : base(options) { }
}