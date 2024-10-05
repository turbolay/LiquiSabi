using LiquiSabi.ApplicationCore.Utils.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LiquiSabi.ApplicationCore;

public static class DeploymentConfiguration
{
    private static readonly string ConfigFileName = Path.Combine(EnvironmentHelpers.GetDataDir(Path.Combine("LiquiSabi", "Client")), "DeploymentConfig.json");

    // Twitter block
    public static string ConsumerKey { get; private set; }
    public static string ConsumerSecret { get; private set; }
    public static string AccessToken { get; private set; }
    public static string TokenSecret { get; private set; }

    // Nostr block
    public static string Nsec { get; private set; }

    // Website block
    public static string Website { get; private set; }

    public static void LoadConfiguration()
    {
        if (!File.Exists(ConfigFileName))
        {
            CreateEmptyConfigFile();
            throw new FileNotFoundException($"Configuration file {ConfigFileName} was not found. An empty file has been created. Please fill it with appropriate values.");
        }

        string jsonString = File.ReadAllText(ConfigFileName);
        JObject config = JObject.Parse(jsonString);

        ConsumerKey = config["Twitter"]["ConsumerKey"].Value<string>();
        ConsumerSecret = config["Twitter"]["ConsumerSecret"].Value<string>();
        AccessToken = config["Twitter"]["AccessToken"].Value<string>();
        TokenSecret = config["Twitter"]["TokenSecret"].Value<string>();

        Nsec = config["Nostr"]["Nsec"].Value<string>();

        Website = config["Website"]["Url"].Value<string>();

        if (string.IsNullOrEmpty(ConsumerKey) || string.IsNullOrEmpty(ConsumerSecret) ||
            string.IsNullOrEmpty(AccessToken) || string.IsNullOrEmpty(TokenSecret) ||
            string.IsNullOrEmpty(Nsec) || string.IsNullOrEmpty(Website))
        {
            throw new InvalidOperationException("One or more configuration values are empty. Please fill all values in the config.json file.");
        }
    }

    private static void CreateEmptyConfigFile()
    {
        var emptyConfig = new JObject
        {
            ["Twitter"] = new JObject
            {
                ["ConsumerKey"] = "",
                ["ConsumerSecret"] = "",
                ["AccessToken"] = "",
                ["TokenSecret"] = ""
            },
            ["Nostr"] = new JObject
            {
                ["Nsec"] = ""
            },
            ["Website"] = new JObject
            {
                ["Url"] = ""
            }
        };

        File.WriteAllText(ConfigFileName, emptyConfig.ToString(Formatting.Indented));
    }
}