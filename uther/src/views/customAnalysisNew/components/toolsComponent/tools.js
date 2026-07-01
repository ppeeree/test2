/**
 * father：针对左右两个功能模块的传参
 */

// import { DataArrayTexture } from "three/src/Three.js"

// slet child = document.querySelector('#child')
export const mouseEvent = (evt, splitDomId, splitType, limit, fatherId) => {
  let child = document.getElementById(splitDomId)
  let father = fatherId ? document.getElementById(fatherId) : null
  let changeDom1 = child.previousElementSibling
  let changeDom2 = child.nextElementSibling
  let allRange = {}
  let siblingsWidth = changeDom2.offsetWidth + changeDom1.offsetWidth
  let siblingsHeight = changeDom2.offsetHeight + changeDom1.offsetHeight
  let compRact = child.getClientRects()[0]
  if (father) {
    let react1 = father.getClientRects()[0]
    allRange = {
      ...limit,
      left: react1.left + limit.left,
      top: react1.top + limit.top,
      bottom: react1.bottom - limit.bottom,
      right: react1.right - limit.right,
      offsetX: react1.left + (evt.x - compRact.left),
      offsetY: react1.top + (evt.y - compRact.top)
    }
  } else {
    let react1 = changeDom1.getClientRects()[0]
    let react2 = changeDom2.getClientRects()[0]
    let parentNode = changeDom2.parentNode.getClientRects()[0]
    allRange = {
      left: react1.left + limit.left,
      top: react1.top + limit.top,
      bottom: react2.bottom - limit.bottom,
      right: (react2.left + changeDom2.offsetWidth) - limit.right,
      offsetX: parentNode.left + (evt.x - compRact.left),
      offsetY: parentNode.top + (evt.y - compRact.top)
    }
  }
  document.onmousemove = function (ev) {
    let pt = calcPositon(ev, allRange)
    if (splitType == 'vertical') {
      child.style.left = pt.left + 'px'
    } else {
      child.style.top = pt.top + 'px'
    }
  }
  document.onmouseup = function () {
    document.onmousemove = null
    document.onmouseup = null
    if (splitType == 'vertical') {
      let dom1NewWidth = father ? child.style.left : (child.style.left.replace(/\px/g, '') - changeDom1.offsetLeft + 'px')
      changeDom1.style.width = dom1NewWidth
      changeDom2.style.width = father ? father.clientWidth - child.offsetLeft - child.clientWidth + 'px' : siblingsWidth - dom1NewWidth.replace(/\px/g, '') + 'px'
      changeDom2.style.left = `${child.offsetLeft + child.clientWidth}px`
      if (father) {
        colResize(fatherId)
      }
    } else {
      let dom1NewHeight = father ? child.style.top : (child.style.top.replace(/\px/g, '') - changeDom1.offsetTop + 'px')
      changeDom1.style.height = dom1NewHeight
      changeDom2.style.height = father ? father.clientHeight - child.offsetTop - child.clientHeight + 'px' : siblingsHeight - dom1NewHeight.replace(/\px/g, '') + 'px'
      changeDom2.style.top = `${child.offsetTop + child.clientHeight}px`
    }
  }
}
const calcPositon = (pt, bounds) => {
  const left =
    (pt.x > bounds.left && pt.x < bounds.right
      ? pt.x
      : pt.x >= bounds.right
        ? bounds.right
        : bounds.left) - bounds.offsetX
  const top =
    (pt.y > bounds.top && pt.y < bounds.bottom
      ? pt.y
      : pt.y >= bounds.bottom
        ? bounds.bottom
        : bounds.top) - bounds.offsetY
  return { left, top }
}



