using ACH.ACHLog.SeriLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACH.Helper.FileSystem
{
    /// <summary>
    /// 文件系统操作类
    /// </summary>
    public class FileSystemHelper
    {
        #region 文件删除操作
        /// <summary>
        /// 批量删除文件
        /// </summary>
        /// <param name="files">文件路径数组</param>
        /// <returns>删除成功的数量</returns>
        public static int DeleteFiles(string[] files)
        {
            int deletedCount = 0;
            foreach (string file in files)
            {
                if (DeleteFileSystemEntry(file, true))
                {
                    deletedCount++;
                }
            }
            return deletedCount;
        }

        /// <summary>
        /// 批量删除目录
        /// </summary>
        /// <param name="directories">目录路径数组</param>
        /// <returns>删除成功的数量</returns>
        public static int DeleteDirectories(string[] directories)
        {
            int deletedCount = 0;
            foreach (string directory in directories)
            {
                if (DeleteFileSystemEntry(directory, false))
                {
                    deletedCount++;
                }
            }
            return deletedCount;
        }

        /// <summary>
        /// 删除超出时间的文件和目录
        /// </summary>
        /// <param name="rootPath">根目录</param>
        /// <param name="time">截止时间</param>
        public static void DeleteOvertimeFileSystemEntries(string rootPath, DateTime time)
        {
            try
            {
                ALog.Debug($"删除超出范围文件系统项开始");
                if (!Directory.Exists(rootPath))
                    return;

                // 删除过期文件夹
                var dirs = Directory.GetDirectories(rootPath);
                foreach (var dir in dirs)
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(dir);
                    if (directoryInfo.CreationTime < time)
                    {
                        if (DeleteFileSystemEntry(dir, false))
                        {
                            ALog.Information($"删除过期文件夹：{dir}");
                        }
                    }
                }

                // 删除过期文件
                var files = Directory.GetFiles(rootPath);
                foreach (string file in files)
                {
                    if (File.GetLastWriteTime(file) < time)
                    {
                        if (DeleteFileSystemEntry(file, true))
                        {
                            ALog.Information($"删除过期文件：{file}");
                        }
                    }
                }
                ALog.Debug($"删除超出范围的文件完成");
            }
            catch (Exception ex)
            {
                ALog.Error(ex, $"DeleteOvertimeFileSystemEntries-删除{rootPath}失败");
            }
        }

        /// <summary>
        /// 删除单个文件系统项（文件或目录）
        /// </summary>
        /// <param name="path">文件或目录路径</param>
        /// <param name="isFile">是否为文件</param>
        /// <returns>是否删除成功</returns>
        private static bool DeleteFileSystemEntry(string path, bool isFile)
        {
            try
            {
                if (isFile)
                {
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                        ALog.Debug($"删除文件成功: {path}");
                        return true;
                    }
                }
                else
                {
                    if (Directory.Exists(path))
                    {
                        Directory.Delete(path, true);
                        ALog.Debug($"删除目录成功: {path}");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                string type = isFile ? "文件" : "目录";
                ALog.Error(ex, $"删除{type}失败: {path}");
            }
            return false;
        }

        /// <summary>
        /// 根据文件模式删除文件
        /// </summary>
        /// <param name="directoryPath">目录路径</param>
        /// <param name="searchPattern">文件搜索模式</param>
        /// <param name="searchOption">搜索选项</param>
        /// <returns>删除成功的数量</returns>
        public static int DeleteFilesByPattern(string directoryPath, string searchPattern, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    ALog.Debug($"目录不存在，跳过删除: {directoryPath}");
                    return 0;
                }

                string[] files = Directory.GetFiles(directoryPath, searchPattern, searchOption);
                return DeleteFiles(files);
            }
            catch (Exception ex)
            {
                ALog.Error(ex, $"DeleteFilesByPattern-删除{directoryPath}中的文件失败");
                return 0;
            }
        }

        /// <summary>
        /// 根据谓词条件删除目录
        /// </summary>
        /// <param name="directoryPath">目录路径</param>
        /// <param name="predicate">目录筛选谓词</param>
        /// <param name="searchOption">搜索选项</param>
        /// <returns>删除成功的数量</returns>
        public static int DeleteDirectoriesByPredicate(string directoryPath, Func<string, bool> predicate, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    ALog.Debug($"目录不存在，跳过删除: {directoryPath}");
                    return 0;
                }

                string[] allDirs = Directory.GetDirectories(directoryPath, "*", searchOption);
                string[] filteredDirs = allDirs.Where(predicate).ToArray();
                return DeleteDirectories(filteredDirs);
            }
            catch (Exception ex)
            {
                ALog.Error(ex, $"DeleteDirectoriesByPredicate-删除{directoryPath}中的目录失败");
                return 0;
            }
        }

        /// <summary>
        /// 清理临时目录
        /// </summary>
        /// <param name="tempDirectoryPath">临时目录路径</param>
        public static void CleanupTempDirectory(string tempDirectoryPath)
        {
            // 直接清理临时目录
            if (!string.IsNullOrEmpty(tempDirectoryPath) && Directory.Exists(tempDirectoryPath))
            {
                try
                {
                    Directory.Delete(tempDirectoryPath, true);
                    ALog.Debug($"清理临时目录: {tempDirectoryPath}");
                }
                catch (Exception ex)
                {
                    ALog.Debug($"清理临时目录失败（忽略）: {tempDirectoryPath}");
                }
            }
        }
        #endregion


        #region 文件转移
        /// <summary>
        /// 将文件COPY到新地址下
        /// </summary>
        /// <param name="destinationPath">目标文件夹路径</param>
        /// <param name="sourceFilePath">源文件路径</param>
        public static void CopyFile(string destinationPath, string sourceFilePath)
        {
            try
            {
                // 目标目录不存在则创建
                if (!Directory.Exists(destinationPath))
                    Directory.CreateDirectory(destinationPath);

                // 如果来源是目录，直接返回
                if (Directory.Exists(sourceFilePath))
                    return;

                string destinationFilePath = Path.Combine(destinationPath, Path.GetFileName(sourceFilePath));

                // 目标文件已存在则删除
                if (File.Exists(destinationFilePath))
                    File.Delete(destinationFilePath);

                File.Copy(sourceFilePath, destinationFilePath);
            }
            catch (Exception ex)
            {
                ALog.Error(ex, $"CopyFile-文件COPY异常");
            }
        }

        /// <summary>
        /// 异步将文件COPY到新地址下
        /// </summary>
        /// <param name="destinationPath">目标文件夹路径</param>
        /// <param name="sourceFilePath">源文件路径</param>
        public static async Task CopyFileAsync(string destinationPath, string sourceFilePath)
        {
            try
            {
                // 目标目录不存在则创建
                if (!Directory.Exists(destinationPath))
                    Directory.CreateDirectory(destinationPath);

                // 如果来源是目录，直接返回
                if (Directory.Exists(sourceFilePath))
                    return;

                string destinationFilePath = Path.Combine(destinationPath, Path.GetFileName(sourceFilePath));

                // 目标文件已存在则删除
                if (File.Exists(destinationFilePath))
                    File.Delete(destinationFilePath);

                using (FileStream sourceStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (FileStream destinationStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.Read))
                {
                    await sourceStream.CopyToAsync(destinationStream);
                }
            }
            catch (IOException ex) when (ex.Message.Contains("正在使用"))
            {
                // 处理文件被占用的情况，添加重试
                for (int retry = 0; retry < 3; retry++)
                {
                    await Task.Delay(100);
                    try
                    {
                        // 重新复制
                        using (FileStream sourceStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        using (FileStream destinationStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.Read))
                        {
                            await sourceStream.CopyToAsync(destinationStream);
                        }
                        break;
                    }
                    catch { if (retry == 2) throw; }
                }
            }
            catch (Exception ex)
            {
                ALog.Error(ex, $"CopyFileAsync-文件COPY异常");
            }
        }


        #endregion


        #region 临时目录管理
        /// <summary>
        /// 创建临时目录（基于时间戳命名）
        /// </summary>
        /// <param name="basePath">基础目录路径</param>
        /// <param name="prefix">目录前缀（可选）</param>
        /// <returns>创建的临时目录路径</returns>
        public static string CreateTempDirectory(string basePath, string prefix = "")
        {
            try
            {
                // 确保基础目录存在
                if (!Directory.Exists(basePath))
                {
                    Directory.CreateDirectory(basePath);
                    ALog.Debug($"创建基础目录: {basePath}");
                }

                // 生成基于时间戳的目录名
                string timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                string dirName = string.IsNullOrEmpty(prefix) ? timestamp : $"{prefix}_{timestamp}";
                string tempPath = Path.Combine(basePath, dirName);

                // 创建临时目录
                Directory.CreateDirectory(tempPath);
                ALog.Debug($"创建临时目录: {tempPath}");

                return tempPath;
            }
            catch (Exception ex)
            {
                ALog.Error(ex, $"CreateTempDirectory-创建临时目录失败");
                throw;
            }
        }

        /// <summary>
        /// 检查目录是否为空
        /// </summary>
        /// <param name="directoryPath">目录路径</param>
        /// <returns>是否为空</returns>
        public static bool IsDirectoryEmpty(string directoryPath)
        {
            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    ALog.Debug($"目录不存在: {directoryPath}");
                    return true;
                }

                bool isEmpty = !Directory.EnumerateFileSystemEntries(directoryPath).Any();
                if (isEmpty)
                {
                    ALog.Information($"目录为空: {directoryPath}");
                }
                return isEmpty;
            }
            catch (Exception ex)
            {
                ALog.Error(ex, $"IsDirectoryEmpty-检查目录是否为空失败");
                return true; // 异常时视为空，避免后续操作失败
            }
        }

        /// <summary>
        /// 创建并验证临时目录
        /// </summary>
        /// <param name="basePath">基础目录路径</param>
        /// <param name="prefix">目录前缀（可选）</param>
        /// <returns>创建的临时目录路径</returns>
        public static string CreateAndValidateTempDirectory(string basePath, string prefix = "")
        {
            string tempPath = CreateTempDirectory(basePath, prefix);
            // 验证目录是否创建成功
            if (!Directory.Exists(tempPath))
            {
                throw new DirectoryNotFoundException($"临时目录创建失败: {tempPath}");
            }
            return tempPath;
        }
        #endregion
    }
}
