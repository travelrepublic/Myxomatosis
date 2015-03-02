using System;
using System.Text;

namespace TravelRepublic.RxRabbitMQClient.Logging
{
    internal class RabbitMqConsoleLogger : IRabbitMqClientLogger
    {
        #region IRabbitMqClientLogger Members

        public void LogTrace(string message, params object[] args)
        {
            ColoredConsole.Log(ConsoleColor.Gray, message, args);
        }

        public void LogInfo(string message, params object[] args)
        {
            ColoredConsole.Log(ConsoleColor.Gray, message, args);
        }

        public void LogWarn(string message, params object[] args)
        {
            ColoredConsole.Log(ConsoleColor.Gray, message, args);
        }

        public void LogError(string message, params object[] args)
        {
            ColoredConsole.Log(ConsoleColor.Gray, message, args);
        }

        public void LogError(string message, Exception exception)
        {
            var exceptionBuilder = new StringBuilder()
                .AppendLine(message)
                .AppendLine("Exception Type: " + exception.GetType())
                .AppendLine("Exception Message: " + exception.Message)
                .AppendLine("Stack Trace: " + exception.StackTrace);

            ColoredConsole.Log(ConsoleColor.Red, exceptionBuilder);
        }

        #endregion IRabbitMqClientLogger Members

        #region Nested type: ColoredConsole

        private class ColoredConsole : IDisposable
        {
            #region Constructors

            private ColoredConsole(ConsoleColor colour)
            {
                Console.ForegroundColor = colour;
            }

            #endregion Constructors

            #region IDisposable Members

            public void Dispose()
            {
                Console.ResetColor();
            }

            #endregion IDisposable Members

            public static void Log(ConsoleColor colour, string message, params object[] args)
            {
                using (new ColoredConsole(colour)) Console.WriteLine("[" + DateTime.Now + "]:" + message, args);
            }

            public static void Log(ConsoleColor colour, object message)
            {
                using (new ColoredConsole(colour)) Console.WriteLine("[" + DateTime.Now + "]:" + message);
            }
        }

        #endregion Nested type: ColoredConsole
    }
}