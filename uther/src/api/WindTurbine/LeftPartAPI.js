import request from '@/router/axios' //导入已经写好的拦截器

//指数趋势--健康指数(左上)
const numTrendApi = (data) => {
  return request({
    url: 'api/wtphm-service/appData/healthStatus/getHealthIndex',
    method: 'get',
    params: { ...data }
  })
}

// 大部件健康状况 热力图
/* const compHealthStateApi = (data) => {
  return request({
    url: 'api/wtphm-service/appData/healthStatus/getHealthStatus',
    method: 'get',
    params: { ...data }
  })
} */
const compHealthStateApi = (data) => {
  return request({
    url: 'NetApi/Component/CompStatusTrend',
    method: 'get',
    params: { ...data }
  })
}

//部件故障
const compFaultStateApi = (data) => {
  return request({
    url: 'api/wtphm-service/appData/diagStatus/getEntityFault',
    method: 'get',
    params: { ...data }
  })
}
//部件故障弹出框
const compFaultHisStateApi = (data) => {
  return request({
    url: 'api/wtphm-service/appData/diagStatus/getEntityFaultTrend',
    method: 'get',
    params: { ...data }
  })
}
// 诊断报告
const failureReportApi = (entityId) => {
  return request({
    url: 'api/wtphm-service/diagStatus/getDiagReport',
    method: 'get',
    params: { entityId }
  })
}
// 历史诊断信息
const falutHistoryInfo = (data) => {
  return request({
    url: 'api/wtphm-service/appData/diagStatus/getFaultTrend',
    method: 'get',
    params: { ...data }
  })
}

export {
  numTrendApi, falutHistoryInfo, compHealthStateApi, compFaultHisStateApi, compFaultStateApi, failureReportApi
}