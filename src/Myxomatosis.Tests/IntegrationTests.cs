using Myxomatosis.Connection;
using Myxomatosis.Connection.Message;
using Myxomatosis.Connection.Queue;
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
        private IRabbitQueue<MyMessage> _queueConnection;
        private IListeningConnection<MyMessage> _subscription;

        [SetUp]
        public void Init()
        {
            _queueConnection = ObservableConnectionFactory.Create()
                .GetQueue<MyMessage>("TestExchange", "TestQueue");

            Enumerable.Range(0, 10).ToList().ForEach(i => { _queueConnection.Publish(new MyMessage { Greeting = "Message: " + i }); });
        }

        [Test]
        public void Test()
        {
            _subscription = _queueConnection.Listen(TimeSpan.FromSeconds(1), Filter);
            _subscription
                .ToObservable()
                .SubscribeWithAck(rm => { Console.WriteLine("Recieved message: " + rm.Message.Greeting); });

            Task.Delay(TimeSpan.FromSeconds(5));

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
}