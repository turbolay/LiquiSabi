using LiquiSabi.ApplicationCore.Utils.Affiliation.Models;

namespace LiquiSabi.ApplicationCore.Utils.WabiSabi.Models;

public record RoundStateResponse(RoundState[] RoundStates, CoinJoinFeeRateMedian[] CoinJoinFeeRateMedians, AffiliateInformation AffiliateInformation);