// 自定义Id，不重复
export const getId = () => {
  return (Math.random() + new Date().getTime()).toString(32).slice(0, 8)
}
// 初始化行高度，定位或者新增行，删减行可用
export const rowResize = (fatherId, isReposition) => {
  let father = document.getElementById(fatherId)
  let heightP = father.clientHeight
  // let widthP = father.clientWidth
  let items = father.querySelectorAll('.rowbox')
  let splitItems = father.querySelectorAll('.x-splitter-horizontal')
  let splitHeight = 4
  if (isReposition) {
    let topDistance = 0
    let splitTop = 0
    for (let i = 0; i < items.length; i++) {
      i !== 0 ? topDistance += items[i - 1].clientHeight : null
      splitTop += items[i].clientHeight
      items[i].style.top = topDistance + splitHeight * i + 'px'
      splitItems[i] ? splitItems[i].style.top = splitTop + splitHeight * i + 'px' : null
    }
  } else {
    let itemHeight =
      (heightP - (items.length - 1) * splitHeight) / items.length
    for (let i = 0; i < items.length; i++) {
      items[i].style.height = itemHeight + 'px'
      items[i].style.top = (itemHeight + splitHeight) * i + 'px'
    }
    for (let j = 0; j < splitItems.length; j++) {
      splitItems[j].style.top = itemHeight * (j + 1) + splitHeight * j + 'px'
      splitItems[j].style.height = splitHeight + 'px'
      splitItems[j].style.width = '100%'
      splitItems[j].style.left = 0 + 'px'
    }
  }
}

// 初始化行内的列宽度，定位 or 响应新增列，删减列可用
export const colResize = (fatherId, isReposition) => {
  let father = document.getElementById(fatherId)
  // 传入id为多个row的父id时children
  let rowDoms = father.querySelectorAll('.rowbox')
  if (rowDoms.length) {
    for (let m = 0; m < rowDoms.length; m++) {
      colResizeComputed(rowDoms[m], isReposition)
    }
  } else {
    colResizeComputed(father, isReposition)
  }
}

// 行内元素重新计算
export const colResizeComputed = (father, isReposition) => {
  // isReposition:已经定义了宽度和高度，只重新计算定位
  let widthP = father.clientWidth
  // let heightP = father.clientHeight
  let children = father.children
  let items = []
  let splitItems = []
  for (let i = 0; i < children.length; i++) {
    if (children[i].classList.contains('colbox')) {
      items.push(children[i])
    } else if (children[i].classList.contains('splitter')) {
      splitItems.push(children[i])
    }
  }
  let splitWidth = 4
  if (isReposition) {
    let leftDistance = 0
    let splitLeft = 0
    for (let m = 0; m < items.length; m++) {
      m !== 0 ? leftDistance += items[m - 1].clientWidth : null
      splitLeft += items[m].clientWidth
      items[m].style.left = leftDistance + splitWidth * m + 'px'
      splitItems[m] ? splitItems[m].style.left = splitLeft + splitWidth * m + 'px' : null
    }
  } else {
    let itemWidth = (widthP - (items.length - 1) * splitWidth) / items.length
    for (let i = 0; i < items.length; i++) {
      items[i].style.width = itemWidth + 'px'
      items[i].style.left = (itemWidth + splitWidth) * i + 'px'
    }
    for (let j = 0; j < splitItems.length; j++) {
      splitItems[j].style.left = itemWidth * (j + 1) + splitWidth * j + 'px'
      splitItems[j].style.width = splitWidth + 'px'
      splitItems[j].style.height = '100%'
      splitItems[j].style.top = 0 + 'px'
    }
  }

}

export const chartENZHList = {
  empty: '空',
  RCA: '转速相关性分析图',
  PCA: '功率相关性分析图',
  WCA: '风速相关性分析图',
  DA: '特征值分布',
  OA: '倾角分布图',
  OrderEnvelopeSpectrum: '阶次包络谱波形',
  OrderEnvelope: '阶次包络波形',
  OrderSpectrum: '阶次谱波形',
  Order: '阶次波形',
  Envelope: '包络波形',
  EnvelopeSpectrum: '包络谱波形',
  Spd: '转速波形',
  Waterfall: '瀑布图',
  Cepstrum: '倒谱图',
  FreqDomain: '频域波形',
  TimeDomain: '时域波形',
  BearingAnalysis: '轴承诊断波形',
  Trend: '趋势图'
}

