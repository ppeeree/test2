<template>
  <div class="fault-chart" ref="faultChart"></div>
</template>
<script>
import * as echarts from 'echarts'
export default {
  name: 'faultChart',
  props: {
    chartData: {
      type: Object,
      default: () => {}
    }
  },
  data() {
    return {
      selectedItem: {},
      chart: null,
      option: {
        backgroundColor: '#fff',
        // 1. 取消鼠标划上的阴影效果
        tooltip: {
          trigger: 'axis',
          axisPointer: {
            type: 'none' // 设置为 'none' 即可取消默认的阴影高亮
          }
        },
        legend: {
          data: ['注意', '警告', '危险'],
          top: '0',
          right: '0',
          itemWidth: 12,
          itemHeight: 12,
          textStyle: {
            color: '#666',
            fontSize: 12
          }
        },
        grid: {
          left: '3%',
          right: '4%',
          bottom: '6%',
          top: '15%',
          containLabel: true
        },
        xAxis: {
          type: 'category',
          axisTick: { show: false },
          axisLine: { show: true, lineStyle: { color: '#E0E0E0' } },
          data: ['发电机驱动端轴承故障', '发电机非驱动端轴承故障', '齿轮箱高速轴齿轮故障'],
          axisLabel: {
            color: '#333',
            fontSize: 12,
            interval: 0
          }
        },

        yAxis: {
          type: 'value',
          splitLine: {
            lineStyle: {
              type: 'dashed',
              color: '#E0E0E0'
            }
          },
          axisLabel: {
            color: '#666'
          }
        },
        series: [
          {
            name: '注意',
            type: 'bar',
            coordinateSystem: 'cartesian2d',
            barWidth: '10%',
            barGap: '50%',
            itemStyle: {
              color: '#FFD700', // 黄色
              borderRadius: [10, 10, 10, 10],
              // 2. 增加点击效果配置
              // 定义选中状态下的样式
              borderColor: '#333', // 选中时的边框颜色
              borderWidth: 0 // 默认无边框
            },
            // 开启选中模式
            selectedMode: 'single', // 'single'为单选，'multiple'为多选
            select: {
              itemStyle: {
                borderWidth: 2, // 选中时显示边框
                borderColor: '#999' // 选中时边框颜色
                // 也可以在这里改变颜色，例如 color: '#B8860B' 让颜色变深
              }
            },
            label: {
              show: true,
              position: 'top',
              color: '#2B2B2B'
            },
            data: []
          },
          {
            name: '警告',
            type: 'bar',
            coordinateSystem: 'cartesian2d',
            barWidth: '10%',
            barGap: '50%',
            itemStyle: {
              color: '#FF8C00', // 橙色
              borderRadius: [10, 10, 10, 10],
              borderColor: '#333',
              borderWidth: 0
            },
            selectedMode: 'single',
            select: {
              itemStyle: {
                borderWidth: 2,
                borderColor: '#999'
              }
            },
            label: {
              show: true,
              position: 'top',
              color: '#2B2B2B'
            },
            data: []
          },
          {
            name: '危险',
            type: 'bar',
            coordinateSystem: 'cartesian2d',
            barWidth: '10%',
            barGap: '50%',
            itemStyle: {
              color: '#FF4500', // 红色
              borderRadius: [10, 10, 10, 10],
              borderColor: '#333',
              borderWidth: 0
            },
            selectedMode: 'single',
            select: {
              itemStyle: {
                borderWidth: 2,
                borderColor: '#999'
              }
            },
            label: {
              show: true,
              position: 'top',
              color: '#2B2B2B'
            },
            data: []
          }
        ]
      }
    }
  },
  watch: {
    chartData: {
      handler(val) {
        this.option.series[0].data = val[0]
        this.option.series[1].data = val[1]
        this.option.series[2].data = val[2]
        if (this.chart) {
          this.chart.setOption(this.option)
        } else {
          this.initChart()
        }
      }
    }
  },
  mounted() {
    // this.initChart()
    this.resizeHandler = new ResizeObserver(() => {
      this.chart && this.chart.resize()
    })
    this.resizeHandler.observe(this.$refs.faultChart)
  },
  beforeUnmount() {
    if (this.resizeHandler) {
      this.resizeHandler.disconnect()
      this.resizeHandler = null
    }
    if (!this.chart) {
      return
    }
    this.chart.dispose()
    this.chart = null
  },
  methods: {
    initChart() {
      this.chart = echarts.init(this.$refs.faultChart)
      this.chart.setOption(this.option)
      this.chart.on('click', params => {
        if (params.componentType === 'series') {
          if (this.selectedItem?.name) {
            this.chart.dispatchAction({
              type: 'unselect',
              seriesIndex: this.selectedItem.seriesIndex,
              dataIndex: this.selectedItem.dataIndex
            })
          }
          if (
            this.selectedItem.name !== params.name ||
            this.selectedItem.seriesName !== params.seriesName
          ) {
            this.selectedItem = params
            this.$emit('selectedItem', params.name + '_' + params.seriesName)
          } else {
            this.selectedItem = {}
            this.$emit('selectedItem', '')
          }
        }
      })
    },
    clearSelected() {
      if (this.selectedItem?.name) {
        this.chart.dispatchAction({
          type: 'unselect',
          seriesIndex: this.selectedItem.seriesIndex,
          dataIndex: this.selectedItem.dataIndex
        })
        this.selectedItem = {}
      }
    }
  }
}
</script>
<style lang="scss" scoped>
.fault-chart {
  width: 100%;
  height: 100%;
}
</style>
