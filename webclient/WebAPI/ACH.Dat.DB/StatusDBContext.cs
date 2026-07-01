using ACH.ACHLog.SeriLog;
using ACH.Alarm.Entity;
using ACH.DataEntity.Alarm;
using Microsoft.Extensions.Configuration;
using SqlSugar;
using System;
using System.IO;
namespace ACH.DBConn.Dat
{
    public class StatusDBContext
    {
        private IConfiguration configuration;
        public StatusDBContext(IConfiguration _configuration)
        {
            configuration = _configuration;
        }

        /// <summary>
        /// 创建SD数据库链接：rootpath\风场ID\SD_风场ID.dat
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public SqlSugarClient GetStatusDBConn(string stationID)
        {
            // 数据库文件夹路径
            string waveDataPath = configuration.GetConnectionString("SaveWavePath");
            string DBFolder = Path.Combine(waveDataPath, stationID);
            if (!Directory.Exists(DBFolder))
            {
                Directory.CreateDirectory(DBFolder);
            }

            // 数据库文件路径
            string dbFilePath = Path.Combine(DBFolder, $"SD_{stationID}.dat");
            SqlSugarClient Db = new SqlSugarClient(new ConnectionConfig()
            {
                ConnectionString = $"Data Source={dbFilePath}",
                DbType = SqlSugar.DbType.Sqlite,
                IsAutoCloseConnection = true
            });
            // 数据库不存在，创建数据库
            if (!File.Exists(dbFilePath))
            {
                try
                {
                    Db.DbMaintenance.CreateDatabase();
                    Db.CodeFirst.InitTables<ChannelStatusAlarm>();
                    Db.CodeFirst.InitTables<CompItem>();
                    Db.CodeFirst.InitTables<DeviceItem>();
                    Db.CodeFirst.InitTables<EigenValueAlarm>();
                    Db.CodeFirst.InitTables<EigenValueData>();
                }
                catch (Exception ex)
                {
                    ALog.Error($"SD数据库初始化报错：{ex}");
                    throw new Exception(ex.Message);
                }
            }
            return Db;
        }
    }
}
