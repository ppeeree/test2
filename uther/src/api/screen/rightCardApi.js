import request from '@/router/axios'

// 首页展示

/*******
 * @description: 风场综合指数排名统计信息
 * @param {*} stationId 风场ID(1564479552429559809)
 * @param {*} startTime	起始时间2022-09-15 00:00:00
 * @param {*} endTime 结束时间2022-11-15 00:00:00
 * @return {*}
 */
const getWindTurbineIndexRankingStatistics = (stationId, startTime, endTime) => {
  return request({
    url: 'api/wtphm-service/dataStatistics/getWindTurbineIndexRankingStatistics',
    method: 'get',
    params: {
      stationId,
      startTime,
      endTime
    }
  })
}

/*******
 * @description: 事件跟踪详情
 * @param {*} id 机组ID/风场ID
 * @param {*} type	WINDRURBINE/WINDPARK
 * @param {*} endTime
 * @param {*} startTime
 * @return {*}
 */
const getEventStatistic = data => {
  return request({
    url: 'api/wtphm-service/appData/eventManage/getEventStatistic',
    method: 'get',
    params: { ...data }
  })
}

/*******
 * @description: 事件发展趋势
 * @param {*} id 机组ID/风场ID
 * @param {*} type	WINDRURBINE/WINDPARK
 * @param {*} endTime
 * @param {*} startTime
 * @return {*}
 */
const getEventTrend = data => {
  return request({
    url: 'api/wtphm-service/appData/eventManage/getEventTrend',
    method: 'get',
    params: { ...data }
  })
}

export {
  getWindTurbineIndexRankingStatistics,
  getEventStatistic,
  getEventTrend
}
