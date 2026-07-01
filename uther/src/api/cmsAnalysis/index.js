import request from '@/router/axios'

export const getSYWaveData = (id) => {
  return request({
    url: '/api/anylzer/devTimeDomainWaveApi/devTimeDomainWaveData?id=1',
    method: 'get',
    params: {
      id,
    }
  })
}
export const getTrendData = (param) => {
  return request({
    url: '/api/anylzer/EigenValueTrendApi/detailRowData?id=1',
    method: 'get',
    params: {
      ...param
    }
  })
}
export const getTrendDatas = (param) => {
  return request({
    url: '/api/anylzer/EigenValueTrendCopyApi/detailRowData',
    method: 'get',
    params: {
      ...param
    }
  })
}

export const getFFTWaveData = (showa) => {
  return request({
    url: '/api/anylzer/devTimeDomainWaveApi/devTimeDomainWaveDataListFFT?id=1',
    method: 'get',
    params: {
      ...showa
    }
  })
}

export const getDeviceTree = (showa) => {
  return request({
    url: '/api/anylzer/tree/deviceTree',
    method: 'get',
    params: {
      ...showa
    }
  })
}

// 滤波
export const postFilterFrequency = (param) => {
  return request({
    url: '/api/anylzer/filter/fromFrequencyDomain',
    method: 'post',
    data: {
      ...param
    }
  })
}
// 获取故障频率
export const getDamageFailureDatas = (param) => {
  return request({
    url: '/api/anylzer/damagefailure/damageFailureDatas',
    method: 'get',
    params:param
  })
}

// 特征值平滑处理
export const getTrendSmooth = (param) => {
  return request({
    url: '/api/anylzer/EigenValueTrendCopyApi/detailRowDataSmooth',
    method: 'get',
    params:param
  })
}

