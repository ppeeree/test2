<template>
  <div style="width: 100%; height: 100%">
    <div
      @contextmenu.prevent="
        e => {
          return false
        }
      "
      ref="chartBox"
      style="width: 100%; height: 100%; color: #000"
      v-loading="loading"
      element-loading-text="数据加载中，请稍后..."
      element-loading-spinner="el-icon-loading"
    ></div>
    <div class="noData" v-if="noData">无数据</div>
    <div class="filterIcon" v-if="selectOption.length">
      <span class="titleText" v-if="titleText">{{ titleText }}</span>
      <el-select size="mini" v-model="selectValue" placeholder="请选择">
        <el-option
          v-for="item in newSelectOption"
          :key="item.code"
          :label="item.name"
          :value="item.value"
        >
        </el-option>
      </el-select>
    </div>
  </div>
</template>
<script>
import { getEvAnalyzerDataApi } from '@/api/analysis/index.js'
import * as echarts from 'echarts'
import { SimpleLineChart } from 'acharts'
import isEqual from 'lodash/isEqual'
import { defaultEvName, deepFreeze } from '../toolsComponent/tools'
import dayjs from 'dayjs'
import { dealTrendData } from '@/util/transfrom.js'
export default {
  props: {
    keyId: {
      type: String,
      require: true,
      default: 'chart'
    },
    theme: {
      type: String,
      require: false,
      default: 'light'
    },
    titleText: {
      type: String,
      require: false,
      default: ''
    },
    selectOption: {
      type: Array,
      require: false,
      default: () => {
        return []
      }
    },
    filterParam: {
      type: Object,
      require: false,
      default: () => {
        return {}
      }
    },
    isConnect: {
      type: Boolean,
      require: false,
      default: false
    }
  },
  inject: ['getChartList'],
  provide() {
    return {
      getChart: this.getChartList
    }
  },
  data() {
    return {
      myChart: null,
      myChartInstance: null,
      loading: false,
      noData: true,
      chartData: [],
      dataSourse: {
        data: [],
        xType: 'time',
        yType: 'value',
        titleText: '趋势图'
      },
      props: { multiple: true },
      selectValue: ''
    }
  },
  watch: {
    selectValue: {
      handler(val) {
        if (val.length) {
          this.getEvAnalyzerDataApiFunc()
        }
        if (this.keyId == this.$parent.selectedChart) {
          this.$emit('getSelectValue', val)
        }
      },
      deep: true,
      immediate: true
    },
    newSelectOption: {
      handler() {
        this.getDefaultSelecValue()
      },
      deep: true,
      immediate: true
    },
    filterParam: {
      handler(val, older) {
        // 强制刷新页面，重新获取趋势图
        // if (!isEqual(val, older)) {
        this.getEvAnalyzerDataApiFunc()
        // }
      },
      deep: true
    }
  },
  created() {},
  computed: {
    newSelectOption() {
      return this.selectOption.map(i => {
        return { ...i, value: i.ids.toString() }
      })
    }
  },
  mounted() {
    this.creatResizeOb()
  },
  beforeUnmount() {
    this.removeResizeOb()
    this.initChartInst()
  },
  methods: {
    getDefaultSelecValue() {
      let isTrue = false
      Object.keys(defaultEvName).forEach(item => {
        if (this.titleText.includes(item)) {
          this.newSelectOption.forEach(i => {
            if (i.name == defaultEvName[item]) {
              this.selectValue = i.value
              isTrue = true
            }
          })
        }
      })
      if (!isTrue) {
        this.selectValue = this.newSelectOption[0].value
      }
    },
    creatResizeOb() {
      this.removeResizeOb()
      this.resizeOb = new ResizeObserver(entries => {
        this.myChartInstance && this.myChartInstance.resize()
      })
      // 指定观察dom
      this.resizeOb.observe(this.$refs.chartBox)
    },
    removeResizeOb() {
      if (this.resizeOb) {
        this.resizeOb.disconnect()
        this.resizeOb = null
      }
    },
    initChartInst() {
      // 清理包装器实例（必须先于 dispose，否则 DOM 已被移除）
      if (this.myChartInstance) {
        this.myChartInstance.destroyedInstance()
        this.myChartInstance = null
      }
      // 清理图表实例
      if (this.myChart) {
        this.myChart.dispose()
        this.myChart = null
      }
    },
    initChart() {
      this.initChartInst()
      // this.removeResizeOb()
      if (this.chartData.length) {
        this.noData = false
        this.myChart = echarts.init(this.$refs.chartBox)
        this.myChartInstance = new SimpleLineChart(
          this.myChart,
          {
            ...this.dataSourse,
            data: this.chartData,
            titleText: '趋势图'
          },
          this.getWavePointer.bind(this),
          {
            theme: this.theme,
            isShowTitle: false
            // isConnect: true
          }
          //  this.connectEvent.bind(this)
        )
        //  this.creatResizeOb()
      } else {
        this.noData = true
      }
    },
    // 联动触发事件
    /*  connectEvent(data) {
        this.$emit('connectEvent', this.keyId, data)
    }, */
    operation(name, content) {
      if (!this.myChartInstance) return
      if (name == 'addRecord') {
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
            this.$emit('getImgData', {
              data: e.target.result,
              recordChart: '趋势图',
              isOpen: true,
              recordChartInfo
            })
          }
        }
      } else {
        this.myChartInstance.toolboxFeatures(name, content)
      }
    },
    changeSelectLegend(legendName) {
      this.myChartInstance.legendChange(legendName)
    },
    getWavePointer(data, ctrlKey) {
      this.$emit('clickEvent', this.keyId, data, ctrlKey)
    },
    updateMarkLineIndicator(data) {
      this.myChartInstance && this.myChartInstance.updateMarkLineIndicator(data)
    },
    clearWavePointer() {
      this.myChartInstance && this.myChartInstance.clearUpHLWave()
    },
    // 测点下特征值趋势查询接口
    getEvAnalyzerDataApiFunc() {
      this.loading = true
      let idList = this.selectValue.split(',')
      this.dispatchParamRequest(idList)
    },
    dispatchParamRequest(idList) {
      let { timeValue, workRange } = this.filterParam
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
            wkCond: {
              ROTSPEED_MCS: workRange
            }
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
            wkCond: {
              ROTSPEED_MCS: workRange
            }
          }
        })
      }
      Promise.allSettled(paramList.map(ele => getEvAnalyzerDataApi(ele)))
        .then(res => {
          this.loading = false
          let success = res.filter(i => i.status === 'fulfilled')
          if (!success.length) {
            return
          }
          const middle = success.flatMap(item => (item.value ? item.value.data.data : undefined))
          let arr = []
          middle.forEach(item => {
            if (item) {
              arr = [...arr, ...dealTrendData(item)]
            }
          })
          this.chartData = deepFreeze(arr) //[...this.chartData, ...arr]
          this.initChart()
          /*    if (!this.chartData.length) {
            this.loading = false
          } */
        })
        .catch(error => {
          alert(error)
        })
    }
  }
}
</script>
<style lang="scss" scoped>
.noData {
  width: 100%;
  height: 100%;
  font-size: 20px;
  color: #aaa;
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
.filterIcon {
  position: absolute;
  left: 10px;
  top: 2px;
  font-size: 14px;
  .titleText {
    margin-right: 10px;
    font-size: 14px;
    font-weight: bolder;
  }
}
</style>
