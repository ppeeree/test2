<template>
  <div style="width: 100%; height: 100%">
    <div
      @contextmenu.prevent
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
import { EvTrendLine, DipAngleChart } from 'acharts'
import debounce from 'lodash/debounce'
export default {
  props: {
    dataSourse: {
      type: Object,
      required: true,
      default() {
        return {
          data: [],
          xType: 'time',
          yType: 'value',
          titleText: '趋势分析'
        }
      }
    },
    chartType: {
      type: String,
      required: false,
      default: 'TA'
    },
    theme: {
      type: String,
      required: false,
      default: 'light'
    },
    loading: {
      type: Boolean,
      required: false,
      default: false
    },
    isShowTitle: {
      type: String,
      required: false,
      default: 'show'
    },
    isMultipleCLick: {
      type: String,
      required: false,
      default: 'more'
    }
  },
  data() {
    return {
      // myChart: null,
      myChartInstance: null,
      noData: true
    }
  },
  watch: {
    dataSourse: {
      handler() {
        this.initChart()
      },
      deep: true,
      immediate: true
    }
  },
  // 优化：增加防抖后的 resize 方法，提升窗口缩放时的性能
  created() {
    this.debouncedResize = debounce(() => {
      if (this.myChartInstance) {
        this.myChartInstance.resize()
      }
    }, 300)
  },
  mounted() {
    // this.initChart()
  },
  beforeDestroy() {
    this.removeResizeOb()
    this.initChartInst()
  },
  methods: {
    creatResizeOb() {
      this.removeResizeOb()
      this.resizeOb = new ResizeObserver(entries => {
        this.debouncedResize()
      })
      // 指定观察dom
      if (this.$refs.chartBox) {
        this.resizeOb.observe(this.$refs.chartBox)
      }
    },
    removeResizeOb() {
      if (this.resizeOb) {
        this.resizeOb.disconnect()
        this.resizeOb = null
      }
    },
    initChartInst() {
      if (this.myChartInstance) {
        this.myChartInstance.destroyedInstance()
        this.myChartInstance = null
      }
    },
    initChart: debounce(function () {
      this.initChartInst()
      this.removeResizeOb()
      if (this.dataSourse.data.length) {
        this.noData = false
        // this.myChart = echarts.init(this.$refs.chartBox)
        if (this.chartType == 'OA') {
          this.myChartInstance = new DipAngleChart(
            echarts.init(this.$refs.chartBox),
            {
              ...this.dataSourse,
              dimensions: ['X倾角°', 'Y倾角°'],
              titleText: '倾角分布图'
            },
            {
              theme: this.theme
            }
          )
        } else {
          this.myChartInstance = new EvTrendLine(
            echarts.init(this.$refs.chartBox),
            this.dataSourse,
            this.getWavePointer.bind(this),
            {
              theme: this.theme,
              isShowTitle: this.isShowTitle && this.isShowTitle == 'hide' ? false : true
            },
            this.isMultipleCLick
          )
        }

        this.creatResizeOb()
      } else {
        this.initChartInst()
        this.noData = true
      }
    }, 100),
    getWavePointer(data) {
      this.$emit('clickEvent', data)
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
