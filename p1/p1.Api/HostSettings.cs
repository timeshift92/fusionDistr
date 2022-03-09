namespace p1.Api;

public abstract class HostSettings
{
    public bool UseInMemoryAuthService { get; set; } = false;
    public string PublisherId { get; set; } = "p";
}