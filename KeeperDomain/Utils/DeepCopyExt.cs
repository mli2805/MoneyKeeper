using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

namespace KeeperDomain
{
    public static class DeepCopyExt
    {
        // T and its properties must have attribute Serializable
        public static T DeepCopyBin<T>(this T self)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                var bf = new BinaryFormatter();
                bf.Serialize(ms, self);

                ms.Position = 0;
                return (T)bf.Deserialize(ms);
            }
        }

        // T and its properties must have parameterless constructor
        public static T DeepCopyXml<T>(this T self)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                var xs = new XmlSerializer(typeof(T));
                xs.Serialize(ms, self);

                ms.Position = 0;
                return (T)xs.Deserialize(ms);
            }
        }
    }
}
