import request from '@/router/axios' //导入已经写好的拦截器

// 机组信息
export const getUnitApi = data => {
  return request({
    url: '/NetApi/BladeSystem/getEntityBasicInfo',
    method: 'get',
    params: { ...data }
  })
}

//机组页部件卡片
export const getUnitFaultApi = data => {
  return request({
    url: 'api/wtphm-service/appData/diagStatus/getEntityFaultInfo',
    method: 'get',
    params: { ...data }
  })
}

/* export const getLatestTurbine = data => {
  return request({
    url: 'api/wtphm-service/measData/getLatestEntityValue',
    method: 'get',
    params: { ...data }
  })
} */

// NET. 获取实时特针值列表
/* export const getLatestTurbine = data => {
  return request({
    url: 'NetApi/Component/CompEvList',
    method: 'get',
    params: { ...data }
  })
} */
export const getLatestTurbine = data => {
  return request({
    url: 'NetApi/Component/CompEvStatusList',
    method: 'get',
    params: { ...data }
  })
}
// 特征值及特征值趋势获取
export const getEigenValueApi = data => {
  return request({
    url: 'api/wtphm-service/measData/getEigenValue',
    method: 'get',
    params: { ...data }
  })
}

// 特征值趋势
export const getEigenTrendApi = data => {
  return request({
    url: 'api/wtphm-service/appData/measData/getEigenValueTrend',
    method: 'get',
    params: { ...data }
  })
}

// 波形数据
/* export const getEigenWaveTrendApi = data => {
  return request({
    url: 'api/wtphm-service/measData/getWaveformData',
    method: 'get',
    params: { ...data }
  })
} */
// NET. 波形数据
export const getEigenWaveTrendApi = data => {
  return request({
    url: 'NetApi/Analysis/WaveList',
    method: 'get',
    params: { ...data }
  })
}

// SCADA数据
export const getSacdaDataApi = data => {
  return request({
    url: 'api/wtphm-service/measData/getEntityScada',
    method: 'get',
    params: { ...data }
  })
}

// 更多特征值数据
export const getAllEvListTreeApi = data => {
  return request({
    url: 'api/wtphm-service/measData/getEvDataListByMeaslocList',
    method: 'get',
    params: { ...data }
  })
}
// 特征值筛选下拉
export const getOptions = data => {
  return request({
    url: 'api/wtphm-service/measData/getEvDataListByMeasloc',
    method: 'get',
    params: { ...data }
  })
}
// 故障特征值接口
export const getDamageFailureDatas = data => {
  return request({
    url: 'api/wtphm-service/appData/diagStatus/getDamageFailure',
    method: 'get',
    params: { ...data }
  })
}

// 滤波
export const postFilterFrequency = param => {
  return request({
    url: '/api/wtphm-service/appData/diagStatus/waveFilter',
    method: 'post',
    data: {
      ...param
    }
  })
}

// 获取部件健康状态
export const getCompStatusApi = data => {
  return request({
    url: 'api/wtphm-service/healthStatus/getEntityStatus',
    method: 'get',
    params: { ...data }
  })
}

// 获取设备型号+测量方案
export const getModelPlanApi = param => {
  return request({
    url: '/NetApi/BladeSystem/getFanMeasureConfigInfo',
    method: 'get',
    params: { ...param }
  })
}


// 获取设备型号+测量方案
export const infoParentTreeApi = windTurbineId => {
  return request({
    url: `api/wtphm-service/deviceModel/infoParentTree/${windTurbineId}`,
    method: 'get',
    params: { windTurbineId }
  })
}

