import request from '@/router/axios'
// 首页展示

// 设备树
export const getDeptTree = (tenantId) => {
  return request({
    url: '/cesium/basic-server/frontPage/deptTree',
    method: 'get',
    params: {
      tenantId,
    }
  })
}

// 无人机状态数量
export const getUAVCount = (deptIds) => {
  return request({
    url: '/cesium/basic-server/frontPage/planeStatusStatistics',
    method: 'get',
    params: {
      deptIds,
    }
  })
}
  
// 各区域无人机数量
export const getAreaUAVCount = (data) => {
  return request({
    url: '/cesium/basic-server/frontPage/planeAreaStatistics',
    method: 'get',
    params: {
      ...data,
    }
  })
}
// 无人机场站位置信息等
export const getStationLocation = (data) => {
  return request({
    url: '/cesium/basic-server/frontPage/planeLocationStatistics',
    method: 'get',
    params: {
      ...data,
    }
  })
}

// 故障消缺管理
export const getDefectManage = (data) => {
  return request({
    url: '/cesium/basic-server/frontPage/defectManagement',
    method: 'get',
    params: {
      ...data,
    }
  })
}
// 巡检管理
export const getInspectManage = (data) => {
  return request({
    url: '/cesium/basic-server/frontPage/inpsectManagement',
    method: 'get',
    params: {
      ...data,
    }
  })
}
// 无人机作业管理
export const getPlaneManage = (data) => {
  return request({
    url: '/cesium/basic-server/frontPage/planeManagement',
    method: 'get',
    params: {
      ...data,
    }
  })
}

// 场站设备信息
export const getStationDepts = (data) => {
  return request({
    url: '/cesium/3D-server/station/device/list',
    method: 'get',
    params: {
      ...data,
    }
  })
}

// 视频获取地址
export const getVideoSrc = (params) => {
  return request({
    // url: '/api/app-business-server/app/uavData/planeMediaUrl',
    url: '/cesium/app-business-server/app/business/planeMediaUrl',
    method: 'get',
    params:params
  })
}
