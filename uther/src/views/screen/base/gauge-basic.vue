<template>
  <!-- <div v-show="nodata" class="nodata">无数据</div> -->
  <div :ref="chartId" style="width: 200px; height: 300px"></div>
</template>
<script>
import * as echarts from 'echarts'
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
let colorSet = [
  [0.83, color]
  // [1, '#255873']
]
let colorSet1 = [[1, '#255873']]
let rich = {
  // white: {
  //   fontSize: 20,
  //   color: '#fff',
  //   fontWeight: '500',
  //   padding: [-150, 0, 0, 0]
  // },
  bule: {
    fontSize: 35,
    fontFamily: 'DINBold',
    color: '#fff',
    padding: [-75, 0, 0, 0]
  },
  radius: {
    width: 76,
    height: 25,
    // lineHeight:80,
    borderWidth: 0.7,
    borderColor: '#7CB7FF',
    fontSize: 15,
    color: '#7CB7FF',
    backgroundColor: 'rgba(27,33,91,0)',
    borderRadius: 5,
    textAlign: 'center'
  },
  size: {
    height: 400,
    padding: [0, 0, 0, 0]
  }
}
export default {
  props: {
    unique: {
      //当要在同一个页面，显示多个图表
      type: [String, Number],
      default: 'index'
    },
    dataArr: {
      type: Array,
      default: () => {
        let arr = [
          {
            value: 68,
            name: '健康指数'
          }
        ]
        return arr
      },
      required: true
    }
  },
  data() {
    return {
      chartId: `pie-${this.unique}`,
      chart: null,
      option: {
        title: {
          text: '测试风电场',
          left: 'center',
          bottom: '5%',
          textStyle: {
            color: '#fff'
          }
        },
        tooltip: {
          formatter: '{a} <br/>{b} : {c}%'
        },
        graphic: [
          {
            type: 'image',
            id: 'logo',
            right: 'center',
            bottom: 'center',
            z: 0,
            bounding: 'all',
            style: {
              image: '/img/screen/echarts/bg1.png',
              width: 150,
              height: 150
            }
          }
        ],
        series: [
          {
            //内圆
            type: 'pie',
            radius: '35%',
            center: ['50%', '50%'],
            itemStyle: {
              normal: {
                color: new echarts.graphic.RadialGradient(
                  1,
                  1,
                  0,
                  [
                    {
                      offset: 0,
                      color: 'rgba(0,102,255,0)'
                    },
                    {
                      offset: 0.5,
                      // color: '#1E2B57'
                      color: 'rgba(0,102,255,0.6)'
                    },
                    {
                      offset: 1,
                      color: 'rgba(0,102,255,1)'
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
            radius: '60%',
            z: 10,
            startAngle: '225',
            endAngle: '-45',
            pointer: {
              show: false
            },
            detail: {
              formatter: function (value) {
                let num = Math.round(value)
                return '{bule|' + num + '}' + '{size|' + '}\n{radius|健康指数}'
              },
              rich: rich,
              offsetCenter: ['0%', '93%']
            },
            data: this.dataArr,
            title: {
              show: false
            },
            axisLine: {
              show: true,
              roundCap: true,
              lineStyle: {
                color: colorSet,
                width: 10,
                // shadowBlur: 15,
                // shadowColor: '#B0C4DE',
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
                width: 25,
                type: 'solid'
              }
            },
            axisLabel: {
              show: false
            }
          },
          // 背景园
          {
            type: 'gauge',
            radius: '60%',
            startAngle: '225',
            endAngle: '-45',
            pointer: {
              show: false
            },
            detail: {
              formatter: function (value) {
                let num = Math.round(value)
                return '{bule|' + num + '}' + '{size|' + '}\n{radius|健康指数}'
              },
              rich: rich,
              offsetCenter: ['0%', '93%']
            },
            data: this.dataArr,
            title: {
              show: false
            },
            axisLine: {
              show: true,
              roundCap: true,
              lineStyle: {
                color: colorSet1,
                width: 10,
                // shadowBlur: 15,
                // shadowColor: '#B0C4DE',
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
                width: 25,
                type: 'solid'
              }
            },
            axisLabel: {
              show: false
            }
          },
          // 刻度外圆白线
          // {
          //   name: '灰色内圈', //刻度背景
          //   type: 'gauge',
          //   z: 2,
          //   radius: '45%',
          //   startAngle: '225',
          //   endAngle: '-45',
          //   //center: ["50%", "75%"], //整体的位置设置
          //   axisLine: {
          //     // 坐标轴线
          //     lineStyle: {
          //       // 属性lineStyle控制线条样式
          //       color: [[1, '#2C7DB8']],
          //       width: 1.3,
          //       opacity: 1 //刻度背景宽度
          //     }
          //   },
          //   splitLine: {
          //     show: false
          //   },
          //   // data: [{
          //   //     show: false,
          //   //     value: '80'
          //   // }], //作用不清楚
          //   axisLabel: {
          //     show: false
          //   },
          //   pointer: {
          //     show: false
          //   },
          //   axisTick: {
          //     show: false
          //   },
          //   detail: {
          //     show: 0
          //   }
          // },
          // 外层线圈
          {
            name: '灰色内圈', //刻度背景
            type: 'gauge',
            z: 2,
            radius: '95%',
            startAngle: '225',
            endAngle: '-45',
            //center: ["50%", "75%"], //整体的位置设置
            axisLine: {
              // 坐标轴线
              lineStyle: {
                // 属性lineStyle控制线条样式
                color: [[1, '#356683']],
                width: 2,
                opacity: 1 //刻度背景宽度
              }
            },
            axisLabel: {
              show: true,
              color: '#fff',
              distance: -8,
              formatter: function (v) {
                switch (v + '') {
                  case '0':
                    return '0'
                  case '10':
                    return ''
                  case '20':
                    return '20'
                  case '30':
                    return ''
                  case '40':
                    return '40'
                  case '50':
                    return ''
                  case '60':
                    return '60'
                  case '70':
                    return ''
                  case '80':
                    return '80'
                  case '90':
                    return ''
                  case '100':
                    return '100'
                }
              }
            }, //刻度标签。
            splitLine: {
              show: false
            },
            // axisLabel: {
            //   show: false
            // },
            pointer: {
              show: false
            },
            axisTick: {
              show: false
            },
            detail: {
              show: 0
            }
          },
          // 刻度
          {
            name: '白色圈刻度',
            type: 'gauge',
            radius: '63%',
            startAngle: 225, //刻度起始
            endAngle: -45, //刻度结束
            z: 4,
            axisTick: {
              show: false
            },
            splitLine: {
              length: 8, //刻度节点线长度
              lineStyle: {
                width: 3,
                color: 'auto'
              }, //刻度节点线
              show: true
            },
            axisLabel: {
              show: false
            }, //刻度节点文字颜色
            pointer: {
              show: false
            },
            axisLine: {
              show: false,
              lineStyle: {
                color: [
                  [0, '#FF0F0D'],
                  [0.3, '#FF6B0E'],
                  [0.7, '#FFE604'],
                  [1, '#2ED133']
                ]
              }
            },
            detail: {
              show: false
            },
            data: [
              {
                value: 0,
                name: ''
              }
            ]
          }
          // {
          //   name: '外部刻度',
          //   type: 'gauge',
          //   //  center: ['20%', '50%'],
          //   radius: '100%',
          //   min: 0, //最小刻度
          //   max: 100, //最大刻度
          //   splitNumber: 5, //刻度数量
          //   startAngle: 225,
          //   endAngle: -45,
          //   axisLine: {
          //     show: false,
          //     // 仪表盘刻度线
          //     lineStyle: {
          //       width: 2,
          //       color: [[1, '#FFFFFF']]
          //     }
          //   },
          //   //仪表盘文字
          //   axisLabel: {
          //     show: true,
          //     color: '#fff',
          //     distance: 25,
          //     formatter: function (v) {
          //       switch (v + '') {
          //         case '0':
          //           return '0'
          //         case '10':
          //           return '10'
          //         case '20':
          //           return '20'
          //         case '30':
          //           return '30'
          //         case '40':
          //           return '40'
          //         case '50':
          //           return '50'
          //         case '60':
          //           return '60'
          //         case '70':
          //           return '70'
          //         case '80':
          //           return '80'
          //         case '90':
          //           return '90'
          //         case '100':
          //           return '100'
          //       }
          //     }
          //   }, //刻度标签。
          //   axisTick: {
          //     show: true,
          //     splitNumber: 7,
          //     lineStyle: {
          //       color: '#fff', //用颜色渐变函数不起作用
          //       width: 2
          //     },
          //     length: -8
          //   }, //刻度样式
          //   splitLine: {
          //     show: false,
          //     length: -20,
          //     lineStyle: {
          //       color: '#fff' //用颜色渐变函数不起作用
          //     }
          //   }, //分隔线样式
          //   detail: {
          //     show: false
          //   },
          //   pointer: {
          //     show: false
          //   }
          // }
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
      handler: function (val) {
        if (val.length === 0) {
          return
        }
        this.option.series[1].data = val
        this.chart.setOption(this.option, true)
      },
      deep: true
    }
  },
  methods: {}
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
