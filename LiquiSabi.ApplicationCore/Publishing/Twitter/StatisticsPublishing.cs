using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using LiquiSabi.ApplicationCore.Data;
using LiquiSabi.ApplicationCore.Utils.Logging;
using LiquiSabi.ApplicationCore.Utils.Tor.Http.Extensions;

namespace LiquiSabi.ApplicationCore.Publishing.Twitter
{
    public static class StatisticsPublishing
    {
        private static readonly HttpClient HttpClient = new();

        public static async Task PublishToTwitter(List<Analyzer.Analysis> analyses, CancellationToken? cancellationToken)
        {
            if (analyses.Count == 0)
            {
                return;
            }

            var tweet = MarketingHelper.BuildSummary(analyses);
            if (tweet is not null)
            {
                Logger.LogInfo(tweet);
                var result = await PostTweetWithRetryAsync(tweet, cancellationToken ?? CancellationToken.None);
                Logger.LogInfo($"Twitter thread published: {result}");
            }
        }
        
        private static async Task<string> PostTweetWithRetryAsync(string tweet, CancellationToken cancellationToken, string replyToTweetId = "", int maxRetries = 3)
        {
            int retryCount = 0;
            while (true)
            {
                try
                {
                    return await PostTweetAsync(tweet, cancellationToken, replyToTweetId);
                }
                catch (Exception ex) when (retryCount < maxRetries)
                {
                    if (retryCount == 0)
                    {
                        Logger.LogInfo("Failure to send this tweet:\n" + tweet);
                    }
                    retryCount++;
                    Logger.LogInfo($"Attempt {retryCount} failed: {ex.Message}. Retrying...");
                }
            }
        }
        
        public static async Task<string> PostTweetAsync(string text, CancellationToken cancellationToken, string replyTo = "")
        {
            var timestamp = CreateTimestamp();
            var nonce = CreateNonce();

            string body;
            if (replyTo == "")
            {
                body = JsonSerializer.Serialize(new { text });
            }
            else
            {
                var reply = new Tweet
                {
                    Text = text,
                    Reply = new Reply
                    {
                        InReplyToTweetId = replyTo
                    }
                };
                body = JsonSerializer.Serialize(reply);
            }

            var uri = new Uri("https://api.twitter.com/2/tweets");

            var request = new HttpRequestMessage
            {
                RequestUri = uri,
                Method = HttpMethod.Post,
                Content = new StringContent(body, Encoding.ASCII, "application/json")
            };

            var signatureBase64 = CreateSignature(uri.ToString(), "POST", nonce, timestamp);

            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("OAuth",
                    $@"oauth_consumer_key=""{Uri.EscapeDataString(DeploymentConfiguration.ConsumerKey)}""" +
                    $@",oauth_token=""{Uri.EscapeDataString(DeploymentConfiguration.AccessToken)}""" +
                    $@",oauth_signature_method=""HMAC-SHA1"",oauth_timestamp=""{Uri.EscapeDataString(timestamp)}""" +
                    $@",oauth_nonce=""{Uri.EscapeDataString(nonce)}"",oauth_version=""1.0""" +
                    $@",oauth_signature=""{Uri.EscapeDataString(signatureBase64)}""");

            var response = await HttpClient.SendAsync(request, cancellationToken);
            
            response.EnsureSuccessStatusCode();
            
            Root responseJson = await response.Content.ReadAsJsonAsync<Root>();

            return responseJson.data.id;
        }

        private static string CreateSignature(string url, string method, string nonce, string timestamp)
        {
            var parameters = new Dictionary<string, string>();

            parameters.Add("oauth_consumer_key", DeploymentConfiguration.ConsumerKey);
            parameters.Add("oauth_nonce", nonce);
            parameters.Add("oauth_signature_method", "HMAC-SHA1");
            parameters.Add("oauth_timestamp", timestamp);
            parameters.Add("oauth_token", DeploymentConfiguration.AccessToken);
            parameters.Add("oauth_version", "1.0");

            var sigBaseString = CombineQueryParams(parameters);

            var signatureBaseString =
                method + "&" +
                Uri.EscapeDataString(url) + "&" +
                Uri.EscapeDataString(sigBaseString);

            var compositeKey =
                Uri.EscapeDataString(DeploymentConfiguration.ConsumerSecret) + "&" +
                Uri.EscapeDataString(DeploymentConfiguration.TokenSecret);

            using (var hasher = new HMACSHA1(Encoding.ASCII.GetBytes(compositeKey)))
            {
                return Convert.ToBase64String(hasher.ComputeHash(
                    Encoding.ASCII.GetBytes(signatureBaseString)));
            }
        }

        private static string CreateTimestamp()
        {
            var totalSeconds = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc))
                .TotalSeconds;

            return Convert.ToInt64(totalSeconds).ToString();
        }

        private static string CreateNonce()
        {
            return Convert.ToBase64String(
                new ASCIIEncoding().GetBytes(
                    DateTime.Now.Ticks.ToString()));
        }

        private static string CombineQueryParams(Dictionary<string, string> parameters)
        {
            var sb = new StringBuilder();

            var first = true;

            foreach (var param in parameters)
            {
                if (!first)
                {
                    sb.Append("&");
                }

                sb.Append(param.Key);
                sb.Append("=");
                sb.Append(Uri.EscapeDataString(param.Value));

                first = false;
            }

            return sb.ToString();
        }
    }
    
    public class Data
    {
        public List<string> edit_history_tweet_ids { get; set; }
        public string id { get; set; }
        public string text { get; set; }
    }

    public class Root
    {
        public Data data { get; set; }
    }
    
    public class Tweet
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("reply")]
        public Reply Reply { get; set; }
    }

    public class Reply
    {
        [JsonPropertyName("in_reply_to_tweet_id")]
        public string InReplyToTweetId { get; set; }
    }
}