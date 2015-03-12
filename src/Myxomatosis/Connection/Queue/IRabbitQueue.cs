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

    public interface IExchange
    {
        void Publish(byte[] message);

        void Publish(byte[] message, string routingKey);

        void Publish(byte[] message, IDictionary<string, byte[]> headers);

        void Publish(byte[] message, IDictionary<string, byte[]> headers, string routingKey);

        void Publish<T>(T message);

        void Publish<T>(T message, string routingKey);

        void Publish<T>(T message, IDictionary<string, object> headers);

        void Publish<T>(T message, IDictionary<string, object> headers, string routingKey);
    }
}