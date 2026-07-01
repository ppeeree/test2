using ACH.ACHLog.SeriLog;
using ACH.DataEntity.ReportData;
using ACH.DBConn.Dat;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ACH.DBRepository.App
{
    public class ReportRepository : IReportRepository
    {
        /// <summary>
        /// 数据库操作客户端
        /// </summary>
        private readonly SqlSugarClient _crudClient;
        /// <summary>
        /// 数据库操作客户端，用于事务管理
        /// </summary>
        public SqlSugarClient DbClient => _crudClient;

        /// <summary>
        /// 构造函数
        /// </summary>
        public ReportRepository()
        {
            _crudClient = AppDBContext.GetAppDBConn();
        }

        /// <summary>
        /// 保存分析结果和诊断集合记录
        /// </summary>
        public void SaveAnalyzerRecordAndDiagCollection(DeviceDiagnosisAnalyzerRecord analyzerRecord, List<DeviceDiagnosisConclusion> conclusions)
        {
            try
            {
                if (analyzerRecord == null || conclusions == null)
                {
                    throw new ArgumentNullException(typeof(DeviceDiagnosisAnalyzerRecord) + " or " + typeof(DeviceDiagnosisConclusion) + " is null");
                }
                //开始事务，一起提交，如果其中一个失败，则全部回滚
                _crudClient.BeginTran();
                _crudClient.Insertable(analyzerRecord).ExecuteCommand();
                //删除旧的诊断结论，重新插入新的诊断结论。即每次都以最后一次的诊断结论为准
                _crudClient.Deleteable<DeviceDiagnosisConclusion>().Where(x => x.WindturbineId == analyzerRecord.WindturbineId).ExecuteCommand();
                //插入新的诊断结论
                _crudClient.Insertable(conclusions).ExecuteCommand();
                _crudClient.CommitTran();
            }
            catch
            {
                _crudClient.RollbackTran();
                throw;
            }
        }
        /// <summary>
        /// 根据条件查询分析记录
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public List<T> QueryableList<T>(Expression<Func<T, bool>> expression) where T : class, new()
        {
            return _crudClient.Queryable<T>().Where(expression).ToList();
        }
        /// <summary>
        /// 根据条件查询分析记录，返回单个对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public T QueryableSingle<T>(Expression<Func<T, bool>> expression) where T : class, new()
        {
            return _crudClient.Queryable<T>().Where(expression).First();
        }
        /// <summary>
        /// 根据主键ID查询记录，返回单个对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public T GetById<T>(object id) where T : class, new()
        {
            return _crudClient.Queryable<T>().InSingle(id);
        }
        /// <summary>
        /// 查询所有记录，返回列表对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<T> QueryableAll<T>() where T : class, new()
        {
            return _crudClient.Queryable<T>().ToList();
        }
        /// <summary>
        /// 查询分组记录，返回列表对象，每组只返回一个对象，不重复。
        /// 注意：此方法在23服务器上无法使用，因为23服务器MySql默认开启了ONLY_FULL_GROUP_BY，会导致分组查询出错。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="keySelector"></param>
        /// <returns></returns>
        public List<T> QueryableGroup<T>(Expression<Func<T, bool>> expression, Expression<Func<T, object>> keySelector) where T : class, new()
        {
            return _crudClient.Queryable<T>().Where(expression).GroupBy(keySelector).ToList();
        }
        /// <summary>
        /// 分表查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="splitKey">确定分表表名关键字</param>
        /// <returns></returns>
        public List<T> QueryableSplit<T>(object splitKey, Expression<Func<T, bool>> expression) where T : class, new()
        {
            string tableName = _crudClient.SplitHelper<T>().GetTableName(splitKey); // 确定对应的数据库表
            return _crudClient.Queryable<T>().AS(tableName).Where(expression).ToList();
        }
        /// <summary>
        /// 分页查询，返回列表对象。支持排序和分组。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalCount"></param>
        /// <param name="expression"></param>
        /// <param name="orderBy"></param>
        /// <param name="isAsc"></param>
        /// <returns></returns>
        public List<T> QueryByPageList<T>(int pageIndex, int pageSize, ref int totalCount,
                          Expression<Func<T, bool>> expression = null,
                          Expression<Func<T, object>> orderBy = null,
                          bool isAsc = true)
        {
            var query = _crudClient.Queryable<T>();

            if (expression != null)
            {
                query = query.Where(expression);
            }

            totalCount = query.Count();

            if (orderBy != null)
            {
                query = isAsc ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);
            }

            return query.ToPageList(pageIndex, pageSize);
        }
        /// <summary>
        /// 保存诊断报告，包括分析记录和诊断结论
        /// </summary>
        /// <param name="report"></param>
        /// <param name="analyzerRecords"></param>
        /// <param name="conclusions"></param>
        public void SaveDiagnosisReport(DeviceDiagnosisReport report, List<DeviceReportAnalyzerRecord> analyzerRecords, List<DeviceReportDiagnosisConclusion> conclusions)
        {
            try
            {
                if (report == null || conclusions == null || analyzerRecords == null)
                {
                    throw new ArgumentNullException(typeof(DeviceDiagnosisReport) + " or " + typeof(DeviceReportAnalyzerRecord) + " or " + typeof(DeviceReportDiagnosisConclusion) + " is null");
                }
                //开始事务，一起提交，如果其中一个失败，则全部回滚
                _crudClient.BeginTran();
                //查询同天的报告，如果有则删除旧记录
                //var delReport = _crudClient.Queryable<DeviceDiagnosisReport>().Where(x => x.CreatedTime.Date == report.CreatedTime.Date);
                //if (delReport.Count() > 0)
                //{
                //    var delReportGuids = delReport.Select(x => x.ReportGuid).ToList();
                //    _crudClient.Deleteable<DeviceReportDiagnosisConclusion>().Where(x => delReportGuids.Contains(x.ReportGuid)).ExecuteCommand();
                //    _crudClient.Deleteable<DeviceReportAnalyzerRecord>().Where(x => delReportGuids.Contains(x.ReportGuid)).ExecuteCommand();
                //    _crudClient.Deleteable<DeviceDiagnosisReport>().Where(x => delReportGuids.Contains(x.ReportGuid)).ExecuteCommand();
                //}
                //删除旧的诊断结论，重新插入新的诊断结论。即每次都以最后一次的诊断结论为准
                _crudClient.Deleteable<DeviceDiagnosisConclusion>().Where(x => x.WindturbineId == report.WindturbineId).ExecuteCommand();
                var newConclustion = conclusions.Select(x => new DeviceDiagnosisConclusion
                {
                    WindturbineId = report.WindturbineId,
                    DiagnosisConclusion = x.DiagnosisConclusion,
                    CompName = x.CompName,
                    WarningLevel = x.WarningLevel,
                    MaintainAdvice = x.MaintainAdvice
                }).ToList();
                _crudClient.Insertable(newConclustion).ExecuteCommand();
                _crudClient.Insertable(report).ExecuteCommand();
                _crudClient.Insertable(analyzerRecords).ExecuteCommand();
                _crudClient.Insertable(conclusions).ExecuteCommand();
                _crudClient.CommitTran();
            }
            catch
            {
                _crudClient.RollbackTran();
                throw;
            }
        }
        /// <summary>
        /// 删除诊断报告，包括分析记录和诊断结论
        /// </summary>
        /// <param name="reportGuid"></param>
        /// <exception cref="Exception"></exception>
        public void DeleteDiagnosisReport(string reportGuid)
        {
            if (_crudClient.Queryable<DeviceDiagnosisReport>().Where(x => x.ReportGuid == reportGuid).Count() <= 0)
                throw new Exception($"DiagReportGuid {reportGuid} not found!");
            if (_crudClient.Queryable<WindParkReportDeviceRelation>().Where(x => x.WindTurbineReportGuid == reportGuid).Count() > 0)
                throw new Exception($"该报告已关联风场汇总诊断报告，无法删除!");
            try
            {
                //开始事务，一起提交，如果其中一个失败，则全部回滚
                _crudClient.BeginTran();
                _crudClient.Deleteable<DeviceReportAnalyzerRecord>().Where(x => x.ReportGuid == reportGuid).ExecuteCommand();
                _crudClient.Deleteable<DeviceReportDiagnosisConclusion>().Where(x => x.ReportGuid == reportGuid).ExecuteCommand();
                _crudClient.Deleteable<DeviceDiagnosisReport>().Where(x => x.ReportGuid == reportGuid).ExecuteCommand();
                _crudClient.CommitTran();
            }
            catch
            {
                _crudClient.RollbackTran();
                throw;
            }
        }
        /// <summary>
        /// 保存风电场诊断报告，包括设备关系记录
        /// </summary>
        /// <param name="report"></param>
        /// <param name="deviceRelations"></param>
        public void SaveWindParkDiagnosisReport(WindParkDiagnosisReport report, List<WindParkReportDeviceRelation> deviceRelations)
        {
            if (report == null || deviceRelations == null)
            {
                throw new ArgumentNullException(typeof(WindParkDiagnosisReport) + " or " + typeof(WindParkReportDeviceRelation) + " is null");
            }

            using (SqlSugarClient db = AppDBContext.GetAppDBConn())
            {
                try
                {
                    db.BeginTran();

                    // 删除同风电场和同月份下的报告
                    var existingReport = db.Queryable<WindParkDiagnosisReport>()
                        .Where(x => x.WindParkId == report.WindParkId && x.CreateTime.ToString("yyyy-MM-dd") == report.CreateTime.ToString("yyyy-MM-dd"))
                        .First(); // 查询单条记录更高效

                    if (existingReport != null)
                    {
                        // 删除历史数据
                        db.Deleteable<WindParkReportDeviceRelation>()
                          .Where(x => x.WindParkReportGuid == existingReport.ReportGuid)
                          .ExecuteCommand();

                        db.Deleteable<WindParkDiagnosisReport>()
                          .Where(x => x.ReportGuid == existingReport.ReportGuid)
                          .ExecuteCommand();
                    }

                    // 插入新数据
                    db.Insertable(report).ExecuteCommand();
                    db.Insertable(deviceRelations).ExecuteCommand();

                    db.CommitTran();
                }
                catch (Exception ex)
                {
                    db.RollbackTran();
                    // 记录日志 + 重新抛出
                    throw new InvalidOperationException($"保存风电场报告失败: {ex.Message}", ex);
                }

            }
        }

        /// <summary>
        /// 删除风电场诊断报告，包括设备关系记录
        /// </summary>
        /// <param name="reportGuid"></param>
        /// <exception cref="Exception"></exception>
        public void DeleteWindParkDiagnosisReport(string reportGuid)
        {
            if (_crudClient.Queryable<WindParkDiagnosisReport>().Where(x => x.ReportGuid == reportGuid).Count() <= 0)
                throw new Exception($"DiagReportGuid {reportGuid} not found!");
            try
            {
                //开始事务，一起提交，如果其中一个失败，则全部回滚
                _crudClient.BeginTran();
                _crudClient.Deleteable<WindParkReportDeviceRelation>().Where(x => x.WindParkReportGuid == reportGuid).ExecuteCommand();
                _crudClient.Deleteable<WindParkDiagnosisReport>().Where(x => x.ReportGuid == reportGuid).ExecuteCommand();
                _crudClient.CommitTran();
            }
            catch
            {
                _crudClient.RollbackTran();
                throw;
            }
        }


        /*public void AddRecord(List<DiagnosticRecord> datas)
        {
            try
            {
                using (SqlSugarClient db = AppDBContext.GetAppDBConn())
                {
                    db.Insertable(datas).SplitTable().ExecuteCommand();
                }
            }
            catch (Exception ex)
            {
                ALog.Error($"报警记录插入失败 {ex}");
            }
        }

        /// <summary>
        /// 2、查询范围内所有诊断记录
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="compName"></param>
        public List<DiagnosticRecord> GetDiagRecordByParam(DateTime startTime, DateTime endTime, string compName)
        {
            try
            {
                List<DiagnosticRecord> res = new List<DiagnosticRecord>();
                using (SqlSugarClient db = AppDBContext.GetAppDBConn())
                {
                    res = db.Queryable<DiagnosticRecord>()
                        .Where(o => o.CreateTime >= startTime && o.CreateTime <= endTime && o.CompName == compName).ToList();
                    return res;
                }
            }
            catch (Exception ex)
            {
                ALog.Error($"报警记录查询失败 {ex}");
                return new List<DiagnosticRecord>();
            }
        }*/

        public DeviceDiagnosisReport GetDeviceDiagnosisReportByID(string reportGuid)
        {
            DeviceDiagnosisReport res = new DeviceDiagnosisReport();
            using (SqlSugarClient db = AppDBContext.GetAppDBConn())
            {
                var data = db.Queryable<DeviceDiagnosisReport>().Where(o => o.ReportGuid == reportGuid).ToList();
                if (data != null && data.Count != 0)
                {
                    res = data.FirstOrDefault();
                }
                return res;
            }
        }
    }
}
