// import 'echarts-gl'
import {
  colorSeires,
  accMul,
  imageBase64Data,
  echartsYaxisLabelFormatter
} from '../../tools/tool.js'
import * as echarts from 'echarts'
import { Toolbox } from '../../tools/command/toolbox.js'
import { getOptions } from '../../commonJs/optionConfig.js'

// 此组件为瀑布图
// 输入为echart对象，输出为工具的操作，及数据的加载
export class WaterFallChart {
  constructor(echart, echartData, { theme, isAddNote, isDataDown } = {}) {
    // ...
    this.myChart = echart // echart 实例对象
    this.zoom = {
      start: 0,
      end: 100
    }
    this.themeType = theme || 'light' // 'dark'
    this.legendData = [] // lengend 数据
    this.echartData = echartData // 图表数据源
    this.selectedLegend = [] // 高亮选中的legend
    this.markindex = 0 // 备注增加，索引标记
    this.rightClickedNoteId = ''// 右键菜单选中的备注Id
    this.dataZoomType = ''
    this.dataZoomDataList = []
    this.noteDom = null
    this.menuDom = null // 右键菜单dom
    this.Toolbox = new Toolbox(this.myChart, this.themeType, {
      isYB: false,
      isSetX: false,
      isSetY: false,
      isStacked: false,
      isZoom: false,
      isAddNote: typeof (isAddNote) == 'undefined' ? true : isAddNote,
      isDataDown: typeof (isDataDown) == 'undefined' ? true : isDataDown,
    }, this.hotUpdate.bind(this))
    this.initData(echartData)
  }
  // 返回图谱的base64数据格式
  imageBase64Data(isExport) {
    return imageBase64Data(this.myChart, this.selectedLegend, isExport)
  }
  // 工具条组件中调用或者修改实例属性
  hotUpdate(data) {
    if (data) {
      for (let key in data) {
        this[key] = data[key]
      }
    } else {
      return {
        selectedLegend: this.selectedLegend,
        dataZoomDataList: this.dataZoomDataList,
        echartData: this.echartData
      }
    }
  }
  /**
   * 初始化绘制图函数
   * @param {*} lineDataSourse 数据源
   */
  initData(lineDataSourse) {
    this.echartData = lineDataSourse || this.echartData
    const { dimensions, xAxisType, zAxisType, yAxisType, titleText, data } = this.echartData
    let legendData = []
    data.forEach(i => {
      legendData.push(i.name)
    })
    this.legendData = legendData
    this.selectedLegend = legendData
    this.echartData.legendData = legendData
    let legendItemWidth = Math.round((this.myChart.getWidth() - 60) / legendData.length)
    const { gridStyle, titleStyle, backgroundColor, color, legendStyle, xAxisStyle, yAxisStyle } = getOptions(this.themeType)
    let chartData = {
      animation: false,           // 关键
      animationDuration: 0,
      animationDurationUpdate: 0,
      aria: {
        enabled: false,
        label: {
          enabled: false,
        }
      },
      backgroundColor,
      color: colorSeires,
      title: {
        ...titleStyle,
        text: titleText || '瀑布图',
      },
      tooltip: {
        hideDelay: 500,
        extraCssText: 'z-index:999'
      },
      xAxis3D: {
        name: dimensions[0],
        nameGap: 40,
        type: xAxisType || 'value',
        // data: XData,
        ...xAxisStyle
      },
      yAxis3D: {
        type: yAxisType || 'category',
        name: dimensions[1],
        nameGap: 40,
        data: this.legendData,
        ...yAxisStyle,
        axisLabel: {
          show: true,
          interval: 0,   //使y轴都显示
          formatter: function (name, index) {
            return echarts.format.truncateText(name, 200, '12px', '…');
          },
          textStyle: {
            fontSize: 12,
            color: this.themeType == 'light' ? '#000' : '#fff'
          },
          tooltip: {
            show: true
          },
        },
      },
      zAxis3D: {
        type: zAxisType || 'value',
        nameGap: 40,
        // name: data.Unit
        // 这里用到传值的Unit
        name: dimensions[2],
        nameLocation: 'center',
        ...yAxisStyle,
        axisLabel: {
          ...yAxisStyle.axisLabel,
          formatter: (val) => {
            return echartsYaxisLabelFormatter(val)
          }
        },
        // max: zMaxValue
      },
      grid3D: {
        ...gridStyle,
        boxWidth: 300,
        boxHeight: 100,
        boxDepth: 150,
        left: 0,
        right: 0,
        width: '100%',
        height: '90%',
        axisLine: {
          lineStyle: {
            color: '#ccc'
          }
        },
        axisPointer: {
          show: true,
          lineStyle: {
            color: this.themeType == 'light' ? '#000' : '#fff',
            opacity: 0.2,
            width: 1
          }
        },
        splitLine: {
          interval: {
            opacity: 0.5,
            width: 0.5
          }
        },
        light: {
          main: {
            intensity: 1.2
          },
          ambient: {
            intensity: 0.3
          }
        },
        viewControl: {
          alpha: 20,
          beta: 5,
          distance: 300,
          center: [0, 0, 0]
        }
      },
      legend: {
        ...legendStyle,
        triggerEvent: true,
        formatter: function (name) {
          return echarts.format.truncateText(name, legendItemWidth - 10, '14px Microsoft Yahei', '…');
        },
        tooltip: {
          show: true,
          // 将 tooltip 限制在图表区域内
          confine: true,
          // 自定义位置
          position: function (point, params, dom, rect, size) {
            // size.viewSize 是图表区域大小
            // 确保 tooltip 不会超出边界
            return [Math.min(point[0], size.viewSize[0] - size.contentSize[0] - 10),
            Math.min(point[1], size.viewSize[1] - size.contentSize[1] - 20)];
          }
        },
        itemHeight: 12,
        icon: "circle",//显示样式
        bottom: 0,
        left: "center",
        data: legendData || []
      },
      series: this.getSeries(data)
    }
    this.myChart.setOption(chartData)
    this.initOperate()
  }
  updateData() {
    this.echartData = lineDataSourse || this.echartData
    const { dimensions, xAxisType, zAxisType, yAxisType, titleText, data } = this.echartData
    let legendData = []
    data.forEach(i => {
      legendData.push(i.name)
    })
    this.legendData = legendData
    this.selectedLegend = legendData
    this.echartData.legendData = legendData
    let legendItemWidth = Math.round((this.myChart.getWidth() - 60) / legendData.length)
    const { gridStyle, titleStyle, backgroundColor, color, legendStyle, xAxisStyle, yAxisStyle } = getOptions(this.themeType)
    let originOptions = this.myChart.getOption()
    let chartData = {
      ...originOptions,
      xAxis3D: {
        name: dimensions[0],
        nameGap: 40,
        type: xAxisType || 'value',
        // data: XData,
        ...xAxisStyle
      },
      yAxis3D: {
        type: yAxisType || 'category',
        name: dimensions[1],
        nameGap: 40,
        data: this.legendData,
        ...yAxisStyle,
        axisLabel: {
          show: true,
          interval: 0,   //使y轴都显示
          formatter: function (name, index) {
            return echarts.format.truncateText(name, 200, '12px', '…');
          },
          textStyle: {
            fontSize: 12,
            color: this.themeType == 'light' ? '#000' : '#fff',
          },
          tooltip: {
            show: true
          },
        },
      },
      zAxis3D: {
        type: zAxisType || 'value',
        nameGap: 40,
        // name: data.Unit
        // 这里用到传值的Unit
        name: dimensions[2],
        nameLocation: 'center',
        ...yAxisStyle,
        axisLabel: {
          ...yAxisStyle.axisLabel,
          formatter: (val) => {
            return echartsYaxisLabelFormatter(val)
          }
        },
      },
      legend: {
        ...legendStyle,
        triggerEvent: true,
        formatter: function (name) {
          return echarts.format.truncateText(name, legendItemWidth - 10, '14px Microsoft Yahei', '…');
        },
        tooltip: {
          show: true,
          // 将 tooltip 限制在图表区域内
          confine: true,
          // 自定义位置
          position: function (point, params, dom, rect, size) {
            // size.viewSize 是图表区域大小
            // 确保 tooltip 不会超出边界
            return [Math.min(point[0], size.viewSize[0] - size.contentSize[0] - 10),
            Math.min(point[1], size.viewSize[1] - size.contentSize[1] - 20)];
          }
        },
        itemHeight: 12,
        icon: "circle",//显示样式
        bottom: 0,
        left: "center",
        data: legendData || []
      },
      series: this.getSeries(data)
    }
    this.myChart.setOption(chartData)
  }
  // 组装series
  getSeries(arr) {
    let seriesData = []
    arr.length &&
      arr.forEach((item, index) => {
        const { name, id, source } = item
        let newData = source.map(item => {
          return {
            value: [item[0], index, item[1]]
          }
        })
        seriesData.push({
          name: name,
          id: id,
          type: 'line3D',
          data: newData,
          label: {
            show: false,
            textStyle: {
              fontSize: 16,
              borderWidth: 1
            }
          },
          itemStyle: {
            opacity: 0.4
          },
          tooltip: {
            extraCssText: 'z-index:999',
            formatter: function (param) {
              // console.log(param)
              return param.marker + 'x：' + param.value[0] + '&nbsp;&nbsp; z：' + param.value[2]
              // 使用forEachIndex作为forEach循环中的index
              // return getWaterfallToolTipData(e.data.value[0], forEachIndex)
            }
          },
          emphasis: {
            label: {
              textStyle: {
                fontSize: 20,
                color: '#288ce2'
              }
            },
            itemStyle: {
              color: '#ccc'
            }
          },
          itemStyle: {
            color: colorSeires[index % colorSeires.length] // colorArrList[i][index % colorArrList[i].length]
          },
          lineStyle: {
            width: 1
          },
        })
      })
    return seriesData
  }

