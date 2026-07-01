import * as echarts from 'echarts'
import {
  colorSeires,
  computedMarkValue,
  imageBase64Data,
  debounce,
  echartsYaxisLabelFormatter,
} from '../../tools/tool.js'
import { Toolbox } from '../../tools/command/toolbox.js'
import { YBInstance } from '../../tools/YBLine/clickWay/YBInstance.js'
import { getOptions } from '../../commonJs/optionConfig.js'
let chartPaddingTop = 40
let chartPaddingBottom = 70
let chartGriBwn = 30

// 此组件为时域分析
// 输入为echart对象，输出为工具的操作，及数据的加载
export class TimeDomainChart {
  constructor(echart, echartData, datazoomEvent, { theme, isSetY, isSetX, isAddNote, isDataDown, isStacked, isShuang,
    isBei,
    isBian,
    isGap,
    isPeak,
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

    this.rightClickedYBLineId = '' // 右键菜单选中的游标线Id
    this.selectedPointList = []

    this.dataZoomDataList = []
    this.stackedType = 'overlay'
    this.isFullData = typeof (isFullData) == 'undefined' ? false : isFullData
    this.isShowTitle = typeof (isShowTitle) == 'undefined' ? true : isShowTitle
    this.Toolbox = new Toolbox(this.myChart, this.themeType, {
      isFullData: this.isFullData,
      isYB: true, // 显示游标
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
      this.changeStackedType.bind(this),
    )
    this.myYB = new YBInstance(this.myChart, this.hotUpdate.bind(this), this.getYBPointer.bind(this))
    this.initData(echartData, echart)
  }

  // 返回图谱的base64数据格式
  imageBase64Data(isExport) {
    return imageBase64Data(this.myChart, this.selectedLegend, isExport)
  }
  // 放大缩小响应，重新请求获取数据
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
  }
  // 缩放操作后数据重新响应绘制 
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
  initData(lineDataSourse, echart) {
    // console.log('chartInitStart:' + new Date().getTime())
    this.echartData = lineDataSourse || this.echartData
    const { dimensions, xAxisType, yAxisType, titleText, data } = this.echartData
    let legendData = []
    data.map(item => {
      legendData.push(item.name)
    })
    // this.echartData.data = newDataArr
    this.echartData.legendData = legendData
    this.legendData = legendData
    this.selectedLegend = legendData
    let relationOp = this.changeStackedType()
    const { titleStyle, legendStyle, backgroundColor } = getOptions(this.themeType)
    let legendItemWidth = Math.floor((this.myChart.getWidth() - 70) / legendData.length)
    let chartData = {
      animation: false,           // 关键
      animationDuration: 0,
      animationDurationUpdate: 0,
      large: true,                     // 开启大点模式（关键！）
      progressive: 50000,             // 分块渲染，每5万点一帧
      progressiveThreshold: 100000,   // >10万点时自动触发
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
      toolbox: {
        id: 'toolbox',
        show: false,
      },
      tooltip: {
        show: true,
        trigger: 'axis', // 'item',//'axis',
        confine: true,
        axisPointer: {
          type: 'line',
          axis: 'x',
          snap: true,
          label: {
            show: false
          },
          z: 15
        },
        formatter: function (params) {
          let str = ''
          if (params.length) {
            str += `<p style='text-align:center;margin-bottom:5px;border-bottom:1px solid #ccc'> ${dimensions[0] || 'X'}: ${params[0].value[0]}</p>`
            params.forEach(item => {
              let value = item.value ? item.value[1] : ''
              str +=
                `<p style='text-align:left'>` + item.marker +
                '&nbsp;' +
                `<span style='font-weight:bold'>` +
                value +
                `</span></p>`
            })
            return str
          }
        },
        textStyle: {
          color: '#1A1A1A',
          fontSize: 12
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
        width: "100%",
        icon: "circle",//显示样式
        bottom: 0,
        left: "center",
        data: legendData
      },
      ...relationOp,
      dataZoom: [
      ],
      series: [
        ...this.getSeries(data, 'overlay')
      ]
    }
    // console.log('chartInitStart1:' + new Date().getTime())
    this.initOperate()
    this.myChart.setOption(chartData)
    //  console.log('chartInitend:' + new Date().getTime())
  }
  // 组装series
  getSeries(data, stackedType) {
    let seriesData = []
    data.length &&
      data.forEach((item, index) => {
        const { name, id, source, xInterval } = item
        seriesData.push({
          type: 'line',
          /*   symbol: 'none',
            symbolSize: 0, */
          showSymbol: false,
          animation: false,
          animationDurationUpdate: 0,
          data: source,
          color: colorSeires[index % colorSeires.length],
          lineStyle: {
            width: 1
          },
          /*    select: {
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
                 formatter: "{@[0]}\n{@[1]}"
               },
               itemStyle: {
                 shadowColor: '#000',
                 shadowBlur: 5
               }
             },
             selectedMode: 'multiple',*/
          emphasis: {
            disabled: true
          },
          name: name,
          id: id,
          xAxisIndex: stackedType == 'overlay' ? 0 : index,
          yAxisIndex: stackedType == 'overlay' ? 0 : index,
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
      })
    return seriesData
  }

  // 图谱堆叠方式修改
  changeStackedType(type, theme) {
    let stackedType = type || 'overlay'
    this.stackedType = stackedType
    const { xAxisType, yAxisType, dimensions, data } = this.echartData
    let relationOp = {
      axisPointer: {
        link: { xAxisIndex: "all" },
      },
      dataZoom: [],
      grid: [],
      xAxis: [],
      yAxis: [],
      series: [],
      graphic: [],
    }
    const { gridStyle, xAxisStyle, yAxisStyle } = getOptions(theme || this.themeType)
    // let maxXais = this.echartData.data.map(i => i.xAxisMax)
    let xAxisBase = {
      ...xAxisStyle,
      type: xAxisType || 'value',//dataSoures.xAxisType || 'time',//category
      nameLocation: 'center',
      nameGap: 30,
      min: 'dataMin', // 0
      // max: Math.max(...maxXais),
      max: 'dataMax', // 避免多条曲线最大值不同的情况下，操作legend显示问题
      axisLabel: {
        ...xAxisStyle.axisLabel,
        width: 100,
      },
    };
    let yAxisBase = {
      type: yAxisType || 'value',
      alignTicks: true,
      scale: true,
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
      relationOp.grid = [{
        ...gridBase,
        top: chartPaddingTop,
        height: echartHeight - (chartPaddingTop + chartPaddingBottom)
      }]
      relationOp.yAxis = [{
        ...yAxisBase,
        name: yAxisName.length == 1 ? yAxisName[0] : '', // Y轴名称，每组数据的维度名称相同则显示，不同则不显示
        gridIndex: 0,
      }]
      relationOp.xAxis = [{
        ...xAxisBase,
        name: dimensions[0],
        gridIndex: 0
      }]

    } else if (stackedType == 'stacked') {
      let unitHeight = (echartHeight - chartPaddingTop - chartPaddingBottom - (this.selectedLegend.length - 1) * chartGriBwn) / this.selectedLegend.length
      if (this.selectedLegend.length) {
        this.selectedLegend.forEach((item, index) => {
          let { dimensions } = data.find(i => i.name == item)
          relationOp.grid.push({
            ...gridBase,
            top: index * unitHeight + (index * chartGriBwn) + chartPaddingTop,
            height: unitHeight
          })
          relationOp.xAxis.push({
            ...xAxisBase,
            name: index == this.selectedLegend.length - 1 ? dimensions[0] : '',
            gridIndex: index,
          })
          relationOp.yAxis.push({
            ...yAxisBase,
            name: dimensions[1],
            gridIndex: index
          })
        })
      } else {
        relationOp.grid = [{
          ...gridBase,
          top: chartPaddingTop,
          height: echartHeight - (chartPaddingTop + chartPaddingBottom)
        }]
        relationOp.yAxis = [{
          ...yAxisBase,
          name: yAxisName.length == 1 ? yAxisName[0] : '', // Y轴名称，每组数据的维度名称相同则显示，不同则不显示
          gridIndex: 0,
        }]
        relationOp.xAxis = [{
          ...xAxisBase,
          name: dimensions[0],
          gridIndex: 0
        }]
      }
    }
    let newSeries = []
    let newDataZoom = []
    let options = this.myChart.getOption()
    if (options) {
      const { series, graphic, dataZoom } = options
      series.forEach((item, index) => {
        if (item.name !== 'markline_level') {
          const obj = data.find(i => i.id == item.id)
          if (obj) {
            let axisIndex = 0
            if (stackedType == 'stacked') {
              axisIndex = this.selectedLegend.findIndex(i => i == item.name)
            }
            if (axisIndex == -1) { axisIndex = 0 }
            newSeries.push({
              ...item,
              xAxisIndex: axisIndex,
              yAxisIndex: axisIndex,
            })
          }
        }
      })
      /*   if (graphic && graphic.length) {
          graphic[0].elements.forEach(i => {
            let gridIndex = newSeries.find(item => i.info == item.id).xAxisIndex
            let topNumber = relationOp.grid[gridIndex].top || 0
            let gridHeight = relationOp.grid[gridIndex].height || 0
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
    relationOp.series = newSeries // this.getSeries(data, stackedType)
    // relationOp.graphic = newGraphic
    relationOp.dataZoom = newDataZoom
    return relationOp
  }
  // 初始化后echarts的监听事件:放大事件，双击事件，图例改变事件，点击事件，右键菜单事件
  initOperate() {
    /* this.myChart.on('finished', function () {
      console.log('finished:' + new Date().getTime())
    }); */
    this.myChart.off('datazoom')
    this.myChart.on('datazoom', param => {
      setTimeout(() => {
        if (param['batch']) {
          if ('startValue' in param.batch[0]) {
            this.dataZoomDataList = [...this.dataZoomDataList, param.batch]
            this.Toolbox.updateBackDomStyle(this.dataZoomDataList)
            !this.isFullData && this.respondDatazoom(param.batch[0])
          }
        }
      }, 300)
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
      let relationOp = this.changeStackedType(this.stackedType, this.themeType)
      let option = this.myChart.getOption()
      this.myChart.setOption({
        ...option,
        ...relationOp
      }, { notMerge: true, lazyUpdate: true })
    })
    // ==start 原游标方案：双击图谱，缩放还原，
    /*  this.myChart.getZr().off('dblclick')
     this.myChart.getZr().on('dblclick', () => {
       if (this.dataZoomDataList.length) {
         this.respondDatazoom({ start: 0, end: 100 })
       } else { return }
       // 工具栏放大数据存储清空
       this.dataZoomDataList = []
       this.Toolbox.updateBackDomStyle(this.dataZoomDataList)
       // end
     })
     this.myChart.off('click')
     this.myChart.on('click', param => {
       if (param.componentType == 'graphic') {
         let { id } = param.event.target
         this.myYB.lastYBLine = id
         this.myYB.lastYBType = id.split('_')[0]  //  "beipinGroup0_1"
       }
       if (param.componentType == 'series') {
         let { transform } = param.event.target
         this.handleTooltipBlock(param, transform.slice(-2))
       }
     }) */
    // ==end 原游标方案：双击图谱，缩放还原，
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
      // ctrl+点击legend 只选中当前项，其他全不选
      if (param.componentType == 'legend' && param.event.event.ctrlKey) {
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
        return
      }
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
  YBOperate(title, value) {
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
      this.selectedLegend.findIndex(ii => ii == i.name) > -1
    )
    grid.forEach((item, index) => {
      // 获取每个grid里面显示的series的集合
      let gridIndexSeries = visibleLine.filter(i =>
        i.xAxisIndex == index
      )
      if (gridIndexSeries.length) {
        let markLineData = []
        let dataArr = Array.from(gridIndexSeries, i => (series.find(ii => ii.name == i.name).data))
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
  resize(size) {
    this.myChart.resize({
      width: size?.width || 'auto',
      height: size?.height || 'auto',
    })
    this.handleTooltipBlock()
    let options = this.myChart.getOption()
    let newGrid = this.reComputedGrid(options)
    // let graphicList = this.myYB.resize(newGrid, options)
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
        icon: "circle",
        bottom: 0,
        left: 'center',
        data: this.legendData || []
      },
      grid: newGrid,
      //   graphic: graphicList,
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
    dom.innerHTML = 'x：' + value[0] + '<br>' + 'y：' + value[1]
    parentDom.appendChild(dom)
  }

}




