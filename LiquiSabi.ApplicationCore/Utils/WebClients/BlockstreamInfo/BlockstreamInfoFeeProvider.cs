using LiquiSabi.ApplicationCore.Utils.Bases;
using LiquiSabi.ApplicationCore.Utils.Blockchain.Analysis.FeesEstimation;

namespace LiquiSabi.ApplicationCore.Utils.WebClients.BlockstreamInfo;

public class BlockstreamInfoFeeProvider : PeriodicRunner, IThirdPartyFeeProvider
{
	public BlockstreamInfoFeeProvider(TimeSpan period, BlockstreamInfoClient blockstreamInfoClient) : base(period)
	{
		BlockstreamInfoClient = blockstreamInfoClient;
	}

	public event EventHandler<AllFeeEstimate>? AllFeeEstimateArrived;

	public BlockstreamInfoClient BlockstreamInfoClient { get; set; }
	public AllFeeEstimate? LastAllFeeEstimate { get; private set; }
	public bool InError { get; private set; } = false;
	public bool IsPaused { get; set; } = false;

	protected override async Task ActionAsync(CancellationToken cancel)
	{
		if (IsPaused)
		{
			return;
		}
		try
		{
			var allFeeEstimate = await BlockstreamInfoClient.GetFeeEstimatesAsync(cancel).ConfigureAwait(false);
			LastAllFeeEstimate = allFeeEstimate;

			if (allFeeEstimate.Estimations.Any())
			{
				AllFeeEstimateArrived?.Invoke(this, allFeeEstimate);
			}

			InError = false;
		}
		catch
		{
			InError = true;
			throw;
		}
	}
}
