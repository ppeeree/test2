import { judgeYbExist, getClosestNumber } from '../ybCommonTools'
let colorList = ['#1D8DC0', '#BE9B17', '#006E38', '#4E0DDD', '#9219CF', '#BF0025', '#FF6F00'
]
// 间隔游标
export class JianGeYB {
  constructor(lineNum) {
    // ...
    //this.ybBase = new YbBase();
    this.lineNum = lineNum || 5 // 一组游标线数量
    this.diffNumGap = 0 // X轴差值间隔
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
    this.lineColor = colorList[prefixName.slice(11) % 7]//'beipinGroup0'
    this.diffNumGap = (data[data.length - 1][0] - data[0][0]) / (this.lineNum + 2)
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
          width: item == 0 ? 2 : 1,
          type: [5, 5, 2, 5],
          dashOffset: 0,
        },
        dataType: item,
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
   *   // 游标拖拽方法
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
    let currentIndex = currentSeries.dataIndex
    let graphicIndex = Number(lineId.split('_')[1])
    let { lineArr } = judgeYbExist(options, this.prefixName, this.seriesId)
    let changePositionData = []
    if (graphicIndex == 0) {
      lineArr.forEach((item, i) => {
        if (i !== 0) {
          let { closeIndex, closeValue } = getClosestNumber(data, data[currentIndex][0] + this.diffNumGap * i)
          changePositionData.push(closeValue)
          lineArr[i].xAxis = closeValue ? closeValue[0].toString() : ''
          lineArr[i].value = closeValue || []
          lineArr[i].dataIndex = closeIndex
        } else {
          let unit = data[currentIndex]
          changePositionData.push(unit)
          lineArr[i].xAxis = unit ? unit[0].toString() : ''
          lineArr[i].value = unit || []
          lineArr[i].dataIndex = currentIndex
        }
      })

    } else {
      // 不能移动越过标准线
      if (data[currentIndex][0] - lineArr[0].value[0] < 0) {
        // 判断是否移到标准线左侧
        lineArr.forEach((item, index) => {
          changePositionData.push(lineArr[0].value)
          lineArr[index].xAxis = lineArr[0].xAxis
          lineArr[index].value = lineArr[0].value
          lineArr[index].dataIndex = lineArr[0].dataIndex
        })
      } else {
        this.diffNumGap = (data[currentIndex][0] - lineArr[0].value[0]) / graphicIndex
        lineArr.forEach((item, index) => {
          if (index !== 0) {
            let { closeIndex, closeValue } = getClosestNumber(data, lineArr[0].value[0] + this.diffNumGap * index)
            changePositionData.push(closeValue)
            lineArr[index].xAxis = closeValue ? closeValue[0].toString() : ''
            lineArr[index].value = closeValue || []
            lineArr[index].dataIndex = closeIndex
          } else {
            changePositionData.push(item.value)
          }
        })
      }
    }
    this.YBxyList = changePositionData
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
    let { lineArr } = judgeYbExist(options, this.prefixName, this.seriesId)
    let dataIndexArr = []
    if (lineId === this.prefixName + '_0') {
      let currentIndex = lineArr[0].dataIndex
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
        if (currentIndex == 0 || currentIndex == lineArr[0].dataIndex) return;
        this.diffNumGap = Math.abs(data[currentIndex - 1][0] - lineArr[0].value[0]) / graphicIndex
      } else { // right
        if (currentIndex == data.length - 1 || currentIndex == lineArr[0].dataIndex) return;
        this.diffNumGap = Math.abs(data[currentIndex + 1][0] - lineArr[0].value[0]) / graphicIndex
      }
      lineArr.forEach((item, index) => {
        if (index == 0) {
          dataIndexArr.push(item.value)
        } else {
          let { closeIndex, closeValue } = getClosestNumber(data, lineArr[0].value[0] + this.diffNumGap * index)
          item.xAxis = closeValue ? closeValue[0].toString() : ''
          item.value = closeValue || []
          item.dataIndex = closeIndex
          dataIndexArr.push(closeValue)
        }
      })
    }
    this.YBxyList = dataIndexArr
    this.drawLine(lineArr, options, echart)
  }
}
