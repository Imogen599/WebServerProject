using Microsoft.Data.Sqlite;
using System.Security.Cryptography;

namespace MyAuthServer.SQL
{
	public static class SQLDatabaseManager
	{
		internal static async void PrepareDatabases()
		{
			using var connection = await ConnectToSQLDatabase("loginInfoDatabase");
			var sql = @"CREATE TABLE IF NOT EXISTS logins(
				id INTEGER PRIMARY KEY,
				username TEXT NOT NULL,
				password TEXT NOT NULL,
				salt TEXT NOT NULL
				);";

			using var command = new SqliteCommand(sql, connection);
			command.ExecuteNonQuery();

			using var connection2 = await ConnectToSQLDatabase("sessionTokenDatabase");
			var sql2 = @"CREATE TABLE IF NOT EXISTS sessionTokens(
				id INTEGER PRIMARY KEY,
				token TEXT NOT NULL,
				userId INTEGER NOT NULL,
				deviceId TEXT NOT NULL,
				expiry DATETIME NOT NULL
			);";

			using var command2 = new SqliteCommand(sql2, connection2);
			command2.ExecuteNonQuery();
		}

		internal static async Task<SqliteConnection> ConnectToSQLDatabase(string databaseName)
		{
			var connection = new SqliteConnection($"Data Source=Databases/{databaseName}.db");
			await connection.OpenAsync();
			return connection;
		}

		internal static byte[] GenerateSalt()
		{
			byte[] salt = new byte[32];
			using var rng = RandomNumberGenerator.Create();
			rng.GetBytes(salt);
			return salt;
		}

		internal static byte[] HashPassword(string password, byte[] salt, int iterations = 10000)
		{
			using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA512);
			return pbkdf2.GetBytes(32);
		}
	}
}
