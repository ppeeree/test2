<template>
  <div :ref="chartId" style="width: 100%; height: 200px"></div>
</template>
<script>
import * as echarts from 'echarts'
let center = ['50%', '50%']
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
    pieDataOpt: {
      type: Array,
      default: () => [],
      require: true
    },
    legendData: {
      type: Array,
      default: () => [],
      require: true
    }
  },
  data () {
    return {
      chartId: `pie-${this.unique}`,
      chart: null,
      option: {
        color: ['#EAEA26', '#906BF9', '#FE5656', '#01E17E', '#3DD1F9', '#FFAD05'],
        grid: {
          left: 100,
          top: 50,
          bottom: 10,
          right: 10,
          containLabel: true
        },
        tooltip: {
          trigger: 'item',
          formatter: '{b} : {c} ({d}%)'
        },
        legend: {
          type: 'scroll',
          orient: 'vartical',
          // x: "right",
          top: 'center',
          right: '0',
          // bottom: "0%",
          itemWidth: 16,
          itemHeight: 8,
          itemGap: 16,
          textStyle: {
            color: '#A3E2F4',
            fontSize: 12,
            fontWeight: 0
          },
          data: this.legendData
        },
        polar: {},
        angleAxis: {
          interval: 1,
          type: 'category',
          data: [],
          z: 10,
          axisLine: {
            show: false,
            lineStyle: {
              color: '#0B4A6B',
              width: 1,
              type: 'solid'
            }
          },
          axisLabel: {
            interval: 0,
            show: true,
            color: '#0B4A6B',
            margin: 8,
            fontSize: 16
          }
        },
        radiusAxis: {
          min: 40,
          max: 120,
          interval: 20,
          axisLine: {
            show: false,
            lineStyle: {
              color: '#0B3E5E',
              width: 1,
              type: 'solid'
            }
          },
          axisLabel: {
            formatter: '{value} %',
            show: false,
            padding: [0, 0, 20, 0],
            color: '#0B3E5E',
            fontSize: 16
          },
          splitLine: {
            lineStyle: {
              color: '#0B3E5E',
              width: 2,
              type: 'solid'
            }
          }
        },
        calculable: true,
        series: [
          {
            type: 'pie',
            radius: ['5%', '10%'],
            center,
            hoverAnimation: false,
            labelLine: {
              normal: {
                show: false,
                length: 30,
                length2: 55
              },
              emphasis: {
                show: false
              }
            },
            data: [
              {
                name: '',
                value: 0,
                itemStyle: {
                  normal: {
                    color: '#0B4A6B'
                  }
                }
              }
            ]
          },
          {
            type: 'pie',
            radius: ['90%', '95%'],
            center,
            hoverAnimation: false,
            labelLine: {
              normal: {
                show: false,
                length: 30,
                length2: 55
              },
              emphasis: {
                show: false
              }
            },
            name: '',
            data: [
              {
                name: '',
                value: 0,
                itemStyle: {
                  normal: {
                    color: '#0B4A6B'
                  }
                }
              }
            ]
          },
          {
            stack: 'a',
            type: 'pie',
            radius: ['20%', '80%'],
            center,
            roseType: 'area',
            zlevel: 10,
            label: {
              normal: {
                show: true,
                formatter: '{c}',
                textStyle: {
                  fontSize: 12
                },
                position: 'outside'
              },
              emphasis: {
                show: true
              }
            },
            labelLine: {
              normal: {
                show: true,
                length: 20,
                length2: 55
              },
              emphasis: {
                show: false
              }
            },
            data: this.pieDataOpt
          }
        ]
      }
    }
  },
  computed: {
    // nodata() {
    //   return this.seriesData.length <= 0;
    // }
  },
  mounted: function () {
    this.chart = echarts.init(this.$refs[this.chartId])
    this.chart.setOption(this.option)
    window.addEventListener('resize', () => {
      this.chart.resize()
    })
  },
  watch: {
    legendData: {
      handler:function (val) {
        if (JSON.stringify(val) === '[]') {
          return
        }
        this.init()
        this.chart.setOption(this.option, true)
      },
      deep: true,
      // immediate: true
    }
  },
  methods: {
    init () {
      this.option.legend.data = this.legendData
      this.option.series.at(-1).data = this.pieDataOpt
    }
  }
}
</script>
<style lang="less" scoped>
.bar {
  position: relative;
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
