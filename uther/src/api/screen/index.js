import request from '@/router/axios'

// 根据用户获取用户的地图上层区域的资源
const getUserMapInfo = (param) => {
  return request({
    url: '/NetApi/WindPark/GetMapInfoByUserID',
    method: 'get',
    params: {
      ...param
    }
  })
}
// 机组模型接口的数据
const getFJModelData = (vendors_provided_id) => {
  return request({
    url: 'api/main-server/fanModelAPI/detailfanModelData',
    method: 'get',
    params: {
      vendors_provided_id
    }
  })
}

/* const getFanModelData = (stationId) => {
  if (!stationId) {
    stationId = JSON.parse(sessionStorage.getItem('selectWindFarm'))?.id || JSON.parse(localStorage.getItem('saber-userInfo'))?.content.dept_id || null
  }
  return request({
    url: 'api/wtphm-service/appData/windTurbine/getFanModelData',
    method: 'get',
    params: {
      stationId
    }
  })
} */
const getAllWindParkInfo = (param) => {
  return request({
    url: '/NetApi/WindPark/GetStationInfoList',
    method: 'get',
    params: {
      ...param
    }
  })
}

const getFanModelData = (stationId) => {
  if (!stationId) {
    stationId = JSON.parse(sessionStorage.getItem('selectWindFarm'))?.id || JSON.parse(localStorage.getItem('saber-userInfo'))?.content.dept_id || null
  }
  return request({
    url: '/NetApi/WindPark/GetDeviceCoordinatesList',
    method: 'get',
    params: {
      stationId
    }
  })
}

const getWeather = () => {
  return request({
    url: 'https://v0.yiketianqi.com/api',
    method: 'get',
    params: {
      appid: '14959126',
      version: 'v61',
      unescape: '1',
      appsecret: '6rJ4fXvu'
    }
  })
}

/******* 
 * @description: 查看设备类型信息
 * @param {Number} deptCode 
 * @return {*}
 */
const getEnitiyListByEntityType = (deptCode = 9000) => {
  return request({
    url: 'api/wtphm-service/accountModelInfo/getEnitiyListByEntityType',
    method: 'get',
    params: {
      deptCode
    }
  })
}

/******* 
 * @description: 获取场站列表
 * @param {*} code 
 * @param {*} type 类型(行政区域(region)/组织架构(dept))
 * @return {*}
 */
const getStationList = (type, code) => {
  return request({
    url: '/NetApi/BladeSystem/getStationList',
    method: 'get',
    params: {
      code,
      type
    }
  })
}
// NET. 获取风场列表
/* const getStationList = (type, code) => {
  return request({
    url: 'NetApi/WindPark/WindParkListAPI',
    method: 'get',
    params: {
      code,
      type
    }
  })
} */

/******* 
 * @description: 查看设备树信息
 * @param {*} stationId 	风场ID(1564479552429559809)
 * userId 用户Id
 * @return {*}
 */
/* const getEnitiyTree = (param) => {
  debugger
  return request({
    url: 'api/wtphm-service/accountSpaceBasicInfo/getEnitiyTree',
    method: 'get',
    params: {
      ...param
    }
  })
} */
const getEnitiyTree = (param) => {
  return request({
    url: 'NetApi/WindPark/AllDeviceList',
    method: 'get',
    params: {
      ...param
    }
  })
}

/******* 
 * @description: 事件信息
 * @param {*} stationId 	风场ID(1564479552429559809)
 * @return {*}
 */
const getUnitDefaultCard = (stationId) => {
  return request({
    url: 'api/wtphm-service/appData/diagStatus/getEventInformation',
    method: 'get',
    params: {
      stationId
    }
  })
}

/******* 
 * @description: 分类结果
 * @param {*} type 类型
 * @param {*} value 具体
 * @return {*} 
 */
const getClassificationResults = (data) => {
  return request({
    url: 'api/wtphm-service/appData/eventManage/getEventIndexByCond',
    method: 'get',
    params: {
      ...data
    }
  })
}

/* const windFieldStatusApi = (data) => {
  return request({
    url: 'api/wtphm-service/dataStatistics/deptTypeForPlace/healthStatusStatistic',
    method: 'get',
    params: {
      ...data
    }
  })
} */

//NET 根据风场Code获取机组列表
const windFieldStatusApi = (data) => {
  return request({
    url: 'NetApi/WindPark/WindTurbineStatusList',
    method: 'get',
    params: {
      ...data
    }
  })
}

export { getFJModelData, getUserMapInfo, getAllWindParkInfo, getWeather, getEnitiyListByEntityType, getStationList, getEnitiyTree, getFanModelData, getUnitDefaultCard, getClassificationResults, windFieldStatusApi }