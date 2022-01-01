using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.Protobufs
{
    /// <summary>
    /// Protobuf 序列化工具
    /// </summary>
    public static class ProtobufSerialization
    {
        public static byte[]? ToArray<T>(this T o) where T : IMessage
        {
            if (o == null) return null;

            using (MemoryStream ms = new MemoryStream())
            {
                o.WriteTo(ms);
                return ms.ToArray();
            }
        }

        public static T? ToObject<T>(this byte[] buf) where T : IMessage<T>, new()
        {
            if (buf == null) return default(T);
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    ms.Write(buf, 0, buf.Length);
                    ms.Seek(0, SeekOrigin.Begin);

                    MessageParser<T> parser = new MessageParser<T>(() => new T());
                    return parser.ParseFrom(ms);
                }
            }
            catch
            {
                return default;
            }
        }

        public static IMessage? ToObject(this byte[] buf, Type type)
        {
            if (buf == null) return default;
            IMessage? message = (IMessage?)Activator.CreateInstance(type);
            if(message == null) return default;    
            try
            {
                message.MergeFrom(buf);
                return message;
            }
            catch
            {
                return default;
            }
        }
    }
}
