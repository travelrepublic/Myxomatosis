using System;

namespace TravelRepublic.RxRabbitMQClient.Logging
{
    public interface IRabbitMqClientLogger
    {
        void LogTrace(string message, params object[] args);

        void LogInfo(string message, params object[] args);

        void LogWarn(string message, params object[] args);

        void LogError(string message, params object[] args);

        void LogError(string message, Exception exception);
    }
}