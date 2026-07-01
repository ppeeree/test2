import { JianGeYB } from './newjgyb.js'
import { SingleYB } from './single.js'
import { SYB } from './newsyb.js'
import { BianPinYB } from './newbipyb.js'
import { BeiPinYB } from './newbeipyb.js'
import { judgeYbExist, initAllGraphicPosition, getClosestNumber } from '../ybCommonTools.js'
import {
  debounce, accSub
} from '../../tool.js'

export class YBInstance {
  constructor(myChart, hotUpdate, getYBPointer) {
    this.myChart = myChart
    this.BeiPinlineNum = 5
    this.BianLineNum = 7
    this.JianGeLineNum = 5

    this.SingleYB = {}
    this.SYB = {}//实例对象
    this.JianGeYB = {}// 间隔游标
    this.BianPinYB = {}
    this.BeiPinYB = {}

    this.lastYBType = ''
    this.lastYBLine = ''
    this.openYBList = []

    this.groupNum = 0

    this.YBLineDataList = [] // 游标位置信息

    this.YBkey = {
      SYB: 'shuangGroup',
      JianGeYB: 'jiangeGroup',
      SingleYB: 'singleGroup',
      BeiPinYB: 'beipinGroup',
      BianPinYB: 'bianpinGroup',
    }
    // 事件
    this.getYBPointer = getYBPointer
  }

  setYBlineNum(value) {
    this.BeiPinlineNum = value.beipin ? Number(value.beipin) : 5
    this.BianLineNum = value.bianpin ? Number(value.bianpin) * 2 + 1 : 7
    this.JianGeLineNum = value.jiange ? Number(value.jiange) : 5
  }

  resetYBlineNum() {
    this.BeiPinlineNum = 5
    this.BianLineNum = 7
    this.JianGeLineNum = 5
  }

