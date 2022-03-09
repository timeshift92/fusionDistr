using System.Data;
using System.Reflection;
using Abstractions;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.OpenApi.Models;
using p1.Web;
using Services;
using Stl.Fusion;
using Stl.Fusion.Authentication;
using Stl.Fusion.Bridge;
using Stl.Fusion.Client;
using Stl.Fusion.EntityFramework;
using Stl.Fusion.Extensions;
using Stl.Fusion.Operations.Reprocessing;
using Stl.Fusion.Server;
using Stl.Fusion.UI;
using Stl.IO;

namespace p1.Api;

public class Startup
{
    private IConfiguration Cfg { get; }
    private IWebHostEnvironment Env { get; }
    private ILogger Log { get; set; } = NullLogger<Startup>.Instance;

    public Startup(IConfiguration cfg, IWebHostEnvironment environment)
    {
        Cfg = cfg;
        Env = environment;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        // Logging
        services.AddLogging(logging => {
            logging.ClearProviders();
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Information);
            if (Env.IsDevelopment()) {
                logging.AddFilter(typeof(App).Namespace, LogLevel.Information);
                logging.AddFilter("Microsoft", LogLevel.Warning);
                logging.AddFilter("Microsoft.AspNetCore.Hosting", LogLevel.Information);
                logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Information);
                logging.AddFilter("Stl.Fusion.Operations", LogLevel.Information);
            }
        });

  

        // DbContext & related services
        var appTempDir = FilePath.GetApplicationTempDirectory("", true);
        var dbPath = appTempDir & "App.db";
        services.AddDbContextFactory<AppDbContext>(dbContext => {
            
                dbContext.UseSqlite($"Data Source={dbPath}");
            if (Env.IsDevelopment())
                dbContext.EnableSensitiveDataLogging();
        });
        services.AddTransient(c => new DbOperationScope<AppDbContext>(c) {
            IsolationLevel = IsolationLevel.Serializable,
        });
        services.AddDbContextServices<AppDbContext>(dbContext => {
            // This is the best way to add DbContext-related services from Stl.Fusion.EntityFramework
            dbContext.AddOperations((_, o) => {
                // We use FileBasedDbOperationLogChangeMonitor, so unconditional wake up period
                // can be arbitrary long - all depends on the reliability of Notifier-Monitor chain.
                o.UnconditionalWakeUpPeriod = TimeSpan.FromSeconds(Env.IsDevelopment() ? 60 : 5);
            });
            var operationLogChangeAlertPath = dbPath + "_changed";
            dbContext.AddFileBasedOperationLogChangeTracking(operationLogChangeAlertPath);
            
            
        });


        // Web
        services.AddCors(cors => cors.AddDefaultPolicy(
            policy => policy
                .WithOrigins("http://localhost:5005", "http://localhost:6000","http://localhost:5000")
                .WithFusionHeaders()
        ));
        services.Configure<ForwardedHeadersOptions>(options => {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });

        // Fusion services
        services.AddSingleton(new Publisher.Options() { Id = "p" });
        var fusion = services.AddFusion();
        var fusionServer = fusion.AddWebServer();
        var fusionClient = fusion.AddRestEaseClient();
       
        fusion.AddOperationReprocessor();
        // You don't need to manually add TransientFailureDetector -
        // it's here only to show that operation reprocessor works
        // when TodoService.AddOrUpdate throws this exception.
        // Database-related transient errors are auto-detected by
        // DbOperationScopeProvider<TDbContext> (it uses DbContext's
        // IExecutionStrategy to do this).
        services.TryAddEnumerable(ServiceDescriptor.Singleton(
            TransientFailureDetector.New(e => e is DbUpdateConcurrencyException)));

        // Compute service(s)
        fusion.AddComputeService<IUserService, UserService>();

        fusion.AddFusionTime();
        fusion.AddBackendStatus<CustomBackendStatus>();
// We don't care about Sessions in this sample, but IBackendStatus
// service assumes it's there, so let's register a fake one
        services.AddSingleton(new SessionFactory().CreateSession());

        // Default update delay is 0.5s
        services.AddTransient<IUpdateDelayer>(c => new UpdateDelayer(c.UICommandTracker(), 0.5));

       

        // Web
        services.AddRouting();
        services.AddMvc().AddApplicationPart(Assembly.GetExecutingAssembly());

        // Swagger & debug tools
        services.AddSwaggerGen(c => {
            c.SwaggerDoc("v1", new OpenApiInfo {
                Title = "Templates.TodoApp API", Version = "v1"
            });
        });
    }

    public void Configure(IApplicationBuilder app, ILogger<Startup> log)
    {
        Log = log;

       

        if (Env.IsDevelopment()) {
            app.UseDeveloperExceptionPage();
        }
        else {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }
        //app.UseHttpsRedirection();

        app.UseForwardedHeaders(new ForwardedHeadersOptions {
            ForwardedHeaders = ForwardedHeaders.XForwardedProto
        });

        app.UseWebSockets(new WebSocketOptions() {
            KeepAliveInterval = TimeSpan.FromSeconds(30),
        });

        // Static + Swagger
        app.UseStaticFiles();
        app.UseSwagger();
        app.UseSwaggerUI(c => {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
        });

        // API controllers
        app.UseRouting();
        app.UseCors();
        app.UseEndpoints(endpoints => {
            endpoints.MapFusionWebSocketServer();
            endpoints.MapControllers();
        });
    }
}