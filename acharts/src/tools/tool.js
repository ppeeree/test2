
// import i18n from '../locales/index' // 国际化
// import i18n from '../../../../../locales/index' // 国际化
import * as XLSX from 'xlsx'
import * as echarts from 'echarts'
// import { whiteThemOption } from '../commonJs/optionConfig'
/**
 * 图谱转换成base64
 * @param {*} echart 图谱实例
 * @param {*} selectedLegend 选中的图例
 * @returns 
 */
export function imageBase64Data(echart, selectedLegend, isExport) {
  const nativeOption = echart.getOption()
  let echartHeight = echart.getHeight()
  let echartWidth = echart.getWidth()
  let { legend, title, markPoint, grid, xAxis, yAxis, series, graphic, polar } = nativeOption
  // 拼接导出图谱标题上的时间范围
  let subTitle = ''
  if (xAxis && xAxis.length && xAxis[0].type === 'time') {
    let arr = echart.getModel().getComponent('xAxis').axis.scale._extent
    subTitle = ` ${timestampToTime(arr[0])} - ${timestampToTime(arr[1])}`
  }
  let newSeries = []
  let newMarkPoint = []
  let newGraphic = []
  if (selectedLegend.length === 1) {
    series.forEach(i => {
      if (selectedLegend.includes(i.name)) {
        let markLineData = i.markLine && i.markLine.data.length ? i.markLine.data.map(j => {
          return {
            ...j,
            lineStyle: {
              ...j.lineStyle,
              color: "#000"
            },
            label: {
              ...j.label,
              color: "#000"
            },
          }
        }) : []
        let markPointData = i.markPoint && i.markPoint.data.length ? i.markPoint.data.map(j => {
          return {
            ...j,
            seriesColor: '#0781FF',
            itemStyle: {
              ...j.itemStyle,
              color: '#0781FF',
              borderColor: "#0781FF"
            }
          }
        }) : []
        let newData = i.data.map(j => {
          if (j.itemStyle) {
            return {
              ...j,
              itemStyle: {
                ...j.itemStyle,
                color: "#0781FF",
                borderColor: "#0781FF"
              }
            }
          } else {
            return j
          }

        })
        newSeries.push({
          ...i,
          data: newData,
          color: "#0781FF",
          markLine: {
            ...i.markLine,
            data: markLineData
          },
          markPoint: {
            ...i.markPoint,
            data: markPointData
          }
        }
        )
      } else if (i.name === 'markline_level') {
        newSeries.push(i)
      }
    })
    markPoint.data?.length && markPoint.data.forEach(i => {
      newMarkPoint.push({
        ...i,
        itemStyle: {
          ...i.itemStyle,
          color: "#0781FF",
          borderColor: "#0781FF"
        },
        seriesColor: "#0781FF"
      })

    })
  }
  if (graphic && graphic.length) {
    graphic[0].elements.forEach(i => {
      if (i.id.includes('marks')) {
        newGraphic.push({
          ...i,
          left: isExport ? i.left / echartWidth * 900 : i.left,
          top: isExport ? i.top / echartHeight * 360 : i.top,
        })
      } else {
        newGraphic.push(i)
      }
    })
  }
  let isVertical = selectedLegend[0].length * 12 > (echartWidth / 3)
  let content = ''
  // 导出数据流使用svg
  if (isExport) {
    // 导出svg，但是渲染使用canva，需要重新实例化一个svg图谱，导出后再进行销毁
    // 1. 创建一个肉眼不可见的容器
    const wrapper = document.createElement('div');
    wrapper.style.cssText = 'width:0;height:0;position:absolute;overflow:hidden;';
    document.body.appendChild(wrapper);

    // 2. 正常 init（svg 渲染器）
    let svgEchart = echarts.init(wrapper, null, {
      renderer: 'svg',
      width: 900,
      height: 360,
    })
    let newOptions = {
      ...nativeOption,
      title: title ? getNewArr(title, { text: subTitle, show: true }) : {},
      grid: grid ? getNewArr(grid, { height: 'auto', bottom: (selectedLegend.length > 1 && isVertical) ? (selectedLegend.length * 20 + 55) : (grid[0].bottom || 70) }) : {},
      legend: {
        ...legend,
        data: selectedLegend,
        left: 50,
        orient: isVertical ? 'vertical' : 'horizontal',
        formatter: function (name) {
          return name
        },
        icon: 'circle',
        itemWidth: 7,
        itemHeight: 7,
        bottom: 0,
        textStyle: {
          color: '#000',
          fontSize: 12,
        }
      },
      markPoint: {
        ...markPoint,
        data: newMarkPoint.length ? newMarkPoint : markPoint.data
      },
      graphic: newGraphic,
      series: newSeries.length ? newSeries : series,
    }
    svgEchart.setOption(newOptions)
    // 3. 拿矢量串
    // const svgStr = svgChart.getSVG();
    content = svgEchart.getDataURL({
      excludeComponents: ['dataZoom', 'toolbox'],//排除标题
      pixelRatio: 2,
    })
    // 4. 立即销毁 & 移除 DOM
    svgEchart.dispose();
    setTimeout(() => {
      document.body.removeChild(wrapper);
    }, 100)

  } else {
    // 导出图片流使用canvas
    echart.setOption({
      title: title ? getNewArr(title, { text: subTitle, show: true }) : {},
      grid: grid ? getNewArr(grid, { height: 'auto', bottom: (selectedLegend.length > 1 && isVertical) ? (selectedLegend.length * 20 + 55) : (grid[0].bottom || 70) }) : {},
      legend: {
        ...legend,
        data: selectedLegend,
        left: 50,
        orient: isVertical ? 'vertical' : 'horizontal',
        formatter: function (name) {
          return name
        },
      },
      series: newSeries.length ? newSeries : series,
      markPoint: {
        ...markPoint,
        data: newMarkPoint.length ? newMarkPoint : markPoint.data
      },
      graphic: newGraphic
    })
    echart.resize({
      width: echartWidth,
      height: echartHeight + (polar ? 0 : (isVertical ? (selectedLegend.length) * 20 : 0)), // 360 是添加诊断分析记录的默认固定尺寸
    })
    content = echart.getDataURL({
      excludeComponents: ['dataZoom', 'toolbox'],//排除标题
      pixelRatio: 2,
    })
    echart.setOption({
      grid,
      title,
      xAxis,
      yAxis,
      legend,
      series,
      markPoint,
      graphic
    })
    echart.resize({
      width: echartWidth,
      height: echartHeight
    })
  }
  // 直接处理URI编码的SVG
  if (content.startsWith('data:image/svg+xml;charset=UTF-8,')) {
    const svgContent = decodeURIComponent(content.replace('data:image/svg+xml;charset=UTF-8,', '')); // 移除前缀
    const blob = new Blob([svgContent], { type: 'image/svg+xml' });
    return blob
  } else {
    let base64ToBlob = function (code) {
      const parts = code.split(';base64,')
      const contentType = parts[0].split(':')[1]
      const raw = window.atob(parts[1])
      const rawLength = raw.length
      const uInt8Array = new Uint8Array(rawLength)
      for (let i = 0; i < rawLength; ++i) {
        uInt8Array[i] = raw.charCodeAt(i)
      }
      return new Blob([uInt8Array], {
        type: contentType
      })
    }
    let blob = base64ToBlob(content)
    return blob
  }
}
// 给数组对象添加新属性
export function getNewArr(arr, obj) {
  if ('text' in obj) {
    return arr.map(i => {
      return { ...i, ...obj, text: i.text + obj.text }
    })
  } else {
    return arr.map(i => {
      return { ...i, ...obj }
    })
  }
}
// 保存图片
export function savePicture(echart, title, selectedLengend, isCopy) {
  let blob = imageBase64Data(echart, selectedLengend)
  if (isCopy) {// 复制到剪切板
    try {
      navigator.clipboard.write([
        new ClipboardItem({ 'image/png': blob })
      ])
      // alert('图表图片已复制')
    } catch (err) {
      console.error(err)
      alert('复制失败，请检查浏览器权限')
    }
    return
  }
  const aLink = document.createElement('a')
  const evt = document.createEvent('HTMLEvents')
  evt.initEvent('click', true, true) // initEvent 不加后两个参数在FF下会报错  事件类型，是否冒泡，是否阻止浏览器的默认行为
  let name = title || echart.getOption().title[0].text
  aLink.download = name.replace(/\./g, "·") //名称中不能有半角点，否则下载时后缀名会错误
  aLink.href = URL.createObjectURL(blob)
  aLink.click()
  setTimeout(() => URL.revokeObjectURL(aLink), 100);//释放URL 对象
}
// 复制图片二进制流到剪贴板
export function copyPicture(echart, selectedLengend) {
  window.navigator.permissions.query({ name: "clipboard-write" }).then(res => {
    if (res.state == 'granted') {// 已授权
      // eslint-disable-next-line no-undef
      const { ClipboardItem } = window
      let blob = imageBase64Data(echart, selectedLengend)
      const clipboardItemInput = new ClipboardItem({ "image/png": blob });
      navigator.clipboard.write([clipboardItemInput]);
    } else {
      alert('无剪切板读写权限！')
    }
  }).catch(err => {
    alert(err)
  })
}
// 趋势图计算标记线值
export function computedMarkValue(arr, type) {
  let resault = arr.map(item => {
    return item.map(i => {
      return Number(i[1])
    })
  })
  let oneDomension = resault.flat()
  let length = oneDomension.length
  let min = Infinity, max = -Infinity, avg = -Infinity, mid = -Infinity
  if (type) {
    if (type == 'max') {
      // 最大值
      while (length--) {
        max = oneDomension[length] > max ? oneDomension[length] : max;
      }
      return max
    } else if (type == 'min') {
      // 最小值
      while (length--) {
        min = oneDomension[length] < min ? oneDomension[length] : min;
      }
      return min
    } else if (type == 'avg') {
      // 平均值
      let total = oneDomension.reduce((accumulator, currentValue) => accumulator + currentValue); // 获取数组元素之和
      avg = Math.round(total / length * 10000) / 10000
      return avg
    } else if (type == 'mid') {
      let sortArr = oneDomension.sort((a, b) => a - b)
      let middle = (oneDomension.length + 1) / 2
      if (length % 2 == 1) {
        mid = sortArr[middle - 1]
      } else {
        mid = (sortArr[middle - 1.5] + sortArr[middle - 0.5]) / 2
      }
      return mid
    }
  } else {
    // s数据量过大，math.max()会报错溢出
    // 最大值
    while (length--) {
      max = oneDomension[length] > max ? oneDomension[length] : max;
    }
    // 最小值
    while (length--) {
      min = oneDomension[length] < min ? oneDomension[length] : min;
    }
    // 平均值
    let total = oneDomension.reduce((accumulator, currentValue) => accumulator + currentValue); // 获取数组元素之和
    avg = Math.round(total / length * 100) / 100
    // 中位数
    let sortArr = oneDomension.sort((a, b) => a - b)
    let middle = (oneDomension.length + 1) / 2
    if (length % 2 == 1) {
      mid = sortArr[middle - 1]
    } else {
      mid = (sortArr[middle - 1.5] + sortArr[middle - 0.5]) / 2
    }
    //  mid = Math.round(mid * 100) / 100
    return { max, min, avg, mid }
  }
}
// 放大
export function addEnlargement(echart) {
  let viewList = echart._componentsViews
  for (let i = 0; i < viewList.length; i++) {
    let arr = viewList[i].__id.split('_')
    let type = arr[arr.length - 1]
    if (type == 'toolbox') {
      viewList[i]._features.dataZoom.onclick('', '', 'zoom')
      break
    }
  }
}
// 回退
export function addRollback(echart) {
  if (echart) {
    let viewList = echart._componentsViews
    for (let i = 0; i < viewList.length; i++) {
      let arr = viewList[i].__id.split('_')
      let type = arr[arr.length - 1]
      if (type == 'toolbox') {
        viewList[i]._features.dataZoom.onclick('', '', 'back')
        break
      }
    }
  }
  // }
}

