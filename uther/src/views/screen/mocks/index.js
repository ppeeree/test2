export function createListData() {
  const data = {
    data: {
      code: 200,
      success: true,
      data: [
        {
          deviceName: '001#',
          createTime: '211212212',
          id: 1,
          state: 'Normal'
        },
        {
          deviceName: '002#',
          createTime: '211212212',
          id: 2,
          state: 'careful'
        },
        {
          deviceName: '004#',
          createTime: '211212212',
          id: 3,
          state: 'DANGER'
        },
        {
          deviceName: '5555555555555555',
          createTime: '211212212',
          id: 4,
          state: 'Warning'
        },
        {
          deviceName: '005#',
          createTime: '211212212',
          id: 5,
          state: 'DANGER'
        },
        {
          deviceName: '006#',
          createTime: '211212212',
          id: 6,
          state: 'Warning'
        }
      ],
      msg: '操作成功'
    },
    status: 200,
    statusText: 'OK',
    headers: {
      connection: 'close',
      'content-type': 'application/json;charset=UTF-8',
      date: 'Fri, 12 Aug 2022 02:16:40 GMT',
      server: 'nginx/1.20.2',
      'transfer-encoding': 'chunked',
      'x-powered-by': 'Express'
    },
    config: {
      transformRequest: {},
      transformResponse: {},
      timeout: 300000,
      xsrfCookieName: 'XSRF-TOKEN',
      xsrfHeaderName: 'X-XSRF-TOKEN',
      maxContentLength: -1,
      headers: {
        Accept: 'application/json, text/plain, */*',
        Authorization: 'Basic c2FiZXI6c2FiZXJfc2VjcmV0',
        'Blade-Auth':
          'bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ0ZW5hbnRfaWQiOiIwMDAwMDAiLCJ1c2VyX25hbWUiOiJhZG1pbiIsInJlYWxfbmFtZSI6IueuoeeQhuWRmCIsImF2YXRhciI6Imh0dHBzOi8vZ3cuYWxpcGF5b2JqZWN0cy5jb20vem9zL3Jtc3BvcnRhbC9CaWF6ZmFueG1hbU5Sb3h4VnhrYS5wbmciLCJhdXRob3JpdGllcyI6WyJhZG1pbmlzdHJhdG9yIl0sImNsaWVudF9pZCI6InNhYmVyIiwicm9sZV9uYW1lIjoiYWRtaW5pc3RyYXRvciIsImxpY2Vuc2UiOiJwb3dlcmVkIGJ5IGJsYWRleCIsInBvc3RfaWQiOiIxMTIzNTk4ODE3NzM4Njc1MjAxIiwidXNlcl9pZCI6IjExMjM1OTg4MjE3Mzg2NzUyMDEiLCJyb2xlX2lkIjoiMTEyMzU5ODgxNjczODY3NTIwMSIsInNjb3BlIjpbImFsbCJdLCJuaWNrX25hbWUiOiLnrqHnkIblkZgiLCJvYXV0aF9pZCI6IiIsImRldGFpbCI6eyJ0eXBlIjoid2ViIn0sImV4cCI6MTY2MDI3MjQ5NiwiZGVwdF9pZCI6IjExMjM1OTg4MTM3Mzg2NzUyMDEiLCJqdGkiOiJkNzFjYzk3MC0xNTdjLTQwOTAtYWViYS03Yjg4ODlmY2U2NTIiLCJhY2NvdW50IjoiYWRtaW4ifQ.uv8ujJjrUTzPkR3OSO2LQJ117VE0F5gtro2ua1q6Tc4'
      },
      withCredentials: true,
      method: 'get',
      url: '/cesium/3D-server/station/device/list',
      params: {
        stationGuid: '',
        deptId: '1501818445505761282'
      }
    },
    request: {}
  }
  return data
}

export function mockListUAVData() {
  const data = [
    '0',
    '107.520349',
    '33.984809',
    4600,
    '10',
    '107.520740',
    '33.984228',
    4600,
    '20',
    '107.518694',
    '33.978311',
    4600,
    '30',
    '107.513647',
    '33.969877',
    4600,
    '40',
    '107.509573',
    '33.961327',
    4600,
    '50',
    '107.504204',
    '33.951362',
    4600,
    '60',
    '107.486295',
    '33.955010',
    4600,
    '70',
    107.472624,
    33.954283,
    4600,
    '80',
    107.458362,
    33.949541,
    4600,
    '90',
    107.427582,
    33.942311,
    4600,
    '100',
    107.391872,
    33.961465,
    4600,
    '120',
    107.398043,
    33.970011,
    4600,
    '140',
    107.485557,
    33.962639,
    4600
  ]
  return data
}

