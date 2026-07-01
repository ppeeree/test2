/* eslint-disable no-empty */
import * as echarts from 'echarts'
import {
  computedMarkValue,
  colorSeires,
  imageBase64Data,
  echartsYaxisLabelFormatter,
  debounce
} from '../../tools/tool.js'
import { Toolbox } from '../../tools/command/toolbox.js'
// import { this.Toolbox.creatToolBox, this.Toolbox.updateBackDomStyle } from '../tools/command/toolbox.js'
import { getOptions } from '../../commonJs/optionConfig.js'
import dayjs from 'dayjs'
let chartPaddingTop = 40
let chartPaddingBottom = 70
let chartGriBwn = 15
// 此组件为振动趋势图
// 输入为echart对象，输出为工具的操作，及数据的加载
export class EvTrendLine {
  constructor(echart, lineData, getWavePointer, { theme, isShowWavePointer, isStacked, isSetY, isShowTitle, isAddNote, isDataDown } = {}, isMultipleClick,  /* clickEvent, alertMessage */) {
    this.myChart = echart
    this.echartData = lineData
    this.themeType = theme || 'light'
    // 初始是否显示波形点
    this.isShowWavePointer = typeof (isShowWavePointer) == 'undefined' ? true : isShowWavePointer,
      this.isSmooth = false
    this.isShowTitle = typeof (isShowTitle) == 'undefined' ? true : isShowTitle,
      this.zoom = {
        start: 0,
        end: 100
      }
    this.legendData = []
    this.markLineList = []
    this.touchTwoEve = false
    this.selectedLegend = []
    this.clickEvent = getWavePointer
    this.isMultipleClick = isMultipleClick || 'more'// 点击是否支持多选，more：多选，默认为more，none:不能点击，single：单击
    this.computedNumber = {}
    this.dataZoomDataList = []
    this.stackedType = 'overlay'
    this.initData(lineData)
    this.Toolbox = new Toolbox(this.myChart,
      this.themeType,
      {
        isYB: false,
        isMarkLine: true,
        isFilterWavePointer: true,
        isStacked: typeof (isStacked) == 'undefined' ? true : isStacked,
        isSetY: typeof (isSetY) == 'undefined' ? true : isSetY,
        operaterMarkline: this.operaterMarkline.bind(this),
        showWavePointer: this.showWavePointer.bind(this),
        isAddNote: typeof (isAddNote) == 'undefined' ? true : isAddNote,
        isDataDown: typeof (isDataDown) == 'undefined' ? true : isDataDown,
      },
      this.hotUpdate.bind(this),
      this.changeStackedType.bind(this))
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
      if (that.isMultipleClick == 'none') {
        return
      }
      const { ctrlKey } = param.event.event
      if (param.componentType == "series") {
        if (param.value && param.value.length) {
          if (ctrlKey && that.isMultipleClick == 'more') {
            that.addClickPointer(param)
          } else {
            that.oneClickPointer(param)
          }
        }
      }
      if (param.componentType == "markPoint") {
        if (ctrlKey && that.isMultipleClick == 'more') {
          that.unClickedWavePointer(param)
        } else {
          that.oneClickPointer({ value: [...param.data.coord, '', true] })
        }
      }
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
    const { gridStyle, xAxisStyle, yAxisStyle } = getOptions(theme || this.themeType)
    let xAxisBase = {
      ...xAxisStyle,
      type: xType || 'time',//dataSoures.xAxisType || 'time',//category
      nameLocation: 'end',
      /*  hideOverlap: true, */
      splitNumber: 3,
      axisLabel: {
        ...xAxisStyle.axisLabel,
        width: 100,
        showMinLabel: true,
        showMaxLabel: true,
        alignMinLabel: 'left',
        alignMaxLabel: 'right',
        overflow: 'break',
        // padding: [0, 5, 0, 5],
        formatter: (data) => {
          if (xType == 'time' || !xType) {
            return dayjs(data).format('YYYY-MM-DD')
          }
        },
      },
    };
    let yAxisBase = {
      type: 'value',
      alignTicks: true,
      scale: true,
      boundaryGap: ['0%', '1%'],
      nameLocation: 'center',
      nameGap: 45,
      nameRotate: 90,
      ...yAxisStyle,
      axisLabel: {
        ...yAxisStyle.axisLabel,
        formatter: (val) => {
          // return val.toExponential(1)
          return echartsYaxisLabelFormatter(val)
        }
      },
    }
    let gridBase = {
      ...gridStyle,
      left: 70,
      right: 30,
    }
    let echartHeight = this.myChart.getHeight()
    let yAxisName = []
    this.selectedLegend.forEach((item, index) => {
      let { dimensions } = data.find(i => i.name == item)
      dimensions && dimensions.length ? yAxisName.push(dimensions[1]) : null;
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
          gridIndex: index
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
        } else {
          if (!newSeries.find(i => i.name == 'markline_level')) {
            newSeries.push({
              ...item,
              markLine: { data: [] }
            })
          }
        }
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
    relationOp.series = newSeries // this.getSeries(data, stackedType, yAxisName)
    relationOp.dataZoom = newDataZoom
    return relationOp
  }
  // 初始化
  initData(dataSourse) {
    this.echartData = dataSourse || this.echartData
    if (this.echartData.data && this.echartData.data.length) {
      let legendData = this.echartData.data.map(i => { return i.name })
      this.selectedLegend = legendData
      this.legendData = legendData
      this.echartData.legendData = legendData
      const { xType, yType, titleText, data } = this.echartData
      // const { lineArr, minv, maxv } = this.getMarklines()
      const { gridStyle, backgroundColor, titleStyle, legendStyle, color, toolboxStyle, toolboxFeature, xAxisStyle, yAxisStyle } = getOptions(this.themeType)
      let yAxisName = []
      data.forEach((item, index) => {
        let { dimensions } = item
        dimensions && dimensions.length ? yAxisName.push(dimensions[1]) : null
      })
      yAxisName = [...new Set(yAxisName)] // 去重
      let relationOp = this.changeStackedType()
      let legendItemWidth = Math.floor((this.myChart.getWidth() - 60) / legendData.length)
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
          text: titleText,
          show: this.isShowTitle,
        },
        /*   grid: {
            ...gridStyle,
            left: 50,
            right: 50,
            top: 60,
            bottom: 90,
            right: yAxisName.length > 1 ? 80 : 50
          }, */
        toolbox: {
          ...toolboxStyle,
          id: 'toolbox',
          show: false,
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
            snap: true,
          },
          formatter: function (params) {
            let str = ''
            if (params.length) {
              str += `<p style='text-align:center;margin-bottom:5px;border-bottom:1px solid #ccc'>${params[0].value[0]}</p>`
              params.forEach(item => {
                str +=
                  `<p style='margin:0;line-height:18px;text-align:left;padding-left:10px'><span style='display:inline-block;margin-right:4px;border-radius:10px;width:10px;height:10px;background-color:${item.borderColor};'></span>&nbsp; ${item.value ? item.value[1] : ''}&nbsp;&nbsp;${item.value[2] != '' ? item.value[2] : ''}</p>`
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
          width: "100%",
          data: legendData || [],
        },
        ...relationOp,
        dataZoom: [
          {
            type: 'inside',
            id: 'Xinside',
            xAxisIndex: [0],
          },
          {
            id: 'Xslider',
            type: 'slider',
            xAxisIndex: [0],
            left: 'center',
            height: 15,
            bottom: 35,
            orient: "horizontal",
          },
          /*  {
             type: 'inside',
             id: 'Yinside',
             yAxisIndex: [0],
           }, */
          {
            type: 'slider',
            id: 'Yslider',
            /*   start: this.zoom.start,
              end: this.zoom.end, */
            orient: "vertical",
            width: 13,
            right: 10,
            // height: '70%'
            top: 50,
            bottom: 70,
            yAxisIndex: [0],
            //filterMode: 'empty'
          }
        ],
        series: this.getSeries(data, 'overlay', yAxisName)
      }
      this.addEvent()
      this.myChart.setOption(options)
      // 默认展示VDI报警线
      this.operaterMarkline('VDI')
      // this.computedNumber = computedMarkValue(Array.from(this.echartData.data, i => (i.source)))
    } else { // 无数据
      return
    }
  }

  // 获取series数据（进行数据点symbol转换）
  getSeries(data, stackedType, yAxisName) {
    let arr = []
    if (data.length) {
      data.forEach((element, index) => {
        const { id, name, dimensions, source } = element
        arr.push({
          // 核心：渐进渲染配置
          progressive: 500,        // 每帧渲染 500 个数据点
          progressiveThreshold: 2000, // 数据量超过 2000 时启用渐进渲染
          hoverAnimation: false,

          name: name,
          id: id,
          xAxisIndex: stackedType == 'overlay' ? 0 : (stackedType == 'stacked' ? index : yAxisName.indexOf(dimensions[1])),
          yAxisIndex: stackedType == 'overlay' ? 0 : (stackedType == 'stacked' ? index : yAxisName.indexOf(dimensions[1])),
          encode: { x: 0, y: 1, tooltip: [0, 1, 2] },
          type: 'line',
          lineStyle: {
            width: 1
          },
          // sampling: 'lttb',
          color: colorSeires[index % colorSeires.length],
          data: this.getDataInfo(source, colorSeires[index % colorSeires.length]),
          markPoint: {
            data: [],
          },
          markArea: {
          },
        })
      })
    }
    return arr
  }

  getDataInfo(arr, linecolor) {
    let result = []
    arr.map((item, index) => {
      if (this.isShowWavePointer) {
        result.push({
          value: item,
          symbolKeepAspect: true,
          symbol: item[3] ? 'circle' : 'none',
          symbolSize: item[3] ? 15 : 0,
          symbolOffset: [0, 0],
          itemStyle: {
            color: item[4] ? '#FF0F0D' : linecolor,
            borderColor: linecolor,
            borderJoin: 'bevel',
            borderCap: 'butt',
            borderWidth: item[4] ? 0.01 : 2,
          },
        })
      } else {
        result.push({
          value: item,
          symbol: 'circle',
          symbolSize: 0,
        })
      }
    })
    return result
  }
  // updateData
  updateData(dataSourse) {
    this.echartData = dataSourse || this.echartData
    const { data } = this.echartData
    if (data && data.length) {
      let legendData = data.map(i => { return i.name })
      this.selectedLegend = legendData
      this.legendData = legendData
      this.echartData.legendData = legendData
      let yAxisName = []
      data.forEach((item, index) => {
        let { dimensions } = item
        yAxisName.push(dimensions[1])
      })
      yAxisName = [...new Set(yAxisName)] // 去重
      let relationOp = this.changeStackedType(this.stackedType, this.themeType)
      let legendItemWidth = Math.floor((this.myChart.getWidth() - 60) / legendData.length)
      let originOptions = this.myChart.getOption()
      const { legendStyle } = getOptions(this.themeType)
      let { series } = originOptions
      let newSeries = [] // this.getSeries(data, 'overlay', yAxisName, [])
      let waveClickedData = []
      data.forEach((item, index) => {
        let obj = series.find(i => i.id == item.id)
        if (obj) {
          let arr = obj.markPoint.data
          let newArr = []
          if (arr.length) {
            arr.forEach(i => {
              if (item.source.find(ii => ii[0] == i.coord[0])) {
                newArr.push(i)
              }
            })
          }
          waveClickedData = [...waveClickedData, ...newArr]
          newSeries.push({
            ...obj,
            name: item.name,
            data: this.getDataInfo(item.source, obj.color),
            markPoint: {
              ...obj.markPoint,
              data: newArr,
            },
          })
        } else {
          newSeries.push({
            name: item.name,
            id: item.id,
            xAxisIndex: this.stackedType == 'overlay' ? 0 : (this.stackedType == 'stacked' ? index : yAxisName.indexOf(dimensions[1])),
            yAxisIndex: this.stackedType == 'overlay' ? 0 : (this.stackedType == 'stacked' ? index : yAxisName.indexOf(dimensions[1])),
            encode: { x: 0, y: 1, tooltip: [0, 1, 2] },
            type: 'line',
            lineStyle: {
              width: 1
            },
            color: colorSeires[(colorSeires.length - index - 4) % colorSeires.length],
            data: this.getDataInfo(item.source, colorSeires[(colorSeires.length - index - 4) % colorSeires.length]),
            markPoint: {
              /*   symbol: 'emptyCircle',
                symbolSize: 15, */
              /*  itemStyle: {
                 borderColor: '#e03f83',
               }, */
              data: [],
            },
          })
        }
      })
      let options = {
        ...originOptions,
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
          icon: "circle", // 显示样式
          bottom: 0,
          left: "center",
          width: "100%",
          data: legendData || [],
        },
        ...relationOp,
        series: [...newSeries]
      }
      this.myChart.setOption(options, true)
      this.clickEvent(waveClickedData)
    }
  }
  // 点击波形点，高亮效果
  addClickPointer(param) {
    let { seriesId, value, dataIndex, color } = param
    let resultSameX = []
    // 获取当前相同采集时间点，不同series的数据点
    let option = this.myChart.getOption()
    let waveClickedData = []
    option.series.filter(i => i.name !== 'markline_level').forEach(i => {
      let result = i.data.find(item => item.value[0] == value[0] && item.value[3])
      if (result) {
        let seriesMarkpointer = option.series.find(item => item.id == i.id).markPoint.data// + 'wave_pop')
        seriesMarkpointer.push({
          seriesColor: i.color,
          name: i.name + '&&' + result.value[0],
          seriesId: i.id,
          coord: result.value,
          symbol: 'emptyCircle',
          symbolSize: 20,
          itemStyle: {
            color: i.color,
          },
          other: { ...this.echartData.data.find(ii => ii.id == i.id).other }
        })
        resultSameX.push({
          seriesId: i.id,
          value: result.value,
          markPointData: seriesMarkpointer
        })
        waveClickedData = [...waveClickedData, ...seriesMarkpointer]
      } else {
        waveClickedData = [...waveClickedData, ...i.markPoint.data]
      }
    })
    let changeSeries = []
    changeSeries = resultSameX.map(item => {
      return {
        id: item.seriesId, //+ 'wave_pop',
        markPoint: {
          data: item.markPointData
        }
      }
    })
    this.myChart.setOption({
      series: changeSeries
    })
    this.clickEvent(waveClickedData)
  }
  // 单选高亮
  oneClickPointer(param) {
    let { value } = param
    // 获取当前相同采集时间点，不同series的数据点
    let option = this.myChart.getOption()
    let waveClickedData = []
    let newSeries = option.series.map(i => {
      if (i.name != 'markline_level') {
        let seriesMarkpointer = []
        let result = i.data.find(item => item.value[0] == value[0] && item.value[3])
        if (result) {
          seriesMarkpointer = [{
            seriesColor: i.color,
            name: i.name + '&&' + result.value[0],
            seriesId: i.id,
            coord: result.value,
            symbol: 'emptyCircle',
            symbolSize: 20,
            itemStyle: {
              color: i.color,
            },
            other: { ...this.echartData.data.find(ii => ii.id == i.id).other }
          }]
        }
        waveClickedData = [...waveClickedData, ...seriesMarkpointer]
        return {
          ...i,
          markPoint: {
            data: seriesMarkpointer
          }
        }
      } else {
        return i
      }
    })
    this.myChart.setOption({
      series: newSeries
    })
    // console.log(newSeries)
    this.clickEvent(waveClickedData)
  }
  // 取消高亮显示
  unClickedWavePointer(param) {
    const { coord, name, seriesId } = param.data
    // let ids = name.split('&&')
    let { series } = this.myChart.getOption()
    let waveClickedData = []
    let markPointData = []
    series.forEach(item => {
      // if (item.id.includes('wave_pop')) {
      if (item.markPoint && item.markPoint.data && item.markPoint.data.length) {
        let result = item.markPoint.data.find(i => i.coord[0] == coord[0])
        if (result) {
          let data = item.markPoint.data.filter(i => i.coord[0] != coord[0])
          markPointData.push({
            ...item,
            markPoint: {
              data: data
            }
          })
          waveClickedData = [...waveClickedData, ...data]
        } else {
          waveClickedData = [...waveClickedData, ...item.markPoint.data]
        }
      }
      //  }
      /* if (item.id == seriesId + 'wave_pop') {
        markPointData = item.markPoint.data
      } */
    })
    // let result = markPointData.filter(i => i.name != param.name)
    this.myChart.setOption({
      series: [
        ...markPointData
      ]
    })
    // waveClickedData = waveClickedData.filter(i => i.name != param.name)
    this.clickEvent(waveClickedData)
    /*  this.clickEvent(waveClickedData, {
       coord, name
     }, 'delete') */
  }
  // 清空所有的波形高亮点
  clearUpHLWave() {
    let { series } = this.myChart.getOption()
    let seriesData = []
    series.forEach(item => {
      //  if (item.id.includes('wave_pop')) {
      seriesData.push({
        id: item.id,
        markPoint: {
          data: []
        }
      })
      // }
    })
    this.myChart.setOption({
      series: [...seriesData]
    })
  }
  // 显示波形点
  showWavePointer(isShow) {
    this.isShowWavePointer = isShow
    let seriesDatas = []
    if (this.echartData.data && this.echartData.data.length) {
      this.echartData.data.forEach((element, index) => {
        seriesDatas.push({
          name: element.name,
          id: element.id,
          type: "line",
          showAllSymbol: true,
          data: this.getDataInfo(element.source, element.color)
        })
      })
      this.myChart.setOption({
        series: seriesDatas
      })
    }
  }
  getMarkLineColor(type) {
    let color = ''
    let name = ''
    switch (type) {
      case 'max':
        color = '#D900FF'
        name = '最大值'
        break;
      case 'min':
        color = '#B387FF'
        name = '最小值'
        break;
      case 'avg':
        color = '#50FBD6'
        name = '平均值'
        break;
      case 'mid':
        color = '#FF00E5';
        name = '中位数'
        break;
      case 'warning_down':
        color = '#FFC300'
        name = 'VDI下限'
        break;
      case 'warning_up':
        color = '#FA5151';
        name = 'VDI上限'
        break;
      default:
        return;
    }
    return color
  }
  // 取消显示波形点
  /*  hiddenWavePointer() {
     this.isShowWavePointer = false
   } */
  /*   getMarklines() {
      let lineArr = []
      let compareData0 = null
      let compareData1 = null
      if (this.markLineList.length) {
        for (let i = 0; i < this.markLineList.length; i++) {
          let key = ''
          let color = ''
          let name = ''
          switch (this.markLineList[i]) {
            case 'MAX':
              key = "max"
              color = '#D900FF'
              name = '最大值'
              break;
            case 'MIN':
              key = "min"
              color = '#B387FF'
              name = '最小值'
              break;
            case 'AVG':
              key = "avg";
              color = '#50FBD6'
              name = '平均值'
              break;
            case 'MID':
              key = 'mid';
              color = '#FF00E5';
              name = '中位数'
              break;
            case 'VDI':
              key = ['vdiMin', 'vdiMax']
              color = ['#FFC300', '#FA5151']
              name = ['VDI下限', 'VDI上限']
              break;
            default:
              return;
          }
          if (Array.isArray(key)) {
            compareData1 = this.echartData[key[1]]// Math.max(this.echartData[key[1]], this.echartData[key[0]], compareData1)
            compareData0 = this.echartData[key[0]]// Math.min(this.echartData[key[1]], this.echartData[key[0]], compareData0)
          }
          let yValue = Array.isArray(key) ? [this.echartData[key[0]], this.echartData[key[1]]] : this.echartData[key]
          if (Array.isArray(yValue)) {
            yValue.map((item, index) => {
              lineArr.push({
                name: name[index],
                yAxis: item,
                lineStyle: {
                  color: color[index],
                  type: 'solid'
                },
                label: {
                  formatter: `${name[index]}: ${item}`,
                  position: index == 0 ? 'insideEndBottom' : 'insideEndTop',
                  color: '#fff',
                  backgroundColor: 'rgba(0, 0, 0, 0.6)',
                  padding: [3, 5, 3, 5]
                }
              })
            })
          } else {
            lineArr.push({
              name,
              yAxis: yValue,
              lineStyle: {
                color: color,
                type: 'solid'
              },
              label: {
                backgroundColor: 'rgba(0, 0, 0, 0.6)',
                formatter: `${name}: ${yValue}`,
                position: 'insideEndBottom',
                color: '#fff',
                padding: [3, 5, 3, 5]
              }
            })
          }
        }
      }
      compareData1 = compareData1 !== null && compareData1 < this.echartData['max'] ? null : compareData1
      compareData0 = compareData0 !== null && compareData0 > this.echartData['min'] ? null : compareData0
      return {
        lineArr,
        minv: compareData0,
        maxv: compareData1
      }
    } */

  operaterMarkline(type) {
    const { grid, series, yAxis } = this.myChart.getOption()
    let arr = series.filter(i => i.name == 'markline_level')
    if (arr.length) {
      let newArr = []
      let marklineData = []
      arr.forEach(item => {
        marklineData.push(...item.markLine.data)
        newArr.push({
          ...item,
          markLine: { data: [] }
        })
      })
      if (marklineData.length && marklineData[0].name == type) {
        this.myChart.setOption({
          series: [...series.filter(i => i.name != 'markline_level'), ...newArr],
          yAxis: yAxis.map(i => {
            return {
              ...i,
              min: null,
              max: null
            }
          })
        })
        return
      }
    }
    let markLineSeries = []
    let newYAxis = []
    let visibleLine = series.filter(i =>
      this.selectedLegend.findIndex(ii => ii == i.name) > -1
    )
    grid.forEach((item, index) => {
      let yMax = this.myChart.getModel().getComponent('yAxis', index).axis.scale._extent[1];
      let yMin = this.myChart.getModel().getComponent('yAxis', index).axis.scale._extent[0];
      let isRecomputerYmax = false
      // 获取每个grid里面显示的series的集合
      let gridIndexSeries = visibleLine.filter(i =>
        i.xAxisIndex == index
      )
      if (gridIndexSeries.length) {
        let markLineData = []
        let dataArr = Array.from(gridIndexSeries, i => (this.echartData.data.find(ii => ii.name == i.name).source))
        if (type !== 'VDI') {
          let result = Number(computedMarkValue(dataArr, type))
          markLineData.push({
            name: type,
            yAxis: result, // result,
            lineStyle: {
              color: this.getMarkLineColor(type),
              type: 'solid'
            },
            label: {
              backgroundColor: 'rgba(0, 0, 0, 0.6)',
              formatter: `${type}: ${result}`,
              position: 'insideEndBottom',
              color: '#fff',
              padding: [3, 5, 3, 5]
            }
          })
        } else if (type == 'VDI') {
          /*   let enginList = []
            gridIndexSeries.forEach(i => {
              enginList.push(i.name.split('_')[0])
            }) */
          if (
            gridIndexSeries.length == 1 // || new Set(enginList).size === 1
          ) {
            // VDI只能显示一条曲线的，即grid里面只有一条series
            // t同时要判断这条数据是否有报警定义
            let dataSourse = this.echartData.data.find(ii => ii.name == gridIndexSeries[0].name)
            if (dataSourse.VDI && dataSourse.VDI.length) {
              markLineData.push({
                name: 'warning_down',
                yAxis: Number(dataSourse.VDI[0]),
                lineStyle: {
                  color: this.getMarkLineColor('warning_down'),
                  type: 'solid'
                },
                label: {
                  show: false,
                  backgroundColor: 'rgba(0, 0, 0, 0.6)',
                  formatter: `warning_down: ${dataSourse.VDI[0]}`,
                  position: 'insideEndBottom',
                  color: '#fff',
                  padding: [3, 5, 3, 5]
                },
                emphasis: {
                  label: {
                    show: true
                  }
                }
              })
              markLineData.push({
                name: 'warning_up',
                yAxis: Number(dataSourse.VDI[1]),
                lineStyle: {
                  color: this.getMarkLineColor('warning_up'),
                  type: 'solid'
                },
                label: {
                  show: false,
                  backgroundColor: 'rgba(0, 0, 0, 0.6)',
                  formatter: `warning_up: ${dataSourse.VDI[1]}`,
                  position: 'insideEndBottom',
                  color: '#fff',
                  padding: [3, 5, 3, 5]
                },
                emphasis: {
                  label: {
                    show: true
                  }
                }
              })
              if (Math.max(Number(dataSourse.VDI[1]), Number(dataSourse.VDI[0])) > yMax) {
                isRecomputerYmax = true
                yMax = Math.max(Number(dataSourse.VDI[1]), Number(dataSourse.VDI[0]))
              }
              if (Math.min(Number(dataSourse.VDI[1]), Number(dataSourse.VDI[0])) < yMin) {
                isRecomputerYmax = true
                yMin = Math.min(Number(dataSourse.VDI[1]), Number(dataSourse.VDI[0]))
              }
            }
          }
        }
        if (isRecomputerYmax) {
          newYAxis.push({
            ...yAxis[index],
            max: yMax,
            min: yMin
          })
        } else {
          newYAxis.push({
            ...yAxis[index],
            max: null,
            min: null
          })
        }
        markLineSeries.push({
          id: 'markline_level' + index,
          name: "markline_level",
          type: 'line',
          data: [],
          tooltip: {
            show: false
          },
          markLine: {
            precision: 8, // 精度，小数点后几位，默认2，如果画0.003，画出来就是0
            symbol: 'none',
            data: markLineData
          },
          xAxisIndex: index,
          yAxisIndex: index
        })

      }
    })
    this.myChart.setOption({
      series: [
        ...series.filter(i => i.name !== 'markline_level'),
        ...markLineSeries
      ],
      yAxis: newYAxis
    })
  }

  addMarkline({ type, value, color }) {
    let valueAA;
    // const { max, min, mid, avg } = this.computedNumber
    if (this.computedNumber[type]) {
      valueAA = this.computedNumber[type]
    } else {
      valueAA = value
    }
    let { series } = this.myChart.getOption()
    let lineArr = series.find(i => i.id == 'markline_level').markLine.data
    if (lineArr.length && lineArr.filter(i => i.name == type).length) {
      let index = lineArr.findIndex(i => i.name == type)
      if (lineArr[index].yAxis == Number(valueAA) && lineArr[index].lineStyle.color == color) {
        return alert('已存在此设置的标记线')
      } else {
        lineArr.splice(index, 1, {
          name: type,
          yAxis: Number(valueAA),
          lineStyle: {
            color: color,
            type: 'solid'
          },
          label: {
            backgroundColor: 'rgba(0, 0, 0, 0.6)',
            formatter: `${type}: ${valueAA}`,
            position: 'insideEndBottom',
            color: '#fff',
            padding: [3, 5, 3, 5]
          }
        })
      }
    } else {
      lineArr.push({
        name: type,
        yAxis: Number(valueAA),
        lineStyle: {
          color: color,
          type: 'solid'
        },
        label: {
          backgroundColor: 'rgba(0, 0, 0, 0.6)',
          formatter: `${type}: ${valueAA.toFixed(5)}`,
          position: 'insideEndBottom',
          color: '#fff',
          padding: [3, 5, 3, 5]
        }
      })
    }
    this.myChart.setOption({
      series: [{
        id: 'markline_level',
        name: "markline_level",
        type: 'line',
        data: [],
        tooltip: {
          show: false
        },
        markLine: {
          data: lineArr
        }
      }]
    })
  }
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
  resize(size) {
    this.myChart.resize({
      width: size?.width || 'auto',
      height: size?.height || 'auto',
    })
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

  clear() {
    this.myChart.clear()
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
