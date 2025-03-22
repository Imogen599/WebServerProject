using Microsoft.Data.Sqlite;
using System.Security.Cryptography;

namespace MyAuthServer.SQL
{
	/// <summary>
	/// Creates the sql databases, and handles the direct connection to them.
	/// </summary>
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
	}
}
