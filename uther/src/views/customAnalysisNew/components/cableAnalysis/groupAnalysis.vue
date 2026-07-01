<template>
  <div class="group-analysis" style="width: 100%; height: 100%; overflow: hidden">
    <h3>
      <span class="group-analysis__title">{{ titleText }}</span>
      <chart-tools
        :selectedChart="selectedChart"
        ref="chartTools"
        :key="titleText"
        @operationChange="operationChange"
      />
    </h3>
    <div class="group-analysis__left" style="width: 100%; height: 100%; padding-top: 30px">
      <div class="analysis_content_item">
        <div
          @click="selectedChart = 'distribution'"
          :style="{
            width: '400px',
            height: '100%',
            position: 'relative',
            border: '1px solid #ccc',
            margin: '1px 0',
            border: selectedChart == 'distribution' ? '1px solid #409eff' : '1px solid #eee'
          }"
        >
          <distribution-chart
            ref="distribution"
            :style="{
              width: '100%',
              height: 'calc(100% - 2px)'
            }"
            titleText="拉索力分布图"
            :filterParam="filterParam"
            @clickEvent="getWavePointer"
            @getImgData="getImgData"
            :dataSource="trackLineData"
            :loading="loading"
          ></distribution-chart>
        </div>
        <div
          @click="selectedChart = 'chart2'"
          :style="{
            width: 'calc(100% - 405px)',
            height: '100%',
            position: 'relative',
            border: '1px solid #ccc',
            margin: '1px 0',
            border: selectedChart == 'chart2' ? '1px solid #409eff' : '1px solid #eee'
          }"
        >
          <!-- :filterParam="filterParam" -->
          <!--  :dataSource="chartData" -->
          <line-chart
            ref="chart2"
            keyId="chart2"
            :style="{
              width: '100%',
              height: '100%'
            }"
            :titleText="titleText"
            :filterParam="filterParam"
            @clickEvent="getWavePointer"
            @getImgData="getImgData"
            :loading="loading"
            :warningLine="false"
          ></line-chart>
        </div>
      </div>
    </div>
  </div>
</template>
<script>
import groupBy from 'lodash/groupBy'
import { getEvAnalyzerDataApi } from '@/api/analysis/index.js'
import chartTools from '../chartComponent/chartTool.vue'
import lineChart from '../chartComponent/lineChart.vue' //'../chartComponent/lineChart.vue'
import Splitter from '../splitter'
import dayjs from 'dayjs'
import DistributionChart from './distributionChart.vue'

