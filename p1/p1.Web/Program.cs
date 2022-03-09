using Abstractions;
using Abstractions.Client;
using Blazor.Extensions.Logging;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Stl.Fusion.Client;
using Stl.Fusion.Blazor;
using Stl.Fusion.Extensions;
using Stl.Fusion.UI;
using Microsoft.Extensions.Http;
using p1.Web;
using Services;
using Stl.Fusion;
using Stl.Fusion.Authentication;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
Random random = new Random();
string RandomString(int length)
{
    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    return new string(Enumerable.Repeat(chars, length)
        .Select(s => s[random.Next(s.Length)]).ToArray());
}


//Write("Enter SessionId to use: ");
//var sessionId = ReadLine()!.Trim();
var session = new Session(RandomString(16));


var env = builder.HostEnvironment;

builder.Services.AddLogging(logging => {
    // logging.ClearProviders();

    logging.AddBrowserConsole();
    logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
    logging.SetMinimumLevel(LogLevel.Information);
    if (env.IsDevelopment()) {
        logging.AddFilter("Microsoft", LogLevel.Warning);
        logging.AddFilter("Microsoft.AspNetCore.Hosting", LogLevel.Information);
        logging.AddFilter("Stl.Fusion.Operations", LogLevel.Information);
    }
});


builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5000") });


var baseUri = new Uri("http://localhost:5000");
var apiBaseUri = new Uri($"{baseUri}api/");

builder.Services.ConfigureAll<HttpClientFactoryOptions>(options => {
    // Replica Services construct HttpClients using IHttpClientFactory, so this is
    // the right way to make all HttpClients to have BaseAddress = apiBaseUri by default.
    options.HttpClientActions.Add(client => client.BaseAddress = apiBaseUri);
});
var fusion = builder.Services.AddFusion();


var fusionClient = fusion.AddRestEaseClient(
    (c, o) => {
        o.BaseUri = baseUri;
    }).ConfigureHttpClientFactory(
    (c, name, o) => {
        var isFusionClient = (name ?? "").StartsWith("Stl.Fusion");
        var clientBaseUri = isFusionClient ? baseUri : apiBaseUri;
        o.HttpClientActions.Add(client => client.BaseAddress = clientBaseUri);

    }).ConfigureWebSocketChannel((c, o) => {
        o.BaseUri = baseUri;
        o.IsLoggingEnabled = true;

    });
fusionClient.AddReplicaService<IUserService, IUserClientRef>();


// Default update delay is 0.1s

fusion.AddBlazorUIServices();
fusion.AddFusionTime();
fusion.AddBackendStatus<CustomBackendStatus>();
// We don't care about Sessions in this sample, but IBackendStatus
// service assumes it's there, so let's register a fake one
builder.Services.AddSingleton(new SessionFactory().CreateSession());
builder.Services.AddTransient<IUpdateDelayer>(c => new UpdateDelayer(c.UICommandTracker(), 1));

 await builder.Build().RunAsync();