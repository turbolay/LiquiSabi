using NBitcoin;

namespace LiquiSabi.ApplicationCore.Utils.WabiSabi.Backend.Banning;

public record CoinVerifyResult(Coin Coin, bool ShouldBan, bool ShouldRemove);
