using System;

namespace Myxomatosis.Connection.Message
{
    public interface IRabbitMessageModel
    {
        void Acknowledge();

        void Error(string exchangeName = null);

        void Error(Exception exception, string exchangeName = null);

        void Reject();
    }
}