using Myxomatosis.Connection.Exchange;
using Myxomatosis.Connection.Message;
using Myxomatosis.Serialization;
using System;

namespace Myxomatosis.Connection.Errors
{
    public class UnhandledErrorHandler : IRabbitMessageErrorHandler
    {
        private readonly IRabbitPublisher _publisher;
        private readonly ISerializer _serializer;
        private readonly string ErrorExchangeName = "ErrorExchange";

        #region Constructors

        public UnhandledErrorHandler(IRabbitPublisher publisher, ISerializer errorMessageSerializer)
        {
            _publisher = publisher;
            _serializer = errorMessageSerializer;
        }

        #endregion Constructors

        #region IRabbitMessageErrorHandler Members

        public virtual void Error(RabbitMessage message, string errorExchange = null)
        {
            var errorMessage = new ErrorMessage
            {
                Message = MessageDetails.FromRabbitMessage(message)
            };

            _publisher.Publish(_serializer.Serialize(errorMessage), errorExchange ?? ErrorExchangeName);
        }

        public virtual void Error(RabbitMessage message, Exception exception, string errorExchange = null)
        {
            var errorMessage = new ErrorMessage
            {
                Message = MessageDetails.FromRabbitMessage(message),
                Exception = ExceptionDetails.FromException(exception)
            };

            _publisher.Publish(_serializer.Serialize(errorMessage), errorExchange ?? ErrorExchangeName);
        }

        #endregion IRabbitMessageErrorHandler Members
    }
}