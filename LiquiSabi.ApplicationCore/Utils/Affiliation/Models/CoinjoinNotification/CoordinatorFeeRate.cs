namespace LiquiSabi.ApplicationCore.Utils.Affiliation.Models.CoinjoinNotification;

public record CoordinatorFeeRate(decimal FeeRate)
{
	public static implicit operator CoordinatorFeeRate(decimal feeRate) => new(feeRate);
}
