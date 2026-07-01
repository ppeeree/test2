/* eslint-disable no-empty */
import * as echarts from 'echarts'
import {
  colorSeires,
  imageBase64Data
} from '../../tools/tool.js'
import { Toolbox } from '../../tools/command/toolbox.js'
// import { this.Toolbox.creatToolBox, this.Toolbox.updateBackDomStyle } from '../tools/command/toolbox.js'
import { getOptions } from '../../commonJs/optionConfig.js'
// import { histogram } from 'echarts-stat'
import ecStat from 'echarts-stat'
const histogram = ecStat.histogram || ecStat.default?.histogram
// 此组件为倾覆分析图谱
// 输入为echart对象，输出为工具的操作，及数据的加载
export class DipAngleChart {
  constructor(echart, lineData, { theme, isAddNote, isDataDown } = {} /* clickEvent, alertMessage */) {
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
    this.legendData = []
    this.selectedLegend = []
    this.isMultipleClick = 'more'
    this.dataZoomDataList = []
    this.Toolbox = new Toolbox(this.myChart, this.themeType,
      {
        isYB: false,
        isStacked: false,
        isMarkLine: false,
        isZoom: false,
        isSetX: false,
        isSetY: false,
        isAddNote: typeof (isAddNote) == 'undefined' ? true : isAddNote,
        isDataDown: typeof (isDataDown) == 'undefined' ? true : isDataDown,
      },
      this.hotUpdate.bind(this),
    )
    this.initData(lineData)
  }