export function mockGetEigenValue() {
  return {
    code: 200,
    success: true,
    data: {
      name: '塔筒',
      time: '2023-03-09 16:51:40',
      data: [
        {
          compId: '36aa02911e3645e5b55c0d20c30fa51c',
          compName: '塔顶',
          compState: '',
          time: '2023-03-09 16:51:40',
          top: '12',
          left: '50',
          boxTop: '-3',
          boxLeft: '90',
          flang: [
            {
              bladeName: '倾角',
              bladeData: []
            }
          ]
        },
        {
          compId: '039814b004574901aef54527513cdc28',
          compName: '塔基',
          compState: '',
          time: '2023-03-09 16:51:40',
          top: '55',
          left: '-175',
          boxTop: '84',
          boxLeft: '90',
          flang: [
            {
              bladeName: '倾角',
              bladeData: []
            }
          ]
        },
        {
          compId: '6592e7de4ce9472383784127d79c0390',
          compName: '钢绞线',
          compState: 'normal',
          time: '2023-03-09 16:51:40',
          top: '25',
          left: '50',
          boxTop: '-3',
          boxLeft: '-36',
          flang: [
            {
              bladeName: '1',
              bladeData: [
                {
                  eigenValue: 11100,
                  eigenValueCode: 'CBF',
                  eigenValueId: 'TWP001001STLU1A0OC&&CBF',
                  eigenValueName: '张拉力',
                  eigenValueStatus: '',
                  eigenValueUnit: 'kN',
                  judgment: false,
                  vdi: false
                },
                {
                  eigenValue: 0.0207,
                  eigenValueCode: 'IRF03',
                  eigenValueId: 'TWP001001STLU1A0OC&&IRF03',
                  eigenValueName: '2阶固有频率',
                  eigenValueStatus: '',
                  eigenValueUnit: 'Hz',
                  judgment: false,
                  vdi: false
                },
                {
                  eigenValue: 0.0191,
                  eigenValueCode: 'IRF03',
                  eigenValueId: 'TWP001001STLU1A0OC&&IRF03',
                  eigenValueName: '3阶固有频率',
                  eigenValueStatus: '',
                  eigenValueUnit: 'Hz',
                  judgment: false,
                  vdi: false
                }
              ]
            },
            {
              bladeName: '2',
              bladeData: [
                {
                  eigenValue: 11100,
                  eigenValueCode: 'CBF',
                  eigenValueId: 'TWP001001STLU1A0OC&&CBF',
                  eigenValueName: '张拉力',
                  eigenValueStatus: '',
                  eigenValueUnit: 'kN',
                  judgment: false,
                  vdi: false
                },
                {
                  eigenValue: 0.0207,
                  eigenValueCode: 'IRF03',
                  eigenValueId: 'TWP001001STLU1A0OC&&IRF03',
                  eigenValueName: '2阶固有频率',
                  eigenValueStatus: '',
                  eigenValueUnit: 'Hz',
                  judgment: false,
                  vdi: false
                },
                {
                  eigenValue: 0.0191,
                  eigenValueCode: 'IRF03',
                  eigenValueId: 'TWP001001STLU1A0OC&&IRF03',
                  eigenValueName: '3阶固有频率',
                  eigenValueStatus: '',
                  eigenValueUnit: 'Hz',
                  judgment: false,
                  vdi: false
                }
              ]
            },
            {
              bladeName: '3',
              bladeData: [
                {
                  eigenValue: 11100,
                  eigenValueCode: 'CBF',
                  eigenValueId: 'TWP001001STLU1A0OC&&CBF',
                  eigenValueName: '张拉力',
                  eigenValueStatus: '',
                  eigenValueUnit: 'kN',
                  judgment: false,
                  vdi: false
                },
                {
                  eigenValue: 0.0207,
                  eigenValueCode: 'IRF03',
                  eigenValueId: 'TWP001001STLU1A0OC&&IRF03',
                  eigenValueName: '2阶固有频率',
                  eigenValueStatus: '',
                  eigenValueUnit: 'Hz',
                  judgment: false,
                  vdi: false
                },
                {
                  eigenValue: 0.0191,
                  eigenValueCode: 'IRF03',
                  eigenValueId: 'TWP001001STLU1A0OC&&IRF03',
                  eigenValueName: '3阶固有频率',
                  eigenValueStatus: '',
                  eigenValueUnit: 'Hz',
                  judgment: false,
                  vdi: false
                }
              ]
            },
            {
              bladeName: '4',
              bladeData: [
                {
                  eigenValue: 11100,
                  eigenValueCode: 'CBF',
                  eigenValueId: 'TWP001001STLU1A0OC&&CBF',
                  eigenValueName: '张拉力',
                  eigenValueStatus: '',
                  eigenValueUnit: 'kN',
                  judgment: false,
                  vdi: false
                },
                {
                  eigenValue: 0.0207,
                  eigenValueCode: 'IRF03',
                  eigenValueId: 'TWP001001STLU1A0OC&&IRF03',
                  eigenValueName: '2阶固有频率',
                  eigenValueStatus: '',
                  eigenValueUnit: 'Hz',
                  judgment: false,
                  vdi: false
                },
                {
                  eigenValue: 0.0191,
                  eigenValueCode: 'IRF03',
                  eigenValueId: 'TWP001001STLU1A0OC&&IRF03',
                  eigenValueName: '3阶固有频率',
                  eigenValueStatus: '',
                  eigenValueUnit: 'Hz',
                  judgment: false,
                  vdi: false
                }
              ]
            },
            {
              bladeName: '5',
              bladeData: [
                {
                  eigenValue: 11100,
                  eigenValueCode: 'CBF',
                  eigenValueId: 'TWP001001STLU1A0OC&&CBF',
                  eigenValueName: '张拉力',
                  eigenValueStatus: '',
                  eigenValueUnit: 'kN',
                  judgment: false,
                  vdi: false
                },
                {
                  eigenValue: 0.0207,
                  eigenValueCode: 'IRF03',
                  eigenValueId: 'TWP001001STLU1A0OC&&IRF03',
                  eigenValueName: '2阶固有频率',
                  eigenValueStatus: '',
                  eigenValueUnit: 'Hz',
                  judgment: false,
                  vdi: false
                },
                {
                  eigenValue: 0.0191,
                  eigenValueCode: 'IRF03',
                  eigenValueId: 'TWP001001STLU1A0OC&&IRF03',
                  eigenValueName: '3阶固有频率',
                  eigenValueStatus: '',
                  eigenValueUnit: 'Hz',
                  judgment: false,
                  vdi: false
                }
              ]
            },
            {
              bladeName: '6',
              bladeData: [
                {
                  eigenValue: 11100,
                  eigenValueCode: 'CBF',
                  eigenValueId: 'TWP001001STLU1A0OC&&CBF',
                  eigenValueName: '张拉力',
                  eigenValueStatus: '',
                  eigenValueUnit: 'kN',
                  judgment: false,
                  vdi: false
                },
                {
                  eigenValue: 0.0207,
                  eigenValueCode: 'IRF03',
                  eigenValueId: 'TWP001001STLU1A0OC&&IRF03',
                  eigenValueName: '2阶固有频率',
                  eigenValueStatus: '',
                  eigenValueUnit: 'Hz',
                  judgment: false,
                  vdi: false
                },
                {
                  eigenValue: 0.0191,
                  eigenValueCode: 'IRF03',
                  eigenValueId: 'TWP001001STLU1A0OC&&IRF03',
                  eigenValueName: '3阶固有频率',
                  eigenValueStatus: '',
                  eigenValueUnit: 'Hz',
                  judgment: false,
                  vdi: false
                }
              ]
            }
          ]
        },
        {
          compId: '6592e7de4ce9472383784127d79c0390',
          compName: '钢绞线',
          compState: 'normal',
          time: '2023-03-09 16:51:40',
          top: '73',
          left: '50',
          boxTop: '49',
          boxLeft: '-36',
          flang: [
            {
              bladeName: '1',
              bladeData: [
                {
                  eigenValue: 11100,
                  eigenValueCode: 'CBF',
                  eigenValueId: 'TWP001001STLU1A0OC&&CBF',
                  eigenValueName: '张拉力',
                  eigenValueStatus: '',
                  eigenValueUnit: 'kN',
                  judgment: false,
                  vdi: false
                },
                {
                  eigenValue: 0.0207,
                  eigenValueCode: 'IRF03',
                  eigenValueId: 'TWP001001STLU1A0OC&&IRF03',
                  eigenValueName: '2阶固有频率',
                  eigenValueStatus: '',
                  eigenValueUnit: 'Hz',
                  judgment: false,
                  vdi: false
                },
                {
                  eigenValue: 0.0191,
                  eigenValueCode: 'IRF03',
                  eigenValueId: 'TWP001001STLU1A0OC&&IRF03',
                  eigenValueName: '3阶固有频率',
                  eigenValueStatus: '',
                  eigenValueUnit: 'Hz',
                  judgment: false,
                  vdi: false
                }
              ]
            },
            {
              bladeName: '2',
              bladeData: [
                {
                  eigenValue: 11100,
                  eigenValueCode: 'CBF',
                  eigenValueId: 'TWP001001STLU1A0OC&&CBF',
                  eigenValueName: '张拉力',
                  eigenValueStatus: '',
                  eigenValueUnit: 'kN',
                  judgment: false,
                  vdi: false
                },
                {
                  eigenValue: 0.0207,
                  eigenValueCode: 'IRF03',
                  eigenValueId: 'TWP001001STLU1A0OC&&IRF03',
                  eigenValueName: '2阶固有频率',
                  eigenValueStatus: '',
                  eigenValueUnit: 'Hz',
                  judgment: false,
                  vdi: false
                },
                {
                  eigenValue: 0.0191,
                  eigenValueCode: 'IRF03',
                  eigenValueId: 'TWP001001STLU1A0OC&&IRF03',
                  eigenValueName: '3阶固有频率',
                  eigenValueStatus: '',
                  eigenValueUnit: 'Hz',
                  judgment: false,
                  vdi: false
                }
              ]
            },
            {
              bladeName: '3',
              bladeData: [
                {
                  eigenValue: 11100,
                  eigenValueCode: 'CBF',
                  eigenValueId: 'TWP001001STLU1A0OC&&CBF',
                  eigenValueName: '张拉力',
                  eigenValueStatus: '',
                  eigenValueUnit: 'kN',
                  judgment: false,
                  vdi: false
                },
                {
                  eigenValue: 0.0207,
                  eigenValueCode: 'IRF03',
                  eigenValueId: 'TWP001001STLU1A0OC&&IRF03',
                  eigenValueName: '2阶固有频率',
                  eigenValueStatus: '',
                  eigenValueUnit: 'Hz',
                  judgment: false,
                  vdi: false
                },
                {
                  eigenValue: 0.0191,
                  eigenValueCode: 'IRF03',
                  eigenValueId: 'TWP001001STLU1A0OC&&IRF03',
                  eigenValueName: '3阶固有频率',
                  eigenValueStatus: '',
                  eigenValueUnit: 'Hz',
                  judgment: false,
                  vdi: false
                }
              ]
            },
            {
              bladeName: '4',
              bladeData: [
                {
                  eigenValue: 11100,
                  eigenValueCode: 'CBF',
                  eigenValueId: 'TWP001001STLU1A0OC&&CBF',
                  eigenValueName: '张拉力',
                  eigenValueStatus: '',
                  eigenValueUnit: 'kN',
                  judgment: false,
                  vdi: false
                },
                {
                  eigenValue: 0.0207,
                  eigenValueCode: 'IRF03',
                  eigenValueId: 'TWP001001STLU1A0OC&&IRF03',
                  eigenValueName: '2阶固有频率',
                  eigenValueStatus: '',
                  eigenValueUnit: 'Hz',
                  judgment: false,
                  vdi: false
                },
                {
                  eigenValue: 0.0191,
                  eigenValueCode: 'IRF03',
                  eigenValueId: 'TWP001001STLU1A0OC&&IRF03',
                  eigenValueName: '3阶固有频率',
                  eigenValueStatus: '',
                  eigenValueUnit: 'Hz',
                  judgment: false,
                  vdi: false
                }
              ]
            },
            {
              bladeName: '5',
              bladeData: [
                {
                  eigenValue: 11100,
                  eigenValueCode: 'CBF',
                  eigenValueId: 'TWP001001STLU1A0OC&&CBF',
                  eigenValueName: '张拉力',
                  eigenValueStatus: '',
                  eigenValueUnit: 'kN',
                  judgment: false,
                  vdi: false
                },
                {
                  eigenValue: 0.0207,
                  eigenValueCode: 'IRF03',
                  eigenValueId: 'TWP001001STLU1A0OC&&IRF03',
                  eigenValueName: '2阶固有频率',
                  eigenValueStatus: '',
                  eigenValueUnit: 'Hz',
                  judgment: false,
                  vdi: false
                },
                {
                  eigenValue: 0.0191,
                  eigenValueCode: 'IRF03',
                  eigenValueId: 'TWP001001STLU1A0OC&&IRF03',
                  eigenValueName: '3阶固有频率',
                  eigenValueStatus: '',
                  eigenValueUnit: 'Hz',
                  judgment: false,
                  vdi: false
                }
              ]
            },
            {
              bladeName: '6',
              bladeData: [
                {
                  eigenValue: 11100,
                  eigenValueCode: 'CBF',
                  eigenValueId: 'TWP001001STLU1A0OC&&CBF',
                  eigenValueName: '张拉力',
                  eigenValueStatus: '',
                  eigenValueUnit: 'kN',
                  judgment: false,
                  vdi: false
                },
                {
                  eigenValue: 0.0207,
                  eigenValueCode: 'IRF03',
                  eigenValueId: 'TWP001001STLU1A0OC&&IRF03',
                  eigenValueName: '2阶固有频率',
                  eigenValueStatus: '',
                  eigenValueUnit: 'Hz',
                  judgment: false,
                  vdi: false
                },
                {
                  eigenValue: 0.0191,
                  eigenValueCode: 'IRF03',
                  eigenValueId: 'TWP001001STLU1A0OC&&IRF03',
                  eigenValueName: '3阶固有频率',
                  eigenValueStatus: '',
                  eigenValueUnit: 'Hz',
                  judgment: false,
                  vdi: false
                }
              ]
            }
          ]
        }
      ]
    },
    msg: '操作成功'
  }
}

