using LiquiSabi.ApplicationCore.Utils.WabiSabi.Models;

namespace LiquiSabi.ApplicationCore.Utils.WabiSabi.Client.CoinJoinProgressEvents;

public class EnteringSigningPhase : RoundStateChanged
{
	public EnteringSigningPhase(RoundState roundState, DateTimeOffset timeoutAt) : base(roundState, timeoutAt)
	{
	}
}
