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
        private readonly Queue<string> _lifeCycleAudit;

        #region Constructors

        public RabbitMessage()
        {
            _lifeCycleAudit = new Queue<string>();
            _lifeCycleAudit.Enqueue(string.Format("{0}: Created", DateTime.Now));
        }

        #endregion Constructors

        internal Guid Id { get; set; }

        public IModel Channel { get; set; }

        public ulong DeliveryTag { get; set; }

        public IRabbitMessageErrorHandler ErrorHandler { get; set; }

        internal List<RabbitMessage> UnprocessedQueue { get; set; }

        private string DebuggerDisplay
        {
            get { return _lifeCycleAudit.Peek(); }
        }

        #region IRabbitMessage Members

        public byte[] RawMessage { get; internal set; }

        public IDictionary<string, byte[]> RawHeaders { get; internal set; }

        #endregion IRabbitMessage Members

        #region IRabbitMessageModel Members

        void IRabbitMessageModel.Acknowledge()
        {
            Channel.BasicAck(DeliveryTag, false);
            _lifeCycleAudit.Enqueue(string.Format("{0}:  Acknowledged", DateTime.Now));
            UnprocessedQueue.RemoveAll(rm => rm.Id == Id);
        }

        void IRabbitMessageModel.Error()
        {
            ErrorHandler.Error(this);
            _lifeCycleAudit.Enqueue(string.Format("{0}:  Errored", DateTime.Now));
            UnprocessedQueue.RemoveAll(rm => rm.Id == Id);
        }

        void IRabbitMessageModel.Error(Exception exception)
        {
            if (exception == null)
                throw new ArgumentException("Expected a non-null Exception", "exception");
            ErrorHandler.Error(this, exception);
            _lifeCycleAudit.Enqueue(string.Format("{0}:  Errored with exception {1}", DateTime.Now, exception.Message));
            UnprocessedQueue.RemoveAll(rm => rm.Id == Id);
        }

        public void Reject()
        {
            Channel.BasicNack(DeliveryTag, false, true);
            _lifeCycleAudit.Enqueue(string.Format("{0}:  Rejected", DateTime.Now));
            UnprocessedQueue.RemoveAll(rm => rm.Id == Id);
        }

        #endregion IRabbitMessageModel Members
    }
}