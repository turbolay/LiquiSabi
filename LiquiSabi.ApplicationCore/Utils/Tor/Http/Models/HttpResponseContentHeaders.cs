using System.Net.Http.Headers;

namespace LiquiSabi.ApplicationCore.Utils.Tor.Http.Models;

public record HttpResponseContentHeaders(
	HttpResponseHeaders ResponseHeaders,
	HttpContentHeaders ContentHeaders);
