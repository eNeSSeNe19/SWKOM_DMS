using log4net;
using log4net.Config;
using System;
using System.IO;
using System.Reflection;

namespace SWKOM_DMS.logging
{
    public class Log4NetWrapper : ILoggerWrapper
    {
        private readonly ILog _logger;

        //public Log4NetWrapper()
        //{
        //    var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
        //    XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

        //    _logger = LogManager.GetLogger(typeof(Log4NetWrapper));
        //}

        public Log4NetWrapper()
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "C:\\Users\\eNeSSeNe\\Desktop\\SWKOM_DMS\\SWKOM_DMS\\logging\\log4net.config");
            XmlConfigurator.Configure(logRepository, new FileInfo(configFilePath));

            _logger = LogManager.GetLogger(typeof(Log4NetWrapper));
        }


        public void Debug(string message)
        {
            // Check if the message contains large binary data and handle accordingly
            if (message.Contains("data:image"))
            {
                _logger.Debug("Image data detected, not logging the full data.");
            }
            else
            {
                _logger.Debug(message);
            }
        }

        //public void Debug(string message)
        //{
        //    _logger.Debug(message);
        //}

        public void Error(string message, Exception ex)
        {
            // Optionally, you can also handle exceptions similarly if they contain large data
            if (message.Contains("data:image"))
            {
                _logger.Error("Image data detected in error message, not logging the full data.", ex);
            }
            else
            {
                _logger.Error(message, ex);
            }
        }

        public void Info(string message)
        {
            Console.WriteLine($"Logging INFO: {message}");
            _logger.Info(message);
        }

        public void Warn(string message)
        {
            _logger.Warn(message);
        }

        public void Fatal(string message)
        {
            _logger.Fatal(message);
        }
    }
}
