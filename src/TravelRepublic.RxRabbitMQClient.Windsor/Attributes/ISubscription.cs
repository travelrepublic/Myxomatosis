using System;

namespace TravelRepublic.RxRabbitMQClient.Windsor.Attributes
{
    public interface ISubscription
    {
        IDisposable Subscription { get; set; }
    }

    public class RabbitMessageServiceAttribute : Attribute
    {
    }
}