using System.Collections.Generic;

namespace TravelRepublic.RxRabbitMQClient.Connection.Message
{
    public interface IRabbitMessage
    {
        byte[] RawMessage { get; }

        IDictionary<string, byte[]> RawHeaders { get; }
    }
}