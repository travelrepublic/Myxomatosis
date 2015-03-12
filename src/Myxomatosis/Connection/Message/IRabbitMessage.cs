using System.Collections.Generic;

namespace Myxomatosis.Connection.Message
{
    public interface IRabbitMessage
    {
        byte[] RawMessage { get; }
        IDictionary<string, byte[]> RawHeaders { get; }
    }
}