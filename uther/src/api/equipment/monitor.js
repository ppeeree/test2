import request from '@/router/axios'

// 获取方案
export const getPlan = (id, param) => {
  return request({
    url: `/api/wtphm-service/DeviceMeasloc/getModelMeaslocConf/${id}`,
    method: 'get',
    params: {
      id,
      ...param
    }
  })
}

// 保存方案
export const savePlan = data => {
  return request({
    url: '/api/wtphm-service/DeviceMeasloc/savePlan',
    method: 'post',
    data: {
      ...data
    }
  })
}

// 删除方案
export const remove = id => {
  return request({
    url: `/api/wtphm-service/DeviceMeasloc/delete/${id}`,
    method: 'delete'
  })
}

// 新建获取监测设备
export const getDeviceApi = param => {
  return request({
    url: '/api/wtphm-service/DeviceMeasloc/getMonitorsDevice',
    method: 'get',
    params: {
      ...param
    }
  })
}

// 保存配置
export const saveMark = data => {
  return request({
    url: '/api/wtphm-service/DeviceMeasloc/saveMeaslocPlan',
    method: 'post',
    data: {
      ...data
    }
  })
}

// 获取测点默认值
export const getInitPosition = data => {
  return request({
    url: '/api/wtphm-service/DeviceMeasloc/getMeasureCodeDefaultInfo',
    method: 'get',
    data: {
      ...data
    }
  })
}
