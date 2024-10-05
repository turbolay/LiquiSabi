using LiquiSabi.ApplicationCore.Utils.Bases;
using LiquiSabi.ApplicationCore.Utils.Helpers;
using LiquiSabi.ApplicationCore.Utils.Interfaces;

namespace LiquiSabi.ApplicationCore.Utils.Services;

public class ConfigWatcher : PeriodicRunner
{
	public ConfigWatcher(TimeSpan period, IConfig config, Action executeWhenChanged) : base(period)
	{
		Config = Guard.NotNull(nameof(config), config);
		ExecuteWhenChanged = Guard.NotNull(nameof(executeWhenChanged), executeWhenChanged);
		config.AssertFilePathSet();
	}

	public IConfig Config { get; }
	public Action ExecuteWhenChanged { get; }

	protected override Task ActionAsync(CancellationToken cancel)
	{
		if (Config.CheckFileChange())
		{
			cancel.ThrowIfCancellationRequested();
			Config.LoadFile(createIfMissing: true);

			ExecuteWhenChanged();
		}
		return Task.CompletedTask;
	}
}
