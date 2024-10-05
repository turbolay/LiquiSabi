namespace LiquiSabi.ApplicationCore.Utils.Affiliation.Models.CoinjoinNotification;

public record Header(string Title, string AffiliationId, int Version)
{
	public static Header Create(string affiliationId) => new(Title: "coinjoin notification", AffiliationId: affiliationId, Version: 1);
}