/* 包络波形：Envelope Waveform
包络谱：Envelope Spectrum
阶次波形：Order Waveform
阶次谱：Order Spectrum
瀑布图： Waterfall
频域：Spectrum
时域：Waveform */

// Y轴单位转换重新计算
/**
 * 
 * @param {*} unit 转换后的单位[xunit,yunit]
 * @param {*} dataArr seriesData数组[[x,y],[x,y]]
 */
export const unitChangeRecomputedData = (unit, dataArr) => {
  let result = []
  // Math.LOG10E * Math.log(X)
  if (unit[0] == 'CPM' || unit[1] == 'g' || unit[0] == '对数') {
    result = dataArr.map(element => {
      let newX = unit[0] == 'CPM' ? element[0] * 60 : unit[0] == '对数' ? Math.LOG10E * Math.log(element[0]) : element[0]
      let newY = unit[1] == 'g' ? (element[1] / 9.8).toFixed(7) : element[1]
      element.splice(0, 2, newX, newY)
      return element
    })
    return result
  } else {
    return dataArr
  }
}

export const tabName = {
  CVM: '传动链',
  TVM: '塔筒',
  BVM: '叶片',
  TVM_STE: '塔筒结构',
  TVM_FLG_GAP: '塔筒法兰间隙',
  TVM_CBF: '拉索力',
  CTVM: '传动链-塔筒',
  TVM_STE_FDN: '塔基',
  TVM_STE_TOP: '塔顶',
  传动链: 'CVM',
  塔筒: 'TVM',
  叶片: 'BVM',
  '塔筒结构': 'TVM_STE',
  '塔筒法兰间隙': 'TVM_FLG_GAP',
  '拉索力': 'TVM_CBF',
  '传动链-塔筒': 'CTVM'
}
/* export const typeList =
  [
    {
      key: 'BearingAnalysis',
      value: '轴承诊断'
    },
    {
      key: 'TimeDomain',
      value: '时域'
    },
    {
      key: 'FreqDomain',
      value: '频域'
    },
    {
      key: 'EnvelopeA',
      value: '包络',
      children: [
        {
          key: 'Envelope',
          value: '包络波形'
        },
        {
          key: 'EnvelopeSpectrum',
          value: '包络谱'
        }
      ]
    },
    {
      key: 'OrderType',
      value: '阶次',
      children: [
        {
          key: 'Order',
          value: '阶次波形'
        },
        {
          key: 'OrderSpectrum',
          value: '阶次谱'
        },
        {
          key: 'OrderEnvelope',
          value: '阶次包络'
        },
        {
          key: 'OrderEnvelopeSpectrum',
          value: '阶次包络谱'
        }
      ]
    },
    {
      key: 'Spd',
      value: '转速'
    },
    {
      key: 'Cepstrum',
      value: '倒谱图'
    },
     {
       key: 'Waterfall',
       value: '瀑布图'
     },
{
  key: 'Tezheng',
    value: '特征',
      children: [
        {
          key: 'DA',
          value: '分布分析'
        },
        {
          key: 'Correlation',
          value: '相关性分析',
          children: [
            {
              key: 'RCA',
              value: '转速'
            },
            {
              key: 'PCA',
              value: '功率'
            },
            {
              key: 'WCA',
              value: '风速'
            }
          ]
        }
      ]
}
  ] */
