<template>
  <div class="index_main_card">
    <div id="numTrend" ref="numTrend" :style="{ width: '100%', height: '240px' }"></div>
  </div>
</template>

<script>
let echarts = require('echarts')
// import { numTrendApi } from '@/api/WindTurbine/LeftPartAPI.js'
export default {
  props: {
    typeTrendShow: {
      type: String,
      default: '',
      require: true
    },
    timeData: {
      type: Array,
      default: () => {},
      require: true
    },
    numData: {
      type: Array,
      default: () => {},
      require: true
    }
  },
  data() {
    return {
      numTrend: null
    }
  },
  watch: {
    numData: {
      handler() {
        this.getNumTrend()
      }
    }
  },
  mounted() {
    this.numTrend = echarts.init(document.getElementById('numTrend'))
    this.getNumTrend()
    let dom = this.$refs['numTrend']
    let ro = new ResizeObserver(() => {
      this.numTrend.resize()
    })
    ro.observe(dom)
  },
  methods: {
    //初始化方法
    getNumTrend() {
      this.numTrend.setOption({
        tooltip: {
          show: true,
          trigger: 'axis',
          label: {
            show: true,
            trigger: 'axis',
            formatter: function (params) {
              return params.name + '<br/>健康指数:' + params.data
            }
          }
        },
        grid: {
          top: '5%',
          left: '10%'
          // bottom:'5%'
        },
        xAxis: {
          type: 'category',
          data: this.timeData
        },
        yAxis: {
          type: 'value',
          splitLine: {
            lineStyle: {
              type: 'dashed'
            },
            show: true
          }
        },
        series: [
          {
            name: '健康指数',
            type: 'line',
            smooth: true,
            lineStyle: {
              normal: {
                color: '#00FF00'
              }
            },
            showSymbol: false,
            // 设置线上节点的颜色
            itemStyle: {
              normal: {
                color: '#00FF00'
              }
            },
            areaStyle: {
              color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [
                {
                  offset: 0,
                  color: 'rgba(57,123,106,0.8)'
                },
                {
                  offset: 1,
                  color: 'rgba(57,123,106,0.3)'
                }
              ])
            },
            data: this.numData
          }
        ]
      })
    },
    //关闭
    noneTypeTrend() {
      this.$emit('noneTypeTrend')
    }
  }
}
</script>

<style lang="less" scoped>
// .index_main_card {
//   background: rgba(41, 64, 88, 1);
//   color: white;
//   z-index: 999;
// }
</style>