// 根据单位，换算 options 单位，Value 计算前的值，结果返回换算后的值
// 单位切换，蓝点，红点等多处使用
export function getYValueforUC(options, val) {
  let value = val
  /*   if (options == 'mm/s' || options == 'm/s^2' || options == 'μm' || options == '') {
    value = value;
  } else */ if (options == 'cm/s') {
    value *= 0.1
  } else if (options == 'in/s') {
    value *= 1 / 25.4
  } else if (options == 'um/s') {
    /* 2015/8/12；周彦峰修改；原因：增加转换um/s*/
    value *= 1000
  } else if (options == 'yd/s^2') {
    value /= 0.8361
  } else if (options == 'ft/s^2') {
    value *= 10.764
  } else if (options == 'g') {
    value /= 9.8
  } else if (options == 'mm') {
    value /= 1000
  }
  // value = parseFloat(parseFloat(value).toFixed(3))
  return value
}
// 时间转换
export function timestampToTime1(timestamp) {
  const date = new Date(parseInt(timestamp)) // 时间戳为10位需*1000，时间戳为13位的话不需乘1000
  let Y, M, D, h, m, s
  Y = `${date.getFullYear()}-`
  M = `${date.getMonth() + 1 < 10 ? `0${date.getMonth() + 1}` : date.getMonth() + 1}-`
  D = `${date.getDate() < 10 ? `0${date.getDate()}` : date.getDate()} `
  h = `${date.getHours() < 10 ? `0${date.getHours()}` : date.getHours()}:`
  m = `${date.getMinutes() < 10 ? `0${date.getMinutes()}` : date.getMinutes()}:`
  s = date.getSeconds() < 10 ? `0${date.getSeconds()}` : date.getSeconds()
  return Y + M + D + h + m + s
}
/**
 * 例：this.$outputXlsxFile([[['字段1', '字段2'], [1, 2]],[['字段1', '字段2'], [1, 2]]], [{wch: 10}, {wch: 50}], '测试导出', ['sheetName']) 得到 测试导出.xlsx 文件
 * 第一个参数是导出的数组对象，第二个参数是设置字段宽度，第三个参数是文件名
 */
