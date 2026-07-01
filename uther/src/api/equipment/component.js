import request from '@/router/axios'

// 列表
export const getList = param => {
  return request({
    url: '/api/wtphm-service/EntityConfiguration/getPartConfigList',
    method: 'get',
    params: {
      ...param
    }
  })
}

// 删除
export const remove = param => {
  return request({
    url: '/api/wtphm-service/EntityConfiguration/deleteCompNorm',
    method: 'delete',
    params: {
      ...param
    }
  })
}

//新增
export const add = (data) => {
  return request({
    url: '/api/wtphm-service/EntityConfiguration/addCompNorm',
    method: 'post',
    data: {
      ...data
    }
  })
}

//修改
export const update = param => {
  return request({
    url: '/api/wtphm-service/EntityConfiguration/updateCompNorm',
    method: 'put',
    params: {
      ...param
    }
  })
}

//根据型号获取参数
export const getCompParametersApi = param => {
  return request({
    url: '/api/wtphm-service/EntityConfiguration/getCompParameters',
    method: 'get',
    params: {
      ...param
    }
  })
}

//获取全部部件
export const getCompApi = param => {
  return request({
    url: '/api/wtphm-service/EntityConfiguration/getCompPartAll',
    method: 'get',
    params:{
      ...param
    }
  })
}

//根据聚合部件获取测量位置
export const getDeviceMonitorByFentitycode = param => {
  return request({
    url: '/api/wtphm-service/EntityConfiguration/getDeviceMonitorByFentitycode',
    method: 'get',
    params:{
      ...param
    }
  })
}

//根据部件拿到厂商和型号
export const getModelApi = param => {
  return request({
    url: '/api/wtphm-service/EntityConfiguration/getMetaDataByMetaModelName',
    method: 'get',
    params: {
      ...param
    }
  })
}

export const getDeviceParamApi = param =>{
  return request({
    url: '/api/wtphm-service/EntityConfiguration/deviceDefaultProperty',
    method: 'get',
    params: {
      ...param
    }
  })
}

export const getFactoryModelApi = param =>{
  return request({
    url: '/api/wtphm-service/EntityConfiguration/deviceFactoryAndModelList',
    method: 'get',
    params: {
      ...param
    }
  })
}