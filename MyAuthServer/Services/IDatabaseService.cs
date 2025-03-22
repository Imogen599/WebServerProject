using Microsoft.Data.Sqlite;
using MyAuthServer.SQL;

namespace MyAuthServer.Services
{
	public interface IDatabaseService
	{
		Task<int> ExecuteNonQueryAsync(string sql, params (string, object)[] values);

		Task<List<object>> ExecuteReaderAsync(string sql, Func<SqliteDataReader, List<object>> readDelegate = null, params (string, object)[] values);

		Task<bool> ExecuteReaderHasRowsAsync(string sql, params (string, object)[] values);
	}

	public class SqliteDatabaseService(string databaseName) : IDatabaseService
	{
		private readonly string _databaseName = databaseName;

		public async Task<int> ExecuteNonQueryAsync(string sql, params (string, object)[] values)
		{
			try
			{
				using var connection = await SQLDatabaseManager.ConnectToSQLDatabase(_databaseName);
				using var command = new SqliteCommand(sql, connection);

				foreach (var (name, value) in values)
					command.Parameters.AddWithValue(name, value);

				await command.ExecuteNonQueryAsync();
				return 1;

			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"An error occurred when reading from database {_databaseName}.", ex);
			}
		}

		//public async Task<SqliteDataReader> ExecuteReaderAsync(string sql, params (string, object)[] values)
		//{
		//	List<object> list = [];
		//	try
		//	{
		//		using var connection = await SQLDatabaseManager.ConnectToSQLDatabase(_databaseName);
		//		using var command = new SqliteCommand(sql, connection);

		//		foreach (var (name, value) in values)
		//			command.Parameters.AddWithValue(name, value);

		//		var reader = await command.ExecuteReaderAsync();

		//		return reader;
		//	}
		//	catch (Exception ex)
		//	{
		//		throw new InvalidOperationException($"An error occurred when reading from database {_databaseName}.", ex);
		//	}
		//}

		public async Task<List<object>> ExecuteReaderAsync(string sql, Func<SqliteDataReader, List<object>> readFunction = null, params (string, object)[] values)
		{
			List<object> list = [];
			try
			{
				using var connection = await SQLDatabaseManager.ConnectToSQLDatabase(_databaseName);
				using var command = new SqliteCommand(sql, connection);

				foreach (var (name, value) in values)
					command.Parameters.AddWithValue(name, value);

				var reader = await command.ExecuteReaderAsync();

				if (!reader.HasRows)
					return list;

				// If the reader itself is returned from this method, it gets disposed of, even with managing its lifetime in the caller
				// of this method. So, this is a workaround to be able to get data from the reader still.
				while (await reader.ReadAsync())
					list = readFunction(reader);

				return list;
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"An error occurred when reading from database {_databaseName}.", ex);
			}
		}

		public async Task<bool> ExecuteReaderHasRowsAsync(string sql, params (string, object)[] values)
		{
			try
			{
				using var connection = await SQLDatabaseManager.ConnectToSQLDatabase(_databaseName);
				using var command = new SqliteCommand(sql, connection);

				foreach (var (name, value) in values)
					command.Parameters.AddWithValue(name, value);

				var reader = await command.ExecuteReaderAsync();
				return reader.HasRows;
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"An error occurred when reading from database {_databaseName}.", ex);
			}
		}
	}
}
