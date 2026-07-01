using ACH.ACHLog.SeriLog;
using ACH.Common.Tools;
using ACH.DBConn.Bladex;
using ACH.MeasData.Entity;
using ACH.Upload.DownloadEigenValue.Entity;
using Newtonsoft.Json;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace ACH.Upload.DownloadEigenValue
{
    /// <summary>
    /// 处理运维工具下载的压缩包中的特征值数据
    /// </summary> 

    public class UploadEigenValue
    {
        private string[] FileExtension = new string[] { ".json" };

        // 1、上传特征值数据到theweave数据库
        public void UploadEigenValueData(string path)
        {
            ALog.Information($"现场下载数据-特征值上传开始");

            string jsonPath = Path.Combine(path, "eigenvalueData");
            string[] eigenJsonFiles = Directory.GetFiles(jsonPath);

            foreach (string evItemPath in eigenJsonFiles)
            {
                // 处理特征值数据
                var eigenValues = new List<EigenValueData>();
                List<EvdatatrendItem> originEVData = HandlerJSONToData<EvdatatrendItem>(evItemPath); // 从json文件中读取数组
                if (originEVData == null || originEVData.Count == 0)
                {
                    continue;
                }

                // 将原始数据转化为上传格式
                eigenValues.AddRange(ConvertEigenvalueData(originEVData));

                UploadEV(eigenValues);
                ALog.Information($"文件上传完成：{evItemPath}，{eigenValues.Count}");

                Thread.Sleep(5000);
            }

            ALog.Information("现场下载数据-特征值上传完成");
        }


        /// <summary>
        /// 读取json文件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileEntry"></param>
        /// <returns></returns>
        public List<T> HandlerJSONToData<T>(string fileEntry)
        {
            List<T> result = new List<T>();

            string jsonData = File.ReadAllText(fileEntry);  // 读取文件到字符串

            T[] jsonList = JsonConvert.DeserializeObject<T[]>(jsonData); // 反序列化JSON到对象

            result.AddRange(jsonList);


            return result;
        }



        /// <summary>
        /// 处理若羌数据，转化为标准对象中的特征值列表
        /// </summary>
        /// <param name="eigenvalueData"></param>
        /// <returns></returns>
        public IEnumerable<EigenValueData> ConvertEigenvalueData(List<EvdatatrendItem> eigenvalueData)
        {
            List<EigenValueData> result = new List<EigenValueData>();

            foreach (var item in eigenvalueData)
            {
                EigenValueData eigen = new EigenValueData();

                string deviceID = ConvertDeviceID(item.WindTurbineID);
                string measCode = ConvertMeasCode(item.WindTurbineID, item.MeasLocationID);
                string measID = $"{deviceID}{measCode}";
                ConvertSingleType(item.EigenValueCode, eigen);

                eigen.DeviceID = deviceID;
                eigen.ComponentID = $"{deviceID}{measCode.Substring(0, 3)}"; ;
                eigen.MeasLocID = measID;
                eigen.EigenValueID = $"{measID}&&{item.EigenValueCode}";
                eigen.EigenValueCode = item.EigenValueCode;
                eigen.AcqTime = item.AcquisitionTime;
                eigen.EigenValue = item.EigenValue;
                eigen.SampleRate = 0;
                eigen.DataQuality = 0;

                result.Add(eigen);
            }

            return result;
        }




        /// <summary>
        /// 处理机组ID
        /// </summary>
        /// <param name="windTurbineID"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private string ConvertDeviceID(string windTurbineID)
        {
            string parkID = windTurbineID.Substring(0, windTurbineID.Length - 5);
            string turbineCode = windTurbineID.Substring(windTurbineID.Length - 5);
            return $"{ConfigMapping.GetValuesByKey(parkID)}{turbineCode}";
        }

        /// <summary>
        /// 处理测点ID
        /// </summary>
        /// <param name="measLocationID"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private string ConvertMeasCode(string turbineID, string measLocationID)
        {
            string measCode = measLocationID.Replace(turbineID, "");
            string newMeasCode = ConfigMapping.GetValuesByKey(measCode);
            return newMeasCode;
        }

        /// <summary>
        /// 处理信号类型
        /// </summary>
        /// <param name="eigenValueCode"></param>
        /// <param name="eigen"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void ConvertSingleType(string eigenValueCode, EigenValueData eigen)
        {
            switch (eigenValueCode)
            {
                case string code when (eigenValueCode.Contains("RMS")):
                    eigen.SingleType = DataEntity.Common.EnumSignalType.A;
                    //eigen.SingleTypeName = EnumHelper.GetDescription(eigen.SingleType);
                    break;
                default:
                    eigen.SingleType = DataEntity.Common.EnumSignalType.A;
                    //eigen.SingleTypeName = "加速度";
                    break;
            }

        }

        /// <summary>
        /// 将特征值数据上传至theweave数据库
        /// </summary>
        /// <param name="eigenValues"></param>
        /// <exception cref="NotImplementedException"></exception>
        private static void UploadEV(List<EigenValueData> eigenValues)
        {
            try
            {
                using (SqlSugarClient db = new SqlSugarClient(BladexDBContext.Get23ACHConnection()))
                {
                    if (eigenValues.Count > 0)
                    {
                        // db.Insertable(eigenValues).SplitTable().ExecuteReturnSnowflakeIdList();
                        // db.Fastest<EigenValueData>().PageSize(10000).SplitTable().BulkCopy(eigenValues);
                        //db.Insertable(eigenValues).PageSize(10000).ExecuteReturnSnowflakeIdList();
                        db.Utilities.PageEach(eigenValues, 10000, pageList =>
                        {
                            db.Insertable(pageList).SplitTable().ExecuteReturnSnowflakeIdList();
                        });

                    }
                }
            }
            catch (Exception ex)
            {
                ALog.Error($"数据上传报错：{ex}");
            }
        }
    }
}
