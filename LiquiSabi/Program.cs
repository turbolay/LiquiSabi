using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using LiquiSabi.ApplicationCore.Data;
using LiquiSabi.ApplicationCore.Interfaces;
using LiquiSabi.ApplicationCore.Publishing.Nostr;
using LiquiSabi.ApplicationCore.Rpc;
using LiquiSabi.ApplicationCore.Utils.Config;
using LiquiSabi.ApplicationCore.Utils.Helpers;
using LiquiSabi.ApplicationCore.Utils.Logging;
using LiquiSabi.ApplicationCore.Utils.Rpc;
using LiquiSabi.ApplicationCore.Utils.Services.Terminate;
using LiquiSabi.ApplicationCore.Utils.Tor.Http;
// ReSharper disable InconsistentlySynchronizedField

namespace LiquiSabi;

public static class Program
{
    private static object LockClosure { get; } = new();
    private static readonly CancellationTokenSource CancellationTokenSource = new();

    public static async Task Main(string[] args)
    {
        Logger.InitializeDefaults(Path.Combine(EnvironmentHelpers.GetDataDir(Path.Combine("LiquiSabi", "Client")), "Logs.txt"));
        HandleClosure();

        var host = CreateHostBuilder(args).Build();
        var applicationCore = host.Services.GetRequiredService<ApplicationCore.ApplicationCore>();

        try
        {
            await applicationCore.Run(CancellationTokenSource.Token);
        }
        catch (Exception ex)
        {
            if (!CancellationTokenSource.Token.IsCancellationRequested)
            {
                Logger.LogCritical(ex);
            }
        }
        finally
        {
            await TerminateApplicationAsync(host.Services.GetRequiredService<JsonRpcServer>());
        }
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((_, services) => ConfigureServices(services));

    public static void ConfigureServices(IServiceCollection services)
    {
        var config = new Config(LoadOrCreateConfigs(), Array.Empty<string>());
        var jsonRpcServerConfig = new JsonRpcServerConfiguration(true, config.JsonRpcUser, config.JsonRpcPassword, config.JsonRpcServerPrefixes);
        services
            .AddSingleton<IAnalyzer, Analyzer>()
            .AddSingleton<CoordinatorDiscovery>()
            .AddSingleton<Scraper>(sp =>
            {
                var coordinatorDiscovery = sp.GetRequiredService<CoordinatorDiscovery>();
                return new Scraper(coordinatorDiscovery);
            })
            .AddSingleton<PersistentConfig>()
            .AddSingleton<IRoundDataReaderService, RoundDataReaderService>(sp =>
            {
                return new RoundDataReaderService(sp.GetRequiredService<Scraper>());
            })
            .AddSingleton(config)
            .AddSingleton(jsonRpcServerConfig)
            .AddSingleton<JsonRpcServer>((sp) =>
            {
                var analyzer = sp.GetRequiredService<IAnalyzer>();

                return new JsonRpcServer(
                    new LiquiSabiRpc(),
                    jsonRpcServerConfig,
                    new TerminateService(() =>
                            TerminateApplicationAsync(sp.GetRequiredService<JsonRpcServer>()), () => { })
                );
            })
            .AddSingleton<IRpcServerController, RpcServerController>(sp =>
            {
                var jsonRpcServer = sp.GetRequiredService<JsonRpcServer>();
                var jsonRpcServerConfiguration = sp.GetRequiredService<JsonRpcServerConfiguration>();

                return new RpcServerController(jsonRpcServer, jsonRpcServerConfiguration);
            })
            .AddSingleton<ApplicationCore.ApplicationCore>();

        services.RemoveAll<IHttpMessageHandlerBuilderFilter>();
    }

    private static Task TerminateApplicationAsync(JsonRpcServer jsonRpcServer)
    {
        Logger.LogInfo("Closing.");
        return Task.CompletedTask;
    }

    private static void HandleClosure()
    {
        AppDomain.CurrentDomain.ProcessExit += (_, _) =>
        {
            lock (LockClosure)
            {
                if (!CancellationTokenSource.IsCancellationRequested)
                {
                    CancellationTokenSource.Cancel();
                }
            }
        };

        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true; // Prevent the default Ctrl+C behavior
            lock (LockClosure)
            {
                if (!CancellationTokenSource.IsCancellationRequested)
                {
                    CancellationTokenSource.Cancel();
                }
            }
        };
    }

    private static PersistentConfig LoadOrCreateConfigs()
    {
        Directory.CreateDirectory(Config.DataDir);

        PersistentConfig persistentConfig = new(Path.Combine(Config.DataDir, "Config.json"));
        persistentConfig.LoadFile(createIfMissing: true);

        return persistentConfig;
    }
}