export function mockGetEnitiyTree() {
  return {
    code: 200,
    success: true,
    data: [
      {
        code: 'TWP001',
        id: '1564479552429559809',
        name: '中丁尚堂风场',
        childNode: [
          {
            deptCode: 'TWP001',
            entityId: '5a4ac1a963524ef4b1c87b6600462776',
            entityName: '001#',
            entityCode: '',
            entityType: 'WINDTURBINE',
            healthStatus: 'nostate',
            childNode: [
              {
                deptCode: 'TWP001',
                entityId: '8deaa3e347b94d038e543a91bf8e46b6',
                entityName: '机舱',
                entityCode: '',
                entityType: 'NAC',
                healthStatus: 'nostate',
                childNode: [
                  {
                    deptCode: 'TWP001',
                    entityId: 'd72c63fbddc345c3b02f1248cbf964ce',
                    entityName: '齿轮箱',
                    entityCode: '',
                    entityType: 'GBX',
                    healthStatus: 'nostate',
                    childNode: []
                  },
                  {
                    deptCode: 'TWP001',
                    entityId: 'f5b84d60459246eb9479b5c74d32a74c',
                    entityName: '发电机',
                    entityCode: '',
                    entityType: 'GEN',
                    healthStatus: 'nostate',
                    childNode: []
                  },
                  {
                    deptCode: 'TWP001',
                    entityId: '5c2fe7c6064440e0ae4b5e8705d18d22',
                    entityName: '主轴',
                    entityCode: '',
                    entityType: 'MS',
                    healthStatus: 'nostate',
                    childNode: []
                  }
                ]
              },
              {
                deptCode: 'TWP001',
                entityId: 'ec1716a6e78542389f46ca39f71fa101',
                entityName: '风轮',
                entityCode: '',
                entityType: 'ROT',
                healthStatus: 'nostate',
                childNode: []
              },
              {
                deptCode: 'TWP001',
                entityId: 'f6374f53b6f14698043faec2319ce8a2',
                entityName: '塔筒',
                entityCode: '',
                entityType: 'TOW',
                healthStatus: 'nostate',
                childNode: [
                  {
                    deptCode: 'TWP001',
                    entityId: '039814b004574901aef54527513cdc28',
                    entityName: '塔基',
                    entityCode: '',
                    entityType: 'TOWFDA',
                    healthStatus: 'nostate',
                    childNode: []
                  },
                  {
                    deptCode: 'TWP001',
                    entityId: '36aa02911e3645e5b55c0d20c30fa51c',
                    entityName: '塔顶',
                    entityCode: '',
                    entityType: 'TOWTOP',
                    healthStatus: 'nostate',
                    childNode: []
                  },
                  {
                    deptCode: 'TWP001',
                    entityId: '1673fa67f5084416ae598c7bbf81f6e7',
                    entityName: '钢绞线一',
                    entityCode: '',
                    entityType: 'STL',
                    healthStatus: 'nostate',
                    childNode: []
                  },
                  {
                    deptCode: 'TWP001',
                    entityId: '75af66b323a8543cdebd69dd64b029da',
                    entityName: '钢绞线二',
                    entityCode: '',
                    entityType: 'STL',
                    healthStatus: 'nostate',
                    childNode: []
                  },
                  {
                    deptCode: 'TWP001',
                    entityId: 'bd7d882bbcbfa566515da06c2948ea2e',
                    entityName: '钢绞线三',
                    entityCode: '',
                    entityType: 'STL',
                    healthStatus: 'nostate',
                    childNode: []
                  },
                  {
                    deptCode: 'TWP001',
                    entityId: '063b193cbc9832d0dae9c148b64efa75',
                    entityName: '钢绞线四',
                    entityCode: '',
                    entityType: 'STL',
                    healthStatus: 'nostate',
                    childNode: []
                  },
                  {
                    deptCode: 'TWP001',
                    entityId: '9d14f87bcc7e10d9bc5ca639557a805c',
                    entityName: '钢绞线五',
                    entityCode: '',
                    entityType: 'STL',
                    healthStatus: 'nostate',
                    childNode: []
                  },
                  {
                    deptCode: 'TWP001',
                    entityId: 'd513907da9b0106ad105566db7b21b6f',
                    entityName: '钢绞线六',
                    entityCode: '',
                    entityType: 'STL',
                    healthStatus: 'nostate',
                    childNode: []
                  }
                ]
              }
            ]
          }
        ]
      }
    ],
    msg: '操作成功'
  }
}