  // 初始化后echarts的监听事件:放大事件，双击事件，图例改变事件，点击事件，右键菜单事件
  initOperate() {
    this.myChart.off('datazoom')
    this.myChart.on('datazoom', param => {
      // // console.log(param)
      setTimeout(() => {
        let start, end
        if (param['batch']) {
          start = param.batch[0].start
          end = param.batch[0].end
        } else {
          start = param.start
          end = param.end
        }
        this.zoom = { start, end }
        let options = this.myChart.getOption()
        const { dataZoom } = options
        if (param['batch']) {
          if ('startValue' in param.batch[0]) {
            this.dataZoomDataList = [...this.dataZoomDataList, dataZoom]
            this.Toolbox.updateBackDomStyle(this.dataZoomDataList, this.themeType)
          }
        }
      }, 300)
    })
    this.myChart.getZr().off('dblclick')
    this.myChart.getZr().on('dblclick', () => {
      this.zoom = { start: 0, end: 100 }
      let options = this.myChart.getOption()
      let { dataZoom } = options
      let isRevert = false
      options.dataZoom.forEach(i => {
        if (i.start !== 0 || i.end !== 100) {
          return isRevert = true
        }
      })
      if (isRevert) {
        dataZoom = options.dataZoom.map(item => {
          return { ...item, start: 0, end: 100 }
        })
        this.myChart.setOption({
          dataZoom: dataZoom
        })
      }
      // 工具栏放大数据存储清空
      this.dataZoomDataList = []
      this.Toolbox.updateBackDomStyle(this.dataZoomDataList, this.themeType)
    })

    this.myChart.on('legendselectchanged', params => {
      const { selected } = params
      this.selectedLegend = []
      Object.keys(selected).forEach(i => {
        if (selected[i]) {
          this.selectedLegend.push(i)
        }
      })
    })
    this.myChart.off('click')
    let that = this
    this.myChart.on('click', function (param, ev) {
      const { ctrlKey } = param.event.event
      // ctrl+点击legend 只选中当前项，其他全不选
      if (param.componentType == 'legend' && ctrlKey) {
        that.legendData.forEach((item) => {
          if (item == param.value) {
            that.myChart.dispatchAction({
              type: 'legendSelect',
              name: item,
            })
          } else {
            that.myChart.dispatchAction({
              type: 'legendUnSelect',
              name: item,
            })
          }
        })
      }
    })
    /* this.myChart.getZr().off('contextmenu')
    this.myChart.getZr().on('contextmenu', (param) => {
      this.Toolbox.setStyleMenuContent(param)
    }) */
  }


