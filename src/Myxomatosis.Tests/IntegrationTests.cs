using Myxomatosis.Connection;
using Myxomatosis.Connection.Message;
using Myxomatosis.Connection.Queue;
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
        private string _routingKey;

        private IRabbitQueue<MyMessage> _queueConnection;
        private IListeningConnection<MyMessage> _subscription;

        [SetUp]
        public void Init()
        {
            _exchange = "TestExchange";
            _queue = "TestQueue";
            _routingKey = "RouteMe";

            var rabbitConnection = ObservableConnectionFactory.Create(f => f.WithLogger(new RabbitMqConsoleLogger()));

            _queueConnection = rabbitConnection.GetQueue<MyMessage>(_exchange, _queue, _routingKey);

            Enumerable.Range(0, 10).ToList().ForEach(i =>
            {
                rabbitConnection.GetExchange<MyMessage>(_exchange)
                    .Publish(new MyMessage { Greeting = "Message: " + i }, _routingKey);
            });
        }

        [Test]
        public void Test()
        {
            _subscription = _queueConnection.Listen(TimeSpan.FromSeconds(1));

            Task.Factory.StartNew(() =>
            {
                _subscription
                    .ToObservable()
                    .SubscribeWithAck(rm => { Console.WriteLine("Recieved message: " + rm.Message.Greeting); });
            });

            Task.Delay(TimeSpan.FromSeconds(5)).Wait();

            _subscription.Close(TimeSpan.FromSeconds(1));
        }

        [Test]
        public void ApiTest()
        {
            //            var queueconnection = ObservableConnectionFactory.Create().GetQueue<MyMessage>("MessageExchange", "MessageQueue");
            //
            //            Enumerable.Range(0, 100).ToList().ForEach(i => queueconnection.Publish(new MyMessage
            //            {
            //                Greeting = string.Format("Hello message: {0}", i)
            //            }));

            //            queueconnection
            //                .Listen()
            //                .ToObservable()
            //                .ToMessage<MyMessage>();
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
    }
}