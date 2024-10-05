using System.Runtime.CompilerServices;
using NBitcoin;
using LiquiSabi.ApplicationCore.Utils.Logging;
using LiquiSabi.ApplicationCore.Utils.WabiSabi.Backend.Rounds;
using LiquiSabi.ApplicationCore.Utils.WabiSabi.Models;
using LiquiSabi.ApplicationCore.Utils.Wallets;

namespace LiquiSabi.ApplicationCore.Utils.WabiSabi;

public static class LoggerTools
{
	public static void Log(this Round round, LogLevel logLevel, string logMessage, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerMemberName = "", [CallerLineNumber] int callerLineNumber = -1)
	{
		var roundState = RoundState.FromRound(round);
		roundState.Log(logLevel, logMessage, callerFilePath: callerFilePath, callerMemberName: callerMemberName, callerLineNumber: callerLineNumber);
	}

	public static void LogInfo(this Round round, string logMessage, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerMemberName = "", [CallerLineNumber] int callerLineNumber = -1)
	{
		Log(round, LogLevel.Info, logMessage, callerFilePath: callerFilePath, callerMemberName: callerMemberName, callerLineNumber: callerLineNumber);
	}

	public static void LogWarning(this Round round, string logMessage, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerMemberName = "", [CallerLineNumber] int callerLineNumber = -1)
	{
		Log(round, LogLevel.Warning, logMessage, callerFilePath: callerFilePath, callerMemberName: callerMemberName, callerLineNumber: callerLineNumber);
	}

	public static void LogError(this Round round, string logMessage, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerMemberName = "", [CallerLineNumber] int callerLineNumber = -1)
	{
		Log(round, LogLevel.Error, logMessage, callerFilePath: callerFilePath, callerMemberName: callerMemberName, callerLineNumber: callerLineNumber);
	}

	public static void Log(this RoundState roundState, LogLevel logLevel, string logMessage, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerMemberName = "", [CallerLineNumber] int callerLineNumber = -1)
	{
		string round = roundState.BlameOf == uint256.Zero ? "Round" : "Blame Round";

		Logger.Log(logLevel, $"{round} ({roundState.Id}): {logMessage}", callerFilePath: callerFilePath, callerMemberName: callerMemberName, callerLineNumber: callerLineNumber);
	}

	public static void LogInfo(this RoundState roundState, string logMessage, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerMemberName = "", [CallerLineNumber] int callerLineNumber = -1)
	{
		Log(roundState, LogLevel.Info, logMessage, callerFilePath: callerFilePath, callerMemberName: callerMemberName, callerLineNumber: callerLineNumber);
	}

	public static void LogDebug(this RoundState roundState, string logMessage, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerMemberName = "", [CallerLineNumber] int callerLineNumber = -1)
	{
		Log(roundState, LogLevel.Debug, logMessage, callerFilePath: callerFilePath, callerMemberName: callerMemberName, callerLineNumber: callerLineNumber);
	}

	public static void Log(this IWallet wallet, LogLevel logLevel, string logMessage, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerMemberName = "", [CallerLineNumber] int callerLineNumber = -1)
	{
		Logger.Log(logLevel, $"Wallet ({wallet.WalletName}): {logMessage}", callerFilePath: callerFilePath, callerMemberName: callerMemberName, callerLineNumber: callerLineNumber);
	}

	public static void LogInfo(this IWallet wallet, string logMessage, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerMemberName = "", [CallerLineNumber] int callerLineNumber = -1)
	{
		Log(wallet, LogLevel.Info, logMessage, callerFilePath: callerFilePath, callerMemberName: callerMemberName, callerLineNumber: callerLineNumber);
	}

	public static void LogDebug(this IWallet wallet, string logMessage, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerMemberName = "", [CallerLineNumber] int callerLineNumber = -1)
	{
		Log(wallet, LogLevel.Debug, logMessage, callerFilePath: callerFilePath, callerMemberName: callerMemberName, callerLineNumber: callerLineNumber);
	}

	public static void LogWarning(this IWallet wallet, string logMessage, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerMemberName = "", [CallerLineNumber] int callerLineNumber = -1)
	{
		Log(wallet, LogLevel.Warning, logMessage, callerFilePath: callerFilePath, callerMemberName: callerMemberName, callerLineNumber: callerLineNumber);
	}

	public static void LogError(this IWallet wallet, string logMessage, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerMemberName = "", [CallerLineNumber] int callerLineNumber = -1)
	{
		Log(wallet, LogLevel.Error, logMessage, callerFilePath: callerFilePath, callerMemberName: callerMemberName, callerLineNumber: callerLineNumber);
	}

	public static void LogTrace(this IWallet wallet, string logMessage, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerMemberName = "", [CallerLineNumber] int callerLineNumber = -1)
	{
		Log(wallet, LogLevel.Trace, logMessage, callerFilePath: callerFilePath, callerMemberName: callerMemberName, callerLineNumber: callerLineNumber);
	}
}