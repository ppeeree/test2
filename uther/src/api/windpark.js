import request from '@/router/axios'
//机组页部件卡片
export const getTurbineHisStateApi = data => {
  return request({
    url: 'api/wtphm-service/dataStatistics/deptTypeForPlace/windturbineHealthTrend',
    method: 'get',
    params: { ...data }
  })
}