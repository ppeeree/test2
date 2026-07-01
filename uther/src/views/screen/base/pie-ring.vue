<template>
  <div :ref="chartId" :style="{ width, height }"></div>
</template>
<script>
import * as echarts from 'echarts'
import difference from 'lodash/difference'
import cloneDeep from 'lodash/cloneDeep'
import isEmpty from 'lodash/isEmpty'
import isNull from 'lodash/isNull'
import round from 'lodash/round'
let peiName = { 'pie-first': 'levelPie', 'pie-second': 'eventTypePie', 'pie-third': 'statusPie' }

export default {
  props: {
    unique: {
      //当要在同一个页面，显示多个图表
      type: [String, Number],
      default: 'index'
    },
    peiData: {
      type: Array,
      default: () => []
    },
    width: {
      type: String,
      default: '150px'
    },
    height: {
      type: String,
      default: '100px'
    },
    color: {
      type: Array,
      default: () => ['#00C3FF', '#563AD2', '#DE8536', '#ED5450']
    },
    legendGrid: {
      type: Object,
      default: () => {}
    },
    radiusGrid: {
      type: Object,
      default: () => {}
    },
    seriesName: {
      type: String,
      default: '事件等级'
    }
  },
  data() {
    return {
      chartId: `pie-${this.unique}`,
      chart: null,
      data_name: [],
      totalNum: 0,
      titleLabel: {
        value: null,
        unit: '%'
      },
      option: {
        color: this.color,
        tooltip: {
          trigger: 'item',
          formatter: params => {
            const { seriesName, value, percent } = params
            const dat = params.name.split('  ')
            let str = dat[0]
            return `${seriesName} <br/>${str}: ${value} (${percent}%)`
          },
          confine: true
        },
        legend: {
          orient: 'vertical',
          selectedMode: true,
          itemHeight: '12',
          align: 'left',
          left: 'right',
          top: 'center',
          textStyle: {
            color: '#fff',
            fontWeight: 'normal',
            fontSize: '14',
            rich: {
              a: {
                padding: [0, -7],
                verticalAlign: 'sub',
                color: '#fff',
                fontSize: '14'
              },
              b: {
                padding: [0, 14],
                verticalAlign: 'sub',
                color: '#fff',
                fontSize: '14'
              }
            }
          },
          formatter: name => {
            const dat = name.split('  ')
            let num = dat[0],
              str = dat[1]
            return ['{a|' + `${num}` + '}{b|' + `${str}` + '}']
          },
          tooltip: {
            show: true,
            trigger: 'item'
          },
          ...this.legendGrid
        },
        series: [
          {
            name: this.seriesName,
            type: 'pie',
            radius: ['44%', '75%'],
            center: ['27%', 'center'],
            legendHoverLink: false,
            data: [],
            labelLine: {
              normal: {
                show: false
              }
            },
            label: {
              show: true,
              position: 'center',
              formatter: () =>
                isNull(this.titleLabel.value)
                  ? ''
                  : `{titleValue|${round(
                      (this.handleLabel.value / this.totalNum) * 100
                    )}}{titleUnit|${this.handleLabel.unit}}`,
              rich: {
                titleUnit: {
                  color: '#fff',
                  fontSize: 12,
                  padding: [13, 0, 0, 0]
                },
                titleValue: {
                  color: '#fff',
                  fontSize: 20,
                  fontWeight: 600,
                  padding: [13, 0, 5, 0]
                }
              }
            },
            ...this.radiusGrid
          }
        ]
      }
    }
  },
  mounted: function () {
    this.$bus.$on('levelEvent', params => {
      this.$emit('clickPartEvent')
      this.disClearPatch(params.id, params.all)
    })
    this.init()
    window.addEventListener('resize', () => {
      this.chart.resize()
    })
    difference(Object.keys(peiName), [this.chartId]).forEach(item => {
      this.$bus.$on(peiName[item], params => {
        this.disClearPatch(params.id, params.all)
      })
    })
    this.chartEvent()
  },
  computed: {
    handleLabel() {
      return this.titleLabel
    }
  },
  watch: {
    peiData: {
      handler: function () {
        // 机组切换，从有数据切换到无数据，环形图不更新
        /*    if (val.length === 0) {
          return
        } */
        this.getOption()
        this.chart.setOption(this.option)
        // this.init()
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
    init() {
      this.getOption()
      this.chart = echarts.init(this.$refs[this.chartId])
      this.chart.setOption(this.option)
    },
    getOption() {
      const data = cloneDeep(this.peiData)
      this.option.legend.data = data
      this.option.series[0].data = data
      this.summation(data)
      this.option.legend.right = this.width === '180px' ? 0 : 30
      for (let n in data) {
        data[n]['name'] = data[n]['name'] + '  ' + data[n]['value']
        this.option.legend.data.push(data[n]['name'])
      }
    },
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

      this.$emit('clickPartEvent', param)
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
          this.$bus.$emit('stateTypeClick')
          this.$bus.$emit('progressClick')
          this.$bus.$emit('leftTalbe')
          this.$bus.$emit('barStacked')
          this.$bus.$emit('pieDoughnutRe', {
            all: true,
            id: null
          })
          this.$bus.$emit(peiName[this.chartId], {
            all: true,
            id: null
          })
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
    summation(data) {
      this.totalNum = 0
      data.forEach(item => {
        if (!isEmpty(item)) {
          this.totalNum += item.value
        }
      })
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
