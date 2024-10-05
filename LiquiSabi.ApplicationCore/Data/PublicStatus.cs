using LiquiSabi.ApplicationCore.Publishing.Nostr;
using LiquiSabi.ApplicationCore.Utils.WabiSabi.Models;

namespace LiquiSabi.ApplicationCore.Data
{
    public record PublicStatus(CoordinatorDiscovery.Coordinator Coordinator, DateTimeOffset ScrapedAt, RoundStateResponse Rounds, HumanMonitorResponse HumanMonitor);
}