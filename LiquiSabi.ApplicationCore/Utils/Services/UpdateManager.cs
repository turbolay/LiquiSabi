using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;
using LiquiSabi.ApplicationCore.Utils.Helpers;
using LiquiSabi.ApplicationCore.Utils.Logging;
using LiquiSabi.ApplicationCore.Utils.Microservices;
using LiquiSabi.ApplicationCore.Utils.Models;
using LiquiSabi.ApplicationCore.Utils.Tor.Http;

namespace LiquiSabi.ApplicationCore.Utils.Services;

public class UpdateManager : IDisposable
{
	private string InstallerPath { get; set; } = "";
	private const byte MaxTries = 2;
	private const string ReleaseURL = "https://api.github.com/repos/zkSNACKs/WalletWasabi/releases/latest";

	public UpdateManager(string dataDir, bool downloadNewVersion, IHttpClient httpClient)
	{
		InstallerDir = Path.Combine(dataDir, "Installer");
		HttpClient = httpClient;
		DownloadNewVersion = downloadNewVersion;
	}

	private async void UpdateChecker_UpdateStatusChangedAsync(object? sender, UpdateStatus updateStatus)
	{
		var tries = 0;
		bool updateAvailable = !updateStatus.ClientUpToDate || !updateStatus.BackendCompatible;
		Version targetVersion = updateStatus.ClientVersion;

		if (!updateAvailable)
		{
			// After updating Wasabi, remove old installer file.
			Cleanup();
			return;
		}

		if (DownloadNewVersion && !RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
		{
			do
			{
				tries++;
				try
				{
					(string installerPath, Version newVersion) = await GetInstallerAsync(targetVersion).ConfigureAwait(false);
					InstallerPath = installerPath;
					Logger.LogInfo($"Version {newVersion} downloaded successfully.");
					updateStatus.IsReadyToInstall = true;
					updateStatus.ClientVersion = newVersion;
					break;
				}
				catch (OperationCanceledException ex)
				{
					Logger.LogTrace($"Getting new update was canceled.", ex);
					break;
				}
				catch (InvalidOperationException ex)
				{
					Logger.LogError($"Getting new update failed with error.", ex);
					Cleanup();
					break;
				}
				catch (Exception ex)
				{
					Logger.LogError($"Getting new update failed with error.", ex);
				}
			} while (tries < MaxTries);
		}

		UpdateAvailableToGet?.Invoke(this, updateStatus);
	}

	/// <summary>
	/// Get or download installer for the newest release.
	/// </summary>
	/// <param name="targetVersion">This does not contains the revision number, because backend always sends zero.</param>
	private async Task<(string filePath, Version newVersion)> GetInstallerAsync(Version targetVersion)
	{
		var result = await GetLatestReleaseFromGithubAsync(targetVersion).ConfigureAwait(false);
		var sha256SumsFilePath = Path.Combine(InstallerDir, "SHA256SUMS.asc");

		// This will throw InvalidOperationException in case of invalid signature.
		await DownloadAndValidateWasabiSignatureAsync(sha256SumsFilePath, result.Sha256SumsUrl, result.WasabiSigUrl).ConfigureAwait(false);

		var installerFilePath = Path.Combine(InstallerDir, result.InstallerFileName);

		try
		{
			if (!File.Exists(installerFilePath))
			{
				EnsureToRemoveCorruptedFiles();

				// This should also be done using Tor.
				// TODO: https://github.com/zkSNACKs/WalletWasabi/issues/8800
				Logger.LogInfo($"Trying to download new version: {result.LatestVersion}");
				using HttpClient httpClient = new();

				// Get file stream and copy it to downloads folder to access.
				using var stream = await httpClient.GetStreamAsync(result.InstallerDownloadUrl, CancellationToken).ConfigureAwait(false);
				Logger.LogInfo("Installer downloaded, copying...");

				await CopyStreamContentToFileAsync(stream, installerFilePath).ConfigureAwait(false);
			}
			string expectedHash = await GetHashFromSha256SumsFileAsync(result.InstallerFileName, sha256SumsFilePath).ConfigureAwait(false);
			await VerifyInstallerHashAsync(installerFilePath, expectedHash).ConfigureAwait(false);
		}
		catch (IOException)
		{
			CancellationToken.ThrowIfCancellationRequested();
			throw;
		}

		return (installerFilePath, result.LatestVersion);
	}

	private async Task VerifyInstallerHashAsync(string installerFilePath, string expectedHash)
	{
		var bytes = await WasabiSignerHelpers.GetShaComputedBytesOfFileAsync(installerFilePath, CancellationToken).ConfigureAwait(false);
		string downloadedHash = Convert.ToHexString(bytes).ToLower();

		if (expectedHash != downloadedHash)
		{
			throw new InvalidOperationException("Downloaded file hash doesn't match expected hash.");
		}
	}

	private async Task<string> GetHashFromSha256SumsFileAsync(string installerFileName, string sha256SumsFilePath)
	{
		string[] lines = await File.ReadAllLinesAsync(sha256SumsFilePath).ConfigureAwait(false);
		var correctLine = lines.FirstOrDefault(line => line.Contains(installerFileName))
			?? throw new InvalidOperationException($"{installerFileName} was not found.");
		return correctLine.Split(" ")[0];
	}

	private async Task CopyStreamContentToFileAsync(Stream stream, string filePath)
	{
		if (File.Exists(filePath))
		{
			return;
		}
		var tmpFilePath = $"{filePath}.tmp";
		IoHelpers.EnsureContainingDirectoryExists(tmpFilePath);
		using (var file = File.OpenWrite(tmpFilePath))
		{
			await stream.CopyToAsync(file, CancellationToken).ConfigureAwait(false);

			// Closing the file to rename.
			file.Close();
		};
		File.Move(tmpFilePath, filePath);
	}

	private async Task<(Version LatestVersion, string InstallerDownloadUrl, string InstallerFileName, string Sha256SumsUrl, string WasabiSigUrl)> GetLatestReleaseFromGithubAsync(Version targetVersion)
	{
		using HttpRequestMessage message = new(HttpMethod.Get, ReleaseURL);
		message.Headers.UserAgent.Add(new("WalletWasabi", "2.0"));
		var response = await HttpClient.SendAsync(message, CancellationToken).ConfigureAwait(false);

		JObject jsonResponse = JObject.Parse(await response.Content.ReadAsStringAsync(CancellationToken).ConfigureAwait(false));

		string softwareVersion = jsonResponse["tag_name"]?.ToString() ?? throw new InvalidDataException("Endpoint gave back wrong json data or it's changed.");

		// "tag_name" will have a 'v' at the beginning, needs to be removed.
		Version githubVersion = new(softwareVersion[1..]);
		Version shortGithubVersion = new(githubVersion.Major, githubVersion.Minor, githubVersion.Build);
		if (targetVersion != shortGithubVersion)
		{
			throw new InvalidDataException("Target version from backend does not match with the latest GitHub release. This should be impossible.");
		}

		// Get all asset names and download URLs to find the correct one.
		List<JToken> assetsInfo = jsonResponse["assets"]?.Children().ToList() ?? throw new InvalidDataException("Missing assets from response.");
		List<string> assetDownloadURLs = new();
		foreach (JToken asset in assetsInfo)
		{
			assetDownloadURLs.Add(asset["browser_download_url"]?.ToString() ?? throw new InvalidDataException("Missing download url from response."));
		}

		string sha256SumsUrl = assetDownloadURLs.First(url => url.Contains("SHA256SUMS.asc"));
		string wasabiSigUrl = assetDownloadURLs.First(url => url.Contains("SHA256SUMS.wasabisig"));

		(string url, string fileName) = GetAssetToDownload(assetDownloadURLs);

		return (githubVersion, url, fileName, sha256SumsUrl, wasabiSigUrl);
	}

	private async Task DownloadAndValidateWasabiSignatureAsync(string sha256SumsFilePath, string sha256SumsUrl, string wasabiSigUrl)
	{
		var wasabiSigFilePath = Path.Combine(InstallerDir, "SHA256SUMS.wasabisig");

		using HttpClient httpClient = new();

		try
		{
			using (var stream = await httpClient.GetStreamAsync(sha256SumsUrl, CancellationToken).ConfigureAwait(false))
			{
				await CopyStreamContentToFileAsync(stream, sha256SumsFilePath).ConfigureAwait(false);
			}

			using (var stream = await httpClient.GetStreamAsync(wasabiSigUrl, CancellationToken).ConfigureAwait(false))
			{
				await CopyStreamContentToFileAsync(stream, wasabiSigFilePath).ConfigureAwait(false);
			}

			await WasabiSignerHelpers.VerifySha256SumsFileAsync(sha256SumsFilePath).ConfigureAwait(false);
		}
		catch (HttpRequestException exc)
		{
			string message = "";
			if (exc.StatusCode is HttpStatusCode.NotFound)
			{
				message = "Wasabi signature files were not found under the API.";
			}
			else
			{
				message = "Something went wrong while getting Wasabi signature files.";
			}
			throw new InvalidOperationException(message, exc);
		}
		catch (IOException)
		{
			// There's a chance to get IOException when closing Wasabi during stream copying. Throw OperationCancelledException instead.
			CancellationToken.ThrowIfCancellationRequested();
			throw;
		}
	}

	private (string url, string fileName) GetAssetToDownload(List<string> assetDownloadURLs)
	{
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			var url = assetDownloadURLs.First(url => url.Contains(".msi"));
			return (url, url.Split("/").Last());
		}
		else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
		{
			var cpu = RuntimeInformation.ProcessArchitecture;
			if (cpu.ToString() == "Arm64")
			{
				var arm64url = assetDownloadURLs.First(url => url.Contains("arm64.dmg"));
				return (arm64url, arm64url.Split("/").Last());
			}
			var url = assetDownloadURLs.First(url => url.Contains(".dmg") && !url.Contains("arm64"));
			return (url, url.Split("/").Last());
		}
		else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
		{
			throw new InvalidOperationException("For Linux, get the correct update manually.");
		}
		else
		{
			throw new InvalidOperationException("OS not recognized, download manually.");
		}
	}

