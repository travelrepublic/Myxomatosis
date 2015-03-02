using NUnit.Framework;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using TravelRepublic.RxRabbitMQClient.Connection;
using TravelRepublic.RxRabbitMQClient.Connection.Message;
using TravelRepublic.RxRabbitMQClient.Connection.Queue;

namespace TravelRepublic.RxRabbitMQClient.Tests
{
    [TestFixture]
    internal class IntegrationTests
    {
        private IListeningConnection<MyMessage> _subscription;

        private IRabbitQueue<MyMessage> _queueConnection;

        [SetUp]
        public void Init()
        {
            _queueConnection = ObservableConnectionFactory.Create()
                .GetQueue<MyMessage>("TestExchange", "TestQueue");

            Enumerable.Range(0, 10).ToList().ForEach(i => { _queueConnection.Publish(new MyMessage { Message = "Message: " + i }); });
        }

        [Test]
        public void SimpleTest()
        {
            _subscription.MessageSource
                .SimpleSubscribe(rm => { Console.WriteLine("Recieved message: " + rm.Message.Message); });

            Task.Delay(TimeSpan.FromSeconds(5));

            _subscription.Close(TimeSpan.FromSeconds(1));
        }

        [Test]
        public void Test()
        {
            _subscription = _queueConnection.Listen(TimeSpan.FromSeconds(1), Filter);
            _subscription
            .MessageSource
            .SimpleSubscribe(rm => { Console.WriteLine("Recieved message: " + rm.Message.Message); });

            Task.Delay(TimeSpan.FromSeconds(5));

            _subscription.Close(TimeSpan.FromSeconds(1));
        }

        private IObservable<RabbitMessage> Filter(IObservable<RabbitMessage> stream)
        {
            return stream.Take(5);
        }
    }

    public class MyMessage
    {
        public string Message { get; set; }
    }
}