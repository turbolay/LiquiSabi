namespace LiquiSabi.ApplicationCore.Utils.WabiSabi.Backend.Models;

public abstract record ExceptionData
{
}

public record EmptyExceptionData() : ExceptionData
{
	public static readonly EmptyExceptionData Instance = new();
}
