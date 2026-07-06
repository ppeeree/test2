<template>
  <div
    class="colbox trend_content"
    ref="trendContent"
    data-key="trend"
    :id="itemId"
    style="width: 100%; height: 100%"
  >
    <div style="width: 100%; height: 100%; position: relative">
      <div class="title_block">
        <span
          style="font-weight: bolder; float: left; color: #000; line-height: 35px; font-size: 14px"
          >特征值趋势分析</span
        >
      </div>
      <div class="trend_left" ref="trend_left" :style="{ width: '100%' }">
        <div
          @contextmenu.prevent="
            e => {
              return false
            }
          "
          ref="chartBox"
          style="width: 100%; height: 100%; padding-bottom: 1px; color: #000"
          v-loading="loading"
          element-loading-text="数据加载中，请稍后..."
          element-loading-spinner="el-icon-loading"
        ></div>
        <div class="noData" v-if="noData">趋势图</div>
        <div class="filterIcon" v-if="selectOption.length > 0">
          <el-select size="mini" v-model="selectValue" placeholder="请选择">
            <el-option
              v-for="item in selectOption"
              :key="item.code"
              :label="item.name"
              :value="item.value"
            >
            </el-option>
          </el-select>
        </div>
        <i
          v-if="chartData.length"
          class="el-icon-edit openIcon"
          title="添加诊断记录"
          @click="addRecord"
        ></i>
      </div>
    </div>
  </div>