export function outputXlsxFile(data, xlsxName, sheetNameArr) {
  /* convert state to workbook */
  const wb = XLSX.utils.book_new()
  if (sheetNameArr.length) {
    data.forEach((item, index) => {
      const ws = XLSX.utils.aoa_to_sheet(item)
      let long = item[0].length
      let wscols = []
      for (let i = 0; i < long; i++) {
        wscols.push({ wch: 30 })
      }
      ws['!cols'] = wscols
      XLSX.utils.book_append_sheet(wb, ws, sheetNameArr[index].slice(0, 31))
    })
    XLSX.writeFile(wb, xlsxName + '.xlsx')
  }
}
// 判断空对象
export function isEmptyObject(obj) {
  for (var key in obj) {
    return false
  }
  return true
}

export function setTooltip(option) {
  var html = ''
  if (option.componentType === 'markLine') {
    html = `X:${option.data.coord[0]} ${option.seriesName} Y:${option.data.coord[1]}`
  } else if (option.componentType === 'markPoint') {
    html = `X:${option.data.coord[0]} ${option.seriesName} Y:${option.data.coord[1]}`
  } else {
    html = `X:${option.name}  ${option.seriesName} Y:${option.value}`
  }
  return html
}
export function setyAxisInterval(chart) {
  const yvalue = chart.getModel().getComponent('yAxis', 0).axis.scale.getExtent()
  var difvalue = yvalue[1] - yvalue[0]
  if (difvalue) {
    chart.setOption({
      yAxis: {
        interval: (yvalue[1] - yvalue[0]) / 5
      }
    })
  } else {
    var option = chart.setOption()
    delete option.yAxis[0].interval
    chart.clear()
    chart.setOption(option, true)
  }
}

