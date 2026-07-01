using ACH.ACHLog.SeriLog;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ACH.DBRepository.TheWeave
{
    public class SftpHelper
    {
        private readonly string _host;
        private readonly int _port;
        private readonly string _username;
        private readonly string _password;
        private SftpClient client;
        public SftpHelper(string host, int port, string username, string password)
        {
            _host = host;
            _port = port;
            _username = username;
            _password = password;
            client = new SftpClient(_host, _port, _username, _password);
        }
        public void Connect()
        {
            client.Connect();
        }

        public void Disconnect()
        {
            client.Disconnect();
        }
        public List<ISftpFile> GetFileList(string path)
        {
            try
            {
                using (var sftp = new SftpClient(_host, _port, _username, _password))
                {
                    sftp.Connect();
                    var fileList = sftp.ListDirectory(path).ToList();
                    sftp.Disconnect();
                    return fileList;
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Logger.Error(ex, "Xftp GetFileList");
                return null;
            }
        }
        public void DownloadFile(string remotePath, string localPath)
        {
            // 如果文件在本地已存在，不做其他操作
            if (File.Exists(localPath))
            {
                return;
            }

            // 获取下载波形的文件夹地址
            var folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wavedata");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            // 判定远程上文件是否存在
            if (!client.Exists(remotePath))
            {
                ALog.Information($"DownloadFile-client上目标文件不存在：{remotePath}");
            }


            using (FileStream fs = new FileStream(localPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                try
                {
                    client.DownloadFile(remotePath, fs);
                    fs.Flush(true);
                    ALog.Information($"DownloadFile-下载到本地完成：{localPath}");
                }
                catch (Exception ex)
                {
                    ALog.Error(ex, $"DownloadFile-下载到本地失败：{localPath}");
                    try { File.Delete(localPath); } catch { }
                }
            }
        }
    }
}
