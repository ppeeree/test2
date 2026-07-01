import * as echarts from 'echarts'
import {
  colorSeires,
  computedMarkValue,
  debounce,
  imageBase64Data,
  echartsYaxisLabelFormatter,
} from '../../tools/tool.js'
import { YBInstance } from '../../tools/YBLine/clickWay/YBInstance.js'
import { Toolbox } from '../../tools/command/toolbox.js'
import { getOptions } from '../../commonJs/optionConfig.js'
let chartPaddingTop = 40
let chartPaddingBottom = 70
let chartGriBwn = 30
let colorList = ['#4175B3', '#5E3B80', '#59A753', '#3677D6', '#106C11', '#5090EE']
let shapes = ['circle', 'roundRect', 'triangle', 'diamond', 'pin', 'arrow']

// 此组件为时域分析
// 输入为echart对象，输出为工具的操作，及数据的加载
// 轴承故障诊断显示图谱
export class AttachResultLineChart {
  constructor(echart, echartData, datazoomEvent, { theme, isAddNote, isSetY, isSetX, isDataDown, isStacked, isShuang,
    isBei,
    isBian,
    isGap,
    isFullData, // 是否全量数据 ,不需要缩放的时候请求数据
    isShowTitle
  } = {}) {
    // ...
    this.datazoomEvent = datazoomEvent
    this.myChart = echart // echart 实例对象
    this.themeType = theme || 'light' // 'dark'
    this.legendData = [] // lengend 数据
    this.echartData = echartData // 图表数据源
    this.selectedLegend = [] // 高亮选中的legend
    this.isFullData = typeof (isFullData) == 'undefined' ? false : isFullData
    this.isShowTitle = typeof (isShowTitle) == 'undefined' ? true : isShowTitle
    this.allFaultAndAdvise = {} // 故障维修建议字典表

    this.rightClickedYBLineId = '' // 右键菜单选中的游标线Id

    this.dataZoomDataList = []

    this.selectedPointList = []

    this.Toolbox = new Toolbox(this.myChart, this.themeType, {
      isFullData: this.isFullData,
      isYB: true,
      isZoomY: false, // Y轴放大
      isSetX: typeof (isSetX) == 'undefined' ? true : isSetX, // X轴范围设置
      isStacked: typeof (isStacked) == 'undefined' ? true : isStacked, // 是否设置layout
      isSetY: typeof (isSetY) == 'undefined' ? true : isSetY, // Y轴范围设置
      isAddNote: typeof (isAddNote) == 'undefined' ? true : isAddNote, // 增加备注
      isDataDown: typeof (isDataDown) == 'undefined' ? true : isDataDown, // 数据下载
      YBList: {
        isSingle: typeof (isSingle) == 'undefined' ? true : isSingle,
        isShuang: typeof (isShuang) == 'undefined' ? true : isShuang,
        isBei: typeof (isBei) == 'undefined' ? false : isBei,
        isBian: typeof (isBian) == 'undefined' ? false : isBian,
        isGap: typeof (isGap) == 'undefined' ? true : isGap,
        isPeak: true,
      },
      YBEvent: this.YBOperate.bind(this)
    },
      this.hotUpdate.bind(this),
    )
    this.myYB = new YBInstance(this.myChart, this.hotUpdate.bind(this), this.getYBPointer.bind(this))
    this.initData(echartData, echart)
  }

