using Microsoft.EntityFrameworkCore;
using Polly;
using Quartz;
using Serilog;
using System.Net;
using WorkerService1.Contexts;
using WorkerService1.Jobs;

Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File("logs/myapp.txt", rollingInterval: RollingInterval.Day, restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error)
            .CreateLogger();

IHost host = Host.CreateDefaultBuilder(args)
    .UseSerilog()
    .ConfigureServices((HostBuilderContext hostContext, IServiceCollection services) =>
    {
        services.AddDbContextFactory<ERPContext>(options =>
        {
            options.UseSqlServer(hostContext.Configuration.GetConnectionString("DefaultConnectionString"));
            options.EnableSensitiveDataLogging();
        });

        services.AddQuartz(q =>
        {

            q.ScheduleJob<AYJob>(trigger =>
            {
                trigger.WithIdentity("AYTrigger").StartNow().WithSimpleSchedule(x =>
                {
                    x.WithIntervalInHours(3).RepeatForever();
                })
                    .WithDescription("my awesome trigger configured for a job with single call");
            });

            q.ScheduleJob<YBJob>(trigger =>
            {
                trigger.WithIdentity("YBTrigger").StartNow().WithSimpleSchedule(x =>
                {
                    x.WithIntervalInHours(3).RepeatForever();
                })
                    .WithDescription("my awesome trigger configured for a job with single call");
            });

            q.ScheduleJob<HHJob>(trigger =>
            {
                trigger.WithIdentity("HHTrigger").StartNow().WithSimpleSchedule(x =>
                {
                    x.WithIntervalInHours(3).RepeatForever();
                })
                    .WithDescription("my awesome trigger configured for a job with single call");
            });

            q.ScheduleJob<HFJob>(trigger =>
            {
                trigger.WithIdentity("HFTrigger").StartNow().WithSimpleSchedule(x =>
                {
                    x.WithIntervalInHours(3).RepeatForever();
                })
                    .WithDescription("my awesome trigger configured for a job with single call");
            });

            q.ScheduleJob<LQJob>(trigger =>
            {
                trigger.WithIdentity("LQTrigger").StartNow().WithSimpleSchedule(x =>
                {
                    x.WithIntervalInHours(3).RepeatForever();
                })
                    .WithDescription("my awesome trigger configured for a job with single call");
            });
        });

        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
        });

        var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(10));

        var policyRegistry = services.AddPolicyRegistry();

        policyRegistry.Add("Regular", timeoutPolicy);

        services.AddHttpClient("LQ", client =>
        {
            client.BaseAddress = new Uri("http://www.sdlqyy.cn");
        }).AddPolicyHandlerFromRegistry("Regular").ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler()
        {
            AllowAutoRedirect = true,
            AutomaticDecompression = DecompressionMethods.GZip,
            ConnectTimeout = TimeSpan.FromSeconds(600.0)
        });

        services.AddHttpClient("AY", client =>
        {
            client.BaseAddress = new Uri("https://ec.ayyywl.com");
        }).AddPolicyHandlerFromRegistry("Regular").ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler()
        {
            AllowAutoRedirect = true,
            AutomaticDecompression = DecompressionMethods.GZip,
            ConnectTimeout = TimeSpan.FromSeconds(600.0)
        });

        services.AddHttpClient("YB", client =>
        {
            client.BaseAddress = new Uri("https://www.jzybyy.com");
        }).AddPolicyHandlerFromRegistry("Regular").ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler()
        {
            AllowAutoRedirect = true,
            AutomaticDecompression = DecompressionMethods.GZip,
            ConnectTimeout = TimeSpan.FromSeconds(600.0)
        });

        services.AddHttpClient("HF", client =>
        {
            client.BaseAddress = new Uri("http://ds.lyhfyy.com");
        }).AddPolicyHandlerFromRegistry("Regular").ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler()
        {
            AllowAutoRedirect = true,
            AutomaticDecompression = DecompressionMethods.GZip,
            ConnectTimeout = TimeSpan.FromSeconds(600.0)
        });

        services.AddHttpClient("HH", client =>
        {
            client.BaseAddress = new Uri("https://hhey.shaphar.com");
        }).AddPolicyHandlerFromRegistry("Regular").ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler()
        {
            AllowAutoRedirect = true,
            AutomaticDecompression = DecompressionMethods.GZip,
            ConnectTimeout = TimeSpan.FromSeconds(600.0)
        });
    })
    .Build();

host.Run();
