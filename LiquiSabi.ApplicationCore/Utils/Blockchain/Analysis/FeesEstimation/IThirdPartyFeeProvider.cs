namespace LiquiSabi.ApplicationCore.Utils.Blockchain.Analysis.FeesEstimation;

public interface IThirdPartyFeeProvider
{
	event EventHandler<AllFeeEstimate>? AllFeeEstimateArrived;

	AllFeeEstimate? LastAllFeeEstimate { get; }
	bool InError { get; }
}