  // 创建游标实例
  // 创建游标分析组
  /**
   * 
   * @param {*} type 创建的游标类型
   * @param {*} selectedLegend 创建游标的选中曲线
   * @param {*} defaultLinePointerList 创建游标初始默认值
   */
  creatYBGroup(type, selectedLegend, defaultLinePointerList) {
    // 如果选择了曲线
    if (selectedLegend.length) {
      // 获取图表的配置项
      let options = this.myChart.getOption()
      // 获取图表的系列数据
      let { series } = options
      // 初始化游标类型和游标线
      let lastYBType = '', lastYBLine = ''
      // 遍历选择的曲线
      selectedLegend.forEach(item => {
        // 获取曲线的索引和曲线数据
        let obj, objIndex
        objIndex = series.findIndex(i => i.name == item)
        obj = series[objIndex]
        // 生成游标类型和游标线的名称
        let prefixName = this.YBkey[type] + this.groupNum
        this.openYBList.push(prefixName)
        lastYBType = prefixName
        lastYBLine = prefixName + '_0'
        this.groupNum++
        // 生成游标参数
        let params = { data: obj.data, seriesId: obj.id, yAxisIndex: obj.yAxisIndex || 0, options }
        // 根据游标类型创建游标
        switch (type) {
          case 'SYB':
            this.SYB[prefixName] = new SYB()
            this.SYB[prefixName].addYBLines(prefixName,
              this.myChart, params, this.changeYBPointerData.bind(this))
            break;
          case 'JianGeYB':
            this.JianGeYB[prefixName] = new JianGeYB(this.JianGeLineNum)
            this.JianGeYB[prefixName].addYBLines(prefixName,
              this.myChart, params, this.changeYBPointerData.bind(this))
            break;
          case 'SingleYB':
            this.SingleYB[prefixName] = new SingleYB()
            this.SingleYB[prefixName].addYBLines(prefixName,
              this.myChart, params, this.changeYBPointerData.bind(this))
            break;
          case 'BeiPinYB':
            this.BeiPinlineNum = defaultLinePointerList ? defaultLinePointerList.length : this.BeiPinlineNum
            this.BeiPinYB[prefixName] = new BeiPinYB(this.BeiPinlineNum)
            this.BeiPinYB[prefixName].addYBLines(prefixName,
              this.myChart, params, this.changeYBPointerData.bind(this), defaultLinePointerList)
            break;
          case 'BianPinYB':
            this.BianPinYB[prefixName] = new BianPinYB(this.BianLineNum)
            this.BianPinYB[prefixName].addYBLines(prefixName,
              this.myChart, params, this.changeYBPointerData.bind(this))
            lastYBLine = lastYBType + '_' + Math.floor(this.BianLineNum / 2)
            break;
        }
      })
      // 设置最后一个游标类型和游标线
      this.lastYBType = lastYBType
      // 设置最后一个游标线
      setTimeout(() => {
        this.changeLastYBLine(lastYBLine)
      }, 100)
      // this.lastYBLine = lastYBline
    } else {
      // 如果没有选择曲线，提示选择曲线
      alert('请选择一条曲线进行游标分析！')
    }
  }
  // toolbox: 清空游标线
  closeAllYB() {
    let options = this.myChart.getOption()
    let { series } = options
    if (!this.openYBList.length) {
      let marklineSeries = series.filter(i => i.name == 'markline_level')
      if (marklineSeries.length) {
        marklineSeries.map(i => i.markLine.data = [])
        let newSeries = series.filter(i => i.name != 'markline_level')
        this.myChart.setOption({
          series: [...newSeries, ...marklineSeries]
        })
      } else {
        return;
      }
    }
    if (!this.openYBList.length && !series.filter(i => i.name !== 'markline_level').length) { return; }
    let newSeries = []
    series.forEach(item => {
      let obj = item
      // if (item.name != 'markline_level') {
      if (item && item.markLine) {
        obj.markLine.data = []
      }
      if (item && item.markPoint) {
        obj.markPoint.data = []
      }
      //  }
      newSeries.push(obj)
    })
    this.myChart.setOption({
      ...options,
      series: newSeries //.filter(i => i.name !== 'markline_level'),
    }, { notMerge: true, replaceMerge: ['series'], lazyUpdate: true })
    // 数据重置
    this.initInstanceData()
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
    Object.keys(this.BeiPinYB).forEach(key => {
      this.BeiPinYB[key] = null
      delete this.BeiPinYB[key]
    })
    Object.keys(this.BianPinYB).forEach(key => {
      this.BianPinYB[key] = null
      delete this.BianPinYB[key]
    })
    this.openYBList = []
    //  this.lastYBLine = ''
    this.lastYBType = ''
    this.YBLineDataList = []
    this.lastYBLine = ''
    this.getYBPointer(this.YBLineDataList, '')
  }
  // toolbox:删除单组游标线
  closeYB(lineId) {
    let lineIdnew = lineId
    if (!lineIdnew || !lineIdnew.length) return;
    // lineID 格式：'shuangGroup0_0'
    let prefixName = lineIdnew.split('_')[0]
    let key = ''
    if (prefixName.includes('shuang')) {
      const { seriesId } = this.SYB[prefixName]
      this.closeYBLines(seriesId, prefixName, this.myChart)
      key = 'shuangGroup'
      this.SYB[prefixName] = null
      delete this.SYB[prefixName]
    } else if (prefixName.includes('jiange')) {
      const { seriesId } = this.JianGeYB[prefixName]
      this.closeYBLines(seriesId, prefixName, this.myChart)
      key = 'jiangeGroup'
      this.JianGeYB[prefixName] = null
      delete this.JianGeYB[prefixName]
    } else if (prefixName.includes('single')) {
      const { seriesId } = this.SingleYB[prefixName]
      this.closeYBLines(seriesId, prefixName, this.myChart)
      key = 'singleGroup'
      this.SingleYB[prefixName] = null
      delete this.SingleYB[prefixName]
    } else if (prefixName.includes('bei')) {
      const { seriesId } = this.BeiPinYB[prefixName]
      this.closeYBLines(seriesId, prefixName, this.myChart)
      key = 'beipinGroup'
      this.BeiPinYB[prefixName] = null
      delete this.BeiPinYB[prefixName]
    } else if (prefixName.includes('bian')) {
      const { seriesId } = this.BianPinYB[prefixName]
      this.closeYBLines(seriesId, prefixName, this.myChart)
      key = 'bianpinGroup'
      this.BianPinYB[prefixName] = null
      delete this.BianPinYB[prefixName]
    }
    this.YBLineDataList.forEach(i => {
      if (i.children[key].length) {
        let index = i.children[key].findIndex(i => i.name == prefixName)
        index > -1 && i.children[key].splice(index, 1)
      }
    })
    this.openYBList.splice(this.openYBList.indexOf(prefixName), 1)
    if (this.openYBList.length == 0) {
      this.YBLineDataList = []
    }
    if (this.lastYBType == prefixName) {
      this.resetMovedYB()
    }
    this.getYBPointer(this.YBLineDataList, this.lastYBType)
  }

