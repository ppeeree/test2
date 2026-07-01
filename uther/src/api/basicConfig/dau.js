import request from '@/router/axios'

//系统自检
export const getStationMonitorApi = (param) => {
  return request({
    url: '/NetApi/SystemCheck/GetStationMonitorTypeList',
    method: 'get',
    params: {
      ...param,
    }
  })
}

//机组采集器状态数据
export const getHADUStatusListApi = (param) => {
  return request({
    url: '/NetApi/SystemCheck/GetHADUStatusList',
    method: 'get',
    params: {
      ...param,
    }
  })
}
// 获取单个机组通道状态列表
export const getTurbineDAUListApi = (param) => {
  return request({
    url: '/NetApi/SystemCheck/GetHADUChannelStatusList',
    method: 'get',
    params: {
      ...param,
    }
  })
}
// 获取DAU偏置电压趋势数据
export const getChartDataApi = (param) => {
  return request({
    url: '/NetApi/SystemCheck/GetTimeframeMonitorData',
    method: 'get',
    params: {
      ...param,
    }
  })
}
// 获取晃度和倾角仪自检数据
export const getModBusListApi = (param) => {
  return request({
    url: '/NetApi/SystemCheck/GetModbusStatusList',
    method: 'get',
    params: {
      ...param,
    }
  })
}

// 获取风场采集器类型
export const getEchoChartDataApi = (param) => {
  return request({
    url: '/NetApi/SystemCheck/GetBFMEchoWaveData',
    method: 'get',
    params: {
      ...param,
    }
  })
}

// 更新机组IP
export const updateHADUChannelMapperIPApi = (param) => {
  return request({
    url: '/NetApi/HADUStatus/UpdateHADUChannelMapperIP',
    method: 'post',
    data: {
      ...param,
    }
  })
}

//更新通道号
export const updateHADUChannelNumApi = (param) => {
  return request({
    url: '/NetApi/HADUStatus/UpdateHADUChannelNum',
    method: 'post',
    data: {
      ...param,
    }
  })
}
//更新ModbusIP
export const updateHADUModbusApi = (param) => {
  return request({
    url: '/NetApi/HADUStatus/UpdateHADUModbus',
    method: 'post',
    data: {
      ...param,
    }
  })
}
// 获取倾角仪列表
export const getModbusListApi = (param) => {
  return request({
    url: '/NetApi/HADUStatus/GetModbusList',
    method: 'get',
    params: {
      ...param,
    }
  })
}

// 采集单元手动配置，新增配置页面接口
// 获取风场下测点列表接口
export const getMeasIDByStationIDApi = (param) => {
  return request({
    url: '/NetApi/HADUChannelConfig/GetMeasIDByStationID',
    method: 'get',
    params: {
      ...param,
    }
  })
}
// 获取表格数据
export const getHADUConfigListApi = (param) => {
  return request({
    url: '/NetApi/HADUChannelConfig/GetHADUConfigList',
    method: 'get',
    params: {
      ...param,
    }
  })
}

// 表格：新增配置接口
export const addHADUConfigApi = (param) => {
  return request({
    url: '/NetApi/HADUChannelConfig/AddHADUConfig',
    method: 'post',
    data: {
      ...param,
    }
  })
}
// 表格：编辑配置接口
export const updateHADUConfigApi = (param) => {
  return request({
    url: '/NetApi/HADUChannelConfig/UpdateHADUConfig',
    method: 'post',
    data: {
      ...param,
    }
  })
}

// 表格：删除配置接口
export const deleteHADUConfigApi = (param) => {
  return request({
    url: '/NetApi/HADUChannelConfig/DeleteHADUConfig',
    method: 'get',
    params: {
      ...param,
    }
  })
}
//机组所有采集数据
export const getTurbineTableListApi = (param) => {
  return request({
    url: '/NetApi/HADUStatus/GetHADUStatusList',
    method: 'get',
    params: {
      ...param,
    }
  })
}
