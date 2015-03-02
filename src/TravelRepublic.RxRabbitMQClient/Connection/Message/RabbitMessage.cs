using System;
using System.Collections.Generic;
using RabbitMQ.Client;
using TravelRepublic.RxRabbitMQClient.Connection.Errors;

namespace TravelRepublic.RxRabbitMQClient.Connection.Message
{
    public class RabbitMessage<T> : RabbitMessage
    {
        public T Message { get; set; }

        public IDictionary<string, object> Headers { get; internal set; }
    }

    public class RabbitMessage : IRabbitMessage, IRabbitMessageModel
    {
        public IModel Channel { get; set; }

        public ulong DeliveryTag { get; set; }

        public IRabbitMessageErrorHandler ErrorHandler { get; set; }

        #region IRabbitMessage Members

        public byte[] RawMessage { get; internal set; }

        public IDictionary<string, byte[]> RawHeaders { get; internal set; }

        #endregion IRabbitMessage Members

        #region IRabbitMessageModel Members

        void IRabbitMessageModel.Acknowledge()
        {
            Channel.BasicAck(DeliveryTag, false);
        }

        void IRabbitMessageModel.Error()
        {
            ErrorHandler.Error(this);
        }

        void IRabbitMessageModel.Error(Exception exception)
        {
            ErrorHandler.Error(this, exception);
        }

        #endregion IRabbitMessageModel Members

        public void Reject()
        {
            Channel.BasicNack(DeliveryTag, false, true);
        }
    }
}