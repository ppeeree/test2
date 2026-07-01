<template>
  <div style="width: 100%; height: calc(100% - 2px)">
    <h3 style="position: absolute; width: 100%; top: 1px; left: 10px; font-size: 12px; z-index: 10">
      拉索力分布图
      <el-select
        style="float: right; margin-right: 10px; width: 140px"
        v-model="selectedValue"
        size="mini"
        placeholder="请选择"
      >
        <el-option
          v-for="item in options"
          :key="item.value"
          :label="item.label"
          :value="item.value"
        >
        </el-option>
      </el-select>
    </h3>
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
  </div>
</template>
<script>
import * as echarts from 'echarts'
import { PolarChart } from 'acharts'
import isEqual from 'lodash/isEqual'
export default {
  props: {
    loading: {
      type: Boolean,
      require: false
    },
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
    dataSource: {
      type: Object,
      require: true,
      default: () => {
        return {}
      }
    }
  },
  data() {
    return {
      selectedValue: '',
      options: [],
      myChart: null,
      myChartInstance: null,
      noData: false,
      chartData: {
        data: [],
        steelNumber: 30,
        dimensions: ['kN', '法向'], //
        titleText: '拉索力分布图' //
      },
      props: { multiple: true }
    }
  },
  watch: {
    dataSource: {
      handler(val, older) {
        if (!isEqual(val, older)) {
          this.initData()
        }
      },
      deep: true,
      immediate: true
    },
    selectedValue: {
      handler(val, older) {
        this.initChartData()
      },
      deep: true,
      immediate: true
    }
  },
  created() {},
  computed: {},
  mounted() {
    this.initChart()
  },
  beforeUnmount() {
    this.removeResizeOb()
    this.initChartInst()
  },
  methods: {
    /*   changeSelectedTurbine() {
      this.initChartData()
    }, */
    initData() {
      let optionList = []
      Object.keys(this.dataSource).forEach(key => {
        optionList.push({
          value: key,
          label: key
        })
      })
      this.options = optionList
      if (this.options.length) {
        if (this.selectedValue == this.options[0].value) {
          this.initChartData()
        } else {
          this.selectedValue = this.options[0].value
        }
      }
    },
    initChartData() {
      if (this.selectedValue == '') return
      let data = this.dataSource[this.selectedValue]
      let chartData = [
        {
          name: 'max',
          id: data[0].id + 'max',
          source: []
        },
        {
          name: 'min',
          id: data[0].id + 'min',
          source: []
        },
        {
          name: 'avg',
          id: data[0].id + 'avg',
          source: []
        }
      ]
      data.forEach(item => {
        const { avg, min, max, code } = item
        chartData[0].source.push([max, code])
        chartData[1].source.push([min, code])
        chartData[2].source.push([avg, code])
      })
      this.chartData.data = chartData
      this.chartData.steelNumber = data[0].steelNumber
      this.initChart()
    },
    creatResizeOb() {
      if (this.resizeOb) {
        this.resizeOb.disconnect()
        this.resizeOb = null
      }
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
      this.removeResizeOb()
      if (this.chartData.data.length) {
        this.noData = false
        this.myChart = echarts.init(this.$refs.chartBox)
        this.myChartInstance = new PolarChart(this.myChart, this.chartData, {
          theme: this.theme,
          isShowTitle: false
        })
        this.creatResizeOb()
      } else {
        this.noData = true
      }
    },
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
          if (that.chartData.data.length) {
            let recordChartInfo = that.chartData.data.find(
              item => item.name == this.myChartInstance.selectedLegend[0]
            ).other
            this.$emit('getImgData', {
              data: e.target.result,
              recordChart: '拉索力分布图',
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
    clearWavePointer() {
      return
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
</style>
