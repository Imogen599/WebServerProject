using MyAuthServer.RequestStructures;
using MyAuthServer.SQL;
using Serilog;

namespace MyAuthServer.Services
{
	public interface IUserService
	{
		Task<UserPasswordChangeModel> ChangeUserPassword(int userId, ChangePasswordRequest request, string ip);

		Task<bool> IsUsernameTakenAsync(string username);

		Task<UserLoginModel> LoginUserAsync(CustomLoginRequest request);

		Task LogoutAsync(BasicRequest request);

		Task RegisterUserAsync(CustomLoginRequest request);
	}

	public class UserService(IDatabaseServiceFactory databaseServiceFactory) : IUserService
	{
		private static readonly string changePasswordSQL1 = SQLQueryBuilder.Begin(SQLKeywords.SELECT).From("logins").Where("id", SQLKeywords.EQUALS).BuildAndClear();

		private static readonly string changePasswordSQL2 = SQLQueryBuilder.Begin(SQLKeywords.UPDATE, "logins").Set("password").AdditionalSets("salt").Where("username", SQLKeywords.EQUALS).And("id", SQLKeywords.EQUALS).BuildAndClear();

		public async Task<UserPasswordChangeModel> ChangeUserPassword(int userId, ChangePasswordRequest request, string ip)
		{
			var list = await _databaseFactory.GetDatabaseService("loginInfoDatabase").ExecuteReaderAsync(changePasswordSQL1, reader =>
			{
				List<object> result = [];
				result.Add(reader.GetInt32(0));
				result.Add(reader.GetString(1));
				result.Add(reader.GetString(2));
				result.Add(reader.GetString(3));
				return result;
			}, [("@id", userId)]);

			if (list.Count == 0)
				return new(false, "The user you are trying to change the password for does not exist");

			var id = (int)list[0];
			string username = (string)list[1];
			var currentPasswordHash = (string)list[2];
			var currentSalt = (string)list[3];

			// 3. Check that the passwords don't match.
			var usableSalt = Convert.FromBase64String(currentSalt);
			var suppliedPasswordHash = Convert.ToBase64String(SQLDatabaseManager.HashPassword(request.Password, usableSalt));

			if (currentPasswordHash == suppliedPasswordHash)
				return new(false, "Your new password cannot be the same as your current password");

			var newSalt = SQLDatabaseManager.GenerateSalt();
			var newPasswordHash = SQLDatabaseManager.HashPassword(request.Password, newSalt);
			var stringSalt = Convert.ToBase64String(newSalt);
			var stringPassword = Convert.ToBase64String(newPasswordHash);

			await _databaseFactory.GetDatabaseService("loginInfoDatabase").ExecuteNonQueryAsync(changePasswordSQL2, [("@password", stringPassword), ("@salt", stringSalt),("@username", username), ("@id", id)]);
			Log.Information($"User {username} has had their password changed from IP: {ip}.");
			return new(true, string.Empty);
		}

		private readonly IDatabaseServiceFactory _databaseFactory = databaseServiceFactory;

		private static readonly string isUsernameRegisteredSQL = SQLQueryBuilder.Begin(SQLKeywords.SELECT).From("logins").Where("username", SQLKeywords.EQUALS).BuildAndClear();

		public async Task<bool> IsUsernameTakenAsync(string username)
		{
			var database = _databaseFactory.GetDatabaseService("loginInfoDatabase");
			bool result = await database.ExecuteReaderHasRowsAsync(isUsernameRegisteredSQL, [("@username", username)]);
			return result;
			//using var connection = await SQLDatabaseManager.ConnectToSQLDatabase("loginInfoDatabase");
			//using var command = new SqliteCommand(isUsernameRegisteredSQL, connection);

			//(string, object)[] values = [("@username", username)];
			//foreach (var (name, value) in values)
			//	command.Parameters.AddWithValue(name, value);

			//using var reader2 = await command.ExecuteReaderAsync();
			//return reader2.HasRows;
		}

		private static readonly string verifyCredentialsSQL = SQLQueryBuilder.Begin(SQLKeywords.SELECT).From("logins").Where("username", SQLKeywords.EQUALS).BuildAndClear();

		public async Task<UserLoginModel> LoginUserAsync(CustomLoginRequest request)
		{
			var list = await _databaseFactory.GetDatabaseService("loginInfoDatabase").ExecuteReaderAsync(verifyCredentialsSQL, reader =>
			{
				List<object> result = [];
				result.Add(reader.GetInt32(0));
				result.Add(reader.GetString(1));
				result.Add(reader.GetString(2));
				result.Add(reader.GetString(3));
				return result;
			}, [("@username", request.Username)]);


			var id = (int)list[0];
			var username = (string)list[1];
			var passwordHash = (string)list[2];
			var salt = (string)list[3];

			// Hash the given password, using the salt stored.
			var usableSalt = Convert.FromBase64String(salt);
			var suppliedPasswordHash = Convert.ToBase64String(SQLDatabaseManager.HashPassword(request.Password, usableSalt));

			if (username == request.Username && passwordHash == suppliedPasswordHash)
			{
				var token = Guid.NewGuid().ToString();
				
				return new(true, token, id);
			}
			return new(false, string.Empty, 0);
		}

		private static readonly string registerUserSQL = SQLQueryBuilder.Begin(SQLKeywords.INSERT).IntoValues("logins", "username", "password", "salt").BuildAndClear();

		public async Task RegisterUserAsync(CustomLoginRequest request)
		{
			var salt = SQLDatabaseManager.GenerateSalt();
			var password = SQLDatabaseManager.HashPassword(request.Password, salt);
			await _databaseFactory.GetDatabaseService("loginInfoDatabase").ExecuteNonQueryAsync(registerUserSQL, [("@username", request.Username), ("@password", Convert.ToBase64String(password)), ("@salt", Convert.ToBase64String(salt))]);
		}

		private static readonly string removeSessionSQL = SQLQueryBuilder.Begin(SQLKeywords.DELETE).From("sessionTokens").Where("token", SQLKeywords.EQUALS).And("deviceId", SQLKeywords.EQUALS).BuildAndClear();

		public async Task LogoutAsync(BasicRequest request)
		{
			await _databaseFactory.GetDatabaseService("sessionTokenDatabase").ExecuteNonQueryAsync(removeSessionSQL, [("@token", request.SessionToken), ("@deviceId", request.DeviceId)]);
		}
	}
}
