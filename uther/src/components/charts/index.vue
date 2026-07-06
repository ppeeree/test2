<template>
  <div
    ref="echartId"
    :id="idName || ''"
    :style="{ height: height + 'px', width: width + 'px' }"
  ></div>
</template>

<script>
import * as echarts from 'echarts'
// import { mapState } from 'vuex'

const hasAxis = axis => {
  return Array.isArray(axis) ? axis.length > 0 : !!axis
}

const normalizeEchartsOption = option => {
  if (!option || !hasAxis(option.xAxis) || !hasAxis(option.yAxis)) {
    return option
  }
  const patchSeries = series => {
    if (
      series &&
      series.type === 'bar' &&
      !series.coordinateSystem &&
      series.polarIndex === undefined
    ) {
      return {
        ...series,
        coordinateSystem: 'cartesian2d'
      }
    }
    return series
  }
  const series = Array.isArray(option.series)
    ? option.series.map(patchSeries)
    : patchSeries(option.series)
  return {
    ...option,
    series
  }
}

export default {
  props: {
    chartData: {
      type: Object,
      default: () => {}
    },
    width: {
      type: String,
      required: false,
      default: '100%'
    },
    height: {
      type: String,
      required: false
    },
    isClick: {
      type: Boolean,
      default: false
    },
    idName: {
      type: String,
      required: false
    }
  },
  data() {
    return {
      chart: null
    }
  },
  computed: {
    /*   ...mapState({
      showCollapse: state => state.common.showCollapse,
      isCollapse: state => state.common.isCollapse
    }) */
  },
  beforeCreate() {
    setTimeout(() => {
      this.resizeHandler()
    }, 500)
  },
  // 1.监听菜单收缩变化 图表自适应 2.监听数据变化 初始化图表
  watch: {
    // collapse() {
    //   this.resizeHandler();
    // },
    isCollapse() {
      setTimeout(() => {
        this.resizeHandler()
      }, 500)
    },
    showCollapse() {
      setTimeout(() => {
        this.resizeHandler()
      }, 500)
    },
    chartData: {
      deep: true,
      handler(val) {
        if (!this.chart) {
          return
        }
        this.chart.clear()
        val && this.chart.setOption(normalizeEchartsOption(val), true)
      }
    },
    width: {
      handler(val) {
        this.resizeHandler(val)
      },
      deep: true
    },
    height: {
      handler() {
        this.resizeHandler()
      },
      deep: true
    }
  },
  mounted() {
    this.resizeHandler = this.$utils.debounce(() => {
      if (this.chart) {
        if (this.height && this.width) {
          this.chart.resize({ width: this.width + 'px', height: this.height + 'px' })
        } else {
          this.chart.resize()
        }
      }
    }, 100)
    this.initResizeEvent()
    this.initChart()
  },
  beforeUnmount() {
    this.destroyResizeEvent()
    if (!this.chart) {
      return
    }
    this.chart.dispose()
    this.chart = null
  },
  methods: {
    initChart() {
      this.chart = echarts.init(this.$refs.echartId)
      this.chartData && this.chart.setOption(normalizeEchartsOption(this.chartData), true)
      // 点击事件
      if (this.isClick) {
        this.chart.off('click')
        this.chart.on('click', (param, ev) => {
          this.$emit('clickEvent', param)
        })
      }
    },
    // 监听浏览器窗口变化 图表自适应
    initResizeEvent() {
      window.addEventListener('resize', this.resizeHandler)
    },
    destroyResizeEvent() {
      window.removeEventListener('resize', this.resizeHandler)
    }
  }
}
</script>
<style lang="scss" scoped></style>
