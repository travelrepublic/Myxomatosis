using System;

namespace TravelRepublic.RxRabbitMQClient.Connection.Message
{
    public interface IRabbitMessageModel
    {
        void Acknowledge();

        void Error();

        void Error(Exception exception);
    }
}