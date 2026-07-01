import { createEnum } from './exp'

/**
 * 事件类型枚举
 */
export const GeneratorEnum = createEnum({
  inspection: '巡检',
  health: '健康',
  work: '工作'
})

/**
 * 事件状态枚举
 */
export const eventStatusEnum = createEnum({
  0: '未处理',
  1: '已处理',
  notdone: '未处理',
  done: '已处理'
})

/**
 * 事件等级枚举
 * @type {{
 * 1: '一级',
 * 2: '二级',
 * 3: '三级',
 * 4: '四级'
 * }}
 */
export const eventLevelEnum = createEnum({
  1: '一级',
  2: '二级',
  3: '三级',
  4: '四级'
})

/**
 * 事件等级颜色枚举
 * @type {{
 * Normal: '#2ED133',
 * careful: '#FFE604',
 * DANGER: '#FF0F0D',
 * Warning: '#FF6B0E'
 * }}
 */
export const levelColorEnum = createEnum({
  normal: '#2ED133',
  // careful: '#FFE604',
  attention: '#FFE604',
  danger: '#FF0F0D',
  warning: '#FF6B0E',
  nostate: '#8D8D8D',
  unknown: '#8D8D8D',
  null: 'transparent'
})

/**
 * 事件枚举
 * @type {{
 * dangerousNumber: '#危险',
 * noteNumber: '#注意',
 * warnNumber: '#警告',
 * normalNumber: '#正常'
 * }}
 */
export const stateTypeEnum = createEnum({
  dangerousNumber: '危险',
  noteNumber: '注意',
  warnNumber: '警告',
  normalNumber: '正常',
  nostateNumber: '无状态'
})

/**
 * 事件枚举
 * @type {{
 * dangerousNumber: '#危险',
 * noteNumber: '#注意',
 * warnNumber: '#警告',
 * normalNumber: '#正常',
 * nostate: '#无状态'
 * }}
 */

export const levelColor = createEnum({
  normal: '#2ED133',
  attention: '#FFE604',
  danger: '#FF0F0D',
  warning: '#FF6B0E',
  nostate: '#8D8D8D',
  unknown: '#8D8D8D',
  null: '#8D8D8D'
})
export const classLevelEnum = createEnum({
  severe: '严重',
  moderate: '中等',
  mild: '轻微',
  noFault: '无故障'
})

export const falutLevelColor = createEnum({
  severe: '#FF0000',
  moderate: '#FF5D5D',
  mild: '#FF9B9B',
  noFault: '#FCCECE'
})

export const eventType = createEnum({
  巡检事件: '巡检',
  健康事件: '健康'
})

/**
 * 设备列表实体类型枚举
 * @type {{
 * WINDTURBINE: '风力发电机组',
 * JC: '机巢',
 * LDSYZ: '陆地升压站',
 * HSSYZ: '海上升压站',
 * WRJ: '无人机'
 * }}
 */
export const entityTypeEnum = createEnum({
  windturArr: '风力发电机组',
  JCArr: '机巢',
  LDSYZArr: '陆地升压站',
  HSSYZArr: '海上升压站',
  WRJArr: '无人机'
})

/**
 * 设备列表实体类型枚举
 * @type {{
 * WINDTURBINE: 'windturArr',
 * JC: 'JCArr',
 * LDSYZ: 'LDSYZArr',
 * HSSYZ: 'HSSYZArr',
 * WRJ: 'WRJArr'
 * }}
 */
export const entityTypeDataEnum = createEnum({
  WINDTURBINE: 'windturArr',
  JC: 'JCArr',
  LDSYZ: 'LDSYZArr',
  HSSYZ: 'HSSYZArr',
  WRJ: 'WRJArr'
})

export const compSimpleCode = createEnum({
  turbine: 'WINDTURBINE',
  engine: 'NAC',
  tower: 'TOW',
  blade: 'ROT'
})

export const compNameEnum = createEnum({
  turbine: '机组',
  engine: '机舱',
  tower: '塔筒',
  blade: '风轮'
})

export const compIconImgList = createEnum({
  机舱: 'engineroomIcon',
  风轮: 'bladeIcon',
  叶片: 'bladeOneIcon',
  塔筒: 'towerBaseIcon',
  齿轮箱: 'gearboxIcon',
  发电机: 'engineIcon',
  主轴承: 'mainbearIcon',
  主轴: 'mainbearIcon',
  一层法兰: 'flangeIcon',
  二层法兰: 'flangeIcon',
  三层法兰: 'flangeIcon',
  四层法兰: 'flangeIcon',
  叶片一: 'bladeOneIcon',
  叶片二: 'bladeOneIcon',
  叶片三: 'bladeOneIcon',
  塔顶: 'towerTopIcon',
  塔基: 'towerBaseIcon',
  钢索: 'steelLineIcon'
})
export const typeImg = createEnum({
  GBX: 'gearbox',
  GEN: 'alternator',
  MSS: 'mainbearing',
  MS: 'mainbearing',
  TOW: 'tower',
  BLA: 'blade',
  BOL: 'bolt',
  FLG: 'flange',
  TFN: 'towerbase',
  TTP: 'towerTop',
  ROT: 'windwheel',
  NAC: 'cabinwheel',
  BJZ: 'bearing',
  SSD: 'steelLineIcon'
})

