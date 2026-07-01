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
