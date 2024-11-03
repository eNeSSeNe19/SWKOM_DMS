using System;

namespace SWKOM_DMS.logging
{
    public interface ILoggerWrapper
    {
        void Debug(string message);
        void Error(string message, Exception ex);
        void Fatal(string message);
        void Warn(string message);
        void Info(string message);
    }
}
