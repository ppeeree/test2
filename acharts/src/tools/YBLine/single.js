
import { debounce } from '../tool.js'
import { closest, judgeYbExist, onBDLineMouseover, initGraphicPosition } from './ybCommonTools'
let colorList = ['#EF6C00',
  '#FFAB40',
  '#00E5FF',
  '#6200EA',
  '#BF00B5',
  '#0026FF'
]
// 单游标
export class SingleYB {
  constructor() {
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
    /* let { startValue, endValue } = options.dataZoom[0]
    let startPointer = closest(data, startValue)
    let endPointer = closest(data, endValue) */
    // 计算数据点间隔
    // let gapIndex = Math.floor((endPointer.index - startPointer.index) / 2)
    let gapIndex = Math.floor(data.length / 3)
    let pointerList = []
    let marklineDatas = [0].map((item, index) => {
      let pointer = data[/* startPointer.index +  */(item + 1) * gapIndex]
      pointerList.push(pointer)
      return {
        name: prefixName,
        xAxis: pointer[0].toString(),
        value: pointer,
        gridIndex: this.gridIndex,
        dataIndex: Math.round((item + 1) * gapIndex),
        lineStyle: {
          color: this.lineColor,
          width: 1,
          type: 'solid',
          opacity: 0.2
        },
        dataType: item + 1,
        label: {
          show: false,
          opacity: 1,
          color: this.lineColor,
          formatter: (param) => {
            return param.data.dataType
          }
        },
      }
    })

    let graphicList = marklineDatas.map((item, index) => {
      let leftX = echart.convertToPixel({ gridIndex: this.gridIndex }, [item.xAxis, 0])[0]
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
          lineDash: 'solid',
        },
        id: prefixName + '_' + index,
        invisible: false,
        info: this.seriesId,
        draggable: true,
        cursor: 'move',
        ondragend: param => {
          this.onPointDragging(data, [param.offsetX, param.offsetY], echart)
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
    const { graphic, series } = options
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
    let { markLine, markPoint, name, color } = options.series.find(i => i.id == this.seriesId)
    let others = markLine.data.filter(i => i.name !== this.prefixName)
    let othersPoint = markPoint.data.filter(i => i.name !== this.prefixName)
    let points = Array.from(lineArr, i => ({
      name: this.prefixName, coord: i.value, itemStyle: {
        borderColor: this.lineColor,
      },
    }))
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
    this.getYBPointer([lineArr[0].value], this.lineColor, this.prefixName, this.seriesId, { name, color })
  }

  // 游标拖拽方法
  /**
   * 
   * @param {*} pos 新的坐标位置
   * @param {*} graphicIndex 拖拽的游标index
   */
  onPointDragging(data, pos, echart) {
    let options = echart.getOption()
    let newData = echart.convertFromPixel({ gridIndex: this.gridIndex }, pos)
    const { value, index } = closest(data, newData[0])
    let currentIndex = index
    if (currentIndex < 0) {
      currentIndex = 0
    } else if (currentIndex > data.length - 1) {
      currentIndex = data.length - 1
    }
    let { lineArr } = judgeYbExist(options, this.prefixName, this.seriesId)
    let unit = data[currentIndex]
    lineArr[0].xAxis = unit[0].toString()
    lineArr[0].value = unit
    lineArr[0].dataIndex = currentIndex
    let newGraphic = initGraphicPosition(this.gridIndex, [unit], options, this.prefixName, echart)
    this.YBxyList = [unit]
    this.drawLine(lineArr, newGraphic, options, echart)
  }



  // 游标平移
  /**
   * 
   * @param {*} direction 键盘点击方向
   * @param {*} lineId 控制的游标线id
   */
  translation(direction, id, echart) {
    let dataIndex
    let options = echart.getOption()
    let { data } = options.series.find(i => i.id == this.seriesId)
    let { lineArr } = judgeYbExist(options, this.prefixName, this.seriesId)
    if (direction == 'left') {
      if (lineArr[0].dataIndex == 0) return;
      dataIndex = lineArr[0].dataIndex - 1
    } else {
      if (lineArr[0].dataIndex == data.length - 1) return;
      dataIndex = lineArr[0].dataIndex + 1
    }
    let unit = data[dataIndex]
    lineArr[0].xAxis = unit[0].toString()
    lineArr[0].value = unit
    lineArr[0].dataIndex = dataIndex
    let newGraphic = initGraphicPosition(this.gridIndex, [unit], options, this.prefixName, echart)
    this.YBxyList = [unit]
    this.drawLine(lineArr, newGraphic, options, echart)
  }
}
