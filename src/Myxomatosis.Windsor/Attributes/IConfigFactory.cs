namespace Myxomatosis.Windsor.Attributes
{
    public interface IConfigFactory<TConfig>
    {
        TConfig GetConfig();
    }
}