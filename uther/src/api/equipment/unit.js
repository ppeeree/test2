import request from '@/router/axios'

// 列表
export const getList = param => {
  return request({
    url: '/api/wtphm-service/EntityConfiguration/getDauConfigList',
    method: 'get',
    params: {
      ...param
    }
  })
}

//新增
export const add = (data) => {
  return request({
    url: '/api/wtphm-service/EntityConfiguration/addDauConfig',
    method: 'post',
    data: {
      ...data
    }
  })
}

// 删除
export const remove = (data) => {
  return request({
    url: '/api/wtphm-service/EntityConfiguration/deleteDauConfig',
    method: 'delete',
    data: {
      ...data
    }
  })
}

//修改
export const update = (data) => {
  return request({
    url: '/api/wtphm-service/EntityConfiguration/upDauConfig',
    method: 'put',
    data: {
      ...data
    }
  })
}

//根据机组匹配测点
export const getMeasureListApi = (windturbineIds) => {
  return request({
    url: '/api/wtphm-service/EntityConfiguration/getPartMeasureListByWindturbineId',
    method: 'get',
    params: {
      windturbineIds,
    }
  })
}