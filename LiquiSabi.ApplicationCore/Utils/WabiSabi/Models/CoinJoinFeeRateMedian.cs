using NBitcoin;

namespace LiquiSabi.ApplicationCore.Utils.WabiSabi.Models;

public record CoinJoinFeeRateMedian(TimeSpan TimeFrame, FeeRate MedianFeeRate);
