
import { judgeYbExist, getClosestNumber } from '../ybCommonTools'
let colorList = [
  '#91001C', '#C36D2B', '#B7A90F', '#00923D', '#21CACA', '#7400F9', '#FF00D4'
]
// 边频游标
export class BianPinYB {
  constructor(lineNum) {
    // ...
    this.lineNum = lineNum || 7
    this.middleLineIndex = Math.floor(this.lineNum / 2) // 游标中间的线是第几根
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
   * @param {*} param2 添加游标线的一组数据及这组数据对应的seriesId
   * @param {*} getYBPointer 游标点位置传出去的方法
   */
  addYBLines(prefixName, echart, { data, seriesId, yAxisIndex, options }, getYBPointer) {
    this.seriesId = seriesId
    this.gridIndex = yAxisIndex
    this.getYBPointer = getYBPointer
    this.prefixName = prefixName
    this.lineColor = colorList[prefixName.slice(12) % 6] //'bianpinGroup0'
    this.diffNumGap = (data[data.length - 1][0] - data[0][0]) / (this.lineNum + 3)
    let pointerList = []
    let marklineDatas = []
    for (let item = 0; item < this.lineNum; item++) {
      let { closeIndex, closeValue } = getClosestNumber(data, data[0][0] + (item + 1) * this.diffNumGap)
      pointerList.push(closeValue)
      marklineDatas.push({
        name: prefixName,
        id: prefixName + '_' + item,
        xAxis: closeValue[0].toString(),
        value: closeValue,
        gridIndex: this.gridIndex,
        dataIndex: closeIndex,
        lineStyle: {
          color: this.lineColor,
          width: item == this.middleLineIndex ? 2 : 1,
          type: 'dotted',
        },
        // symbol: item == this.middleLineIndex ? 'triangle' : 'none',
        dataType: item - this.middleLineIndex,
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
          backgroundColor: '',//item == this.middleLineIndex ? '#000' : '',
        },
      })
    }
    this.YBxyList = pointerList
    this.drawLine(marklineDatas, options, echart)
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
    let { lineArr } = judgeYbExist(options, this.prefixName, this.seriesId)
    let graphicIndex = Number(lineId.split('_')[1])
    let diffNumGap = lineArr[graphicIndex].value[0] - data[newDataIndex][0]
    let dataIndexArr = []
    // 移动中间游标线
    if (lineId === this.prefixName + '_' + this.middleLineIndex) {
      lineArr.forEach((item, i) => {
        let { closeIndex, closeValue } = getClosestNumber(data, item.value[0] - diffNumGap)
        lineArr[i].xAxis = closeValue ? closeValue[0].toString() : ''
        lineArr[i].value = closeValue || []
        lineArr[i].dataIndex = closeIndex
        dataIndexArr.push(closeValue)
      })
    } else {
      // 移动两侧游标线
      this.diffNumGap = Math.abs(data[newDataIndex][0] - lineArr[this.middleLineIndex].value[0]) / Math.abs(graphicIndex - this.middleLineIndex)
      lineArr.forEach((item, index) => {
        let itemXNew = 0
        if (index < this.middleLineIndex) {
          itemXNew = lineArr[this.middleLineIndex].value[0] - this.diffNumGap * (this.middleLineIndex - index)
        } else if (index > this.middleLineIndex) {
          itemXNew = lineArr[this.middleLineIndex].value[0] + this.diffNumGap * (index - this.middleLineIndex)
        }
        if (index === this.middleLineIndex) {
          dataIndexArr.push(item.value)
        } else {
          let { closeIndex, closeValue } = getClosestNumber(data, itemXNew)
          lineArr[index].xAxis = closeValue ? closeValue[0].toString() : ''
          lineArr[index].value = closeValue || []
          lineArr[index].dataIndex = closeIndex
          dataIndexArr.push(closeValue)
        }
      })
    }
    this.YBxyList = dataIndexArr
    this.drawLine(lineArr, options, echart)
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
    let dataIndexArr = []
    let { lineArr } = judgeYbExist(options, this.prefixName, this.seriesId)
    if (lineId === this.prefixName + '_' + this.middleLineIndex) {
      let currentIndex = lineArr[this.middleLineIndex].dataIndex
      let displacement
      if (direction == 'left') {
        if (data[currentIndex - 1]) {
          displacement = data[currentIndex][0] - data[currentIndex - 1][0]
        } else {
          return
        }
      } else {
        if (data[currentIndex + 1]) {
          displacement = data[currentIndex + 1][0] - data[currentIndex][0]
        } else {
          return
        }
      }
      lineArr.forEach((item, i) => {
        let { closeIndex, closeValue } = getClosestNumber(data, direction == 'left' ? (item.value[0] - displacement) : (item.value[0] + displacement))
        lineArr[i].xAxis = closeValue ? closeValue[0].toString() : ''
        lineArr[i].value = closeValue || []
        lineArr[i].dataIndex = closeIndex
        dataIndexArr.push(closeValue)
      })
    } else {
      let graphicIndex = Number(lineId.split('_')[1])
      let currentIndex = lineArr[graphicIndex].dataIndex
      if (direction == 'left') {
        if (currentIndex == 0 || currentIndex == lineArr[this.middleLineIndex].dataIndex) return;
        this.diffNumGap = Math.abs(data[currentIndex - 1][0] - lineArr[this.middleLineIndex].value[0]) / Math.abs(graphicIndex - this.middleLineIndex)
      } else { // right
        if (currentIndex == data.length - 1 || currentIndex == lineArr[this.middleLineIndex].dataIndex) return;
        this.diffNumGap = Math.abs(data[currentIndex + 1][0] - lineArr[this.middleLineIndex].value[0]) / Math.abs(graphicIndex - this.middleLineIndex)
      }
      lineArr.forEach((item, index) => {
        let itemXNew
        if (index < this.middleLineIndex) {
          itemXNew = lineArr[this.middleLineIndex].value[0] - this.diffNumGap * (this.middleLineIndex - index)
        } else if (index > this.middleLineIndex) {
          itemXNew = lineArr[this.middleLineIndex].value[0] + this.diffNumGap * (index - this.middleLineIndex)
        }
        if (index === this.middleLineIndex) {
          dataIndexArr.push(item.value)
        } else {
          let { closeIndex, closeValue } = getClosestNumber(data, itemXNew)
          lineArr[index].xAxis = closeValue ? closeValue[0].toString() : ''
          lineArr[index].value = closeValue || []
          lineArr[index].dataIndex = closeIndex
          dataIndexArr.push(closeValue)
        }
      })
    }
    this.YBxyList = dataIndexArr
    this.drawLine(lineArr, options, echart)
  }

}