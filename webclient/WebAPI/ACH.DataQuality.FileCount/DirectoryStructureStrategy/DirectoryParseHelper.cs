using ACH.DataQuality.FileCount.FileTypeStrategy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace ACH.DataQuality.FileCount.DirectoryStructureStrategy
{
    /// <summary>
    /// 目录解析辅助方法
    /// </summary>
    public static class DirectoryParseHelper
    {
        /// <summary>
        /// 检查目录名是否匹配指定模式
        /// </summary>
        public static bool IsDirectoryMatchPattern(string directoryName, string pattern)
        {
            return Regex.IsMatch(directoryName, pattern);
        }

        /// <summary>
        /// 检查目录名是否匹配指定日期格式
        /// </summary>
        public static bool IsDirectoryMatchDate(string directoryName, DateTime date, string dateFormat)
        {
            return directoryName == date.ToString(dateFormat);
        }

        /// <summary>
        /// 统计文件列表中的文件类型信息
        /// </summary>
        public static CMSDataStatistics StatisticsDayFiles(IEnumerable<FileInfo> fileInfos, DateTime date, string deviceId, Dictionary<string, IFileTypeStrategy> fileTypeStrategies)
        {
            CMSDataStatistics statistics = new CMSDataStatistics()
            {
                DeviceID = deviceId,
                Stime = date
            };

            // 使用文件类型策略进行统计
            foreach (FileInfo file in fileInfos)
            {
                string extension = file.Extension.ToLower();
                if (fileTypeStrategies.TryGetValue(extension, out var strategy))
                {
                    strategy.UpdateStatistics(statistics, file);
                }
            }

            return statistics;
        }

        /// <summary>
        /// 按日期统计文件
        /// </summary>
        public static List<CMSDataStatistics> GetFileCountByDate(string fullName, DateTime statisticsDate, Dictionary<string, IFileTypeStrategy> fileTypeStrategies)
        {
            List<CMSDataStatistics> result = new List<CMSDataStatistics>();
            DirectoryInfo directoryInfo = new DirectoryInfo(fullName);

            // 检查是否为日期目录
            if (IsDirectoryMatchPattern(directoryInfo.Name, @"^\d{4}-(0[1-9]|1[0-2])-(0[1-9]|[12][0-9]|3[01])$"))
            {
                // 统计目录下的文件
                result.Add(StatisticsDayFiles(directoryInfo.GetFiles(), statisticsDate, directoryInfo.Parent?.Name, fileTypeStrategies));
                return result;
            }

            // 非日期格式目录，继续递归
            foreach (DirectoryInfo subDir in directoryInfo.GetDirectories())
            {
                result.AddRange(GetFileCountByDate(subDir.FullName, statisticsDate, fileTypeStrategies));
            }

            return result;
        }
    }
}
