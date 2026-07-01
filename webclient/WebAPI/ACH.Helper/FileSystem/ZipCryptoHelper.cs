using ACH.ACHLog.SeriLog;
using ACH.Helper.Others;
using Ionic.Zip;
using Serilog;
using System;
using System.IO;
using System.Linq;

namespace ACH.Helper.FileSystem
{
    public class ZipCryptoHelper
    {
        private static readonly LogStore _log = LogStore.Instance;

        private static string password = "ACH@1601"; // 加密密码

        /// <summary>
        /// 加密压缩文件夹并删除原来文件（支持 AES-256 / ZipCrypto）
        /// </summary>
        /// <param name="folderPath">待压缩目录</param>
        /// <param name="zipPath">输出的zip路径</param>
        /// <param name="useAes256">true=AES-256，false=传统 ZipCrypto</param>
        /// <param name="progressCallback">进度回调函数</param>
        public static void CreateEncryptedZip(string folderPath, string zipPath, bool useAes256 = true, Action<ZipProgressEventArgs> progressCallback = null)
        {
            // 记录开始日志
            ALog.Debug($"开始创建加密压缩包: 源目录={folderPath}, 目标路径={zipPath}");

            try
            {
                if (!Directory.Exists(folderPath))
                {
                    ALog.Debug($"创建源目录: {folderPath}");
                    Directory.CreateDirectory(folderPath);
                }

                if (File.Exists(zipPath))
                {
                    ALog.Debug($"删除已存在的压缩包: {zipPath}");
                    File.Delete(zipPath);
                }

                // 检查目录大小
                long totalSize = GetDirectorySize(folderPath);
                ALog.Debug($"目录大小: {totalSize / (1024 * 1024)}MB");
                if (progressCallback != null)
                {
                    _log.AddLog($"目录大小: {totalSize / (1024 * 1024)}MB");
                }

                const long size = (10L * 1024 * 1024 * 1024);
                if (totalSize > size) // 10GB
                {
                    ALog.Debug($"目录大小超过10GB，可能需要较长时间: {totalSize / (1024 * 1024 * 1024)}GB");
                    if (progressCallback != null)
                    {
                        _log.AddLog($"目录大小超过10GB，可能需要较长时间: {totalSize / (1024 * 1024 * 1024)}GB");
                    }
                }

                using (var zip = new ZipFile())
                {
                    zip.AlternateEncoding = System.Text.Encoding.UTF8;
                    zip.AlternateEncodingUsage = ZipOption.AsNecessary;

                    zip.Password = password;
                    zip.Encryption = useAes256 ? EncryptionAlgorithm.WinZipAes256 : EncryptionAlgorithm.PkzipWeak;
                    zip.UseZip64WhenSaving = Zip64Option.AsNecessary;
                    zip.CompressionLevel = Ionic.Zlib.CompressionLevel.BestSpeed;
                    zip.BufferSize = 8192; // 设置适当的缓冲区大小，减少内存消耗

                    // 添加进度回调
                    if (progressCallback != null)
                    {
                        zip.SaveProgress += (sender, e) => progressCallback(e);
                    }

                    ALog.Debug($"开始添加目录到压缩包: {folderPath}");
                    zip.AddDirectory(folderPath, "");
                    ALog.Debug($"开始保存压缩包: {zipPath}");
                    zip.Save(zipPath);
                }

                if (!File.Exists(zipPath))
                {
                    ALog.Error($"压缩包创建失败，文件不存在: {zipPath}");
                    throw new FileNotFoundException("CreateEncryptedZip-生成压缩包失败", zipPath);
                }

                FileInfo zipFileInfo = new FileInfo(zipPath);
                if (zipFileInfo.Length == 0)
                {
                    ALog.Error($"压缩包为空: {zipPath}");
                    File.Delete(zipPath);
                    throw new InvalidOperationException("CreateEncryptedZip-生成压缩包为空");
                }

                // 只有在打包成功后才删除原目录
                ALog.Debug($"压缩包创建成功，大小: {zipFileInfo.Length / (1024 * 1024)}MB: {zipPath}");
                ALog.Debug($"删除原目录: {folderPath}");
                Directory.Delete(folderPath, true);

                // 记录成功日志
                ALog.Debug($"加密压缩包创建完成: {zipPath}");
            }
            catch (Exception ex)
            {
                // 记录详细错误日志
                ALog.Error(ex, $"CreateEncryptedZip-创建加密压缩包失败: {zipPath}");

                // 重新抛出异常，确保上层调用者知道失败
                throw;
            }
        }

        /// <summary>
        /// 获取目录大小
        /// </summary>
        /// <param name="directoryPath">目录路径</param>
        /// <returns>目录大小（字节）</returns>
        private static long GetDirectorySize(string directoryPath)
        {
            long totalSize = 0;
            try
            {
                // 获取目录中的所有文件
                string[] files = Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    try
                    {
                        FileInfo fileInfo = new FileInfo(file);
                        totalSize += fileInfo.Length;
                    }
                    catch (Exception ex)
                    {
                        ALog.Debug($"获取文件大小异常: {file}, {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                ALog.Debug($"获取目录大小异常: {directoryPath}, {ex.Message}");
            }
            return totalSize;
        }
        /*public static void CreateEncryptedZip(string folderPath, string zipPath, bool useAes256 = true)
        {

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            if (FileSystem.Exists(zipPath))
                FileSystem.Delete(zipPath);

            using (var zip = new ZipFile())
            {
                zip.AlternateEncoding = DevTree.Text.Encoding.UTF8;
                zip.AlternateEncodingUsage = ZipOption.AsNecessary;

                zip.Password = password;
                zip.Encryption = useAes256 ? EncryptionAlgorithm.WinZipAes256 : EncryptionAlgorithm.PkzipWeak;

                // 允许文件夹大于4GB
                zip.UseZip64WhenSaving = Zip64Option.AsNecessary;

                zip.AddDirectory(folderPath, "");   // 保留目录结构
                zip.CompressionLevel = Ionic.Zlib.CompressionLevel.BestSpeed;
                zip.Save(zipPath);
            }

            if (!FileSystem.Exists(zipPath))
            {
                throw new FileNotFoundException("CreateEncryptedZip-生成压缩包失败", zipPath);
            }

            // 删除原目录
            Directory.Delete(folderPath, true);

        }*/


        /// <summary>
        /// 解密解压到指定目录
        /// </summary>
        /// <param name="zipPath">带密码 zip</param>
        /// <param name="extractDir">目标目录</param>
        public static void ExtractEncryptedZip(string zipPath, string extractDir)
        {
            try
            {
                if (!File.Exists(zipPath))
                    throw new FileNotFoundException(zipPath);

                Directory.CreateDirectory(extractDir);

                using (var zip = ZipFile.Read(zipPath))
                {
                    zip.Password = password;   // 密码错误会抛异常
                    foreach (var entry in zip)
                    {
                        entry.Extract(extractDir, ExtractExistingFileAction.OverwriteSilently);
                    }
                }
            }
            catch (Exception ex)
            {
                ALog.Error(ex, $"ExtractEncryptedZip-{zipPath}解压到{extractDir}异常");
            }
        }
    }
}
