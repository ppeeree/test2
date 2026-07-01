<template>
  <div class="bar">
    <div :ref="chartId" style="width: 100%; height: 200px"></div>
  </div>
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
      default: '',
      required: true
    }
  },
  data () {
    return {
      chartId: `pie-${this.unique}`,
      chart: null,
      option: {
        tooltip: {
          trigger: 'axis',
          axisPointer: { type: 'shadow' }
        },
        grid: {
          left: '4%',
          right: '4%',
          bottom: '0%',
          containLabel: true
        },
        xAxis: [
          {
            type: 'category',
            axisTick: { show: false },
            axisLabel: {
              textStyle: { fontSize: 14, color: '#4D4D4D' }
            },
            axisLine: {
              lineStyle: { color: '#707070' }
            },
            data: ['低能见度1', '低能见度2', '低能见度3', '低能见度4']
          }
        ],
        yAxis: {
          type: 'value',
          name: '航班数量',
          nameTextStyle: { fontSize: 14, color: '#fff' },
          axisLabel: {
            textStyle: { fontSize: 12, color: '#4D4D4D' }
          },
          nameLocation: 'end',
          axisLine: {
            lineStyle: { color: '#707070' }
          }
        },
        series: [
          {
            name: '直接访问',
            type: 'bar',
            barWidth: '30%',
            data: [
              {
                name: '低能见度1',
                value: '70',
                itemStyle: { color: '#1F78B4' }
              },
              {
                name: '低能见度2',
                value: '50',
                itemStyle: { color: '#A6CEE3' }
              },
              {
                name: '低能见度3',
                value: '30',
                itemStyle: { color: '#B2DF8A' }
              },
              {
                name: '低能见度4',
                value: '25',
                itemStyle: { color: '#33A02C' }
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
  },
  mounted: function () {
    this.chart = echarts.init(this.$refs[this.chartId])
    this.chart.setOption(this.option)
    window.addEventListener('resize', () => {
      this.chart.resize()
    })
  },
  watch: {},
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
