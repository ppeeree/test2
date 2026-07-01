import request from '@/router/axios'

//获取诊断数据下载表格数据
export const getDayTableListApi = (param) => {
  return request({
    url: '/NetApi/DownloadTask/GetZipPathList',
    method: 'get',
    params: {
      ...param,
    }
  })
}
// 诊断数据下载: 下载csv文件
export const downloadCSVFileApi = (param) => {
  return request({
    url: '/NetApi/DownloadTask/DownloadCSVFile',
    method: 'get',
    params: {
      ...param,
    }
  })
}
// 删除下载记录
export const deleteTaskApi = (param) => {
  return request({
    url: '/NetApi/DownloadTask/DeleteDayZipList',
    method: 'post',
    data: {
      ...param,
    }
  })
}
// 自定义下载接口：获取风场配置的部件类型
export const getMeasTypeListApi = (param) => {
  return request({
    url: '/NetApi/DownloadTask/GetMonitorType',
    method: 'get',
    params: {
      ...param,
    }
  })
}

// 自定义下载接口：新增自定义下载
export const addTaskApi = (param) => {
  return request({
    url: '/NetApi/DownloadTask/AddTask',
    method: 'post',
    data: {
      ...param,
    }
  })
}

// 自定义下载接口：轮询调用，展示数据下载log
export const getDownloadLogsApi = (param) => {
  return request({
    url: '/NetApi/DownloadTask/GetDownloadLogs',
    method: 'get',
    params: {
      ...param,
    }
  })
}

// 自定义下载接口：获取下载波形的保存格式
export const getDownloadWaveSaveTypesApi = (param) => {
  return request({
    url: '/NetApi/DownloadTask/GetDownloadWaveSaveType',
    method: 'get',
    params: {
      ...param,
    }
  })
}