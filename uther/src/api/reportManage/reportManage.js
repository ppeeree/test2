
import request from '@/router/axios'

//上传报告
export const addReport = (data) => {
  return request({
    headers: {
      "Content-Type": "multipart/form-data"
    },
    url: '/api/wtphm-service/diagnosticRecord/windParkReport/Upload',
    method: 'post',
    data
  })
}

// 报告列表接口
/* export const getList = (params) => {
  return request({
    url: '/api/wtphm-service/diagnosticRecord/getWindParkReportList',
    method: 'get',
    params: {
      ...params,
    }
  })
} */
export const getList = (params) => {
  return request({
    url: 'NetApi/DiagnosticReport/GetWindParkReportList',
    method: 'get',
    params: {
      ...params
    }
  })
}

// 删除报告
/* export const deleteReportApi = (params) => {
  return request({
    url: '/api/wtphm-service/diagnosticRecord/windParkReport/delete/' + params.id,
    method: 'delete',
  })
} */
export const deleteReportApi = (params) => {
  return request({
    url: 'NetApi/DiagnosticReport/DeleteWindParkDiagnosisReport',
    method: 'post',
    data: {
      ReportGuid: params.id,
      DiagReportGuid: params.id
    }
  })
}

// 报告base64获取
export const getReportBase64Code = (params) => {
  return request({
    url: '/api/wtphm-service/diagnosticRecord/base64DiagnoseReport',
    method: 'get',
    params: {
      ...params
    }
  })
}
