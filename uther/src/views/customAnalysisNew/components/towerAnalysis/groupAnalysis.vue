<template>
  <div class="group-analysis" style="width: 100%; height: 100%">
    <h3>
      <span class="group-analysis__title">{{ titleText }}</span>
      <chart-tools
        :selectedChart="selectedChart"
        ref="chartTools"
        :key="titleText"
        @operationChange="operationChange"
      />
    </h3>
    <div class="group-analysis__left" style="width: 100%; height: 100%; padding-top: 28px">
      <div class="analysis_content_item_top">
        <div
          @click="selectedChart = 'track'"
          :style="{
            width: '380px',
            height: '100%',
            position: 'relative',
            border: '1px solid #ccc',
            margin: '1px 0',
            border: selectedChart == 'track' ? '1px solid #409eff' : '1px solid #eee'
          }"
        >
          <!--  :dataSource="trackLineData" -->
          <track-chart
            ref="track"
            :style="{
              width: '100%',
              height: '100%'
            }"
            :titleText="titleText.includes('塔基') ? '塔基倾斜率轨迹图' : '塔筒偏移量轨迹图'"
            :filterParam="filterParam"
            @clickEvent="getWavePointer"
            @getImgData="getImgData"
            :loading="loading"
          ></track-chart>
        </div>
        <div
          @click="selectedChart = 'towerLine'"
          :style="{
            width: 'calc(100% - 385px)',
            height: '100%',
            position: 'relative',
            border: '1px solid #ccc',
            margin: '1px 0',
            border: selectedChart == 'towerLine' ? '1px solid #409eff' : '1px solid #eee'
          }"
        >
          <line-chart
            ref="towerLine"
            keyId="towerLine"
            :style="{
              width: '100%',
              height: '100%'
            }"
            :titleText="titleText.includes('塔基') ? '塔基倾斜率趋势' : '塔筒沉降量趋势'"
            @clickEvent="getWavePointer"
            @getImgData="getImgData"
            :loading="loading"
          ></line-chart>
        </div>
      </div>
      <div style="width: 100%; height: calc(100% - 235px)">
        <div
          class="group-analysis__content__item"
          v-for="item in chartList"
          :key="item.code"
          @click="selectedChart = item.code"
          :style="{
            height: 100 / chartList.length + '%',
            minHeight: '200px',
            paddingBottom: '1px',
            border: selectedChart == item.code ? '1px solid #409eff' : '1px solid #eee'
          }"
        >
          <unit-chart
            :ref="item.code"
            :keyId="item.code"
            style="width: 100%; height: 100%"
            :titleText="item.name"
            :selectOption="item.children"
            :filterParam="filterParam"
            @clickEvent="getWavePointer"
            @getImgData="getImgData"
            @getSelectValue="getSelectValue"
          ></unit-chart>
        </div>
      </div>
    </div>
  </div>
</template>
<script>
import { getEvAnalyzerDataApi } from '@/api/analysis/index.js'
import chartTools from '../chartComponent/chartTool.vue'
import unitChart from '../chartComponent/unitChart.vue'
import lineChart from '../chartComponent/lineChart.vue'
import Splitter from '../splitter'
import TrackChart from './trackChart.vue'
import dayjs from 'dayjs'

