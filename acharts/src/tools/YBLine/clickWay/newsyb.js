import { judgeYbExist, getClosestNumber } from '../ybCommonTools'
let colorList = [
  '#EF6C00', '#C9B60C', '#2C9D00', '#21ADBD', '#6200EA', '#BF00B5', '#001BB7'
]

// 双游标
export class SYB {
  constructor() {
    this.diffNumGap = 0 // X轴差值间隔
    this.getYBPointer = null // 游标数据传输方法
    this.prefixName = '' // 此组游标名称前缀，在实例组件中定义
    this.lineColor = '' // 此组游标线颜色
    this.seriesId = '' // 此组游标对应的seriesId
    this.YBxyList = []
    this.gridIndex = 0
  }

  /*
  echart:echarts示例对象
  lineDataSourse:数据源{
    data：[]
  }
  */
  /**
   * 绘制游标线
   * @param {*} prefixName 游标名称
   * @param {*} echart 图谱对象
   * @param {*} param2 数据对象：在哪个serieID，哪条数据上增加游标
   * @param {*} getYBPointer 接收游标位置坐标的方法
   */
  addYBLines(prefixName, echart, { data, seriesId, yAxisIndex, options }, getYBPointer) {
    this.seriesId = seriesId
    this.gridIndex = yAxisIndex
    this.prefixName = prefixName
    this.getYBPointer = getYBPointer
    this.lineColor = colorList[prefixName.slice(11) % 7]
    this.diffNumGap = (data[data.length - 1][0] - data[0][0]) / 4
    let markLineData = []
    let pointerList = []
    for (let i = 0; i < 2; i++) {
      let { closeIndex, closeValue } = getClosestNumber(data, data[0][0] + (i + 1) * this.diffNumGap)
      pointerList.push(closeValue)
      markLineData.push({
        name: prefixName,
        id: prefixName + '_' + i,
        gridIndex: this.gridIndex,
        xAxis: closeValue[0].toString(),
        value: closeValue,
        dataIndex: closeIndex,
        lineStyle: {
          color: this.lineColor,//'#00f7ff',
          width: i == 0 ? 2 : 1,
          type: [5, 2],
          dashOffset: 2,
        },
        label: {
          show: true,
          color: this.lineColor,
          formatter: (param) => {
            return ''
          },
          width: 6,
          height: 10,
          position: 'end',
          distance: [-3, -10],
          lineHeight: 30,
          backgroundColor: '',
        },
      })
    }
    this.YBxyList = pointerList
    this.drawLine(markLineData, options, echart)
  }

  /**
   * 
   * @param {*} lineId 移动的游标lineId
   * @param {*} newDataArr 点击的目标位置对应的serires及dataindex信息（数组）
   * @param {*} echart 实例
   * @returns 
   */
  onPointTranslation(lineId, newDataArr, echart) {
    let options = echart.getOption()
    let data = []
    let seriesIndex = 0
    options.series.forEach((i, index) => {
      if (i.id == this.seriesId) {
        data = i.data
        seriesIndex = index
      }
    })
    let currentSeries = newDataArr.find(i => i.seriesIndex == seriesIndex)
    if (!currentSeries) return;
    let newDataIndex = currentSeries.dataIndex
    let newData = data[newDataIndex]
    let graphicIndex = Number(lineId.split('_')[1])
    let { lineArr } = judgeYbExist(options, this.prefixName, this.seriesId)
    if (graphicIndex == 0) {
      // move
      // 顶到右侧边界，不能再拖动, 重置move line位置

      // markline 重置
      lineArr[0] = {
        ...lineArr[0],
        xAxis: newData ? newData[0].toString() : '',
        value: newData ? newData : [],
        dataIndex: newDataIndex
      }
      let { closeIndex, closeValue } = getClosestNumber(data, newData[0] + this.diffNumGap)
      lineArr[1] = {
        ...lineArr[1],
        xAxis: closeValue ? closeValue[0].toString() : '',
        value: closeValue ? closeValue : [],
        dataIndex: closeIndex
      }
    } else {
      // 不能移到标准线的左侧
      if (newData[0] < lineArr[0].value[0]) {
        return
      }
      // 扩大间距
      this.diffNumGap = newData[0] - lineArr[0].value[0]
      lineArr[1].xAxis = newData[0].toString()
      lineArr[1].value = newData
      lineArr[1].dataIndex = newDataIndex
    }
    this.YBxyList = [lineArr[0].value, lineArr[1].value]
    this.drawLine(lineArr, options, echart)
  }