/**
 * 标签图片样式
 * @type {{
 * 叶片: 0,
 * 主轴承: 1,
 * 齿轮箱: 2,
 * 发电机: 3,
 * 塔筒: 4
 * }}
 */
export const iconStyleEnum = createEnum({
  叶片: 'width: 22px;height: 26.38px;',
  主轴承: 'width: 25.16px;height: 15.46px;',
  齿轮箱: 'width: 22.54px;height: 14.7px;',
  发电机: 'width: 23.14px;height: 16.96px;',
  塔筒: 'width: 23.99px;height: 21.79px;'
})

/**
 * 部件异常序号
 * @type {{
 * 叶片: 0,
 * 主轴承: 1,
 * 齿轮箱: 2,
 * 发电机: 3,
 * 塔筒: 4
 * }}
 */
export const faultTrendEnum = createEnum({
  blaFaultCount: 0,
  msFaultCount: 1,
  gbxFaultCount: 2,
  genFaultCount: 3,
  towFaultCount: 4
})

/**
 * 事件类型枚举
 * @type {{
 * danger: '危险',
 * attention: '注意',
 * warning: '警告',
 * normal: '正常'
 * }}
 */
export const eventTypeEnum = createEnum({
  danger: '危险',
  attention: '注意',
  warning: '警告',
  normal: '正常',
  nostate: '无状态',
  unknown: '无状态'
})

/**
 * 日期类型枚举1
 * @type {{
 *  sunday: 1,
 *  monday: 2,
 *  tuesday: 3,
 *  wednesday: 4,
 *  thursday: 5,
 *  friday: 6,
 *  saturday: 7
 * }}
 */
export const localDataNumEnum = createEnum({
  sunday: 1,
  monday: 2,
  tuesday: 3,
  wednesday: 4,
  thursday: 5,
  friday: 6,
  saturday: 7
})

/**
 * 日期类型枚举2
 * @type {{
 *  sunday: 星期日,
 *  monday: 星期一,
 *  tuesday: 星期二,
 *  wednesday: 星期三,
 *  thursday: 星期四,
 *  friday: 星期五,
 *  saturday: 星期六
 * }}
 */
export const localDataUniEnum = createEnum({
  sunday: '星期日',
  monday: '星期一',
  tuesday: '星期二',
  wednesday: '星期三',
  thursday: '星期四',
  friday: '星期五',
  saturday: '星期六'
})

/**
 * 事件序列枚举
 * @type {{
 * danger: '3',
 * attention: '2',
 * warning: '1',
 * normal: '0',
 * nostate: '4'
 * }}
 */
export const eventTypeIndexEnum = createEnum({
  danger: 3,
  attention: 2,
  warning: 1,
  normal: 0,
  nostate: 4
})

/**
 * 事件标签图片样式
 * @type {{
 * 1: '巡检事件',
 * 2: '安全事件',
 * 3: '健康事件',
 * 4: '工作事件'
 * }}
 */
export const GeneratorStelyEnum = createEnum({
  1: 'width: 18.88px;height: 18.31px;',
  2: 'width: 18.88px;height: 18.31px;',
  3: 'width: 20.1px;height: 16.77px;',
  4: 'width: 17.98px;height: 15.87px;'
})

/**
 * 事件数
 * @type {{
 * 1: '巡检事件',
 * 2: '安全事件',
 * 3: '健康事件',
 * 4: '工作事件'
 * }}
 */
export const GeneratorIndexEnum = createEnum({
  inspection: 1,
  health: 3,
  work: 4
})

/**
 * 健康事件图标序号样式
 * @type {{
 * 叶片: 0,
 * 主轴承: 1,
 * 齿轮箱: 2,
 * 发电机: 3,
 * 塔筒: 4
 * }}
 */
export const healthEventStelyEnum = createEnum({
  0: 'width: 19.29px;height: 12.43px;',
  2: 'width: 15.15px;height: 9.88px;',
  3: 'width: 15.86px;height: 11.62px;'
})

export const eventCode = createEnum({
  health: '健康事件',
  inspection: '巡检事件',
  work: '工作事件',
  first: '一级',
  second: '二级',
  third: '三级',
  fourth: '四级',
  done: '已处理',
  doing: '处理中',
  notdone: '未处理'
})

