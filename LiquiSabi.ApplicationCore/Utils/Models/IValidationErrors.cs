namespace LiquiSabi.ApplicationCore.Utils.Models;

public interface IValidationErrors
{
	void Add(ErrorSeverity severity, string error);
}
