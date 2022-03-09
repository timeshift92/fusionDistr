using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.Memory;
using p1.Api;
using Services;
using Stl.Fusion.EntityFramework.Authentication;
using Stl.Fusion.EntityFramework.Extensions;
using Stl.Fusion.EntityFramework.Operations;

var host = Host.CreateDefaultBuilder()
    .ConfigureHostConfiguration(cfg => {
        // Looks like there is no better way to set _default_ URL
        cfg.Sources.Insert(0, new MemoryConfigurationSource() {
            InitialData = new Dictionary<string, string>() {
                {WebHostDefaults.ServerUrlsKey, "http://localhost:5000;http://localhost:5005;http://localhost:6000"},
            }
        });
    })
    .ConfigureWebHostDefaults(webHost => webHost
        .UseDefaultServiceProvider((ctx, options) => {
            if (ctx.HostingEnvironment.IsDevelopment()) {
                options.ValidateScopes = true;
                options.ValidateOnBuild = true;
            }
        })
        .UseStartup<Startup>())
    .Build();

// Ensure the DB is created
var dbContextFactory = host.Services.GetRequiredService<IDbContextFactory<AppDbContext>>();
await using var dbContext = dbContextFactory.CreateDbContext();
//await dbContext.Database.EnsureDeletedAsync();
await dbContext.Database.EnsureCreatedAsync();

await host.RunAsync();