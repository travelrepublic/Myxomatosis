using System;

namespace TravelRepublic.RxRabbitMQClient.Connection
{
    public interface IOpenConnection
    {
        bool IsOpen { get; }

        CloseConnectionResult Close();

        CloseConnectionResult Close(TimeSpan closeTimeout);
    }
}