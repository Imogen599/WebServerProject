using System.Collections.Concurrent;

namespace MyAuthServer.Services
{
	public interface IDatabaseServiceFactory
	{
		IDatabaseService GetDatabaseService(string databaseName);
	}

	public class DatabaseServiceFactory : IDatabaseServiceFactory
	{
		private readonly static ConcurrentDictionary<string, IDatabaseService> _databaseServices = [];

		public IDatabaseService GetDatabaseService(string databaseName)
			=> _databaseServices.GetOrAdd(databaseName, name => new SqliteDatabaseService(name));
	}
}
