namespace Myxomatosis.Serialization
{
    public interface ISerializer
    {
        byte[] Serialize(object payload);

        T Deserialize<T>(byte[] bytes);
    }
}