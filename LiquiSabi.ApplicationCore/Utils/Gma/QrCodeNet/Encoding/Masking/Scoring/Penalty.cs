namespace LiquiSabi.ApplicationCore.Utils.Gma.QrCodeNet.Encoding.Masking.Scoring;

public abstract class Penalty
{
	internal abstract int PenaltyCalculate(BitMatrix matrix);
}
