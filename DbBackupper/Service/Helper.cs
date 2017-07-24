using System;
using NLog;
using Npgsql;

namespace Swsu.Tools.DbBackupper.Service
{
    public static class Helper
    {
        public static ILogger Logger { get; }

        static Helper()
        {
            Logger = LogManager.GetCurrentClassLogger();
        }

	    public static string ParseErrorCode(PostgresException dbe)
	    {
		    if (string.IsNullOrEmpty(dbe.SqlState)) return "Unknown exception";

		    switch (dbe.SqlState)
		    {
				case "28P01":
				    return Resources.PostgresErrorCodes.InvalidPassword;


				default:
				    return "Unknown exception";
		    }
	    }
    }
}