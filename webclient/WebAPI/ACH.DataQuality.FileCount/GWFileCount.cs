using ACH.ACHLog.SeriLog;
using ACH.DataQuality.FileCount.DirectoryStructureStrategy;
using ACH.DataQuality.FileCount.FileTypeStrategy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using static ACH.DataQuality.FileCount.DirectoryStructureStrategy.GWDirectoryStructure;
using static ACH.DataQuality.FileCount.FileTypeStrategy.GWFileTypeStrategy;

namespace ACH.DataQuality.FileCount
{
    /// <summary>
    /// 统计金风目录下波形文件
    /// 解析目录结构，金风数据存在两种目录结构
    /// /opt/goldwind/minio/minioData/cmsdata/his/bladedata/652801/2024-05/652801007/2024-05-01
    /// </summary>
    public class GWFileCount : IFileCount
    {
        /// <summary>
        /// 文件格式示例
        /// CMS短波形：002202_002202001_2021-01-12-01-04-00-000.cms
        /// CMS不含波形：002202_002202001_2021-01-12-01-04-35-000_F.cms
        /// BMS短波形：002202_002202001_2021-01-12-01-04-00-000.bms
        /// BMS不含波形：002202_002202001_10000011_2021-01-12-01-04-00-000_F.bms
        /// TMS短波形：002202_002202001_2021-01-12-01-04-00-000.tms
        /// TMS不含波形：002202_002202001_2021-01-12-01-04-05-000_F.tms
        /// </summary>

        // 目录结构策略集合
        private readonly List<IDirectoryStructureStrategy> _directoryStrategies;

        // 文件类型策略（保持不变）
        public readonly Dictionary<string, IFileTypeStrategy> _fileTypeStrategies;

        public GWFileCount()
        {
            // 初始化目录结构策略（将更具体、更常出现的目录结构策略放在前面，可以减少策略检查，提高性能）
            _directoryStrategies = new List<IDirectoryStructureStrategy>
            {
                new GWDateDirectoryStrategy(),    // 优先处理日期目录
                new GWMonthDirectoryStrategy(),   // 其次处理月份目录
                new GWDeviceDirectoryStrategy()    // 最后处理设备目录
            };

            // 初始化文件类型策略
            _fileTypeStrategies = new Dictionary<string, IFileTypeStrategy>
            {
                {".cms", new CMSFileTypeStrategy()},
                {".bms", new BMSFileTypeStrategy()},
                {".tms", new TMSFileTypeStrategy()}
            };
        }

        /// <summary>
        /// 统计文件个数
        /// </summary>
        /// <param name="path">源文件地址</param>
        /// <param name="statisticsDate">统计日期</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public List<CMSDataStatistics> GetFileCount(string path, DateTime statisticsDate)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException($"路径不存在: {path}");

            List<CMSDataStatistics> result = new List<CMSDataStatistics>();
            DirectoryInfo directoryInfo = new DirectoryInfo(path);

            // 尝试使用所有目录结构策略
            bool handled = false;
            foreach (var strategy in _directoryStrategies)
            {
                if (strategy.CanHandle(directoryInfo, statisticsDate))
                {
                    // 如果当前目录结构策略能处理，则使用该策略处理
                    result.AddRange(strategy.Parse(directoryInfo, statisticsDate, _fileTypeStrategies));
                    handled = true;
                    break;
                }
            }

            // 如果没有策略能处理，递归遍历子目录
            if (!handled)
            {
                foreach (DirectoryInfo subDir in directoryInfo.GetDirectories())
                {
                    result.AddRange(GetFileCount(subDir.FullName, statisticsDate));
                }
            }

            return MergeStatisticsByDeviceAndDate(result);
        }


        /// <summary>
        /// 按照机组和天合并数据
        /// </summary>
        /// <param name="statisticsList"></param>
        /// <returns></returns>
        private static List<CMSDataStatistics> MergeStatisticsByDeviceAndDate(List<CMSDataStatistics> statisticsList)
        {
            // 按照机组和天合并数据
            return statisticsList
                .GroupBy(x => new { x.DeviceID, Date = x.Stime.ToString("yyyy-MM-dd") })
                .Select(group => new CMSDataStatistics
                {
                    DeviceID = group.Key.DeviceID,
                    Stime = DateTime.Parse(group.Key.Date),
                    CMSWaveCount = group.Sum(o => o.CMSWaveCount),
                    CMSLWaveCount = group.Sum(o => o.CMSLWaveCount),
                    CMSFCount = group.Sum(o => o.CMSFCount),
                    BMSWaveCount = group.Sum(o => o.BMSWaveCount),
                    BMSFCount = group.Sum(o => o.BMSFCount),
                    TMSWaveCount = group.Sum(o => o.TMSWaveCount),
                    TMSFWaveCount = group.Sum(o => o.TMSFWaveCount)
                })
                .ToList();
        }

        /// <summary>
        /// 将数据保存至.csv文件
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <param name="fileName"></param>
        /// <param name="statisticsList"></param>
        public void WriteCSVFile(string directoryPath, string fileName, List<CMSDataStatistics> statisticsList)
        {
            try
            {
                // 确保目录存在
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // 拼接完整的文件路径
                string csvFile = Path.Combine(directoryPath, fileName);
                if (File.Exists(csvFile))
                {
                    File.Delete(csvFile);
                }

                using (StreamWriter sw = new StreamWriter(csvFile))
                {
                    sw.WriteLine("DeviceID,Stime,CMSWaveCount,CMSLWaveCount,CMSFCount,BMSWaveCount,BMSFCount,TMSWaveCount,TMSFWaveCount");
                    foreach (CMSDataStatistics item in statisticsList)
                    {
                        sw.WriteLine($"{item.DeviceID},{item.Stime},{item.CMSWaveCount},{item.CMSLWaveCount},{item.CMSFCount},{item.BMSWaveCount},{item.BMSFCount},{item.TMSWaveCount},{item.TMSFWaveCount}");
                    }
                }
            }
            catch (Exception ex)
            {
                ALog.Error($"存储.csv文件报错：{ex}");
            }
        }

    }
}
