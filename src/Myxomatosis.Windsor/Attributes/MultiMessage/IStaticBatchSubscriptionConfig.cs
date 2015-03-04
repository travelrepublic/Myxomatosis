using Myxomatosis.Windsor.Attributes.SingleMessage;

namespace Myxomatosis.Windsor.Attributes.MultiMessage
{
    public interface IStaticBatchSubscriptionConfig : IStaticSubscriptionConfig
    {
        double BufferTimeout { get; set; }

        int BufferSize { get; set; }
    }
}