export default {
  components: {
    chartTools,
    Splitter,
    lineChart,
    DistributionChart
  },
  props: {
    filterParam: {
      type: Object,
      default: () => {
        return {}
      }
    },
    keyid: {
      type: String,
      default: ''
    },
    titleText: {
      type: String,
      default: ''
    },
    evIdList: {
      type: Array,
      default: () => {
        return []
      }
    },
    setSteelNumber: {
      type: Object,
      default: () => {
        return {}
      }
    }
  },
  data() {
    return {
      waveInfoDataListObj: {},
      selectedChart: '',
      value: 'value1',
      dialogVisible: false,
      trackLineData: [],
      chartData: {
        data: [],
        titleText: '拉索力趋势图'
      },
      // resizeOb: null,
      loading: false
    }
  },
  created() {
    // this.getChartData()
  },
  watch: {
    evIdList: {
      handler(val) {
        this.waveInfoDataListObj = {}
        this.getEvAnalyzerDataApiFunc()
      },
      immediate: true,
      deep: true
    },
    filterParam: {
      handler(val) {
        this.waveInfoDataListObj = {}
        this.getEvAnalyzerDataApiFunc()
      },
      deep: true
    },
    waveInfoDataListObj: {
      handler(val) {
        const flatArray = Object.values(val).flatMap(val => val)
        if (flatArray.length) {
          let wavepointers = []
          let waveInfo = []
          flatArray.forEach(item => {
            let { coord, other } = item
            waveInfo.push(other)
            const { windturbineId, measlocId, sampleRate } = other
            // 波形ID：机组ID+测量ID+采集时间
            let unit = windturbineId + '&&' + measlocId + '&&' + coord[0] + '&&' + sampleRate
            wavepointers.push(unit)
          })
          this.$emit('getWavePointer', this.keyid, [...new Set(wavepointers)])
        } else {
          this.$emit('getWavePointer', this.keyid, [])
        }
      },
      deep: true
    },
    selectedChart: {
      handler(val) {
        /*   if (Array.isArray(this.$refs[this.selectedChart])) {
          this.$emit('getSelectValue', this.$refs[this.selectedChart][0].selectValue)
        } */
      }
    }
  },
  mounted() {
    this.$nextTick(() => {
      this.selectedChart = 'distribution'
    })
  },
  /*   beforeUnmount() {
    if (this.resizeOb) {
      this.resizeOb.disconnect()
      this.resizeOb = null
    }
  }, */
  methods: {
    getImgData(data) {
      this.$emit('getImgData', data)
    },
    // 点击趋势上的波形点信息方法
    getWavePointer(key, data, ctrlKey) {
      this.waveInfoDataListObj[key] = data
    },
    getSelectValue(data) {
      this.$emit('getSelectValue', data)
    },
    /*   creatResizeOb() {
      if (this.resizeOb) {
        this.resizeOb.disconnect()
        this.resizeOb = null
      }
      this.resizeOb = new ResizeObserver(entries => {
        entries.forEach(entry => {
          if (entry.target == this.$refs.groupContent) {
            let newWidth = entry.contentRect.width
            let newHeight = entry.contentRect.height
            let rightDom = entry.target.querySelector('.group-analysis__right')
            if (rightDom) {
              let rightWidth = rightDom.clientWidth
              let chartWidth = newWidth - rightWidth - 4
              this.$refs.trend_left.style.width = chartWidth + 'px'
              let splitDom = document.getElementById('groupSplit' + this.keyid)
              if (splitDom) {
                splitDom.style.left = chartWidth + 'px'
              }
              rightDom.style.left = newWidth - rightWidth + 'px'
              rightDom.style.height = newHeight + 'px'
            } else {
              this.$refs.trend_left.style.width = newWidth + 'px'
            }
            this.$refs.trend_left.style.height = newHeight + 'px'
            this.$refs.trend_left.style.left = 0
          }
        })
      })
      // 指定观察dom
      this.resizeOb.observe(this.$refs.trend_left)
      this.resizeOb.observe(this.$refs.groupContent)
    }, */
    operationChange(operation, content) {
      if (this.selectedChart.length) {
        if (Array.isArray(this.$refs[this.selectedChart])) {
          this.$refs[this.selectedChart][0].operation(operation, content)
        } else {
          this.$refs[this.selectedChart].operation(operation, content)
        }
      } else {
        return alert('请选择图表')
      }
    },
    changeSize() {
      /*   const component = this.$refs.groupContent // 获取组件的引用
      const newWindow = window.open('', '_blank') // 打开新窗口
      newWindow.document.write(component.outerHTML) // 将组件的HTML写入新窗口 */
    },
    handleChange() {
      if (!this.selectedChart.length) {
        return alert('请选择图表')
      }
    },
    // 测点下特征值趋势查询接口
    getEvAnalyzerDataApiFunc() {
      this.loading = true
      let { timeValue, workRange } = this.filterParam
      let num = this.evIdList.length
      let unitNum = Math.floor(num / 6)
      let diffNum = (unitNum + 1) * 6 - num
      let paramList = []
      if (num > 6) {
        let startIndex = 0
        for (let i = 0; i < 6; i++) {
          let unitIds = []
          if (i < diffNum) {
            unitIds = this.evIdList.slice(startIndex, startIndex + unitNum)
            startIndex += unitNum
          } else {
            unitIds = this.evIdList.slice(startIndex, startIndex + unitNum + 1)
            startIndex += unitNum + 1
          }
          paramList.push({
            analyzeWay: 'TA',
            endTime: timeValue[1],
            startTime: timeValue[0],
            eigenValueIds: unitIds,
            wkCond: {
              ROTSPEED_MCS: workRange
            }
          })
        }
      } else {
        paramList = this.evIdList.map(item => {
          return {
            analyzeWay: 'TA',
            endTime: dayjs(timeValue[1]).format('YYYY-MM-DD 23:59:59'),
            startTime: dayjs(timeValue[0]).format('YYYY-MM-DD 00:00:00'),
            eigenValueIds: [item],
            wkCond: {
              ROTSPEED_MCS: workRange
            }
          }
        })
      }
      Promise.allSettled(paramList.map(ele => getEvAnalyzerDataApi(ele)))
        .then(res => {
          let success = res.filter(i => i.status === 'fulfilled')
          if (!success.length) {
            this.loading = false
            return
          }
          const middle = success.flatMap(item => (item.value ? item.value.data.data : undefined))
          let lineData = []
          let trackLineData = []
          middle.forEach(item => {
            if (item) {
              const { evdataList } = item
              evdataList.forEach(element => {
                const {
                  windturbineName,
                  windturbineId,
                  windParkName,
                  measlocName,
                  evName,
                  evId,
                  evdata,
                  vdiMin,
                  vdiMax,
                  unitX,
                  unitY,
                  avg,
                  max,
                  min
                } = element
                lineData.push({
                  source: evdata,
                  name: `${evName}_${measlocName}_${windturbineName}_${windParkName}`,
                  VDI: vdiMin === null ? [] : [vdiMin, vdiMax],
                  id: evId,
                  dimensions: [unitX, unitY, '转速', '是否有波形点', ''],
                  other: {
                    ...element,
                    evdata: []
                  }
                  /*  warningLine: {
                    warning: vdiMax,
                    danger: vdiMin
                  } */
                })
                let codelist = measlocName.match(/\d+/g)
                trackLineData.push({
                  id: windturbineId,
                  name: `${windturbineName}_${windParkName}`,
                  steelNumber: this.setSteelNumber[windturbineId],
                  code: Number(codelist[codelist.length - 1]),
                  avg: Number(avg),
                  max: Number(max),
                  min: Number(min)
                })
              })
            }
          })
          // this.chartData.data = lineData
          this.$refs.chart2.initChart({
            data: lineData,
            titleText: '拉索力趋势图'
          })
          this.trackLineData = groupBy(trackLineData, item => item.name)
          this.loading = false
        })
        .catch(error => {
          this.loading = false
          alert(error)
        })
    }
  }
}
</script>
<style lang="scss" scoped>
.group-analysis {
  width: 100%;
  height: 100%;
  background: #fff;
  color: #000;
  position: absolute;
  border-radius: 5px;
  h3 {
    font-size: 14px;
    position: absolute;
    left: 0px;
    top: 0px;
    z-index: 1000;
    width: 100%;
    height: 28px;
    font-weight: normal;
    background: #f8f8f8;
    padding-left: 10px;
    .group-analysis__title {
      float: left;
      line-height: 35px;
      font-size: 14px;
    }
  }
  &__left {
    width: 100%; // calc(100% - 204px);
    height: 100%;
    left: 0;
    position: absolute;
    top: 0px;
    box-sizing: border-box;
    padding: 30px 10px 0 10px;
    display: flex;
    flex-direction: column;
    justify-content: space-around;
    align-items: center;
    overflow-x: hidden;
    overflow-y: auto;
  }
  &__right {
    width: 200px;
    height: 100%;
    right: 0;
    position: absolute;
    top: 0;
    padding: 30px 10px 10px 10px;
    text-align: left;
    line-height: 18px;
    font-size: 14px;
    overflow-y: auto;
    overflow-x: hidden;
    .right_legend {
      width: 100%;
      p {
        width: 100%;
        line-height: 26px;
        span {
          display: inline-block;
          width: 10px;
          height: 10px;
          border-radius: 50%;
          margin-right: 5px;
        }
      }
      .unSelected {
        color: #a59f9f;
        span {
          background: #9c9fa1 !important;
        }
      }
      &__content {
        width: 100%;
        height: calc(100% - 40px);
        position: relative;
        .group-analysis__content__item {
          width: 100%;
          height: 100%;
          position: relative;
          border: 1px solid #ccc;
          margin: 1px 0;
        }
      }
    }
  }

  &__content__item {
    width: 100%;
    position: relative;
    border: 1px solid #ccc;
    margin: 1px 0;
  }
  .analysis_content_item {
    width: 100%;
    height: calc(100% - 2px);
    // margin-bottom: 5px;
    display: flex;
    flex-direction: row;
    justify-content: space-between;
  }
}
</style>
