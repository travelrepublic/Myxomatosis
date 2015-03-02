using System;
using System.Text;
using Newtonsoft.Json;

namespace TravelRepublic.RxRabbitMQClient.Serialization
{
    public class DefaultSerializer : ISerializer
    {
        private static readonly Lazy<ISerializer> _instance;

        #region Constructors

        static DefaultSerializer()
        {
            _instance = new Lazy<ISerializer>(() => new DefaultSerializer());
        }

        private DefaultSerializer()
        {
        }

        #endregion Constructors

        public static ISerializer Instance
        {
            get { return _instance.Value; }
        }

        #region ISerializer Members

        public byte[] Serialize(object payload)
        {
            var jsonString = JsonConvert.SerializeObject(payload);
            return Encoding.UTF8.GetBytes(jsonString);
        }

        public T Deserialize<T>(byte[] bytes)
        {
            var jsonString = Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject<T>(jsonString);
        }

        #endregion ISerializer Members
    }
}