export function mockListProEig() {
  return {
    code: 200,
    success: true,
    data: {
      name: '塔筒',
      time: '2023-02-24 09:57:49',
      data: [
        {
          compId: '62ced8c489fff4f86f2c091d64dbe9a0',
          compName: '塔顶',
          compState: '',
          time: '',
          top: '12',
          left: '50',
          boxTop: '-3',
          boxLeft: '90',
          flang: [
            {
              bladeName: '倾角',
              bladeData: []
            }
          ]
        },
        {
          compId: '6a79461bce1a83fe7e4e6906aa822a19',
          compName: '塔基',
          compState: 'normal',
          time: '2023-02-24 09:57:49',
          top: '55',
          left: '-175',
          boxTop: '84',
          boxLeft: '90',
          flang: [
            {
              bladeName: '倾角',
              bladeData: [
                {
                  eigenValue: '0.53',
                  eigenValueCode: 'XI',
                  eigenValueId: 'TWP001002TOWFDAXI&&XI',
                  eigenValueName: 'X倾角',
                  eigenValueStatus: '',
                  eigenValueUnit: '°',
                  judgment: true,
                  vdi: false
                },
                {
                  eigenValue: '0.76',
                  eigenValueCode: 'YI',
                  eigenValueId: 'TWP001002TOWFDAYI&&YI',
                  eigenValueName: 'Y倾角',
                  eigenValueStatus: '',
                  eigenValueUnit: '°',
                  judgment: true,
                  vdi: false
                }
              ]
            }
          ]
        },
        {
          compId: 'b8f7f32cdf7b4f569ef38fbaa2370aad',
          compName: '钢绞线',
          compState: 'normal',
          time: '2023-02-23 13:45:08',
          top: '25',
          left: '50',
          boxTop: '-3',
          boxLeft: '-36',
          flang: [
            {
              bladeName: '钢索上部',
              total: 20,
              bladeData: [
                [
                  {
                    number: '1',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '7653ea5766ac4e3aafcf9920bee7e282',
                    eigenValueName: '张拉力',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'kN',
                    judgment: false,
                    vdi: false
                  },
                  {
                    number: '1',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '5f2f0a26c02a48c499872afe1988fb9e',
                    eigenValueName: '固有频率',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'Hz',
                    judgment: false,
                    vdi: false
                  },
                  {
                    number: '1',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '16d7f8bcdd224f869cf582acb64de713',
                    eigenValueName: 'RMS',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'm/s',
                    judgment: false,
                    vdi: false
                  }
                ],
                [
                  {
                    number: '2',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '7653ea5766ac4e3aafcf9920bee7e282',
                    eigenValueName: '张拉力',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'kN',
                    judgment: false,
                    vdi: false
                  },
                  {
                    number: '2',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '5f2f0a26c02a48c499872afe1988fb9e',
                    eigenValueName: '固有频率',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'Hz',
                    judgment: false,
                    vdi: false
                  },
                  {
                    number: '2',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '16d7f8bcdd224f869cf582acb64de713',
                    eigenValueName: 'RMS',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'm/s',
                    judgment: false,
                    vdi: false
                  }
                ],
                [
                  {
                    number: '3',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '7653ea5766ac4e3aafcf9920bee7e282',
                    eigenValueName: '张拉力',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'kN',
                    judgment: false,
                    vdi: false
                  },
                  {
                    number: '3',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '5f2f0a26c02a48c499872afe1988fb9e',
                    eigenValueName: '固有频率',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'Hz',
                    judgment: false,
                    vdi: false
                  },
                  {
                    number: '3',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '16d7f8bcdd224f869cf582acb64de713',
                    eigenValueName: 'RMS',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'm/s',
                    judgment: false,
                    vdi: false
                  }
                ],
                [
                  {
                    number: '4',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '7653ea5766ac4e3aafcf9920bee7e282',
                    eigenValueName: '张拉力',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'kN',
                    judgment: false,
                    vdi: false
                  },
                  {
                    number: '4',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '5f2f0a26c02a48c499872afe1988fb9e',
                    eigenValueName: '固有频率',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'Hz',
                    judgment: false,
                    vdi: false
                  },
                  {
                    number: '4',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '16d7f8bcdd224f869cf582acb64de713',
                    eigenValueName: 'RMS',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'm/s',
                    judgment: false,
                    vdi: false
                  }
                ],
                [
                  {
                    number: '5',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '7653ea5766ac4e3aafcf9920bee7e282',
                    eigenValueName: '张拉力',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'kN',
                    judgment: false,
                    vdi: false
                  },
                  {
                    number: '5',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '5f2f0a26c02a48c499872afe1988fb9e',
                    eigenValueName: '固有频率',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'Hz',
                    judgment: false,
                    vdi: false
                  },
                  {
                    number: '5',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '16d7f8bcdd224f869cf582acb64de713',
                    eigenValueName: 'RMS',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'm/s',
                    judgment: false,
                    vdi: false
                  }
                ],
                [
                  {
                    number: '6',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '7653ea5766ac4e3aafcf9920bee7e282',
                    eigenValueName: '张拉力',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'kN',
                    judgment: false,
                    vdi: false
                  },
                  {
                    number: '6',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '5f2f0a26c02a48c499872afe1988fb9e',
                    eigenValueName: '固有频率',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'Hz',
                    judgment: false,
                    vdi: false
                  },
                  {
                    number: '6',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '16d7f8bcdd224f869cf582acb64de713',
                    eigenValueName: 'RMS',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'm/s',
                    judgment: false,
                    vdi: false
                  }
                ],
                [
                  {
                    number: '7',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '7653ea5766ac4e3aafcf9920bee7e282',
                    eigenValueName: '张拉力',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'kN',
                    judgment: false,
                    vdi: false
                  },
                  {
                    number: '7',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '5f2f0a26c02a48c499872afe1988fb9e',
                    eigenValueName: '固有频率',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'Hz',
                    judgment: false,
                    vdi: false
                  },
                  {
                    number: '7',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '16d7f8bcdd224f869cf582acb64de713',
                    eigenValueName: 'RMS',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'm/s',
                    judgment: false,
                    vdi: false
                  }
                ],
                [
                  {
                    number: '8',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '7653ea5766ac4e3aafcf9920bee7e282',
                    eigenValueName: '张拉力',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'kN',
                    judgment: false,
                    vdi: false
                  },
                  {
                    number: '8',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '5f2f0a26c02a48c499872afe1988fb9e',
                    eigenValueName: '固有频率',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'Hz',
                    judgment: false,
                    vdi: false
                  },
                  {
                    number: '8',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '16d7f8bcdd224f869cf582acb64de713',
                    eigenValueName: 'RMS',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'm/s',
                    judgment: false,
                    vdi: false
                  }
                ],
                [
                  {
                    number: '9',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '7653ea5766ac4e3aafcf9920bee7e282',
                    eigenValueName: '张拉力',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'kN',
                    judgment: false,
                    vdi: false
                  },
                  {
                    number: '2',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '5f2f0a26c02a48c499872afe1988fb9e',
                    eigenValueName: '固有频率',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'Hz',
                    judgment: false,
                    vdi: false
                  },
                  {
                    number: '3',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '16d7f8bcdd224f869cf582acb64de713',
                    eigenValueName: 'RMS',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'm/s',
                    judgment: false,
                    vdi: false
                  }
                ],
                [
                  {
                    number: '10',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '7653ea5766ac4e3aafcf9920bee7e282',
                    eigenValueName: '张拉力',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'kN',
                    judgment: false,
                    vdi: false
                  },
                  {
                    number: '2',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '5f2f0a26c02a48c499872afe1988fb9e',
                    eigenValueName: '固有频率',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'Hz',
                    judgment: false,
                    vdi: false
                  },
                  {
                    number: '3',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '16d7f8bcdd224f869cf582acb64de713',
                    eigenValueName: 'RMS',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'm/s',
                    judgment: false,
                    vdi: false
                  }
                ],
                [
                  {
                    number: '11',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '7653ea5766ac4e3aafcf9920bee7e282',
                    eigenValueName: '张拉力',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'kN',
                    judgment: false,
                    vdi: false
                  },
                  {
                    number: '2',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '5f2f0a26c02a48c499872afe1988fb9e',
                    eigenValueName: '固有频率',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'Hz',
                    judgment: false,
                    vdi: false
                  },
                  {
                    number: '3',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '16d7f8bcdd224f869cf582acb64de713',
                    eigenValueName: 'RMS',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'm/s',
                    judgment: false,
                    vdi: false
                  }
                ],
                [
                  {
                    number: '12',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '7653ea5766ac4e3aafcf9920bee7e282',
                    eigenValueName: '张拉力',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'kN',
                    judgment: false,
                    vdi: false
                  },
                  {
                    number: '2',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '5f2f0a26c02a48c499872afe1988fb9e',
                    eigenValueName: '固有频率',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'Hz',
                    judgment: false,
                    vdi: false
                  },
                  {
                    number: '3',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '16d7f8bcdd224f869cf582acb64de713',
                    eigenValueName: 'RMS',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'm/s',
                    judgment: false,
                    vdi: false
                  }
                ]
              ]
            }
          ]
        },
        {
          compId: 'b8f7f32cdf7b4f569ef38fbaa2370aad',
          compName: '钢绞线',
          compState: 'normal',
          time: '2023-02-23 13:45:08',
          top: '73',
          left: '50',
          boxTop: '49',
          boxLeft: '-36',
          flang: [
            {
              bladeName: '钢索下部',
              total: 20,
              bladeData: [
                [
                  {
                    number: '1',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '7653ea5766ac4e3aafcf9920bee7e282',
                    eigenValueName: '张拉力',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'kN',
                    judgment: false,
                    vdi: false
                  },
                  {
                    number: '2',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '5f2f0a26c02a48c499872afe1988fb9e',
                    eigenValueName: '固有频率',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'Hz',
                    judgment: false,
                    vdi: false
                  },
                  {
                    number: '3',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '16d7f8bcdd224f869cf582acb64de713',
                    eigenValueName: 'RMS',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'm/s',
                    judgment: false,
                    vdi: false
                  }
                ],
                [
                  {
                    number: '2',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '7653ea5766ac4e3aafcf9920bee7e282',
                    eigenValueName: '张拉力',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'kN',
                    judgment: false,
                    vdi: false
                  },
                  {
                    number: '3',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '5f2f0a26c02a48c499872afe1988fb9e',
                    eigenValueName: '固有频率',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'Hz',
                    judgment: false,
                    vdi: false
                  },
                  {
                    number: '3',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '16d7f8bcdd224f869cf582acb64de713',
                    eigenValueName: 'RMS',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'm/s',
                    judgment: false,
                    vdi: false
                  }
                ],
                [
                  {
                    number: '3',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '7653ea5766ac4e3aafcf9920bee7e282',
                    eigenValueName: '张拉力',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'kN',
                    judgment: false,
                    vdi: false
                  },
                  {
                    number: '2',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '5f2f0a26c02a48c499872afe1988fb9e',
                    eigenValueName: '固有频率',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'Hz',
                    judgment: false,
                    vdi: false
                  },
                  {
                    number: '3',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '16d7f8bcdd224f869cf582acb64de713',
                    eigenValueName: 'RMS',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'm/s',
                    judgment: false,
                    vdi: false
                  }
                ],
                [
                  {
                    number: '4',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '7653ea5766ac4e3aafcf9920bee7e282',
                    eigenValueName: '张拉力',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'kN',
                    judgment: false,
                    vdi: false
                  },
                  {
                    number: '2',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '5f2f0a26c02a48c499872afe1988fb9e',
                    eigenValueName: '固有频率',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'Hz',
                    judgment: false,
                    vdi: false
                  },
                  {
                    number: '3',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '16d7f8bcdd224f869cf582acb64de713',
                    eigenValueName: 'RMS',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'm/s',
                    judgment: false,
                    vdi: false
                  }
                ],
                [
                  {
                    number: '5',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '7653ea5766ac4e3aafcf9920bee7e282',
                    eigenValueName: '张拉力',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'kN',
                    judgment: false,
                    vdi: false
                  },
                  {
                    number: '2',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '5f2f0a26c02a48c499872afe1988fb9e',
                    eigenValueName: '固有频率',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'Hz',
                    judgment: false,
                    vdi: false
                  },
                  {
                    number: '3',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '16d7f8bcdd224f869cf582acb64de713',
                    eigenValueName: 'RMS',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'm/s',
                    judgment: false,
                    vdi: false
                  }
                ],
                [
                  {
                    number: '6',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '7653ea5766ac4e3aafcf9920bee7e282',
                    eigenValueName: '张拉力',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'kN',
                    judgment: false,
                    vdi: false
                  },
                  {
                    number: '2',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '5f2f0a26c02a48c499872afe1988fb9e',
                    eigenValueName: '固有频率',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'Hz',
                    judgment: false,
                    vdi: false
                  },
                  {
                    number: '3',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '16d7f8bcdd224f869cf582acb64de713',
                    eigenValueName: 'RMS',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'm/s',
                    judgment: false,
                    vdi: false
                  }
                ],
                [
                  {
                    number: '7',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '7653ea5766ac4e3aafcf9920bee7e282',
                    eigenValueName: '张拉力',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'kN',
                    judgment: false,
                    vdi: false
                  },
                  {
                    number: '2',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '5f2f0a26c02a48c499872afe1988fb9e',
                    eigenValueName: '固有频率',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'Hz',
                    judgment: false,
                    vdi: false
                  },
                  {
                    number: '3',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '16d7f8bcdd224f869cf582acb64de713',
                    eigenValueName: 'RMS',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'm/s',
                    judgment: false,
                    vdi: false
                  }
                ],
                [
                  {
                    number: '8',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '7653ea5766ac4e3aafcf9920bee7e282',
                    eigenValueName: '张拉力',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'kN',
                    judgment: false,
                    vdi: false
                  },
                  {
                    number: '2',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '5f2f0a26c02a48c499872afe1988fb9e',
                    eigenValueName: '固有频率',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'Hz',
                    judgment: false,
                    vdi: false
                  },
                  {
                    number: '3',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '16d7f8bcdd224f869cf582acb64de713',
                    eigenValueName: 'RMS',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'm/s',
                    judgment: false,
                    vdi: false
                  }
                ],
                [
                  {
                    number: '9',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '7653ea5766ac4e3aafcf9920bee7e282',
                    eigenValueName: '张拉力',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'kN',
                    judgment: false,
                    vdi: false
                  },
                  {
                    number: '2',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '5f2f0a26c02a48c499872afe1988fb9e',
                    eigenValueName: '固有频率',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'Hz',
                    judgment: false,
                    vdi: false
                  },
                  {
                    number: '3',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '16d7f8bcdd224f869cf582acb64de713',
                    eigenValueName: 'RMS',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'm/s',
                    judgment: false,
                    vdi: false
                  }
                ],
                [
                  {
                    number: '10',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '7653ea5766ac4e3aafcf9920bee7e282',
                    eigenValueName: '张拉力',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'kN',
                    judgment: false,
                    vdi: false
                  },
                  {
                    number: '2',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '5f2f0a26c02a48c499872afe1988fb9e',
                    eigenValueName: '固有频率',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'Hz',
                    judgment: false,
                    vdi: false
                  },
                  {
                    number: '3',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '16d7f8bcdd224f869cf582acb64de713',
                    eigenValueName: 'RMS',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'm/s',
                    judgment: false,
                    vdi: false
                  }
                ],
                [
                  {
                    number: '11',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '7653ea5766ac4e3aafcf9920bee7e282',
                    eigenValueName: '张拉力',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'kN',
                    judgment: false,
                    vdi: false
                  },
                  {
                    number: '2',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '5f2f0a26c02a48c499872afe1988fb9e',
                    eigenValueName: '固有频率',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'Hz',
                    judgment: false,
                    vdi: false
                  },
                  {
                    number: '3',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '16d7f8bcdd224f869cf582acb64de713',
                    eigenValueName: 'RMS',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'm/s',
                    judgment: false,
                    vdi: false
                  }
                ],
                [
                  {
                    number: '12',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '7653ea5766ac4e3aafcf9920bee7e282',
                    eigenValueName: '张拉力',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'kN',
                    judgment: false,
                    vdi: false
                  },
                  {
                    number: '2',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '5f2f0a26c02a48c499872afe1988fb9e',
                    eigenValueName: '固有频率',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'Hz',
                    judgment: false,
                    vdi: false
                  },
                  {
                    number: '3',
                    eigenValue: '0.364',
                    eigenValueCode: '',
                    eigenValueId: '16d7f8bcdd224f869cf582acb64de713',
                    eigenValueName: 'RMS',
                    eigenValueStatus: 'normal',
                    eigenValueUnit: 'm/s',
                    judgment: false,
                    vdi: false
                  }
                ]
              ]
            }
          ]
        }
      ]
    },
    msg: '操作成功'
  }
}
