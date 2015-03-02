using Castle.Core.Logging;
using System;
using System.Text;
using TravelRepublic.RxRabbitMQClient.Logging;

namespace TravelRepublic.RxRabbitMQClient.Windsor
{
    internal class CastleCoreLogger : IRabbitMqClientLogger
    {
        private readonly ILogger _logger;

        public CastleCoreLogger(ILogger logger)
        {
            _logger = logger;
        }

        public void LogTrace(string message, params object[] args)
        {
            _logger.DebugFormat(message, args);
        }

        public void LogInfo(string message, params object[] args)
        {
            _logger.InfoFormat(message, args);
        }

        public void LogWarn(string message, params object[] args)
        {
            _logger.WarnFormat(message, args);
        }

        public void LogError(string message, params object[] args)
        {
            _logger.ErrorFormat(message, args);
        }

        public void LogError(string message, Exception exception)
        {
            var exceptionBuilder = new StringBuilder()
                .AppendLine(message)
                .AppendLine("Exception Type: " + exception.GetType())
                .AppendLine("Exception Message: " + exception.Message)
                .AppendLine("Stack Trace: " + exception.StackTrace)
                .ToString();

            _logger.Error(exceptionBuilder, exception);
        }
    }
}