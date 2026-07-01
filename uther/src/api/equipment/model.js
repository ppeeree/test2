import request from '@/router/axios'

// 左侧树
export const getList = id => {
  return request({
    url: `/api/wtphm-service/deviceModel/getDeviceModel/${id}`,
    method: 'get',
    params: {
      id
    }
  })
}

// 获取聚合部件
export const getPagecomp = param => {
    return request({
        url: '/NetApi/BladeSystem/getPagecompAll',
        method: 'get',
        params: {
            ...param
        }
    })
}

//模型文件列表
export const getPath = param => {
  return request({
    url: '/api/wtphm-service/deviceModel/getModelFile',
    method: 'get',
    params: {
      ...param
    }
  })
}

//保存参数
export const save = data => {
  return request({
    url: '/api/wtphm-service/deviceModel/save',
    method: 'post',
    data: {
      ...data
    }
  })
}

//保存参数
export const remove = id => {
  return request({
    url: `/api/wtphm-service/deviceModel/delete/${id}`,
    method: 'delete',
    params: {
      id
    }
  })
}

//保存参数
export const update = data => {
  return request({
    url: '/api/wtphm-service/deviceModel/update',
    method: 'put',
    data: {
      ...data
    }
  })
}
