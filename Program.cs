using Microsoft.EntityFrameworkCore;
using WorkerService1.Utilities;
using Polly;
using Quartz;
using System.Net;
using WorkerService1.Contexts;
using WorkerService1.Jobs;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((HostBuilderContext hostContext, IServiceCollection services) =>
    {
        services.AddDbContextFactory<ERPContext>(options =>
        {
            options.UseMySQL(hostContext.Configuration.GetConnectionString("DefaultConnectionString")!);
            options.EnableSensitiveDataLogging();
        });

        SocketsHttpHandler defaultHandler = new()
        {
            AllowAutoRedirect = true,
            AutomaticDecompression = DecompressionMethods.GZip,
            ConnectTimeout = TimeSpan.FromSeconds(600.0),
            CookieContainer = HHCookieService.GetHHCookie()
        };

        var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(10));

        var policyRegistry = services.AddPolicyRegistry();

        policyRegistry.Add("Regular", timeoutPolicy);

        services.AddHttpClient("LQ", client =>
        {
            client.BaseAddress = new Uri("http://www.sdlqyy.cn");
        }).AddPolicyHandlerFromRegistry("Regular").ConfigurePrimaryHttpMessageHandler(() => defaultHandler);

        services.AddHttpClient("AY", client =>
        {
            client.BaseAddress = new Uri("https://ec.ayyywl.com");
        }).AddPolicyHandlerFromRegistry("Regular").ConfigurePrimaryHttpMessageHandler(() => defaultHandler);

        services.AddHttpClient("YB", client =>
        {
            client.BaseAddress = new Uri("https://www.jzybyy.com");
        }).AddPolicyHandlerFromRegistry("Regular").ConfigurePrimaryHttpMessageHandler(() => defaultHandler);

        services.AddHttpClient("HF", client =>
        {
            client.BaseAddress = new Uri("http://ds.lyhfyy.com");
        }).AddPolicyHandlerFromRegistry("Regular").ConfigurePrimaryHttpMessageHandler(() => defaultHandler);

        services.AddHttpClient("HH", client =>
        {
            client.BaseAddress = new Uri("https://hhey.shaphar.com");
        }).AddPolicyHandlerFromRegistry("Regular").ConfigurePrimaryHttpMessageHandler(() => defaultHandler);

        services.AddQuartz(q =>
        {
            q.ScheduleJob<HHJob>(trigger =>
            {
                trigger.WithIdentity("AYTrigger").StartNow().WithSimpleSchedule(x =>
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
    })
    .Build();

host.Run();
