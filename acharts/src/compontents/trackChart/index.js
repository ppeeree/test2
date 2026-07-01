/* eslint-disable no-empty */
import * as echarts from 'echarts'
import {
  colorSeires,
  imageBase64Data,
  accMul,
  savePicture,
  outputXlsxFile,
  echartsYaxisLabelFormatter
} from '../../tools/tool.js'
import { getOptions } from '../../commonJs/optionConfig.js'
// 此组件为曲线组件单元，不自带工具条，一切功能受上层组件控制
// 输入为echart对象，输出为工具的操作，及数据的加载
// 塔基塔顶极坐标轨迹图
export class TrackChart {
  constructor(echart, lineData, getWavePointer, { theme, isShowTitle, isAddNote, isDataDown } = {}) {
    this.myChart = echart
    this.echartData = lineData
    this.themeType = theme || 'light'
    this.zoom = {
      start: 0,
      end: 100
    }
    this.markindex = 0 // 备注增加，索引标记
    this.legendData = []
    this.isShowTitle = typeof (isShowTitle) == 'undefined' ? true : isShowTitle
    this.selectedLegend = []
    this.clickEvent = getWavePointer
    this.dataZoomDataList = []
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
  // 返回图谱的base64数据格式
  imageBase64Data(isExport) {
    return imageBase64Data(this.myChart, this.selectedLegend, isExport)
  }
  // 外控legend的显示隐藏
  setLegendShow(legend) {
    this.selectedLegend = legend
    let option = this.myChart.getOption()
    this.myChart.setOption({
      ...option,
    }, true)
  }
  // 监听事件
  addEvent() {
    this.myChart.on('dataZoom', param => {
      const { dataZoom } = this.myChart.getOption()
      if (param['batch']) {
        if ('startValue' in param.batch[0]) {
          this.dataZoomDataList = [...this.dataZoomDataList, dataZoom]
          //  this.Toolbox.updateBackDomStyle(this.dataZoomDataList)
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
  }
  // 初始化
  initData(dataSourse) {
    this.echartData = dataSourse || this.echartData
    if (this.echartData.data && this.echartData.data.length) {
      let legendData = this.echartData.data.map(i => { return i.name })
      let { danger, attention, warning } = this.echartData.data[0].other
      let maxValue = Math.max(danger, attention, warning);
      this.echartData.data.forEach(i => {
        let valueArr = Array.from(i.source, ii => Number(ii[0]))
        maxValue = Math.max(maxValue, Math.max(...valueArr))
      })
      maxValue = accMul(maxValue, 1.15) // maxValue * 1.15 // Math.ceil(maxValue * 1.15)// (maxValue * 1.15).toFixed(5)
      this.selectedLegend = legendData
      this.legendData = legendData
      this.echartData.legendData = legendData
      const { xType, yType, dimensions, titleText, data } = this.echartData
      // const { lineArr, minv, maxv } = this.getMarklines()
      const { gridStyle, backgroundColor, legendStyle, titleStyle, color, toolboxStyle, toolboxFeature, xAxisStyle, yAxisStyle } = getOptions(this.themeType)
      let yAxisName = []
      data.forEach((item, index) => {
        let { dimensions } = item
        dimensions && dimensions.length ? yAxisName.push(dimensions[1]) : null
      })
      yAxisName = [...new Set(yAxisName)] // 去重
      let echartHeight = this.myChart.getHeight()
      let echartWidth = this.myChart.getWidth()
      // let legendItemWidth = Math.floor((echartWidth - 60) / legendData.length)
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
        color: colorSeires,
        title: {
          ...titleStyle,
          text: titleText,
        },
        angleAxis: { // 极坐标角度轴
          type: 'value',
          min: 0,
          max: 360,
          interval: 45,
          startAngle: 90,
          clockwise: false, // 逆时针
          axisLabel: {
            formatter: function (value) {
              return value + '°';
            }
          },
          splitLine: {
            lineStyle: {
              color: '#ddd'
            }
          }
        },
        radiusAxis: { // 极坐标径向轴
          min: 0,
          max: maxValue,
          splitNumber: 3,
          // interval: 2,
          axisLabel: {
            formatter: (val) => {
              return echartsYaxisLabelFormatter(val)
            }
          },
          splitLine: {
            lineStyle: {
              color: '#eee'
            }
          }
        },
        polar: { // 极坐标
          center: ['60%', '50%'],// 中心点位置
          radius: '78%' // 半径
        },
        series: [
          ...this.getSeries(this.echartData.data),
          // 圆形标记线
          {
            type: 'custom',
            coordinateSystem: 'polar',
            label: {
              show: false
            },
            tooltip: {
              show: false
            },
            renderItem: function (params, api, index) {
              let { dataIndex } = params
              var radius = api.value(1) / maxValue * (echartHeight * 0.78 / 2);
              return {
                type: 'circle',
                polarIndex: 0,
                x: echartWidth * 0.6,
                y: echartHeight * 0.5,
                shape: {
                  cx: 0,
                  cy: 0,
                  r: radius
                },
                style: {
                  stroke: dataIndex === 0 ? '#fe0303' : dataIndex === 1 ? '#ff9d00' : '#FFE604',
                  fill: 'transparent',
                  lineWidth: 1
                }
              };
            },
            data: [[1, danger], [3, warning], [5, attention]]  // [角度, 半径]
          },
        ],
        tooltip: {
          trigger: 'item',
          formatter: function (params) {
            return `${dimensions[1]}: ${params.value[1]}°<br/>${dimensions[0]}: ${params.value[0]}`;
          }
        },
        legend: {
          ...legendStyle,
          triggerEvent: true,
          formatter: function (name) {
            return echarts.format.truncateText(name, 100 /* legendItemWidth - 10 */, '14px Microsoft Yahei', '…');
          },
          tooltip: {
            show: true,
            confine: true,
            position: function (point, params, dom, rect, size) {
              return [Math.min(point[0], size.viewSize[0] - size.contentSize[0] - 10),
              Math.min(point[1], size.viewSize[1] - size.contentSize[1] - 20)];
            }
          },
          orient: 'vertical',
          left: 10,
          top: 'bottom',
          icon: "circle",//显示样式
          /*  bottom: 0,
           left: "center", */
          width: echartWidth,
          data: this.legendData || []
        },
        animationDuration: 1000,
      }
      this.myChart.setOption(options)
      this.addEvent()
    } else { // 无数据
      return
    }
  }

  // 获取series数据（进行数据点symbol转换）
  getSeries(data) {
    let arr = []
    if (data.length) {
      data.forEach((element, index) => {
        const { id, name, source } = element
        arr.push({
          id,
          name,
          type: 'scatter',
          coordinateSystem: 'polar',
          data: source,
          symbol: 'circle',
          symbolSize: 5,
          label: {
            show: false,
            formatter: function (params) {
              return params.value[0];
            },
            position: 'right',
            color: '#666'
          },
          emphasis: {
            label: {
              show: false
            }
          }
        })
      })
    }
    return arr
  }
  // 返回图谱的base64数据格式
  imageBase64Data(isExport) {
    return imageBase64Data(this.myChart, this.selectedLegend, isExport)
  }
  // 上级控制工具条功能
  // 根据传入的type参数，执行不同的操作
  toolboxFeatures(type, content) {
    switch (type) {
      case 'reset':
        this.reset()
        break
      case 'confirm':
        this.addRemarks(content, [20, 20], this.myChart)
        break
      case 'clearnote':
        this.deleteAllRemarks(this.myChart)
        break
      case 'copy':
        // alert('此功能暂不支持！')
        savePicture(this.myChart, this.echartData.titleText, this.selectedLegend, true)
        break
      case 'downpic':
        savePicture(this.myChart, this.echartData.titleText, this.selectedLegend);
        break
      case 'datadown':
        this.downLoadData(this.myChart)
        break
      default:
        break
    }
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

  // 自定义图谱的大小尺寸响应事件
  resize() {
    this.myChart.resize()
  }


  // 数据下载
  downLoadData(echart) {
    const data = this.getData(echart)
    const chart_data = []
    data.result.forEach((item, i) => {
      const newchart_data = [[...item.columnList]]
      chart_data[i] = newchart_data.concat(item.data)
    })
    outputXlsxFile(chart_data, data.fileName, data.sheetName)
  }
  getData(echart) {
    const r = {};
    r.fileName = ''
    r.result = [];
    r.sheetName = this.echartData.legendData
    r.fileName = this.echartData.titleText
    this.echartData.data.forEach(item => {
      r.result.push({
        data: item.source,
        columnList: item.dimensions
      })
    })
    return r;
  }
  addRemarks(text, pos, echart) {
    let idName = 'marks' + this.markindex
    let info = {
      id: idName,
      right: pos[0],
      top: pos[1],
      draggable: true,
      $action: 'replace',
      cursor: "move",
      ondrag: (position) => {
        this.onTextDragging(idName, [position.offsetX, position.offsetY], echart);
      },
      type: 'text',
      z: 100,
      style: {
        fill: '#000',
        width: 100,
        overflow: 'break',
        text: text,
        font: '14px Microsoft YaHei'
      }
    }
    let graphicList = []
    let { graphic } = echart.getOption()
    if (!graphic || !graphic.length) {
      graphicList.push(info)
    } else {
      graphicList = [...graphic[0].elements, info]
    }
    echart.setOption({
      graphic: graphicList
    })
    this.markindex++
  }
  onTextDragging(id, position, echart) {
    let { graphic } = echart.getOption()
    let result = graphic[0].elements
    result.forEach(i => {
      if (i.id == id) {
        i.left = position[0]
        i.top = position[1]
      }
    })
    echart.setOption({
      graphic: result
    })
  }
  deleteAllRemarks(echart) {
    let options = echart.getOption()
    let { graphic } = options
    if (!graphic || !graphic.length) {
      return
    } else {
      let list = graphic[0].elements.filter(ele => ele.id.includes('marks'))
      if (list.length) {
        echart.setOption({
          ...options,
          graphic: graphic[0].elements.filter(ele => !ele.id.includes('marks'))
        }, true)
      } else {
        return
      }
    }
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
