import request from '@/router/axios' //导入已经写好的拦截器

// 分析列表数据
/* export const analyzerListApi = (data) => {
  return request({
    url: 'api/wtphm-service/analyzerData/getAnalyzerList',
    method: 'get',
    params: { ...data }
  })
} */

// NET.分析列表
export const analyzerListApi = (data) => {
  return request({
    url: 'NetApi/Analysis/DeviceTree',
    method: 'get',
    params: {
      ...data
    }
  })
}
//1.6.4.3
// 导出一个名为analyzerListApiNew的函数，用于发送请求
export const analyzerListApiNew = (data) => {
  // 发送get请求，请求地址为NetApi/Analysis/DeviceTrees，参数为data
  return request({
    url: 'NetApi/Analysis/DeviceTrees',
    method: 'get',
    params: {
      ...data
    }
  })
}
// 1.6.4.3

export const getGroupTrendData = (data) => {
  return request({
    url: 'NetApi/Analysis/GroupTrend',
    method: 'post',
    data: { ...data }
  })
}
// 特征值列表
/* export const getEvCodeListApi = (data) => {
  return request({
    url: 'api/wtphm-service/analyzerData/getEvCodeList',
    method: 'get',
    params: { ...data }
  })
} */

// NET 特征值列表
export const getEvCodeListApi = (data) => {
  return request({
    url: 'NetApi/Analysis/EVCodeList',
    method: 'get',
    params: { ...data }
  })
}


// 获取默认工况范围
export const getWkRangeApi = (data) => {
  return request({
    url: 'api/wtphm-service/analyzerData/getWkRange',
    method: 'get',
    params: { ...data }
  })
}

// 特征值趋势
/* export const getEvAnalyzerDataApi = (data) => {
  return request({
    url: 'api/wtphm-service/analyzerData/getEvAnalyzerData',
    method: 'post',
    data: { ...data }
  })
} */

// NET. 特征值趋势
export const getEvAnalyzerDataApi = (data, config = {}) => {
  return request({
    url: 'NetApi/Analysis/EvTrendList',
    method: 'post',
    data: { ...data },
    ...config
  })
}
// 获取自诊断结果，按钮上增加红色标记点
export const getBearingDiagnoseResultApi = (data) => {
  return request({
    url: 'NetApi/Analysis/GetBearingDiagnoseResult',
    method: 'get',
    params: { ...data }
  })
}

// 删除分析列表数据
export const deleteWindListApi = (data) => {
  return request({
    url: 'api/wtphm-service/analyzerData/deleteDataById',
    method: 'get',
    params: { ...data }
  })
}

/******* 
 * @description: 获取波形数据
 * @param {Object} data ...
 * @param {*} acqTime 时间
 * @param {*} measlocCode 测点code	
 * @param {*} waveCategory 波形种类(时域/频域/瀑布图/阶次分析/倒谱图),示例值(TimeDomain/FreqDomain/WaterfallCurve/OrderAnalysis/Cepstrum)	
 * @param {*} windturbineId 机组id	
 * @return {*} 
 */
export const getWaveformDataApi = (data) => {
  return request({
    url: 'api/wtphm-service/analyzerData/getWaveformData',
    method: 'get',
    params: { ...data }
  })
}

// 获取波形索引接口
export const getWaveIndexApi = (data) => {
  return request({
    url: 'api/wtphm-service/analyzerData/getWavaIndexByCond',
    method: 'post',
    data: { ...data }
  })
}


// 波形索引点对应的测点波形信息
export const getWaveInfoByTurbine = (data) => {
  return request({
    url: 'api/wtphm-service/analyzerData/getWavaInfoByCond',
    method: 'get',
    params: { ...data }
  })
}

export const saveDiagnosticRecordList = (data) => {
  return request({
    url: 'api/wtphm-service/diagnosticRecord/saveDiagnosticRecord',
    method: 'post',
    data: { ...data }
  })
}

export const getManualDiagnosisList = (data) => {
  return request({
    url: 'api/wtphm-service/diagnosticRecord/getManualDiagnosisList',
    method: 'get',
    params: { ...data }
  })
}

// 获取记录结构树
export const getManualDiagnosisListTreeByWindturbineId = (data) => {
  return request({
    url: 'api/wtphm-service/diagnosticRecord/getManualDiagnosisListTreeByWindturbineId',
    method: 'get',
    params: { ...data }
  })
}

// 获取机组信息
export const getWindturbineAndCompInfo = (data) => {
  return request({
    url: 'api/wtphm-service/diagnosticRecord/getWindturbineAndCompInfo',
    method: 'get',
    params: { ...data }
  })
}

