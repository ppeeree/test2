<template>
  <div class="bar">
    <!-- <div v-show="nodata" class="nodata">无数据</div> -->
    <div :ref="chartId" style="width: 300px; height: 130px"></div>
  </div>
</template>
<script>
import * as echarts from 'echarts'
let color = new echarts.graphic.LinearGradient(0, 0, 1, 0, [
  {
    offset: 0,
    color: '#5CF9FE' // 0% 处的颜色
  },
  {
    offset: 0.17,
    color: '#468EFD' // 100% 处的颜色
  },
  {
    offset: 0.9,
    color: '#468EFD' // 100% 处的颜色
  },
  {
    offset: 1,
    color: '#5CF9FE' // 100% 处的颜色
  }
])
let rich = {
  white: {
    fontSize: 10,
    color: '#fff',
    fontWeight: '500',
    padding: [-10, 0, 0, 0]
  },
  bule: {
    fontSize: 14,
    fontFamily: 'DINBold',
    color: '#fff',
    fontWeight: '700',
    padding: [20, 0, 0, 0]
  },
  radius: {
    width: 130,
    height: 130,
    // lineHeight:80,
    borderWidth: 1,
    borderColor: '#0092F2',
    fontSize: 10,
    color: '#fff',
    backgroundColor: '#1B215B',
    borderRadius: 10,
    textAlign: 'center'
  }
}
let colorSet = [
  [0.91, color],
  [1, '#15337C']
]
export default {
  props: {
    unique: {
      //当要在同一个页面，显示多个图表
      type: [String, Number],
      default: 'index'
    },
    dataArr: {
      type: Array
    }
  },
  data () {
    return {
      chartId: `pie-${this.unique}`,
      option: {
        title: {
          text: '健康指数',
          left: 'center',
          textStyle: {
            fontSize: 15,
            color: '#fff'
          }
        },
        tooltip: {
          formatter: '{a} <br/>{b} : {c}%'
        },
        series: [
          {
            //内圆
            type: 'pie',
            radius: '12%',
            center: ['50%', '55%'],
            z: 0,
            itemStyle: {
              normal: {
                color: new echarts.graphic.RadialGradient(
                  0.5,
                  0.5,
                  1,
                  [
                    {
                      offset: 0,
                      color: 'rgba(17,24,43,0)'
                    },
                    {
                      offset: 0.5,
                      // color: '#1E2B57'
                      color: 'rgba(28,42,91,.6)'
                    },
                    {
                      offset: 1,
                      color: '#141C33'
                      // color:'rgba(17,24,43,0)'
                    }
                  ],
                  false
                ),
                label: {
                  show: false
                },
                labelLine: {
                  show: false
                }
              }
            },
            hoverAnimation: false,
            label: {
              show: false
            },
            tooltip: {
              show: false
            },
            data: [100]
          },
          {
            type: 'gauge',
            radius: '50%',
            center: ['50%', '55%'],
            startAngle: '225',
            endAngle: '-45',
            pointer: {
              show: false
            },
            detail: {
              formatter: function (value) {
                let num = Math.round(value)
                return '{bule|' + num + '}\n{white|分}' + '{size|' + '}'
              },
              rich: rich,
              offsetCenter: ['0%', '20%']
            },
            data: this.dataArr,
            title: {
              show: false
            },
            axisLine: {
              show: true,
              lineStyle: {
                color: colorSet,
                width: 13,
                shadowBlur: 15,
                shadowColor: '#B0C4DE',
                shadowOffsetX: 0,
                shadowOffsetY: 0,
                opacity: 1
              }
            },
            axisTick: {
              show: false
            },
            splitLine: {
              show: false,
              length: 25,
              lineStyle: {
                color: '#00377a',
                width: 2,
                type: 'solid'
              }
            },
            axisLabel: {
              show: false
            }
          },
          {
            //内圆
            type: 'pie',
            radius: '12%',
            center: ['50%', '55%'],
            z: 1,
            itemStyle: {
              normal: {
                color: new echarts.graphic.RadialGradient(
                  0.5,
                  0.5,
                  0.8,
                  [
                    {
                      offset: 0,
                      color: '#4978EC'
                    },
                    {
                      offset: 0.5,
                      color: '#1E2B57'
                    },
                    {
                      offset: 1,
                      color: '#141F3D'
                    }
                  ],
                  false
                ),
                label: {
                  show: false
                },
                labelLine: {
                  show: false
                }
              }
            },
            hoverAnimation: false,
            label: {
              show: false
            },
            tooltip: {
              show: false
            },
            data: [100]
          }
        ]
      }
    }
  },
  computed: {
    // nodata() {
    //   return this.seriesData.length <= 0
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
    dataArr: {
      handler:function (val) {
        if (val.length === 0) {
          return
        }
        this.option.series[1].data = val
        this.chart.setOption(this.option, true)
      },
      deep: true,
      immediate: true
    }
  },
  methods: {}
}
</script>
<style lang="less" scoped>
.bar {
  position: relative;
  margin-top: 35px;
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
