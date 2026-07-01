import request from '@/router/axios'

// 列表
export const getList = param => {
  return request({
    url: '/api/wtphm-service/EntityConfiguration/getPartMeasureList',
    method: 'get',
    params: {
      ...param
    }
  })
}

// 删除
export const remove = param => {
  return request({
    url: '/api/wtphm-service/EntityConfiguration/deletePartMeasure',
    method: 'delete',
    params: {
      ...param
    }
  })
}

//新增
export const add = (data) => {
  return request({
    url: '/api/wtphm-service/EntityConfiguration/addPartMeasure',
    method: 'post',
    data: {
      ...data
    }
  })
}

//修改
export const update = data => {
  return request({
    url: '/api/wtphm-service/EntityConfiguration/upPartMeasure',
    method: 'put',
    data: {
      ...data
    }
  })
}

// 测点状态页面
// 列表
export const getSatusList = param => {
  return request({
    url: '/api/wtphm-service/analyzerData/sensordcdata/list',
    method: 'get',
    params: {
      ...param
    }
  })
}

// 偏置电压趋势
export const getVoltValue = param => {
  return request({
    url: '/api/wtphm-service/analyzerData/sensordcdata/trendChart',
    method: 'get',
    params: {
      ...param
    }
  })
}
