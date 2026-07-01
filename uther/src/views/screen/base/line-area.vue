<template>
  <div :ref="chartId" :style="{ width, height }"></div>
</template>
<script>
import * as echarts from 'echarts'

export default {
  props: {
    unique: {
      //当要在同一个页面，显示多个图表
      type: [String, Number],
      default: 'index'
    },
    title: {
      type: String,
      default: '事件等级发展趋势'
    },
    width: {
      type: String,
      default: '100%'
    },
    height: {
      type: String,
      default: '100%'
    },
    areaData: {
      type: Object,
      default: () => {
        return {
          seriesData: [
            {
              name: '一级',
              value: [
                '0.240',
                '0.100',
                '0.000',
                '0.000',
                '0.500',
                '0.250',
                '0.333',
                '0.400',
                '0.750',
                '0.500',
                '0.167',
                '0.000',
                '0.200',
                '0.250',
                '0.000'
              ]
            },
            {
              name: '二级',
              value: [
                '0.160',
                '0.250',
                '1.000',
                '1.000',
                '0.500',
                '0.250',
                '0.667',
                '0.533',
                '0.250',
                '0.500',
                '0.333',
                '0.500',
                '0.600',
                '0.500',
                '1.000'
              ]
            },
            {
              name: '三级',
              value: [
                '0.240',
                '0.300',
                '0.000',
                '0.000',
                '0.000',
                '0.500',
                '0.000',
                '0.067',
                '0.000',
                '0.000',
                '0.500',
                '0.500',
                '0.000',
                '0.000',
                '0.000'
              ]
            },
            {
              name: '四级',
              value: [
                '0.360',
                '0.350',
                '0.000',
                '0.000',
                '0.000',
                '0.000',
                '0.000',
                '0.000',
                '0.000',
                '0.000',
                '0.000',
                '0.000',
                '0.200',
                '0.250',
                '0.000'
              ]
            }
          ],
          timeList: [
            '10-17',
            '10-18',
            '10-20',
            '10-21',
            '10-25',
            '10-26',
            '10-27',
            '10-28',
            '10-29',
            '10-30',
            '10-31',
            '11-01',
            '12-07',
            '12-08',
            '12-09'
          ]
        }
      }
    }
  },
  data() {
    return {
      chartId: `line-${this.unique}`,
      chart: null,
      option: {
        color: ['rgb(229,184,26)', 'rgb(255,132,0)', 'rgb(255,48,0)', 'rgb(160,0,0)'],
        tooltip: {
          trigger: 'axis',
          formatter(params) {
            let str = `${params[0].name}<br />`
            params.forEach(item => {
              str += `${item.marker} ${item.seriesName}: ${(item.value * 100).toFixed(1)}% <br/>`
            })
            return str
          }
        },
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
          top: '17%',
          containLabel: true
        },
        legend: {
          data: ['一级', '二级', '三级', '四级'],
          orient: 'horizontal',
          icon: 'circle',
          itemWidth: 10,
          itemHeight: 10,
          textStyle: {
            fontSize: 12, //字体大小
            color: '#fff'
          },
          top: '5%',
          left: 'center'
        },
        xAxis: [
          {
            type: 'category',
            boundaryGap: false,
            data: this.areaData.timeList,
            axisLabel: {
              color: '#FFFFFF'
            }
          }
        ],
        yAxis: [
          {
            axisLabel: {
              color: '#FFFFFF', // x轴字体颜色
              formatter(val) {
                return `${val * 100}%`
              }
            },
            /*  max: 1, */
            type: 'value',
            splitNumber: 5,
            splitLine: {
              show: true,
              lineStyle: {
                type: 'dashed'
              }
            },
            show: true
          }
        ],
        series: [
          {
            name: '一级',
            type: 'line',
            stack: 'Total',
            smooth: true,
            lineStyle: {
              width: 0
            },
            showSymbol: false,
            areaStyle: {
              opacity: 5,
              color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [
                {
                  offset: 0,
                  color: 'rgb(229,184,26)'
                },
                {
                  offset: 1,
                  color: 'rgb(229,184,26)'
                }
              ])
            },
            emphasis: {
              focus: 'series'
            },
            data: this.areaData.seriesData.find(i => i.name === '一级').value
          },
          {
            name: '二级',
            type: 'line',
            stack: 'Total',
            smooth: true,
            lineStyle: {
              width: 0
            },
            showSymbol: false,
            areaStyle: {
              opacity: 5,
              color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [
                {
                  offset: 0,
                  color: 'rgb(255,132,0)'
                },
                {
                  offset: 1,
                  color: 'rgb(255,132,0)'
                }
              ])
            },
            emphasis: {
              focus: 'series'
            },
            data: this.areaData.seriesData.find(i => i.name === '二级').value
          },
          {
            name: '三级',
            type: 'line',
            stack: 'Total',
            smooth: true,
            lineStyle: {
              width: 0
            },
            showSymbol: false,
            areaStyle: {
              opacity: 5,
              color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [
                {
                  offset: 0,
                  color: 'rgb(255,48,0)'
                },
                {
                  offset: 1,
                  color: 'rgb(255,48,0)'
                }
              ])
            },
            emphasis: {
              focus: 'series'
            },
            data: this.areaData.seriesData.find(i => i.name === '三级').value
          },
          {
            name: '四级',
            type: 'line',
            stack: 'Total',
            smooth: true,
            lineStyle: {
              width: 0
            },
            showSymbol: false,
            areaStyle: {
              opacity: 5,
              color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [
                {
                  offset: 0,
                  color: 'rgb(160,0,0)'
                },
                {
                  offset: 1,
                  color: 'rgb(160,0,0)'
                }
              ])
            },
            emphasis: {
              focus: 'series'
            },
            data: this.areaData.seriesData.find(i => i.name === '四级').value
          }
        ]
      }
    }
  },
  mounted: function () {
    this.chart = echarts.init(this.$refs[this.chartId])
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
    areaData: {
      handler: function (val) {
        this.handleSeriesData(val)
      },
      deep: true
      // immediate: true
    }
  },
  methods: {
    handleSeriesData(val) {
      const opt = ['一级', '二级', '三级', '四级']
      opt.forEach((item, index) => {
        this.option.series[index].data = val.seriesData.find(i => i.name === item).value
      })
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
