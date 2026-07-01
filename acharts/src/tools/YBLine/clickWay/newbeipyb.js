
import { judgeYbExist, getClosestNumber } from '../ybCommonTools'
let colorList = [
  '#00AC70', '#33AEE4', '#AA00FF', '#651FFF', '#CDA715', '#FF6F00', '#D50000'
]
// 倍频游标
export class BeiPinYB {
  constructor(lineNum) {
    this.lineNum = lineNum || 5
    this.diffNumGap = 0// X轴差值间隔
    this.getYBPointer = null // 游标数据传输方法
    this.prefixName = '' // 此组游标名称前缀，在实例组件中定义
    this.lineColor = '' // 此组游标线颜色
    this.seriesId = '' // 此组游标对应的seriesId
    this.YBxyList = []
    this.gridIndex = 0
  }
  /**
   * 添加游标线及对应方法
   * @param {*} prefixName 此组游标名称前缀，在实例组件中定义
   * @param {*} echart 图表实例
   * @param {*} param2 添加游标线的一组数据及这组数据对应的seriesId基础配置
   * @param {*} getYBPointer 游标点位置传出去的方法
   */
  addYBLines(prefixName, echart, { data, seriesId, yAxisIndex, options }, getYBPointer, defaultLinePointerList) {
    this.seriesId = seriesId
    this.gridIndex = yAxisIndex
    // this.chart = echart
    this.getYBPointer = getYBPointer
    this.prefixName = prefixName
    this.lineColor = defaultLinePointerList ? '#ff0000' : colorList[prefixName.slice(11) % 7] //'beipinGroup0'
    // 计算数据点间隔
    this.diffNumGap = defaultLinePointerList ? (defaultLinePointerList[1].value - defaultLinePointerList[0].vaue) : data[data.length - 1][0] / (this.lineNum + 2)
    let pointerList = []
    let marklineDatas = []
    for (let item = 0; item < this.lineNum; item++) {
      let xValue = defaultLinePointerList ? defaultLinePointerList[item].value : (item + 1) * this.diffNumGap
      let { closeIndex, closeValue } = getClosestNumber(data, xValue)
      pointerList.push(closeValue)
      marklineDatas.push({
        name: prefixName,
        id: prefixName + '_' + item,
        xAxis: closeValue[0].toString(),// 转换为string类型，解决坐标点与makline不重合问题
        value: closeValue,
        dataIndex: closeIndex,
        defaultValue: defaultLinePointerList ? defaultLinePointerList[item].value : null,
        gridIndex: this.gridIndex,
        lineStyle: {
          color: this.lineColor,//index == 0 ? '#00f7ff' : '#e2ff05',
          width: 1,
          type: 'dashed',
        },
        dataType: defaultLinePointerList ? defaultLinePointerList[item].label : (item + 1),
        label: {
          show: true,
          color: this.lineColor,
          formatter: (param) => {
            return param.data.dataType
          },
          width: 6,
          height: 10,
          position: 'end',
          distance: [-3, -10],
          lineHeight: 30,
          fontSize: 10,
          backgroundColor: '',
        },
      })
    }
    this.YBxyList = pointerList
    this.drawLine(marklineDatas, options, echart)
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
    let changePositionData = []
    this.diffNumGap = newData[0] / (graphicIndex + 1)
    lineArr.forEach((item, i) => {
      let { closeIndex, closeValue } = getClosestNumber(data, this.diffNumGap * (i + 1))
      lineArr[i].xAxis = closeValue ? closeValue[0].toString() : ''
      lineArr[i].value = closeValue || []
      lineArr[i].dataIndex = closeIndex
      changePositionData.push(closeValue)
    })
    this.YBxyList = changePositionData
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
    let { markLine, markPoint, color, name } = options.series.find(i => i.id == this.seriesId)
    let others = markLine.data.filter(i => i.name !== this.prefixName)
    let othersPoint = markPoint.data.filter(i => i.name !== this.prefixName)
    let points = []
    lineArr.forEach(i => {
      points.push({
        name: this.prefixName,
        coord: i.value,
        itemStyle: {
          borderColor: this.lineColor,
        }
      })
    })
    echart.setOption({
      series: [{
        id: this.seriesId,
        markLine: {
          animation: false,
          symbol: 'none',
          data: [...others, ...lineArr]
        },
        markPoint: {
          data: [...othersPoint, ...points]
        }
      }]
    })
    this.getYBPointer(this.YBxyList, this.lineColor, this.prefixName, this.seriesId, { color, name })
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
    let { lineArr } = judgeYbExist(options, this.prefixName, this.seriesId)
    let { data } = options.series.find(i => i.id == this.seriesId)
    let changePositionData = []
    let graphicIndex = Number(lineId.split('_')[1])//beipinGroup0_1,取1
    let currentIndex = direction == 'left' ? lineArr[graphicIndex].dataIndex - 1 : lineArr[graphicIndex].dataIndex + 1
    this.diffNumGap = data[currentIndex][0] / (graphicIndex + 1)
    lineArr.forEach((item, i) => {
      let { closeIndex, closeValue } = getClosestNumber(data, this.diffNumGap * (i + 1))
      lineArr[i].xAxis = closeValue ? closeValue[0].toString() : ''
      lineArr[i].value = closeValue || []
      lineArr[i].dataIndex = closeIndex
      changePositionData.push(closeValue)
    })
    this.YBxyList = changePositionData
    this.drawLine(lineArr, options, echart)
  }
}
