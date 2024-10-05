using LiquiSabi.ApplicationCore.Utils.WabiSabi.Backend.Rounds;

namespace LiquiSabi.ApplicationCore.Utils.WabiSabi.Backend.Models;

public record WrongPhaseExceptionData(Phase CurrentPhase) : ExceptionData;
