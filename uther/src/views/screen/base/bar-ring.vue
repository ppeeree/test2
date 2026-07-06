<template>
  <div class="bar">
    <div :ref="chartId" style="width: 130px; height: 130px"></div>
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
    },
    numSqrObj: {
      type: Object,
      default: () => {
        return {}
      },
      required: true
    }
  },
  data () {
    return {
      chartId: `pie-${this.unique}`,
      chart: null,
      option: {
        title: [
          {
            text: `${this.title}正常运行数：${this.isFly ? this.numSqrObj.droneNormal : this.numSqrObj.nestNormal}\n\n较昨日上升${this.numSqrObj.droneRisep}`,
            x: 'center',
            top: '71%',
            textStyle: {
              color: '#FFFFFF',
              fontSize: 10,
              fontWeight: '100'
            }
          },
          {
            text: `${this.isFly ? this.numSqrObj.droneRise : this.numSqrObj.nestRise}%`,
            x: 'center',
            y: '34%',
            textStyle: {
              fontSize: '14',
              color: '#FFFFFF',
              fontFamily: 'DINAlternate-Bold, DINAlternate',
              foontWeight: '600'
            }
          }
        ],
        polar: {
          radius: ['40%', '55%'],
          center: ['50%', '40%']
        },
        angleAxis: {
          max: 100,
          show: false
        },
        radiusAxis: {
          type: 'category',
          data: [this.title || 'value'],
          show: true,
          axisLabel: {
            show: false
          },
          axisLine: {
            show: false
          },
          axisTick: {
            show: false
          }
        },
        series: [
          {
            name: '',
            type: 'bar',
            polarIndex: 0,
            roundCap: true,
            barWidth: 40,
            showBackground: true,
            backgroundStyle: {
              color: 'rgba(66, 66, 66, .3)'
            },
            data: [60],
            coordinateSystem: 'polar',
            itemStyle: {
              normal: {
                color: new echarts.graphic.LinearGradient(0, 1, 0, 0, [
                  {
                    offset: 0,
                    color: '#16CEB9'
                  },
                  {
                    offset: 1,
                    color: '#6648FF'
                  }
                ])
              }
            }
          }
        ]
      }
    }
  },
  computed: {
    // nodata() {
    //   return this.seriesData.length <= 0;
    // }
    isFly () {
      return this.unique === 'first'
    }
  },
  mounted: function () {
    this.chart = echarts.init(this.$refs[this.chartId])
    this.chart.setOption(this.option)
    window.addEventListener('resize', () => {
      this.chart.resize()
    })
  },
  watch: {
    numSqrObj: {
      handler:function (val) {
        if (JSON.stringify(val) === '{}') {
          return
        }
        if (!this.chart) {
          return
        }
        this.option.title[0].text = `${this.title}正常运行数：${this.isFly ? this.numSqrObj.droneNormal : this.numSqrObj.nestNormal}\n\n较昨日上升${this.numSqrObj.droneRisep}`
        this.option.title[1].text = `${parseInt((this.isFly ? this.numSqrObj.droneRise : this.numSqrObj.nestRise) * 100)}%`
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
