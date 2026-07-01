/* eslint-disable no-empty */
import * as echarts from 'echarts'
import {
  colorSeires
} from '../../tools/tool.js'
import { Toolbox } from '../../tools/command/toolbox.js'
// import { this.Toolbox.creatToolBox, this.Toolbox.updateBackDomStyle } from '../tools/command/toolbox.js'
import { getOptions } from '../../commonJs/optionConfig.js'
import dayjs from 'dayjs'

// 此组件为波形索引
// 输入为echart对象，输出为工具的操作，及数据的加载
export class WaveFormIndexChart {
  constructor(echart, lineData, clickEvent, { theme, isAddNote, isDataDown } = {}) {
    this.myChart = echart
    this.echartData = lineData
    this.themeType = theme || 'light' // 'dark'
    // 是否显示波形点
    this.isShowWavePointer = false
    this.isSmooth = false
    this.zoom = {
      start: 0,
      end: 100
    }
    this.markLineList = []
    this.touchTwoEve = false
    this.legendParam = {}
    this.dataZoomDataList = []
    this.selectedLegend = []
    this.legendData = []
    this.clickEvent = clickEvent
    this.Toolbox = new Toolbox(this.myChart, this.themeType, {
      isYB: false,
      isMarkLine: false,
      isAddNote: typeof (isAddNote) == 'undefined' ? true : isAddNote,
      isDataDown: typeof (isDataDown) == 'undefined' ? true : isDataDown,
    }, this.hotUpdate.bind(this))

    this.initData(lineData)
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

  // 监听事件
  addEvent() {
    this.myChart.off('datazoom')
    this.myChart.on('dataZoom', param => {
      const { dataZoom } = this.myChart.getOption()
      if (param['batch']) {
        if ('startValue' in param.batch[0]) {
          this.dataZoomDataList = [...this.dataZoomDataList, dataZoom]
          this.Toolbox.updateBackDomStyle(this.dataZoomDataList)
        }
      }
    })
    this.myChart.off('click')
    this.myChart.on('click', (param, ev) => {
      const { ctrlKey } = param.event.event
      if (param.componentType == "markPoint") {
        if (ctrlKey) {
          this.unClickedWavePointer(param)
        } else {
          this.oneClickPointer({ seriesId: param.data.seriesId, color: param.color, value: param.value, event: param.event })
        }
      }
      else if (param.value && param.value.length) {
        if (ctrlKey) {
          this.addClickPointer(param)
        } else {
          this.oneClickPointer(param)
        }
      }
    })
    this.myChart.getZr().off('dblclick')
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
        // this.echartsDispatch()
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
    /*  this.myChart.getZr().off('contextmenu')
     this.myChart.getZr().on('contextmenu', (param) => {
       this.Toolbox.setStyleMenuContent(param)
     }) */
  }
  // 鼠标默认放大的手型
  echartsDispatch() {
    this.myChart.dispatchAction({
      type: 'takeGlobalCursor',
      key: 'dataZoomSelect',
      dataZoomSelectActive: true
    })
  }

  // 初始化
  initData(dataSourse) {
    this.echartData = dataSourse || this.echartData
    if (this.echartData.data && this.echartData.data.length) {
      const { xAxisType, yAxisType, dimensions, titleText, data } = this.echartData
      // const { lineArr, minv, maxv } = this.getMarklines()
      let legendData = this.echartData.data.map(i => { return i.name })
      this.selectedLegend = legendData
      this.legendData = legendData
      this.echartData.legendData = legendData
      const { gridStyle, backgroundColor, titleStyle, legendStyle, color, toolboxStyle, toolboxFeature, xAxisStyle, yAxisStyle } = getOptions(this.themeType)
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
        grid: {
          ...gridStyle,
          left: 55,
          right: 50,
          top: 60,
          bottom: 60,
          right: 50
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
          trigger: 'item',
          textStyle: {
            fontSize: 12,
            color: '#1A1A1A'
          },
          formatter: function (param) {
            let str = ''
            if (param.componentType == 'series') {
              str += `<p style='text-align:center;margin-bottom:5px;border-bottom:1px solid #ccc'>采集时间</p>`
              str +=
                param.marker +
                '&nbsp;' +
                /*   param.seriesName +
                  '：&nbsp;' + */
                param.value[2] + '&nbsp;'
              '<br/>'
            } else if (param.componentType == "markPoint") {
              str += `<p style='text-align:center;margin-bottom:5px;border-bottom:1px solid #ccc'>采集时间</p>`
              str +=
                param.value[2] + '&nbsp;'
              '<br/>'
            }
            return str
          }
        },
        legend: {
          ...legendStyle,
          width: "90%",
          orient: 'horizontal',//控制纵向、横向
          itemGap: 20,//图例每项之间的间隔
          type: "scroll",//这句
          icon: "circle",//显示样式
          bottom: 0,
          left: "center",
          pageIconInactiveColor: '#ddd',
          pageIconColor: "#999",
          pageTextStyle: {
            color: '#999'
          },
          data: legendData || [],
        },
        dataZoom: [
          {
            type: 'inside',
            id: 'Xinside',
            start: this.zoom.start,
            end: this.zoom.end,
          },
          {
            id: 'Xslider',
            type: 'slider',
            xAxisIndex: [0],
            // filterMode: 'empty',
            start: this.zoom.start,
            end: this.zoom.end,
            height: 5,
            bottom: 30,
            left: 'center',
            orient: "horizontal",
          },
          /*  {
             type: 'inside',
             id: 'Yinside',
             start: this.zoom.start,
             end: this.zoom.end,
           }, */
          {
            type: 'slider',
            id: 'Yslider',
            start: this.zoom.start,
            end: this.zoom.end,
            orient: "vertical",
            width: 5,
            right: 15,
            yAxisIndex: [0],
            // filterMode: 'empty'
          }
        ],
        xAxis: {
          type: xAxisType || 'time',
          name: dimensions[0] || '',
          nameLocation: 'end',
          nameGap: 3,
          ...xAxisStyle,
          // data: dataSoures.categoryX,
          boundaryGap: '0%',
          hideOverlap: true,
          splitNumber: 3,
          axisLabel: {
            show: true,
            ...xAxisStyle.axisLabel,
            width: 100,
            overflow: 'break',
            padding: [0, 5, 0, 5],
            formatter: (data) => {
              return dayjs(data).format('YYYY-MM-DD')
            },
          },
        },
        yAxis: {
          type: yAxisType || 'value',
          name: dimensions[1] || '',
          nameLocation: 'center',
          nameGap: 25,
          nameRotate: 0,
          alignTicks: true,
          scale: true,
          ...yAxisStyle
        },
        series: this.getSeries(this.echartData.data)
      }
      this.myChart.setOption(options)
      this.addEvent()
    }
  }

