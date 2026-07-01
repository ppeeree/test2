import request from '@/router/axios'
import dayjs from 'dayjs'

// 首页展示

/*******
 * @description: 风场机组统计信息
 * @param {*} stationId 	风场ID(1564479552429559809)
 * @return {*}
 */
const getUserTurbinesInfo = data => {
  return request({
    url: 'NetApi/WindPark/GetStationControlInfo',
    method: 'get',
    params: {
      ...data
    }
  })
}
const getWindTurbineStatistics = stationId => {
  return request({
    url: 'NetApi/WindPark/WindParkInfo',
    method: 'get',
    params: {
      ...stationId
    }
  })
}
/*******
 * @description: 风场机组统计信息
 * @param {*} stationId 	风场ID(1564479552429559809)
 * @return {*}
 */
/* const getWindTurbineMonitorNum = stationId => {
  if (!stationId) {
    stationId =
      JSON.parse(sessionStorage.getItem('selectWindFarm'))?.id ||
      JSON.parse(localStorage.getItem('saber-userInfo'))?.content.dept_id ||
      null
  }
  return request({
    url: 'api/wtphm-service/dataStatistics/getWindTurbineMonitorNumberStatistics',
    method: 'get',
    params: {
      stationId
    }
  })
} */

// NET. 获取风场信息
const getWindTurbineMonitorNum = stationId => {
  return request({
    url: 'NetApi/WindPark/WindParkMonitorNum',
    method: 'get',
    params: {
      ...stationId
    }
  })
}


/*******
 * @description: 风场无人机机巢统计信息
 * @param {*} stationId 风场ID(1564479552429559809)
 * @param {*} startTime 开始时间(2022-07-01)
 * @param {*} endTime 结束事件(2022-10-01)
 * @return {*}
 */
const getUavBoxStatistics = stationId => {
  if (!stationId) {
    stationId =
      JSON.parse(sessionStorage.getItem('selectWindFarm'))?.id ||
      JSON.parse(localStorage.getItem('saber-userInfo'))?.content.dept_id ||
      null
  }
  return request({
    url: 'api/wtphm-service/dataStatistics/getUavBoxStatistics',
    method: 'get',
    params: {
      stationId,
      endTime: dayjs().format('YYYY-MM-DD HH:mm:ss'),
      startTime: dayjs().subtract(3, 'month').format('YYYY-MM-DD HH:mm:ss')
    }
  })
}

/*******
 * @description: 风场部件故障统计
 * @param {*} stationId 风场ID(1564479552429559809)
 * @param {*} startTime 开始时间(2022-07-01)
 * @param {*} endTime 结束事件(2022-10-01)
 * @return {*}
 */
const faultStatistic = (stationId, startTime, endTime) => {
  if (!stationId) {
    stationId =
      JSON.parse(sessionStorage.getItem('selectWindFarm'))?.id ||
      JSON.parse(localStorage.getItem('saber-userInfo'))?.content.dept_id ||
      null
  }
  return request({
    url: 'api/wtphm-service/dataStatistics/faultStatistic',
    method: 'get',
    params: {
      stationId,
      startTime,
      endTime
    }
  })
}
/** */
/* const getCompFaultLevelCount = stationId => {
  if (!stationId) {
    stationId =
      JSON.parse(sessionStorage.getItem('selectWindFarm'))?.id ||
      JSON.parse(localStorage.getItem('saber-userInfo'))?.content.dept_id ||
      null
  }
  return request({
    url: 'api/wtphm-service/dataStatistics/getCompFaultStatus/v3',// 'api/wtphm-service/dataStatistics/getFaultTrend',
    method: 'get',
    params: {
      stationId,
      endTime: dayjs().format('YYYY-MM-DD HH:mm:ss'),
      startTime: dayjs().subtract(3, 'month').format('YYYY-MM-DD HH:mm:ss')
    }
  })
} */
// NET. 获取部件报警状态
const getCompFaultLevelCount = data => {
  return request({
    url: 'NetApi/WindPark/WindParkCompStatusList',
    method: 'get',
    params: {
      ...data,
      endTime: dayjs().format('YYYY-MM-DD HH:mm:ss'),
      startTime: dayjs().subtract(3, 'month').format('YYYY-MM-DD HH:mm:ss')
    }
  })
}


