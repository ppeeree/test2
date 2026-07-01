import { debounce } from '../tool.js'
import { closest, judgeYbExist, onBDLineMouseover, initGraphicPosition } from './ybCommonTools'
let colorList = [
  '#BF0025',
  '#FF6F00',
  '#FFEA00',
  '#00C853',
  '#30FEFE',
  '#7400F9',
  '#FF00D4'
]

// 双游标
export class SYB {
  constructor() {
    this.diffIndexGap = 0 // 间隔
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
   * @param {*} graphicBaseConfig graphic基础配置
   * @param {*} getYBPointer 接收游标位置坐标的方法
   */
  addYBLines(prefixName, echart, { data, seriesId, yAxisIndex, options }, graphicBaseConfig, getYBPointer) {
    this.seriesId = seriesId
    this.gridIndex = yAxisIndex
    this.prefixName = prefixName
    this.getYBPointer = getYBPointer
    // let options = echart.getOption()
    let { xAxis, yAxis } = options
    this.lineColor = colorList[prefixName.slice(11) % 7]
    /* let { startValue, endValue, end } = options.dataZoom[0]
    let endPointer =
      end == 100
        ? {
          index: data.length - 1,
          value: data[data.length - 1]
        }
        : closest(data, endValue)
    let startPointer = closest(data, startValue)
    this.diffIndexGap = Math.round((endPointer.index - startPointer.index) / 4)
    let lefPointer = data[startPointer.index + this.diffIndexGap]
    let rightPointer = data[startPointer.index + this.diffIndexGap * 2] */
    this.diffIndexGap = Math.round(data.length / 4)
    let lefPointer = data[this.diffIndexGap]
    let rightPointer = data[this.diffIndexGap * 2]
    let markLineData = [
      {
        name: prefixName,
        gridIndex: this.gridIndex,
        xAxis: lefPointer[0].toString(),
        value: lefPointer,
        dataIndex: /* startPointer.index + */ this.diffIndexGap,
        lineStyle: {
          color: this.lineColor,//'#00f7ff',
          width: 1,
          type: [5, 2],
          dashOffset: 2,
          opacity: 0.2
        }
      },
      {
        name: prefixName,
        gridIndex: this.gridIndex,
        xAxis: rightPointer[0].toString(),
        value: rightPointer,
        dataIndex: /* startPointer.index + */ this.diffIndexGap * 2,
        lineStyle: {
          color: this.lineColor,//'#e2ff05',
          width: 1,
          type: [5, 2],
          dashOffset: 2,
          opacity: 0.2
        },
      }
    ]
    let graphicsList = markLineData.map((item, index) => {
      let leftX = echart.convertToPixel({ gridIndex: this.gridIndex }, [item.xAxis, 0])[0]
      return {
        ...graphicBaseConfig,
        type: 'line',
        left: leftX - 2,
        $action: "replace",
        bounding: "all",
        invisible: false,
        height: 50,
        draggable: true,
        style: {
          stroke: this.lineColor,//index == 0 ? '#fff' : '#fff', // "rgba(0,0,0,0.1)",
          shadowColor: 'transparent',//,index == 0 ? '#00f7ff' : '#e2ff05',
          lineWidth: 1,
          lineDash: [5, 2],
          lineDashOffset: 2,
        },
        id: prefixName + '_' + index,
        info: this.seriesId,
        cursor: index == 0 ? 'move' : 'col-resize',
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
    //  }
    // echart.clear()
    const { graphic } = options
    let graphicData = graphicsList
    if (graphic && graphic.length) {
      graphicData = [...graphic[0].elements, ...graphicsList]
    }
    this.YBxyList = [lefPointer, rightPointer]
    this.drawLine(markLineData, graphicData, options, echart)
  }

  // 游标拖拽方法
  /**
   * 
   * @param {*} pos 拖拽后的位置信息
   * @param {*} graphicIndex 拖拽的对象索引
   */
  onPointDragging(data, pos, graphicIndex, echart) {
    let options = echart.getOption()
    let newData = echart.convertFromPixel({ gridIndex: this.gridIndex }, pos)
    const { value, index } = closest(data, newData[0])
    newData = value
    let currentIndex = index
    let newGraphic = []
    let { lineArr } = judgeYbExist(options, this.prefixName, this.seriesId)
    if (graphicIndex == 0) {
      // move
      // 顶到右侧边界，不能再拖动, 重置move line位置
      let dataIndex1 = currentIndex
      let dataIndex2
      if (currentIndex + this.diffIndexGap > data.length) {
        dataIndex1 = data.length - 1 - this.diffIndexGap
        dataIndex2 = data.length - 1
      } else {
        dataIndex2 = currentIndex + this.diffIndexGap
      }
      let newpointer1 = data[dataIndex1]
      let newpointer2 = data[dataIndex2]
      // end
      // markline 重置
      lineArr[0] = {
        ...lineArr[0],
        xAxis: newpointer1[0].toString(),
        value: newpointer1,
        dataIndex: dataIndex1
      }
      lineArr[1] = {
        ...lineArr[1],
        xAxis: newpointer2[0].toString(),
        value: newpointer2,
        dataIndex: dataIndex2
      }
    } else {
      // 不能移到标准线的左侧
      if (currentIndex < lineArr[0].dataIndex) {
        newData = lineArr[0].value
        currentIndex = lineArr[0].dataIndex
      }
      // 扩大间距
      this.diffIndexGap = currentIndex - lineArr[0].dataIndex
      lineArr[1].xAxis = newData[0].toString()
      lineArr[1].value = newData
      lineArr[1].dataIndex = currentIndex
    }
    newGraphic = initGraphicPosition(this.gridIndex, [lineArr[0].value, lineArr[1].value], options, this.prefixName, echart)
    this.YBxyList = [lineArr[0].value, lineArr[1].value]
    this.drawLine(lineArr, newGraphic, options, echart)
  }

  // 绘制
  /**
   * 
   * @param {*} lineArr 更新后的此组游标的位置信息 
   * @param {*} newGraphic 更新后的graphic的位置信息
   */
  drawLine(lineArr, newGraphic, options, echart) {
    let { markLine, markPoint, name, color } = options.series.find(i => i.id == this.seriesId)
    let others = markLine.data.filter(i => i.name !== this.prefixName)
    let othersPoint = markPoint.data.filter(i => i.name !== this.prefixName)
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
   * 键盘事件游标移动重绘
   * @param {*} direction 移动方向
   * @param {*} lineId 鼠标选中的游标线索引
   * @returns 
   */
  translation(direction, lineId, echart) {
    let options = echart.getOption()
    let { data } = options.series.find(i => i.id == this.seriesId)
    let newpointer1 = []
    let newpointer2 = []
    let dataIndex1, dataIndex2
    let { lineArr } = judgeYbExist(options, this.prefixName, this.seriesId)
    if (lineId === this.prefixName + '_0') {
      if (direction == 'left') {
        if (lineArr[0].dataIndex == 0) return;
        dataIndex1 = lineArr[0].dataIndex - 1
        dataIndex2 = lineArr[1].dataIndex - 1
      } else {
        if (lineArr[0].dataIndex == data.length - 1) return;
        dataIndex1 = lineArr[0].dataIndex + 1
        dataIndex2 = lineArr[1].dataIndex + 1
      }
    } else {
      if (direction == 'left') {
        if (lineArr[0].dataIndex == lineArr[1].dataIndex) return;
        dataIndex1 = lineArr[0].dataIndex
        dataIndex2 = lineArr[1].dataIndex - 1
      } else {
        if (lineArr[1].dataIndex == data.length - 1) return;
        dataIndex1 = lineArr[0].dataIndex
        dataIndex2 = lineArr[1].dataIndex + 1
      }
    }
    newpointer1 = data[dataIndex1]
    newpointer2 = data[dataIndex2]
    let newGraphic = initGraphicPosition(this.gridIndex, [newpointer1, newpointer2], options, this.prefixName, echart)
    // markline 重置
    lineArr[0] = {
      ...lineArr[0],
      xAxis: newpointer1[0].toString(),
      value: newpointer1,
      dataIndex: dataIndex1
    }
    lineArr[1] = {
      ...lineArr[1],
      xAxis: newpointer2[0].toString(),
      value: newpointer2,
      dataIndex: dataIndex2
    }
    this.YBxyList = [lineArr[0].value, lineArr[1].value]
    this.drawLine(lineArr, newGraphic, options, echart)
  }
}