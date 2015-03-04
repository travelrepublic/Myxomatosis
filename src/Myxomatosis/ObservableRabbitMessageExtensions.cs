using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Myxomatosis.Connection;
using Myxomatosis.Connection.Message;

namespace Myxomatosis
{
    public static class ObservableRabbitMessageExtensions
    {
        private static IDisposable InternalSubscribe<T>(this IObservable<T> source, Action<T> action)
            where T : IRabbitMessageModel
        {
            return source.Subscribe(rm =>
            {
                try
                {
                    action(rm);
                    rm.Acknowledge();
                }
                catch (Exception exception)
                {
                    rm.Error(exception);
                    rm.Acknowledge();
                    throw;
                }
            });
        }

        public static IDisposable SubscribeWithAck<T>(this IObservable<T> source, Action<T> action)
            where T : IRabbitMessageModel
        {
            return source.InternalSubscribe(action);
        }

        public static IDisposable SubscribeWithAck<T>(this IObservable<IEnumerable<T>> messageSource, Action<IEnumerable<T>> action) where T : IRabbitMessageModel
        {
            return messageSource.Select(m => new AggregateRabbitMessage<T>(m)).InternalSubscribe<AggregateRabbitMessage<T>>(action);
        }

        public static IDisposable SubscribeOnQueueToMessage<T>(this IObservableConnection connection, string exchange, string subscriberQueue, IRabbitMessageHandler<T> handler)
        {
            return connection.SubscribeOnQueueToMessage(new DefaultSubscriptionConfig(exchange, subscriberQueue), handler);
        }

        public static IDisposable SubscribeOnQueueToMessage<T>(this IObservableConnection connection, ISubscriptionConfig config, IRabbitMessageHandler<T> handler)
        {
            return connection.GetQueue<T>(config.Exchange, config.QueueName)
                .Listen(config.OpenTimeout)
                .ToObservable()
                .Pace(config.Interval).SubscribeWithAck(rm => { handler.Handle(rm.Message); });
        }

        public static IDisposable SubscribeOnQueueToMessage<T>(this IObservableConnection connection, IBatchSubscriptionConfig config, IRabbitMessageHandler<IEnumerable<T>> handler)
        {
            return SubscribeWithAck(connection.GetQueue<T>(config.Exchange, config.QueueName)
                .Listen(config.OpenTimeout)
                .ToObservable()
                .Buffer(config.BufferTimeout, config.BufferSize)
                .Pace(config.Interval)
                .Where(m => m.Any())
                .Select(l => l.AsEnumerable()), l =>
                {
                    var rabbitMessages = l.ToArray();
                    handler.Handle(rabbitMessages.Select(m => m.Message));
                });
        }

        #region Nested type: AggregateRabbitMessage

        private class AggregateRabbitMessage<T> : IRabbitMessageModel, IEnumerable<T> where T : IRabbitMessageModel
        {
            private readonly IEnumerable<T> _messageModels;

            #region Constructors

            public AggregateRabbitMessage(IEnumerable<T> messageModels)
            {
                _messageModels = messageModels;
            }

            #endregion Constructors

            #region IEnumerable<T> Members

            public IEnumerator<T> GetEnumerator()
            {
                return _messageModels.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            #endregion IEnumerable<T> Members

            #region IRabbitMessageModel Members

            public void Acknowledge()
            {
                foreach (var rabbitMessageModel in _messageModels)
                {
                    rabbitMessageModel.Acknowledge();
                }
            }

            public void Error()
            {
                foreach (var rabbitMessageModel in _messageModels)
                {
                    rabbitMessageModel.Error();
                }
            }

            public void Error(Exception exception)
            {
                foreach (var rabbitMessageModel in _messageModels)
                {
                    rabbitMessageModel.Error(exception);
                }
            }

            #endregion IRabbitMessageModel Members
        }

        #endregion Nested type: AggregateRabbitMessage
    }
}