using Myxomatosis.Connection;
using Myxomatosis.Tests.Helpers;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Myxomatosis.Tests
{
    //[TestFixture]
    //public class QueueConnectionTests
    //{
    //    private const string QueueName = "Queue";
    //    private const string Exchange = "Ex";
    //    private FactoryHelper _factoryHelper;

    //    [SetUp]
    //    public void Init()
    //    {
    //        _factoryHelper = new FactoryHelper();
    //    }

    //    [Test]
    //    public void OpenConnectionClosesSuccessfully()
    //    {
    //        IQueueConnection connection;

    //        using (connection = _factoryHelper
    //            .GetListener()
    //            .Queue(QueueName)
    //            .Open())
    //        {
    //            Assert.IsTrue(connection.IsOpen);
    //        }

    //        Assert.IsFalse(connection.IsOpen);
    //    }

    //    [Test]
    //    public void OpenConnectionTakesTooLongToClose()
    //    {
    //        IQueueConnection closeResult;

    //        using (closeResult = _factoryHelper
    //            .SetProcessDuration(TimeSpan.FromSeconds(3))
    //            .GetListener()
    //            .Queue(QueueName)
    //            .Open(c => c.CloseTimeout(TimeSpan.FromSeconds(1)))) ;

    //        Assert.IsTrue(closeResult.IsOpen);
    //    }

    //    [Test]
    //    public void SubscribeToOpenConnection()
    //    {
    //        IDisposable subscription;

    //        using (subscription = _factoryHelper
    //            .GetListener()
    //            .Queue(QueueName)
    //            .Open(c => c.CloseTimeout(TimeSpan.FromSeconds(10)))
    //            .Stream()
    //            .Subscribe(m => { })) ;

    //        Assert.IsNotNull(subscription);
    //    }

    //    [Test]
    //    public void CloseOpenConnectionWithinTimeout()
    //    {
    //        IQueueConnection closeResult;

    //        using (closeResult = _factoryHelper
    //            .SetOpenDuration(TimeSpan.FromSeconds(3))
    //            .SetProcessDuration(TimeSpan.FromSeconds(1))
    //            .GetListener()
    //            .Queue(QueueName)
    //            .Open(c => c.CloseTimeout(TimeSpan.FromSeconds(3)))) ;

    //        Assert.IsFalse(closeResult.IsOpen);
    //    }

    //    [Test]
    //    public void CloseOpenConnectionNotWithinTimeout()
    //    {
    //        IQueueConnection connection;

    //        using (connection = _factoryHelper
    //            .SetOpenDuration(TimeSpan.FromSeconds(3))
    //            .SetProcessDuration(TimeSpan.FromSeconds(5))
    //            .GetListener()
    //            .Queue(QueueName)
    //            .Open(c => c.CloseTimeout(TimeSpan.FromSeconds(1)))) ;

    //        Assert.IsTrue(connection.IsOpen);
    //    }

    //    [Test]
    //    public void CloseOpenConnectionNotWithinTimeoutButIsClosedAfterTimeout()
    //    {
    //        IQueueConnection openConnection;

    //        using (openConnection = _factoryHelper
    //            .SetOpenDuration(TimeSpan.FromSeconds(3))
    //            .SetProcessDuration(TimeSpan.FromSeconds(5))
    //            .GetListener()
    //            .Queue(QueueName)
    //            .Open(c => c.CloseTimeout(TimeSpan.FromSeconds(1)))) ;

    //        Assert.IsTrue(openConnection.IsOpen);

    //        Task.Delay(TimeSpan.FromSeconds(6)).Wait();

    //        Assert.IsFalse(openConnection.IsOpen);
    //    }
    //}
}