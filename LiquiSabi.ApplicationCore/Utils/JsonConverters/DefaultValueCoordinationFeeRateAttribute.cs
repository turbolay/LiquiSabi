using System.ComponentModel;
using NBitcoin;
using LiquiSabi.ApplicationCore.Utils.WabiSabi.Models;

namespace LiquiSabi.ApplicationCore.Utils.JsonConverters;

public class DefaultValueCoordinationFeeRateAttribute : DefaultValueAttribute
{
	public DefaultValueCoordinationFeeRateAttribute(double feeRate, double plebsDontPayThreshold)
		: base(new CoordinationFeeRate((decimal)feeRate, Money.Coins((decimal)plebsDontPayThreshold)))
	{
	}
}
