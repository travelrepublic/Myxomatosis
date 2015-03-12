using Myxomatosis.Connection.Errors;
using Myxomatosis.Connection.Message;
using Myxomatosis.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
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

        private void SubscribeToQueue(QueueSubscription subscription, TaskCompletionSource<object> taskCompletionSource)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                using (var model = connection.CreateModel())
                {
                    try
                    {
                        var consumer = new QueueingBasicConsumer(model);
                        model.ConfirmSelect(); //try publisher confirms

                        var queueName = subscription.SubscriptionData.Queue;

                        model.DeclareQueue(queueName);

                        model.BasicQos(0, subscription.SubscriptionData.PrefetchCount, false); //TODO: make number of concurrent messages retired configurable
                        model.BasicConsume(queueName, false, consumer);
                        subscription.OpenEvent.Set();

                        _logger.LogTrace("Subscription set up with Channel Hash {0}", model.GetHashCode());

                        while (subscription.KeepListening)
                        {
                            BasicDeliverEventArgs basicDeliverEventArgs;
                            if (!consumer.Queue.Dequeue((int)TimeSpan.FromSeconds(5).TotalMilliseconds, out basicDeliverEventArgs))
                                continue;

                            _logger.LogTrace("Dequeued message with delivery tag {0}", basicDeliverEventArgs.DeliveryTag);
                            var body = basicDeliverEventArgs.Body;
                            var headers = basicDeliverEventArgs.BasicProperties.Headers;
                            var message = new RabbitMessage
                            {
                                RawMessage = body,
                                RawHeaders = headers.ToDictionary(kvp => kvp.Key, kvp => kvp.Value as byte[]),
                                Channel = model,
                                DeliveryTag = basicDeliverEventArgs.DeliveryTag,
                                ErrorHandler = _errorHandler
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
        }

        #endregion IRabbitMqSubscriber Members
    }

    public enum TaskCompletion
    {
        Stopped,
        Error
    }
}