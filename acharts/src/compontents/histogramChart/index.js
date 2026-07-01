/* eslint-disable no-empty */
import * as echarts from 'echarts'
import {
  colorSeires,
  imageBase64Data,
  echartsYaxisLabelFormatter
} from '../../tools/tool.js'
import { Toolbox } from '../../tools/command/toolbox.js'
import { getOptions } from '../../commonJs/optionConfig.js'
// import { histogram } from 'echarts-stat'
import ecStat from 'echarts-stat'
const histogram = ecStat.histogram || ecStat.default?.histogram

// 此组件为分布分析
// 输入为echart对象，输出为工具的操作，及数据的加载
export class HistogramChart {
  constructor(echart, dataSource, { theme, isAddNote, isDataDown } = {}) {
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
        isStacked: false,
        isZoomY: false,
        isAddNote: typeof (isAddNote) == 'undefined' ? true : isAddNote,
        isDataDown: typeof (isDataDown) == 'undefined' ? true : isDataDown,
      },
      this.hotUpdate.bind(this))
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

  initData(dataSourse) {
    this.echartData = dataSourse || this.echartData
    if (this.echartData.data && this.echartData.data.length) {
      // 分布分析option
      const { data, titleText, yAxisName } = this.echartData
      let legendData = data.map(i => { return i.name })
      let dimensionsList = [...new Set(data.map(i => { return i.dimensions[0] }))]
      let seriesData = []
      this.selectedLegend = legendData
      this.legendData = legendData
      let xmin = Infinity
      let xmax = -Infinity
      if (data.length) {
        for (let i = 0; i < data.length; i++) {
          let unit = []
          data[i].source.forEach(ii => {
            unit.push(Number(ii[0]))
          })
          let bins = histogram(unit)
          let dataArr = bins.data.map((item, index) => {
            // 左刻度
            let x0 = bins.bins[index].x0
            // 右刻度
            let x1 = bins.bins[index].x1
            // Xinterval = math.subtract(x1, x0)
            // 获得数据集中最值
            xmin = Math.min(xmin, x0)
            xmax = Math.max(xmax, x1)
            // item[0]代表刻度的中间值，item[1]代表出现的次数
            return [x0, x1, item[1], item[4]]
          })
          seriesData.push({
            name: data[i].name,
            id: data[i].id,
            type: 'custom',
            barWidth: '99.3%',
            itemStyle: {
              opacity: 0.8,
            },
            color: colorSeires[i % colorSeires.length],
            // barCategoryGap: 0,
            barCategoryGap: 0,
            /*   label: {
                show: true,
                position: 'insideTop',
                color: '#fff',
              }, */
            data: dataArr,
            renderItem: function (params, api) {
              // 这个根据自己的需求适当调节
              let yValue = api.value(2)
              let start = api.coord([api.value(0), yValue])
              let size = api.size([api.value(1) - api.value(0), yValue])
              let style = api.style()
              return {
                // 矩形及配置
                type: 'rect',
                shape: {
                  x: start[0] + 1,
                  y: start[1],
                  width: size[0] - 2,
                  height: size[1]
                },
                style: style
              }
            },
            encode: { x: [0, 1], y: 2, itemName: 3 },
            /*  itemStyle: {
               opacity: 0.9,
               color: data[i].lineColor
             }, */
            tooltip: {
              formatter(params) {
                return (
                  params.marker +
                  /*  params.seriesName +
                   ': ' + */
                  '区间: ' +
                  params.value[3] +
                  '&nbsp;&nbsp;数量: ' +
                  params.value[2]
                )
              }
            }

          })
        }
      }
      xmin ? xmin = Math.floor((xmin - (xmax - xmin) / 10) * 100) / 100 : xmin
      xmax ? xmax = Math.ceil((xmax + (xmax - xmin) / 10) * 100) / 100 : xmax
      const { gridStyle, titleStyle, backgroundColor, color, legendStyle, xAxisStyle, yAxisStyle } = getOptions(this.themeType)
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
        toolbox: {
          id: 'toolbox',
          show: false,
        },
        backgroundColor,
        title: {
          ...titleStyle,
          text: titleText
        },
        color,
        grid: {
          ...gridStyle,
          bottom: 70,
          right: 20,
          top: 50,
          left: 60,
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
        tooltip: {},
        xAxis: {
          type: 'value',
          name: dimensionsList.length == 1 ? dimensionsList[0] : '',
          // boundaryGap: '5%',
          scale: true, //这个一定要设，不然barWidth和bins对应不上
          ...xAxisStyle,
          min: xmin,
          max: xmax,
          nameLocation: 'center',
          nameGap: 30,
          axisLabel: {
            ...xAxisStyle.axisLabel,
            width: 100,
          },
        },
        yAxis: {
          type: 'value',
          name: '个',
          ...yAxisStyle,
          nameLocation: 'center',
          nameGap: 40,
          nameRotate: 0,
          axisLabel: {
            ...yAxisStyle.axisLabel,
            formatter: (val) => {
              return echartsYaxisLabelFormatter(val)
            }
          },
        },
        dataZoom: [{
          type: 'inside',
          id: 'zoom1',
          xAxisIndex: [0],
        },
       /*  {
          id: 'zoom2',
          start: this.zoom.start,
          end: this.zoom.end,
          height: 5,
          bottom: 30,
        } */],
        series: [
          ...seriesData,
        ]
      };
      this.myChart.setOption(options)
      this.addEvent()
    } else { // 无数据
      return
    }
  }


  // 重置
  reset() {
    this.zoom.start = 0
    this.zoom.end = 100
    const options = this.myChart.getOption()
    let zoomData = []
    let isRevert = false
    options.dataZoom.forEach(i => {
      if (i.start !== 0 || i.end !== 100) {
        return isRevert = true
      }
    })
    if (isRevert) {
      zoomData = options.dataZoom.map(item => {
        return { ...item, start: 0, end: 100 }
      })
      this.myChart.setOption({
        dataZoom: zoomData
      })
    }
    this.clearUpHLWave()
  }

  // 自定义图谱的大小尺寸响应事件
  resize() {
    this.myChart.resize()
    // legend位置重新定义
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
      this.myChart.getZr().off('click')
      this.myChart.getZr().off('dblclick')
    }
  }

}
