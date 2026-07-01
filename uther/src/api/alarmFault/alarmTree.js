import request from '@/router/axios'

// 列表
export const getList = param => {
  return request({
    url: '/api/diag-service/alarmDefine/getAlarmModelActualList',
    method: 'get',
    params: {
      ...param
    }
  })
}

// 新增
export const add = (data) => {
  return request({
    url: '/api/diag-service/alarmDefine/addAlarmModelActual',
    method: 'post',
    data: {
      ...data
    }
  })
}

// 删除
export const remove = windturbineId => {
  return request({
    url: `/api/diag-service/alarmDefine/deleteAlarmModelActual/${windturbineId}`,
    method: 'delete'
  })
}
