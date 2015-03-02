using System;
using System.Threading.Tasks;
using NUnit.Framework;
using TravelRepublic.RxRabbitMQClient.Tests.Helpers;

namespace TravelRepublic.RxRabbitMQClient.Tests
{
    [TestFixture]
    public class QueueConnectionTests
    {
        private FactoryHelper _factoryHelper;

        private const string QueueName = "Queue";
        private const string Exchange = "Ex";

        [SetUp]
        public void Init()
        {
            _factoryHelper = new FactoryHelper();
        }

        [Test]
        public void OpenConnectionClosesSuccessfully()
        {
            var closeResult = _factoryHelper
                .GetListener()
                .GetQueue(Exchange, QueueName)
                .Listen()
                .Close();

            Assert.IsTrue(closeResult.Successful);
        }

        [Test]
        public void OpenConnectionTakesTooLongToClose()
        {
            var closeResult = _factoryHelper
                .SetProcessDuration(TimeSpan.FromSeconds(3))
                .GetListener()
                .GetQueue(Exchange, QueueName)
                .Listen()
                .Close(TimeSpan.FromSeconds(1));

            Assert.IsFalse(closeResult.Successful);
        }

        [Test]
        public void SubscribeToOpenConnection()
        {
            var subscription = _factoryHelper
                .GetListener()
                .GetQueue(Exchange, QueueName)
                .Listen(TimeSpan.FromSeconds(10))
                .MessageSource
                .Subscribe(m => { });

            Assert.IsNotNull(subscription);
        }

        [Test]
        [ExpectedException(typeof(Exception))]
        public void SubscribeToUnOpenedConnectionThrowsException()
        {
            _factoryHelper
                .SetOpenDuration(TimeSpan.FromSeconds(3))
                .GetListener()
                .GetQueue(Exchange, QueueName)
                .Listen(TimeSpan.FromSeconds(1))
                .MessageSource
                .Subscribe(m => { });
        }

        [Test]
        public void CloseOpenConnectionWithinTimeout()
        {
            var closeResult = _factoryHelper
                .SetOpenDuration(TimeSpan.FromSeconds(3))
                .SetProcessDuration(TimeSpan.FromSeconds(1))
                .GetListener()
                .GetQueue(Exchange, QueueName)
                .Listen()
                .Close(TimeSpan.FromSeconds(3));

            Assert.IsTrue(closeResult.Successful);
        }

        [Test]
        public void CloseOpenConnectionNotWithinTimeout()
        {
            var closeResult = _factoryHelper
                .SetOpenDuration(TimeSpan.FromSeconds(3))
                .SetProcessDuration(TimeSpan.FromSeconds(5))
                .GetListener()
                .GetQueue(Exchange, QueueName)
                .Listen()
                .Close(TimeSpan.FromSeconds(1));

            Assert.IsFalse(closeResult.Successful);
        }

        [Test]
        public void CloseOpenConnectionNotWithinTimeoutButIsClosedAfterTimeout()
        {
            var openConnection = _factoryHelper
                .SetOpenDuration(TimeSpan.FromSeconds(3))
                .SetProcessDuration(TimeSpan.FromSeconds(5))
                .GetListener()
                .GetQueue(Exchange, QueueName)
                .Listen();

            var closeResult = openConnection.Close(TimeSpan.FromSeconds(1));

            Assert.IsFalse(closeResult.Successful);

            Task.Delay(TimeSpan.FromSeconds(4));

            Assert.IsFalse(openConnection.IsOpen);


        }

        [Test]
        [ExpectedException(typeof(Exception))]
        public void UnOpenedConnectionThrowsExceptionOnClose()
        {
            _factoryHelper
                .SetOpenDuration(TimeSpan.FromSeconds(3))
                .GetListener()
                .GetQueue(Exchange, QueueName)
                .Listen(TimeSpan.FromSeconds(1))
                .Close();
        }
    }
}