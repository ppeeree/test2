import request from '@/router/axios'

/* export const getListApi = (data) => {
  return request({
    url: 'NetApi/IntelliDiag/LastIntelliDiagRecords',
    method: 'get',
    params: {
      ...data
    }
  })
} */
export const getWindParkTreeApi = (data) => {
  return request({
    url: 'NetApi/WindDeviceTree/WindParkTree',
    method: 'get',
    params: {
      ...data
    }
  })
}
export const getWindTurbineTreeApi = (data) => {
  return request({
    url: 'NetApi/WindDeviceTree/WindTurbineTree',
    method: 'get',
    params: {
      ...data
    }
  })
}

// 获取诊断结果汇总统计信息
export const getSummaryInfoApi = (data) => {
  return request({
    url: 'NetApi/IntelliDiag/IntelliDiagSummary',
    method: 'get',
    params: {
      ...data
    }
  })
}

// 获取诊断结果列表
export const getDiagResultListApi = (data) => {
  return request({
    url: 'NetApi/IntelliDiag/IntelliDiagResults',
    method: 'get',
    params: {
      ...data
    }
  })
}

// 获取诊断最新一次的记录列表
export const getLastIntelliDiagRecordsApi = (data) => {
  return request({
    url: 'NetApi/IntelliDiag/LastIntelliDiagRecords',
    method: 'post',
    data: {
      ...data
    }
  })
}
// 获取诊断记录列表
export const getIntelliDiagRecordsApi = (data) => {
  return request({
    url: 'NetApi/IntelliDiag/TimeRangeIntelliDiagRecords',
    method: 'post',
    data: {
      ...data
    }
  })
}

/**诊断模型配置 */

// 获取诊断模型树+列表
export const getModelTreeApi = (data) => {
  return request({
    url: 'NetApi/IntelliDiag/GetDiagModelTree',
    method: 'get',
    params: {
      ...data
    }
  })
}

// 获取诊断模型参数默认或者已有配置
export const getModelParamsApi = (data) => {
  return request({
    url: 'NetApi/IntelliDiag/GetDiagModelDetail',
    method: 'get',
    params: {
      ...data
    }
  })
}

// 保存诊断模型参数
export const saveModelParamsApi = (data) => {
  return request({
    url: 'NetApi/IntelliDiag/SaveDiagModelDetail',
    method: 'post',
    data: {
      ...data
    }
  })
}

// 删除诊断模型参数配置
export const deleteModelApi = (data) => {
  return request({
    url: 'NetApi/IntelliDiag/DeleteDiagModel',
    method: 'get',
    params: {
      ...data
    }
  })
}

/**诊断模型应用 */
// 应用诊断模型
export const applyModelApi = (data) => {
  return request({
    url: 'NetApi/IntelliDiag/DiagModelLinkWindturbine',
    method: 'post',
    data: {
      ...data
    }
  })
}

// 获取诊断模型应用列表`
export const getModelApplyListApi = (data) => {
  return request({
    url: 'NetApi/IntelliDiag/GetWindParkDiagModelApplyDetail',
    method: 'get',
    params: {
      ...data
    }
  })
}
