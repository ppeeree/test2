using ACH.ACHLog.SeriLog;
using ACH.Helper.FileSystem;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace ACH.Download.DataDownload
{
    /// <summary>
    /// 下载采集服务日志
    /// </summary>
    public class DownloadMonitorServerLog
    {
        // 采集服务日志地址（示例：/opt/ACH.GWHADU/ACH.GWHADU.AcqServer/logs）
        private readonly string monitorServerLogPath = "";

        private IConfiguration _configuration;
        public DownloadMonitorServerLog(IConfiguration configuration)
        {
            _configuration = configuration;
            monitorServerLogPath = configuration["monitorServerPath"];
        }

        /// <summary>
        /// 运行日志下载
        /// </summary>
        /// <param name="temporaryPath">诊断数据临时目录</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        public void DownloadLogs(string temporaryPath, DateTime startTime, DateTime endTime)
        {

            if (!Directory.Exists(monitorServerLogPath))
            {
                ALog.Information($"{monitorServerLogPath}地址不存在，暂不下载采集服务运行日志");
                return;
            }

            // 获取采集日志下的所有文件夹
            string[] deviceLogsFolder = Directory.GetDirectories(monitorServerLogPath);
            foreach (string deviceLog in deviceLogsFolder)
            {
                string logType = deviceLog.ToLower().Contains("error") ? "Error" : "Debug";
                try
                {
                    string[] logsPath = Directory.GetFiles(deviceLog);
                    foreach (string log in logsPath)
                    {
                        FileInfo logFileInfo = new FileInfo(log);

                        if (startTime <= logFileInfo.LastWriteTime && logFileInfo.LastWriteTime <= endTime)
                        {
                            FileSystemHelper.CopyFile(Path.Combine(temporaryPath, "Logs", logType), log);
                        }
                    }

                    ALog.Information($"{deviceLog}中日志COPY完成");
                }
                catch (Exception ex)
                {
                    ALog.Error(ex, $"DownloadLogs-{deviceLog}中采集日志打包异常");
                }
            }
        }
    }
}
