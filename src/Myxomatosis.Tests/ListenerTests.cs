using System;
using Myxomatosis.Tests.Helpers;
using NUnit.Framework;

namespace Myxomatosis.Tests
{
    [TestFixture]
    public class ListenerTests
    {
        private FactoryHelper _factory;
        private const string QueueName = "Queue";
        private const string Exchange = "Ex";

        [SetUp]
        public void Init()
        {
            _factory = new FactoryHelper();
        }

        [Test]
        public void OpensWithinTimeout()
        {
            var listener = _factory
                .SetOpenDuration(TimeSpan.FromSeconds(1))
                .GetListener();

            var open = listener
                .GetQueue(Exchange, QueueName)
                .Listen(TimeSpan.FromSeconds(3));
        }

        [Test]
        public void OpenAlreadyOpenedQueueReturnsImmediately()
        {
            var listener = _factory
                .SetOpenDuration(TimeSpan.FromSeconds(3))
                .GetListener();

            var open = listener.GetQueue(Exchange, QueueName).Listen(TimeSpan.FromSeconds(5));

            //Now demand an open connection with timeout less than the subscription duration
            open = listener.GetQueue(Exchange, QueueName).Listen(TimeSpan.FromSeconds(1));
        }

        [Test]
        [ExpectedException(typeof(Exception))]
        public void DoesNotOpenWithinTimeout()
        {
            var listener = _factory
                .SetOpenDuration(TimeSpan.FromSeconds(5))
                .GetListener();

            var open = listener.GetQueue(Exchange, QueueName).Listen(TimeSpan.FromSeconds(1));
        }



    }
}