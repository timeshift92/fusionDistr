using Abstractions.Models;
using Stl.CommandR;
using Stl.CommandR.Configuration;
using Stl.Fusion;
using Stl.Fusion.Extensions;

namespace Abstractions;

public record AddOrUpdateUserCommand(UserData Item) : ICommand<UserData>
{
    public AddOrUpdateUserCommand() : this(default(UserData)!) { }
}

public interface IUserService
{
    [CommandHandler]
    Task<UserData> AddOrUpdate(AddOrUpdateUserCommand command, CancellationToken cancellationToken = default);

    // Queries
    [ComputeMethod]
    Task<UserData?> Get(int id, CancellationToken cancellationToken = default);
    [ComputeMethod]
    Task<UserData[]> List(CancellationToken cancellationToken = default);

}