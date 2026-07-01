<template>
  <div class="bar">
    <div :ref="chartId" :style="{width,height}"></div>
  </div>
</template>
<script>
import * as echarts from 'echarts'
// let value = 70
let color = new echarts.graphic.LinearGradient(0, 0, 1, 0, [
  {
    offset: 0,
    color: '#FF0F0D' // 0% 处的颜色
  },
  {
    offset: 0.3,
    color: '#FF6B0E' // 100% 处的颜色
  },
  {
    offset: 0.7,
    color: '#FFE604' // 100% 处的颜色
  },
  {
    offset: 1,
    color: '#2ED133' // 100% 处的颜色
  }
])
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
    num: {
      type: Number,
      default: () => 0,
      required: true
    },
    width: {
      type: String,
      default: '130px'
    },
    height: {
      type: String,
      default: '100px'
    }
  },
  data() {
    return {
      chartId: `pie-${this.unique}`,
      chart: null,
      option: {
        angleAxis: {
          show: false,
          max: (100 * 360) / 180, //-45度到225度，二者偏移值是270度除360度
          type: 'value',
          startAngle: 180, //极坐标初始角度
          splitLine: {
            show: false
          }
        },
        graphic: [
          {
            type: 'image',
            id: 'logo',
            right: 'center',
            top: '16%',
            z: 0,
            bounding: 'all',
            style: {
              image: '/img/screen/echarts/bg1.png',
              width: 100,
              height: 100
            }
          }
        ],
        barMaxWidth: 13, //圆环宽度
        radiusAxis: {
          show: false,
          type: 'category'
        },
        //圆环位置和大小
        polar: {
          center: ['50%', '70%'],
          radius: '150%'
        },
        series: [
          {
            type: 'bar',
            data: [
              {
                //上层圆环，显示数据
                value: this.num,
                itemStyle: {
                  color
                }
              }
            ],
            barGap: '-100%', //柱间距离,上下两层圆环重合
            coordinateSystem: 'polar',
            roundCap: true, //顶端圆角
            z: 3 //圆环层级，同zindex
          },
          {
            //下层圆环，显示最大值
            type: 'bar',
            data: [
              {
                value: 100,
                itemStyle: {
                  color: '#255873',
                  opacity: 1,
                  borderWidth: 0
                }
              }
            ],
            barGap: '-100%',
            coordinateSystem: 'polar',
            roundCap: true,
            z: 1
          },
          // {
          //   type: 'pie',
          //   tooltip: {
          //     show: false
          //   },
          //   hoverAnimation: false,
          //   legendHoverLink: false,
          //   radius: ['0%', '38%'],
          //   center: ['50%', '68%'],
          //   label: {
          //     normal: {
          //       show: false
          //     }
          //   },
          //   labelLine: {
          //     normal: {
          //       show: false
          //     }
          //   },
          //   data: [
          //     {
          //       value: 120,
          //       itemStyle: {
          //         normal: {
          //           color: '#23534D'
          //         }
          //       }
          //     }
          //   ]
          // },
          //仪表盘
          {
            type: 'gauge',
            startAngle: 225, //起始角度，同极坐标
            endAngle: -45, //终止角度，同极坐标
            z: 4,
            axisLine: {
              show: false
            },
            splitLine: {
              show: false
            },
            axisTick: {
              show: false
            },
            axisLabel: {
              show: false
            },
            splitLabel: {
              show: false
            },
            pointer: {
              // 分隔线
              show: false, //是否显示指针
              shadowColor: 'auto', //默认透明
              shadowBlur: 5,
              length: '50%',
              width: '2'
            },

            itemStyle: {
              color: '#1598FF',
              borderColor: '#1598FF',
              borderWidth: 3
            },
            detail: {
              formatter: function (value) {
                return value
              },
              color: '#fff',
              fontSize: 20,
              offsetCenter: [0, 21]
            },
            title: {
              show: false
            },
            data: [
              {
                value: this.num
              }
            ]
          }
        ]
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
    this.init(this.num)
    this.chart.setOption(this.option)
    window.addEventListener('resize', () => {
      this.chart.resize()
    })
  },
  watch: {
    num: {
      handler: function(val) {
        this.init(val)
        this.chart.setOption(this.option)
      }
    }
  },
  methods: {
    init(val) {
      this.option.series[0].data[0].value = val
      this.option.series[2].data[0].value = val
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
