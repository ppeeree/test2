using ACH.DataQuality.FileCount.FileTypeStrategy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ACH.DataQuality.FileCount.DirectoryStructureStrategy
{
    /// <summary>
    /// 金风目录格式的文件遍历解析
    /// </summary>
    public class GWDirectoryStructure
    {
        /// <summary>
        /// 月份目录策略（如：2024-05）
        /// </summary>
        public class GWMonthDirectoryStrategy : IDirectoryStructureStrategy
        {
            public bool CanHandle(DirectoryInfo directory, DateTime statisticsDate)
            {
                return DirectoryParseHelper.IsDirectoryMatchPattern(directory.Name, @"^\d{4}-(0[1-9]|1[0-2])$") &&
                       DirectoryParseHelper.IsDirectoryMatchDate(directory.Name, statisticsDate, "yyyy-MM");
            }

            public List<CMSDataStatistics> Parse(DirectoryInfo directory, DateTime statisticsDate, Dictionary<string, IFileTypeStrategy> fileTypeStrategies)
            {
                // 月份目录下的子目录通常是设备目录，需要继续遍历
                List<CMSDataStatistics> result = new List<CMSDataStatistics>();
                foreach (var deviceDir in directory.GetDirectories())
                {
                    // 调用日期目录的策略或继续递归
                    result.AddRange(DirectoryParseHelper.GetFileCountByDate(deviceDir.FullName, statisticsDate, fileTypeStrategies));
                }
                return result;
            }
        }

        /// <summary>
        /// 日期目录策略（如：2024-05-01）
        /// </summary>
        public class GWDateDirectoryStrategy : IDirectoryStructureStrategy
        {
            public bool CanHandle(DirectoryInfo directory, DateTime statisticsDate)
            {
                return DirectoryParseHelper.IsDirectoryMatchPattern(directory.Name, @"^\d{4}-(0[1-9]|1[0-2])-(0[1-9]|[12][0-9]|3[01])$") &&
                       DirectoryParseHelper.IsDirectoryMatchDate(directory.Name, statisticsDate, "yyyy-MM-dd");
            }

            public List<CMSDataStatistics> Parse(DirectoryInfo directory, DateTime statisticsDate, Dictionary<string, IFileTypeStrategy> fileTypeStrategies)
            {
                // 日期目录直接统计文件
                var deviceId = directory.Parent?.Name;
                var statistics = DirectoryParseHelper.StatisticsDayFiles(directory.GetFiles(), statisticsDate, deviceId, fileTypeStrategies);
                return new List<CMSDataStatistics> { statistics };
            }
        }

        /// <summary>
        /// 设备目录策略（如：652801007）
        /// </summary>
        public class GWDeviceDirectoryStrategy : IDirectoryStructureStrategy
        {
            public bool CanHandle(DirectoryInfo directory, DateTime statisticsDate)
            {
                // 假设设备目录是7位数字
                return DirectoryParseHelper.IsDirectoryMatchPattern(directory.Name, @"^\d{7}$");
            }

            public List<CMSDataStatistics> Parse(DirectoryInfo directory, DateTime statisticsDate, Dictionary<string, IFileTypeStrategy> fileTypeStrategies)
            {
                // 设备目录下通常是日期目录，继续遍历
                List<CMSDataStatistics> result = new List<CMSDataStatistics>();
                foreach (var dateDir in directory.GetDirectories())
                {
                    // 检查子目录是否为日期目录
                    if (DirectoryParseHelper.IsDirectoryMatchPattern(dateDir.Name, @"^\d{4}-(0[1-9]|1[0-2])-(0[1-9]|[12][0-9]|3[01])$") &&
                        DirectoryParseHelper.IsDirectoryMatchDate(dateDir.Name, statisticsDate, "yyyy-MM-dd"))
                    {
                        var statistics = DirectoryParseHelper.StatisticsDayFiles(dateDir.GetFiles(), statisticsDate, directory.Name, fileTypeStrategies);
                        result.Add(statistics);
                    }
                }
                return result;
            }
        }
    }
}
