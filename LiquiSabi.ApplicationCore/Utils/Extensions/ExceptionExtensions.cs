using LiquiSabi.ApplicationCore.Utils.Helpers;
using LiquiSabi.ApplicationCore.Utils.Hwi.Exceptions;
using LiquiSabi.ApplicationCore.Utils.Models;

namespace LiquiSabi.ApplicationCore.Utils.Extensions;

public static class ExceptionExtensions
{
	public static string ToTypeMessageString(this Exception ex)
	{
		var trimmed = Guard.Correct(ex.Message);

		if (trimmed.Length == 0)
		{
			if (ex is HwiException hwiEx)
			{
				return $"{hwiEx.GetType().Name}: {hwiEx.ErrorCode}";
			}
			return ex.GetType().Name;
		}
		else
		{
			return $"{ex.GetType().Name}: {ex.Message}";
		}
	}

	public static SerializableException ToSerializableException(this Exception ex)
	{
		return new SerializableException(ex);
	}
}
