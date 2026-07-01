
import { debounce } from '../tool.js'
import { closest, judgeYbExist, onBDLineMouseover, initGraphicPosition } from './ybCommonTools'
let colorList = ['#00E676',
  '#40C4FF',
  '#AA00FF',
  '#651FFF',
  '#FFD740',
  '#FF6F00',
  '#D50000',
]
// 倍频游标
export class BeiPinYB {
  constructor(lineNum) {
    // ...
    //this.ybBase = new YbBase();
    this.lineNum = lineNum || 5
    this.diffIndexGap = 0 // 间隔
    // this.chart = null // 图表实例
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
    // this.chart = echart
    let { xAxis, yAxis } = options
    this.getYBPointer = getYBPointer
    this.prefixName = prefixName
    this.lineColor = colorList[prefixName.slice(11) % 7]//'beipinGroup0'
    // 计算数据点间隔
    let gapIndex = (data.length - 1) / (this.lineNum + 2)
    this.diffIndexGap = gapIndex
    let pointerList = []
    let marklineDatas = []
    for (let item = 0; item < this.lineNum; item++) {
      let pointer = data[Math.round((item + 1) * gapIndex)]
      pointerList.push(pointer)
      marklineDatas.push({
        name: prefixName,
        xAxis: pointer[0].toString(),// 转换为string类型，解决坐标点与makline不重合问题
        value: pointer,
        dataIndex: Math.round((item + 1) * gapIndex),
        gridIndex: this.gridIndex,
        lineStyle: {
          color: this.lineColor,//index == 0 ? '#00f7ff' : '#e2ff05',
          width: 1,
          type: 'dashed',
          opacity: 0.2
        },
        dataType: item + 1,
        label: {
          show: true,
          color: this.lineColor,
          opacity: 1,
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
        updateAnimation: {
          duration: 150,
          easing: 'linear'
        },
        // gridIndex: yAxisIndex,
        style: {
          stroke: this.lineColor,//index == 0 ? '#fff' : '#fff', // "rgba(0,0,0,0.1)",
          shadowColor: 'transparent',//,index == 0 ? '#00f7ff' : '#e2ff05',
          lineWidth: 1,
          lineDash: 'dashed',
        },
        id: prefixName + '_' + index,
        info: this.seriesId,
        invisible: false,
        draggable: true,
        cursor: 'col-resize',//index == 0 ? 'move' : 'e-resize',
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
    let newGraphic = []
    let { lineArr } = judgeYbExist(options, this.prefixName, this.seriesId)
    let changePositionData = []
    this.diffIndexGap = index / (graphicIndex + 1)
    lineArr.forEach((item, i) => {
      let unit = []
      let itemIndex = Math.round(this.diffIndexGap * (i + 1))
      if (itemIndex > data.length - 1) {
        itemIndex = data.length - 1
      } else if (itemIndex < 0) {
        itemIndex = 0
      }
      unit = data[itemIndex]
      lineArr[i].xAxis = unit[0].toString()
      lineArr[i].value = unit
      lineArr[i].dataIndex = itemIndex
      changePositionData.push(unit)
    })
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

  // 游标平移
  /**
   * 
   * @param {*} direction 键盘点击方向
   * @param {*} lineId 控制的游标线id
   */
  translation(direction, lineId, echart) {
    let options = echart.getOption()
    let { lineArr } = judgeYbExist(options, this.prefixName, this.seriesId)
    let { data } = options.series.find(i => i.id == this.seriesId)
    let changePositionData = []
    let graphicIndex = Number(lineId.split('_')[1])//beipinGroup0_1,取1
    let currentIndex = direction == 'left' ? lineArr[graphicIndex].dataIndex - 1 : lineArr[graphicIndex].dataIndex + 1
    this.diffIndexGap = currentIndex / (graphicIndex + 1)
    lineArr.forEach((item, i) => {
      let unit = []
      let itemIndex = Math.round(this.diffIndexGap * (i + 1))
      if (itemIndex > data.length - 1) {
        itemIndex = data.length - 1
      } else if (itemIndex < 0) {
        itemIndex = 0
      }
      unit = data[itemIndex]
      lineArr[i].xAxis = unit[0].toString()
      lineArr[i].value = unit
      lineArr[i].dataIndex = itemIndex
      changePositionData.push(unit)
    })
    let newGraphic = initGraphicPosition(this.gridIndex, changePositionData, options, this.prefixName, echart)
    this.YBxyList = changePositionData
    this.drawLine(lineArr, newGraphic, options, echart)
  }
}
