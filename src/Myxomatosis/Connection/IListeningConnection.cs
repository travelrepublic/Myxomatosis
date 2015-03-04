using System;
using Myxomatosis.Connection.Message;

namespace Myxomatosis.Connection
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