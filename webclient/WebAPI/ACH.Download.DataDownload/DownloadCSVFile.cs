using ACH.ACHLog.SeriLog;
using ACH.Helper.FileSystem;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace ACH.Download.DataDownload
{
    public class DownloadCSVFile
    {
        private IConfiguration _configuration;
        public DownloadCSVFile(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// 获取下载的.csv文件
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="rootPath">csv存储地址</param>
        /// <param name="savePath">临时存储目录</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public void GetCSVFiles(DateTime startTime, DateTime endTime, string rootPath, string savePath)
        {
            try
            {
                // 下载文件夹地址
                if (!Directory.Exists(rootPath))
                {
                    ALog.Information($"没有数据统计结果，暂不下载！");
                    return;
                }

                // 在该地址下找出时间范围内的全部文件
                List<string> filePathList = GetCSVFilePathByTimeRange(rootPath, startTime, endTime);

                // 将获取的文件复制到filePath
                foreach (string filePath in filePathList)
                {
                    FileSystemHelper.CopyFile(savePath, filePath);
                }
            }
            catch (Exception ex)
            {
                ALog.Error(ex, $"GetCSVFiles-csv文件打包异常");
            }
        }


        /// <summary>
        /// 在该地址下找出时间范围内的全部文件
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private List<string> GetCSVFilePathByTimeRange(string folderPath, DateTime startTime, DateTime endTime)
        {
            List<string> res = Directory.GetFiles(folderPath, "*.csv")
                .Where(file =>
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    return TryParseFileNameDate(fileName, out DateTime fileDate) && fileDate >= startTime && fileDate <= endTime;
                }).ToList();

            return res;
        }

        /// <summary>
        /// 解析文件名称中的时间
        /// </summary>
        /// <param name="fileName">文件名格式为 "yyyyMMdd.csv"</param>
        /// <param name="fileDate"></param>
        /// <returns></returns>
        private bool TryParseFileNameDate(string fileName, out DateTime fileDate)
        {
            fileDate = DateTime.MinValue;
            return DateTime.TryParseExact(fileName, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out fileDate);
        }
    }
}