// 数组去重
export function uniq(array) {
  var allArr = []
  for (var i = 0; i < array.length; i++) {
    var flag = true
    for (var j = 0; j < allArr.length; j++) {
      if (array[i].dataIndex === allArr[j].dataIndex) {
        flag = false
      }
    }
    if (flag) {
      allArr.push(array[i])
    }
  }
  return allArr
}

// chart系列颜色1，用于特征值趋势
export const colorList = [
  '#51B031',
  '#4C86EA',
  '#C1BF3A',
  '#A03AC1',
  '#C13A3A',
  '#3AAFC1',
  '#C17C3A',
  '#3A5AC1',
  '#79D501',
  '#018BD6',
  '#D5D201',
  '#C301D5',
  '#D5013D',
  '#01D5D5',
  '#D59601',
  '#2801D5',
  '#35C492',
  '#2D5CAD',
  '#ACB31C',
  '#7A15A2',
  '#AC2D3E',
  '#4396AD',
  '#AD8F3C',
  '#332DAC',
  '#08A400',
  '#3753E5',
  '#A59500',
  '#7C02B9',
  '#C02B1A',
  '#0096AD',
  '#C1651A',
  '#441AC0',
  '#78D791',
  '#78A4D8',
  '#CFD778',
  '#B178D7',
  '#D77878',
  '#78D4D7',
  '#D7A678',
  '#7888D7'
]
// chart系列颜色2，用于波形
export const colorSeires = ['#0781FF', '#59A753', '#D738F7', '#CCAA00', '#0A4386', '#FF9501', '#7C30A1', '#D7977A', '#0752C0', '#106C11', '#957700', '#00B41E', '#C05939', '#00C3C4']


