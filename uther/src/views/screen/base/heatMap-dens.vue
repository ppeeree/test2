<template>
  <div :ref="chartId" :style="{ width, height }"></div>
</template>
<script>
import * as echarts from 'echarts'

let AQI_LEVELS = [
  {
    min: -10,
    max: 0,
    color: '#4E6278'
  },
  {
    min: 0,
    max: 30,
    color: '#FF0F0D'
  },
  {
    min: 30,
    max: 60,
    color: '#FF6B0E'
  },
  {
    min: 60,
    max: 99,
    color: '#FFE604'
  },
  {
    min: 99,
    max: 100,
    color: '#2ED133'
  },
  {
    min: 100,
    max: '+',
    color: '#2ED133'
  }
]

const colorRange = (startColor, endColor) => ({
  type: 'linear',
  // x: 1,
  // y: 0.5,
  colorStops: [
    { offset: 0, color: startColor },
    { offset: 0.5, color: startColor },
    { offset: 0.5, color: endColor },
    { offset: 1, color: endColor }
  ],
  global: true
})

export default {
  props: {
    unique: {
      //当要在同一个页面，显示多个图表
      type: [String, Number],
      default: 'index'
    },
    title: {
      type: String,
      default: '90天内设备健康变化趋势'
    },
    width: {
      type: String,
      default: '100%'
    },
    height: {
      type: String,
      default: '40px'
    },
    legend: {
      type: Array,
      default: () => ['Chr01', 'Chr02', 'Chr03', 'Chr04', 'Chr05', 'Chr06', 'Chr07']
    },
    seriesData: {
      type: Array,
      default: () => {
        return [
          [1, 0, 29347],
          [1, 1, 30988],
          [1, 2, 39432],
          [1, 3, 33406],
          [1, 4, 31455],
          [1, 5, 28189],
          [1, 6, 30125],
          [1, 7, 32800],
          [1, 8, 35085],
          [1, 9, 35448],
          [1, 10, 31886],
          [1, 11, 32252],
          [1, 12, 36602],
          [1, 13, 34415],
          [1, 14, 32204],
          [1, 15, 29886],
          [1, 16, 32927],
          [1, 17, 27369],
          [1, 18, 31644],
          [1, 19, 29025],
          [1, 20, 35832],
          [1, 21, 36785],
          [1, 22, 44284],
          [1, 23, 31072],
          [1, 24, 31226],
          [1, 25, 27325],
          [1, 26, 30570],
          [1, 27, 31700],
          [1, 28, 29573],
          [1, 29, 30883],
          [1, 30, 36886],
          [1, 31, 36551],
          [1, 32, 42812],
          [1, 33, 43955],
          [1, 34, 45543],
          [1, 35, 41317],
          [1, 36, 36036],
          [1, 37, 34297],
          [1, 38, 38088],
          [1, 39, 36746],
          [1, 40, 40991],
          [1, 41, 43321],
          [1, 42, 35813],
          [1, 43, 37090],
          [1, 44, 41736],
          [1, 45, 37179],
          [1, 46, 35738],
          [1, 47, 32705],
          [1, 48, 31669],
          [1, 49, 31067],
          [1, 50, 30019],
          [1, 51, 27886],
          [1, 52, 3773]
        ]
      }
    }
  },
  data() {
    return {
      chartId: `pie-${this.unique}`,
      chart: null,
      option: {
        // title: {
        //   text: this.title,
        //   textStyle: {
        //     fontSize: '16',
        //     color: '#fff',
        //     fontFamily: 'Lato',
        //     foontWeight: '500'
        //   },
        //   top: '3%',
        //   left: 'center'
        // },
        tooltip: {
          trigger: 'item',
          confine: true,
          formatter: item => {
            let hotmaptest = ''
            let { data, marker } = item
            hotmaptest += data[0] + '<br/>'
            hotmaptest += '<div>'
            hotmaptest += '<div style="display: flex;flex-direction: row;justify-content: center;">'
            hotmaptest += '<div>'
            hotmaptest += marker
            // hotmaptest += `<span>${this.handleStatusName(data[2])}</span>`
            hotmaptest += '</div>'
            hotmaptest += `<span>${data[2] < 0 ? 0 : data[2]}</span>`
            hotmaptest += '</div>'
            hotmaptest += '</div>'
            return hotmaptest
          }
        },
        grid: {
          left: '2%',
          right: '2%',
          bottom: '-22%',
          top: '2%',
          containLabel: true
        },
        xAxis: {
          name: 'Chromosomal position /Mb',
          show: false,
          nameLocation: 'middle',
          nameGap: 30,
          type: 'category',
          splitArea: {
            show: false
          },
          axisTick: {
            show: true
          },
          axisLine: {
            show: true,
            lineStyle: {
              color: '#fff'
            }
          }
          // grid:{
          //     height:'60%'
          // }
        },
        yAxis: {
          name: '',
          type: 'category',
          data: [],
          axisLine: {
            show: false,
            lineStyle: {
              color: '#fff'
            }
          },
          axisTick: {
            show: false
          },
          splitArea: {
            show: false
          },
          splitLine: {
            show: false,
            lineStyle: {
              color: 'rgba(4, 17, 33, 1)',
              width: 10
            }
          }
        },
        visualMap: {
          // min: 0,
          // max: 6,
          // calculable: true,
          // orient: 'horizontal', //水平放置
          // type: 'piecewise',
          // itemWidth: 10,
          // itemHeight: 10,
          // itemSymbol: 'circle',
          // top: '5%',
          // left: '50%',
          pieces: [
            {
              lt: 0,
              label: '无状态',
              color: '#4E6278'
            },
            {
              gt: 0,
              lt: 30,
              label: '危险',
              color: '#FF0F0D'
            },
            {
              gt: 30,
              lt: 60,
              label: '警告',
              color: '#FF6B0E'
            },
            {
              gt: 60,
              lt: 99,
              label: '注意',
              color: '#FFE604'
            },
            {
              gte: 99,
              // lt: 2,
              label: '正常',
              color: '#2ED133'
            }
          ],
          textStyle: {
            color: '#D6DADE'
          },
          show: false
        },
        /* visualMap: [
          {
            show: false,
            type: 'piecewise',
            pieces: [
              ...AQI_LEVELS.map(v => ({
                lt: v.max,
                gte: v.min,
                color: v.color
              })),
              {
                value: AQI_LEVELS[0].max,
                color: colorRange(AQI_LEVELS[0].color, AQI_LEVELS[1].color)
              },
              {
                value: AQI_LEVELS[1].max,
                color: colorRange(AQI_LEVELS[1].color, AQI_LEVELS[2].color)
              },
              {
                value: AQI_LEVELS[2].max,
                color: colorRange(AQI_LEVELS[2].color, AQI_LEVELS[3].color)
              },
              {
                value: AQI_LEVELS[3].max,
                color: colorRange(AQI_LEVELS[3].color, AQI_LEVELS[3].color)
              }
            ]
          }
        ],*/
        series: [
          {
            name: '健康指数',
            type: 'heatmap',
            data: [],
            label: {
              normal: {
                show: false
              }
            },
            itemStyle: {
              // borderColor: '#fff',
              //   borderWidth: 1,
              emphasis: {
                shadowBlur: 10,
                shadowColor: 'rgba(0, 0, 0, 0.5)'
              }
            },
            zlevel: -1
          }
        ]
      }
    }
  },
  mounted: function () {
    this.chart = echarts.init(this.$refs[this.chartId])
    this.init(this.seriesData)
    // this.chart.setOption(this.option)
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
    legend: {
      handler: function (val) {
        this.option.yAxis.data = val
        this.chart.setOption(this.option)
      }
      // immediate: true
    },
    seriesData: {
      handler: function (val) {
        this.option.series[0].data = val.map(item => [item[1], item[0], item[2] || '-'])
        this.chart.setOption(this.option)
      },
      deep: true
      // immediate: true
    }
  },
  methods: {
    init(val) {
      this.option.yAxis.data = this.legend
      this.option.series[0].data = val.map(item => [item[1], item[0], item[2] || '-'])
      this.chart.setOption(this.option)
    },
    handleStatusName(val) {
      if (val > 75) {
        return '正常'
      } else if (val > 50) {
        return '注意'
      } else if (val > 25) {
        return '警告'
      } else if (val > 0) {
        return '危险'
      } else {
        return '无状态'
      }
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
