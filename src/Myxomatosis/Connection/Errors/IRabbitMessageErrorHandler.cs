using System;
using Myxomatosis.Connection.Message;

namespace Myxomatosis.Connection.Errors
{
    public interface IRabbitMessageErrorHandler
    {
        void Error(RabbitMessage message);

        void Error(RabbitMessage message, Exception exception);
    }
}