  // 获取series数据（进行数据点symbol转换）
  getSeries(data) {
    let arr = []
    if (data.length) {
      data.forEach((element, index) => {
        const { id, name, source } = element
        let newData = source.map(item => {
          return [item[0], Number(item[1]), item[2]]
        })
        arr.push({
          name: name,
          id: id,
          type: 'scatter',
          symbolSize: 5,
          itemStyle: {
            barBorderRadius: [0, 0, 0, 0],
          },
          color: colorSeires[index % colorSeires.length],
          emphasis: {
            scale: true,
            itemStyle: {
              borderColor: colorSeires[index % colorSeires.length],
              borderWidth: 5,
            },
          },
          markPoint: {
            symbol: 'emptyCircle',
            symbolSize: 15,
            data: [],
            itemStyle: {
              normal: {
                borderColor: '#e03f83',
              }
            },
            label: {
              normal: {
                formatter: ''
              }
            }
          },
          data: newData,
        })
      })
    }
    return arr
  }

  getDataInfo(arr) {
    let result = []
    arr.map((item, index) => {
      if (this.isShowWavePointer) {
        result.push({
          value: item,
          symbol: 'circle',
          symbolSize: item[3] ? 8 : 1,
        })
      } else {
        result.push({
          value: item,
          symbol: 'circle',
          symbolSize: 1,
        })
      }
    })
    return result
  }

