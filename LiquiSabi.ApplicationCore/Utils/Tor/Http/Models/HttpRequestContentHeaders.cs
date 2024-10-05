using System.Net.Http.Headers;

namespace LiquiSabi.ApplicationCore.Utils.Tor.Http.Models;

public record HttpRequestContentHeaders(
	HttpRequestHeaders RequestHeaders,
	HttpContentHeaders ContentHeaders);
