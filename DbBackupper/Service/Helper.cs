using System;
using System.Diagnostics;
using System.Reflection;
using Npgsql;
using Swsu.Lignis.Logger.PrtAgent;
using Swsu.Lignis.SCMF.ModuleSCMF;

namespace Swsu.Tools.DbBackupper.Service
{
	public static class Helper
	{
		#region Properties

		private static AppLogger Logger { get; }
		private static ModuleSCMF ModuleScmf { get; }

		#endregion

		#region Properties

		static Helper()
		{
			var processName = Process.GetCurrentProcess().ProcessName;
			var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
			ModuleScmf = new ModuleSCMF(processName, version, Properties.Resources.ApplicationTitle);
			Logger = new AppLogger(null, processName, -1);
		}

		#endregion

		#region Methods
		
		public static void LogError(string message, Exception e)
		{
			Debug.WriteLine(message + "\n" + e);
			Logger.Error(Properties.Resources.LogSource, message, e);
			ModuleScmf.AddError(message + "\n" + e.Message);
		}

		public static void LogError(Exception e)
		{
			Debug.WriteLine(e);
			Logger.Error(Properties.Resources.LogSource, e);
			ModuleScmf.AddError(e.Message);
		}

		public static void LogInfo(string message)
		{
			Debug.WriteLine(message);
			Logger.Info(Properties.Resources.LogSource, message);
		}

		public static void LogFatal(Exception e)
		{
			Debug.WriteLine(e);
			Logger.Fatal(Properties.Resources.LogSource, e);
			ModuleScmf.AddError(e.Message);
		}

		public static void LogFatal(string message, Exception e)
		{
			Debug.WriteLine(message + "\n" + e);
			Logger.Fatal(Properties.Resources.LogSource, message, e);
			ModuleScmf.AddError(message + "\n" + e.StackTrace);
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

		#endregion
	}
}