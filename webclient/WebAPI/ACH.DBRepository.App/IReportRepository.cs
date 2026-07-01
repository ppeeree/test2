using ACH.DataEntity.ReportData;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ACH.DBRepository.App
{
    /// <summary>
    /// 对诊断报告的数据库查询
    /// </summary>
    public interface IReportRepository
    {
        public void SaveAnalyzerRecordAndDiagCollection(DeviceDiagnosisAnalyzerRecord analyzerRecord, List<DeviceDiagnosisConclusion> conclusions);

        public List<T> QueryableList<T>(Expression<Func<T, bool>> expression) where T : class, new();

        public T QueryableSingle<T>(Expression<Func<T, bool>> expression) where T : class, new();

        public List<T> QueryableAll<T>() where T : class, new();

        public List<T> QueryableSplit<T>(object splitKey, Expression<Func<T, bool>> expression) where T : class, new();

        public List<T> QueryByPageList<T>(int pageIndex, int pageSize, ref int totalCount,
                          Expression<Func<T, bool>> expression = null,
                          Expression<Func<T, object>> orderBy = null,
                          bool isAsc = true);

        public void SaveDiagnosisReport(DeviceDiagnosisReport report, List<DeviceReportAnalyzerRecord> analyzerRecords, List<DeviceReportDiagnosisConclusion> conclusions);

        public void DeleteDiagnosisReport(string reportGuid);

        public void SaveWindParkDiagnosisReport(WindParkDiagnosisReport report, List<WindParkReportDeviceRelation> deviceRelations);

        public void DeleteWindParkDiagnosisReport(string reportGuid);

        /*public void AddRecord(List<DiagnosticRecord> datas);

        public List<DiagnosticRecord> GetDiagRecordByParam(DateTime startTime, DateTime endTime, string compName);*/


        /// <summary>
        /// 根据guid获取机组诊断报告
        /// </summary>
        /// <param name="reportGuid"></param>
        /// <returns></returns>
        public DeviceDiagnosisReport GetDeviceDiagnosisReportByID(string reportGuid);
    }
}
