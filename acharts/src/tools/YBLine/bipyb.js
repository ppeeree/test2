
import { closest, judgeYbExist, onBDLineMouseover, initGraphicPosition } from './ybCommonTools'
import { debounce } from '../tool.js'
let colorList = [
  '#304FFE',
  '#1DE9B6',
  '#40C4FF',
  '#D500F9',
  '#D50000',
  '#FFAB40',
]
// 边频游标
export class BianPinYB {
  constructor(lineNum) {
    // ...
    //this.ybBase = new YbBase();
    this.lineNum = lineNum || 7
    this.middleLineIndex = Math.floor(this.lineNum / 2) // 游标中间的线是第几根
    // this.sideLineNum = this.middleLine - 1 // 游标中间一侧的游标线数量
    this.diffIndexGap = 0 // 间隔
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
   * @param {*} graphicBaseConfig 计算后(游标线的上下位置，长度)的游标线基础配置
   * @param {*} getYBPointer 游标点位置传出去的方法
   */
  addYBLines(prefixName, echart, { data, seriesId, yAxisIndex, options }, graphicBaseConfig, getYBPointer) {
    let { xAxis, yAxis } = options
    this.seriesId = seriesId
    this.gridIndex = yAxisIndex
    this.getYBPointer = getYBPointer
    this.prefixName = prefixName
    this.lineColor = colorList[prefixName.slice(12) % 7] //'bianpinGroup0'
    /*  let { startValue, endValue } = options.dataZoom[0]
     let startPointer = closest(data, startValue)
     let endPointer = closest(data, endValue) */
    // 计算数据点间隔
    // let gapIndex = (endPointer.index - startPointer.index) / 10
    let gapIndex = data.length / (this.lineNum + 3)
    this.diffIndexGap = gapIndex
    let pointerList = []
    let marklineDatas = []
    for (let item = 0; item < this.lineNum; item++) {
      let pointer = data[Math.round(/* startPointer.index + */(item + 1) * gapIndex)]
      pointerList.push(pointer)
      marklineDatas.push({
        name: prefixName,
        xAxis: pointer[0],
        value: pointer,
        gridIndex: this.gridIndex,
        dataIndex: Math.round(/* startPointer.index + */(item + 1) * gapIndex),
        lineStyle: {
          color: this.lineColor,//index == 3 ? '#00f7ff' : '#e2ff05',
          width: 1,
          type: 'dotted',
          opacity: 0.2,
        },
        dataType: item - this.middleLineIndex,
        label: {
          show: true,
          opacity: 1,
          color: this.lineColor,
          formatter: (param) => {
            return param.data.dataType
          }
        },
      })
    }
    let diffPointer = []
    // let { lineArr } = judgeYbExist(options)
    let graphicList = marklineDatas.map((item, index) => {
      let leftX = echart.convertToPixel({ gridIndex: this.gridIndex }, [item.xAxis, 0])[0]
      diffPointer.push(leftX)
      return {
        ...graphicBaseConfig,
        type: 'line',
        left: leftX - 2,
        $action: "replace",
        bounding: "all",
        style: {
          stroke: this.lineColor,//index == 0 ? '#fff' : '#fff', // "rgba(0,0,0,0.1)",
          shadowColor: 'transparent',//,index == 0 ? '#00f7ff' : '#e2ff05',
          lineWidth: 1,
          lineDash: 'dotted',
        },
        id: prefixName + '_' + index,
        info: this.seriesId,
        invisible: false,
        draggable: true,
        cursor: index == this.middleLineIndex ? 'move' : 'col-resize',
        ondragend: param => {
          this.onPointDragging(data, [param.offsetX, param.offsetY], index, echart)
        },
        onmouseover: (param) => {
          debounce(() => {
            onBDLineMouseover([param.offsetX, param.offsetY], this.YBxyList, { xunit: xAxis[0].name, yunit: yAxis[0].name }, index, document.getElementById(echart.id + 'YBBlock'), echart, this.prefixName, this.lineColor)
          }, 100)()
        },
        onmouseout: () => {
          debounce(() => {
            document.getElementById(echart.id + 'YBBlock').style.display = 'none'
          }, 100)()
        },
      }
    })
    const { graphic } = options
    let graphicData = graphicList
    if (graphic && graphic.length) {
      graphicData = [...graphic[0].elements, ...graphicList]
    }
    this.YBxyList = pointerList
    this.drawLine(marklineDatas, graphicData, options, echart)
  }

  // 游标拖拽方法
  /**
   * 
   * @param {*} pos 新的坐标位置
   * @param {*} graphicIndex 拖拽的游标index
   */
  onPointDragging(data, pos, graphicIndex, echart) {
    let options = echart.getOption()
    let newData = echart.convertFromPixel({ gridIndex: this.gridIndex }, pos)
    const { value, index } = closest(data, newData[0])
    newData = value
    let currentIndex = index
    let newGraphic = []
    let { lineArr } = judgeYbExist(options, this.prefixName, this.seriesId)
    let changePositionData = []
    if (graphicIndex == this.middleLineIndex) {
      // move
      let limitType = 'mid'
      // 判断两边是否顶到最大值或者最小值
      if (currentIndex - this.diffIndexGap * this.middleLineIndex < 0) {
        limitType = 'left'
      } else if (currentIndex + this.diffIndexGap * this.middleLineIndex > data.length - 1) {
        limitType = 'right'
      }
      lineArr.forEach((item, index) => {
        let dataIndex
        if (limitType === 'left') {
          dataIndex = this.diffIndexGap * index
        } else if (limitType === 'right') {
          dataIndex = data.length - 1 - (this.lineNum - 1 - index) * this.diffIndexGap
        } else {
          if (index < this.middleLineIndex) {
            dataIndex = currentIndex - this.diffIndexGap * (this.middleLineIndex - index)
          } else if (index > this.middleLineIndex) {
            dataIndex = currentIndex + this.diffIndexGap * (index - this.middleLineIndex)
          } else {
            dataIndex = currentIndex
          }
        }
        if (dataIndex < 0) {
          dataIndex = 0
        } else if (dataIndex > data.length - 1) {
          dataIndex = data.length - 1
        }
        changePositionData.push(data[Math.round(dataIndex)])
        lineArr[index].xAxis = data[Math.round(dataIndex)][0].toString()
        lineArr[index].value = data[Math.round(dataIndex)]
        lineArr[index].dataIndex = Math.round(dataIndex)
      })

    } else {
      let middlePos = echart.convertToPixel({ gridIndex: this.gridIndex }, [lineArr[this.middleLineIndex].xAxis, 0])
      // 不能移动越过标准线
      if (
        (graphicIndex > this.middleLineIndex && pos[0] - middlePos[0] < 0) ||
        (graphicIndex < this.middleLineIndex && pos[0] - middlePos[0] > 0)
      ) {
        lineArr.forEach((item, index) => {
          changePositionData.push(lineArr[this.middleLineIndex].value)
          lineArr[index].xAxis = lineArr[this.middleLineIndex].xAxis.toString()
          lineArr[index].value = lineArr[this.middleLineIndex].value
          lineArr[index].dataIndex = lineArr[this.middleLineIndex].dataIndex
        })
      } else {
        this.diffIndexGap = Math.abs(currentIndex - lineArr[this.middleLineIndex].dataIndex) / Math.abs(graphicIndex - this.middleLineIndex)
        lineArr.forEach((item, index) => {
          let dataIndex
          if (index < this.middleLineIndex) {
            dataIndex = Math.round(lineArr[this.middleLineIndex].dataIndex - this.diffIndexGap * (this.middleLineIndex - index))
          } else if (index > this.middleLineIndex) {
            dataIndex = Math.round(lineArr[this.middleLineIndex].dataIndex + this.diffIndexGap * (index - this.middleLineIndex))
          } else {
            dataIndex = lineArr[this.middleLineIndex].dataIndex
          }
          if (dataIndex < 0) {
            dataIndex = 0
          } else if (dataIndex > data.length - 1) {
            dataIndex = data.length - 1
          }
          changePositionData.push(data[Math.round(dataIndex)])
          lineArr[index].xAxis = data[Math.round(dataIndex)][0].toString()
          lineArr[index].value = data[Math.round(dataIndex)]
          lineArr[index].dataIndex = Math.round(dataIndex)
        })
      }
    }
    newGraphic = initGraphicPosition(this.gridIndex, changePositionData, options, this.prefixName, echart)
    this.YBxyList = changePositionData
    this.drawLine(lineArr, newGraphic, options, echart)
  }

  // 绘制
  /**
   * 
   * @param {*} lineArr 更新后的此组游标的位置信息 
   * @param {*} newGraphic 更新后的graphic的位置信息
   */
  drawLine(lineArr, newGraphic, options, echart) {
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
      graphic: newGraphic,
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

  translation(direction, lineId, echart) {
    let options = echart.getOption()
    let { data } = options.series.find(i => i.id == this.seriesId)
    let dataIndexArr = []
    let { lineArr } = judgeYbExist(options, this.prefixName, this.seriesId)
    if (lineId === this.prefixName + '_' + this.middleLineIndex) {
      if (direction == 'left') {
        if (lineArr[0].dataIndex == 0) return;
        dataIndexArr = lineArr.map(i => {
          return i.dataIndex - 1
        })
      } else {
        if (lineArr[lineArr.length - 1].dataIndex == data.length - 1) return;
        dataIndexArr = lineArr.map(i => {
          return i.dataIndex + 1
        })
      }
    } else {
      let graphicIndex = Number(lineId.split('_')[1])
      let currentIndex = lineArr[graphicIndex].dataIndex
      if (direction == 'left') {
        if (currentIndex == 0 || currentIndex == lineArr[this.middleLineIndex].dataIndex) return;
        this.diffIndexGap = Math.abs(currentIndex - 1 - lineArr[this.middleLineIndex].dataIndex) / Math.abs(graphicIndex - this.middleLineIndex)
      } else { // right
        if (currentIndex == data.length - 1 || currentIndex == lineArr[this.middleLineIndex].dataIndex) return;
        this.diffIndexGap = Math.abs(currentIndex + 1 - lineArr[this.middleLineIndex].dataIndex) / Math.abs(graphicIndex - this.middleLineIndex)
      }
      lineArr.forEach((item, index) => {
        let dataIndex
        if (index < this.middleLineIndex) {
          dataIndex = Math.round(lineArr[this.middleLineIndex].dataIndex - this.diffIndexGap * (this.middleLineIndex - index))
        } else if (index > this.middleLineIndex) {
          dataIndex = Math.round(lineArr[this.middleLineIndex].dataIndex + this.diffIndexGap * (index - this.middleLineIndex))
        } else {
          dataIndex = lineArr[this.middleLineIndex].dataIndex
        }
        if (dataIndex < 0) {
          dataIndex = 0
        } else if (dataIndex > data.length - 1) {
          dataIndex = data.length - 1
        }
        dataIndexArr.push(dataIndex)
      })
    }
    let pointer = dataIndexArr.map(i => {
      return data[i]
    })
    let newGraphic = initGraphicPosition(this.gridIndex, pointer, options, this.prefixName, echart)
    // markline 重置
    lineArr.forEach((item, index) => {
      item.xAxis = pointer[index][0].toString()
      item.value = pointer[index]
      item.dataIndex = dataIndexArr[index]
    })
    this.YBxyList = pointer
    this.drawLine(lineArr, newGraphic, options, echart)
  }

}