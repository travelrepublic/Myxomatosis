using System;
using System.Collections.Generic;
using TravelRepublic.RxRabbitMQClient.Connection.Message;

namespace TravelRepublic.RxRabbitMQClient.Connection.Queue
{
    public delegate IObservable<RabbitMessage> StreamTransform(IObservable<RabbitMessage> stream);

    public interface IRabbitQueue<T>
    {
        IListeningConnection<T> Listen();

        IListeningConnection<T> Listen(TimeSpan openTimeout);

        IListeningConnection<T> Listen(TimeSpan openTimeout, StreamTransform transform);

        void Publish(T message);

        void Publish(T message, IDictionary<string, object> headers);
    }

    public interface IRabbitQueue
    {
        IListeningConnection Listen();

        IListeningConnection Listen(TimeSpan openTimeout);

        IListeningConnection Listen(TimeSpan openTimeout, StreamTransform transform);

        void Publish(byte[] payload);
    }
}