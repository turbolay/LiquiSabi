using LiquiSabi.ApplicationCore.Utils.Wallets;

namespace LiquiSabi.ApplicationCore.Utils.WabiSabi.Client;

public interface IWalletProvider
{
	Task<IEnumerable<IWallet>> GetWalletsAsync();
}
