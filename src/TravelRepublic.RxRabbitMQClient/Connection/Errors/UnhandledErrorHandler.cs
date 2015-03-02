using System;
using TravelRepublic.RxRabbitMQClient.Connection.Exchange;
using TravelRepublic.RxRabbitMQClient.Connection.Message;
using TravelRepublic.RxRabbitMQClient.Serialization;

namespace TravelRepublic.RxRabbitMQClient.Connection.Errors
{
    public class UnhandledErrorHandler : IRabbitMessageErrorHandler
    {
        private readonly IRabbitPublisher _publisher;
        private readonly ISerializer _serializer;
        private readonly string ErrorQueueName = "ErrorExchange";

        #region Constructors

        public UnhandledErrorHandler(IRabbitPublisher publisher, ISerializer errorMessageSerializer)
        {
            _publisher = publisher;
            _serializer = errorMessageSerializer;
        }

        #endregion Constructors

        #region IRabbitMessageErrorHandler Members

        public virtual void Error(RabbitMessage message)
        {
            var errorMessage = new ErrorMessage
            {
                Message = MessageDetails.FromRabbitMessage(message)
            };

            _publisher.Publish(_serializer.Serialize(errorMessage), ErrorQueueName);
        }

        public virtual void Error(RabbitMessage message, Exception exception)
        {
            var errorMessage = new ErrorMessage
            {
                Message = MessageDetails.FromRabbitMessage(message),
                Exception = ExceptionDetails.FromException(exception)
            };

            _publisher.Publish(_serializer.Serialize(errorMessage), ErrorQueueName);
        }

        #endregion IRabbitMessageErrorHandler Members
    }
}