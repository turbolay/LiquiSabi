using System.ComponentModel.DataAnnotations;

namespace LiquiSabi.ApplicationCore.Utils.Backend.Models;

/// <summary>
/// Satoshi per byte.
/// </summary>
public class FeeEstimationPair
{
	[Required]
	public long Economical { get; set; }

	[Required]
	public long Conservative { get; set; }
}