/**
 * 数字相加
 * @param {*} arg1 
 * @param {*} arg2 
 * @returns 
 */
export function accAdd(arg1, arg2) {
  return changeNum(arg1, arg2)
}

/**
 * 数字相减
 * @param {*} arg1 
 * @param {*} arg2 
 * @returns 
 */
export function accSub(arg1, arg2) {
  return changeNum(arg1, arg2, false)
}

/**
 * 数字相乘
 * @param {*} arg1 
 * @param {*} arg2 
 * @returns 
 */
export function accMul(arg1, arg2) {
  let m = 0
  m = accAdd(m, getDecimalLength(arg1))
  m = accAdd(m, getDecimalLength(arg2))
  return getNum(arg1) * getNum(arg2) / Math.pow(10, m)
}

/**
 * 数字相除
 * @param {*} arg1 
 * @param {*} arg2 
 * @returns 
 */
export function accDiv(arg1, arg2) {
  let t1, t2
  t1 = getDecimalLength(arg1)
  t2 = getDecimalLength(arg2)
  if (t1 - t2 > 0) {
    return (getNum(arg1) / getNum(arg2)) / Math.pow(10, t1 - t2)
  } else {
    return (getNum(arg1) / getNum(arg2)) * Math.pow(10, t2 - t1)
  }
}

function changeNum(arg1 = '', arg2 = '', isAdd = true) {
  function changeInteger(arg, r, maxR) {
    if (r != maxR) {
      let addZero = ''
      for (let i = 0; i < maxR - r; i++) {
        addZero += '0'
      }
      arg = Number(arg.toString().replace('.', '') + addZero)
    } else {
      arg = getNum(arg)
    }
    return arg
  }
  let r1, r2, maxR, m
  r1 = getDecimalLength(arg1)
  r2 = getDecimalLength(arg2)
  maxR = Math.max(r1, r2)
  arg1 = changeInteger(arg1, r1, maxR)
  arg2 = changeInteger(arg2, r2, maxR)
  m = Math.pow(10, maxR)
  if (isAdd) {
    return (arg1 + arg2) / m
  } else {
    return (arg1 - arg2) / m
  }
}

function getDecimalLength(arg = '') {
  try {
    return arg.toString().split('.')[1].length
  } catch (e) {
    return 0
  }
}

