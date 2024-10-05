using LiquiSabi.ApplicationCore.Utils.WabiSabi.Models;

namespace LiquiSabi.ApplicationCore.Utils.WabiSabi.Client.CoinJoinProgressEvents;

public class RoundEnded : CoinJoinProgressEventArgs
{
	public RoundEnded(RoundState lastRoundState)
	{
		LastRoundState = lastRoundState;
	}

	public RoundState LastRoundState { get; }
	public bool IsStopped { get; set; }
}
