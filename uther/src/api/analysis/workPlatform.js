import request from '@/router/axios' //导入已经写好的拦截器

// 获取单个风场机组树
export const getWindTurbineReportTree = param => {
  return request({
    url: 'NetApi/DiagnosticReport/GetWindParkTurbineTree',
    method: 'get',
    params: {
      ...param
    }
  })
}

// 删除机组报告
export const deleteTurbineReport = param => {
  // reportGuid
  return request({
    url: 'NetApi/DiagnosticReport/DeleteDiagnosisReport',
    method: 'post',
    data: {
      ...param
    }
  })
}

// 获取机组诊断报告详情
export const getTurbineReportDetail = param => {
  // reportGuid
  return request({
    url: 'NetApi/DiagnosticReport/GetDiagnosisReport',
    method: 'get',
    params: {
      ...param
    }
  })
}

// 保存机组诊断报告详情
export const saveTurbineReportDetail = param => {
  // reportGuid
  return request({
    url: 'NetApi/DiagnosticReport/SaveDiagnosisReport',
    method: 'post',
    data: {
      ...param
    }
  })
}

// 获取分析记录树
export const getTurbineAnalysisTree = param => {
  // reportGuid
  return request({
    url: 'NetApi/DiagnosticReport/GetAnalyzerRecordTree',
    method: 'get',
    params: {
      ...param
    }
  })
}
// 获取风场报告历史树结构
export const getWindparkHistoryTree = param => {
  // reportGuid
  return request({
    url: 'NetApi/DiagnosticReport/GetWindParkDiagnosisReportTree',
    method: 'get',
    params: {
      ...param
    }
  })
}
// 获取机组诊断结论
export const getTurbineConclusion = param => {
  // windturbineId
  return request({
    url: 'NetApi/DiagnosticReport/GetDiagnosisConclusion',
    method: 'get',
    params: {
      ...param
    }
  })
}

// 获取风场报告信息
export const getWindParkReportDetail = param => {
  return request({
    url: 'NetApi/DiagnosticReport/GetWindParkReportSummaryInfo',
    method: 'get',
    params: {
      ...param
    }
  })
}

// 获取已有的风场报告信息
export const getWindParkExitReportDetail = param => {
  return request({
    url: 'NetApi/DiagnosticReport/GetWindParkDiagReportDetail',
    method: 'get',
    params: {
      ...param
    }
  })
}

// 保存风场报告信息
export const saveWindParkDiagReport = param => {
  // reportGuid
  return request({
    url: 'NetApi/DiagnosticReport/SaveWindParkDiagReport',
    method: 'post',
    data: {
      ...param
    }
  })
}