using System;

namespace Myxomatosis.Windsor.Attributes
{
    public interface ISubscription
    {
        IDisposable Subscription { get; set; }
    }

    public class RabbitMessageServiceAttribute : Attribute
    {
    }
}