  // 返回图谱的base64数据格式
  imageBase64Data(isExport) {
    return imageBase64Data(this.myChart, this.selectedLegend, isExport)
  }
  // 放大缩小响应，游标点插入
  respondDatazoom(dataZoom) {
    this.myChart.dispatchAction({
      type: 'takeGlobalCursor',
      key: 'dataZoomSelect',
      dataZoomSelectActive: false
    });
    this.myChart.showLoading({
      textColor: this.themeType == 'light' ? '#000' : '#fff',
      maskColor: this.themeType == 'light' ? 'rgba(238, 238, 238, 0.5)' : 'rgba(0, 0, 0, 0.3)',
      zlevel: 20,
    })
    this.handleTooltipBlock()
    this.datazoomEvent(dataZoom)
    // 方案1：放大回退重计算游标线的点，不在样本中，插入点
    // 添加了游标线
    /* if (isExist) {
      let insertPointer = []
      let valueObj = {}
      lineArr.forEach(item => {
        if (item.xAxis > startValue && item.xAxis < endValue) {
          if (!newsource.find(i => i[0] == item.xAxis)) {
            insertPointer.push(item.value)
          }
        }
        if (valueObj[item.name]) {
          valueObj[item.name].push(item.value)
        } else {
          valueObj[item.name] = [item.value]
        }
      })
      if (insertPointer.length) {
        newsource = [...newsource, ...insertPointer].sort((a, b) => a[0] - b[0])
      }
      this.myChart.setOption({
        dataZoom: dataZoom,
        series: [{
          id,
          data: newsource
        }]
      })
      let graphicList = initAllGraphicPosition(valueObj, options, this.myChart)
      this.myChart.setOption({
        graphic: graphicList,
      })
    } else {
      this.myChart.setOption({
        dataZoom: dataZoom,
        series: [{
          id,
          data: newsource
        }]
      })
    } */
    // 方案2：放大回退等操作，清除游标线
    /*  this.YBOperate('清空游标')
     this.myChart.setOption({
       dataZoom: dataZoom,
       series: [{
         id,
         data: newsource
       }]
     })
     if (this.Toolbox.dataZoomType != '') {
       this.myChart.dispatchAction({
         type: 'takeGlobalCursor',
         key: 'dataZoomSelect',
         dataZoomSelectActive: true
       });
     }
     this.myChart.hideLoading() */
  }
  respondDatazoomData(dataZoom, echartData) {
    this.echartData = { ...this.echartData, ...echartData }
    let newSeries = []
    let options = this.myChart.getOption()
    for (let i = 0; i < echartData.data.length; i++) {
      const { source, id } = echartData.data[i]
      let marklineReComputer = {}
      let unit = options.series.find(i => i.id == id)
      if (unit.markLine.data.length) {
        marklineReComputer = this.myYB.reDrawLine(unit, source)
      }
      newSeries.push({
        id,
        data: source,
        ...marklineReComputer
      })
    }
    // 放大回退等操作，清除游标线
    // this.YBOperate('清空游标')
    const { start, end, startValue, endValue } = dataZoom
    this.myChart.dispatchAction({
      type: 'dataZoom',
      start,
      end,
      startValue,
      endValue,
    })
    this.myChart.setOption({
      series: [...newSeries]
    })
    if (this.Toolbox.dataZoomType != '') {
      this.myChart.dispatchAction({
        type: 'takeGlobalCursor',
        key: 'dataZoomSelect',
        dataZoomSelectActive: true
      });
    }
    if (this.selectedPointList.length) {
      for (let i = 0; i < this.selectedPointList.length; i++) {
        let arr = this.selectedPointList[i].split('_')
        this.myChart.dispatchAction({
          type: 'unselect',
          seriesId: arr[0],
          dataIndex: arr[1]
        })
      }
      this.selectedPointList = []
    }
    this.myChart.hideLoading()
  }
  // 工具条组件中调用或者修改实例属性
  hotUpdate(data, dataZoom) {
    if (data) {
      for (let key in data) {
        this[key] = data[key]
        if (key === 'dataZoomDataList' && dataZoom) {
          if (this.isFullData) {
            this.myChart.dispatchAction({
              type: 'dataZoom',
              start: dataZoom[0].start,
              end: dataZoom[0].end,
              startValue: dataZoom[0].startValue,
              endValue: dataZoom[0].endValue,
            })
          } else {
            this.respondDatazoom(dataZoom[0])
          }
        }
      }
    } else {
      return {
        selectedLegend: this.selectedLegend,
        dataZoomDataList: this.dataZoomDataList,
        rightClickedYBLineId: this.rightClickedYBLineId,
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
    let { dimensions, xAxisType, yAxisType, titleText, data } = this.echartData
    let legendData = []
    data.map(item => {
      legendData.push(item.name)
    })
    this.echartData.legendData = legendData
    this.legendData = legendData
    this.selectedLegend = [legendData[0]]
    const { titleStyle, backgroundColor, legendStyle, gridStyle, xAxisStyle, yAxisStyle, } = getOptions(this.themeType)
    let legendItemWidth = Math.round((this.myChart.getWidth() - 60) / legendData.length)
    const { visualMapList, seriesData } = this.getSeries(data)
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
      grid: {
        ...gridStyle,
        left: 70,
        right: 30,
      },
      backgroundColor,
      color: colorSeires,
      title: {
        ...titleStyle,
        text: titleText,
        show: this.isShowTitle,
      },
      toolbox: {
        id: 'toolbox',
        show: false,
      },
      xAxis: {
        ...xAxisStyle,
        name: dimensions[0],
        type: xAxisType || 'value',
        nameLocation: 'center',
        nameGap: 30,
        min: 'dataMin', //0, 放大后，清除游标操作，重新渲染X轴最小值为0bug
        max: 'dataMax',
        /* max: data.find(i => i.name == this.selectedLegend[0]).xAxisMax, */
        axisLabel: {
          ...xAxisStyle.axisLabel,
          width: 100,
        },
      },
      yAxis: {
        type: yAxisType || 'value',
        alignTicks: true,
        scale: true,
        name: dimensions[1],
        ...yAxisStyle,
        nameLocation: 'center',
        nameGap: 45,
        nameRotate: 90,
        axisLabel: {
          ...yAxisStyle.axisLabel,
          formatter: (val) => {
            return echartsYaxisLabelFormatter(val)
          }
        },
      },
      tooltip: {
        show: true,
        trigger: 'axis',//'item',
        /*  axisPointer: {
           type: "line",
           axis: 'x'
         }, */
        formatter: function (params) {
          let str = ''
          if (params.length) {
            str += `<p style='text-align:center;margin-bottom:5px;border-bottom:1px solid #ccc'> ${dimensions[0] || 'X'}: ${params[0].value[0]}</p>`
            params.forEach(item => {
              str +=
                item.marker +
                '&nbsp;' +
                `<span style='font-weight:bold'>` +
                item.value[1] +
                `</span>` +
                '<br/>'
            })
          }
          return str

        },
        textStyle: {
          color: '#1A1A1A',
          fontSize: 12
        }
      },
      legend: {
        ...legendStyle,
        selectedMode: 'single', // 单选
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
        width: "90%",
        icon: "circle",//显示样式
        bottom: 0,
        left: "center",
        data: legendData || []
      },
      dataZoom: [
        /*  {
           type: 'inside',
           id: 'Xinside',
           start: 0,
           end: 100,
           xAxisIndex: [0],
           moveOnMouseMove: false,
           zoomOnMouseWheel: false,
           moveOnMouseWheel: false,
         }, */
        /* {
          id: 'Xslider',
          type: 'slider',
          realtime: false,
          xAxisIndex: [0],
          zoomLock: true,
          // filterMode: 'empty',
          start: 0,
          end: 100,
          minValueSpan: 0,
          maxValueSpan: 120,
          height: 5,
          bottom: 30,
          orient: "horizontal",
        },
        {
          type: 'slider',
          id: 'Yslider',
          realtime: false,
          orient: "vertical",
          width: 5,
          right: 15,
          // // height: '70%'
          top: 50,
          bottom: 70,
          yAxisIndex: [0],
          zoomLock: true,
          // filterMode: 'empty'
        } */
      ],
      series: seriesData,
      visualMap: visualMapList
    }
    this.initOperate()
    this.myChart.setOption(chartData)
    if (visualMapList.length) {
      let obj = visualMapList.find(i => i.id == this.selectedLegend[0])
      if (obj && obj.categories && obj.categories.length == 1) {
        this.adviseDom(true, this.allFaultAndAdvise[obj.categories[0]], obj.categories[0])
      }
    }
  }

  // 组装series
  getSeries(data) {
    let seriesData = []
    let visualMapList = []
    let allFaultAndAdvise = {}
    data.length &&
      data.forEach((item, index) => {
        const { name, id, source, markList } = item
        let categoriesList = []
        let symbolShape = {}
        let faultData = []
        if (markList.length) {
          markList.forEach((item, index) => {
            let { data, faultName, intervalFrequency, maintenanceProposal } = item
            let name = `${faultName}：${intervalFrequency[0]}${intervalFrequency[1] ? '-' + intervalFrequency[1] : ''}Hz`
            categoriesList.push(name)
            allFaultAndAdvise[name] = maintenanceProposal
            symbolShape[name] = shapes[index]
            data.forEach(ii => {
              faultData.push({
                value: [...ii, name],
                /*   emphasis: {
                    label: {
                      show: true,
                      position: 'top',
                      distance: 2,
                      formatter: (val) => {
                        return val.value[0]
                      }
                    }
                  }, */
                label: {
                  show: true,
                  position: 'top',
                  color: this.themeType == 'light' ? '#999' : '#eee',
                  distance: 2,
                  formatter: (val) => {
                    return val.value[0]
                  }
                },
              })
              // faultData.push([...ii, name])
            })
          })
        }
        seriesData.push({
          type: 'line',
          /*    symbol: 'none',
             symbolSize: 0, */
          showSymbol: false,
          animation: false,
          animationDurationUpdate: 0,
          data: source, //computedShowDataPoint(this.myChart, this.echartData.xInterval, [0, 100], source),//[],//newsource, // newsource, //Array.isArray(source[0]) ? source : [],
          color: colorSeires[index % colorSeires.length],
          lineStyle: {
            width: 1
          },
          name: name,
          id: id,
          // zlevel: 3,
          // 点击数据点显示XY轴数据配置
          /*  label: {
             show: false
           }, */
          /*  select: {
             disabled: false,
             label: {
               show: true,
               position: ['100%', '-200%'],
               color: '#000',
               offset: [5, 0],
               lineHeight: 16,
               padding: 5,
               borderWidth: 1,
               borderColor: '#eee',
               zIndex: 10,
               formatter: "{@[0]}\n{@[1]}"
             },
             itemStyle: {
               shadowColor: '#000',
               shadowBlur: 5
             }
           },
           selectedMode: 'multiple', 
          emphasis: {
            disabled: true,
          },*/
          // end
          markLine: {
            precision: 4,
            animation: false,
            symbol: 'none',
            label: {
              show: false,
              fontSize: 12,
              // fontWeight: 'bolder',
              // backgroundColor: 'rgba(0,0,0,0.5)',
              // color: '#fff',
              position: 'end',
              distance: [5, 0],
              // padding: [5, 10, 5, 10]
            },
            emphasis: {
              disabled: true
            },
            data: []
          },
          markPoint: {
            animation: false,
            symbolSize: 5,
            symbol: 'circle',
            itemStyle: {
              color: 'transparent',
              borderWidth: 2
            },
            data: []
          }
        })
        seriesData.push({
          name: name,
          type: 'scatter',
          id: id + 'custom',
          // zlevel: 1,
          animationDurationUpdate: 0,
          encode: {
            x: 0,
            y: 1
          },
          data: faultData,
        })
        categoriesList.length ? visualMapList.push({
          id: name,
          show: index == 0,
          orient: 'vertical',
          left: 71,
          top: 50,
          textGap: 5,
          textStyle: {
            color: this.themeType == 'light' ? '#999' : '#fff',
          },
          backgroundColor: this.themeType == 'light' ? 'rgba(238,238,238,0.4)' : 'rgba(0,0,0,0.4)',
          padding: [5, 10],
          hoverLink: true,
          categories: categoriesList,
          seriesIndex: index * 2 + 1,
          dimension: 2,
          zlevel: 20,
          // showLabel: true,
          inRange: {
            color: colorList,
            symbolSize: 14,
            symbol: symbolShape,
            opacity: 1,
          },
          outOfRange: {
            color: 'rgba(189,200,200,0.2)',
            symbolSize: 14,
            symbol: symbolShape,
            opacity: 0,
          }
        }) : null
      })
    this.allFaultAndAdvise = allFaultAndAdvise
    return { seriesData, visualMapList }
  }


  // 更新图谱上的故障标记信息
  updatedMarkInfo(markInfoList, noRecomputedVisual) {
    let visualMapList = []
    let customSeriesData = []
    let allFaultAndAdvise = {}
    let { series, visualMap } = this.myChart.getOption()
    if (markInfoList.length) {
      markInfoList.forEach((item, j) => {
        let { id, name, markList, source } = item
        if (!source) {
          source = series.find(i => i.id == id).data
        }
        let categoriesList = []
        let symbolShape = {}
        let seriesData = { id: id + 'custom', data: [] }
        markList.forEach((ii, index) => {
          const { data, faultName, intervalFrequency, maintenanceProposal } = ii
          let name = `${faultName}：${intervalFrequency[0]}${intervalFrequency[1] ? '-' + intervalFrequency[1] : ''}Hz`
          categoriesList.push(name)
          allFaultAndAdvise[name] = maintenanceProposal
          symbolShape[name] = shapes[index]
          data.forEach(i => {
            // seriesData.data.push([...i, name]
            seriesData.data.push({
              value: [...i, name],
              label: {
                show: true,
                position: 'top',
                distance: 2,
                color: this.themeType == 'light' ? '#999' : '#eee',
                formatter: (val) => {
                  return val.value[0]
                }
              },
            })
          }
          )
        })
        customSeriesData.push(seriesData)
        !noRecomputedVisual && visualMapList.push({
          id: name,
          show: j == 0,
          orient: 'vertical',
          left: 71,
          top: 50,
          textGap: 5,
          backgroundColor: this.themeType == 'light' ? 'rgba(238,238,238,0.4)' : 'rgba(0,0,0,0.4)',
          padding: [5, 10],
          hoverLink: true,
          categories: categoriesList,
          // zlevel: 20,
          seriesIndex: j * 2 + 1,
          dimension: 2,
          // showLabel: true,
          inRange: {
            color: colorList,
            symbolSize: 14,
            symbol: symbolShape,
            opacity: 1,
          },
          outOfRange: {
            color: 'rgba(189,200,200,0.3)',
            symbolSize: 14,
            symbol: symbolShape,
            opacity: 0,
          }
        })
      })
    } else {
      let seriesData = series.filter(item => item.id.includes('custom'))
      if (seriesData.length) {
        try {
          customSeriesData = seriesData.map(item => {
            visualMap.length && visualMapList.push({
              id: item.name,
              categories: [],
            })
            if (item.data.length) {
              return {
                ...item,
                data: []
              }
            } else {
              throw 'err'
            }

          })
        } catch {
          console.log('err')
        }

      }
    }
    // 默认显示
    /*  if (Object.values(allFaultAndAdvise).length == 1) {
       this.adviseDom(true, Object.values(allFaultAndAdvise)[0])
     } */
    if (!noRecomputedVisual) {
      this.allFaultAndAdvise = allFaultAndAdvise
      this.myChart.setOption({
        series: customSeriesData,
        visualMap: visualMapList
      })
    } else {
      this.myChart.setOption({
        series: customSeriesData,
      })
    }
    // 初始化是否显示故障建议
    this.adviseDom(false)
    if (visualMapList.length) {
      let obj = visualMapList.find(i => i.id == this.selectedLegend[0])
      if (obj && obj.categories && obj.categories.length == 1) {
        this.adviseDom(true, this.allFaultAndAdvise[obj.categories[0]], obj.categories[0])
      }
    }
  }

  // 建议提示框
  adviseDom(visible, str, selectedCategories) {
    // 展示选中项的数据点lable
    /*    let { series } = this.myChart.getOption()
       let changeSeries = series.find(i => i.name == this.selectedLegend[0] && i.id.includes('custom'))
       if (visible) {
         changeSeries.data = changeSeries.data.map(item => {
           return {
             ...item,
             label: {
               ...item.label,
               show: item.value[2] == selectedCategories
             }
           }
         })
       } else {
         changeSeries.data = changeSeries.data.map(item => {
           return {
             ...item,
             label: {
               ...item.label,
               show: false
             }
           }
         })
       }
       this.myChart.setOption({
         series: [
           changeSeries
         ]
       }) */

    let id = this.myChart.id + 'advise_tooltip'
    let dom = document.getElementById(id)
    if (!visible && dom == null) {
      return
    }
    if (dom == null) {
      dom = document.createElement('div')
      dom.setAttribute('id', id)
      dom.classList.add('advise_tooltip')
      dom.classList.add('advise_tooltip_' + this.themeType)
      let spanDom = document.createElement('span')
      spanDom.innerHTML = 'X'
      spanDom.setAttribute('class', 'advise_tooltip_closeicon')
      spanDom.onclick = function (e) {
        e.target.parentElement.style.display = 'none'
        return false
      }
      dom.appendChild(spanDom)
      dom.insertAdjacentHTML('afterbegin', `<p></p>`)
      this.myChart.getDom().appendChild(dom)
    }
    if (visible && str.length) {
      dom.firstChild.innerHTML = str
      dom.style.display = 'block'
    } else {
      dom.style.display = 'none'
    }

  }

  // 初始化后echarts的监听事件:放大事件，双击事件，图例改变事件，点击事件，右键菜单事件
  initOperate() {
    this.myChart.off('datazoom')
    this.myChart.on('datazoom', param => {
      if (param['batch']) {
        if ('startValue' in param.batch[0]) {
          this.dataZoomDataList = [...this.dataZoomDataList, param.batch]
          this.Toolbox.updateBackDomStyle(this.dataZoomDataList)
          !this.isFullData && this.respondDatazoom(param.batch[0])
        }
      }
    })
    this.myChart.off('legendselectchanged')
    this.myChart.on('legendselectchanged', params => {
      const { selected } = params
      this.selectedLegend = []
      Object.keys(selected).forEach(i => {
        if (selected[i]) {
          this.selectedLegend.push(i)
        }
      })
      let option = this.myChart.getOption()
      let { visualMap, graphic, grid } = option
      let categoriesList = []
      let newVisualMap = visualMap.map((item, index) => {
        if (item.id == this.selectedLegend[0]) {
          categoriesList = item.categories ? item.categories : []
        }
        return { ...item, show: item.id == this.selectedLegend[0] }
      })
      /*     let newGraphic = []
          if (graphic && graphic.length) {
            let topNumber = grid[0].top
            let gridHeight = grid[0].height
            graphic[0].elements.forEach(i => {
              let name = this.echartData.data.find(item => item.id == i.info).name
              newGraphic.push({
                ...i,
                top: topNumber,
                shape: {
                  x1: 0,
                  y1: this.selectedLegend.includes(name) ? topNumber : 500,
                  x2: 0,
                  y2: this.selectedLegend.includes(name) ? (topNumber + gridHeight) : 500
                },
              })
            })
          } */
      this.myChart.setOption({
        visualMap: newVisualMap,
        // graphic: newGraphic
      }, { lazyUpdate: true })
      if (categoriesList.length == 1) {
        this.adviseDom(true, this.allFaultAndAdvise[categoriesList[0]], categoriesList[0])
      } else {
        this.adviseDom(false)
      }
    })
    // ==start 原游标方案：双击图谱，缩放还原，
    /*  this.myChart.getZr().off('dblclick')
     this.myChart.getZr().on('dblclick', () => {
       if (this.dataZoomDataList.length) {
         this.respondDatazoom({ start: 0, end: 100 })
       } else { return }
       this.dataZoomDataList = []
       this.Toolbox.updateBackDomStyle(this.dataZoomDataList)
     })
     this.myChart.off('click')
     this.myChart.on('click', param => {
       if (param.componentType == "graphic") {
         let id = param.event.target.id
         this.myYB.lastYBLine = id
         this.myYB.lastYBType = id.split('_')[0]
       }
       if (param.componentType == 'series') {
         let { transform } = param.event.target
         this.handleTooltipBlock(param, transform.slice(-2))
       }
     }) */
    // ====end
    // ==start 新游标方案：双击axisLIne，实现游标移动，点击缩放还原图标，进行图谱还原
    let hightLightData = {}
    this.myChart.off('highlight')
    this.myChart.on('highlight', param => {
      hightLightData = param
    })
    this.myChart.getZr().off('dblclick')
    this.myChart.getZr().on('dblclick', (param) => {
      if (param.topTarget && param.topTarget.type == 'Line') {
        this.myYB.moveYBLine(hightLightData.batch)
      }
    })
    let clickTimer = null;
    this.myChart.off('click')
    this.myChart.on('click', param => {
      if (clickTimer) clearTimeout(clickTimer);
      clickTimer = setTimeout(() => {
        // 单击事件的处理逻辑
        if (param.componentType == "markLine" && param.seriesName !== 'markline_level') {
          let { id } = param.data
          this.myYB.changeLastYBLine(id)
          this.myYB.lastYBType = id.split('_')[0]  //  "beipinGroup0_1"
        }
        if (param.componentType == 'series') {
          let { transform } = param.event.target
          this.handleTooltipBlock(param, transform.slice(-2))
        }
        // 清除定时器
        clickTimer = null;
      }, 500); // 500毫秒可以调整为适合你应用的值
    })
    this.myChart.off('downplay')
    this.myChart.on('downplay', param => {
      debounce(() => {
        this.myYB.hideTooltip()
      }, 300)()
    })
    this.myChart.off('mousemove')
    this.myChart.on('mousemove', param => {
      debounce(() => {
        if (param.componentType == 'markLine' && param.seriesName !== 'markline_level') {
          this.myYB.onBDLineMouseover(param)
        }
      }, 300)()
    })
    // ==end 新游标方案：
    // visual map切换监听
    this.myChart.off('datarangeselected')
    this.myChart.on('datarangeselected', param => {
      let trueArray = Object.keys(param.selected).filter(i => param.selected[i])
      if (trueArray.length == 1) {
        this.adviseDom(true, this.allFaultAndAdvise[trueArray[0]], trueArray[0])
      } else {
        this.adviseDom(false)
      }
    })
    let dom = this.myChart.getDom()
    dom.onkeydown = null
    dom.setAttribute('tabindex', '1')
    dom.onkeydown = (e) => {
      // e.preventDefault(); 键盘输入事件会消除掉
      this.keydown(e)
    }
  }
  /**
  * @description  键盘事件监听
  * @param {*} e 事件对象
  */
  keydown(e) {
    //事件对象兼容
    let e1 = e || event || window.event || arguments.callee.caller.arguments[0]
    //键盘按键判断:左箭头-37;上箭头-38；右箭头-39;下箭头-40
    //左
    if (e1 && e1.keyCode == 37) {
      // 按下左箭头,每点击一次，向左拖动一个数据点
      this.myYB.keyChange('left')
    } else if (e1 && e1.keyCode == 39) {
      // 按下右箭头，每点击一次，向右拖动一个数据点
      this.myYB.keyChange('right')
    }
  }


  /**
  * @description  点击事件，对应按钮事件分派
  * @param {*} title 点击的按钮title
  * @param {*} liDom 点击的按钮DOM, 进行样式修改
  * @param {*} nodes 放大互斥的按钮集合，进行样式修改
  */
  YBOperate(title) {
    if (title == '清空游标') {
      // 清空游标，同时清除峰值
      this.myYB.closeAllYB()
      this.rightClickedYBLineId = ''
      if (this.Toolbox.dataZoomType != '') {
        this.myChart.dispatchAction({
          type: 'takeGlobalCursor',
          key: 'dataZoomSelect',
          dataZoomSelectActive: true
        });
      }
    } else if (title == '双游标') {
      this.myYB.creatYBGroup('SYB', this.selectedLegend)
    } else if (title == '间隔游标') {
      this.myYB.creatYBGroup('JianGeYB', this.selectedLegend)
    } else if (title == '单游标') {
      this.myYB.creatYBGroup('SingleYB', this.selectedLegend)
    } else if (title == '倍频游标') {
      this.myYB.creatYBGroup('BeiPinYB', this.selectedLegend)
    } else if (title == '边频游标') {
      this.myYB.creatYBGroup('BianPinYB', this.selectedLegend)
    } else if (title == '峰值') {
      this.addPeakLine()
    } else if (title == 'setYbNum') {
      this.myYB.setYBlineNum(value)
    } else if (title == 'resetYbNum') {
      this.myYB.resetYBlineNum()
    } else if (title == 'delete') {
      this.myYB.closeYB(this.rightClickedYBLineId)
      this.rightClickedYBLineId = ''
      if (this.Toolbox.dataZoomType != '') {
        this.myChart.dispatchAction({
          type: 'takeGlobalCursor',
          key: 'dataZoomSelect',
          dataZoomSelectActive: true
        });
      }
    }
  }

  /**
  * 显示峰值线
  */
  addPeakLine() {
    let type = 'max'
    const { grid, series } = this.myChart.getOption()
    let markLineSeries = []
    let visibleLine = series.filter(i =>
      this.selectedLegend.findIndex(ii => ii == i.name && i.type == 'line') > -1
    )
    grid.forEach((item, index) => {
      // 获取每个grid里面显示的series的集合
      /*   let gridIndexSeries = visibleLine.filter(i =>
          i.xAxisIndex == index
        ) */
      if (visibleLine.length) {
        let markLineData = []
        let dataArr = Array.from(visibleLine, i => i.data)
        let result = computedMarkValue(dataArr, type)
        markLineData.push({
          name: type,
          yAxis: Number(result),
          lineStyle: {
            color: '#D900FF',
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
        markLineSeries.push({
          id: 'markline_level' + index,
          name: "markline_level",
          type: 'line',
          data: [],
          tooltip: {
            show: false
          },
          markLine: {
            precision: 8,
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
      ]
    })
  }

  // 取消监听事件，属性初始化
  destroyedInstance() {
    const dom = this.myChart.getDom()
    if (dom) dom.onkeydown = null
    this.myChart.off('datazoom')
    this.myChart.getZr().off('click')
    this.myChart.getZr().off('dblclick')
    this.SYB = {}
    this.SingleYB = {}
    this.JianGeYB = {}
    this.openYBList = []
    this.lastYBLine = ''
    this.lastYBType = ''
    this.YBLineDataList = []
    this.getYBPointer(this.YBLineDataList, '')
  }

  // 初始化游标数据
  initInstanceData() {
    Object.keys(this.SYB).forEach(key => {
      this.SYB[key] = null
      delete this.SYB[key]
    })
    Object.keys(this.SingleYB).forEach(key => {
      this.SingleYB[key] = null
      delete this.SingleYB[key]
    })
    Object.keys(this.JianGeYB).forEach(key => {
      this.JianGeYB[key] = null
      delete this.JianGeYB[key]
    })
    this.openYBList = []
    this.lastYBLine = ''
    this.lastYBType = ''
    this.YBLineDataList = []
    this.getYBPointer(this.YBLineDataList, '')
  }

  getYBPointer(YBArray) {
    this.Toolbox.YBDomContent(YBArray, this.selectedLegend)
  }

  reComputedGrid(options) {
    let newGrid = []
    let { grid } = options
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
    // 标记点清除
    this.handleTooltipBlock()
    this.myChart.resize()
    let options = this.myChart.getOption()
    let newGrid = this.reComputedGrid(options)
    //  let graphicList = this.myYB.resize(newGrid, options)
    let chartWidth = this.myChart.getWidth()
    let legendItemWidth = Math.floor((chartWidth - 60) / this.legendData.length)
    this.myChart.setOption({
      legend: {
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
        icon: "circle",
        bottom: 0,
        left: 'center',
        data: this.legendData || []
      },
      grid: newGrid,
      // graphic: graphicList,
    })
    // 工具条位置重新计算
    let dvBlock = document.getElementById(this.myChart.id + 'toolbox')
    dvBlock ? dvBlock.style.left = (chartWidth - dvBlock.getBoundingClientRect().width) / 2 + 'px' : null
  }

  handleTooltipBlock(data, position) {
    // 删除全部
    let parentDom = this.myChart.getDom()
    if (!data) {
      let doms = parentDom.getElementsByClassName('xy_tooltip')
      if (doms.length) {
        for (let i = doms.length - 1; i >= 0; i--) {
          parentDom.removeChild(doms[i])
        }
      }
      return
    }
    // 新增or删除某一项
    let { seriesId, dataIndex, value } = data
    let id = seriesId + dataIndex
    let dom = document.getElementById(id)
    if (dom) {
      parentDom.removeChild(dom)
      return
    }
    dom = document.createElement('div')
    dom.setAttribute('id', id)
    dom.classList.add('xy_tooltip')
    let chartWidth = this.myChart.getWidth()
    let positionWidth = position[0] + 5
    if ((positionWidth + 80) > chartWidth) {
      positionWidth = position[0] - 5 - 80
      dom.classList.add('xy_tooltip_left')
    }
    dom.style.left = positionWidth + 'px'
    dom.style.top = position[1] + 5 + 'px'
    dom.innerHTML = 'x:' + value[0] + '<br>' + 'y:' + value[1]
    parentDom.appendChild(dom)
  }

}