export const typeList = [
  {
    value: 'opeNewWindow',
    eventName: 'openPage',
    label: '打开新窗口'
  },
  {
    value: 'addChart',
    label: '增加图谱',
    children: [
      {
        value: 'left',
        eventName: 'addChart',
        label: '左边'
      },
      {
        value: 'right',
        eventName: 'addChart',
        label: '右边'
      }
    ]
  },
  {
    value: 'changeChart',
    label: '更改图谱类型',
    children: [
      {
        value: 'EnvelopeA',
        label: '包络',
        children: [
          {
            value: 'Envelope',
            eventName: 'changeChartType',
            label: '包络波形'
          },
          {
            value: 'EnvelopeSpectrum',
            eventName: 'changeChartType',
            label: '包络谱'
          }
        ]
      },
      {
        value: 'OrderType',
        label: '阶次',
        children: [
          {
            value: 'Order',
            eventName: 'changeChartType',
            label: '阶次波形'
          },
          {
            value: 'OrderSpectrum',
            eventName: 'changeChartType',
            label: '阶次谱'
          },
          {
            value: 'OrderEnvelope',
            eventName: 'changeChartType',
            label: '阶次包络'
          },
          {
            value: 'OrderEnvelopeSpectrum',
            eventName: 'changeChartType',
            label: '阶次包络谱'
          }
        ]
      },
      {
        value: 'BearingAnalysis',
        eventName: 'changeChartType',
        label: '轴承诊断'
      },
      {
        value: 'TimeDomain',
        eventName: 'changeChartType',
        label: '时域'
      },
      {
        value: 'FreqDomain',
        eventName: 'changeChartType',
        label: '频域'
      },
      {
        value: 'Spd',
        eventName: 'changeChartType',
        label: '转速'
      },
      {
        value: 'Cepstrum',
        eventName: 'changeChartType',
        label: '倒谱图'
      },
      {
        value: 'Tezheng',
        label: '特征',
        children: [
          {
            value: 'DA',
            eventName: 'changeChartType',
            label: '分布分析'
          },
          {
            value: 'Correlation',
            label: '相关性分析',
            children: [
              {
                value: 'RCA',
                eventName: 'changeChartType',
                label: '转速'
              },
              {
                value: 'PCA',
                eventName: 'changeChartType',
                label: '功率'
              },
              {
                value: 'WCA',
                eventName: 'changeChartType',
                label: '风速'
              }
            ]
          }
        ]
      }
    ]
  },
  {
    value: 'removeChart',
    eventName: 'removeChart',
    label: '移除图谱'
  },
  {
    value: 'deleteRow',
    eventName: 'removeRow',
    label: '移除整行'
  },
  {
    value: 'downloadData',
    eventName: 'downloadData',
    label: '数据下载'
  }
]
export const defaultEvName = {
  主轴轴承1: '0.1-10有效值',
  主轴轴承2: '0.1-10有效值',
  齿轮箱输入轴: '0.1-10有效值',
  齿轮箱一级行星轮: '10-2000有效值',
  齿轮箱二级行星轮: '10-2000有效值',
  齿轮箱三级行星轮: '10-2000有效值',
  齿轮箱低速轴: '10-2000有效值',
  齿轮箱中间轴: '10-2000有效值',
  齿轮箱高速轴: '10-2000有效值',
  齿轮箱输出轴: '10-2000有效值',
  发电机驱动端: '10-5000有效值',
  发电机非驱动端: '10-5000有效值'
}

export const getAnalysisLayout = (analysisType) => {
  switch (analysisType) {
    case 'TVM_STE':
      return {
        key: 'rowbox',
        rowHeight: 49.5,
        charts: [
          {
            key: 'colbox',
            chartType: 'TimeDomain',
            chartWidth: 100
          }
        ]
      }
    default:
      return {
        key: 'rowbox',
        rowHeight: 49.5,
        charts: [
          {
            key: 'colbox',
            chartType: 'TimeDomain',
            chartWidth: 49.65
          },
          {
            key: 'colbox',
            chartType: 'FreqDomain',
            chartWidth: 49.65
          }
        ]
      }
  }

}

export const deepFreeze = (obj) => {
  Object.freeze(obj)
  Object.getOwnPropertyNames(obj).forEach(name => {
    const val = obj[name]
    if (val && typeof val === 'object') deepFreeze(val)
  })
  return obj
}