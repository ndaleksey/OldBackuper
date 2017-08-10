using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
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
					cmd.CommandText = $"DROP DATABASE IF EXISTS \"{name}\"; " +
					                  $"CREATE DATABASE \"{name}\" " +
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

			var newBuilder = GetConnectionStringBuilderCopy(builder);

			newBuilder.Database = "postgres";

			using (var con = new NpgsqlConnection(newBuilder))
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
			if (builder == null) throw new NullReferenceException(Resources.Messages.ConnectionBuilderGettingError);

			return new NpgsqlConnectionStringBuilder(builder.ConnectionString);
		}

		public static Task<IEnumerable<Connection>> GetActiveConnectionsAsync(NpgsqlConnectionStringBuilder builder)
		{
			return Task.Run(() => GetActiveConnections(builder));
		}

		private static IEnumerable<Connection> GetActiveConnections(NpgsqlConnectionStringBuilder builder)
		{
			using (var con = new NpgsqlConnection(builder))
			{
				con.Open();

				using (var cmd = con.CreateCommand())
				{
					var currentPid = GetCurrentConnectionPid(con);

					cmd.CommandText =
						$"SELECT pid, client_addr::text FROM pg_stat_activity WHERE datname = '{builder.Database}' AND pid <> {currentPid}";
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							var pid = reader.GetInt32(0);
							var host = reader.GetString(1);
							yield return new Connection(pid, host);
						}
					}
				}
			}
		}

		private static int GetCurrentConnectionPid(NpgsqlConnection con)
		{
			using (var cmd = con.CreateCommand())
			{
				cmd.CommandText = "SELECT pg_backend_pid()";

				using (var reader = cmd.ExecuteReader())
				{
					reader.Read();

					return reader.GetInt32(0);
				}
			}
		}

		public static Task StopActiveConnectionAsync(NpgsqlConnectionStringBuilder builder, List<Connection> connections)
		{
			return Task.Run(() => StopActiveConnection(builder, connections));
		}

		private static void StopActiveConnection(NpgsqlConnectionStringBuilder builder,
			IReadOnlyCollection<Connection> connections)
		{
			if (connections == null || connections.Count == 0) return;

			using (var con = new NpgsqlConnection(builder))
			{
				con.Open();

				using (var cmd = con.CreateCommand())
				{
					var script = new StringBuilder();

					foreach (var connection in connections)

						script.Append(
							$"SELECT pg_terminate_backend({connection.Pid}), pg_cancel_backend({connection.Pid}), * FROM pg_stat_activity " +
							$"WHERE datname = '{builder.Database}'; ");

					cmd.CommandText = script.ToString();
					cmd.ExecuteNonQuery();
				}
			}
		}
	}

	public class Connection
	{
		#region Properties

		public string Host { get; }
		public int Pid { get; }

		#endregion

		#region Constructors

		public Connection(int pid, string host)
		{
			Pid = pid;
			Host = host;
		}

		#endregion
	}
}