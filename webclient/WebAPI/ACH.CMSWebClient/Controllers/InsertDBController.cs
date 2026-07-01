using ACH.ACHLog.SeriLog;
using ACH.Alarm.Entity;
using ACH.CMSWebClient.ControllerImplement;
using ACH.CMSWebClient.ControllerModel;
using ACH.DataEntity.AnylzerData;
using ACH.DataRepository.DevTree;
using ACH.DBConn.Dat;
using ACH.DevTree.DataRepository;
using ACH.DevTree.Entity;
using ACH.Helper.ApiReponse;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace ACH.CMSWebClient.Controllers
{
    /// <summary>
    /// 接口测试+数据库添加数据（前端页面不调用）
    /// </summary>
    [Route("NetApi/[controller]")]
    [ApiController]
    [ExceptionFilterAttribute]
    public class InsertDBController : ControllerBase
    {
        private readonly CreateReponse _createReponse = new CreateReponse();
        static IDevTreeRepsitory _devTreeRepository = DevTreeRepsitory.Instance;
        InsertDBMethods insertDBMethods = new InsertDBMethods();
        StatusDBContext statusDBContext;
        public InsertDBController(IConfiguration configuration)
        {
            statusDBContext = new StatusDBContext(configuration);
        }
        /// <summary>
        /// 测试接口状态
        /// </summary>
        /// <returns></returns>
        [HttpGet("test")]
        public ActionResult Test()
        {
            try
            {
                return new ObjectResult(new ApiResponse<string>
                {
                    Code = 200,
                    Data = "",
                    Message = "操作成功",
                    Success = true
                });
            }
            catch (Exception ex)
            {
                ALog.Error(ex, $"测试接口报错");
                return new ObjectResult(new ApiResponse<string>
                {
                    Code = 500,
                    Data = "",
                    Message = "操作失败",
                    Success = false
                });
            }
        }

        #region 数据库更新

        /// <summary>
        /// 更新数据库表字段
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("UpdateDB")]
        public IActionResult UpdateDB([FromQuery] string type, string stationID)
        {
            if (type == "app")
            {
                AppDBContext.GetAppDBConn();
            }
            else if (type == "devtree")
            {
                DevTreeDBContext.GetDevTreeDBConn();
            }
            else if (type == "sd")
            {

                statusDBContext.GetStatusDBConn(stationID);
            }
            return Ok();
        }

        /// <summary>
        /// 更新数据库列名称
        /// </summary>
        /// <returns></returns>
        [HttpGet("UpdateDBColumn")]
        public IActionResult UpdateDBColumn()
        {
            var data = _devTreeRepository.GetAllMeasLocation().GroupBy(o => o.StationID);

            foreach (var item in data)
            {
                try
                {
                    using var db = statusDBContext.GetStatusDBConn(item.Key);

                    // 列已存在就跳过
                    var cols = db.DbMaintenance.GetColumnInfosByTableName("ChannelStatusAlarm", false);
                    if (cols.Any(c => c.DataType.Equals("ADUID", StringComparison.OrdinalIgnoreCase)))
                    {
                        ALog.Debug($"该{item.Key}表列已修改");
                    }

                    var ver = db.Ado.GetDataTable("SELECT sqlite_version()").Rows[0][0].ToString();
                    if (new Version(ver) >= new Version(3, 25))
                    {
                        db.Ado.ExecuteCommand("ALTER TABLE ChannelStatusAlarm RENAME COLUMN HADUID TO ADUID;");
                    }
                    else
                    {
                        db.Ado.ExecuteCommand("ALTER TABLE ChannelStatusAlarm RENAME TO ChannelStatusAlarm_old;");
                        db.CodeFirst.InitTables<ChannelStatusAlarm>();
                        db.Ado.ExecuteCommand(@"
            INSERT INTO ChannelStatusAlarm
                   (ADUID, DeviceID, DeviceType, ChannelNumber, MeaslocID,
                    ChannelStatus, Voltage, AcqTime)
            SELECT HADUID, DeviceID, DeviceType, ChannelNumber, MeaslocID,
                   ChannelStatus, Voltage, AcqTime
            FROM   ChannelStatusAlarm_old;");
                        db.Ado.ExecuteCommand("DROP TABLE ChannelStatusAlarm_old;");
                    }
                }
                catch (Exception ex)
                {
                    ALog.Error(ex, $"{item.Key}数据库修改异常");
                }
            }
            return Ok();
        }

        /// <summary>
        /// 给SD数据库的ChannelStatusAlarm表，新增ADUID字段
        /// </summary>
        /// <returns></returns>
        [HttpGet("AddADUISColumn")]
        public IActionResult AddADUISColumn()
        {
            // 获取所有风场
            var data = _devTreeRepository.GetAllMeasLocation().GroupBy(o => o.StationID);
            int successCount = 0, skipCount = 0, errorCount = 0;

            foreach (var item in data)
            {
                try
                {
                    // 遍历创建每个风场的数据库连接 
                    using var db = statusDBContext.GetStatusDBConn(item.Key);

                    // 检查列是否存在（统一使用 DbMaintenance）
                    var cols = db.DbMaintenance.GetColumnInfosByTableName("ChannelStatusAlarm", false);
                    if (cols.Any(c => c.DbColumnName.Equals("ADUID", StringComparison.OrdinalIgnoreCase)))
                    {
                        ALog.Debug($"风场{item.Key}：ADUID列已存在，跳过");
                        skipCount++;
                        continue;
                    }

                    // 新增 ADUID 列（允许NULL，后续填充数据）
                    db.Ado.ExecuteCommand("ALTER TABLE ChannelStatusAlarm ADD COLUMN ADUID TEXT;");

                    ALog.Debug($"风场{item.Key}：ADUID列新增成功");
                    successCount++;
                }
                catch (Exception ex)
                {
                    ALog.Error(ex, $"风场{item.Key}新增ADUID列异常");
                    errorCount++;
                }
            }

            return Ok(new
            {
                message = "数据库列更新完成",
                total = data.Count(),
                success = successCount,
                skip = skipCount,
                error = errorCount
            });
        }

        /// <summary>
        /// 给SD数据库ChannelStatusAlarm表的ADUID字段新增数据
        /// </summary>
        /// <returns></returns>
        [HttpGet("AddADUISColumnData")]
        public IActionResult AddADUISColumnData([FromQuery] string? stationID)
        {
            List<DevMeasLocation> dev = new List<DevMeasLocation>();
            if (string.IsNullOrEmpty(stationID))
            {
                dev = _devTreeRepository.GetAllMeasLocation();
            }
            else
            {
                dev = _devTreeRepository.GetAllMeasLocation(stationID);
            }

            // 按照风场ID分组
            var data = dev.GroupBy(o => o.StationID);
            int successCount = 0, skipCount = 0, errorCount = 0;
            int totalUpdateCount = 0;

            foreach (var item in data)
            {
                try
                {
                    // 遍历创建每个风场的数据库连接 
                    using var db = statusDBContext.GetStatusDBConn(item.Key);

                    // 统一使用 DbMaintenance 检查列是否存在
                    var cols = db.DbMaintenance.GetColumnInfosByTableName("ChannelStatusAlarm", false);
                    if (!cols.Any(c => c.DbColumnName.Equals("ADUID", StringComparison.OrdinalIgnoreCase)))
                    {
                        ALog.Debug($"风场{item.Key}：ChannelStatusAlarm表没有ADUID列，跳过更新");
                        skipCount++;
                        continue;
                    }

                    // 查询需要更新的数据（ADUID为空的记录）
                    List<ChannelStatusAlarm> channelStatusAlarms = db.Queryable<ChannelStatusAlarm>()
                        .Where(o => o.ADUID == null || o.ADUID == "")  // 只更新ADUID为空的记录
                        .ToList();

                    if (!channelStatusAlarms.Any())
                    {
                        ALog.Debug($"风场{item.Key}：没有需要更新的数据");
                        skipCount++;
                        continue;
                    }

                    int updateCount = 0;
                    foreach (var channelStatusAlarm in channelStatusAlarms)
                    {
                        try
                        {
                            // 获取ADUID数据
                            HADUChannelInfo haduChannelInfo = _devTreeRepository.GetHADUChannelInfoByMeasID(channelStatusAlarm.MeaslocID);
                            string newADUID = haduChannelInfo?.HADUID?.ToString() ?? channelStatusAlarm.DeviceID;

                            // 更新数据库表数据
                            int result = db.Updateable<ChannelStatusAlarm>()
                                .SetColumns(o => o.ADUID == newADUID)
                                .Where(o => o.MeaslocID == channelStatusAlarm.MeaslocID)
                                .ExecuteCommand();

                            if (result > 0)
                            {
                                updateCount++;
                            }
                        }
                        catch (Exception ex)
                        {
                            ALog.Error(ex, $"更新失败 MeaslocID: {channelStatusAlarm.MeaslocID}");
                        }
                    }

                    totalUpdateCount += updateCount;
                    ALog.Debug($"风场{item.Key}：ADUID数据填充完成，更新{updateCount}条记录");
                    successCount++;
                }
                catch (Exception ex)
                {
                    ALog.Error(ex, $"风场{item.Key}ADUID列新增数据异常");
                    errorCount++;
                }
            }

            return Ok(new
            {
                message = "数据填充完成",
                totalWindFarms = data.Count(),
                success = successCount,
                skip = skipCount,
                error = errorCount,
                totalUpdated = totalUpdateCount
            });
        }

        // 如果其他地方需要使用，可以保留 CheckColumnExists 方法
        // 但建议统一使用 DbMaintenance，所以这个方法可以删除
        #endregion

        #region DevTree.dat
        /// <summary>
        /// DevTree.dat：设备树按照01机组进行批量创建
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("CreatDevLocation")]
        public IActionResult CreatDevLocation([FromQuery] string deviceID, int startNum, int endNum)
        {
            InsertDBMethods.CreatDevLocation(deviceID, startNum, endNum);
            return Ok();
        }


        /// <summary>
        /// DevTree.dat：同步Theweave和Dat数据库的设备树
        /// </summary>
        /// <param name="stationID">风场ID</param>
        /// <returns></returns>
        [HttpGet("SyncDevTree")]
        public IActionResult SyncDevTree([FromQuery] string stationID)
        {
            InsertDBMethods.SyncDevTree(stationID);
            return Ok();
        }


        /// <summary>
        /// DevTree.dat：根据风场ID修改风场名称 
        /// </summary>
        /// <param name="stationID"></param>
        /// <param name="changeStationName"></param>
        /// <returns></returns>
        [HttpGet("ChangeStationName")]
        public IActionResult ChangeStationName([FromQuery] string stationID, string changeStationName)
        {
            InsertDBMethods.ChangeStationName(stationID, changeStationName);
            return Ok();
        }


        /// <summary>
        /// DevTree.dat：往DeviceInfo中添加数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("AddDeviceInfo")]
        public IActionResult AddDeviceInfo([FromQuery] string stationID)
        {
            insertDBMethods.AddDeviceInfo(stationID);
            return Ok();
        }

        /// <summary>
        /// DevTree.dat：往StationInfo中添加数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("AddStationInfo")]
        public IActionResult AddStationInfo([FromQuery] string stationID)
        {
            insertDBMethods.AddStationInfo(stationID);
            return Ok();
        }

        /// <summary>
        /// DevTree.dat：往ModbusBusConfig中添加数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("AddModbusBusConfig")]
        public IActionResult AddModbusBusConfig([FromQuery] string stationID)
        {
            insertDBMethods.AddAModbusBusConfig(stationID);
            return Ok();
        }


        /// <summary>
        /// DevTree.dat：往UltrasonicChannelInfo中添加数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("AddUltrasonicChannelInfo")]
        public IActionResult AddUltrasonicChannelInfo([FromQuery] string stationID)
        {
            insertDBMethods.AddUltrasonicChannelInfo(stationID);
            return Ok();
        }

        /// <summary>
        /// DevTree.dat：往UltrasonicChannelMapper中添加数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("AddUltrasonicChannelMapper")]
        public IActionResult AddUltrasonicChannelMapper([FromQuery] string stationID)
        {
            insertDBMethods.AddUltrasonicChannelMapper(stationID);
            return Ok();
        }

        /// <summary>
        /// DevTree.dat：往UltrasonicDeviceInfo中添加数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("AddUltrasonicDeviceInfo")]
        public IActionResult AddUltrasonicDeviceInfo([FromQuery] string stationID)
        {
            insertDBMethods.AddUltrasonicDeviceInfo(stationID);
            return Ok();
        }

        /// <summary>
        /// DevTree.dat：HADUChannelMapperHADUID添加数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("AddHADUChannelMapperHADUID")]
        public IActionResult AddHADUChannelMapperHADUID([FromQuery] string stationID)
        {
            // insertDBMethods.AddHADUChannelMapperHADUID(stationID);
            return Ok();
        }


        /// <summary>
        /// DevTree.dat：HADUChannelInfo添加数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("AddHADUChannelInfo")]
        public IActionResult AddHADUChannelInfo([FromQuery] string stationID)
        {
            insertDBMethods.AddHADUChannelInfo(stationID);
            return Ok();

        }


        /// <summary>
        /// DevTree.dat：HADUInfo添加数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("AddHADUInfo")]
        public IActionResult AddHADUInfo([FromQuery] string stationID)
        {
            insertDBMethods.AddHADUInfo(stationID);
            return Ok();

        }

        /// <summary>
        /// DevTree.dat：更新风场经纬度
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("UpdateStationPosition")]
        public IActionResult UpdateStationPosition([FromQuery] string stationID, double longitude, double latitude)
        {
            using (SqlSugarClient db = DevTreeDBContext.GetDevTreeDBConn())
            {
                StationInfo data = db.Queryable<StationInfo>().First(o => o.StationId == stationID);

                if (data != null)
                {
                    data.Longitude = longitude;
                    data.Latitude = latitude;
                    db.Updateable(data).ExecuteCommand();
                }
            }
            return Ok();
        }
        #endregion

        #region App.dat
        /// <summary>
        /// App.dat：往UserInfo中添加数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("AddUserInfo")]
        public IActionResult AddUserInfo()
        {
            insertDBMethods.AddUserInfo();
            return Ok();
        }


        /// <summary>
        /// App.dat：往UserStationMap中添加数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("AddUserStationMap")]
        public IActionResult AddUserStationMap(string stationID, string userAcount)
        {
            InsertDBMethods.AddUserStationMap(stationID, userAcount);
            return Ok();
        }


        /// <summary>
        /// App.dat：往SystemMenu中添加数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("AddSystemMenu")]
        public IActionResult AddSystemMenu()
        {
            insertDBMethods.AddSystemMenu();
            return Ok();
        }


        /// <summary>
        /// App.dat：往MeasLoc2DConfig中添加数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("AddMeasLoc2DConfig")]
        public IActionResult AddMeasLoc2DConfig()
        {
            insertDBMethods.AddMeasLoc2DConfig();
            return Ok();
        }


        #endregion

        #region SD.dat
        /// <summary>
        /// SD.dat：往ChannelStatusAlarm中添加数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("AddChannelStatusAlarm")]
        public IActionResult AddChannelStatusAlarm([FromQuery] string stationID)
        {
            insertDBMethods.AddChannelStatusAlarm(stationID);
            return Ok();
        }

        /// <summary>
        /// SD.dat：往EigenValueAlarm中添加数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("AddEigenValueAlarm")]
        public IActionResult AddEigenValueAlarm([FromQuery] string stationID)
        {
            insertDBMethods.AddEigenValueAlarm(stationID);
            return Ok();
        }
        #endregion
    }
}
