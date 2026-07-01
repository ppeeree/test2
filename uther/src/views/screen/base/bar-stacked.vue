<template>
  <div :ref="chartId" :style="{ width, height }"></div>
</template>
<script>
import * as echarts from 'echarts'
let color = ['#2ED133', '#7AD922', '#BFE012', '#FFC307', '#FFA409', '#FF660F', '#FF6B0E', '#FF0F0D']
export default {
  props: {
    unique: {
      //当要在同一个页面，显示多个图表
      type: [String, Number],
      default: 'index'
    },
    title: {
      type: String,
      default: '',
      required: true
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
      default: () => [
        {
          title: '1001#',
          data: 20
        },
        {
          title: '1002#',
          data: 30
        },
        {
          title: '1003#',
          data: 40
        },
        {
          title: '1004#',
          data: 50
        },
        {
          title: '1005#',
          data: 60
        },
        {
          title: '1006#',
          data: 85
        },
        {
          title: '1007#',
          data: 90
        },
        {
          title: '1008#',
          data: 100
        }
      ]
    }
  },
  data() {
    return {
      chartId: `pie-${this.unique}`,
      chart: null,
      option: {
        title: {
          show: false
        },
        tooltip: {
          trigger: 'item'
        },
        grid: {
          borderWidth: 0,
          top: '0%',
          left: '5%',
          right: '15%',
          bottom: '3%'
        },
        color: color,
        yAxis: [
          {
            type: 'category',
            inverse: true,
            axisTick: {
              show: false
            },
            axisLine: {
              show: false
            },
            axisLabel: {
              show: false,
              inside: false
            },
            data: []
          },
          {
            type: 'category',
            axisLine: {
              show: false
            },
            axisTick: {
              show: false
            },
            axisLabel: {
              show: true,
              inside: false,
              textStyle: {
                color: '#fff',
                fontSize: '16'
                // fontFamily: 'Arail'
              },
              formatter: function (val) {
                return `${val}`
              }
            },
            splitArea: {
              show: false
            },
            splitLine: {
              show: false
            },
            data: []
          }
        ],
        xAxis: {
          type: 'value',
          axisTick: {
            show: false
          },
          axisLine: {
            show: false
          },
          splitLine: {
            show: false
          },
          axisLabel: {
            show: false
          }
        },
        series: [
          {
            name: '',
            type: 'bar',
            barWidth: '38px',
            data: [0, 0, 0, 0, 0, 0, 0, 0, 0],
            silent: true,
            itemStyle: {
              normal: {
                color: '#173A62'
              }
            }
          },
          {
            name: '',
            type: 'bar',
            zlevel: 2,
            barWidth: '10px',
            animationDuration: 1500,
            data: [100, 100, 100, 100, 100, 100, 100],
            barGap: '-100%',
            barCategoryGap: '40%',
            silent: false,
            itemStyle: {
              normal: {
                color: 'rgba(68,84,97,0.4)',
                borderWidth: 0,
                barBorderRadius: [15, 15, 15, 15]
              }
            }
          },
          {
            name: '',
            type: 'bar',
            zlevel: 2,
            barWidth: '10px',
            data: [],
            animationDuration: 1500,
            label: {
              normal: {
                color: '#fff',
                show: true,
                position: [0, 18],
                textStyle: {
                  fontSize: 12
                },
                formatter: function (a) {
                  return a.name
                }
              }
            }
          }
        ],
        animationEasing: 'cubicOut'
      }
    }
  },
  mounted: function () {
    this.$bus.$on('barStacked', () => {
      this.disClearPatch()
    })
    this.chart = echarts.init(this.$refs[this.chartId])
    this.chart.setOption(this.option)
    window.addEventListener('resize', () => {
      this.chart.resize()
    })
    this.chartEvent()
  },
  watch: {
    seriesData: {
      handler: function(val) {
        const xList = []
        const yData = []
        const lineY = val.map((item, index) => {
          xList.push(item.title)
          yData.push(item.data)
          return {
            name: item.title,
            color: color[val.length - index - 1],
            value: item.data,
            itemStyle: {
              normal: {
                show: true,
                color: color[val.length - index - 1],
                barBorderRadius: 10
              },
              emphasis: {
                shadowBlur: 15,
                shadowColor: 'rgba(0, 0, 0, 0.1)'
              }
            }
          }
        })
        this.option.yAxis[0].data = xList
        this.option.yAxis[1].data = yData.reverse()
        this.option.series[2].data = lineY
      },
      immediate: true,
      deep: true
    }
  },
  methods: {
    chartEvent() {
      this.chart.on('click', this.handleClickEvent)
    },
    handleClickEvent(param, isParam = true) {
      isParam && this.closeOther()
      let arr = new Array(8).fill(0)
      isParam && (arr[param.dataIndex] = 100)
      this.option.series[0].data = arr
      this.chart.setOption(this.option)
    },
    disClearPatch() {
      this.handleClickEvent({}, false)
    },
    closeOther() {
      this.$bus.$emit('stateTypeClick')
      this.$bus.$emit('progressClick')
      this.$bus.$emit('leftTalbe')
      this.$bus.$emit('pieDoughnut', {
        all: true,
        id: null
      })
      this.$bus.$emit('pieDoughnutRe', {
        all: true,
        id: null
      })
      this.$bus.$emit('levelEvent', {
        all: true,
        id: null
      })
    }
  }
}
</script>
<style lang="less" scoped>
.bar {
  position: relative;
  top: 10px;
  width: 100%;
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
