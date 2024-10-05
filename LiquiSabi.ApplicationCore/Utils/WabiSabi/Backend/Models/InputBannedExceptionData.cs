namespace LiquiSabi.ApplicationCore.Utils.WabiSabi.Backend.Models;

public record InputBannedExceptionData(DateTimeOffset BannedUntil) : ExceptionData;
