using System.Collections.Generic;
using RabbitMQ.Client;

namespace Myxomatosis.Connection
{
    static class RabbitMqClientExtenstions
    {
        public static void DeclareExchange(this IModel model, string exchange, string type)
        {
            model.ExchangeDeclare(exchange, type, true);
        }

        public static void DeclareQueue(this IModel model, string queueName, Dictionary<string, object> args )
        {
            model.QueueDeclare(queueName, true, false, false, args);
        }
    }
}