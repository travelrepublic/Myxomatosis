using Myxomatosis.Connection.Errors;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Myxomatosis.Connection.Message
{
    public class RabbitMessage<T> : RabbitMessage
    {
        public T Message { get; set; }

        public IDictionary<string, object> Headers { get; internal set; }
    }

    [DebuggerDisplay("{DebuggerDisplay}")]
    public class RabbitMessage : IRabbitMessage, IRabbitMessageModel
    {
        internal Guid Id { get; set; }

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

        void IRabbitMessageModel.Error(string exchangeName)
        {
            ErrorHandler.Error(this, exchangeName);
        }

        void IRabbitMessageModel.Error(Exception exception, string exchangeName)
        {
            if (exception == null)
                throw new ArgumentException("Expected a non-null Exception", "exception");
            ErrorHandler.Error(this, exception, exchangeName);
        }

        public void Reject()
        {
            Channel.BasicNack(DeliveryTag, false, true);
        }

        #endregion IRabbitMessageModel Members
    }
}