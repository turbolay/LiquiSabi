using NBitcoin;
using WabiSabi.Crypto.Randomness;

namespace LiquiSabi.ApplicationCore.Utils.WabiSabi.Backend.Rounds;

public class BlameRound : Round
{
	public BlameRound(RoundParameters parameters, Round blameOf, ISet<OutPoint> blameWhitelist, WasabiRandom random)
		: base(parameters, random)
	{
		BlameOf = blameOf;
		BlameWhitelist = blameWhitelist;
		InputRegistrationTimeFrame = TimeFrame.Create(Parameters.BlameInputRegistrationTimeout).StartNow();
	}

	public Round BlameOf { get; }
	public ISet<OutPoint> BlameWhitelist { get; }

	public override bool IsInputRegistrationEnded(int maxInputCount)
	{
		return base.IsInputRegistrationEnded(BlameWhitelist.Count);
	}
}
