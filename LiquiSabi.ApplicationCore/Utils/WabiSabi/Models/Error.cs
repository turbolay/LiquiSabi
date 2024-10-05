using LiquiSabi.ApplicationCore.Utils.WabiSabi.Backend.Models;

namespace LiquiSabi.ApplicationCore.Utils.WabiSabi.Models;

public record Error(
	string Type,
	string ErrorCode,
	string Description,
	ExceptionData ExceptionData
);
