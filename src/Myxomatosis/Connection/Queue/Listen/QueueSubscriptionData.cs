using System.Collections.Generic;

namespace Myxomatosis.Connection.Queue.Listen
{
    public class QueueSubscriptionData
    {
        #region Constructors

        public QueueSubscriptionData(string queue)
        {
            Queue = queue;
            PrefetchCount = 50;
        }

        #endregion Constructors

        public string Queue { get; private set; }

        public ushort PrefetchCount { get; set; }

        public Dictionary<string, object> Args { get; set; } 
    }
}