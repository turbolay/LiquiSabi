using WabiSabi.Crypto.Randomness;
using LiquiSabi.ApplicationCore.Utils.Extensions;

namespace LiquiSabi.ApplicationCore.Utils.Crypto.Randomness;

public class MockRandom : WasabiRandom
{
	public List<byte[]> GetBytesResults { get; } = new List<byte[]>();

	public override void GetBytes(Span<byte> output)
	{
		var first = GetBytesResults.First();
		GetBytesResults.RemoveFirst();
		first.AsSpan().CopyTo(output);
	}

	public override void GetBytes(byte[] output)
	{
		throw new NotImplementedException();
	}

	public override int GetInt(int fromInclusive, int toExclusive)
	{
		throw new NotImplementedException();
	}
}
