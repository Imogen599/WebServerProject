using Serilog;

namespace MyAuthServer.Services
{
	public sealed class SessionCleanupService(ISessionTokenService sessionTokenService) : TimedHostedService
	{
		private readonly ISessionTokenService _sessionTokenService = sessionTokenService;

		protected override TimeSpan Period => TimeSpan.FromHours(0.5);

		protected sealed override async Task PerformServiceAsync(CancellationToken _)
		{
			try
			{
				await _sessionTokenService.PerformPeriodicCleanup();
				Log.Information($"Successfully performed session token cleanup.");

			}
			catch (Exception)
			{
				// While exceptions are logged directly already, I feel this warrants a specific error log due to what it would entail.
				Log.Error($"Was unable to perform session token cleanup due to exception.");
			}
		}
	}
}
