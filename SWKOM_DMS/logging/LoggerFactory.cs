using System.Diagnostics;

namespace SWKOM_DMS.logging
{
    public class LoggerFactory
    {
        public static ILoggerWrapper GetLogger()
        {
            return new Log4NetWrapper();
        }
    }
}
