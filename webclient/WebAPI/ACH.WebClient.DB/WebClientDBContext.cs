using Microsoft.Extensions.Configuration;
using SqlSugar;

namespace ACH.WebClient.DB
{
    public class WebClientDBContext
    {
        private IConfiguration configuration;
        public WebClientDBContext(IConfiguration _configuration)
        {
            configuration = _configuration;
            db = configuration.GetConnectionString("DbType") ?? "sqlite";
            _ConnectionString = "Data Source=demo.db";
            TheWeaveConn = configuration.GetConnectionString("TheWeaveConn");
        }
        public static string db;
        public static string _ConnectionString;
        public static string TheWeaveConn;

        public static ConnectionConfig GetACHConnection()
        {
            return new ConnectionConfig()
            {
                ConnectionString = _ConnectionString,
                DbType = SqlSugar.DbType.Sqlite,
                IsAutoCloseConnection = true
            };
        }

        public static ConnectionConfig Get23ACHConnection()
        {
            return new ConnectionConfig()
            {
                ConnectionString = "server=192.168.124.23;user id=test;password=123456;database=theweave;Charset=utf8",
                DbType = SqlSugar.DbType.MySql,
                IsAutoCloseConnection = true
            };
        }
    }
}
