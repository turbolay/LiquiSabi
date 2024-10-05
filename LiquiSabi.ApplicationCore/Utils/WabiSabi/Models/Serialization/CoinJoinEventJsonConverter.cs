using LiquiSabi.ApplicationCore.Utils.WabiSabi.Models.MultipartyTransaction;

namespace LiquiSabi.ApplicationCore.Utils.WabiSabi.Models.Serialization;

public class CoinJoinEventJsonConverter : GenericInterfaceJsonConverter<IEvent>
{
	public CoinJoinEventJsonConverter() : base(new[] { typeof(InputAdded), typeof(OutputAdded), typeof(RoundCreated) })
	{
	}
}
