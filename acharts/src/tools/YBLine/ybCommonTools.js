import {
  accSub,
  accAdd,
} from '../tool.js'
// 寻找距离相近点
/*
arr:数据源数组
num：匹配的相近点的参考值
返回值为{index：数组中的index，value:匹配点的值，数组[x,y]}
*/
export function closest(arr, num) {
  let left = 0
  let right = arr.length - 1
  while (left <= right) {
    const middle = Math.floor((right + left) / 2)
    if (right - left <= 1) {
      break
    }
    const val = arr[middle][0]
    if (val === num) {
      return { index: middle, value: arr[middle] }
    } else if (val > num) {
      right = middle
    } else {
      left = middle
    }
  }
  const leftValue = arr[left][0]
  const rightValue = arr[right][0]
  return rightValue - num > num - leftValue
    ? { index: left, value: arr[left] }
    : { index: right, value: arr[right] }
}

// 游标线是否存在，及游标相关数据的过滤返回方法
/* export function judgeYbExist(options, name) {
  let obj = {
    isExist: false,
    lineArr: []
  }
  if (options.series[1].markLine) {
    if (options.series[1].markLine.data && options.series[1].markLine.data.length) {
      let arr = []
      if (Array.isArray(name)) {
        arr = options.series[1].markLine.data.filter(i => name.includes(i.name))
      } else {
        arr = options.series[1].markLine.data.filter(i => i.name === name)
      }
      if (arr.length) {
        obj.isExist = true
        obj.lineArr = arr
      }
    }
  }
  return obj
} */
// 游标线是否存在，及游标相关数据的过滤返回方法
/**
 * 
 * @param {*} options 图谱options
 * @param {*} name markline name
 * @param {*} seriesId 
 * @returns 
 */
export function judgeYbExist(options, name, seriesId) {
  let obj = {
    isExist: false,
    lineArr: []
  }
  let arr = []
  let marklineDataList = []
  if (seriesId) {
    marklineDataList = options.series.find(i => i.id == seriesId).markLine.data
  } else {
    marklineDataList = options.series.map(item => {
      return item.markLine ? item.markLine.data : []
    }).flat()
  }
  if (Array.isArray(name)) {
    arr = marklineDataList.filter(i => name.includes(i.name))
  } else {
    arr = marklineDataList.filter(i => i.name === name)
  }
  if (arr.length) {
    obj.isExist = true
    obj.lineArr = arr
  }
  return obj
}

// 放大监听，graphic位置重新计算
//garphic位置由坐标点转变为距离
/**
 * 位置由坐标点转变为距离
 * @param {*} posArr 游标对应的数据点数组
 * @param {*} options 图谱option
 * @param {*} chartInst 图谱实例
 * @returns 计算后的graphic位置数组
 */
export function initAllGraphicPosition(paramArrObj, options, chartInst) {
  let { graphic } = options
  for (let key in paramArrObj) {
    paramArrObj[key].forEach((element, index) => {
      let position = chartInst.convertToPixel({ gridIndex: element.gridIndex }, element.value)
      graphic[0].elements.find(i => i.id == key + '_' + index).left = position[0] - 2
    })
  }
  return graphic
}
/**
 * 鼠标划上显示游标线信息
 * @param {*} param0 鼠标划上x,y坐标位置信息
 * @param {*} lineArr 此组游标线对应的series上面的data值的数组集合
 * @param {*} param2 xunit:x轴name单位,yunit:y轴name单位
 * @param {*} lineIndex 鼠标事件的游标线index
 * @param {*} YBDOM 游标信息弹框DOM
 * @param {*} chart 图谱实例
 * @param {*} prefixName 当前组游标前缀
 * @param {*} lineColor 游标线体颜色
 */
