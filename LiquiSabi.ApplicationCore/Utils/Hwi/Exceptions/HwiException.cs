namespace LiquiSabi.ApplicationCore.Utils.Hwi.Exceptions;

public class HwiException : Exception
{
	public HwiException(HwiErrorCode errorCode, string message) : base(message)
	{
		ErrorCode = errorCode;
	}

	public HwiErrorCode ErrorCode { get; }
}
