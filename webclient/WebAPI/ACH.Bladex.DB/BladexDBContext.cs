using ACH.ACHLog.SeriLog;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using SqlSugar;
using System;

namespace ACH.DBConn.Bladex
{
    /// <summary>
    /// 创建数据库连接
    /// </summary>
    public class BladexDBContext
    {
        private readonly IConfiguration configuration;
        public BladexDBContext()
        {
            configuration = new ConfigurationBuilder()
                .Add(new JsonConfigurationSource { Path = "appsettings.json", ReloadOnChange = false })
                .Build();
        }


        /// <summary>
        /// 连接JAVA数据库：地址在appsetting中配置
        /// </summary>
        /// <returns></returns>
        public ConnectionConfig GetJAVAConnection()
        {
            var conn = configuration.GetConnectionString("SQLConn");
            if (string.IsNullOrWhiteSpace(conn))
            {
                return new ConnectionConfig()
                {
                    ConnectionString = "server=127.0.0.1;user id=root;password=Achxa123;database=corecloud_wtphm",
                    DbType = SqlSugar.DbType.MySql,
                    IsAutoCloseConnection = true
                };
            }
            else
            {
                return new ConnectionConfig()
                {
                    ConnectionString = conn,
                    DbType = SqlSugar.DbType.MySql,
                    IsAutoCloseConnection = true
                };
            }
        }

        /// <summary>
        /// 连接23-theweave数据库
        /// </summary>
        /// <returns></returns>
        public static ConnectionConfig Get23ACHConnection()
        {
            return new ConnectionConfig()
            {
                ConnectionString = "server=192.168.124.23;user id=test;password=123456;database=theweave;Charset=utf8",
                DbType = SqlSugar.DbType.MySql,
                IsAutoCloseConnection = true
            };
        }


        /// <summary>
        /// 获取TheWeave数据库链接
        /// </summary>
        /// <returns></returns>
        public SqlSugarClient GetTheWeaveDBConn()
        {
            ConnectionConfig connectionConfig = new ConnectionConfig();
            try
            {
                string conn = "server=127.0.0.1;user id=root;password=Achxa123;database=corecloud_wtphm";
                var connSQL = configuration.GetConnectionString("theweaveConn");
                if (!string.IsNullOrWhiteSpace(connSQL))
                {
                    conn = connSQL;
                }
                connectionConfig = new ConnectionConfig()
                {
                    ConnectionString = conn,
                    DbType = SqlSugar.DbType.MySql,
                    IsAutoCloseConnection = true
                };
            }
            catch (Exception ex)
            {
                ALog.Error(ex, $"创建theweave数据库链接异常，默认返回23theweave数据库连接");
                connectionConfig = new ConnectionConfig()
                {
                    ConnectionString = "server=192.168.124.23;user id=test;password=123456;database=theweave;Charset=utf8",
                    DbType = SqlSugar.DbType.MySql,
                    IsAutoCloseConnection = true
                };
            }
            return new SqlSugarClient(connectionConfig);
        }
    }
}
