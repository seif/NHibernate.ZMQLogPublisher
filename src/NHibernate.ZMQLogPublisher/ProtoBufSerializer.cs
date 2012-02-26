namespace NHibernate.ZMQLogPublisher
{
    using System.IO;

    using ProtoBuf;

    public class ProtoBufSerializer<T>
    {
        public static T Deserialize(byte[] data)
        {
            using (MemoryStream stream = new MemoryStream(data))
            {
                return Serializer.DeserializeWithLengthPrefix<T>(stream, PrefixStyle.Fixed32);
            }
        }

        public static byte[] Serialize(T obj)
        {
            using(MemoryStream stream = new MemoryStream())
            {
                byte[] data;
                Serializer.SerializeWithLengthPrefix(stream, obj, PrefixStyle.Fixed32);
                return stream.ToArray();
            }
            
        }
    }
}