  // 工具条组件中调用或者修改实例属性
  hotUpdate(data) {
    if (data) {
      // const { dataZoomDataList } = data
      for (let key in data) {
        this[key] = data[key]
      }
      //  this.dataZoomDataList = dataZoomDataList
    } else {
      return {
        selectedLegend: this.selectedLegend,
        dataZoomDataList: this.dataZoomDataList,
        echartData: this.echartData
      }
    }
  }
  // 获取图片数据流
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
          this.Toolbox.updateBackDomStyle(this.dataZoomDataList, this.themeType)
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
        this.Toolbox.updateBackDomStyle(this.dataZoomDataList, this.themeType)
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
    /* this.myChart.getZr().off('contextmenu')
    this.myChart.getZr().on('contextmenu', (param) => {
      this.Toolbox.setStyleMenuContent(param)
    }) */
  }

  initData(dataSourse) {
    this.echartData = dataSourse || this.echartData
    const { data, dimensions, titleText } = this.echartData
    if (data && data.length) {
      let xmin = Infinity
      let xmax = -Infinity
      let ymin = Infinity
      let ymax = -Infinity
      let legendData = []
      let xFBdataArr = []
      let yFBdataArr = []
      let xyScatterArr = []
      // let colors = []
      for (let i = 0; i < data.length; i++) {
        legendData.push(data[i].name)
        //  colors.push(this.echartData[i].lineColor)
        let newData = Array.from(data[i].source, item => [
          Number(item[1]),
          Number(item[2]),
          item[0]
        ])
        xyScatterArr.push(newData)
        let xDatas = []
        let yDatas = []
        newData.forEach(i => {
          xDatas.push(i[0])
          yDatas.push(i[1])
        })
        let xbins = histogram(xDatas)
        // let Xinterval
        let xdataArr = xbins.data.map((item, index) => {
          // 左刻度
          let x0 = xbins.bins[index].x0
          // 右刻度
          let x1 = xbins.bins[index].x1
          // Xinterval = math.subtract(x1, x0)
          // 获得数据集中最值
          xmin = Math.min(xmin, x0)
          xmax = Math.max(xmax, x1)
          // item[0]代表刻度的中间值，item[1]代表出现的次数
          return [x0, x1, item[1], item[4]]
        })
        xFBdataArr.push(xdataArr)
        let ybins = histogram(yDatas)
        // let Yinterval

        let ydataArr = ybins.data.map((item, index) => {
          // 左刻度
          let x0 = ybins.bins[index].x0
          // 右刻度
          let x1 = ybins.bins[index].x1
          // Yinterval = math.subtract(x1, x0)
          // 获得数据集中最值
          ymin = Math.min(ymin, x0)
          ymax = Math.max(ymax, x1)
          // item[0]代表刻度的中间值，item[1]代表出现的次数
          return [x0, x1, item[1], item[4]]
        })
        yFBdataArr.push(ydataArr)
      }
      this.legendData = legendData
      this.selectedLegend = legendData
      this.echartData.legendData = legendData
      let minV = Math.min(xmin, ymin)
      let maxV = Math.max(xmax, ymax)
      let seriesDataArr = []
      xyScatterArr.forEach((item, index) => {
        let arr = [{
          id: legendData[index],
          name: legendData[index],
          itemStyle: {
            color: colorSeires[index % colorSeires.length],
            opacity: 0.8
          },
          type: 'scatter',
          xAxisIndex: 0,
          yAxisIndex: 0,
          encode: {
            x: 0,
            y: 1,
            tooltip: [0, 1, 2]
          },
          data: item,
          markPoint: {
            data: []
          },
          tooltip: {
            formatter(params) {
              return (
                params.value[2] +
                '</br>' +
                params.marker /* + params.seriesName */ + ' ' +
                'x: ' +
                params.value[0] +
                ' y: ' +
                params.value[1]
              )
            }
          }
        }, {
          name: legendData[index],
          type: 'custom',
          itemStyle: {
            color: colorSeires[index % colorSeires.length],
            opacity: 0.8
          },
          // color: colorSeires[index], //colors[index],
          xAxisIndex: 1,
          yAxisIndex: 1,
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
                width: size[0] - 2 < 1 ? 1 : size[0] - 2,// 值太小柱子太小看不见，设置最小宽度
                height: size[1]
              },
              style: style
            }
          },
          barWidth: '99.3%',
          label: {
            show: false,
            position: 'top'
          },
          encode: { x: [0, 1], y: 2, itemName: 3 },
          // datasetIndex: 1,
          data: xFBdataArr[index]
        },
        {
          name: legendData[index],
          itemStyle: {
            color: colorSeires[index % colorSeires.length],
            opacity: 0.8
          },
          // color: colorSeires[index], //colors[index],
          type: 'custom',
          xAxisIndex: 2,
          yAxisIndex: 2,
          barWidth: '99.3%',
          label: {
            show: false,
            position: 'right'
          },
          renderItem: function (params, api) {
            // 这个根据自己的需求适当调节
            let xValue = api.value(2)
            let start = api.coord([0, api.value(1)])
            let size = api.size([xValue, api.value(1) - api.value(0)])
            let style = api.style()
            return {
              // 矩形及配置
              type: 'rect',
              shape: {
                x: start[0],
                y: start[1],
                width: size[0],
                height: size[1] < 1 ? 1 : size[1] // 值太小，柱子太小看不见，设置最小高度
              },
              style: style
            }
          },
          // encode: { x: 1, y: 0, itemName: 4 },
          encode: { y: [0, 1], x: 2, itemName: 3 },
          data: yFBdataArr[index]
          // datasetIndex: 2
        }
        ];
        seriesDataArr = [...seriesDataArr, ...arr]
      })
      let legendItemWidth = Math.round((this.myChart.getWidth() - 60) / legendData.length)
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
        color: colorSeires,
        tooltip: {},
        backgroundColor,
        legend: {
          ...legendStyle,
          triggerEvent: true,
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
          data: legendData || []
        },
        title: {
          ...titleStyle,
          text: titleText,
          show: false,
        },
        grid: [
          {
            // 左下
            ...gridStyle,
            top: '35%',
            right: '30%',
            left: '13%',
            bottom: 60,
            show: true,
            // backgroundColor: '#000',
            containLabel: false,
            // borderColor: '#555'
          },
          {
            // 左上
            ...gridStyle,
            bottom: '69%',
            right: '30%',
            top: 50,
            left: '13%',
            show: true,
            containLabel: false,
            // backgroundColor: '#000',
            // borderColor: '#555'
          },
          {
            // 右下
            ...gridStyle,
            top: '35%',
            left: '74%',
            bottom: 60,
            right: 20,
            containLabel: false,
            show: true,
            // backgroundColor: '#000',
            // borderColor: '#555'
          }
        ],
        xAxis: [
          {
            ...xAxisStyle,
            name: dimensions[0],
            nameLocation: 'center',
            nameGap: 30,
            scale: true,
            gridIndex: 0,
            min: minV && minV < -1 ? minV : -1,
            max: maxV && maxV > 1 ? maxV : 1,
            /*  min: xmin,
             max: xmax */
            // interval: Xinterval
          },
          {
            ...xAxisStyle,
            type: 'value',
            scale: true,
            axisTick: { show: false },
            axisLabel: { show: false },
            axisLine: { show: false },
            gridIndex: 1,
            min: minV && minV < -1 ? minV : -1,
            max: maxV && maxV > 1 ? maxV : 1,
            /*   min: xmin,
              max: xmax, */
            // interval: Xinterval,
          },
          {
            ...xAxisStyle,
            type: 'value',
            scale: false, // 不能修改
            gridIndex: 2,
            position: 'top',
            axisLabel: {
              show: true,
              ...xAxisStyle.axisLabel,
              rotate: -90
            },
          }
        ],
        yAxis: [
          {
            ...yAxisStyle,
            name: dimensions[1],
            nameLocation: 'center',
            nameGap: 40,
            nameRotate: 90,
            gridIndex: 0,
            min: minV && minV < -1 ? minV : -1,
            max: maxV && maxV > 1 ? maxV : 1,
            axisLabel: {
              ...yAxisStyle.axisLabel,
              show: true,
              showMaxLabel: true,
              showMinLabel: true,
            },
            /*  min: ymin,
             max: ymax */
            // interval: Yinterval
          },
          {
            ...yAxisStyle,
            type: 'value',
            gridIndex: 1,
          },
          {
            ...yAxisStyle,
            type: 'value',
            // type: 'category',
            axisTick: { show: false },
            axisLabel: { show: false },
            axisLine: { show: false },
            gridIndex: 2,
            min: minV && minV < -1 ? minV : -1,
            max: maxV && maxV > 1 ? maxV : 1,
            /*   min: ymin,
              max: ymax */
            // interval: Yinterval
          }
        ],
        series: [
          // 自定义刚度圆
          {
            name: 'circle',
            type: 'custom',
            xAxisIndex: 0,
            yAxisIndex: 0,
            data: [0],
            tooltip: {
              show: false
            },
            renderItem: function (params, api) {
              var start = api.coord([0, 0])
              var r = api.size([0, 1])[1]
              return {
                type: 'circle',
                shape: {
                  cx: start[0],
                  cy: start[1],
                  r
                },
                style: api.style({ fill: 'transparent', stroke: '#aaa' })
              }
            }
          },
          ...seriesDataArr
        ]
      }
      this.myChart.setOption(options)
      this.addEvent()
    } else {
      return
    }
  }
  // 点击波形点，高亮效果
  addClickPointer(param) {
    let { seriesId, value, dataIndex, color } = param
    let option = this.myChart.getOption()
    let waveClickedData = []
    let seriesMarkpointer = option.series.find(i => i.id == seriesId).markPoint.data
    option.series.forEach(i => {
      if (i.id == seriesId) {
        seriesMarkpointer.push({
          seriesColor: i.color,
          name: i.name + '&&' + result.value[0],
          seriesId: i.id,
          coord: result.value,
          symbol: 'emptyCircle',
          symbolSize: 15,
          itemStyle: {
            color: i.color,
          },
          other: { ...this.echartData.data.find(ii => ii.id == i.id).other }
        })
        waveClickedData = [...waveClickedData, ...seriesMarkpointer]
      } else {
        waveClickedData = [...waveClickedData, ...i.markPoint.data]
      }
    })
    this.myChart.setOption({
      series: [{
        id: seriesId,
        markPoint: {
          data: seriesMarkpointer
        }
      }]
    })
    this.clickEvent(waveClickedData)
  }
  // 单选高亮
  oneClickPointer(param) {
    let { seriesId, value, dataIndex } = param
    let resultSameX = []
    // 获取当前相同采集时间点，不同series的数据点
    let option = this.myChart.getOption()
    let newSeries = option.series.map(item => {
      if (item.id) {
        return {
          ...item,
          markPoint: {
            data: []
          }
        }
      } else {
        return item
      }
    })
    // let { markPoint, color } = option.series.find(i => i.id == seriesId)
    let waveClickedData = []
    waveClickedData.push({
      seriesColor: color,
      seriesId: seriesId,
      coord: value,
      symbol: 'emptyCircle',
      symbolSize: 15,
      itemStyle: {
        color: color,
      },
      //  other: { ...this.echartData.data.find(ii => ii.id == i.id).other }
    })
    this.myChart.setOption({
      series: [
        ...newSeries,
        {
          id: seriesId,
          markPoint: {
            data: waveClickedData
          }
        }
      ]
    })
    this.clickEvent(waveClickedData)
  }
  // 取消高亮显示
  unClickedWavePointer(param) {
    const { coord, name, seriesId } = param.data
    // let ids = name.split('&&')
    let { series } = this.myChart.getOption()
    let waveClickedData = []
    let markPointData = []
    series.filter(item => item.id === seriesId).forEach(item => {
      if (item.markPoint && item.markPoint.data && item.markPoint.data.length) {
        let result = item.markPoint.data.find(i => i.coord[0] == coord[0] && i.coord[1] == coord[1])
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
    })
    this.myChart.setOption({
      series: [
        ...markPointData
      ]
    })
    this.clickEvent(waveClickedData)

  }
  // 清空所有的波形高亮点
  clearUpHLWave() {
    let { series } = this.myChart.getOption()
    let seriesData = []
    series.forEach(item => {
      if (item.id) {
        seriesData.push({
          id: item.id,
          markPoint: {
            data: []
          }
        })
      }
    })
    this.myChart.setOption({
      series: [...seriesData]
    })
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