  // toolbox:删除单组游标线：删除单组游标后，重新获取焦点的游标线
  resetMovedYB() {
    let arr = this.openYBList
    let lineId = ''
    if (arr.length) {
      this.lastYBType = arr[arr.length - 1]
      if (this.lastYBType.includes('shuang') || this.lastYBType.includes('jiange') || this.lastYBType.includes('single') || this.lastYBType.includes('bei')) {
        lineId = this.lastYBType + '_0'
      } else if (this.lastYBType.includes('bian')) {
        lineId = this.lastYBType + '_' + Math.floor(this.BianLineNum / 2)
      }
      this.changeLastYBLine(lineId)
    } else {
      this.lastYBType = ''
      this.lastYBLine = ''
    }

  }

  changeLastYBLine(afterLine) {
    //  if (this.lastYBLine == afterLine) { return }
    let options = this.myChart.getOption()
    let newSeries = options.series.map(item => {
      const { markLine } = item
      let newMarkLline = []
      if (markLine && markLine.data && markLine.data.length) {
        newMarkLline = markLine.data.map(i => {
          return {
            ...i,
            label: {
              ...i.label,
              backgroundColor: i.id == afterLine ? '#666' : ''
            }
          }
        })
      }
      return {
        ...item,
        markLine: {
          ...item.markLine,
          data: newMarkLline
        }
      }
    })
    this.myChart.setOption({
      series: newSeries
    })
    this.lastYBLine = afterLine
  }

