import { debounce } from '../tool.js'
import { closest, judgeYbExist, onBDLineMouseover, initGraphicPosition } from './ybCommonTools'
let colorList = ['#40C4FF',
  '#FFD740',
  '#00E676',
  '#651FFF',
  '#AA00FF',
  '#BF0025',
  '#FF6F00',
]
// 间隔游标
export class JianGeYB {
  constructor(lineNum) {
    // ...
    //this.ybBase = new YbBase();
    this.lineNum = lineNum || 5 // 一组游标线数量
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
    this.seriesId = seriesId
    this.gridIndex = yAxisIndex
    let { xAxis, yAxis } = options
    this.getYBPointer = getYBPointer
    this.prefixName = prefixName
    this.lineColor = colorList[prefixName.slice(11) % 7]//'beipinGroup0'
    /*  let { startValue, endValue } = options.dataZoom[0]
     let startPointer = closest(data, startValue)
     let endPointer = closest(data, endValue) */
    // 计算数据点间隔
    // let gapIndex = Math.floor((endPointer.index - startPointer.index) / 7)
    let gapIndex = Math.floor(data.length / (this.lineNum + 2))
    this.diffIndexGap = gapIndex
    let pointerList = []
    let marklineDatas = []
    for (let item = 0; item < this.lineNum; item++) {
      let pointer = data[/* startPointer.index +  */(item + 1) * gapIndex]
      pointerList.push(pointer)
      marklineDatas.push({
        name: prefixName,
        xAxis: pointer[0].toString(),
        value: pointer,
        gridIndex: this.gridIndex,
        dataIndex: /* startPointer.index +  */(item + 1) * gapIndex,
        lineStyle: {
          color: this.lineColor,
          width: 1,
          type: [5, 5, 1, 5],
          dashOffset: 0,
          opacity: 0.2
        },
        dataType: item,
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
    let graphicList = pointerList.map((item, index) => {
      let leftX = echart.convertToPixel({ gridIndex: this.gridIndex }, [item[0], 0])[0]
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
          lineDash: [5, 5, 1, 5],
          lineDashOffset: 0,
        },
        id: prefixName + '_' + index,
        info: this.seriesId,
        invisible: false,
        draggable: true,
        cursor: index == 0 ? 'move' : 'col-resize',
        ondragend: param => {
          this.onPointDragging([param.offsetX, param.offsetY], index, echart)
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

  // 游标拖拽方法
  /**
  * 
  * @param {*} pos 新的坐标位置
  * @param {*} graphicIndex 拖拽的游标index
  */
  onPointDragging(pos, graphicIndex, echart) {
    let options = echart.getOption()
    let data = options.series.find(i => i.id == this.seriesId).data
    let newData = echart.convertFromPixel({ gridIndex: this.gridIndex }, pos)
    const { value, index } = closest(data, newData[0])
    newData = value
    let currentIndex = index
    let newGraphic = []
    let { lineArr } = judgeYbExist(options, this.prefixName, this.seriesId)
    let changePositionData = []
    if (graphicIndex == 0) {
      // move
      let limitType = 'mid'
      // 判断两边是否顶到最大值或者最小值
      if (currentIndex < 0) {
        limitType = 'left'
      } else if (currentIndex + this.diffIndexGap * (this.lineNum - 1) > data.length - 1) {
        limitType = 'right'
      }
      lineArr.forEach((item, i) => {
        let unit
        let unitIndex
        if (limitType === 'left') {
          unitIndex = this.diffIndexGap * i
        } else if (limitType === 'right') {
          unitIndex = data.length - 1 - (this.lineNum - 1 - i) * this.diffIndexGap
        } else {
          if (i !== 0) {
            unitIndex = currentIndex + this.diffIndexGap * i
          } else {
            unitIndex = currentIndex
          }
        }
        if (unitIndex < 0) {
          unitIndex = 0
        } else if (unitIndex > data.length - 1) {
          unitIndex = data.length - 1
        }
        unit = data[Math.round(unitIndex)]
        changePositionData.push(unit)
        lineArr[i].xAxis = unit[0].toString()
        lineArr[i].value = unit
        lineArr[i].dataIndex = unitIndex
      })

    } else {
      // 不能移动越过标准线
      let baseLine = echart.convertToPixel({ gridIndex: this.gridIndex }, [lineArr[0].xAxis, 0])
      if (pos[0] - baseLine[0] < 0) {
        // 判断是否移到标准线左侧
        lineArr.forEach((item, index) => {
          changePositionData.push(lineArr[0].value)
          lineArr[index].xAxis = lineArr[0].xAxis
          lineArr[index].value = lineArr[0].value
          lineArr[index].dataIndex = lineArr[0].dataIndex
        })
      } else {
        this.diffIndexGap = Math.abs(currentIndex - lineArr[0].dataIndex) / graphicIndex
        lineArr.forEach((item, index) => {
          let unitIndex
          if (index !== 0) {
            unitIndex = Math.round(lineArr[0].dataIndex + this.diffIndexGap * index)
          } else {
            unitIndex = lineArr[0].dataIndex
          }
          if (unitIndex < 0) {
            unitIndex = 0
          } else if (unitIndex > data.length - 1) {
            unitIndex = data.length - 1
          }
          let unit = data[unitIndex]
          changePositionData.push(unit)
          index !== 0 ? (lineArr[index].xAxis = unit[0].toString()) : null
          index !== 0 ? (lineArr[index].value = unit) : null
          index !== 0 ? (lineArr[index].dataIndex = unitIndex) : null
        })
      }
    }
    newGraphic = initGraphicPosition(this.gridIndex, changePositionData, options, this.prefixName, echart)
    this.YBxyList = changePositionData
    this.drawLine(lineArr, newGraphic, options, echart)
  }

  // 游标平移
  translation(direction, lineId, echart) {
    let options = echart.getOption()
    let { data } = options.series.find(i => i.id == this.seriesId)
    let { lineArr } = judgeYbExist(options, this.prefixName, this.seriesId)
    let dataIndexArr = []
    if (lineId === this.prefixName + '_0') {
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
        if (currentIndex == 0 || currentIndex == lineArr[0].dataIndex) return;
        this.diffIndexGap = Math.abs(currentIndex - 1 - lineArr[0].dataIndex) / graphicIndex
      } else { // right
        if (currentIndex == data.length - 1 || currentIndex == lineArr[0].dataIndex) return;
        this.diffIndexGap = Math.abs(currentIndex + 1 - lineArr[0].dataIndex) / graphicIndex
      }
      lineArr.forEach((item, index) => {
        let dataIndex
        if (index == 0) {
          dataIndex = lineArr[0].dataIndex
        } else {
          dataIndex = Math.round(lineArr[0].dataIndex + this.diffIndexGap * index)
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
    lineArr.forEach((item, index) => {
      item.xAxis = pointer[index][0].toString()
      item.value = pointer[index]
      item.dataIndex = dataIndexArr[index]
    })
    this.YBxyList = pointer
    this.drawLine(lineArr, newGraphic, options, echart)
  }
}
