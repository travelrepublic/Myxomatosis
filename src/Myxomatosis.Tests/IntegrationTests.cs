using Myxomatosis.Connection;
using Myxomatosis.Connection.Message;
using Myxomatosis.Logging;
using NUnit.Framework;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Myxomatosis.Tests
{
    [TestFixture]
    internal class IntegrationTests
    {
        private string _exchange;
        private string _queue;
        private IQueueOpener _queueConnection;
        private string _routingKey;
        private IQueueConnection _subscription;

        [SetUp]
        public void Init()
        {
            _exchange = "TestExchange";
            _queue = "TestQueue";
            _routingKey = "RouteMe";

            var rabbitConnection = ObservableConnectionFactory.Create(f => f.WithLogger(new RabbitMqConsoleLogger())
                .WithUserName("test")
                .WithPassword("test")
                .WithHostName(Environment.MachineName));

            rabbitConnection.SetUp(s => s.Exchange("EVERYTHING").Topic.BoundToQueue("LEFTOVERS"));
            rabbitConnection.SetUp(s => s.Exchange(_exchange).Fanout
                .BoundToQueue(_queue)
                .BoundToExchange("EVERYTHING"));

            _queueConnection = rabbitConnection.Queue(_queue);

            Enumerable.Range(0, 10).ToList().ForEach(i =>
            {
                Console.WriteLine("Publishing {0}", i);
                rabbitConnection.Exchange(_exchange)
                    .Publish(new MyMessage {Greeting = "Message: " + i});
            });
        }

        [Test]
        public void Test()
        {
            _subscription = _queueConnection.Open(c => c.OpenTimeout(TimeSpan.FromSeconds(1)).PrefetchCount(5));

            Task.Factory.StartNew(() =>
            {
                var observable = _subscription.Stream<MyMessage>();
                observable.Count().Subscribe(i => { Console.WriteLine("Count changed"); });

                using (observable
                    .Pace(TimeSpan.FromSeconds(5))
                    .SubscribeWithAck(rm =>
                    {
                        Console.WriteLine("Recieved message: " + rm.Message.Greeting);
                        Console.WriteLine(observable.Count());
                    })) ;
            });

            Task.Delay(TimeSpan.FromSeconds(5)).Wait();
        }

        [Test]
        public void ApiTest()
        {
            var connection = ObservableConnectionFactory.Create();

            connection.SetUp(s => s.Exchange(_exchange).Fanout.BoundToQueue(_queue));

            var queueconnection = connection.Queue("MessageQueue");
            var exchange = connection.Exchange(_exchange);

            Enumerable.Range(0, 100).ToList().ForEach(i => exchange.Publish(new MyMessage
            {
                Greeting = string.Format("Hello message: {0}", i)
            }));

            queueconnection
                .Open()
                .Stream<MyMessage>();
        }

        private IObservable<RabbitMessage> Filter(IObservable<RabbitMessage> stream)
        {
            return stream.Take(5);
        }
    }

    public class MyMessage
    {
        public string Greeting { get; set; }
    }

    public class NullLogger : IRabbitMqClientLogger
    {
        #region IRabbitMqClientLogger Members

        public void LogTrace(string message, params object[] args)
        {
        }

        public void LogInfo(string message, params object[] args)
        {
        }

        public void LogWarn(string message, params object[] args)
        {
        }

        public void LogError(string message, params object[] args)
        {
        }

        public void LogError(string message, Exception exception)
        {
        }

        #endregion IRabbitMqClientLogger Members
    }
}