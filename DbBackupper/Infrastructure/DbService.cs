using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using Swsu.Tools.DbBackupper.ViewModel;

namespace Swsu.Tools.DbBackupper.Infrastructure
{
	public static class DbService
	{
		public static Task CreateDatabaseAsync(NpgsqlConnectionStringBuilder connectionStringBuilder, string name)
		{
			return Task.Run(() => CreateDatabase(connectionStringBuilder, name));
		}

		private static void CreateDatabase(NpgsqlConnectionStringBuilder builder, string name)
		{
			var newBuilder = GetConnectionStringBuilderCopy(builder);

			newBuilder.Database = "postgres";

			using (var con = new NpgsqlConnection(newBuilder))
			{
				con.Open();
				
				using (var cmd = con.CreateCommand())
				{
					cmd.Connection = con;
					cmd.CommandText = $"DROP DATABASE IF EXISTS {name}; " +
					                  $"CREATE DATABASE {name} " +
					                  "WITH OWNER = postgres " +
					                  "ENCODING = 'UTF-8' " +
					                  "TABLESPACE = pg_default " +
					                  "LC_COLLATE = 'Russian_Russia.1251' " +
					                  "LC_CTYPE = 'Russian_Russia.1251' " +
					                  "CONNECTION LIMIT = -1";
					cmd.ExecuteNonQuery();
				}
			}
		}

		public static Task<List<string>> GetDatabasesAsync(NpgsqlConnectionStringBuilder builder)
		{
			return Task.Run(() => GetDatabases(builder));
		}

		private static List<string> GetDatabases(NpgsqlConnectionStringBuilder builder)
		{
			var databases = new List<string>();

			using (var con = new NpgsqlConnection(builder))
			{
				con.Open();

				using (var cmd = con.CreateCommand())
				{
					cmd.CommandText = "SELECT datname FROM pg_database WHERE datistemplate = false ORDER BY datname";

					using (var reader = cmd.ExecuteReader())
					{
						if (!reader.HasRows) return databases;

						while (reader.Read())
							databases.Add(reader.GetString(0));
						
					}
				}
			}
			return databases;
		}

		public static Task<List<Node>> GetDbSchemaObjectsAsync(NpgsqlConnectionStringBuilder builder, string dbName)
		{
			return Task.Run(() => GetDbSchemaObjects(builder, dbName));
		}

		private static List<Node> GetDbSchemaObjects(NpgsqlConnectionStringBuilder builder, string dbName)
		{
			var objects = new List<Node>();

			using (var con = new NpgsqlConnection(builder))
			{
				con.Open();

				using (var cmd = con.CreateCommand())
				{
					cmd.Connection = con;
					cmd.CommandText =
						"SELECT DISTINCT table_schema, table_name " +
						"FROM information_schema.tables " +
						$"WHERE table_catalog = '{dbName}' AND table_schema <> 'pg_catalog' AND table_schema <> 'information_schema' " +
						"ORDER BY table_schema, table_name";

					using (var reader = cmd.ExecuteReader())
					{
						if (!reader.HasRows) return objects;

						var prevSchemaName = string.Empty;
						Node curSchema = null;

						while (reader.Read())
						{
							var schemaName = reader.GetString(0);
							var tableName = reader.GetString(1);

							if (prevSchemaName != schemaName)
							{
								curSchema = new Node(schemaName);
								objects.Add(curSchema);
								prevSchemaName = schemaName;
							}
							
							curSchema?.Children.Add(new Node(tableName));
						}
					}
				}
			}
			return objects;
		}

		private static NpgsqlConnectionStringBuilder GetConnectionStringBuilderCopy(NpgsqlConnectionStringBuilder builder)
		{
			if (builder == null) throw new NullReferenceException("Ошибка получения копии NpgsqlConnectionStringBuilder");
			return new NpgsqlConnectionStringBuilder(builder.ConnectionString);
		}
	}
}