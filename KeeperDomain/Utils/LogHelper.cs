using System;

namespace KeeperDomain
{
    public static class LogHelper
    {
        public static LogFile LogFile { get; set; }

        public static void AppendLine(string message)
        {
            LogFile.AppendLine(message);
        }

        public static void AppendLine(Exception e, string where)
        {
            LogFile.AppendLine(where + ":  " + e.Message);
            while (e.InnerException != null)
            {
                e = e.InnerException;
                AppendLine(e.Message);
            }

        }
    }
}
