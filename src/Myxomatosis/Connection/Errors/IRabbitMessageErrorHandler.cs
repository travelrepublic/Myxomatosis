using System;
using Myxomatosis.Connection.Message;

namespace Myxomatosis.Connection.Errors
{
    public interface IRabbitMessageErrorHandler
    {
        void Error(RabbitMessage message, string errorExchange = null);

        void Error(RabbitMessage message, Exception exception, string errorExchange = null);
    }
}