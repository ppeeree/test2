using ACH.ACHLog.SeriLog;
using ACH.DataEntity.DevTreeData;
using ACH.DevTree.Entity;
using SqlSugar;
using System;
using System.IO;

namespace ACH.DBConn.Dat
{
    public class DevTreeDBContext
    {
        /// <summary>
        /// 创建DevTree数据库链接
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static SqlSugarClient GetDevTreeDBConn()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var path = Path.GetFullPath(Path.Combine(baseDir, ".."));
            string _dbFilePath = Path.Combine(path, "DAT");

            string dbFilePath = Path.Combine(_dbFilePath, "DevTree.dat");
            SqlSugarClient Db = new SqlSugarClient(new ConnectionConfig()
            {
                ConnectionString = $"Data Source={dbFilePath}",
                DbType = SqlSugar.DbType.Sqlite,
                IsAutoCloseConnection = true
            });

            /* if (!FileSystem.Exists(dbFilePath))
             {*/
            try
            {
                Db.DbMaintenance.CreateDatabase();

                Db.CodeFirst.InitTables<DeviceInfo>();
                Db.CodeFirst.InitTables<StationInfo>();
            }
            catch (Exception ex)
            {
                ALog.Error($"DevTree数据库初始化报错：{ex}");
                throw new Exception(ex.Message);
            }
            //}

            return Db;
        }
    }
}
