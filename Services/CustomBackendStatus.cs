using System.Reactive;
using Abstractions;
using Microsoft.Extensions.Logging;
using Stl.Fusion;
using Stl.Fusion.Authentication;
using Stl.Fusion.Extensions;

namespace Services;
public class CustomBackendStatus : BackendStatus
{
    private readonly IUserService _userService;
    private readonly ILogger _log;

    public CustomBackendStatus(IUserService userService, ILogger<CustomBackendStatus> log)
        : base(null!)
    {
        _log = log;
        _userService = userService;
    }

    [ComputeMethod]
    protected override async Task<Unit> HitBackend(
        Session session,
        string backend,
        CancellationToken cancellationToken = default)
    {
        await _userService.List(cancellationToken);
        return default;
    }
}