import request from '@/router/axios'

//获取报告列表
export const getDiagnoseTasksApi = param => {
  return request({
    url: '/api/wtphm-service/diagnosticRecord/getDiagnoseTasks',
    method: 'get',
    params: {
      ...param
    }
  })
}

//添加诊断任务
export const addDiagnoseTaskApi = param => {
  return request({
    url: '/api/wtphm-service/diagnosticRecord/addDiagnoseTask',
    method: 'post',
    data: {
      ...param
    }
  })
}

//删除诊断任务
export const deleteDiagnoseTaskApi = param => {
  return request({
    url: '/api/wtphm-service/diagnosticRecord/deleteDiagnoseTask',
    method: 'get',
    params: {
      ...param
    }
  })
}


//获取诊断任务
export const selectDiagnoseTaskApi = param => {
  return request({
    url: '/api/wtphm-service/diagnosticRecord/selectDiagnoseTask',
    method: 'get',
    params: {
      ...param
    }
  })
}

//保存诊断任务
export const saveDiagnoseTaskInfoApi = param => {
  return request({
    url: '/api/wtphm-service/diagnosticRecord/saveDiagnoseTaskInfo',
    method: 'post',
    data: {
      ...param
    }
  })
}

//new -- 获取报告列表
export const getWindParkReportsApi = param => {
  return request({
    url: '/api/wtphm-service/diagnosticRecord/getWindParkReports',
    method: 'get',
    params: {
      ...param
    }
  })
}

//new -- 查看报告内容
export const selectWindParkReportApi = param => {
  return request({
    url: '/api/wtphm-service/diagnosticRecord/selectWindParkReport',
    method: 'get',
    params: {
      ...param
    }
  })
}
//new -- 正常机组后台自动生成诊断报告
export const creatNormalTurbineReport = param => {
  return request({
    url: '/api/wtphm-service/diagnosticRecord/addWindturbineHealthDiagnosticRecord',
    method: 'get',
    params: {
      ...param
    }
  })
}

// 获取风场报告信息
export const getWindParkReportApi = param => {
  return request({
    url: '/api/wtphm-service/diagnosticRecord/getWindParkInfo',
    method: 'get',
    params: {
      ...param
    }
  })
}

//new -- 更新报告
export const updateWindParkReportApi = param => {
  return request({
    url: '/api/wtphm-service/diagnosticRecord/updateWindParkReport',
    method: 'post',
    data: {
      ...param
    }
  })
}

//下载报告
export const downloadDiagnoseReportApi = param => {
  return request({
    url: '/api/wtphm-service/diagnosticRecord/downloadDiagnoseReport',
    method: 'get',
    params: {
      ...param
    },
    responseType: 'arraybuffer',
    headers: {
      'Content-Type': 'application/octet-stream'
    }
  })
}

// 诊断结论下拉列表及对应的维护建议

export const getDefaultDiagResultApi = param => {
  return request({
    url: 'NetApi/DiagnosticReport/GetDefaultDiagResult',
    method: 'get',
    params: {
      ...param
    }
  })
}

// 获取机组运行建议词典
export const getDefaultRuningAdviceApi = param => {
  return request({
    url: 'NetApi/DiagnosticReport/GetDefaultRuningAdvice',
    method: 'get',
    params: {
      ...param
    }
  })
}

// 添加分析记录时获取默认值
export const getDefaultAnalysisRecord = param => {
  return request({
    url: 'NetApi/DiagnosticReport/GetLastAnalyzerDescAndDiagnosisConclusion',
    method: 'get',
    params: {
      ...param
    }
    // data: JSON.stringify(param)
  })
}

// 保存分析记录及机组诊断结论
export const saveAnalysisRecord = param => {
  return request({
    url: 'NetApi/DiagnosticReport/SaveAnalyzerRecordAndDiagnosisConclusion',
    method: 'post',
    data: {
      ...param
    }
  })
}