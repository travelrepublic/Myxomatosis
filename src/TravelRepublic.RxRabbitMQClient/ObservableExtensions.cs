using System;
using System.Linq;
using System.Reactive.Linq;
using TravelRepublic.RxRabbitMQClient.Connection.Message;
using TravelRepublic.RxRabbitMQClient.Serialization;

namespace TravelRepublic.RxRabbitMQClient
{
    public static class ObservableExtensions
    {
        public static IObservable<T> Pace<T>(this IObservable<T> source, TimeSpan interval)
        {
            return source.Select(i => Observable.Empty<T>()
                .Delay(interval)
                .StartWith(i)).Concat();
        }

        public static IObservable<RabbitMessage<T>> ToMessage<T>(this IObservable<RabbitMessage> observable)
        {
            return ToMessage<T>(observable, DefaultSerializer.Instance);
        }

        public static IObservable<RabbitMessage<T>> ToMessage<T>(this IObservable<RabbitMessage> observable, ISerializer serializer)
        {
            return observable.Select(m => new RabbitMessage<T>
            {
                DeliveryTag = m.DeliveryTag,
                Channel = m.Channel,
                RawHeaders = m.RawHeaders,
                Headers = m.RawHeaders.ToDictionary(i => i.Key, i => serializer.Deserialize<object>(i.Value)),
                RawMessage = m.RawMessage,
                Message = serializer.Deserialize<T>(m.RawMessage),
                ErrorHandler = m.ErrorHandler
            });
        }
    }
}