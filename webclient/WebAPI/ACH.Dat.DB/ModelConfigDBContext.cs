using ACH.ACHLog.SeriLog;
using ACH.DataEntity.Alarm;
using SqlSugar;
using System;
using System.IO;

namespace ACH.DBConn.Dat
{
    public class ModelConfigDBContext
    {
        /// <summary>
        /// 创建App数据库链接
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static SqlSugarClient GetModelConfigDBConn()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var path = Path.GetFullPath(Path.Combine(baseDir, ".."));
            string _dbFilePath = Path.Combine(path, "DAT");

            string dbFilePath = Path.Combine(_dbFilePath, "ModelConfig.dat");
            using SqlSugarClient Db = new SqlSugarClient(new ConnectionConfig()
            {
                ConnectionString = $"Data Source={dbFilePath}",
                DbType = SqlSugar.DbType.Sqlite,
                IsAutoCloseConnection = true
            });
            if (!File.Exists(dbFilePath))
            {
                try
                {
                    Db.DbMaintenance.CreateDatabase();
                    Db.CodeFirst.InitTables<AlarmConfig>();
                }
                catch (Exception ex)
                {
                    ALog.Error($"ModelConfig数据库初始化报错：{ex}");
                    throw new Exception(ex.Message);
                }
            }
            return Db;
        }
    }
}