function getNum(arg = '') {
  return Number(arg.toString().replace('.', ''))
}

// 防抖动
export const debounce = (fn, wait = 1000) => {
  let timer
  return function () {
    let context = this
    let args = arguments
    timer && clearTimeout(timer)
    timer = setTimeout(() => {
      // console.log('定时器函数执行成功了')
      fn.apply(context, args)
    }, wait)
  }
}

export function getTargetNode(ele, target) {
  //ele是内部元素，target是你想找到的包裹元素
  if (!ele || ele === document) return false;
  return ele === target ? true : getTargetNode(ele.parentNode, target);
}

export function echartsYaxisLabelFormatter(value) {
  // 【关键修改】优先处理 0，直接返回 '0'
  if (value === 0) {
    return '0';
  }

  // 对于过大或过小的数，使用科学计数法
  if (Math.abs(value) < 0.0001 || Math.abs(value) >= 10000) {
    return value.toExponential(2);
  }

  // 普通数值...
  if (Math.abs(value) < 1) {
    return parseFloat(value.toFixed(8)).toString();
  }

  return parseFloat(value.toFixed(4)).toString();
}
/* export function echartsYaxisLabelFormatter(value) {
  var res = value.toString();
  var numN1 = 0;
  var numN2 = 1;
  var num1 = 0;
  var num2 = 0;
  var t1 = 1;
  for (var k = 0; k < res.length; k++) {
    if (res[k] == ".") t1 = 0;
    if (t1) num1++; else num2++;
  }
  if (Math.abs(value) < 1 && res.length > 4) {
    for (var i = 2; i < res.length; i++) {
      if (res[i] == "0") {
        numN2++;
      }
      else if (res[i] == ".") continue;
      else break;
    }
    var v = parseFloat(value);
    v = accMul(v, Math.pow(10, numN2));
    return v.toString() + "e-" + numN2;
  }
  else if (num1 > 4) {
    if (res[0] == "-") numN1 = num1 - 2;
    else numN1 = num1 - 1;
    var v = parseFloat(value);
    v = accDiv(v, Math.pow(10, numN1));
    if (num2 > 4) v = v.toFixed(4);
    return v.toString() + "e" + numN1;
  }
  else return parseFloat(value)
} */

export function getWorkerUrl(moduleID) {
  let url2 = moduleID;
  if (isCrossOriginUrl(url2)) {
    const script = `importScripts("${url2}");`;
    let blob;
    try {
      blob = new Blob([script], {
        type: "application/javascript"
      });
    } catch (e2) {
      const BlobBuilder = window.BlobBuilder || window.WebKitBlobBuilder || window.MozBlobBuilder || window.MSBlobBuilder;
      const blobBuilder = new BlobBuilder();
      blobBuilder.append(script);
      blob = blobBuilder.getBlob("application/javascript");
    }
    const URL2 = window.URL || window.webkitURL;
    url2 = URL2.createObjectURL(blob);
    // }
    return url2;
  }
}

export function isCrossOriginUrl(url2) {
  let a;
  if (!defined_default(a)) {
    a = document.createElement("a");
  }
  a.href = window.location.href;
  const host = a.host;
  const protocol = a.protocol;
  a.href = url2;
  a.href = a.href;
  return protocol !== a.protocol || host !== a.host;
}

function defined_default(value) {
  return value !== void 0 && value !== null;
}

// 时间戳转换成年月日
export function timestampToTime(timestamp) {
  var date = new Date(timestamp); //时间戳为10位需*1000，时间戳为13位的话不需乘1000
  var Y = date.getFullYear() + '-'; //年  
  var M = `${date.getMonth() + 1 < 10 ? `0${date.getMonth() + 1}` : date.getMonth() + 1}-`
  var D = `${date.getDate() < 10 ? `0${date.getDate()}` : date.getDate()} `
  // var h = date.getHours() + ':';
  // var m = date.getMinutes() + ':';
  // var s = date.getSeconds();
  return Y + M + D //+ h + m + s;
}