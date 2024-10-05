using System.Text.Json;
using NBitcoin;
using NBitcoin.RPC;
using LiquiSabi.ApplicationCore.Utils.Blockchain.Analysis.FeesEstimation;
using LiquiSabi.ApplicationCore.Utils.Tor.Http;
using LiquiSabi.ApplicationCore.Utils.Tor.Http.Extensions;
using LiquiSabi.ApplicationCore.Utils.Tor.Socks5.Pool.Circuits;
using LiquiSabi.ApplicationCore.Utils.WebClients.Wasabi;

namespace LiquiSabi.ApplicationCore.Utils.WebClients.BlockstreamInfo;

public class BlockstreamInfoClient
{
	public BlockstreamInfoClient(Network network, WasabiHttpClientFactory httpClientFactory)
	{
		string uriString;

		if (httpClientFactory.IsTorEnabled)
		{
			uriString = network == Network.TestNet
				? "http://explorerzydxu5ecjrkwceayqybizmpjjznk5izmitf2modhcusuqlid.onion/testnet"
				: "http://explorerzydxu5ecjrkwceayqybizmpjjznk5izmitf2modhcusuqlid.onion";
		}
		else
		{
			uriString = network == Network.TestNet
				? "https://blockstream.info/testnet"
				: "https://blockstream.info";
		}

		HttpClient = httpClientFactory.NewHttpClient(() => new Uri(uriString), Mode.DefaultCircuit);
	}

	private IHttpClient HttpClient { get; }

	public async Task<AllFeeEstimate> GetFeeEstimatesAsync(CancellationToken cancel)
	{
		using HttpResponseMessage response = await HttpClient.SendAsync(HttpMethod.Get, "api/fee-estimates", null, cancel).ConfigureAwait(false);

		if (!response.IsSuccessStatusCode)
		{
			await response.ThrowRequestExceptionFromContentAsync(cancel).ConfigureAwait(false);
		}

		var responseString = await response.Content.ReadAsStringAsync(cancel).ConfigureAwait(false);
		var parsed = JsonDocument.Parse(responseString).RootElement;
		var myDic = new Dictionary<int, int>();
		foreach (var elem in parsed.EnumerateObject())
		{
			myDic.Add(int.Parse(elem.Name), (int)Math.Ceiling(elem.Value.GetDouble()));
		}

		return new AllFeeEstimate(EstimateSmartFeeMode.Conservative, myDic, isAccurate: true);
	}
}
