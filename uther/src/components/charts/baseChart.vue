<template>
  <div ref="echartId" :style="{ height: height || '300px', width: width || '100%' }"></div>
</template>

<script>
import * as echarts from 'echarts'
import { mapState } from 'vuex'

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
      default: '100%'
    },
    height: {
      type: String,
      default: '100%'
    }
  },
  data() {
    return {
      chart: null
    }
  },
  computed: {
    ...mapState({
      showCollapse: state => state.common.showCollapse,
      isCollapse: state => state.common.isCollapse
    })
  },
  // 1.监听菜单收缩变化 图表自适应 2.监听数据变化 初始化图表
  watch: {
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
        this.chart && this.chart.setOption(normalizeEchartsOption(val), true)
      }
    }
  },
  updated() {},
  mounted() {
    this.$nextTick(() => {
      this.initChart()
      this.echartsOnClick()
    })
    this.resizeHandler = new ResizeObserver(() => {
      this.chart && this.chart.resize()
    })
    this.resizeHandler.observe(this.$refs.echartId)
  },
  beforeUnmount() {
    this.removeResizeOb()
    if (!this.chart) {
      return
    }
    this.chart.dispose()
    this.chart = null
  },
  methods: {
    initChart() {
      this.chart = echarts.init(this.$refs.echartId)
      this.chart.setOption(normalizeEchartsOption(this.chartData), true)
    },
    removeResizeOb() {
      if (this.resizeHandler) {
        this.resizeHandler.disconnect()
        this.resizeHandler = null
      }
    },
    // 监听浏览器窗口变化 图表自适应
    /*  initResizeEvent() {
      window.addEventListener('resize', this.resizeHandler)
    },
    destroyResizeEvent() {
      window.removeEventListener('resize', this.resizeHandler)
    }, */
    /**
     * 图标添加点击事件
     */
    echartsOnClick() {
      const _this = this
      this.chart.on('click', params => {
        params.data && _this.$emit('itemCilck', params.data)
        console.log(params.data, '图表点击事件')
      })
    }
  }
}
</script>
<style lang="scss" scoped></style>
