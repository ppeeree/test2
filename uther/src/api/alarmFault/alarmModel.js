import request from '@/router/axios'

// 模型关系树
export const alarmTree = param => {
  return request({
    url: '/api/diag-service/alarmDefine/getAlarmModelList',
    method: 'get',
    params: {
      ...param
    }
  })
}

//新增报警树
export const addTree = (data) => {
  return request({
    url: '/api/diag-service/alarmDefine/addAlarmModel',
    method: 'post',
    data: {
      ...data
    }
  })
}

//删除报警树
export const deleteTree = param => {
  return request({
    url: '/api/diag-service/alarmDefine/deleteAlarmModel',
    method: 'delete',
    params: {
      ...param
    }
  })
}

//导入模型文件
export const upload = param => {
  return request({
    url: '/api/diag-service/alarmDefine/upFileAlarmModel',
    method: 'post',
    params: {
      ...param
    }
  })
}

// 列表
export const getList = param => {
  return request({
    url: '/api/diag-service/alarmDefine/getAlarmSetUpByModelId',
    method: 'get',
    params: {
      ...param
    }
  })
}

//新增报警列表
export const add = (data) => {
  return request({
    url: '/api/diag-service/alarmDefine/addAlarmSetUp',
    method: 'post',
    data: {
      ...data
    }
  })
}

//修改
export const update = (data) => {
  return request({
    url: '/api/diag-service/alarmDefine/upAlarmSetUp',
    method: 'put',
    data: {
      ...data
    }
  })
}

//删除报警列表
export const remove = param => {
  return request({
    url: '/api/diag-service/alarmDefine/deleteAlarmSetUp',
    method: 'delete',
    params: {
      ...param
    }
  })
}

//获取全部部件
export const getAllComp = param => {
  return request({
    url: '/api/diag-service/alarmDefine/getCompPartAll',
    method: 'get',
    params: {
      ...param
    }
  })
}

//获取物理量
export const getUnits = param => {
  return request({
    url: '/api/diag-service/alarmDefine/getADFUnits',
    method: 'get',
    params: {
      ...param
    }
  })
}

//根据测点选择特征值
export const getSelectValue = param => {
  return request({
    url: '/api/diag-service/alarmDefine/getADFByOpt',
    method: 'get',
    params: {
      ...param
    }
  })
}
