/* eslint-disable no-empty */
import * as echarts from 'echarts'
import {
  colorSeires,
  imageBase64Data,
  debounce,
  savePicture,
  outputXlsxFile,
  echartsYaxisLabelFormatter,
  colorList
} from '../../tools/tool.js'
import { Toolbox } from '../../tools/command/toolbox.js'
import { getOptions } from '../../commonJs/optionConfig.js'

// 此组件为曲线组件单元，不自带工具条，一切功能受上层组件控制
// 输入为echart对象，输出为工具的操作，及数据的加载
/**
 * 拉索力分布图极坐标
 */
export class PolarChart {
  constructor(echart, lineData, { theme, isShowTitle, isAddNote, isDataDown } = {}) {
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
    const { dimensions, titleText, data, steelNumber } = this.echartData
    if (data && data.length) {
      let legendData = data.map(i => { return i.name })
      let maxValue = 0;
      data.forEach(i => {
        let valueArr = Array.from(i.source, ii => Number(ii[0]))
        maxValue = Math.max(maxValue, Math.max(...valueArr))
      })
      maxValue = Math.ceil(maxValue * 1.15)
      let codeList = data[0].source.map(i => { return i[1] })
      this.selectedLegend = legendData
      this.legendData = legendData
      this.echartData.legendData = legendData

      const { backgroundColor, legendStyle, titleStyle } = getOptions(this.themeType)
      let echartWidth = this.myChart.getWidth()
      let legendItemWidth = Math.floor((echartWidth - 60) / legendData.length)
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
          show: this.isShowTitle,
        },
        angleAxis: { // 极坐标角度轴
          type: 'value',
          min: 1,
          max: steelNumber ? steelNumber % 2 == 0 ? steelNumber + 1 : steelNumber + 2 : 31,
          splitNumber: 30,
          interval: 1,
          startAngle: 90,
          clockwise: true, // 顺时针
          axisTick: {
            inside: true,
            alignWithLabel: true,
          },
          axisLabel: {
            /*  margin: 5, */
            align: 'center',
            verticalAlign: 'middle',
            formatter: function (value, index) {
              let newValue = value < 10 ? '0' + value : value
              if (codeList.includes(index + 1)) {
                return "{a|" + newValue + "}"
              } else {
                return "{b|" + newValue + "}";
              }
            },
            rich: {
              a: {
                color: 'blue',
                fontSize: 12,
                borderColor: 'blue',
                borderRadius: 50,
                borderWidth: 1,
                padding: 5,
                width: 10,
                height: 10,
              },
              b: {
                color: '#ddd',
                fontSize: 10,
                borderColor: '#ddd',
                borderRadius: 50,
                borderWidth: 1,
                padding: 5,
                width: 10,
                height: 10,
              },

            },
            splitLine: {
              lineStyle: {
                color: '#ddd'
              }
            }
          },
        },
        radiusAxis: { // 极坐标径向轴
          min: 0,
          max: maxValue,
          splitNumber: 3,
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
          center: ['50%', '50%'],// 中心点位置
          radius: '70%' // 半径
        },
        series: [
          ...this.getSeries(data),
          // 圆形标记线
          /*   {
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
                var radius = api.value(1) / maxValue * (echartHeight * 0.65 / 2);
                return {
                  type: 'circle',
                  polarIndex: 0,
                  x: echartWidth / 2,
                  y: echartHeight / 2,
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
            }, */
        ],
        tooltip: {
          trigger: 'item',
          formatter: function (params) {
            return `${dimensions[1]}: ${params.value[1] < 10 ? '0' + params.value[1] : params.value[1]}<br/>${params.value[0]}${dimensions[0]}`;
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
            confine: true,
            position: function (point, params, dom, rect, size) {
              return [Math.min(point[0], size.viewSize[0] - size.contentSize[0] - 10),
              Math.min(point[1], size.viewSize[1] - size.contentSize[1] - 20)];
            }
          },
          icon: "circle",//显示样式
          bottom: -5,
          left: "center",
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
          type: 'line',
          coordinateSystem: 'polar',
          data: source,//[...source, source[0]], // 闭环，回到初始值
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
        savePicture(this.myChart, this.echartData.titleText, this.selectedLegend, true)
        //  alert('此功能暂不支持！')
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
