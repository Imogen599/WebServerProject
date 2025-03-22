using System.Collections.Concurrent;

namespace MyAuthServer.Services
{
	/// <summary>
	/// Provides a factory for getting instances of <see cref="IDatabaseService"/> for a specfic database.
	/// </summary>
	public interface IDatabaseServiceFactory
	{
		/// <summary>
		/// Gets a database service for the provided database.
		/// </summary>
		/// <param name="databaseName">The name of the database to get a service for.</param>
		IDatabaseService GetDatabaseService(string databaseName);
	}

	/// <summary>
	/// Main implementation of <see cref="IDatabaseServiceFactory"/>.
	/// </summary>
	public class DatabaseServiceFactory : IDatabaseServiceFactory
	{
		private readonly static ConcurrentDictionary<string, IDatabaseService> _databaseServices = [];

		public IDatabaseService GetDatabaseService(string databaseName)
			=> _databaseServices.GetOrAdd(databaseName, name => new SqliteDatabaseService(name));
	}
}
