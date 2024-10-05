using LiquiSabi.ApplicationCore.Utils.WabiSabi.Models;

namespace LiquiSabi.ApplicationCore.Utils.WabiSabi.Client.CoinJoinProgressEvents;

public class EnteringOutputRegistrationPhase : RoundStateChanged
{
	public EnteringOutputRegistrationPhase(RoundState roundState, DateTimeOffset timeoutAt) : base(roundState, timeoutAt)
	{
	}
}
