using System;

namespace Myxomatosis.Connection
{
    public interface IOpenConnection
    {
        bool IsOpen { get; }

        CloseConnectionResult Close();

        CloseConnectionResult Close(TimeSpan closeTimeout);
    }
}