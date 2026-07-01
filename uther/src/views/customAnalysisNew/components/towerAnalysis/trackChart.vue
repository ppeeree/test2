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
  </div>
</template>
<script>
import * as echarts from 'echarts'
import { TrackChart } from 'acharts'
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
    }
    /*    dataSource: {
      type: Array,
      require: true,
      default: () => {
        return []
      }
    } */
  },
  data() {
    return {
      myChart: null,
      myChartInstance: null,
      noData: false,
      chartData: {
        data: [],
        dimensions: ['倾斜率', '角度', '采集时间'], //['X倾角°', 'Y倾角°'],
        titleText: '塔基倾斜率轨迹图' //塔基倾角落点图
      },
      props: { multiple: true }
    }
  },
  watch: {
    /*  selectValue: {
      handler(val) {
        if (val.length) {
          this.getEvAnalyzerDataApiFunc()
        }
      },
      deep: true,
      immediate: true
    }, */
    /*  dataSource: {
      handler(val, older) {
        if (!isEqual(val, older)) {
          this.initChart()
        }
      },
      deep: true,
      immediate: true
    } */
  },
  created() {},
  computed: {},
  mounted() {},
  beforeUnmount() {
    this.removeResizeOb()
    this.initChartInst()
  },
  methods: {
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
    initChart(dataSource) {
      this.initChartInst()
      this.removeResizeOb()
      if (dataSource.length) {
        this.noData = false
        this.myChart = echarts.init(this.$refs.chartBox)
        this.myChartInstance = new TrackChart(
          this.myChart,
          {
            ...this.chartData,
            data: dataSource
          },
          {
            theme: this.theme,
            isShowTitle: false
          }
        )
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
        reader.onload = e => {
          if (this.myChartInstance.echartData.data.length) {
            let recordChartInfo = this.myChartInstance.echartData.data.find(
              item => item.name == this.myChartInstance.selectedLegend[0]
            ).other
            this.$emit('getImgData', {
              data: e.target.result,
              recordChart: '塔基沉降轨迹图',
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