  // 绘制
  /**
   * 
   * @param {*} lineArr 更新后的此组游标的位置信息 
   * @param {*} options
   * @param {*} echart 实例
   */
  drawLine(lineArr, options, echart) {
    let { markLine, markPoint, name, color } = options.series.find(i => i.id == this.seriesId)
    let others = markLine.data.filter(i => i.name !== this.prefixName)
    let othersPoint = markPoint.data.filter(i => i.name !== this.prefixName)
    echart.setOption({
      series: [{
        id: this.seriesId,
        markLine: {
          animation: false,
          symbol: 'none',
          data: [...others, ...lineArr]
        },
        markPoint: {
          data: [...othersPoint, {
            name: this.prefixName,
            coord: this.YBxyList[0],
            itemStyle: {
              borderColor: this.lineColor,
            },
          }, {
            name: this.prefixName,
            coord: this.YBxyList[1],
            itemStyle: {
              borderColor: this.lineColor,
            },
          }]
        }
      }]
    })
    this.getYBPointer(this.YBxyList, this.lineColor, this.prefixName, this.seriesId, { name, color })
  }

  /**
  * 键盘左右键移动
  * @param {*} direction 移动的方向
  * @param {*} lineId 移动的目标线
  * @param {*} echart 实例
  * @returns 
  */
  translation(direction, lineId, echart) {
    let options = echart.getOption()
    let { data } = options.series.find(i => i.id == this.seriesId)
    let dataIndex1, dataIndex2
    let newData = []
    let { lineArr } = judgeYbExist(options, this.prefixName, this.seriesId)
    if (lineId === this.prefixName + '_0') {
      if (lineArr[0].dataIndex) {
        dataIndex1 = direction == 'left' ? lineArr[0].dataIndex - 1 : lineArr[0].dataIndex + 1
        newData = data[dataIndex1]
      } else if (lineArr[1].dataIndex) {
        let newDiff = direction == 'left' ? (data[lineArr[1].dataIndex - 1][0] - this.diffNumGap) : (data[lineArr[1].dataIndex + 1][0] - this.diffNumGap)
        let { closeIndex, closeValue } = getClosestNumber(data, newDiff)
        dataIndex1 = closeIndex
        newData = closeValue
      } else {
        let newDiff = direction == 'left' ? (lineArr[0].value[0] - (data[1][0] - data[0][0])) : (lineArr[0].value[0] + (data[1][0] - data[0][0]))
        let { closeIndex, closeValue } = getClosestNumber(data, newDiff)
        dataIndex1 = closeIndex
        newData = closeValue
      }
      lineArr[0] = {
        ...lineArr[0],
        xAxis: newData[0] ? newData[0].toString() : '',
        value: newData ? newData : [],
        dataIndex: dataIndex1
      }
      let { closeIndex, closeValue } = getClosestNumber(data, lineArr[0].value[0] + this.diffNumGap)
      lineArr[1] = {
        ...lineArr[1],
        xAxis: closeValue ? closeValue[0].toString() : '',
        value: closeValue ? closeValue : [],
        dataIndex: closeIndex
      }
    } else {
      // 不能移到标准线的左侧
      if (lineArr[1].dataIndex) {
        dataIndex2 = direction == 'left' ? lineArr[1].dataIndex - 1 : lineArr[1].dataIndex + 1
        newData = data[dataIndex2]
      } else {
        let newDiff = direction == 'left' ? (lineArr[1].value[0] - (data[1][0] - data[0][0])) : (lineArr[1].value[0] + (data[1][0] - data[0][0]))
        let { closeIndex, closeValue } = getClosestNumber(data, newDiff)
        dataIndex2 = closeIndex
        newData = closeValue
      }
      if (newData[0] < lineArr[0].value[0]) {
        return
      }
      // 扩大间距
      this.diffNumGap = newData[0] - lineArr[0].value[0]
      lineArr[1].xAxis = newData[0].toString()
      lineArr[1].value = newData
      lineArr[1].dataIndex = dataIndex2
    }
    this.YBxyList = [lineArr[0].value, lineArr[1].value]
    this.drawLine(lineArr, options, echart)
  }
}