</template>
<script>
import { getEvAnalyzerDataApi, getGroupTrendData } from '@/api/analysis/index.js'
import axios from 'axios'
import dayjs from 'dayjs'
import { EvTrendLine } from 'acharts'
import * as echarts from 'echarts'
/* import dateRange from '../../aside/timeRange.vue'
import workRange from '../../aside/workRange.vue'
import Splitter from '../splitter' */
import isEqual from 'lodash/isEqual'
export default {
  props: ['waveDetailInfoList', 'treeDataAndFilterP', 'type'],
  components: {
    /*  Splitter,
    dateRange,
    workRange */
  },
  data() {
    return {
      selectOption: [],
      selectValue: '',
      noData: true,
      loading: false,
      waveInfoDataList: [],
      dialogVisible: false,
      chartType: 'trend',
      chartData: [],
      dataSourse: {
        data: [],
        xType: 'time',
        yType: 'value',
        titleText: '趋势分析'
      },
      menuList: [
        {
          key: 'left',
          value: '左'
        },
        {
          key: 'right',
          value: '右'
        }
      ],
      // myChart: null,
      myChartInstance: null,
      rightClickTurbineId: '',
      // 标记是否正在初始化 selectOption（防止 selectValue watch 级联触发）
      isSelectOptionLoading: false,
      // 请求版本号，用于取消过时请求的结果处理
      requestVersion: 0,
      // ResizeObserver 防抖定时器
      resizeTimer: null,
      // 缓存 getWavePointer 引用，避免每次 initChart 重复 bind
      cachedGetWavePointer: null,
      pendingTrendCancel: null,
      queryTimer: null,
      resizeFrame: null,
      groupTrendCache: new Map(),
      trendDataCache: new Map()
    }
  },
  computed: {
    // 右侧根据时间点整合的点击波形点的数据
  },
  watch: {
    treeDataAndFilterP: {
      handler(newVal, oldVal) {
        let newClickedList = newVal?.checkedMeasList || []
        let oldClickedList = oldVal?.checkedMeasList || []
        let newfilterParam = newVal?.filterParam || {}
        let oldfilterParam = oldVal?.filterParam || {}
        if (!isEqual(newClickedList, oldClickedList) || !isEqual(newfilterParam, oldfilterParam)) {
          this.waveInfoDataList = []
          if (!newClickedList.length) {
            return this.$message.warning('请勾选需要查询的数据！')
          }
          if (!isEqual(newClickedList, oldClickedList)) {
            /*  // 标记正在加载 selectOption，防止 selectValue watch 级联重复请求
            this.isSelectOptionLoading = true */
            this.getSelectOption()
          }
          if (isEqual(newClickedList, oldClickedList) && !isEqual(newfilterParam, oldfilterParam)) {
            this.getEvAnalyzerDataApiFunc()
          }
        } else {
          if (newClickedList.length) {
            this.$emit('getWavePointer', [])
            this.getEvAnalyzerDataApiFunc()
          }
          return
        }
      },
      deep: true,
      immediate: true
    },
    selectValue: {
      handler(val) {
        // 如果是 getSelectOption 内部设置的 selectValue，跳过本次触发，避免重复请求
        /*  if (this.isSelectOptionLoading) {
          this.isSelectOptionLoading = false
          this.$emit('changeSelectedEigenValue', val)
          return
        } */
        if (val.length) {
          this.getEvAnalyzerDataApiFunc()
        } else {
          this.cancelPendingTrendRequest(true)
          this.chartData = Object.freeze([])
          this.initChartInst()
          this.waveInfoDataList = []
        }
        this.$emit('changeSelectedEigenValue', val)
      },
      immediate: true
    },
    waveInfoDataList: {
      handler() {
        this.currentWaveInfoDataListOut()
      }
    }
  },
  activated() {
    this.creatResizeOb()
  },
  deactivated() {
    this.cleanupResizeOb()
  },
  mounted() {},
  beforeUnmount() {
    this.cancelPendingTrendRequest()
    this.initChartInst()
    this.cleanupResizeOb()
    // 清理防抖定时器
    if (this.resizeTimer) {
      clearTimeout(this.resizeTimer)
      this.resizeTimer = null
    }
    if (this.queryTimer) {
      clearTimeout(this.queryTimer)
      this.queryTimer = null
    }
    if (this.resizeFrame) {
      cancelAnimationFrame(this.resizeFrame)
      this.resizeFrame = null
    }
  },
  methods: {
    // tab切换后初始化特征值相关图谱的传参信息
    initSelectedEigenValue() {
      this.$emit('changeSelectedEigenValue', this.selectValue)
    },
    // tab切换后初始化测点相关图谱的传参信息
    currentWaveInfoDataListOut() {
      if (this.waveInfoDataList.length) {
        let wavepointers = []
        wavepointers = this.waveInfoDataList.map(item => {
          let { coord, other } = item
          const { windturbineId, measlocId, sampleRate } = other
          // 波形ID：机组id+测量id+采样时间
          return windturbineId + '&&' + measlocId + '&&' + coord[0] + '&&' + sampleRate
        })
        this.$emit('getWavePointer', [...new Set(wavepointers)])
      } else {
        this.$emit('getWavePointer', [])
      }
    },
    getSelectOption() {
      const cacheKey = this.getGroupTrendCacheKey()
      if (this.groupTrendCache.has(cacheKey)) {
        this.applySelectOption(this.groupTrendCache.get(cacheKey))
        return
      }
      getGroupTrendData({
        GTCAttributes: this.treeDataAndFilterP.checkedMeasList,
        GTCType: this.type
      }).then(res => {
        const optionList = res.data.data || []
        this.setLimitedCache(this.groupTrendCache, cacheKey, optionList, 30)
        this.applySelectOption(optionList)
      })
    },
    applySelectOption(optionList) {
      if (optionList.length > 0) {
        this.selectOption = optionList.map(i => {
          return {
            ...i,
            value: i.ids.toString()
          }
        })
        const nextValue = optionList[0].ids.toString()
        if (this.selectValue === nextValue) {
          this.getEvAnalyzerDataApiFunc()
        } else {
          this.selectValue = nextValue
        }
      } else {
        this.selectOption = []
        this.selectValue = ''
      }
    },
    getCheckedMeasCacheKey() {
      const checkedMeasList = this.treeDataAndFilterP?.checkedMeasList || []
      return checkedMeasList
        .map(item => `${item.windturbineID || ''}:${item.measLoctionID || ''}`)
        .sort()
        .join('|')
    },
    getGroupTrendCacheKey() {
      return `${this.type || ''}::${this.getCheckedMeasCacheKey()}`
    },
    getTrendDataCacheKey(param) {
      return JSON.stringify({
        analyzeWay: param.analyzeWay,
        startTime: param.startTime,
        endTime: param.endTime,
        eigenValueIds: [...(param.eigenValueIds || [])].sort(),
        wkCond: param.wkCond || {}
      })
    },
    setLimitedCache(cache, key, value, limit) {
      if (cache.has(key)) {
        cache.delete(key)
      }
      cache.set(key, value)
      while (cache.size > limit) {
        cache.delete(cache.keys().next().value)
      }
    },
    getTrendDataWithCache(param, cancelToken) {
      const cacheKey = this.getTrendDataCacheKey(param)
      if (this.trendDataCache.has(cacheKey)) {
        return Promise.resolve({ data: this.trendDataCache.get(cacheKey) })
      }
      return getEvAnalyzerDataApi(param, {
        cancelToken,
        meta: {
          skipNProgress: true
        }
      }).then(res => {
        this.setLimitedCache(this.trendDataCache, cacheKey, res.data, 80)
        return res
      })
    },
    initChartInst() {
      if (this.myChartInstance) {
        this.myChartInstance.destroyedInstance()
        this.myChartInstance = null
      }
    },
    cleanupResizeOb() {
      if (this.resizeOb) {
        this.resizeOb.disconnect()
        this.resizeOb = null
      }
    },
    creatResizeOb() {
      this.cleanupResizeOb()
      this.resizeOb = new ResizeObserver(entries => {
        if (this.resizeFrame) {
          cancelAnimationFrame(this.resizeFrame)
        }
        this.resizeFrame = requestAnimationFrame(() => {
          this.resizeFrame = null
          entries.forEach(entry => {
            if (entry.target == this.$refs.trendContent) {
              let newWidth = entry.contentRect.width
              let newHeight = entry.contentRect.height
              if (this.$refs.trend_left) {
                this.$refs.trend_left.style.width = newWidth + 'px'
                this.$refs.trend_left.style.height = newHeight + 'px'
                this.$refs.trend_left.style.left = 0
              }
            } else {
              // 防抖处理
              if (this.resizeTimer) clearTimeout(this.resizeTimer)
              this.resizeTimer = setTimeout(() => {
                this.myChartInstance && this.myChartInstance.resize()
              }, 300)
            }
          })
        })
      })
      // 指定观察dom
      this.resizeOb.observe(this.$refs.chartBox)
      this.resizeOb.observe(this.$refs.trendContent)
    },
    initChart() {
      this.loading = false
      if (this.chartData.length) {
        this.noData = false
        /*   this.myChart = echarts.init(this.$refs.chartBox, null, {
          renderer: 'canvas'
          // useHandler: 'proxy' // 5.4.0+ 内置委托，监听器瞬间降到 <100
        }) */
        // 缓存 bind 引用，避免每次创建新函数导致旧回调无法被 GC 回收
        if (!this.cachedGetWavePointer) {
          this.cachedGetWavePointer = this.getWavePointer.bind(this)
        }
        const chartSource = {
          ...this.dataSourse,
          data: this.chartData,
          titleText: '趋势图'
        }
        if (this.myChartInstance && this.myChartInstance.updateData) {
          this.myChartInstance.updateData(chartSource)
          this.myChartInstance.resize()
          return
        }
        this.myChartInstance = new EvTrendLine(
          echarts.init(this.$refs.chartBox, null, {
            renderer: 'canvas'
            // useHandler: 'proxy' // 5.4.0+ 内置委托，监听器瞬间降到 <100
          }),
          chartSource,
          this.cachedGetWavePointer,
          {
            theme: 'light',
            isShowTitle: false
          }
        )
        //  this.creatResizeOb()
        // 默认选中点
        /*   if (sessionStorage.getItem('checkData')) {
          const { acqTime } = JSON.parse(sessionStorage.getItem('checkData'))
          this.myChartInstance.oneClickPointer({ value: [acqTime, '', '', true] })
          sessionStorage.removeItem('checkData')
        } */
      } else {
        this.initChartInst()
        this.noData = true
      }
    },
    // 点击趋势上的波形点信息方法
    getWavePointer(data) {
      this.waveInfoDataList = data
    },
    // 测点下特征值趋势查询接口
    getEvAnalyzerDataApiFunc() {
      let idList = this.selectValue.split(',')
      if (idList.length > 6) {
        this.$confirm('请求数据量过多(>6)，避免影响使用体验，是否继续？', {
          confirmButtonText: '确定',
          cancelButtonText: '取消',
          type: 'warning'
        }).then(() => {
          this.scheduleParamRequest(idList)
        })
      } else {
        this.scheduleParamRequest(idList)
      }
    },
    scheduleParamRequest(idList) {
      if (this.queryTimer) {
        clearTimeout(this.queryTimer)
      }
      this.queryTimer = setTimeout(() => {
        this.queryTimer = null
        this.dispatchParamRequest(idList)
      }, 80)
    },
    cancelPendingTrendRequest(invalidate = false) {
      if (invalidate) {
        this.requestVersion += 1
      }
      if (this.pendingTrendCancel) {
        this.pendingTrendCancel('cancel stale trend request')
        this.pendingTrendCancel = null
      }
    },
    dispatchParamRequest(idList) {
      this.loading = true
      this.cancelPendingTrendRequest()
      const cancelSource = axios.CancelToken.source()
      this.pendingTrendCancel = cancelSource.cancel
      // 递增版本号，用于忽略过时请求的结果
      const currentVersion = ++this.requestVersion
      let { filterParam } = this.treeDataAndFilterP
      let { timeValue, wkCond } = filterParam
      if (!timeValue.length) {
        timeValue = [
          dayjs().subtract(90, 'day').format('YYYY-MM-DD 00:00:00'),
          dayjs().format('YYYY-MM-DD 23:59:59')
        ]
      }
      let num = idList.length
      let unitNum = Math.floor(num / 6)
      let diffNum = (unitNum + 1) * 6 - num
      let paramList = []
      if (num > 6) {
        let startIndex = 0
        for (let i = 0; i < 6; i++) {
          let unitIds = []
          if (i < diffNum) {
            unitIds = idList.slice(startIndex, startIndex + unitNum)
            startIndex += unitNum
          } else {
            unitIds = idList.slice(startIndex, startIndex + unitNum + 1)
            startIndex += unitNum + 1
          }
          paramList.push({
            analyzeWay: 'TA',
            endTime: timeValue[1],
            startTime: timeValue[0],
            eigenValueIds: unitIds,
            wkCond
          })
        }
      } else {
        paramList = idList.map(item => {
          return {
            analyzeWay: 'TA',
            endTime: timeValue[1],
            startTime: timeValue[0],
            eigenValueIds: [item],
            windturbineIds: [],
            wkCond
          }
        })
      }
      Promise.allSettled(
        paramList.map(ele =>
          this.getTrendDataWithCache(ele, cancelSource.token)
        )
      )
        .then(res => {
          // 如果版本号不匹配，说明已有新请求发出，忽略本次结果
          if (currentVersion !== this.requestVersion) return
          this.pendingTrendCancel = null
          this.loading = false
          const hasOnlyCancel = res.length && res.every(i => i.status === 'rejected' && axios.isCancel(i.reason))
          if (hasOnlyCancel) return
          let success = res.filter(i => i.status === 'fulfilled')
          if (!success.length) {
            return
          }
          const middle = success.flatMap(item => (item.value ? item.value.data.data : undefined))
          let arr = []
          middle.forEach(item => {
            if (item && item.evdataList && item.evdataList.length) {
              const dealt = this.dealTrendData(item)
              arr.push(...dealt)
            }
          })
          this.chartData = arr
          this.emitDataTimeRange(arr)
          /*   if (!this.chartData.length) {
            return this.$message({
              message: '未查询到数据',
              type: 'warning',
              duration: 1000
            })
          } */
          this.initChart()
        })
        .catch(error => {
          if (currentVersion !== this.requestVersion) return
          this.pendingTrendCancel = null
          if (axios.isCancel && axios.isCancel(error)) return
          this.loading = false
          console.error('趋势数据请求失败:', error)
          alert(error)
        })
    },
    emitDataTimeRange(chartData) {
      let startTime = null
      let endTime = null
      chartData.forEach(item => {
        const source = Array.isArray(item.source) ? item.source : []
        source.forEach(point => {
          const time = Array.isArray(point) ? point[0] : point && point[0]
          if (time) {
            const parsedTime = dayjs(time)
            if (parsedTime.isValid()) {
              const timeValue = parsedTime.valueOf()
              if (startTime === null || timeValue < startTime) startTime = timeValue
              if (endTime === null || timeValue > endTime) endTime = timeValue
            }
          }
        })
      })
      if (startTime === null || endTime === null) return

      this.$emit('syncDataTimeRange', [
        dayjs(startTime).format('YYYY-MM-DD'),
        dayjs(endTime).format('YYYY-MM-DD')
      ])
    },
    dealTrendData(data) {
      const { evdataList } = data
      let lineDatas = []
      evdataList.forEach(element => {
        const {
          windturbineName,
          windParkName,
          sampleRate,
          evName,
          measlocName,
          vdiMax,
          vdiMin,
          evdata,
          evId,
          unitX,
          unitY
        } = element
        let name = `${evName}_${sampleRate + 'Hz_'}${measlocName}_${windturbineName}_${windParkName}`
        lineDatas.push({
          source: evdata,
          name: name,
          sourceName: name,
          VDI: vdiMin === null ? [] : [vdiMin, vdiMax],
          id: evId + '~' + sampleRate,
          dimensions: [unitX, unitY, '转速(rpm)', '是否存在波形'],
          other: {
            ...element,
            evdata: []
          }
        })
      })
      return lineDatas
    },

    addRecord() {
      // 设定规则：只能给一条曲线增加分析记录
      if (this.myChartInstance.selectedLegend.length != 1) {
        return this.$message.warning('请选择一条曲线进行分析！')
      }
      let reader = new FileReader()
      let data = this.myChartInstance.imageBase64Data(true)
      reader.readAsDataURL(data)
      let that = this
      reader.onload = e => {
        if (that.chartData.length) {
          let recordChartInfo = that.chartData.find(
            item => item.name == this.myChartInstance.selectedLegend[0]
          ).other
          that.$emit('getImgData', {
            data: e.target.result,
            recordChart: '趋势图',
            isOpen: true,
            recordChartInfo
          })
        } else {
          return
        }
      }
    }
  }
}
</script>
<style lang="scss" scoped>
.trend_content {
  :deep(.el-dialog){
    height: auto !important;
    padding-bottom: 10px;
    max-height: 80%;
    overflow: auto;
    .el-table th.el-table__cell > .cell {
      color: #999;
    }
    .el-table tr {
      color: #999;
    }
  }
}
.colbox {
  // border: 1px solid #000;
  text-align: center;
  height: 100%;
  //padding-top: 15px;
  background: #fff;

  h3 {
    position: absolute;
    right: 10px;
    top: 10px;
    z-index: 20;
    width: auto;
    height: 28px;
    /*  .refreshBtn {
      &:hover {
        color: blue;
      }
    } */
  }
  .trend_left {
    width: calc(100% - 234px);
    height: 100%;
    left: 0;
    position: absolute;
    top: 0;
    padding-top: 37px;
  }
  #trendSplit {
    right: 230px;
    height: calc(100% - 10px);
    top: 10px;
  }
  .inlineBlock {
    display: inline-block;
    width: 15px;
    height: 7px;
    border-radius: 5px;
    margin: 0 15px;
  }
  .noData {
    width: 100%;
    height: 100%;
    font-size: 40px;
    color: #eee;
    text-align: center;
    display: flex; /*实现垂直居中*/
    align-items: center; /*实现水平居中*/
    justify-content: center;
    text-align: justify;
    margin: 0 auto;
    position: absolute;
    left: 0;
    top: 0;
  }
  .dialogMolPlist {
    position: absolute;
    overflow: hidden;
    background: #fff;
    color: #000;
    display: none;
    z-index: 9999;
    width: 240px;
    height: auto;
    min-height: 200px;
    padding: 10px;
    border-radius: 10px;
    border: 1px solid #999;
    word-break: break-word;
    .el-checkbox {
      width: 100%;
      text-align: left;
      color: #000;
    }
  }
  :deep(.el-dialog__body){
    padding: 0;
  }
}
.block_btn {
  float: right;
}
.filterIcon {
  position: absolute;
  left: 10px;
  top: 37px;
  font-size: 14px;
}
.title_block {
  padding-left: 15px;
  position: absolute;
  width: 100%;
  height: 35px;
  left: 0;
  top: 0;
  border-bottom: 2px solid #ccc;
  z-index: 10;
}
/* .condition_filter {
  height: 30px;
  float: right;
  margin-right: 10px;
  text-align: right;
  p {
    display: inline-block;
    width: 300px;
    font-size: 12px;
    text-align: left;
    color: #000;
    label {
      display: inline-block;
      width: 40px;
    }
    .inline-input {
      width: 100px;
      :deep(.el-input__inner){
        padding: 0;
        text-align: center;
      }
    }
    input {
      height: 30px;
      line-height: 30px;
    }
    :deep(.el-range-editor){
      width: 200px;
      height: 30px;
      line-height: 30px;
    }
    :deep(.el-date-editor){
      .el-range-separator {
        color: #303133;
        padding: 0;
      }
      .el-range-input {
        width: 45%;
      }
      .el-range__icon {
        display: none;
      }
      .el-range__close-icon {
        display: none;
      }
    }
  }
} */
.openIcon {
  position: absolute;
  right: 15px;
  top: 50px;
  cursor: pointer;
  font-size: 18px;
  color: #000;
}
</style>
