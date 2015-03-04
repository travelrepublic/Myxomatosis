using System;
using System.Threading.Tasks;
using Myxomatosis.Api;
using Myxomatosis.Connection;
using Myxomatosis.Connection.Queue.Listen;
using Myxomatosis.Serialization;

namespace Myxomatosis.Tests.Helpers
{
    public class FactoryHelper
    {
        private readonly MockSubscriberThread _supplierThreadMock;

        #region Constructors

        public FactoryHelper()
        {
            _supplierThreadMock = new MockSubscriberThread(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3));
            SetOpenDuration(TimeSpan.FromSeconds(1));
        }

        #endregion Constructors

        public FactoryHelper SetOpenDuration(TimeSpan timeSpan)
        {
            _supplierThreadMock.OpenTimeout = timeSpan;
            return this;
        }

        public FactoryHelper SetProcessDuration(TimeSpan timeSpan)
        {
            _supplierThreadMock.ProcessingInterval = timeSpan;
            return this;
        }

        public IObservableConnection GetListener()
        {
            return new Listener(_supplierThreadMock, null, new SubscriptionManager(), DefaultSerializer.Instance);
        }

        #region Nested type: MockSubscriberThread

        private class MockSubscriberThread : IRabbitMqSubscriber
        {
            #region Constructors

            public MockSubscriberThread(TimeSpan openTimeout, TimeSpan processingInterval)
            {
                OpenTimeout = openTimeout;
                ProcessingInterval = processingInterval;
            }

            #endregion Constructors

            public TimeSpan OpenTimeout { get; set; }

            public TimeSpan ProcessingInterval { get; set; }

            #region IRabbitMqSubscriber Members

            public void Subscribe(QueueSubscription subscription)
            {
                Task.Delay(OpenTimeout).Wait();
                subscription.OpenEvent.Set();

                while (subscription.KeepListening)
                {
                    Task.Delay(ProcessingInterval).Wait();
                }
            }

            #endregion IRabbitMqSubscriber Members
        }

        #endregion Nested type: MockSubscriberThread
    }
}