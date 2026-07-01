<template>
  <div :ref="chartId" :style="{ width, height }"></div>
</template>
<script>
import * as echarts from 'echarts'
let fontColor = '#fff'
export default {
  props: {
    unique: {
      //当要在同一个页面，显示多个图表
      type: [String, Number],
      default: 'index'
    },
    title: {
      type: String,
      default: '健康指数趋势'
    },
    width: {
      type: String,
      default: '100%'
    },
    height: {
      type: String,
      default: '100%'
    },
    seriesData: {
      type: Array,
      default: () => {
        return [
          {
            name: '无人机',
            color: '#01D0F9',
            area: ['rgba(1,208,249,0)', 'rgba(1,208,249,0.5)', 'rgba(1,208,249,1)'],
            data: [40, 52, 11, 14, 50],
            time: ['00:00', '04:00', '08:00', '12:00', '16:00']
          },
          {
            name: '机巢',
            color: '#25B93D',
            area: ['rgba(37,185,61, 0)', 'rgba(37,185,61, 0.5)', 'rgba(37,185,61, 1)'],
            data: [20, 32, 101, 34, 90],
            time: ['00:00', '04:00', '08:00', '12:00', '16:00']
          }
        ]
      }
    }
  },
  data() {
    return {
      chartId: `pie-${this.unique}`,
      chart: null,
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
        grid: {
          left: '3%',
          right: '4%',
          bottom: '3%',
          containLabel: true
        },
        tooltip: {
          trigger: 'axis',
          axisPointer: {
            type: 'cross',
            label: {
              backgroundColor: '#6a7985'
            }
          }
        },
        legend: {
          icon: 'Rect',
          orient: 'horizontal',
          itemWidth: 20,
          itemHeight: 2,
          textStyle: {
            fontSize: 12, //字体大小
            color: fontColor //字体颜色
          },
          top: '5%',
          right: 'center' //距离右侧
        },
        xAxis: [
          {
            type: 'category',
            boundaryGap: false,
            axisLabel: {
              color: fontColor
            },
            axisLine: {
              show: true,
              lineStyle: {
                color: '#fff'
              }
            },
            axisTick: {
              show: false
            },
            splitLine: {
              show: false
            },
            data: []
          }
        ],
        yAxis: [
          {
            type: 'value',
            splitNumber: 5,
            splitLine: {
              lineStyle: {
                type: 'dashed',
                color: '#9EA3B4',
                opacity: 0.3
              }
            },
            axisLabel: {
              textStyle: {
                color: '#D6E1FF'
              }
            }
          }
        ],
        series: []
      }
    }
  },
  mounted: function () {
    this.chart = echarts.init(this.$refs[this.chartId])
    this.init()
    this.chart.setOption(this.option)
    // window.addEventListener('resize', () => {
    //   this.chart.resize()
    // })
    let dom = this.$refs[this.chartId]
    let ro = new ResizeObserver(() => {
      this.chart.resize()
    })
    ro.observe(dom)
  },
  watch: {
    title: {
      handler: function (val) {
        this.option.title.text = val
        this.chart.setOption(this.option)
      }
    },
    seriesData: {
      handler: function (val) {
        this.init(val)
        this.chart.setOption(this.option)
      },
      deep: true
    }
  },
  methods: {
    init(val = this.seriesData) {
      let optData = []
      val.forEach(e => {
        optData.push({
          name: e.name,
          type: 'line',
          smooth: true,
          symbol: 'circle',
          symbolSize: 1,
          areaStyle: {
            color: new echarts.graphic.LinearGradient(
              0,
              0,
              0,
              1,
              [
                {
                  offset: 0,
                  color: e.area[0]
                },
                {
                  offset: 0.5,
                  color: e.area[1]
                },
                {
                  offset: 1,
                  color: e.area[2]
                }
              ],
              false
            )
          },
          lineStyle: {
            color: e.color,
            width: 3
          },
          itemStyle: {
            color: e.color
          },

          data: e.data
        })
      })
      this.option.series = optData
      this.option.xAxis[0].data = val[0].time
    }
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