export const onBDLineMouseover = ([x, y], lineArr, { xunit, yunit }, lineIndex, YBDOM, chart, prefixName, lineColor) => {
  /*  let options = chart.getOption()
   let { lineArr } = judgeYbExist(options, prefixName, seriesId)
   let { xAxis, yAxis } = options
   let xunit = xAxis[0].name
   let yunit = yAxis[0].name */
  let htmlStr = ''
  let distance, ff //频率 计算方式：1/distance 取三位有效数字,只针对时域波形，频谱类不显示
  if (prefixName.includes('shuang') || prefixName.includes('jiange')) {
    distance = Math.abs(accSub(Number(lineArr[1][0]), Number(lineArr[0][0])))
    ff = (1 / distance).toPrecision(3)
    htmlStr = `<p>△X: <span>${distance}</span>&nbsp;${xunit == 'Hz' ? '' : `f：<span>${ff}</span>`
      } </p>
    <p>x：<span>${lineArr[lineIndex][0]}</span>&nbsp;${xunit} &nbsp;&nbsp;y：<span>${lineArr[lineIndex][1]}</span>&nbsp;${yunit}</p>`
  } else if (prefixName.includes('single')) {
    htmlStr = `<p>x：<span>${lineArr[lineIndex][0]}</span>&nbsp;${xunit} &nbsp;&nbsp;y：<span>${lineArr[lineIndex][1]}&nbsp;${yunit}</p>`
  } else if (prefixName.includes('bian')) {
    distance = Math.abs(accSub(Number(lineArr[1][0]), Number(lineArr[0][0])))
    ff = (1 / distance).toPrecision(3)
    htmlStr = `<p>中心频率：<span>${lineArr[Math.floor(lineArr.length / 2)][0]}</span></p><p>△X: <span>${distance}</span>&nbsp; ${xunit == 'Hz' ? '' : `f：<span>${ff}</span>`
      }</p><p>x：<span>${lineArr[lineIndex][0]}</span>&nbsp;${xunit} &nbsp;&nbsp;y：<span>${lineArr[lineIndex][1]}</span>&nbsp;${yunit}</p>`
  } else if (prefixName.includes('bei')) {
    htmlStr = `<p>基频：<span>${lineArr[0][0]}</span> &nbsp;&nbsp;<span>${lineIndex + 1}</span>倍频</p><p>x：<span>${lineArr[lineIndex][0]}</span>&nbsp;${xunit} &nbsp;&nbsp;y：<span>${lineArr[lineIndex][1]}</span>&nbsp;${yunit}</p>`
  }
  YBDOM.innerHTML = htmlStr
  YBDOM.style.display = 'block'
  const { width, height } = YBDOM.getBoundingClientRect()
  const chartWidth = chart.getWidth()
  const chartHeight = chart.getHeight()
  let positionX = x + 10, positionY = y + 10
  if (width + x + 10 > chartWidth) {
    positionX = x - width - 10
  }
  if (height + y + 10 > chartHeight) {
    positionY = y - height - 10
  }
  YBDOM.style.left = positionX + 'px'
  YBDOM.style.top = positionY + 'px'
  YBDOM.style.color = lineColor
}

//garphic位置由坐标点转变为距离
/**
 * 
 * @param {*} paramArr 坐标点数组
 * @param {*} options 图谱的option
 * @param {*} chartInst 图谱实例
 * @param {*} data 进行游标分析的数据
 * @returns 计算后的garphic的数据
 */
export const initGraphicPosition = (gridIndex, paramArr, options, prefixName, echart) => {
  const { graphic } = options
  paramArr.forEach((element, index) => {
    let position = echart.convertToPixel({ gridIndex }, [element[0], 0])
    graphic[0].elements.find(i => i.id == prefixName + '_' + index).left = position[0] - 2
  })
  return graphic
}

/**
 * 
 * @param {*} seriesId 删除当前游标所在的seriesId
 * @param {*} prefixName 当前组游标的前缀名称
 * @param {*} echart echart实例
 */
export const closeYBLines = (seriesId, prefixName, echart) => {
  let options = echart.getOption()
  let { series, graphic } = options
  let { markLine, markPoint } = series.find(i => i.id == seriesId)
  let others = markLine.data.filter(i => i.name !== prefixName)
  let othersP = markPoint.data.filter(i => i.name !== prefixName)
  series.find(i => i.id == seriesId).markLine.data = others
  series.find(i => i.id == seriesId).markPoint.data = othersP
  let othersGraphic = []
  if (graphic && graphic.length) {
    othersGraphic = graphic[0].elements.filter(i => !i.id.includes(prefixName))
  }
  echart.setOption({
    ...options,
    graphic: othersGraphic,
    series,
  }, true)
  document.getElementById(echart.id + 'YBBlock').style.display = 'none'
}
/**
 * 获取附近点的值
 * @param {} arr 数据源
 * @param {*} num x值
 * @returns value及index
 */
export const getClosestNumber = (arr, num) => {
  if (num < arr[0][0] || num > arr[arr.length - 1][0]) {
    return {
      closestIndex: '',
      closeValue: [num, '']
    }
  }
  let closestIndex = arr.reduce((acc, current, index) => {
    const diff = Math.abs(current[0] - num);
    const accDiff = Math.abs(arr[acc][0] - num);
    return diff < accDiff ? index : acc;
  }, 0);
  return { closeIndex: closestIndex, closeValue: arr[closestIndex] };
}