using NLog;

namespace Swsu.Tools.DbBackupper.Service
{
    public static class Helper
    {
        public static ILogger Logger { get; }

        static Helper()
        {
            Logger = LogManager.GetCurrentClassLogger();
        }
    }
}