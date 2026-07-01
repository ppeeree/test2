<template>
  <div class="bar" :style="{ opacity: optDataArr.length ? '1' : '0.3', marginLeft: '3px' }">
    <div :ref="chartId" :style="{ width, height }"></div>
  </div>
</template>
<script>
import * as echarts from 'echarts'
import isNull from 'lodash/isNull'

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
    optDataArr: {
      type: Array,
      default: () => [],
      require: true
    },
    width: {
      type: String,
      default: '270px'
    },
    height: {
      type: String,
      default: '220px'
    }
  },
  data() {
    return {
      chartId: `pie-${this.unique}`,
      chart: null,
      titleLabel: {
        value: null,
        unit: '台'
      },
      option: {
        tooltip: {
          trigger: 'item',
          confine: true,
          formatter: '{a} <br/>{b}: {c} ({d}%)'
        },
        color: ['#88C2F1', '#48E5E5', '#3CD495', '#2B8EF3', '#3254DD'],
        series: [
          {
            name: '部件故障',
            type: 'pie',
            radius: ['40%', '67%'],
            center: 'center',
            data: this.optDataArr,
            label: {
              formatter: '{b|{b}}',
              borderRadius: 4,
              padding: [10, -24],
              offset: [7, -7],
              rich: {
                b: {
                  color: '#fff',
                  fontSize: 12
                  // lineHeight: 10
                },
                per: {
                  fontSize: 12,
                  padding: [2, 4],
                  borderRadius: 2
                }
              }
            },
            labelLine: {
              normal: {
                length: 20,
                length2: 15
              }
            }
          },
          {
            name: '部件故障',
            type: 'pie',
            radius: ['40%', '67%'],
            center: 'center',
            data: this.optDataArr,
            label: {
              show: true,
              position: 'center',
              formatter: () =>
                isNull(this.titleLabel.value)
                  ? ''
                  : `{titleValue|${this.handleLabel.value}}{titleUnit|${this.handleLabel.unit}}`,
              rich: {
                titleUnit: {
                  color: '#fff',
                  fontSize: 16,
                  padding: [15, 0, 0, 0]
                },
                titleValue: {
                  color: '#fff',
                  fontSize: 30,
                  fontWeight: 600,
                  padding: [15, 5, 5, 5]
                }
              }
            },
            labelLine: {
              normal: {
                length: 20,
                length2: 15
              }
            }
          }
        ]
      },
      params: null
    }
  },
  computed: {
    handleLabel() {
      return this.titleLabel
    }
  },
  mounted: function () {
    this.$bus.$on('pieDoughnutRe', params => {
      this.disClearPatch(params.id, params.all)
      this.setSessionItem('icoName', '')
      this.params && this.$emit('clickPartBus', this.params)
    })
    this.chart = echarts.init(this.$refs[this.chartId])
    this.chart.setOption(this.option)
    window.addEventListener('resize', () => {
      this.chart.resize()
    })
    this.chartEvent()
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
    'titleLabel.value': {
      handler: function () {
        this.chart.setOption(this.option, true)
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

      this.titleLabel.value = param.data.value

      this.disClearPatch(param.dataIndex)
      this.$emit('clickPartRe', param)
      this.params = param
      // param: 点击对象的所有相关参数
      // doSomethings
    },
    disClearPatch(id, all = false) {
      for (let index = 0; index < 5; index++) {
        if (all) {
          this.chart.dispatchAction({
            type: 'downplay',
            seriesIndex: 0,
            dataIndex: index
          })
          this.titleLabel.value = null
        } else {
          this.$bus.$emit('pieDoughnut', {
            all: true,
            id: null
          })
          this.$bus.$emit('levelEvent', {
            all: true,
            id: null
          })
          this.$bus.$emit('stateTypeClick')
          this.$bus.$emit('progressClick')
          this.$bus.$emit('leftTalbe')
          this.$bus.$emit('barStacked')
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
  right: 27px;
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
