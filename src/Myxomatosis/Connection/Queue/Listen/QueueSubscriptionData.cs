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
    }
}