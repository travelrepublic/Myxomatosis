using Myxomatosis.Connection.Queue;
using System;

namespace Myxomatosis.Connection
{
    public interface IObservableConnection
    {
        IRabbitQueue GetQueue(string exchange, string queueName);

        IRabbitQueue GetQueue(string exchange, string queueName, ExchangeType exchangeType);

        IRabbitQueue GetQueue(string exchange, string queueName, string routingKey);

        IRabbitQueue GetQueue(string exchange, string queueName, string routingKey, ExchangeType exchangeType);

        IRabbitQueue<T> GetQueue<T>(string exchange, string queueName);

        IRabbitQueue<T> GetQueue<T>(string exchange, string queueName, ExchangeType exchangeType);

        IRabbitQueue<T> GetQueue<T>(string exchange, string queueName, string routingKey);

        IRabbitQueue<T> GetQueue<T>(string exchange, string queueName, string routingKey, ExchangeType exchangeType);

        IRabbitExchange GetExchange(string exchange);

        IRabbitExchange<T> GetExchange<T>(string exchange);

        IRabbitExchange GetExchange(string exchange, ExchangeType exchangeType);

        IRabbitExchange<T> GetExchange<T>(string exchange, ExchangeType exchangeType);
    }

    public enum ExchangeType
    {
        Topic,
        Fanout
    }

    internal static class ExchangeTypeHelpers
    {
        public static string ToRabbitExchange(this ExchangeType exchangeType)
        {
            switch (exchangeType)
            {
                case ExchangeType.Fanout:
                    return "fanout";

                case ExchangeType.Topic:
                    return "topic";
            }

            throw new Exception("Unknown Exchange Type");
        }
    }
}