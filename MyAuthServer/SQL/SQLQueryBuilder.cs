namespace MyAuthServer.SQL
{
	public sealed class SQLQueryBuilder
	{
		private SQLQueryBuilder() { }

		private static SQLQueryBuilder instance;

		private string sql;

		public static SQLQueryBuilder Begin(string mainCommand, string table = null)
		{
			instance ??= new();
			instance.sql = $"{mainCommand}{(table != null ? $" {table}" : string.Empty)}";
			return instance;
		}

		public SQLQueryBuilder From(string tableName)
		{
			sql += $" {SQLKeywords.FROM} {tableName}";
			return this;
		}

		public SQLQueryBuilder IntoValues(string tableName, params string[] values)
		{
			sql += $" {SQLKeywords.INTO} {tableName}(";

			for (int i = 0; i < values.Length; i++)
				sql += $"{values[i]}{(i == values.Length - 1 ? "" : ", ")}";

			sql += ") VALUES (";

			for (int i = 0; i < values.Length; i++)
				sql += $"@{values[i]}{(i == values.Length - 1 ? "" : ", ")}";
			sql += ")";
			return this;
		}

		public SQLQueryBuilder Where(string parameter, string comparisonSign)
		{
			sql += $" {SQLKeywords.WHERE} {parameter} {comparisonSign} @{parameter}";
			return this;
		}

		public SQLQueryBuilder And(string parameter, string comparisonSign)
		{
			sql += $" {SQLKeywords.AND} {parameter} {comparisonSign} @{parameter}";
			return this;
		}

		public SQLQueryBuilder Set(string parameter)
		{
			sql += $" {SQLKeywords.SET} {parameter} {SQLKeywords.EQUALS} @{parameter}";
			return this;
		}

		public SQLQueryBuilder AdditionalSets(string parameter)
		{
			sql += $", {parameter} {SQLKeywords.EQUALS} @{parameter}";
			return this;
		}

		public string BuildAndClear() => sql + ";";

		public override string ToString() => BuildAndClear();
	}
}