	private void EnsureToRemoveCorruptedFiles()
	{
		DirectoryInfo folder = new(InstallerDir);
		if (folder.Exists)
		{
			IEnumerable<FileSystemInfo> corruptedFiles = folder.GetFileSystemInfos().Where(file => file.Extension.Equals(".tmp"));
			foreach (var file in corruptedFiles)
			{
				File.Delete(file.FullName);
			}
		}
	}

	private void Cleanup()
	{
		try
		{
			var folder = new DirectoryInfo(InstallerDir);
			if (folder.Exists)
			{
				Directory.Delete(InstallerDir, true);
			}
		}
		catch (Exception exc)
		{
			Logger.LogError("Failed to delete installer directory.", exc);
		}
	}

	public event EventHandler<UpdateStatus>? UpdateAvailableToGet;

	public string InstallerDir { get; }
	public IHttpClient HttpClient { get; }

	///<summary> Comes from config file. Decides Wasabi should download the new installer in the background or not.</summary>
	public bool DownloadNewVersion { get; }

	///<summary> Install new version on shutdown or not.</summary>
	public bool DoUpdateOnClose { get; set; }

	private UpdateChecker? UpdateChecker { get; set; }
	private CancellationToken CancellationToken { get; set; }

	public void StartInstallingNewVersion()
	{
		try
		{
			ProcessStartInfo startInfo;
			if (!File.Exists(InstallerPath))
			{
				throw new FileNotFoundException(InstallerPath);
			}
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				startInfo = ProcessStartInfoFactory.Make(InstallerPath, "", true);
			}
			else
			{
				startInfo = new()
				{
					FileName = InstallerPath,
					UseShellExecute = true,
					WindowStyle = ProcessWindowStyle.Normal
				};
			}

			using Process? p = Process.Start(startInfo);

			if (p is null)
			{
				throw new InvalidOperationException($"Can't start {nameof(p)} {startInfo.FileName}.");
			}
			if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			{
				// For MacOS, you need to start the process twice, first start => permission denied
				// TODO: find out why and fix.

				p!.WaitForExit(5000);
				p.Start();
			}
		}
		catch (Exception ex)
		{
			Logger.LogError("Failed to install latest release. File might be corrupted.", ex);
		}
	}

	public void Initialize(UpdateChecker updateChecker, CancellationToken cancelationToken)
	{
		UpdateChecker = updateChecker;
		CancellationToken = cancelationToken;
		updateChecker.UpdateStatusChanged += UpdateChecker_UpdateStatusChangedAsync;
	}

	public void Dispose()
	{
		if (UpdateChecker is { } updateChecker)
		{
			updateChecker.UpdateStatusChanged -= UpdateChecker_UpdateStatusChangedAsync;
		}
	}
}