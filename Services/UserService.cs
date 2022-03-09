using System.Reactive;
using Abstractions;
using Abstractions.Models;
using Stl.Fusion;
using Stl.Fusion.EntityFramework;
using Stl.Async;

namespace Services;

public class UserService : DbServiceBase<AppDbContext>, IUserService
{
    public UserService(IServiceProvider services) : base(services)
    {
    }
    public virtual async Task<UserData> AddOrUpdate(AddOrUpdateUserCommand command, CancellationToken cancellationToken = default)
    {
        if (Computed.IsInvalidating())
        {
            _ = List(cancellationToken);
            return default!;
        }

        await using var dbContext = await CreateCommandDbContext(cancellationToken);


        dbContext.UsersData.Add(command.Item);
        await dbContext.SaveChangesAsync(cancellationToken);
        return command.Item;

    }

    public virtual async Task<UserData?> Get(int id, CancellationToken cancellationToken = default)
    {
        await using var dbContext = CreateDbContext();
        return dbContext.UsersData.FirstOrDefault(u => u.Id == id);
    }

    public virtual async Task<UserData[]> List(CancellationToken cancellationToken = default)
    {
        await using var dbContext = CreateDbContext();
        return dbContext.UsersData.ToArray();
    }

    [ComputeMethod]
    protected virtual Task<Unit> PseudoGetAny() => TaskExt.UnitTask;

}