  // updateData
  updateData(dataSourse) {
    this.echartData = dataSourse || this.echartData
    if (this.echartData.data && this.echartData.data.length) {
      let legendData = this.echartData.data.map(i => { return i.name })
      this.selectedLegend = legendData
      this.legendData = legendData
      this.echartData.legendData = legendData
      let legendItemWidth = Math.round((this.myChart.getWidth() - 60) / legendData.length)
      let originOptions = this.myChart.getOption()
      let { series } = originOptions
      let newSeries = []
      this.echartData.data.forEach((element, index) => {
        const { id, name, source } = element
        let newData = source.map(item => {
          return [item[0], Number(item[1]), item[2]]
        })
        let obj = series.find(i => i.id == element.id)
        if (obj) {
          newSeries.push({
            ...obj,
            data: newData,
            /*  markPoint: {
               symbol: 'emptyCircle',
               symbolSize: 15,
               data: [],
               itemStyle: {
                 normal: {
                   borderColor: '#e03f83',
                 }
               },
             }, */
          })
        } else {
          newSeries.push({
            name: name,
            id: id,
            type: 'scatter',
            symbolSize: 5,
            itemStyle: {
              barBorderRadius: [0, 0, 0, 0],
            },
            color: colorSeires[(colorSeires.length - index) % colorSeires.length],
            emphasis: {
              scale: true,
              itemStyle: {
                // borderColor: colorSeires[index % colorSeires.length],
                borderWidth: 5,
              },
            },
            markPoint: {
              symbol: 'emptyCircle',
              symbolSize: 15,
              data: [],
              itemStyle: {
                normal: {
                  borderColor: '#e03f83',
                }
              },
            },
            data: newData,
          })
        }
      })
      const { legendStyle } = getOptions(this.themeType)
      let options = {
        ...originOptions,
        legend: {
          ...legendStyle,
          formatter: function (name) {
            return echarts.format.truncateText(name, legendItemWidth - 10, '14px Microsoft Yahei', '…');
          },
          tooltip: {
            show: true
          },
          icon: "circle",//显示样式
          bottom: 0,
          left: "center",
          width: "90%",
          data: legendData || [],
        },
        series: [...newSeries]
      }
      this.myChart.setOption(options, true)
    }
  }
  // 点击波形点，高亮效果
  addClickPointer(param) {
    // console.log(param)
    let { seriesId, color, value, event } = param
    let option = this.myChart.getOption()
    let markPointData = []
    option.series.forEach(item => {
      if (item.id == seriesId/*  + 'wave_pop' */) {
        markPointData = item.markPoint.data
      }
    })
    markPointData.push({
      name: seriesId + '&&' + value[2],
      coord: [value[0], value[1]],
      seriesId: seriesId,
      value,
    })
    this.myChart.setOption({
      series: [{
        id: seriesId /* + 'wave_pop' */,
        markPoint: {
          data: markPointData
        }
      }]
    })
    this.clickEvent({ id: seriesId, time: value[2], seriesColor: color }, 'add', event)
  }
  // 单选高亮
  oneClickPointer(param) {
    let { seriesId, color, value, event } = param
    let markPointData = [{
      name: seriesId + '&&' + value[2],
      coord: [value[0], value[1]],
      seriesId: seriesId,
      value,
    }]
    let option = this.myChart.getOption()
    let newSeries = option.series.map(item => {
      item.markPoint = {
        data: item.id == seriesId ? markPointData : []
      }
      return item
    })
    this.myChart.setOption({
      series: newSeries
    })
    this.clickEvent({ id: seriesId, time: value[2], seriesColor: color }, 'replace', event)
  }
  // 取消高亮
  unClickedWavePointer(param) {
    const { seriesId, name, value } = param.data
    let { series } = this.myChart.getOption()
    let markPointData = []
    series.forEach(item => {
      if (item.id == seriesId /* + 'wave_pop' */) {
        markPointData = item.markPoint.data
      }
    })
    let result = markPointData.filter(i => i.name != param.name)
    this.myChart.setOption({
      series: [{
        id: seriesId /* + 'wave_pop' */,
        markPoint: {
          data: result
        }
      }]
    })
    this.clickEvent({
      id: seriesId, time: value[2]
    }, 'delete')
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
  }

  // 自定义图谱的大小尺寸响应事件
  resize() {
    this.myChart.resize()
    // legend位置重新定义
    let chartWidth = this.myChart.getWidth()
    let legendItemWidth = Math.floor((chartWidth - 60) / this.legendData.length)
    this.myChart.setOption({
      legend: {
        formatter: function (name) {
          return echarts.format.truncateText(name, legendItemWidth - 10, '14px Microsoft Yahei', '…');
        },
        tooltip: {
          show: true
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
}
