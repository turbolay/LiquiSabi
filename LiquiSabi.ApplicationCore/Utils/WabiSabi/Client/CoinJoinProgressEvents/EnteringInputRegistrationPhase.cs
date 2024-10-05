using LiquiSabi.ApplicationCore.Utils.WabiSabi.Models;

namespace LiquiSabi.ApplicationCore.Utils.WabiSabi.Client.CoinJoinProgressEvents;

public class EnteringInputRegistrationPhase : RoundStateChanged
{
	public EnteringInputRegistrationPhase(RoundState roundState, DateTimeOffset timeoutAt) : base(roundState, timeoutAt)
	{
	}
}
