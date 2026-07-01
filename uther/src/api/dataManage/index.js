import request from '@/router/axios'

//  列表
export const getList = (param) => {
  return request({
    url: '/api/wtphm-service/DataSync/getPackageTasks',
    method: 'get',
    params: {
      ...param
    }
  })
}

//下载设置
/* export const addPackageTaskApi = params => {
  return request({
    url: '/api/wtphm-service/DataSync/addPackageTask',
    method: 'post',
    data: {
      ...params
    }
  })
} */
export const addPackageTaskApi = params => {
  return request({
    url: 'NetApi/ACHOutPut/AddDownloadTask',
    method: 'post',
    data: {
      ...params
    }
  })
}
// 删除
export const remove = (param) => {
  return request({
    url: '/api/wtphm-service/DataSync/deletePackageTask',
    method: 'get',
    params: {
      ...param
    }
  })
}

// 下载
export const downLoadFile = (param) => {
  return request({
    url: '/api/wtphm-service/DataSync/downloadPackageFile',
    method: 'get',
    params: {
      ...param
    }
  })
}

//清理
export const clearPath = (param) => {
  return request({
    url: '/api/wtphm-service/DataSync/clearPackageFile',
    method: 'get',
    params: {
      ...param
    }
  })
}

// 数据量检查
export const getCheckDataApi = (param) => {
  return request({
    url: 'NetApi/DataVolumeCheck/TimeRangeWaveCount',
    method: 'get',
    params: {
      ...param
    }
  })
}