namespace MyAuthServer.Services
{
	public abstract class TimedHostedService : BackgroundService
	{
		protected abstract TimeSpan Period { get; }

		protected sealed override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			using PeriodicTimer timer = new(Period);

			while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
			{
				try
				{
					await PerformServiceAsync(stoppingToken);
				}
				catch (Exception) { }
			}
		}

		protected abstract Task PerformServiceAsync(CancellationToken stoppingToken);
	}
}
