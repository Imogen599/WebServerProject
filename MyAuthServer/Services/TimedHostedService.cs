namespace MyAuthServer.Services
{
	/// <summary>
	/// A background service that runs on a periodic timer.
	/// </summary>
	public abstract class TimedHostedService : BackgroundService
	{
		/// <summary>
		/// The period of time between the service being performed.
		/// </summary>
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

		/// <summary>
		/// Called after <see cref="Period"/> amount of time repeatedly.
		/// </summary>
		/// <param name="stoppingToken"></param>
		/// <returns></returns>
		protected abstract Task PerformServiceAsync(CancellationToken stoppingToken);
	}
}
