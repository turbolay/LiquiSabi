using System.Collections.Immutable;
using LiquiSabi.ApplicationCore.Utils.Helpers;

namespace LiquiSabi.ApplicationCore.Utils.WabiSabi.Client.CredentialDependencies;

public abstract class RequestNode
{
	public RequestNode(IEnumerable<long> values, int inDegree, int outDegree, int zeroOnlyOutDegree)
	{
		Values = Guard.InRange(nameof(values), values, DependencyGraph.K, DependencyGraph.K).ToImmutableArray();
		MaxInDegree = inDegree;
		MaxOutDegree = outDegree;
		MaxZeroOnlyOutDegree = zeroOnlyOutDegree;
	}

	public ImmutableArray<long> Values { get; }

	public int MaxInDegree { get; }

	public int MaxOutDegree { get; }

	public int MaxZeroOnlyOutDegree { get; }

	public long InitialBalance(CredentialType type) => Values[(int)type];
}
