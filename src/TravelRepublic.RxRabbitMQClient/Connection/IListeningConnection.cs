using System;
using TravelRepublic.RxRabbitMQClient.Connection.Message;

namespace TravelRepublic.RxRabbitMQClient.Connection
{
    public interface IListeningConnection<T> : IOpenConnection
    {
        IObservable<RabbitMessage<T>> ToObservable();
    }

    public interface IListeningConnection : IOpenConnection
    {
        IObservable<RabbitMessage> ToObservable();
    }
}