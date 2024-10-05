using LiquiSabi.ApplicationCore.Utils.Affiliation.Models.CoinjoinNotification;

namespace LiquiSabi.ApplicationCore.Utils.Affiliation.Models;

public record CoinJoinNotificationRequest(Body Body, byte[] Signature);
