using ACH.ACHLog.SeriLog;
using ACH.DataEntity.App;
using ACH.DataEntity.ReportData;
using SqlSugar;
using System;
using System.IO;

namespace ACH.DBConn.Dat
{
    public class AppDBContext
    {
        /// <summary>
        /// 创建App数据库链接
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static SqlSugarClient GetAppDBConn()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var path = Path.GetFullPath(Path.Combine(baseDir, ".."));
            string _dbFilePath = Path.Combine(path, "DAT");

            string dbFilePath = Path.Combine(_dbFilePath, "App.dat");
            using SqlSugarClient Db = new SqlSugarClient(new ConnectionConfig()
            {
                ConnectionString = $"Data Source={dbFilePath}",
                DbType = SqlSugar.DbType.Sqlite,
                IsAutoCloseConnection = true
            });
            /*if (!File.Exists(dbFilePath))
            {*/
            try
            {
                Db.DbMaintenance.CreateDatabase();

                // 创建诊断报告相关数据表
                Db.CodeFirst.InitTables<DefaultDiagnosisConclusion>();
                Db.CodeFirst.InitTables<DefaultRuningAdvice>();
                Db.CodeFirst.InitTables<DeviceDiagnosisAnalyzerRecord>();
                Db.CodeFirst.InitTables<DeviceDiagnosisConclusion>();
                Db.CodeFirst.InitTables<DeviceDiagnosisReport>();
                Db.CodeFirst.InitTables<DeviceReportAnalyzerRecord>();
                Db.CodeFirst.InitTables<DeviceReportDiagnosisConclusion>();
                // Db.CodeFirst.InitTables<DiagnosticRecord>();
                Db.CodeFirst.InitTables<WindParkDiagnosisReport>();
                Db.CodeFirst.InitTables<WindParkReportDeviceRelation>();


                // 用户信息
                Db.CodeFirst.InitTables<UserInfo>();
                Db.CodeFirst.InitTables<UserStationMapper>();
                Db.CodeFirst.InitTables<UserRoleMapper>();
                Db.CodeFirst.InitTables<RoleInfo>();

                // 页面按钮数据库
                Db.CodeFirst.InitTables<SystemTopMenu>();
                Db.CodeFirst.InitTables<SystemMenu>();

                // 测点二维信息表
                Db.CodeFirst.InitTables<MeaslocCircleModelConfig>();
                Db.CodeFirst.InitTables<MeaslocCircleConfigMapper>();
            }
            catch (Exception ex)
            {
                ALog.Error($"APP数据库初始化报错：{ex}");
                throw new Exception(ex.Message);
            }
            // }
            return Db;
        }
    }
}
