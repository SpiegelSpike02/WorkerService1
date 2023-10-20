using Microsoft.EntityFrameworkCore;
using Polly;
using Quartz;
using System.Net;
using WorkerService1.Contexts;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((HostBuilderContext hostContext, IServiceCollection services) =>
    {
        services.AddDbContextFactory<ERPContext>(options =>
        {
            options.UseSqlServer(hostContext.Configuration.GetConnectionString("DefaultConnectionString"));
            options.EnableSensitiveDataLogging();
        });

        services.AddQuartz(q =>
        {

        });

        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
        });

        SocketsHttpHandler defaultHandler = new()
        {
            AllowAutoRedirect = true,
            AutomaticDecompression = DecompressionMethods.GZip,
            ConnectTimeout = TimeSpan.FromSeconds(600.0)
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
    })
    .Build();

host.Run();
