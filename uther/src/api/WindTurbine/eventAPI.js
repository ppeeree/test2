import request from '@/router/axios' //导入已经写好的拦截器

//事件详情接口
export const getEventDetailApi = data => {
  return request({
    url: 'api/wtphm-service/appData/eventManage/getEventDetail',
    methods: 'get',
    params: {
      ...data
    }
  })
}

export const postEventApi = (param) => {
  return request({
    url: 'api/wtphm-service/appData/eventManage/insertEventDisposeWay',
    method: 'post',
    data: {
      ...param
    }
  })
}

//获取四级事件
export const getImportEventApi = data => {
  return request({
    url: 'api/wtphm-service/appData/eventManage/getEventByDegree',
    methods: 'get',
    params: {
      ...data
    }
  })
}