export default {
  components: {
    chartTools,
    unitChart,
    Splitter,
    TrackChart,
    lineChart
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
    chartList: {
      type: Array,
      default: () => {
        return []
      }
    }
  },
  data() {
    return {
      waveInfoDataListObj: {},
      selectedChart: '',
      value: 'value1',
      dialogVisible: false,
      isExpand: false,
      lineData: [],
      trackLineData: [],
      // resizeOb: null,
      loading: false
    }
  },
  computed: {
    chartData() {
      return this.$store.state.analysis.chartData
    }
  },
  created() {
    // this.getChartData()
  },
  watch: {
    chartList: {
      handler(val) {
        this.waveInfoDataListObj = {}
      }
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
            // 需要处理倾斜率一个点对应两个测点位置的情况
            if (measlocId.includes(',')) {
              let measlocIds = measlocId.split(',')
              measlocIds.forEach(i => {
                let unit = windturbineId + '&&' + i + '&&' + coord[0] + '&&' + sampleRate
                wavepointers.push(unit)
              })
            } else {
              // 波形ID：机组ID+测量ID+采集时间
              let unit = windturbineId + '&&' + measlocId + '&&' + coord[0] + '&&' + sampleRate
              wavepointers.push(unit)
            }
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
        if (Array.isArray(this.$refs[this.selectedChart])) {
          this.$emit('getSelectValue', this.$refs[this.selectedChart][0].selectValue)
        }
      }
    }
  },
  mounted() {
    this.$nextTick(() => {
      this.selectedChart = 'towerLine'
    })
  },
  /*  beforeUnmount() {
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
      // 没按ctrl键，清除其他图谱所有选中点
      if (!ctrlKey) {
        let allChartList = [
          {
            code: 'towerLine'
          }
        ].concat(this.chartList)
        allChartList.forEach(item => {
          if (item.code !== key) {
            if (Array.isArray(this.$refs[item.code])) {
              this.$refs[item.code][0].clearWavePointer()
            } else {
              this.$refs[item.code].clearWavePointer()
            }
            this.waveInfoDataListObj[item.code] = []
          }
        })
      }
      this.waveInfoDataListObj[key] = data
    },
    // 初始化使用
    getSelctedChartEigenValue() {
      if (this.selectedChart && this.selectedChart.length && this.$refs[this.selectedChart][0]) {
        return this.$refs[this.selectedChart][0].selectValue
      }
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
      this.isExpand = !this.isExpand
      /*   const component = this.$refs.groupContent // 获取组件的引用
      const newWindow = window.open('', '_blank') // 打开新窗口
      newWindow.document.write(component.outerHTML) // 将组件的HTML写入新窗口 */
    },
    handleChange() {
      if (!this.selectedChart.length) {
        return alert('请选择图表')
      }
    },
    // 获取轨迹图计算特征值数据
    // 测点下特征值趋势查询接口
    getEvAnalyzerDataApiFunc(checkedMeasList) {
      this.loading = true
      let { timeValue, workRange } = this.filterParam
      let windturbineIDs = []
      checkedMeasList.forEach(item => {
        let { measLoctionID, windturbineID } = item
        if (measLoctionID.includes('TOWFDN')) {
          // 勾选了塔基相关测点
          windturbineIDs.push(windturbineID)
        }
      })
      windturbineIDs = [...new Set(windturbineIDs)]
      let num = windturbineIDs.length
      let unitNum = Math.floor(num / 6)
      let diffNum = (unitNum + 1) * 6 - num
      let paramList = []
      if (num > 6) {
        let startIndex = 0
        for (let i = 0; i < 6; i++) {
          let unitIds = []
          if (i < diffNum) {
            unitIds = windturbineIDs.slice(startIndex, startIndex + unitNum)
            startIndex += unitNum
          } else {
            unitIds = windturbineIDs.slice(startIndex, startIndex + unitNum + 1)
            startIndex += unitNum + 1
          }
          paramList.push({
            analyzeWay: 'FDNSA',
            endTime: timeValue[1],
            startTime: timeValue[0],
            windturbineIds: unitIds,
            wkCond: {
              ROTSPEED_MCS: workRange
            }
          })
        }
      } else {
        paramList = windturbineIDs.map(item => {
          return {
            analyzeWay: 'FDNSA',
            endTime: timeValue[1],
            startTime: timeValue[0],
            windturbineIds: [item],
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
          let trackLineData = []
          let lineData = []
          middle.forEach(item => {
            if (item) {
              const { evdataList } = item
              evdataList.forEach(element => {
                const {
                  windturbineName,
                  windturbineId,
                  windParkName,
                  evName,
                  evId,
                  danger,
                  warning,
                  attention,
                  evdata
                } = element
                trackLineData.push({
                  source: evdata,
                  name: `${windturbineName}_${windParkName}`,
                  id: windturbineId,
                  dimensions: ['倾斜度', '角度', '采集时间'],
                  other: {
                    danger,
                    warning,
                    attention
                  }
                })
                lineData.push({
                  source: Array.from(evdata, i => [i[2], i[0], '', true]),
                  name: `${evName.split(',')[0]}_${windturbineName}_${windParkName}`,
                  VDI: [],
                  id: evId.split(',')[0],
                  dimensions: ['time', '', '转速', '是否有波形点'],
                  other: {
                    ...element,
                    evdata: []
                  },
                  warningLine: {
                    danger,
                    warning,
                    attention
                  }
                })
              })
            }
          })
          // this.trackLineData = trackLineData
          this.$refs.track.initChart(trackLineData)
          this.$refs.towerLine.initChart({
            data: lineData,
            dimensions: ['采集时间', '倾斜率']
          })
          // this.lineData = lineData
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
    &::-webkit-scrollbar {
      width: 12px;
    }
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
  .analysis_content_item_top {
    width: 100%;
    height: 230px;
    margin-bottom: 5px;
    display: flex;
    flex-direction: row;
    justify-content: space-between;
  }
}
</style>
