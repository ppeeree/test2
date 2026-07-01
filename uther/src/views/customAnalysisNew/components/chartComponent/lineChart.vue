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
    <span class="titleText" v-if="titleText">{{ titleText }}</span>
  </div>
</template>
<script>
import * as echarts from 'echarts'
import { SimpleLineChart } from 'acharts'
export default {
  props: {
    loading: {
      type: Boolean,
      default: false
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
    /* dataSource: {
      type: Object,
      require: true,
      default: () => {
        return {}
      }
    }, */
    warningLine: {
      type: Boolean,
      require: false,
      default: true
    }
  },
  data() {
    return {
      myChart: null,
      myChartInstance: null,
      noData: true,
      chartData: {
        data: [],
        xType: 'time',
        yType: 'value',
        dimensions: ['采集时间', '倾斜率']
      },
      props: { multiple: true }
    }
  },
  /*  watch: {
    dataSource: {
      handler(val, older) {
        this.initChart()
      },
      deep: true,
      immediate: true
    }
  }, */
  computed: {},
  mounted() {
    this.creatResizeOb()
  },
  beforeUnmount() {
    this.removeResizeOb()
    this.initChartInst()
  },
  methods: {
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
    initChart(dataSource) {
      this.initChartInst()
      // this.removeResizeOb()
      if (dataSource.data.length) {
        this.noData = false
        this.myChart = echarts.init(this.$refs.chartBox)
        this.myChartInstance = new SimpleLineChart(
          this.myChart,
          {
            ...this.chartData,
            ...dataSource,
            titleText: this.titleText
          },
          this.getWavePointer.bind(this),
          {
            theme: this.theme,
            isShowTitle: false,
            isShowWarningLine: this.warningLine
          }
        )
        //  this.creatResizeOb()
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
        reader.onload = e => {
          if (this.myChartInstance.echartData.data.length) {
            let recordChartInfo = this.myChartInstance.echartData.data.find(
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
    clearWavePointer() {
      this.myChartInstance && this.myChartInstance.clearUpHLWave()
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

.titleText {
  position: absolute;
  left: 10px;
  top: 2px;
  margin-right: 10px;
  font-size: 14px;
  font-weight: bolder;
}
</style>
