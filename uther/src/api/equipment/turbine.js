import request from '@/router/axios'

// 查询多风场风机详细信息
const detailFan = data => {
  return request({
    url: 'api/wtphm-service/newFan/detailFan',
    method: 'get',
    params: {
      ...data
    }
  })
}

const deleteFan = id => {
  return request({
    url: 'api/wtphm-service/newFan/deleteFanDelete',
    method: 'post',
    params: {
      id
    }
  })
}

const getProvince = () => {
  return request({
    url: 'api/wtphm-service/newFan/getProvince',
    method: 'get'
  })
}

const insertFan = data => {
  return request({
    url: 'api/wtphm-service/newFan/insertFan',
    method: 'post',
    data: {
      ...data
    }
  })
}

const updateFanDelete = data => {
  return request({
    url: 'api/wtphm-service/newFan/updateFanDelete',
    method: 'post',
    data: {
      ...data
    }
  })
}

const downTemplate = () => {
  return request({
    url: 'api/wtphm-service/newFan/downTemplate',
    method: 'post',
    responseType: 'blob'
  })
}

const upload = (data) => {
  return request({
    url: 'api/wtphm-service/newFan/upload',
    method: 'post',
    data
  })
}

export { detailFan, deleteFan, getProvince, insertFan, updateFanDelete, downTemplate, upload }
