using ACH.ACHLog.SeriLog;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;

namespace ACH.DBRepository.TheWeave
{
    public class DataConvertHelp
    {
        public static byte[]? GetWaveData(string path)
        {
            if (!string.IsNullOrWhiteSpace(path))
            {
                if (File.Exists(path))
                {
                    return DataConvertHelp.GetWaveByteData(path);
                }

                // 检查本地是否存在
                string fileName = Path.GetFileName(path);
                string desPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wavedata", fileName);
                if (File.Exists(desPath))
                {
                    return DataConvertHelp.GetWaveByteData(desPath);
                }
                else
                {
                    // ftp 下载数据
                    SftpHelper helper = new SftpHelper("192.168.124.23", 22, "root", "Achxa_123#@!");
                    helper.Connect();
                    helper.DownloadFile(path, desPath);
                    if (!File.Exists(desPath))
                    {
                        return null;
                    }
                    helper.Disconnect();
                    return DataConvertHelp.GetWaveByteData(desPath);
                }

            }
            return null;
        }

        private static byte[] GetWaveByteData(string filePath)
        {
            try
            {
                switch (System.IO.Path.GetExtension(filePath).ToUpper())
                {
                    case ".ZXHC":
                        return ConvertTxtToByte(filePath, new string[] { "\n" });
                    case ".ACH":
                    case ".W":
                        return DeEncrypt(filePath);
                    default:
                        break;
                }
                return null;
            }
            catch (System.Exception ex)
            {
                return null;
            }
        }

        private static byte[] ConvertTxtToByte(string filePath, string[] spliter)
        {
            byte[] bytes = DeEncrypt(filePath);
            string text = System.Text.Encoding.UTF8.GetString(bytes);// System.IO.FileSystem.ReadAllText(measLocID);
            string[] strArr = text.Split(spliter, StringSplitOptions.None);

            List<byte> bytes1 = new List<byte>();
            for (int inx = 0; inx < strArr.Length; inx++)
            {
                float temp;
                if (float.TryParse(strArr[inx], out temp) == true)
                {
                    bytes1.AddRange(BitConverter.GetBytes(temp));
                }
            }
            return bytes1.ToArray();
        }
        public static byte[] DeEncrypt(string fileToUnZip)
        {
            ALog.Debug("DeEncrypt-获取数据" + fileToUnZip);
            byte[] data = null;
            ZipInputStream zipStream = null;
            ZipEntry ent = null;
            FileStream fs = null;
            // 文件不存在
            if (!File.Exists(fileToUnZip))
                return data;
            byte[] head = new byte[10];
            // 读取全部字节数据
            fs = new FileStream(fileToUnZip, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            byte[] filedata = new byte[fs.Length];
            fs.Read(filedata, 0, (int)fs.Length);
            // 判断数据类型
            try
            {
                // 读取文件数据
                MemoryStream ms = new MemoryStream(filedata);
                ms.Position = 0;
                zipStream = new ZipInputStream(ms);
                zipStream.GetNextEntry();
                long leng = zipStream.Length;

                data = new byte[zipStream.Length];
                zipStream.Read(data, 0, data.Length);

            }
            catch (Exception ex)
            {
                ALog.Error(ex, "解压数据异常");
                throw new Exception("解压数据异常！");//此处需抛出异常 否则上层无法捕捉到加密异常 会拿到错误的波形
                //return data;
            }
            finally
            {
                if (zipStream != null)
                {
                    zipStream.Close();
                    zipStream.Dispose();
                }
                if (ent != null)
                {
                    ent = null;
                }
            }
            return data;
        }
        public static double[] ConvertByteToDouble(byte[] deArray)
        {
            System.Diagnostics.Debug.Assert(deArray != null);
            System.Diagnostics.Debug.Assert(deArray.Length % 4 == 0);

            float[] fltArray = new float[deArray.Length / 4];
            Buffer.BlockCopy(deArray, 0, fltArray, 0, deArray.Length);
            double[] dataArray = Array.ConvertAll<float, double>(fltArray, item => (double)item);
            return dataArray;
        }
        public static double[] ConvertByteToDoubles(byte[] byteArray)
        {
            // 确保输入数组不为空且长度是 8 的倍数
            System.Diagnostics.Debug.Assert(byteArray != null, "Input byte array is null.");
            System.Diagnostics.Debug.Assert(byteArray.Length % 8 == 0, "Input byte array length is not a multiple of 8.");

            // 计算 double 数组的长度
            int doubleArrayLength = byteArray.Length / 8;
            double[] doubleArray = new double[doubleArrayLength];

            // 使用 Buffer.BlockCopy 来高效地复制字节到 double
            for (int i = 0; i < doubleArrayLength; i++)
            {
                int byteIndex = i * 8;
                Buffer.BlockCopy(byteArray, byteIndex, doubleArray, i * sizeof(double), sizeof(double));
            }
            return doubleArray;
        }
        public static float[] ConvertByteToFloat(byte[] _WaveData)
        {
            float[] _waveData = new float[_WaveData.Length / sizeof(float)];

            Buffer.BlockCopy(_WaveData, 0, _waveData, 0, _WaveData.Length);

            return _waveData;
        }
        public static byte[] GetByteArray(double[] doubleArray)
        {
            var waveNum = doubleArray.Length;
            float[] fltArray = new float[waveNum];
            var bytesArray = new byte[waveNum * 4];
            fltArray = Array.ConvertAll(doubleArray, item => (float)item);
            Buffer.BlockCopy(fltArray, 0, bytesArray, 0, waveNum * 4);
            return bytesArray;
        }
    }
}
