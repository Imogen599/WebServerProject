using MyAuthServer.SQL;

namespace MyAuthServer.Services
{
	public interface ISessionTokenService
	{
		Task CreateSession(int userId, string token, string deviceId);

		Task<UserIDFetchModel> FetchUserIdForSession(string token, string deviceId);

		Task PerformPeriodicCleanup();

		Task RemoveSession(string token, string deviceId);

		Task<bool> ValidateSession(string token, string deviceId);
	}

	public class SessionTokenService(IDatabaseServiceFactory databaseServiceFactory) : ISessionTokenService
	{
		private static readonly string createSessionSQL = SQLQueryBuilder.Begin(SQLKeywords.INSERT).IntoValues("sessionTokens", "token", "userId", "deviceId", "expiry").BuildAndClear();

		public async Task CreateSession(int userId, string token, string deviceId)
		{
			await _databaseFactory.GetDatabaseService("sessionTokenDatabase").ExecuteNonQueryAsync(createSessionSQL, [("@token", token), ("@userId", userId), ("@deviceId", deviceId), ("@expiry", DateTime.Now.AddHours(1))]);
		}

		private static readonly string fetchUserIdSQL = SQLQueryBuilder.Begin(SQLKeywords.SELECT).From("sessionTokens").Where("deviceId", SQLKeywords.EQUALS).And("token", SQLKeywords.EQUALS).BuildAndClear();

		public async Task<UserIDFetchModel> FetchUserIdForSession(string token, string deviceId)
		{
			var list = await _databaseFactory.GetDatabaseService("sessionTokenDatabase").ExecuteReaderAsync(fetchUserIdSQL, reader =>
			{
				List<object> result = [];
				result.Add(reader.GetInt32(2));
				return result;
			},
			[("@token", token), ("deviceId", deviceId)]);

			if (list.Count == 0)
				return new(false, 0);

			return new(true, (int)list[0]);
		}

		private static readonly string PerformPeriodicCleanupSQL = SQLQueryBuilder.Begin("DELETE").From("sessionTokens").Where("expiry", SQLKeywords.LESSER_THAN_OR_EQUAL_TO).BuildAndClear();

		public async Task PerformPeriodicCleanup()
		{
			await _databaseFactory.GetDatabaseService("sessionTokenDatabase").ExecuteNonQueryAsync(PerformPeriodicCleanupSQL, [("@expiry", DateTime.Now)]);
		}

		private readonly IDatabaseServiceFactory _databaseFactory = databaseServiceFactory;

		private static readonly string removeSessionSQL = SQLQueryBuilder.Begin(SQLKeywords.DELETE).From("sessionTokens").Where("@token", SQLKeywords.EQUALS).And("@deviceId", SQLKeywords.EQUALS).BuildAndClear();

		public async Task RemoveSession(string token, string deviceId)
			=> await _databaseFactory.GetDatabaseService("sessionTokenDatabase").ExecuteNonQueryAsync(removeSessionSQL, [("@token", token), ("@deviceId", deviceId)]);

		private static readonly string validateSessionSQL1 = SQLQueryBuilder.Begin(SQLKeywords.SELECT).From("sessionTokens").Where("deviceId", SQLKeywords.EQUALS).And("token", SQLKeywords.EQUALS).BuildAndClear();

		private static readonly string validateSessionSQL2 = SQLQueryBuilder.Begin(SQLKeywords.UPDATE, "sessionTokens").Set("expiry").Where("deviceId", SQLKeywords.EQUALS).And("token", SQLKeywords.EQUALS).BuildAndClear();

		public async Task<bool> ValidateSession(string token, string deviceId)
		{
			var list = await _databaseFactory.GetDatabaseService("sessionTokenDatabase").ExecuteReaderAsync(validateSessionSQL1, reader =>
			{
				List<object> results = [];
				results.Add(reader.GetDateTime(4));
				return results;
			},
			[("@token", token), ("@deviceId", deviceId)]);

			if (list.Count == 0)
				return false;


			var expiry = (DateTime)list[0];

			// It has expired. It will be removed.
			if (expiry <= DateTime.Now)
			{
				await RemoveSession(token, deviceId);
				return false;
			}

			// Update the expiry time.
			await _databaseFactory.GetDatabaseService("sessionTokenDatabase").ExecuteNonQueryAsync(validateSessionSQL2, [("@token", token), ("@deviceId", deviceId), ("@expiry", DateTime.Now.AddHours(1))]);
			return true;
		}
	}
}
