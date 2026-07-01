
import { judgeYbExist } from '../ybCommonTools'
let colorList = ['#FFAB40', '#1DBA92', '#40C4FF', '#D500F9', '#D50000', '#304FFE', '#7FA301'
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
  addYBLines(prefixName, echart, { data, seriesId, yAxisIndex, options }, getYBPointer) {
    this.seriesId = seriesId
    this.gridIndex = yAxisIndex
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
        id: prefixName + '_' + item,
        xAxis: pointer[0].toString(),
        value: pointer,
        gridIndex: this.gridIndex,
        dataIndex: Math.round((item + 1) * gapIndex),
        lineStyle: {
          color: this.lineColor,
          width: 2,
          type: [4, 1],
          dashOffset: 0,
        },
        dataType: item + 1,
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
      }
    })
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
    let { markLine, markPoint, name, color } = options.series.find(i => i.id == this.seriesId)
    let others = markLine.data.filter(i => i.name !== this.prefixName)
    let othersPoint = markPoint.data.filter(i => i.name !== this.prefixName)
    let points = Array.from(lineArr, i => ({
      name: this.prefixName, coord: i.value, itemStyle: {
        borderColor: this.lineColor,
      },
    }))
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
    this.getYBPointer([lineArr[0].value], this.lineColor, this.prefixName, this.seriesId, { name, color })
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
    let currentIndex = currentSeries.dataIndex
    let newData = data[currentIndex]
    let { lineArr } = judgeYbExist(options, this.prefixName, this.seriesId)
    lineArr[0].xAxis = newData[0].toString()
    lineArr[0].value = newData
    lineArr[0].dataIndex = currentIndex
    this.YBxyList = [newData]
    this.drawLine(lineArr, options, echart)
  }



  /**
 * 键盘左右键移动
 * @param {*} direction 移动的方向
 * @param {*} lineId 移动的目标线
 * @param {*} echart 实例
 * @returns 
 */
  translation(direction, id, echart) {
    let dataIndex
    let options = echart.getOption()
    let { data } = options.series.find(i => i.id == this.seriesId)
    let { lineArr } = judgeYbExist(options, this.prefixName, this.seriesId)
    if (direction == 'left') {
      //  if (lineArr[0].dataIndex == 0) return;
      dataIndex = lineArr[0].dataIndex - 1
    } else {
      // if (lineArr[0].dataIndex == data.length - 1) return;
      dataIndex = lineArr[0].dataIndex + 1
    }
    let unit = data[dataIndex]
    lineArr[0].xAxis = unit ? unit[0].toString() : ''
    lineArr[0].value = unit || []
    lineArr[0].dataIndex = dataIndex
    this.YBxyList = [unit]
    this.drawLine(lineArr, options, echart)
  }
}
