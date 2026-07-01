<template>
  <div class="bar">
    <div :ref="chartId" :style="{ width, height }"></div>
  </div>
</template>
<script>
import * as echarts from 'echarts'
export default {
  inject: ['pieDoughnutRe'],
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
    optDataArr: {
      type: Array,
      default: () => [],
      require: true
    },
    width: {
      type: String,
      default: '177px'
    },
    height: {
      type: String,
      default: '130px'
    },
    color: {
      type: Array,
      default: () => []
    },
    seriesName: {
      type: String,
      default: ''
    },
    centerLeft: {
      type: String,
      default: '46%'
    },
    centerUp: {
      type: String,
      default: '60%'
    }
  },
  data() {
    return {
      chartId: `pie-${this.unique}`,
      chart: null,
      option: {
        title: {
          text: this.title,
          left: 'left',
          textStyle: {
            fontSize: 15,
            color: '#fff'
          }
        },
        // color: this.color,
        tooltip: {
          trigger: 'item',
          position: 'right',
          //appendToBody: true,
          formatter: '{a} <br/>{b}: {c} ({d}%)'
        },
        series: [
          {
            name: this.seriesName,
            type: 'pie',
            radius: '80%',
            center: [this.centerLeft, this.centerUp],
            data: this.optDataArr
            /* labelLine: {
              normal: {
                length: -5,
                length2: -30
              }
            }, */
            /*   labelLine: {
              length: 5,
              length2: 30,
              maxSurfaceAngle: 20
            }, */
            /* label: {
              position: 'outer',
              alignTo: 'labelLine',
              bleedMargin: 5,
              formatter: '{b|{b}}\n{b|{d}%}',
              rich: {
                b: {
                  color: '#fff',
                  fontSize: 12,
                  lineHeight: 16
                }
              }
            } */
            /* label: {
              formatter: '{b|{b}}\n{b|{d}%}',
              borderRadius: 4,
              padding: [20, -30],
              offset: [0, 0],
              rich: {
                b: {
                  color: '#fff',
                  fontSize: 12,
                  lineHeight: 16
                }
              }
            } */
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
    this.$bus.$on('pieDoughnut', params => {
      this.disClearPatch(params.id, params.all)
      this.setSessionItem('icoName', '')
      this.params && this.$emit('clickPartBus', this.params)
    })
    if (this.chartId === 'pie-first') {
      this.$bus.$on('duration', params => {
        this.disClearPatch(params.id, params.all)
      })
    }
    if (this.chartId === 'pie-second') {
      this.$bus.$on('factory', params => {
        this.disClearPatch(params.id, params.all)
      })
    }
    this.chart = echarts.init(this.$refs[this.chartId])
    this.chart.setOption(this.option)
    window.addEventListener('resize', () => {
      this.chart.resize()
    })
    //  this.chartEvent()
  },
  watch: {
    optDataArr: {
      handler: function (val) {
        if (JSON.stringify(val) === '[]') {
          return
        }
        this.option.series[0].data = val
        this.chart.setOption(this.option, true)
      },
      deep: true
    },
    seriesName: {
      handler: function (val) {
        this.option.series[0].name = val
      }
    }
  },
  methods: {
    chartEvent() {
      this.chart.on('click', this.handleClickEvent)
    },
    handleClickEvent(param) {
      // 默认高亮展示第一条数据
      this.chart.dispatchAction({
        type: 'highlight',
        seriesIndex: 0,
        dataIndex: param.dataIndexs
      })
      this.$emit('clickPart', param)
      this.disClearPatch(param.dataIndex)
      // param: 点击对象的所有相关参数
      // doSomethings
    },

    disClearPatch(id, all = false) {
      for (let index = 0; index < 4; index++) {
        if (all) {
          this.chart.dispatchAction({
            type: 'downplay',
            seriesIndex: 0,
            dataIndex: index
          })
        } else {
          this.$bus.$emit('leftTalbe')
          this.$bus.$emit('stateTypeClick')
          this.$bus.$emit('progressClick')
          this.$bus.$emit('barStacked')
          this.$bus.$emit('levelEvent', {
            all: true,
            id: null
          })
          this.$bus.$emit('pieDoughnutRe', {
            all: true,
            id: null
          })
          if (this.chartId === 'pie-first') {
            this.$bus.$emit('factory', {
              all: true,
              id: null
            })
          }
          if (this.chartId === 'pie-second') {
            this.$bus.$emit('duration', {
              all: true,
              id: null
            })
          }
          if (id !== index) {
            this.chart.dispatchAction({
              type: 'downplay',
              seriesIndex: 0,
              dataIndex: index
            })
          }
        }
      }
    },
    clearParams() {
      this.params = null
    }
  }
}
</script>
<style lang="less" scoped>
.bar {
  position: relative;
  width: 200px;
  right: -10px;
  top: 0px;
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
