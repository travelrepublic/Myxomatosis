namespace TravelRepublic.RxRabbitMQClient.Windsor.Attributes
{
    public interface IConverter<TSource, TTarget>
    {
        TTarget Convert(TSource source);
    }
}