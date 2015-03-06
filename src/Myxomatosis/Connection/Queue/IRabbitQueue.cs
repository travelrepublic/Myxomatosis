using Myxomatosis.Connection.Message;
using System;
using System.Collections.Generic;

namespace Myxomatosis.Connection.Queue
{
    public delegate IObservable<RabbitMessage> StreamTransform(IObservable<RabbitMessage> stream);

    public interface IRabbitQueue<T>
    {
        IListeningConnection<T> Listen();

        IListeningConnection<T> Listen(TimeSpan openTimeout);

        IListeningConnection<T> Listen(TimeSpan openTimeout, StreamTransform transform);
    }

    public interface IRabbitQueue
    {
        IListeningConnection Listen();

        IListeningConnection Listen(TimeSpan openTimeout);

        IListeningConnection Listen(TimeSpan openTimeout, StreamTransform transform);
    }

    public interface IRabbitExchange
    {
        void Publish(byte[] payload, string routingKey);

        void Publish(byte[] payload, string routingKey, IDictionary<string, object> headers);
    }

    public interface IRabbitExchange<T>
    {
        void Publish(T message, string routingKey);

        void Publish(T message, string routingKey, IDictionary<string, object> headers);
    }
}