  // 游标移动事件
  moveYBLine(dataArr/* data, seriesId, dataIndex */) {
    if (this.lastYBType == '') { return; }
    if (this.lastYBType.includes('bian')) {
      this.BianPinYB[this.lastYBType].onPointTranslation(this.lastYBLine, dataArr, this.myChart)
    } else if (this.lastYBType.includes('shuang')) {
      this.SYB[this.lastYBType].onPointTranslation(this.lastYBLine, dataArr, this.myChart)
    } else if (this.lastYBType.includes('jiange')) {
      this.JianGeYB[this.lastYBType].onPointTranslation(this.lastYBLine, dataArr, this.myChart)
    } else if (this.lastYBType.includes('single')) {
      this.SingleYB[this.lastYBType].onPointTranslation(this.lastYBLine, dataArr, this.myChart)
    } else if (this.lastYBType.includes('bei')) {
      this.BeiPinYB[this.lastYBType].onPointTranslation(this.lastYBLine, dataArr, this.myChart)
    }
  }
  // 鼠标聚焦游标线事件
  onBDLineMouseover(param) {
    const { event, data, seriesId, seriesIndex } = param
    // const { offsetX, offsetY } = event
    if (!data.name) return
    if (data.name == 'max') return
    let option = this.myChart.getOption()
    let { lineArr } = judgeYbExist(option, data.name, seriesId)
    let linePointer = lineArr.map(i => {
      return i.value
    })
    let { xAxis, yAxis } = option
    this.showYBTooltip(linePointer, { xunit: xAxis[0].name, yunit: yAxis[0].name }, data)
  }
  // 单根游标线的tooltip信息：隐藏
  hideTooltip() {
    let dom = document.getElementById(this.myChart.id + 'YBBlock')
    if (dom) {
      dom.style.display = 'none'
    }
  }
  // 单根游标线的tooltip信息：显示
  showYBTooltip(lineArr, { xunit, yunit }, lineData) {
    let YBDOM = document.getElementById(this.myChart.id + 'YBBlock')
    let { id, defaultValue, lineStyle, name } = lineData
    // defaultValue 后台获取的转频值
    let lineIndex = Number(id.split('_')[1])
    let lineColor = lineStyle.color
    let prefixName = name
    let htmlStr = ''
    let distance, ff //频率 计算方式：1/distance 取三位有效数字,只针对时域波形，频谱类不显示
    if (prefixName.includes('shuang') || prefixName.includes('jiange')) {
      distance = Math.abs(accSub(Number(lineArr[1][0]), Number(lineArr[0][0])))
      ff = (1 / distance).toPrecision(3)
      htmlStr = `<p>△X: <span>${distance}</span>&nbsp;${xunit == 'Hz' ? '' : `f：<span>${ff}</span>`
        } x：<span>${lineArr[lineIndex][0]}</span>&nbsp;${xunit} &nbsp;&nbsp;y：<span>${lineArr[lineIndex][1]}</span>&nbsp;${yunit}</p>`
    } else if (prefixName.includes('single')) {
      htmlStr = `<p>x：<span>${lineArr[lineIndex][0]}</span>&nbsp;${xunit} &nbsp;&nbsp;y：<span>${lineArr[lineIndex][1]}&nbsp;${yunit}</p>`
    } else if (prefixName.includes('bian')) {
      distance = Math.abs(accSub(Number(lineArr[1][0]), Number(lineArr[0][0])))
      ff = (1 / distance).toPrecision(3)
      htmlStr = `<p>中心频率：<span>${lineArr[Math.floor(lineArr.length / 2)][0]}</span>  △X: <span>${distance}</span>&nbsp; ${xunit == 'Hz' ? '' : `f：<span>${ff}</span>`
        }x：<span>${lineArr[lineIndex][0]}</span>&nbsp;${xunit} &nbsp;&nbsp;y：<span>${lineArr[lineIndex][1]}</span>&nbsp;${yunit}</p>`
    } else if (prefixName.includes('bei')) {
      htmlStr = `<p>基频：<span>${lineArr[0][0]}</span> &nbsp;&nbsp;<span>${lineIndex + 1}</span>倍频  x：<span>${lineArr[lineIndex][0]}</span>&nbsp;${xunit} &nbsp;&nbsp;y：<span>${lineArr[lineIndex][1]}</span>&nbsp;${yunit} ${defaultValue ? `&nbsp;&nbsp;转频：<span>${defaultValue}&nbsp;${xunit}</span>` : ''}</p>`
    }
    YBDOM.innerHTML = htmlStr
    YBDOM.style.display = 'block'
    /*  const { width, height } = YBDOM.getBoundingClientRect()
     const chartWidth = chart.getWidth()
     const chartHeight = chart.getHeight() */
    let positionX = 10, positionY = 15
    /*   if (width + x + 10 > chartWidth) {
        positionX = x - width - 10
      } */
    /*  if (height + y + 10 > chartHeight) {
       positionY = y - height - 10
     } */
    YBDOM.style.left = positionX + 'px'
    YBDOM.style.bottom = positionY + 'px'
    YBDOM.style.color = lineColor
  }

