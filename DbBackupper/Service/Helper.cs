using System.Diagnostics;
using Npgsql;
using Swsu.Lignis.Logger.PrtAgent;

namespace Swsu.Tools.DbBackupper.Service
{
	public static class Helper
	{
		public static AppLogger Logger { get; }

		static Helper()
		{
			Logger = new AppLogger(null, Process.GetCurrentProcess().ProcessName, -1);
		}

		public static string ParseErrorCode(PostgresException dbe)
		{
			if (string.IsNullOrEmpty(dbe.SqlState)) return "Unknown exception";

			switch (dbe.SqlState)
			{
				case "08000":
					return Resources.PostgresErrorCodes.Err08000;

				case "08003":
					return Resources.PostgresErrorCodes.Err08003;

				case "08006":
					return Resources.PostgresErrorCodes.Err08006;

				case "28000":
					return Resources.PostgresErrorCodes.Err28000;

				case "28P01":
					return Resources.PostgresErrorCodes.Err28P01;

				case "3D000":
					return Resources.PostgresErrorCodes.Err3D000;

				case "3F000":
					return Resources.PostgresErrorCodes.Err3F000;

				case "42601":
					return Resources.PostgresErrorCodes.Err42601;

				case "57P03":
					return Resources.PostgresErrorCodes.Err57P03;

				default:
					return "Unknown exception";
			}
		}
	}
}