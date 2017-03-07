using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myxomatosis.Connection.Message;

namespace Myxomatosis.Connection.Errors
{
    internal class MessageDetails
    {
        public IDictionary<string, string> Headers { get; set; }

        public string Body { get; set; }

        public static MessageDetails FromRabbitMessage(RabbitMessage rabbitMessage)
        {
            return new MessageDetails
            {
                Body = Encoding.UTF8.GetString(rabbitMessage.RawMessage),
                Headers = rabbitMessage.RawHeaders.ToDictionary(kvp => kvp.Key, kvp => Encoding.UTF8.GetString(kvp.Value))
            };
        }
    }
}