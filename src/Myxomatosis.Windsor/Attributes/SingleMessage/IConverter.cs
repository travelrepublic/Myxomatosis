namespace Myxomatosis.Windsor.Attributes.SingleMessage
{
    public interface IConverter<TSource, TTarget>
    {
        TTarget Convert(TSource source);
    }
}