  /**
    * 游标拖拽的对应的位置坐标信息更新
    * @param {*} dataArr // 当前修改的一组游标的位置信息数字
    * @param {*} lineColor // 当前修改的游标线体颜色
    * @param {*} prefixName // 当前修改的游标name
    * @param {*} seriesId // 当前修改的游标对应的seriesId
    */
  changeYBPointerData(dataArr, lineColor, prefixName, seriesId, { name, color }) {
    let arr = ['shuangGroup', 'jiangeGroup', 'beipinGroup', 'bianpinGroup', 'singleGroup']
    let data = {
      dataList: dataArr,
      name: prefixName,
      lineColor: lineColor
    }
    if (this.YBLineDataList.length && this.YBLineDataList.find(i => i.seriesId == seriesId)) {
      let obj, sy
      this.YBLineDataList.forEach((i, index) => {
        if (i.seriesId == seriesId) {
          obj = i
          sy = index
        }
      })
      arr.forEach((i, index) => {
        if (prefixName.includes(i)) {
          let index = obj.children[i].findIndex(item => item.name == prefixName)
          if (index !== -1) {
            obj.children[i].splice(index, 1, data)
          } else {
            obj.children[i].push(data)
          }
        }
      })
      this.YBLineDataList[sy] = obj
    } else {
      let unit = {
        seriesId: seriesId,
        seriesName: "",
        seriesColor: "",
        children: {
          shuangGroup: [],
          beipinGroup: [],
          jiangeGroup: [],
          bianpinGroup: [],
          singleGroup: [],
        }
      }
      unit.seriesName = name
      unit.seriesColor = color
      arr.forEach((i, index) => {
        if (prefixName.includes(i)) {
          unit.children[i].push(data)
        }
      })
      this.YBLineDataList.push(unit)
    }
    debounce(() => {
      this.getYBPointer(this.YBLineDataList, prefixName)// 游标数据，最后一次操作的游标类型
    }, 100)()

  }


  /**
  * @description  键盘事件，游标移动方法调用
  * @param {*} direction 键盘移动方向:left/right
  */
  keyChange(direction) {
    if (this.lastYBType.includes('shuangGroup')) {
      this.SYB[this.lastYBType].translation(direction, this.lastYBLine, this.myChart)
    } else if (this.lastYBType.includes('jiangeGroup')) {
      this.JianGeYB[this.lastYBType].translation(direction, this.lastYBLine, this.myChart)
    } else if (this.lastYBType.includes('singleGroup')) {
      this.SingleYB[this.lastYBType].translation(direction, this.lastYBLine, this.myChart)
    } else if (this.lastYBType.includes('beipinGroup')) {
      this.BeiPinYB[this.lastYBType].translation(direction, this.lastYBLine, this.myChart)
    } else if (this.lastYBType.includes('bianpinGroup')) {
      this.BianPinYB[this.lastYBType].translation(direction, this.lastYBLine, this.myChart)
    }
  }

