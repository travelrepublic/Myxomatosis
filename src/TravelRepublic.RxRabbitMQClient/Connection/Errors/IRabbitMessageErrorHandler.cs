using System;
using TravelRepublic.RxRabbitMQClient.Connection.Message;

namespace TravelRepublic.RxRabbitMQClient.Connection.Errors
{
    public interface IRabbitMessageErrorHandler
    {
        void Error(RabbitMessage message);

        void Error(RabbitMessage message, Exception exception);
    }
}