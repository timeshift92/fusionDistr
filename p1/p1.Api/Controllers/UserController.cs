using Abstractions;
using Abstractions.Models;
using Microsoft.AspNetCore.Mvc;
using Stl.Fusion.Server;

namespace p1.Api.Controllers;

[Route("api/[controller]/[action]")]
[ApiController, JsonifyErrors]
public class UserController : ControllerBase, IUserService
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }
    [HttpPost, Publish]
    public async Task<UserData> AddOrUpdate(AddOrUpdateUserCommand command, CancellationToken cancellationToken = default)
    {
        return await _userService.AddOrUpdate(command, cancellationToken);
    }

    [HttpGet, Publish]
    public async Task<UserData?> Get(int id, CancellationToken cancellationToken = default)
    {
        return await _userService.Get(id, cancellationToken);
    }

    [HttpGet, Publish]
    public async Task<UserData[]> List(CancellationToken cancellationToken = default)
    {
        return await _userService.List(cancellationToken);
    }
}
