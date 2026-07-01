using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace WTPHM.CMSFramework
{
    public class SerializationUtility
    {
        public static TMsg Deserialize<TMsg>(byte[] msgBuffer) where TMsg : class
        {
            byte[] deComBuffer = msgBuffer;// Compression(msgBuffer, CompressionMode.Decompress);

            System.Runtime.Serialization.DataContractSerializer serializer
                            = new System.Runtime.Serialization.DataContractSerializer(typeof(TMsg));

            System.IO.MemoryStream ms = new System.IO.MemoryStream(deComBuffer);
            using (XmlDictionaryReader binaryDictionaryReader = XmlDictionaryReader.CreateBinaryReader(ms, XmlDictionaryReaderQuotas.Max))
            {
                return (TMsg)serializer.ReadObject(binaryDictionaryReader);
            }
        }



        //----------------------------------------------------------------------------------------------
        // create by steel @ 2013-11-06
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="TMsg"></typeparam>
        /// <param name="_msgBuffer"></param>
        /// <returns></returns>
        public static byte[] Serialize<TMsg>(TMsg _msg) where TMsg : class
        {
            System.Runtime.Serialization.DataContractSerializer serializer
                = new System.Runtime.Serialization.DataContractSerializer(typeof(TMsg));

            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                XmlDictionaryWriter binaryDictionaryWriter = XmlDictionaryWriter.CreateBinaryWriter(ms);

                serializer.WriteObject(binaryDictionaryWriter, _msg);

                binaryDictionaryWriter.Flush();

                return ms.ToArray();// Compression(ms.ToArray(), CompressionMode.Compress);
            }
        }





        /// <summary>
        /// 提供内部使用压缩字流的方法
        /// </summary>
        /// <param name="data"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        private static byte[] Compression(byte[] data, CompressionMode mode)
        {
            DeflateStream zip = null;
            try
            {
                if (mode == CompressionMode.Compress)
                {
                    MemoryStream ms = new MemoryStream();
                    zip = new DeflateStream(ms, mode, true);
                    zip.Write(data, 0, data.Length);
                    zip.Close();
                    return ms.ToArray();
                }
                else
                {
                    MemoryStream ms = new MemoryStream();
                    ms.Write(data, 0, data.Length);
                    ms.Flush();
                    ms.Position = 0;
                    zip = new DeflateStream(ms, mode, true);
                    MemoryStream os = new MemoryStream();
                    int SIZE = 1024;
                    byte[] buf = new byte[SIZE];
                    int l = 0;
                    do
                    {
                        l = zip.Read(buf, 0, SIZE);
                        if (l == 0) l = zip.Read(buf, 0, SIZE);
                        os.Write(buf, 0, l);
                    } while (l != 0);
                    zip.Close();
                    return os.ToArray();
                }
            }
            catch
            {
                if (zip != null) zip.Close();
                return null;
            }
            finally
            {
                if (zip != null) zip.Close();
            }
        }
    }
}
