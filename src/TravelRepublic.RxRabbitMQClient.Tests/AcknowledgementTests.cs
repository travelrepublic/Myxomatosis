using NUnit.Framework;
using System;
using TravelRepublic.RxRabbitMQClient.Tests.Helpers;

namespace TravelRepublic.RxRabbitMQClient.Tests
{
    [TestFixture]
    public class AcknowledgementTests
    {
        private const string QueueName = "Queue";
        private const string Exchange = "Exchagne";
        private FactoryHelper _factoryHelper;

        [SetUp]
        public void Init()
        {
            _factoryHelper = new FactoryHelper();
        }

        [Test]
        public void SubscribeToOpenConnection()
        {
            var subscription = _factoryHelper
                .GetListener()
                .GetQueue(Exchange, QueueName)
                .Listen(TimeSpan.FromSeconds(10))
                .ToObservable()
                .SimpleSubscribe(m => { });

            Assert.IsNotNull(subscription);
        }
    }
}