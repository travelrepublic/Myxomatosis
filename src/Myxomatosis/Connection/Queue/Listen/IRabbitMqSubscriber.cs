using System.Threading.Tasks;

namespace Myxomatosis.Connection.Queue.Listen
{
    public interface IRabbitMqSubscriber
    {
        Task<object> SubscribeAsync(QueueSubscription subscription);
    }
}