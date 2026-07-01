<template>
  <div :ref="chartId" :style="{ width, height }"></div>
</template>
<script>
import * as echarts from 'echarts'
import uniq from 'lodash/uniq'
export default {
  props: {
    unique: {
      //当要在同一个页面，显示多个图表
      type: [String, Number],
      default: 'index'
    },
    title: {
      type: String,
      default: ''
    },
    legend: {
      type: Array,
      default: () => ['危险', '警告', '注意', '安全']
    },
    width: {
      type: String,
      default: '100%'
    },
    height: {
      type: String,
      default: '100%'
    },
    xAxisData: {
      type: Array,
      default: () => []
    },
    seriesData: {
      type: Array,
      default: () => []
    }
  },
  data() {
    return {
      chartId: `pie-${this.unique}`,
      chart: null,
      observer: null,
      option: {
        title: {
          text: this.title,
          textStyle: {
            fontSize: '16',
            color: '#fff',
            fontFamily: 'Lato',
            foontWeight: '500'
          },
          top: '5%',
          left: '2%'
        },
        tooltip: {
          // show: false,
          trigger: 'axis'
        },
        toolbox: {
          show: false
        },
        legend: {
          icon: 'rect',
          itemHeight: 4,
          itemWidth: 20,
          data: [],
          textStyle: {
            color: '#fff'
          },
          top: '5%'
        },
        grid: {
          left: '3%',
          right: '4%',
          bottom: '3%',
          containLabel: true
        },
        xAxis: {
          type: 'category',
          boundaryGap: false,
          // data: ['Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'],
          axisLine: {
            show: true, //隐藏X轴轴线
            lineStyle: {
              color: '#fff'
            }
          },
          axisTick: {
            show: false //隐藏X轴刻度
          },
          axisLabel: {
            show: true,
            textStyle: {
              color: '#fff' //X轴文字颜色
            }
          }
        },
        yAxis: {
          type: 'value',
          axisLine: {
            show: false,
            lineStyle: {
              color: '#fff'
            }
          },
          splitLine: {
            show: true,
            lineStyle: {
              type: 'dashed',
              // dashOffset: 20,
              color: '#9EA3B4',
              opacity: 0.3
            }
          },
          axisLabel: {
            color: '#fff'
          },

          axisTick: {
            show: false
          }
        },
        series: []
      }
    }
  },
  computed: {
    // nodata() {
    //   return this.seriesData.length <= 0;
    // }
    isFly() {
      return this.unique === 'first'
    }
  },
  mounted: function () {
    this.chart = echarts.init(this.$refs[this.chartId])
    this.handleSeriesData(this.seriesData)
    this.chart.setOption(this.option)
    let dom = this.$refs[this.chartId]
    this.observer = new ResizeObserver(() => {
      this.chart.resize()
    })
    this.observer.observe(dom)
  },
  destroyed() {
    if (this.chart) {
      this.chart.dispose()
    }
    if (this.observer) {
      this.observer.disconnect()
    }
  },
  watch: {
    seriesData: {
      handler: function (val) {
        this.handleSeriesData(val)
        this.chart.setOption(this.option)
      },
      deep: true
    }
  },
  methods: {
    handleSeriesData(val) {
      if (val.length === 0) return
      this.option.series = []
      val.forEach((item, index) => {
        this.option.series.push({
          name: item.statusName,
          type: 'line',
          showSymbol: false,
          symbolSize: 10,
          smooth: false,
          lineStyle: {
            normal: {
              color: item.color
            }
          },
          itemStyle: {
            color: item.color,
            borderColor: item.color
          },
          data: item.data
        })
      })
      this.option.legend.data = val.map(item => item.statusName)
    }
    /*    handleLegend(val) {
      this.option.legend.data = uniq(val)
    },
    handleTime(val) {
      this.option.xAxis.data = val
    } */
  }
}
</script>
<style lang="less" scoped>
.bar {
  position: relative;
  width: 100%;
  height: 100%;
}
</style>
