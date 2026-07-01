<template>
  <div :ref="chartId" :style="{ width, height }"></div>
</template>
<script>
import * as echarts from 'echarts'
import cloneDeep from 'lodash/cloneDeep'
import difference from 'lodash/difference'
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
    }
  },
  data() {
    return {
      chartId: `pie-${this.unique}`,
      chart: null,
      data_name: [],
      legend: [],
      option: {
        color: this.color,
        tooltip: {
          trigger: 'item',
          formatter: '{b}',
          confine: true
        },
        legend: [
          {
            selectedMode: true,
            orient: 'vertical',
            icon: 'circle',
            align: 'left',
            x: 'right',
            top: 'bottom',
            itemWidth: 10,
            itemHeight: 10,
            textStyle: {
              color: '#fff',
              fontWeight: 'normal',
              fontSize: '12',
              rich: {
                a: {
                  padding: [0, -8, 0, -1],
                  verticalAlign: 'sub',
                  color: '#fff',
                  fontSize: '12'
                },
                b: {
                  padding: [0, 0, 0, 10],
                  verticalAlign: 'sub',
                  color: '#fff',
                  fontSize: '12'
                }
              }
            },
            formatter: name => {
              const dat = name.split('  ')
              let num = dat[0],
                str = dat[1],
                reg = /事件$/g
              num = num.replace(reg, '')
              return ['{a|' + `${num}` + '}{b|' + `${str}` + '}']
            },
            tooltip: {
              show: true,
              trigger: 'item'
            },
            data: [],
            ...this.legendGrid
          },
          {
            selectedMode: true,
            orient: 'vertical',
            icon: 'circle',
            top: 'bottom',
            itemWidth: 10,
            itemHeight: 10,
            x: 'left',
            align: 'left',
            textStyle: {
              color: '#fff',
              fontWeight: 'normal',
              fontSize: '12',
              rich: {
                a: {
                  padding: [0, -8, 0, -1],
                  verticalAlign: 'sub',
                  color: '#fff',
                  fontSize: '12'
                },
                b: {
                  padding: [0, 0, 0, 10],
                  verticalAlign: 'sub',
                  color: '#fff',
                  fontSize: '12'
                }
              }
            },
            formatter: name => {
              const dat = name.split('  ')
              let num = dat[0],
                str = dat[1],
                reg = /事件$/g
              num = num.replace(reg, '')
              return ['{a|' + `${num}` + '}{b|' + `${str}` + '}']
            },
            tooltip: {
              show: true,
              trigger: 'item'
            },
            data: [],
            ...this.legendGrid
          }
        ],
        series: [
          {
            name: '',
            type: 'pie',
            radius: ['35%', '60%'],
            center: ['50%', '35%'],
            legendHoverLink: false,
            data: [],
            labelLine: {
              normal: {
                show: false
              }
            },
            label: {
              normal: {
                // position: 'inner',
                // formatter: '{d}%',
                show: false,
                textStyle: {
                  color: '#fff',
                  //   fontWeight: 'bold',
                  fontSize: 14
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
  watch: {
    peiData: {
      handler: function () {
        // 机组切换，从有数据切换到无数据，环形图不更新
        /*    if (val.length === 0) {
          return
        } */
        this.init()
      },
      deep: true
    }
  },
  methods: {
    init() {
      this.legend = []
      const data = cloneDeep(this.peiData)
      // this.option.legend.data = data
      this.option.series[0].data = data
      for (let n in data) {
        data[n]['name'] = data[n]['name'] + '  ' + data[n]['value']
        this.legend.push(data[n]['name'])
      }
      this.option.legend[0].data = this.legend.slice(
        Math.floor(this.legend.length / 2),
        this.legend.length
      )
      this.option.legend[1].data = this.legend.slice(0, Math.ceil((this.legend.length - 1) / 2))
      this.chart = echarts.init(this.$refs[this.chartId])
      this.chart.setOption(this.option)
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