// 分析记录报告表诊断时间获取
export const getDiaRecordReportListByWindturbineId = (data) => {
  return request({
    url: 'api/wtphm-service/diagnosticRecord/getDiaRecordReportListByWindturbineId',
    method: 'get',
    params: { ...data }
  })
}

// 特征值统计
export const getDefinitionEigenValueByWindturbineId = (data) => {
  return request({
    url: 'api/wtphm-service/diagnosticRecord/getDefinitionEigenValueByWindturbineId',
    method: 'get',
    params: { ...data }
  })
}

// 保存诊断记录报告
export const saveDiagnosticRecordReport = (data) => {
  return request({
    url: 'api/wtphm-service/diagnosticRecord/saveDiagnosticRecordReport',
    method: 'post',
    data: { ...data }
  })
}

// 获取单机诊断记录报告
export const getDiaRecordReportList = (data) => {
  return request({
    url: 'api/wtphm-service/diagnosticRecord/getDiaRecordReportList',
    method: 'get',
    params: { ...data }
  })
}
// 根据月份分组单机报告任务表结构树
export const getReportTasksByMonth = (data) => {
  return request({
    url: 'api/wtphm-service/diagnosticRecord/getReportTasksByMonth',
    method: 'get',
    params: { ...data }
  })
}
// 保存单机报告任务
export const saveWindturbineReportTask = (data) => {
  return request({
    url: 'api/wtphm-service/diagnosticRecord/saveWindturbineReportTask',
    method: 'post',
    data: { ...data }
  })
}
// 保存风场报告
export const addWindParkReport = (data) => {
  return request({
    url: 'api/wtphm-service/diagnosticRecord/addWindParkReport',
    method: 'post',
    data: { ...data }
  })
}
// 更新单机报告任务
export const upWindturbineReportTaskByTerms = (data) => {
  return request({
    url: 'api/wtphm-service/diagnosticRecord/upWindturbineReportTaskByTerms',
    method: 'put',
    params: { ...data }
  })
}


// layout布局
export const getLayoutNameList = (data) => {
  return request({
    url: '/NetApi/BladeSystem/getPageLayout',
    method: 'get',
    params: { ...data }
  })
}

export const storeLayout = (data) => {
  return request({
    url: 'api/wtphm-service/diagnosticRecord/addPageLayout',
    method: 'post',
    data: { ...data }
  })
}
export const removeLayout = (data) => {
  return request({
    url: 'api/wtphm-service/diagnosticRecord/deletePageLayout',
    method: 'post',
    data: { ...data }
  })
}

// 波形列表获取
/* export const getWaveTableList = (data) => {
  return request({
    url: 'api/wtphm-service/measData/getWaveDataInfoList',
    method: 'get',
    params: { ...data }
  })
} */
// NET. 波形列表获取
export const getWaveTableList = (data) => {
  return request({
    url: 'NetApi/Analysis/MeasEventData',
    method: 'get',
    params: { ...data }
  })
}
export const getWaveTableDetailList = (data) => {
  return request({
    url: 'NetApi/Analysis/MeasEventDetails',
    method: 'get',
    params: { ...data }
  })
}

// 波形数据下载
/* export const downloadData = (data) => {
  return request({
    url: 'api/wtphm-service/measData/downloadOriginalWaveformData',
    method: 'post',
    data,
    responseType: 'blob'
  })
} */
export const downloadData = (data) => {
  return request({
    url: 'NetApi/Analysis/DownloadWaveData',
    method: 'post',
    data,
    responseType: 'blob'
  })
}

// 获取有数据的日期
/* export const getHasDataDates = (data) => {
  return request({
    url: 'api/wtphm-service/measData/getMeaslocEventList',
    method: 'get',
    params: { ...data }
  })
} */

export const getHasDataDates = (data) => {
  return request({
    url: 'NetApi/Analysis/HasDataDay',
    method: 'get',
    params: { ...data }
  })
}
//获取转频
export const getRF = (data) => {
  return request({
    url: 'NetApi/Analysis/GetRotorFrequency',
    method: 'get',
    params: { ...data }
  })
}
//获取轴承故障频率
export const getBeaSFaultFrequency = (data) => {
  return request({
    url: 'NetApi/Analysis/GetBearFaultFrequency',
    method: 'get',
    params: { ...data }
  })
}
// 获取边频
export const getSideFrequency = (data) => {
  return request({
    url: 'NetApi/Analysis/GetSideFrequency',
    method: 'get',
    params: { ...data }
  })
}
