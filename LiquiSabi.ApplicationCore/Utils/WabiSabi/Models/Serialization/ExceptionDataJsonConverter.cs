using LiquiSabi.ApplicationCore.Utils.WabiSabi.Backend.Models;

namespace LiquiSabi.ApplicationCore.Utils.WabiSabi.Models.Serialization;

public class ExceptionDataJsonConverter : GenericInterfaceJsonConverter<ExceptionData>
{
	public ExceptionDataJsonConverter() : base(new[] { typeof(InputBannedExceptionData), typeof(EmptyExceptionData), typeof(WrongPhaseExceptionData) })
	{
	}
}
