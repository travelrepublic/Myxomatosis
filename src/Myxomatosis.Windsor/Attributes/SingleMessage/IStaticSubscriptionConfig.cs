namespace Myxomatosis.Windsor.Attributes.SingleMessage
{
    public interface IStaticSubscriptionConfig
    {
        double Interval { get; set; }

        string QueueName { get; set; }

        string Exchange { get; set; }

        string SubscriptionId { get; set; }

        double OpenTimeout { get; set; }

        double CloseTimeout { get; set; }

        string Name { get; set; }
    }
}