/**
 * 设备部件实体类型枚举
 * @type {{
 *  GBX: '齿轮箱',
 *  GEN: '发电机',
 *  MS: '主轴',
 *  BLA: '叶片',
 *  FLG: '法兰',
 *  TOW: '塔筒'
 * }}
 */
export const entityPartEnum = createEnum({
  GBX: '齿轮箱',
  GEN: '发电机',
  MSS: '主轴承',
  BLA: '叶片',
  TOWFL: '法兰',
  TOW: '塔筒',
  NAC: '机舱',
  ROT: '风轮',
  TOWFDN: '塔基',
  TOWPL: '钢索',
  STL: '钢索',
  SSD: '钢索',
  TOWTOP: '塔顶',
  BJZ: '变桨轴承',
  OFE: '变桨轴承',
  PBG: '变桨轴承',
  MS: '主轴',
  MST: '主轴',
  YBG: '偏航轴承',
  BL1: '叶片1',
  BL2: '叶片2',
  BL3: '叶片3',
  PB1: '变桨轴承1',
  PB2: '变桨轴承2',
  PB3: '变桨轴承3'
})

/**
 * 事件等级枚举
 * @type {{
 * first: 1,
 * second: 2,
 * third: 3,
 * fourth: 4
 * }}
 */
export const eventIndexEnum = createEnum({
  first: 1,
  second: 2,
  third: 3,
  fourth: 4
})

/**
 * 处理枚举
 * @type {{
 * notdone: 0,
 * done: 1
 * }}
 */
export const progressEnum = createEnum({
  done: 1,
  notdone: 0
})

/**
 * 处理图片枚举
 * @type {{
 * notdone: 0,
 * done: 1
 * }}
 */
export const progressStyleEnum = createEnum({
  done: 'width: 14.08px;height: 11.55px;',
  notdone: 'width: 15px;height: 15px;'
})

/**
 * 处理等级图片eventLevelColor样式枚举
 * @type {{
 * 1: 'width: 8.32px;height: 15.37px;',
 * 2: 'width: 14.84pxpx;height: 15.37px;',
 * 3: 'width: 19.55px;height: 15.37px;',
 * 4: 'width: 19.94px;height: 15.37px;'
 * }}
 */
export const eventLevelColorStyleEnum = createEnum({
  1: 'width: 8.32px;height: 15.37px;',
  2: 'width: 14.84pxpx;height: 15.37px;',
  3: 'width: 19.55px;height: 15.37px;',
  4: 'width: 19.94px;height: 15.37px;'
})

/**
 * 处理图片枚举
 * @type {{
 * notdone: 0,
 * done: 1
 * }}
 */
export const setValueBackgroundColor = createEnum({
  danger: 'rgba(250, 81, 81, 0.5)',
  warning: 'rgba(255, 230, 4, 0.5)',
  attention: 'rgba(255, 230, 4, 0.5)',
  normal: 'rgba(77, 88, 105, 0.5)',
  null: 'rgba(77, 88, 105, 0.5)',
  '': 'rgba(77, 88, 105, 0.5)'
})

/**
 * 处理图片枚举
 * @type {{
 * notdone: 0,
 * done: 1
 * }}
 */
export const setValueBorderColor = createEnum({
  danger: 'rgba(250, 81, 81, 1)',
  warning: 'rgba(255, 230, 4, 1)',
  attention: 'rgba(255, 230, 4, 1)',
  normal: 'rgba(77, 88, 105, 1)',
  null: 'rgba(77, 88, 105, 1)',
  '': 'rgba(77, 88, 105, 1)'
})

/**
 * 处理图片枚举
 * @type {{
 * notdone: 0,
 * done: 1
 * }}
 */
export const setValueTitleBorder = createEnum({
  danger: '2px solid rgba(250, 81, 81, 1)',
  warning: '2px solid rgba(255, 230, 4, 1)',
  attention: '2px solid rgba(255, 230, 4, 1)',
  normal: '2px solid rgba(77, 88, 105, 1)',
  null: '2px solid rgba(77, 88, 105, 1)',
  '': '2px solid rgba(77, 88, 105, 1)',
  unknown: '2px solid rgba(77, 88, 105, 1)'
})

export const BIG_PARTS = {
  ROT: ['轮毂', '叶片一', '叶片二', '叶片三', '变桨轴承一', '变桨轴承二', '变桨轴承三', '叶片前', '叶片后'],
  NAC: ['机舱'],
  TWW: ['塔顶', '塔筒左', '塔筒右', '塔基', '钢塔', '混塔右', '混塔左', '偏航轴承'],
  YPB: ['变桨轴承一', '变桨轴承二', '变桨轴承三', '偏航轴承']
}