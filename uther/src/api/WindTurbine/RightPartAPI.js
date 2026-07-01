import request from '@/router/axios' //导入已经写好的拦截器

// 右上--事件跟踪
export const getEntityEventStatisticApi=(data)=>{
  return request({
    url: 'api/wtphm-service/appData/eventManage/getEventStatistic',
    methods: 'get',
    params: {
      ...data
    }
  })
}
// 右下 -- 发展趋势
export const getEventTrendApi=(data)=>{
  return request({
    url: 'api/wtphm-service/appData/eventManage/getEventTrend',
    methods: 'get',
    params: {
      ...data
    }
  })
}

