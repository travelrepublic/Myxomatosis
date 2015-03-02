namespace TravelRepublic.RxRabbitMQClient.Connection
{
    public class CloseConnectionResult
    {
        #region Constructors

        public CloseConnectionResult(bool successful)
        {
            Successful = successful;
        }

        #endregion Constructors

        public bool Successful { get; private set; }

        public static implicit operator bool(CloseConnectionResult result)
        {
            return result.Successful;
        }
    }
}