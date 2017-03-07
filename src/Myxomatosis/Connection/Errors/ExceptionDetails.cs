﻿using System;

namespace Myxomatosis.Connection.Errors
{
    internal class ExceptionDetails
    {
        public string Message { get; set; }

        public string Type { get; set; }

        public string StackTrace { get; set; }

        public ExceptionDetails InnerException { get; set; }

        public static ExceptionDetails FromException(Exception exception)
        {
            if (exception == null) return null;

            return new ExceptionDetails()
            {
                Message = exception.Message,
                StackTrace = exception.StackTrace,
                Type = exception.GetType().ToString(),
                InnerException = FromException(exception.InnerException)
            };
        }
    }
}