import request from '@/router/axios'
// 后端开发：Liu Dingding
// 前端：Hugaiping
// 轴承故障
// 列表
export const getFaultList = param => {
  return request({
    url: '/api/wtphm-service/bearing/faultKnowledge/list',
    method: 'get',
    params: {
      ...param
    }
  })
}

// 删除
export const removeFault = id => {
  return request({
    url: '/api/wtphm-service/bearing/faultKnowledge/delete/' + id,
    method: 'delete',
    /*   params: {
        ...param
      } */
  })
}

//新增
export const addFault = (data) => {
  return request({
    headers: {
      "Content-Type": "multipart/form-data"
    },
    url: '/api/wtphm-service/bearing/faultKnowledge/save',
    method: 'post',
    data
  })
}

//修改
export const updateFault = data => {
  return request({
    headers: {
      "Content-Type": "multipart/form-data"
    },
    url: '/api/wtphm-service/bearing/faultKnowledge/update',
    method: 'put',
    data
  })
}

//故障程度词典表
export const getBearingFaultLevelApi = param => {
  return request({
    url: '/api/wtphm-service/bearing/fault/severityDefList',
    method: 'get',
    params: {
      ...param
    }
  })
}

//获取故障类型词典表
export const getBearingFaultTypeApi = param => {
  return request({
    url: '/api/wtphm-service/bearing/faultKnowledge/faultTypeList',
    method: 'get',
    params: {
      ...param
    }
  })
}

//获取故障设备部件词典表
export const getFaultCompApi = param => {
  return request({
    url: '/api/wtphm-service/bearing/fault/deviceComponents',
    method: 'get',
    params: {
      ...param
    }
  })
}


/**轴承设备管理 */
// 轴承设备列表
export const getList = param => {
  return request({
    url: '/api/wtphm-service/bearing/management/list',
    method: 'get',
    params: {
      ...param
    }
  })
}

// 删除
export const remove = param => {
  return request({
    url: '/api/wtphm-service/bearing/management/delete/',
    method: 'delete',
    params: {
      ...param
    }
  })
}

//新增
export const add = (data) => {
  return request({
    url: '/api/wtphm-service/bearing/management/save',
    method: 'post',
    data
  })
}

//修改
export const update = data => {
  return request({
    url: '/api/wtphm-service/bearing/management/update',
    method: 'put',
    data
  })
}

// 轴承厂家型号
export const getBearingVendorApi = param => {
  return request({
    url: '/api/wtphm-service/bearing/management/factoryAndModelList',
    method: 'get',
    params: {
      ...param
    }
  })
}
