/* eslint-disable no-empty */
import * as echarts from 'echarts'
import {
  computedMarkValue,
  colorSeires,
  imageBase64Data,
  echartsYaxisLabelFormatter,
  debounce,
  savePicture,
  outputXlsxFile
} from '../../tools/tool.js'
import { getOptions } from '../../commonJs/optionConfig.js'
import dayjs from 'dayjs'
let chartPaddingTop = 35
let chartPaddingBottom = 45
// 此组件为曲线组件单元，不自带工具条，一切功能受上层组件控制
// 输入为echart对象，输出为工具的操作，及数据的加载
export class SimpleLineChart {
  constructor(echart, lineData, getWavePointer, { theme, isShowTitle, isShowWarningLine, isAddNote, isDataDown, isConnect } = {}, connectEvent) {
    this.myChart = echart
    this.echartData = lineData
    this.themeType = theme || 'light'
    // 初始是否显示报警线
    this.isShowWarningLine = isShowWarningLine || false
    // 初始是否显示波形点
    this.isShowWavePointer = true
    this.isSmooth = false
    this.zoom = {
      start: 0,
      end: 100
    }
    this.markindex = 0 // 备注增加，索引标记
    this.legendData = []
    this.markLineList = []
    this.touchTwoEve = false
    this.isShowTitle = typeof (isShowTitle) == 'undefined' ? true : isShowTitle,
      this.selectedLegend = []
    this.isConnect = isConnect || false
    this.clickEvent = getWavePointer
    // this.connectEvent = connectEvent
    this.isMultipleClick = 'more'// 点击是否支持多选，more：多选，默认为more，none:不能点击，single：单击
    this.dataZoomDataList = []
    this.initData(lineData)
    /*    this.Toolbox = new Toolbox(this.myChart,
         this.themeType,
         {
           isMarkLine: true,
           isFilterWavePointer: true,
           isAddNote: true,
           isDataDown: true,
           isSetY: false
         },
         this.hotUpdate.bind(this),
       ) */
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
    /* this.myChart.on('finished', function () {
      console.log('finished:' + new Date().getTime())
    }); */
    /*  let isFirst = true
     this.myChart.on('finished', function () {
       // 隐藏工具条
       if (isFirst) {
         document.getElementById(this.id + 'toolbox').style.display = 'none'
         isFirst = false
       }
     }); */
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
        // this.Toolbox.updateBackDomStyle(this.dataZoomDataList)
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
      // 切换legend，标记线清除
      let option = this.myChart.getOption()
      let { series, yAxis } = option
      let obj = series.find(i => i.name == 'markline_level')
      if (obj) {
        // 拉索力的报警线如果显示的话，切换legend不清除报警线
        if (obj.markLine.data.length && obj.markLine.data.find(i => i.name.includes('VDI'))) {
          let isLSL = true
          this.selectedLegend.forEach(i => {
            if (i.split('_')[0] !== '拉索力') {
              isLSL = false
            }
          })
          if (isLSL) {
            return
          }
        }
        // 其他标记线清除
        this.myChart.setOption({
          ...option,
          series: [...option.series.filter(i => i.name != 'markline_level'), { ...obj, markLine: { data: [] } }],
          yAxis: [
            {
              ...yAxis[0],
              min: null,
              max: null
            }
          ]
        }, { lazyUpdate: true })
      }
    })
    this.myChart.off('click')
    this.myChart.on('click', (param) => {
      this.chartClickEvent(param)
    })
  }
  chartClickEvent(param) {
    if (this.isMultipleClick == 'none') {
      return
    }
    const { ctrlKey } = param.event.event
    if (param.componentType == "series") {
      if (param.value && param.value.length) {
        if (ctrlKey && this.isMultipleClick == 'more') {
          this.addClickPointer(param, ctrlKey)
        } else {
          this.oneClickPointer(param, ctrlKey)
        }
      }
    }
    // 点击指示线，与点击series上的symbol效果一样
    if (param.componentType == "markLine" && param.name == 'indicator') {
      if (ctrlKey && this.isMultipleClick == 'more') {
        this.addClickPointer({ value: [param.value] }, ctrlKey)
      } else {
        this.oneClickPointer({ value: [param.value] }, ctrlKey)
      }
    }
    // end
    if (param.componentType == "markPoint") {
      if (ctrlKey && this.isMultipleClick == 'more') {
        this.unClickedWavePointer(param, ctrlKey)
      } else {
        this.oneClickPointer({ value: [...param.data.coord, '', true] }, ctrlKey)
      }
    }
    // ctrl+点击legend 只选中当前项，其他全不选
    if (param.componentType == 'legend' && ctrlKey) {
      this.legendData.forEach((item) => {
        if (item == param.value) {
          this.myChart.dispatchAction({
            type: 'legendSelect',
            name: item,
          })
        } else {
          this.myChart.dispatchAction({
            type: 'legendUnSelect',
            name: item,
          })
        }
      })
    }
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
      const { gridStyle, backgroundColor, legendStyle, titleStyle, color, toolboxStyle, toolboxFeature, xAxisStyle, yAxisStyle } = getOptions(this.themeType)
      let yAxisName = []
      data.forEach((item, index) => {
        let { dimensions } = item
        dimensions && dimensions.length ? yAxisName.push(dimensions[1]) : null
      })
      yAxisName = [...new Set(yAxisName)] // 去重
      let legendItemWidth = Math.floor((this.myChart.getWidth() - 80) / legendData.length)
      let { warningLine } = data[0]
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
        axisPointer: {
          link: { xAxisIndex: "all" },
          snap: true,
          triggerTooltip: false
        },
        backgroundColor,
        color,
        title: {
          ...titleStyle,
          text: titleText,
          show: this.isShowTitle,
        },
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
                  `<p style='margin:0;line-height:18px;text-align:left;padding-left:10px'>${item.marker}&nbsp; ${item.value ? item.value[1] : ''}&nbsp;&nbsp;${item.value[2] ? item.value[2] : ''}</p>`
              })
            }
            return str
          }
        },
        grid: {
          ...gridStyle,
          left: 70,
          right: 30,
          top: chartPaddingTop,
          bottom: chartPaddingBottom,
        },
        xAxis: {
          ...xAxisStyle,
          type: xType || 'time',
          nameLocation: 'end',
          splitNumber: 3,
          axisLabel: {
            ...xAxisStyle.axisLabel,
            width: 100,
            overflow: 'break',
            showMinLabel: true,
            showMaxLabel: true,
            alignMinLabel: 'left',
            alignMaxLabel: 'right',
            formatter: (data) => {
              if (xType == 'time' || !xType) {
                return dayjs(data).format('YYYY-MM-DD')
              } else {
                return data
              }
            },
          },
          gridIndex: 0
        },
        yAxis: {
          scale: true,
          max: this.isShowWarningLine ? function (value) {
            return Math.max(value.max, Object.values(warningLine)[0]) * 1.15;
          } : function (value) {
            return value.max * 1.15;
          },
          min: this.isShowWarningLine ? function (value) {
            return Math.min(value.min, Object.values(warningLine)[1]);
          } : function (value) {
            return value.max * 1.15;
          },
          type: 'value',
          alignTicks: true,
          boundaryGap: ['0%', '1%'],
          nameLocation: 'center',
          nameGap: 45,
          nameRotate: 90,
          ...yAxisStyle,
          axisLabel: {
            ...yAxisStyle.axisLabel,
            formatter: (val) => {
              return echartsYaxisLabelFormatter(val)
            }
          },
          name: yAxisName.length == 1 ? yAxisName[0] : '', // Y轴名称，每组数据的维度名称相同则显示，不同则不显示
          gridIndex: 0,
        },
        legend: {
          ...legendStyle,
          triggerEvent: true,
          // selectedMode: 'single',
          formatter: function (name) {
            // console.trace()
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
          icon: "circle", //显示样式
          bottom: 0,
          left: "center",
          width: "100%",
          data: legendData || [],
        },
        dataZoom: [
          {
            type: 'inside',
            id: 'Xinside',
            xAxisIndex: [0],
          },
        ],
        series: this.getSeries(data, 'overlay', yAxisName)
      }
      // 默认展示三级报警线
      if (this.isShowWarningLine) {
        this.myChart.setOption(options)
      } else {
        // 默认展示VDI报警线
        let newOptions = this.operaterMarkline('VDI', true, options)
        if (newOptions) {
          this.myChart.setOption(newOptions)
        }
      }
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
        const { id, name, source, warningLine } = element
        arr.push({
          name: name,
          id: id,
          xAxisIndex: 0,
          yAxisIndex: 0,
          encode: { x: 0, y: 1, tooltip: [0, 1, 2] },
          type: 'line',
          lineStyle: {
            width: 1
          },
          sampling: 'lttb',
          color: colorSeires[index % colorSeires.length],
          data: this.getDataInfo(source, colorSeires[index % colorSeires.length]),
          markPoint: {
            data: [],
          },
          markLine: {
            symbol: 'none', //去掉箭头
            silent: true,//不响应和触发鼠标事件
            lineStyle: {
              width: 1,
              type: 'solid',
              color: '#ccc'
            },
            label: {
              show: true,
            },
            data: [],
          }
        })
      })
    }
    if (this.isShowWarningLine) {
      let lineData = []
      let { warningLine } = data[0]
      Object.keys(warningLine).forEach((item) => {
        warningLine[item] = Number(warningLine[item])
        lineData.push({
          name: item,
          yAxis: Number(warningLine[item]),
          lineStyle: {
            color: item == 'danger' ? '#fe0303' : item == 'warning' ? '#ff9d00' : '#FFE604',
          },
        })
      })
      arr.push({
        name: 'markline_level',
        id: 'markline_level',
        xAxisIndex: 0,
        yAxisIndex: 0,
        type: 'line',
        data: [],
        markLine: {
          precision: 8, // 精度，小数点后几位，默认2，如果画0.003，画出来就是0
          symbol: 'none',
          lineStyle: {
            width: 1,
            type: 'solid'
          },
          label: {
            show: false,
          },
          data: lineData
        },

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
      let originOptions = this.myChart.getOption()
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
              data: [],
            },
          })
        }
      })
      let options = {
        ...originOptions,
        legend: {
          show: true,
          data: legendData || [],
        },
        series: [...newSeries]
      }
      this.myChart.setOption(options, true)
      this.clickEvent(waveClickedData)
    }
  }
  // 点击波形点，高亮效果
  addClickPointer(param, ctrlKey) {
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
            color: i.color
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
      //  }
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
    this.clickEvent(waveClickedData, ctrlKey)
  }
  // 单选高亮
  oneClickPointer(param, ctrlKey) {
    let { value } = param
    let resultSameX = []
    // 获取当前相同采集时间点，不同series的数据点
    let option = this.myChart.getOption()
    let waveClickedData = []
    let newSeries = option.series.map(i => {
      if (i.name != 'markline_level') {
        let seriesMarkpointer = []
        // let seriesMarkLine = []
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
              color: i.color
            },
            other: { ...this.echartData.data.find(ii => ii.id == i.id).other }
          }]
        }
        waveClickedData = [...waveClickedData, ...seriesMarkpointer]
        return {
          ...i,
          markPoint: {
            data: seriesMarkpointer
          },
          /*  markLine: {
             data: seriesMarkLine
           } */
        }
      } else {
        return i
      }
    })
    this.myChart.setOption({
      series: newSeries
    })
    this.clickEvent(waveClickedData, ctrlKey)
  }
  // 取消高亮显示
  unClickedWavePointer(param, ctrlKey) {
    const { coord, name, seriesId } = param.data
    // let ids = name.split('&&')
    let { series } = this.myChart.getOption()
    let waveClickedData = []
    let markPointData = []
    series.forEach(item => {
      if (item.markPoint && item.markPoint.data && item.markPoint.data.length) {
        let result = item.markPoint.data.find(i => i.coord[0] == coord[0])
        if (result) {
          let data = item.markPoint.data.filter(i => i.coord[0] != coord[0])
          //  let markLineData = item.markLine.data.filter(i => i.xAxis != coord[0])
          markPointData.push({
            ...item,
            markPoint: {
              data: data
            },
            /*  markLine: {
               data: markLineData
             } */
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
    // waveClickedData = waveClickedData.filter(i => i.name != param.name)
    this.clickEvent(waveClickedData, ctrlKey)
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
  updateMarkLineIndicator(pointAxisList) {
    let markLineData = []
    let { series } = this.myChart.getOption()
    let obj = series.find(i => i.name == 'markline_level')
    if (pointAxisList && pointAxisList.length) {
      let allData = this.echartData.data.map(i => i.source).flat()
      let allXdata = allData.map(i => i[0])
      pointAxisList.forEach(i => {
        if (allXdata.indexOf(i) > -1) {
          markLineData.push({
            name: 'indicator',
            xAxis: i,
            z: 1,
          })
        }
      })
    }
    if (obj.markLine.data.length) {
      markLineData = [...markLineData, ...obj.markLine.data.filter(i => i.name != 'indicator')]
    }
    this.myChart.setOption({
      series: [...series.filter(i => i.name != 'markline_level'), { ...obj, markLine: { data: markLineData } }],
    })
  }
  // 显示波形点
  /**
   * 显示或隐藏波浪指针
   * @param {boolean} isShow - 是否显示波浪指针的标志
   */
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
      case 'VDI_Ⅱ':
        color = '#FFC300'
        name = 'VDI下限'
        break;
      case 'VDI_Ⅲ':
        color = '#FA5151';
        name = 'VDI上限'
        break;
      default:
        return;
    }
    return color
  }

  /**
   * 处理图表标记线的方法
   * @param {string} type - 标记线类型，如'VDI'或其他类型
   */
  operaterMarkline(type, isInitShowVDI, options) {
    let option = options ? options : this.myChart.getOption()
    let { series, yAxis } = option
    let obj = series.find(i => i.name == 'markline_level')
    if (obj) {
      if (obj.markLine.data.length && obj.markLine.data.find(i => i.name.includes(type))) {
        this.myChart.setOption({
          series: [...series.filter(i => i.name != 'markline_level'), { ...obj, markLine: { data: [] } }],
          yAxis: [
            {
              ...yAxis[0],
              min: null,
              max: null
            }
          ]
        })
        return
      }
    }
    let markLineSeries = []
    let yMax = options ? options.yAxis.max : this.myChart.getModel().getComponent('yAxis', 0).axis.scale._extent[1];
    let yMin = options ? options.yAxis.min : this.myChart.getModel().getComponent('yAxis', 0).axis.scale._extent[0];
    let isRecomputerYmax = false
    if (this.selectedLegend.length) {
      let markLineData = []
      let dataArr = Array.from(this.selectedLegend, i => (this.echartData.data.find(ii => ii.name == i).source))
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
        let isLSL = true
        this.selectedLegend.forEach(i => {
          if (i.split('_')[0] !== '拉索力') {
            isLSL = false
          }
        })

        if (
          this.selectedLegend.length == 1 || isLSL
        ) {
          // VDI只能显示一条曲线的，即grid里面只有一条series
          // t同时要判断这条数据是否有报警定义
          let dataSourse = this.echartData.data.find(ii => ii.name == this.selectedLegend[0])
          if (dataSourse.VDI && dataSourse.VDI.length) {
            markLineData.push({
              name: 'VDI_Ⅱ',
              yAxis: Number(dataSourse.VDI[0]),
              lineStyle: {
                color: this.getMarkLineColor('VDI_Ⅱ'),
                type: 'solid'
              },
              label: {
                show: false,
                backgroundColor: 'rgba(0, 0, 0, 0.6)',
                formatter: `${dataSourse.VDI[0]}`,
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
              name: 'VDI_Ⅲ',
              yAxis: Number(dataSourse.VDI[1]),
              lineStyle: {
                color: this.getMarkLineColor('VDI_Ⅲ'),
                type: 'solid'
              },
              label: {
                show: false,
                backgroundColor: 'rgba(0, 0, 0, 0.6)',
                formatter: `${dataSourse.VDI[1]}`,
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
          } else {
            //  alert('无报警定义！')
          }
        } /* else {
          return //alert('只支持一条趋势的VDI显示')
        } */
      }
      markLineSeries.push({
        id: 'markline_level',
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
      })
    } else {
      return
    }
    // 首次初始化图谱，option内增加VDIline，减少一次setOption耗时，优化性能
    if (isInitShowVDI) {
      return {
        ...options,
        series: [
          ...series.filter(i => i.name !== 'markline_level'),
          ...markLineSeries
        ],
        yAxis: isRecomputerYmax ? [
          {
            ...yAxis[0],
            min: yMin,
            max: parseFloat(yMax)
          }
        ] : [
          {
            ...yAxis[0],
            min: null,
            max: null
          }
        ]
      }
    }
    this.myChart.setOption({
      series: [
        ...series.filter(i => i.name !== 'markline_level'),
        ...markLineSeries
      ],
      yAxis: isRecomputerYmax ? [
        {
          ...yAxis[0],
          min: yMin,
          max: parseFloat(yMax)
        }
      ] : [
        {
          ...yAxis[0],
          min: null,
          max: null
        }
      ]
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

  // 自定义图谱的大小尺寸响应事件
  resize() {
    this.myChart.resize()
  }

  // 上级控制工具条功能
  // 根据传入的type参数，执行不同的操作
  toolboxFeatures(type, content) {
    switch (type) {
      case 'reset':
        this.reset()
        break
      case 'min':
      case 'max':
      case 'avg':
      case 'mid':
      case 'VDI':
        this.operaterMarkline(type)
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
      case 'wavepointer':
        this.isShowWavePointer = !this.isShowWavePointer
        this.showWavePointer(this.isShowWavePointer)
        break
      default:
        break
    }
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
