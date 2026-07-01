import { JianGeYB } from './jgyb.js'
import { SingleYB } from './single.js'
import { SYB } from './syb.js'
import { BianPinYB } from './bipyb.js'
import { BeiPinYB } from './beipyb.js'
import { judgeYbExist, initAllGraphicPosition, closeYBLines } from './ybCommonTools.js'
import {
  debounce
} from '../tool.js'

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
    this.BianLineNum = value.bianpin ? Number(value.bianpin) : 7
    this.JianGeLineNum = value.jiange ? Number(value.jiange) : 5
  }

  resetYBlineNum() {
    this.BeiPinlineNum = 5
    this.BianLineNum = 7
    this.JianGeLineNum = 5
  }

  // 创建游标实例
  creatYBGroup(type, selectedLegend) {
    if (selectedLegend.length) {
      let options = this.myChart.getOption()
      let { grid, series } = options
      selectedLegend.forEach(item => {
        let obj, objIndex
        objIndex = series.findIndex(i => i.name == item)
        obj = series[objIndex]
        let prefixName = this.YBkey[type] + this.groupNum
        this.openYBList.push(prefixName)
        this.lastYBType = prefixName
        this.lastYBLine = prefixName + '_0'
        this.groupNum++
        let topNumber = grid.length == 1 ? grid[0].top : grid[obj.yAxisIndex].top
        let gridHeight = grid[0].height
        let shape = {
          x1: 0,
          y1: topNumber,
          x2: 0,
          y2: topNumber + gridHeight
        }
        let params1 = { data: obj.data, seriesId: obj.id, yAxisIndex: obj.yAxisIndex || 0, options }
        let params2 = {
          type: 'line',
          draggable: true,
          bounding: 'raw',
          focus: 'self',
          z: 10,
          top: topNumber,
          shape
        }
        switch (type) {
          case 'SYB':
            this.SYB[prefixName] = new SYB()
            this.SYB[prefixName].addYBLines(prefixName,
              this.myChart, params1, params2, this.changeYBPointerData.bind(this))
            break;
          case 'JianGeYB':
            this.JianGeYB[prefixName] = new JianGeYB(this.JianGeLineNum)
            this.JianGeYB[prefixName].addYBLines(prefixName,
              this.myChart, params1, params2, this.changeYBPointerData.bind(this))
            break;
          case 'SingleYB':
            this.SingleYB[prefixName] = new SingleYB()
            this.SingleYB[prefixName].addYBLines(prefixName,
              this.myChart, params1, params2, this.changeYBPointerData.bind(this))
            break;
          case 'BeiPinYB':
            this.BeiPinYB[prefixName] = new BeiPinYB(this.BeiPinlineNum)
            this.BeiPinYB[prefixName].addYBLines(prefixName,
              this.myChart, params1, params2, this.changeYBPointerData.bind(this))
            break;
          case 'BianPinYB':
            this.BianPinYB[prefixName] = new BianPinYB(this.BianLineNum)
            this.BianPinYB[prefixName].addYBLines(prefixName,
              this.myChart, params1, params2, this.changeYBPointerData.bind(this))
            break;
        }
      })
    } else {
      alert('请选择一条曲线进行游标分析！')
    }
  }
  // toolbox: 清空游标线
  closeAllYB() {
    let options = this.myChart.getOption()
    let { series, graphic } = options
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
      if (item.name != 'markline_level') {
        if (item && item.markLine) {
          obj.markLine.data = []
        }
        if (item && item.markPoint) {
          obj.markPoint.data = []
        }
      }
      newSeries.push(obj)
    })
    let othersGraphic = []
    if (graphic && graphic.length) {
      othersGraphic = graphic[0].elements.filter(i => !this.openYBList.includes(i.id.split('_')[0]))
    }
    this.myChart.clear()
    this.myChart.setOption({
      ...options,
      graphic: othersGraphic,
      series: newSeries //.filter(i => i.name !== 'markline_level'),
    }, true)
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
    this.lastYBLine = ''
    this.lastYBType = ''
    this.YBLineDataList = []
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
      closeYBLines(seriesId, prefixName, this.myChart)
      key = 'shuangGroup'
      this.SYB[prefixName] = null
      delete this.SYB[prefixName]
    } else if (prefixName.includes('jiange')) {
      const { seriesId } = this.JianGeYB[prefixName]
      closeYBLines(seriesId, prefixName, this.myChart)
      key = 'jiangeGroup'
      this.JianGeYB[prefixName] = null
      delete this.JianGeYB[prefixName]
    } else if (prefixName.includes('single')) {
      const { seriesId } = this.SingleYB[prefixName]
      closeYBLines(seriesId, prefixName, this.myChart)
      key = 'singleGroup'
      this.SingleYB[prefixName] = null
      delete this.SingleYB[prefixName]
    } else if (prefixName.includes('bei')) {
      const { seriesId } = this.BeiPinYB[prefixName]
      closeYBLines(seriesId, prefixName, this.myChart)
      key = 'beipinGroup'
      this.BeiPinYB[prefixName] = null
      delete this.BeiPinYB[prefixName]
    } else if (prefixName.includes('bian')) {
      const { seriesId } = this.BianPinYB[prefixName]
      closeYBLines(seriesId, prefixName, this.myChart)
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
    if (arr.length) {
      this.lastYBType = arr[arr.length - 1]
      if (this.lastYBType.includes('shuang') || this.lastYBType.includes('jiange') || this.lastYBType.includes('single') || this.lastYBType.includes('bei')) {
        this.lastYBLine = this.lastYBType + '_0'
      } else if (this.lastYBType.includes('bian')) {
        this.lastYBLine = this.lastYBType + '_3'
      }
    } else {
      this.lastYBType = ''
      this.lastYBLine = ''
    }
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

  // 图谱尺寸大小变化，游标线长短响应
  // 返回计算后的游标线配置
  resize(newGrid, options) {
    let graphicList = []
    // 游标线位置重新计算定位
    if (this.openYBList.length) {
      let { isExist, lineArr } = judgeYbExist(options, this.openYBList)
      if (isExist) {
        let valueObj = []
        lineArr.forEach(item => {
          if (valueObj[item.name]) {
            valueObj[item.name].push(item)
          } else {
            valueObj[item.name] = [item]
          }
        })
        let newgraphicList = initAllGraphicPosition(valueObj, options, this.myChart)
        let { series } = options
        let gridHeight = newGrid[0].height
        graphicList = newgraphicList[0].elements.map(item => {
          let objIndex = series.find(i => i.id == item.info).xAxisIndex || 0
          let topNumber = newGrid[objIndex].top
          let shape = {
            x1: 0,
            y1: topNumber,
            x2: 0,
            y2: topNumber + gridHeight
          }
          return {
            ...item,
            top: topNumber,
            height: gridHeight,
            shape
          }
        })
      }
    }
    return graphicList
  }
}