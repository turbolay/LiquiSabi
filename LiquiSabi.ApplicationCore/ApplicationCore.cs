using LiquiSabi.ApplicationCore.Data;
using LiquiSabi.ApplicationCore.Interfaces;
using LiquiSabi.ApplicationCore.Publishing.Nostr;
using LiquiSabi.ApplicationCore.Utils.Config;
using LiquiSabi.ApplicationCore.Utils.Helpers;
using LiquiSabi.ApplicationCore.Utils.Logging;
using LiquiSabi.ApplicationCore.Utils.Tor;

namespace LiquiSabi.ApplicationCore;

public class ApplicationCore
{
    private readonly Scraper _roundStatusScraper;
    private readonly IRoundDataReaderService _dataProcessor;
    private readonly IRpcServerController _rpcServerController;
    private readonly CoordinatorDiscovery _coordinatorDiscovery;
    private readonly IAnalyzer _analyzer;

    public ApplicationCore(Scraper roundStatusScraper, 
        IRoundDataReaderService dataProcessor,
        IRpcServerController rpcServerController, 
        CoordinatorDiscovery coordinatorDiscovery, 
        IAnalyzer analyzer)
    {
        _roundStatusScraper = roundStatusScraper;
        _dataProcessor = dataProcessor;
        _rpcServerController = rpcServerController;
        _coordinatorDiscovery = coordinatorDiscovery;
        _analyzer = analyzer;
    }

    public async Task Run(CancellationToken cancellationToken)
    {
        await (new TorProcessManager(new TorSettings(Config.DataDir,  Path.Combine(EnvironmentHelpers.GetFullBaseDirectory(), "TorDaemons"), terminateOnExit: true))).StartAsync(cancellationToken).ConfigureAwait(false);
        
        DeploymentConfiguration.LoadConfiguration();
        await _coordinatorDiscovery.StartAsync(cancellationToken);
        await _roundStatusScraper.StartAsync(cancellationToken);
        await _roundStatusScraper.TriggerAndWaitRoundAsync(cancellationToken);
        await _roundStatusScraper.ToBeProcessedData.Reader.WaitToReadAsync(cancellationToken);
        await _dataProcessor.StartAsync(cancellationToken);
        await _rpcServerController.StartRpcServerAsync(cancellationToken);
        await _analyzer.StartAsync(cancellationToken);
        Logger.LogInfo("Initialized.");
        await Task.Delay(-1, cancellationToken);
    }
}