/* eslint-disable no-empty */
import * as echarts from 'echarts'
import {
  colorSeires,
  imageBase64Data
} from '../../tools/tool.js'
import { Toolbox } from '../../tools/command/toolbox.js'
import { getOptions } from '../../commonJs/optionConfig.js'
let chartPaddingTop = 40
let chartPaddingBottom = 70
let chartGriBwn = 15
// 此组件为相关性分析（散点分布图）
// 输入为echart对象，输出为工具的操作，及数据的加载
export class ScatterChart {
  constructor(echart, dataSource, { theme, isStacked, isSetY, isAddNote, isDataDown } = {}) {
    this.myChart = echart
    this.echartData = dataSource
    this.themeType = theme || 'light' // 'dark'
    this.zoom = {
      start: 0,
      end: 100
    }
    this.legendData = []
    this.selectedLegend = []
    this.dataZoomDataList = []
    this.stackedType = 'overlay'
    this.Toolbox = new Toolbox(this.myChart,
      this.themeType,
      {
        isStacked: typeof (isStacked) == 'undefined' ? true : isStacked,
        isSetY: typeof (isSetY) == 'undefined' ? true : isSetY,
        isAddNote: typeof (isAddNote) == 'undefined' ? true : isAddNote,
        isDataDown: typeof (isDataDown) == 'undefined' ? true : isDataDown,
      },
      this.hotUpdate.bind(this),
      this.changeStackedType.bind(this)
    )
    this.initData(dataSource)
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
  // 返回图谱的base64数据格式
  imageBase64Data(isExport) {
    return imageBase64Data(this.myChart, this.selectedLegend, isExport)
  }
  // 监听事件
  addEvent() {
    this.myChart.on('dataZoom', param => {
      const { dataZoom } = this.myChart.getOption()
      if (param['batch']) {
        if ('startValue' in param.batch[0]) {
          this.dataZoomDataList = [...this.dataZoomDataList, dataZoom]
          this.Toolbox.updateBackDomStyle(this.dataZoomDataList)
        }
      }
    })
    this.myChart.getZr().on('dblclick', () => {
      let options = this.myChart.getOption()
      let dataZoom = []
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
        // 工具栏放大数据存储清空
        this.dataZoomDataList = []
        this.Toolbox.updateBackDomStyle(this.dataZoomDataList)
      } else {
        return
      }
    })
    this.myChart.on('legendselectchanged', params => {
      const { selected } = params
      this.selectedLegend = []
      Object.keys(selected).forEach(i => {
        if (selected[i]) {
          this.selectedLegend.push(i)
        }
      })
      let relationOp = this.changeStackedType(this.stackedType, this.themeType)
      let option = this.myChart.getOption()
      this.myChart.setOption({
        ...option,
        ...relationOp
      }, { lazyUpdate: true })
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
  }

  // 初始化
  initData(dataSourse) {
    this.echartData = dataSourse || this.echartData
    if (this.echartData.data && this.echartData.data.length) {
      const { xType, yType, titleText, data } = this.echartData
      const { gridStyle, backgroundColor, titleStyle, legendStyle, color, toolboxStyle, toolboxFeature, xAxisStyle, yAxisStyle } = getOptions(this.themeType)
      let legendData = data.map(i => { return i.name })
      this.selectedLegend = legendData
      this.legendData = legendData
      this.echartData.legendData = legendData
      let yAxisName = []
      data.forEach(item => {
        let { dimensions } = item
        yAxisName.push(dimensions[1])
      })
      yAxisName = [...new Set(yAxisName)] // 去重
      let relationOp = this.changeStackedType()
      let legendItemWidth = Math.round((this.myChart.getWidth() - 60) / legendData.length)
      let options = {
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
        color,
        title: {
          ...titleStyle,
          text: titleText
        },
        toolbox: {
          ...toolboxStyle,
          id: 'toolbox',
          showTitle: false,
          right: 50,
          feature: {
            // ...toolboxFeature,
            dataZoom: {
              show: true,
              title: {
                zoom: "",
                back: ''
              },
              iconStyle: {
                opacity: 0
              },
              xAxisIndex: false,
              yAxisIndex: false
            },
            restore: {
              show: false
            }
          },
        },
        tooltip: {
          confine: true,
          trigger: 'axis',
          textStyle: {
            fontSize: 12,
            color: '#1A1A1A'
          },
          axisPointer: {
            type: 'line',
            // snap: true,
          },
          formatter: function (params) {
            let str = ''
            if (params.length) {
              str += `<p style='text-align:center;margin-bottom:5px;border-bottom:1px solid #ccc'>${params[0].axisValueLabel}</p>`
              params.forEach(item => {
                str +=
                  `${item.marker}: ${item.value[1]}&nbsp;&nbsp;<br/>`
              })
            }
            return str
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
          icon: "circle",//显示样式
          bottom: 0,
          left: "center",
          width: "90%",
          data: legendData || []
        },
        ...relationOp,
        dataZoom: [
          {
            type: 'inside',
            id: 'Xinside',
            xAxisIndex: [0, 1, 2, 3, 4]
          },
          /*  {
             id: 'Xslider',
             type: 'slider',
             xAxisIndex: [0],
             // filterMode: 'empty',
             start: this.zoom.start,
             end: this.zoom.end,
             height: 5,
             bottom: 30,
             orient: "horizontal",
           }, */
          /* {
                type: 'inside',
               id: 'Yinside',
               start: this.zoom.start,
               end: this.zoom.end,
               yAxisIndex: [0], 
           },*/
          /*   {
              type: 'slider',
              id: 'Yslider',
              start: this.zoom.start,
              end: this.zoom.end,
              orient: "vertical",
              width: 5,
              right: 15,
              // height: '70%'
              top: 50,
              bottom: 70,
              yAxisIndex: [0],
              // filterMode: 'empty'
            } */
        ],
        series: this.getSeries(data, 'overlay', yAxisName)
      }
      this.myChart.setOption(options)
      this.addEvent()
    } else { // 无数据
      return
    }
  }
  updateData(dataSourse) {
    this.echartData = dataSourse || this.echartData
    if (this.echartData.data && this.echartData.data.length) {
      const { data } = this.echartData
      let legendData = data.map(i => { return i.name })
      this.selectedLegend = legendData
      this.legendData = legendData
      this.echartData.legendData = legendData
      let yAxisName = []
      data.forEach(item => {
        let { dimensions } = item
        yAxisName.push(dimensions[1])
      })
      yAxisName = [...new Set(yAxisName)] // 去重
      let relationOp = this.changeStackedType()
      let legendItemWidth = Math.round((this.myChart.getWidth() - 60) / legendData.length)
      let originOptions = this.myChart.getOption()
      let options = {
        ...originOptions,
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
          width: "90%",
          data: legendData || []
        },
        ...relationOp,
        series: this.getSeries(data, 'overlay', yAxisName)
      }
      this.myChart.setOption(options)
    }
  }
  // 获取series数据（进行数据点symbol转换）
  getSeries(data, stackedType, yAxisName) {
    let arr = []
    if (data.length) {
      data.forEach((element, index) => {
        const { id, name, dimensions, source } = element
        arr.push({
          name: name,
          id: id,
          xAxisIndex: stackedType == 'overlay' ? 0 : (stackedType == 'stacked' ? index : yAxisName.indexOf(dimensions[1])),
          yAxisIndex: stackedType == 'overlay' ? 0 : (stackedType == 'stacked' ? index : yAxisName.indexOf(dimensions[1])),
          encode: { x: 0, y: 1, tooltip: [0, 1, 2] },
          type: 'scatter',
          lineStyle: {
            width: 1
          },
          color: colorSeires[index % colorSeires.length],
          data: source,
        })
      })
    }
    return arr
  }

  // 图谱堆叠方式修改
  changeStackedType(type, theme) {
    let stackedType = type || 'overlay'
    this.stackedType = stackedType
    const { xType, yType, data } = this.echartData
    let relationOp = {
      axisPointer: {
        link: { xAxisIndex: "all" },
      },
      dataZoom: [],
      grid: [],
      xAxis: [],
      yAxis: [],
      series: []
    }
    const { gridStyle, backgroundColor, titleStyle, legendStyle, color, toolboxStyle, toolboxFeature, xAxisStyle, yAxisStyle } = getOptions(theme || this.themeType)
    let xAxisBase = {
      ...xAxisStyle,
      type: xType || 'value',//dataSoures.xAxisType || 'time',//category
      nameLocation: 'center',
      boundaryGap: '0%',
      hideOverlap: true,
      splitNumber: 3,
      nameGap: 30,
      axisLabel: {
        ...xAxisStyle.axisLabel,
        width: 100,
        overflow: 'break',
        padding: [0, 5, 0, 5],
      },
    };
    let yAxisBase = {
      type: yType || 'value',
      alignTicks: true,
      scale: true,
      nameLocation: 'center',
      nameGap: 45,
      nameRotate: 90,
      ...yAxisStyle,
    }
    let gridBase = {
      ...gridStyle,
      left: 60,
      right: 30,
    }
    let echartHeight = this.myChart.getHeight()
    let yAxisName = []
    this.selectedLegend.forEach((item, index) => {
      let { dimensions } = data.find(i => i.name == item)
      yAxisName.push(dimensions[1])
    })
    yAxisName = [...new Set(yAxisName)] // 去重
    if (stackedType == 'overlay') {
      relationOp.yAxis = [{
        ...yAxisBase,
        name: yAxisName.length == 1 ? yAxisName[0] : '', // Y轴名称，每组数据的维度名称相同则显示，不同则不显示
        gridIndex: 0,
      }]
      relationOp.xAxis = [{
        ...xAxisBase,
        name: data[0].dimensions[0],
        gridIndex: 0
      }]
      relationOp.grid = [{
        ...gridBase,
        top: chartPaddingTop,
        bottom: chartPaddingBottom,
        height: echartHeight - (chartPaddingTop + chartPaddingBottom)
      }]
    } else if (stackedType == 'stacked') {
      let unitHeight = (echartHeight - chartPaddingTop - chartPaddingBottom - (this.selectedLegend.length - 1) * chartGriBwn) / this.selectedLegend.length
      this.selectedLegend.forEach((item, index) => {
        let { dimensions } = data.find(i => i.name == item)
        relationOp.grid.push({
          ...gridBase,
          top: index * unitHeight + (index * chartGriBwn) + chartPaddingTop,
          height: unitHeight
        })
        relationOp.xAxis.push({
          ...xAxisBase,
          axisLabel: {
            show: index == this.selectedLegend.length - 1,
            ...xAxisBase.axisLabel
          },
          gridIndex: index,
          name: index == this.selectedLegend.length - 1 ? dimensions[0] : '',
        })
        relationOp.yAxis.push({
          ...yAxisBase,
          name: dimensions[1],
          gridIndex: index
        })
      })
    } else if (stackedType == 'group') {
      let unitHeight = (echartHeight - chartPaddingTop - chartPaddingBottom - (yAxisName.length - 1) * chartGriBwn) / yAxisName.length
      yAxisName.forEach((item, index) => {
        relationOp.grid.push({
          ...gridBase,
          top: index * unitHeight + (index * chartGriBwn) + chartPaddingTop,
          height: unitHeight
        })
        relationOp.xAxis.push({
          ...xAxisBase,
          axisLabel: {
            show: index == yAxisName.length - 1,
            ...xAxisBase.axisLabel
          },
          name: index == yAxisName.length - 1 ? data[0].dimensions[0] : '',
          gridIndex: index
        })
        relationOp.yAxis.push({
          ...yAxisBase,
          name: item,
          gridIndex: index
        })
      })
    }
    let newSeries = []
    let newDataZoom = []
    let options = this.myChart.getOption()
    if (options) {
      const { series, dataZoom } = options
      series.forEach((item, index) => {
        if (item.name !== 'markline_level') {
          const obj = data.find(i => i.id == item.id)
          if (obj) {
            let axisIndex = 0
            if (stackedType == 'stacked') {
              // let name = item.name.replace('wave_marker', '')
              axisIndex = this.selectedLegend.findIndex(i => i == item.name)
            } else if (stackedType == 'group') {
              axisIndex = yAxisName.findIndex(i => i == obj.dimensions[1])
            }
            if (axisIndex == -1) { axisIndex = 0 }
            newSeries.push({
              ...item,
              xAxisIndex: axisIndex,
              yAxisIndex: axisIndex,
            })
          }
        }/*  else {
          newSeries.push({
            ...item
          })
        } */
      })
      // dataZoom 组件控制的坐标轴指定
      dataZoom.forEach((item, index) => {
        if (item.id.includes('X')) {
          newDataZoom.push({
            ...item,
            xAxisIndex: [...Array(relationOp.xAxis.length).keys()]
          })
        } else {
          newDataZoom.push({
            ...item,
            yAxisIndex: [...Array(relationOp.yAxis.length).keys()]
          })
        }
      })
    }
    relationOp.series = newSeries// this.getSeries(data, stackedType, yAxisName)
    relationOp.dataZoom = newDataZoom
    return relationOp
  }

  reComputedGrid() {
    let newGrid = []
    let { grid } = this.myChart.getOption()
    let echartHeight = this.myChart.getHeight()
    let unitHeight = (echartHeight - chartPaddingTop - chartPaddingBottom - (grid.length - 1) * chartGriBwn) / grid.length
    grid.forEach((item, index) => {
      newGrid.push({
        ...item,
        top: index * unitHeight + (index * chartGriBwn) + chartPaddingTop,
        height: unitHeight
      })
    })
    return newGrid
  }
  // 自定义图谱的大小尺寸响应事件
  resize() {
    this.myChart.resize()
    // legend位置重新定义
    let newGrid = this.reComputedGrid()
    let chartWidth = this.myChart.getWidth()
    let legendItemWidth = Math.floor((chartWidth - 60) / this.legendData.length)
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
      grid: newGrid
    })
    // 工具条位置重新计算
    let dvBlock = document.getElementById(this.myChart.id + 'toolbox')
    dvBlock ? dvBlock.style.left = (chartWidth - dvBlock.getBoundingClientRect().width) / 2 + 'px' : null
  }

  destroyedInstance() {
    if (this.myChart) {
      const dom = this.myChart.getDom()
      if (dom) dom.onkeydown = null
      this.myChart.off('datazoom')
      this.myChart.off('click')
      this.myChart.off('legendselectchanged')
      this.myChart.getZr().off('click')
      this.myChart.getZr().off('dblclick')
    }
  }
}
