using Myxomatosis.Connection.Errors;
using Myxomatosis.Connection.Message;
using Myxomatosis.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Myxomatosis.Connection.Queue.Listen
{
    /// <summary>
    /// This class holds the connection to RabbitMQ, and pushes messages on the the observable
    /// </summary>
    internal class RabbitMqQueueListener
    {
        private readonly ConnectionFactory _connectionFactory;
        private readonly IRabbitMessageErrorHandler _errorHandler;
        private readonly IRabbitMqClientLogger _logger;

        private Thread _consumingThread;
        private ManualResetEvent _consumingFinishEvent;

        #region Constructors

        public RabbitMqQueueListener(ConnectionFactory connectionFactory,
            IRabbitMessageErrorHandler errorHandler,
            IRabbitMqClientLogger logger)
        {
            _connectionFactory = connectionFactory;
            _errorHandler = errorHandler;
            _logger = logger;
            _consumingThread = null;
        }

        #endregion Constructors

        #region IRabbitMqSubscriber Members

        /// <summary>
        /// Blocking method which starts listening to the queue
        /// </summary>
        /// <param name="subscription"></param>
        public void Listen(QueueSubscriptionToken subscription)
        {
            ListenToQueue(subscription);
        }

        #endregion IRabbitMqSubscriber Members

        private void ListenToQueue(QueueSubscriptionToken subscription)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                using (var model = connection.CreateModel())
                {
                    var consumer = new QueueingBasicConsumer(model);
                    model.ConfirmSelect(); //try publisher confirms

                    var queueName = subscription.QueueName;

                    model.DeclareQueue(queueName, subscription.Args);

                    model.BasicQos(0, subscription.PrefetchCount, false);
                    model.BasicConsume(queueName, false, consumer);
                    subscription.OpenEvent.Set();

                    _logger.LogTrace("Subscription set up with Channel Hash {0}", model.GetHashCode());
                    subscription.KeepListening = true;

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
                subscription.ClosedEvent.Set();
            }
        }

        private void SubscribeToQueue(QueueSubscriptionToken subscription)
        {
            try
            {
                ListenToQueue(subscription);
            }
            catch (EndOfStreamException eosE)
            {
                SubscribeToQueue(subscription);
                _logger.LogWarn("EndOfStreamException caught.  Will try to reconnect...");
            }
            catch (Exception e)
            {
                _logger.LogError("Error in queue subscriber", e);
                return;
            }
            finally
            {
                subscription.MessageObserver.OnCompleted();
            }
        }
    }

    public enum TaskCompletion
    {
        Stopped,
        Error
    }
}