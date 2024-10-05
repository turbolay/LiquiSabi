using NBitcoin;
using LiquiSabi.ApplicationCore.Utils.WabiSabi.Backend.Rounds;

namespace LiquiSabi.ApplicationCore.Utils.WabiSabi.Backend.Events;

public class RoundPhaseChangedEventArgs : EventArgs
{
	public RoundPhaseChangedEventArgs(uint256 roundId, Phase phase) : base()
	{
		RoundId = roundId;
		Phase = phase;
	}

	public uint256 RoundId { get; }
	public Phase Phase { get; }
}