  // 图例选中事件
  legendChangeEvent(legendNameArr) {
    this.selectedLegend = legendNameArr
    this.legendData.forEach(item => {
      this.myChart.dispatchAction({ type: 'legendUnSelect', name: item })
    })
    setTimeout(() => {
      if (legendNameArr.length) {
        legendNameArr.forEach(i => {
          this.myChart.dispatchAction({
            type: 'legendSelect',
            name: `${i.windParkName}_${i.windturbineName}_${i.time}`
          })
        })
      }
    }, 500)
  }


  // 取消监听事件，属性初始化
  destroyedInstance() {
    this.myChart.getDom().onkeydown = null
    this.myChart.off('datazoom')
    this.myChart.getZr().off('click')
    this.myChart.getZr().off('dblclick')
  }

  // 自定义图谱的大小尺寸响应事件
  resize() {
    this.myChart.resize()
    // 游标线位置重新计算定位
    let options = this.myChart.getOption()
    let chartWidth = this.myChart.getWidth()
    let legendItemWidth = Math.floor((this.myChart.getWidth() - 60) / this.legendData.length)
    this.myChart.setOption({
      legend: {
        triggerEvent: true,
        formatter: function (name) {
          return echarts.format.truncateText(name, legendItemWidth - 10, '14px Microsoft Yahei', '…');
        },
        tooltip: {
          show: true,
          // 将 tooltip 限制在图表区域内
          confine: true,
          // 自定义位置
          position: function (point, params, dom, rect, size) {
            // size.viewSize 是图表区域大小
            // 确保 tooltip 不会超出边界
            return [Math.min(point[0], size.viewSize[0] - size.contentSize[0] - 10),
            Math.min(point[1], size.viewSize[1] - size.contentSize[1] - 20)];
          }
        },
        icon: "circle",//显示样式
        bottom: 0,
        left: "center",
        width: chartWidth,
        data: this.legendData || []
      },
      graphic: []//graphicList
    })
    // 工具条位置重新计算
    let dvBlock = document.getElementById(this.myChart.id + 'toolbox')
    dvBlock ? dvBlock.style.left = (chartWidth - dvBlock.getBoundingClientRect().width) / 2 + 'px' : null
  }

}