/*******
 * @description: 风场部件故障趋势
 * @param {*} stationId 风场ID(1564479552429559809)
 * @param {*} startTime 开始时间(2022-07-01)
 * @param {*} endTime 结束事件(2022-10-01)
 * @return {*}
 */
/* const getFaultTrend = stationId => {
  if (!stationId) {
    stationId =
      JSON.parse(sessionStorage.getItem('selectWindFarm'))?.id ||
      JSON.parse(localStorage.getItem('saber-userInfo'))?.content.dept_id ||
      null
  }
  return request({
    url: 'api/wtphm-service/dataStatistics/getCompFaultStatusTrend/v3',// 'api/wtphm-service/dataStatistics/getFaultTrend',
    method: 'get',
    params: {
      stationId,
      endTime: dayjs().format('YYYY-MM-DD HH:mm:ss'),
      startTime: dayjs().subtract(3, 'month').format('YYYY-MM-DD HH:mm:ss')
    }
  })
} */

// NET 部件故障趋势
const getFaultTrend = data => {
  return request({
    url: 'NetApi/WindPark/WindParkCompTrend',// 'api/wtphm-service/dataStatistics/getFaultTrend',
    method: 'get',
    params: {
      ...data,
      endTime: dayjs().format('YYYY-MM-DD HH:mm:ss'),
      startTime: dayjs().subtract(3, 'month').format('YYYY-MM-DD HH:mm:ss')
    }
  })
}

/*******
 * @description: 风场机组健康状态统计
 * @param {*} stationId 风场ID(1564479552429559809)
 * @return {*}
 */
/* const healthStatusStatistic = stationId => {
  if (!stationId) {
    stationId =
      JSON.parse(sessionStorage.getItem('selectWindFarm'))?.id ||
      JSON.parse(localStorage.getItem('saber-userInfo'))?.content.dept_id ||
      null
  }
  return request({
    url: 'api/wtphm-service/dataStatistics/healthStatusStatistic',
    method: 'get',
    params: {
      stationId
    }
  })
}
 */
// NET 机组健康趋势
const healthStatusStatistic = data => {
  return request({
    url: 'NetApi/WindPark/WindTurbineWeekStatus',
    method: 'get',
    params: {
      ...data
    }
  })
}

/*******
 * @description: 风场机组健康状态趋势
 * @param {*} stationId 风场ID(1564479552429559809)
 * @param {*} startTime 开始时间(2022-07-01)
 * @param {*} endTime 结束事件(2022-10-01)
 * @return {*}
 */
/* const healthStatusTrend = (stationId, startTime, endTime) => {
  if (!stationId) {
    stationId =
      JSON.parse(sessionStorage.getItem('selectWindFarm'))?.id ||
      JSON.parse(localStorage.getItem('saber-userInfo'))?.content.dept_id ||
      null
  }
  return request({
    url: 'api/wtphm-service/dataStatistics/healthStatusTrend',
    method: 'get',
    params: {
      stationId,
      endTime: endTime || dayjs().format('YYYY-MM-DD HH:mm:ss'),
      startTime: startTime || dayjs().subtract(3, 'month').format('YYYY-MM-DD HH:mm:ss')
    }
  })
} */

const healthStatusTrend = (data) => {
  return request({
    url: 'NetApi/WindPark/WindTurbineStatusTrend',
    method: 'get',
    params: {
      ...data,
      endTime: dayjs().format('YYYY-MM-DD HH:mm:ss'),
      startTime: dayjs().subtract(3, 'month').format('YYYY-MM-DD HH:mm:ss')
    }
  })
}

export {
  getWindTurbineStatistics,
  getWindTurbineMonitorNum,
  getUavBoxStatistics,
  faultStatistic,
  getFaultTrend,
  healthStatusStatistic,
  healthStatusTrend,
  getCompFaultLevelCount,
  getUserTurbinesInfo
}