  closeYBLines(seriesId, prefixName, echart) {
    let options = echart.getOption()
    let { series } = options
    let { markLine, markPoint } = series.find(i => i.id == seriesId)
    let others = markLine.data.filter(i => i.name !== prefixName)
    let othersP = markPoint.data.filter(i => i.name !== prefixName)
    series.find(i => i.id == seriesId).markLine.data = others
    series.find(i => i.id == seriesId).markPoint.data = othersP
    echart.setOption({
      ...options,
      series,
    }, true)
    document.getElementById(echart.id + 'YBBlock').style.display = 'none'
  }
  /**
   * 缩放后markline根据x轴数据重新寻找邻近点进行绘制
   * @param {} param0 
   * @param {*} sourceData 
   * @returns 
   */
  reDrawLine({ markLine, color, id, name }, sourceData) {
    let marklineData = []
    let markPointData = []
    markLine.data.forEach(item => {
      let { closeIndex, closeValue } = getClosestNumber(sourceData, item.value[0])
      marklineData.push({
        ...item,
        xAxis: closeValue ? closeValue[0].toString() : '',
        value: closeValue || [],
        dataIndex: closeIndex
      })
      markPointData.push({
        name: item.name,
        coord: closeValue,
        itemStyle: {
          borderColor: item.lineStyle.color,
        }
      })
    })
    this.openYBList.forEach(item => {
      let findList = marklineData.filter(i => i.name == item)
      if (findList.length) {
        let YBxyList = findList.map(i => { return i.value })
        this.changeYBPointerData(YBxyList, findList[0].lineStyle.color, item, id, { color, name })
      }
    })
    return {
      markLine: {
        data: [...marklineData]
      },
      markPoint: {
        data: [...markPointData]
      }
    }
  }
  /**
   * 触发修改游标数量函数
   * @param {*} lineId 
   * @param {*} value 
   */
  changeYBLineNum(lineId, value) {
    let ybName = lineId.split('_')[0]
    let groupName = lineId.includes('bei') ? 'beipinGroup' : 'bianpinGroup'
    this.YBLineDataList.forEach(item => {
      if (item['children'][groupName].length) {
        let objArr = item['children'][groupName].filter(i => i.name == ybName)
        if (objArr.length) {
          this.addYbLine(item.seriesId, ybName, objArr[0].dataList, value)
        }
      }
    })
  }
  // 针对倍频和边频游标针对性添加或者删减游标线数量
  addYbLine(seriesId, ybName, dataList, value) {
    if (value == 0) { return }
    let Num = Number(value)
    let options = this.myChart.getOption()
    let { data, markLine } = options.series.find(i => i.id == seriesId)
    let lineArr = markLine.data.filter(i => i.name == ybName)
    let newLineArr = [].concat(lineArr)
    let newPointerList = dataList
    if (ybName.includes('bei')) {
      // 倍频游标
      if (Num > 0) {
        let diff = dataList[1][0] - dataList[0][0]
        for (let i = lineArr.length; i < (Num + lineArr.length); i++) {
          let { closeIndex, closeValue } = getClosestNumber(data, diff * (i + 1))
          newLineArr.push({
            ...lineArr[lineArr.length - 1],
            label: {
              ...lineArr[lineArr.length - 1].label,
              backgroundColor: '',
            },
            id: ybName + '_' + i,
            xAxis: closeValue ? closeValue[0].toString() : '',
            value: closeValue || [],
            dataType: i + 1,
            dataIndex: closeIndex
          })
          newPointerList.push(closeValue)
        }
      } else if (Num < 0) {
        // 如果删除游标倍数>已有倍数，直接删除游标
        if (!(Math.abs(Num) < newLineArr.length)) {
          this.closeYB(ybName + '_0')
          return
        } else {
          newLineArr = newLineArr.slice(0, Num)
          newPointerList = dataList.slice(0, Num)
        }
      } else {
        return
      }
      this.BeiPinYB[ybName].YBxyList = newPointerList
      this.BeiPinYB[ybName].drawLine(newLineArr, options, this.myChart)
      if (!newLineArr.find(i => i.id == this.lastYBLine)) {
        this.lastYBType = ybName
        this.changeLastYBLine(ybName + '_0')
      }


    } else {
      // 边频游标
      let newLineArr1 = []
      let diff = dataList[1][0] - dataList[0][0]
      let middle = this.BianPinYB[ybName].middleLineIndex
      // 如果删除游标倍数>已有倍数，直接删除游标
      if (Num < 0 && !(Math.abs(Num) < middle)) {
        this.closeYB(ybName + '_0')
        return
      }

      let newMiddle = middle + Num
      let newPointerList1 = []
      let isMarkLine = false
      for (let i = 0; i < newMiddle * 2 + 1; i++) {
        let dataType = i - newMiddle
        let unit = lineArr.filter(i => i.dataType == dataType)
        if (!unit.length) {
          // 不存在，增加line
          let { closeIndex, closeValue } = getClosestNumber(data, dataList[middle][0] - (newMiddle - i) * diff)
          newLineArr1.push({
            ...newLineArr[0],
            label: {
              ...newLineArr[0].label,
              backgroundColor: '',
            },
            id: ybName + '_' + i,
            dataType,
            xAxis: closeValue ? closeValue[0].toString() : '',
            value: closeValue || [],
            dataIndex: closeIndex,
          })
          newPointerList1.push(closeValue)
        } else {
          newLineArr1.push({
            ...unit[0],
            id: ybName + '_' + i,
          })
          newPointerList1.push(unit[0].value)
          if (unit[0].label.backgroundColor == '#666') {
            isMarkLine = true
            this.lastYBType = ybName
            this.lastYBLine = ybName + '_' + i
          }
        }
      }
      this.BianPinYB[ybName].middleLineIndex = newMiddle
      this.BianPinYB[ybName].YBxyList = newPointerList1
      this.BianPinYB[ybName].drawLine(newLineArr1, options, this.myChart)
      if (!isMarkLine) {
        this.lastYBType = ybName
        this.changeLastYBLine(ybName + '_' + newMiddle)
      }
    }
  }

}