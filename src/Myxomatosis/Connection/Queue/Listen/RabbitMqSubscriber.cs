using Myxomatosis.Connection.Errors;
using Myxomatosis.Connection.Message;
using Myxomatosis.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Myxomatosis.Connection.Queue.Listen
{
    internal class RabbitMqSubscriber : IRabbitMqSubscriber
    {
        private readonly ConnectionFactory _connectionFactory;
        private readonly IRabbitMessageErrorHandler _errorHandler;
        private readonly IRabbitMqClientLogger _logger;

        #region Constructors

        public RabbitMqSubscriber(ConnectionFactory connectionFactory,
            IRabbitMessageErrorHandler errorHandler,
            IRabbitMqClientLogger logger)
        {
            _connectionFactory = connectionFactory;
            _errorHandler = errorHandler;
            _logger = logger;
        }

        #endregion Constructors

        #region IRabbitMqSubscriber Members

        public async Task<object> SubscribeAsync(QueueSubscription subscription)
        {
            try
            {
                var taskCompletionSource = new TaskCompletionSource<object>();
                Task.Run(() => { SubscribeToQueue(subscription, taskCompletionSource); });
                return taskCompletionSource.Task;
            }
            catch (Exception e)
            {
                _logger.LogError("Error initializing connection", e);
                throw;
            }
        }

        #endregion IRabbitMqSubscriber Members

        private void ListenToQueue(QueueSubscription subscription)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                using (var model = connection.CreateModel())
                {
                    var consumer = new QueueingBasicConsumer(model);
                    model.ConfirmSelect(); //try publisher confirms

                    var queueName = subscription.SubscriptionData.Queue;

                    model.DeclareQueue(queueName);

                    model.BasicQos(0, subscription.SubscriptionData.PrefetchCount, false);
                    model.BasicConsume(queueName, false, consumer);
                    subscription.OpenEvent.Set();

                    _logger.LogTrace("Subscription set up with Channel Hash {0}", model.GetHashCode());

                    while (subscription.KeepListening)
                    {
                        BasicDeliverEventArgs basicDeliverEventArgs = null;
                        if (!consumer.Queue.Dequeue((int)TimeSpan.FromSeconds(5).TotalMilliseconds, out basicDeliverEventArgs))
                        {
                            if (basicDeliverEventArgs != null)
                                throw new Exception("Could not get message within timeout - expected event args to be null");
                            continue;
                        }

                        var body = basicDeliverEventArgs.Body;
                        var headers = basicDeliverEventArgs.BasicProperties.Headers;
                        var message = new RabbitMessage
                        {
                            Id = Guid.NewGuid(),
                            RawMessage = body,
                            RawHeaders = headers.ToDictionary(kvp => kvp.Key, kvp => kvp.Value as byte[]),
                            Channel = model,
                            DeliveryTag = basicDeliverEventArgs.DeliveryTag,
                            ErrorHandler = _errorHandler,
                        };
                        try
                        {
                            subscription.MessageObserver.OnNext(message);
                        }
                        catch (Exception e)
                        {
                            _errorHandler.Error(message, e);
                            subscription.MessageObserver.OnError(e);
                        }
                    }
                }
            }
        }

        private void SubscribeToQueue(QueueSubscription subscription, TaskCompletionSource<object> taskCompletionSource)
        {
            try
            {
                ListenToQueue(subscription);
            }
            catch (EndOfStreamException eosE)
            {
                SubscribeToQueue(subscription, taskCompletionSource);
                _logger.LogWarn("EndOfStreamException caught.  Will try to reconnect...");
            }
            catch (Exception e)
            {
                _logger.LogError("Error in queue subscriber", e);
                taskCompletionSource.SetException(e);
                return;
            }
            finally
            {
                subscription.MessageObserver.OnCompleted();
            }

            taskCompletionSource.SetResult(null);
        }
    }

    public enum TaskCompletion
    {
        Stopped,
        Error
    }
}