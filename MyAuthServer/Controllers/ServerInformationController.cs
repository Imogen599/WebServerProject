using Microsoft.AspNetCore.Mvc;
using MyAuthServer.RequestStructures;
using MyAuthServer.Services;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MyAuthServer.Controllers
{
	[Route("api/serverInfo")]
	[ApiController]
	[RequireHttps]
	public class ServerInformationController(ISessionTokenService sessionTokenService) : Controller
	{
		private static readonly string[] SizeSuffixes = ["bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"];

		private readonly ISessionTokenService _sessionTokenService = sessionTokenService;

		[HttpPost("getServerInfo")]
		public async Task<IActionResult> GetServerInfo([FromBody] BasicRequest request)
		{
			if (!await _sessionTokenService.ValidateSession(request.SessionToken, request.DeviceId))
				return Unauthorized(new { message = "You must be logged in to perform this action." });

			var totalMemory = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;
			long totalRamUsage = -1;
			try
			{
				totalRamUsage = Process.GetProcesses().Sum(x => x.WorkingSet64);
			}
			catch { }

			var platform = OperatingSystem.IsWindows() ? "Windows" : (OperatingSystem.IsMacOS() ? "MacOS" : "Linux");

			var info = $"Current Datetime: {DateTime.Now}\n" +
					$"Running on {platform} (v{Environment.OSVersion.Version}) {RuntimeInformation.ProcessArchitecture}\n" +
					$"Available processor core count: {Environment.ProcessorCount}\n" +
					$"Total RAM Available: {ByteSizeSuffix(totalMemory - totalRamUsage)}, Total RAM Installed: {ByteSizeSuffix(totalMemory)}";
			return Ok(new { message = info });
		}

		public static string ByteSizeSuffix(long value, int decimalPlaces = 1)
		{
			if (value < 0)
				return $"-{ByteSizeSuffix(-value)}";

			if (value == 0)
				return "0 bytes";

			// magnitude is 0 for bytes, 1 for KB, 2, for MB, etc.
			int magnitude = (int)Math.Log(value, 1024);

			// 1L << (magnitude * 10) == 2 ^ (10 * magnitude)
			// [i.e. the number of bytes in the unit corresponding to magnitude]
			decimal adjustedSize = (decimal)value / (1L << (magnitude * 10));

			// Make adjustment when the value is large enough that it would round up to 1000 or more.
			if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
			{
				magnitude += 1;
				adjustedSize /= 1024;
			}

			return string.Format("{0:n" + decimalPlaces + "} {1}", adjustedSize, SizeSuffixes[magnitude]);
		}
	}
}
