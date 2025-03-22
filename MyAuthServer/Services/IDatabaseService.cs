using Microsoft.Data.Sqlite;
using MyAuthServer.SQL;

namespace MyAuthServer.Services
{
	/// <summary>
	/// Provides various Sqlite database related operations.
	/// </summary>
	public interface IDatabaseService
	{
		/// <summary>
		/// Executes a query.
		/// </summary>
		/// <param name="sql">The query to execute.</param>
		/// <param name="values">The parameters and their values to set.</param>
		Task<int> ExecuteNonQueryAsync(string sql, params (string parameterName, object value)[] values);

		/// <summary>
		/// Executes a data reader, allowing reading of database data.
		/// </summary>
		/// <param name="sql">The query to execute.</param>
		/// <param name="readDelegate">A function to read and extract values from the database.</param>
		/// <param name="values">The parameters and their values to set.</param>
		/// <returns>The list of objects create in <paramref name="readDelegate"/>.</returns>
		Task<List<object>> ExecuteReaderAsync(string sql, Func<SqliteDataReader, List<object>> readDelegate = null, params (string parameterName, object value)[] values);

		/// <summary>
		/// Executes a data reader, and returns if there were any rows found.
		/// </summary>
		/// <param name="sql">The query to execute.</param>
		/// <param name="values">The parameters and their values to set.</param>
		Task<bool> ExecuteReaderHasRowsAsync(string sql, params (string parameterName, object value)[] values);
	}

	/// <summary>
	/// Main implementation of <see cref="IDatabaseService"/>. Should be accessed via <see cref="IDatabaseServiceFactory"/>
	/// </summary>
	/// <param name="databaseName"></param>
	public class SqliteDatabaseService(string databaseName) : IDatabaseService
	{
		private readonly string _databaseName = databaseName;

		public async Task<int> ExecuteNonQueryAsync(string sql, params (string parameterName, object value)[] values)
		{
			try
			{
				using var connection = await SQLDatabaseManager.ConnectToSQLDatabase(_databaseName);
				using var command = new SqliteCommand(sql, connection);

				foreach (var (parameterName, value) in values)
					command.Parameters.AddWithValue(parameterName, value);

				await command.ExecuteNonQueryAsync();
				return 1;

			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"An error occurred when reading from database {_databaseName}.", ex);
			}
		}

		public async Task<List<object>> ExecuteReaderAsync(string sql, Func<SqliteDataReader, List<object>> readFunction = null, params (string parameterName, object value)[] values)
		{
			List<object> list = [];
			try
			{
				using var connection = await SQLDatabaseManager.ConnectToSQLDatabase(_databaseName);
				using var command = new SqliteCommand(sql, connection);

				foreach (var (parameterName, value) in values)
					command.Parameters.AddWithValue(parameterName, value);

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

		public async Task<bool> ExecuteReaderHasRowsAsync(string sql, params (string parameterName, object value)[] values)
		{
			try
			{
				using var connection = await SQLDatabaseManager.ConnectToSQLDatabase(_databaseName);
				using var command = new SqliteCommand(sql, connection);

				foreach (var (parameterName, value) in values)
					command.Parameters.AddWithValue(parameterName, value);

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
