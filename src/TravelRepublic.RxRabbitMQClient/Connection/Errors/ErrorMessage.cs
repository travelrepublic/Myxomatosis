﻿namespace TravelRepublic.RxRabbitMQClient.Connection.Errors
{
    public class ErrorMessage
    {
        public ExceptionDetails Exception { get; set; }

        public MessageDetails Message { get; set; }
    }
}