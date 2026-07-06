<template>
  <div class="bar">
    <div :ref="chartId" style="width: 460px; height: 170px"></div>
  </div>
</template>
<script>
import * as echarts from 'echarts'
import { eventTypeEnum, levelColorEnum } from '@/util/constant'
const MAX_VALUE = 100

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
    dataList: {
      type: Array,
      default: () => []
    },
    numSqrObj: {
      type: Object,
      default: () => {
        return {}
      }
    }
  },
  data() {
    return {
      chartId: `pie-${this.unique}`,
      chart: null,
      option: {
        title: {
          show: false,
          text: '具体分布',
          textStyle: {
            color: '#FFFFFF',
            fontWeight: 'normal',
            fontFamily: 'SourceHanSansCN-Regular',
            fontSize: 13,
            lineHeight: 28
          }
        },
        tooltip: {
          trigger: 'axis',
          backgroundColor: '#fff',
          textStyle: {
            color: '#333'
          },
          extraCssText: 'box-shadow: 1px 1px 4px rgba(0, 0, 0, 0.3);',
          axisPointer: {
            type: 'shadow'
          },
          // 定义显示tooltip，参数请使用console.log(params) 查看
          formatter: arr => {
            const lines = [arr[0].axisValue]
            arr.forEach(o => {
              if (o.value != null && o.value !== 0) {
                lines.push(`${o.marker}${o.seriesName}: ${o.value}%`)
              }
            })
            return lines.join('<br/>')
          }
        },
        dataset: {
          source: this.dataList
        },
        grid: {
          top: '10',
          bottom: -0,
          left: '20',
          containLabel: true
        },
        xAxis: {
          type: 'category',
          axisLabel: {
            color: '#fff',
            rotate: 0 //倾斜度
          },
          axisLine: {
            lineStyle: {
              type: 'dashed',
              color: 'rgba(150, 164, 244, 0.3)'
            },
            width: 1
          },
          axisTick: {
            show: false
          }
        },
        yAxis: {
          type: 'value',
          position: 'right',
          axisLabel: {
            color: '#fff',
            formatter: function (val) {
              return parseInt((val / MAX_VALUE) * 100) + '%'
            }
          },
          axisLine: {
            lineStyle: {
              color: '#fff'
            },
            width: 5
          },
          axisTick: {
            show: false
          },
          splitLine: {
            lineStyle: {
              type: 'dashed',
              color: 'rgba(150, 164, 244, 0.3)'
            }
          }
        },
        series: []
      }
    }
  },
  computed: {},
  mounted: function () {
    this.$nextTick(() => {
      const chartDom = this.$refs[this.chartId]
      if (!chartDom) return
      this.chart = echarts.init(chartDom)
      this.initChart()
      window.addEventListener('resize', this.handleResize)
    })
  },
  beforeUnmount() {
    window.removeEventListener('resize', this.handleResize)
    if (this.chart) {
      this.chart.dispose()
      this.chart = null
    }
  },
  watch: {
    dataList: {
      handler: function (val) {
        this.initChart()
      },
      deep: true
    }
  },
  methods: {
    handleResize() {
      if (this.chart) {
        this.chart.resize()
      }
    },
    initChart() {
      if (!this.chart || !this.dataList.length) return
      this.option.xAxis.data = this.dataList.map(i => i.statusTime)
      // 1. 收集所有出现过的状态名
      const states = [...new Set(this.dataList.flatMap(d => d.data.map(([s]) => s)))]
      let total = this.dataList[0].data.reduce((pre, cur) => {
        return pre + cur[1]
      }, 0)
      this.option.series = states.map(i => {
        return {
          name: eventTypeEnum[i],
          type: 'bar',
          coordinateSystem: 'cartesian2d',
          stack: 'all',
          barWidth: 20,
          color: levelColorEnum[i],
          data: this.dataList.map(d => {
            const item = d.data.find(([s]) => s === i)
            const v = item ? item[1] : null
            return v === 0 ? null : (v / total) * MAX_VALUE
          })
        }
      })
      this.chart.setOption(this.option)
    }
  }
}
</script>
<style lang="less" scoped>
.bar {
  position: relative;
  width: 100%;
  bottom: 9px;
  .nodata {
    position: absolute;
    top: 0;
    z-index: 100;
    width: 100%;
    height: 280px;
    background-color: #fff;
    display: flex;
    justify-content: center;
    align-items: center;
  }
}
</style>
