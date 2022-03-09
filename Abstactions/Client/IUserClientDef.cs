using Abstractions.Models;
using RestEase;
using Stl.Fusion;
using Stl.Fusion.Extensions;

namespace Abstractions.Client;

[BasePath("User")]
public interface IUserClientRef
{
    [Post("AddOrUpdate")]
    Task<UserData> AddOrUpdate([Body] AddOrUpdateUserCommand command, CancellationToken cancellationToken = default);

    [Get("Get")]
    Task<UserData?> Get(int id, CancellationToken cancellationToken = default);
    [Get("List")]
    Task<UserData[]> List(CancellationToken cancellationToken = default);

}