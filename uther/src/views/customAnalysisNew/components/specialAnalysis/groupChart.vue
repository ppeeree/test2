<template>
  <div class="group-analysis" style="width: 100%; height: 100%">
    <h3>
      <span class="group-analysis__title">{{ titleText }}</span>
      <chart-tools ref="chartTools" :key="titleText" @operationChange="operationChange" />
    </h3>
    <div class="group-analysis__left" style="width: 100%">
      <div
        class="group-analysis__content__item"
        v-for="item in chartList"
        :key="item.code"
        @click="selectedChart = item.code"
        :style="{
          height: 94 / chartList.length + '%',
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
          :isConnect="true"
          @clickEvent="getWavePointer"
          @getImgData="getImgData"
          @getSelectValue="getSelectValue"
          @connectEvent="connectEvent"
        ></unit-chart>
      </div>
    </div>
  </div>
</template>
<script>
import chartTools from '../chartComponent/chartTool.vue'
import unitChart from '../chartComponent/unitChart.vue'
import Splitter from '../splitter'
export default {
  components: {
    chartTools,
    unitChart,
    Splitter
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
      isExpand: false
    }
  },
  watch: {
    chartList: {
      handler(val) {
        this.waveInfoDataListObj = {}
        // 联动代码
        if (val.length) {
          this.$nextTick(() => {})
        }
        // 联动代码结束
      },
      immediate: true
    },
    waveInfoDataListObj: {
      handler(val) {
        let xList = []
        const flatArray = Object.values(val).flatMap(val => val)
        if (flatArray.length) {
          let wavepointers = []
          let waveInfo = []
          wavepointers = flatArray.map(item => {
            let { coord, other } = item
            waveInfo.push(other)
            xList.push(coord[0])
            const { windturbineId, measlocId, sampleRate } = other
            // 波形ID：机组ID+测量ID+采集时间
            return windturbineId + '&&' + measlocId + '&&' + coord[0] + '&&' + sampleRate //
          })
          this.$emit('getWavePointer', this.keyid, [...new Set(wavepointers)])
        } else {
          this.$emit('getWavePointer', this.keyid, [])
        }
        // 刷新重绘图谱的markline标记线
        for (let i = 0; i < this.chartList.length; i++) {
          if (this.$refs[this.chartList[i].code] && this.$refs[this.chartList[i].code][0]) {
            this.$refs[this.chartList[i].code][0].updateMarkLineIndicator(xList)
          }
        }
      },
      deep: true
    },
    selectedChart: {
      handler(val) {
        this.$emit('getSelectValue', this.$refs[this.selectedChart][0].selectValue)
      }
    }
  },
  mounted() {
    this.$nextTick(() => {
      this.selectedChart = this.chartList.length ? this.chartList[0].code : ''
    })
  },
  /*  beforeUnmount() {
    if (this.resizeOb) {
      this.resizeOb.disconnect()
      this.resizeOb = null
    }
  }, */
  methods: {
    // 初始化使用
    getSelctedChartEigenValue() {
      if (this.selectedChart.length && this.$refs[this.selectedChart][0]) {
        return this.$refs[this.selectedChart][0].selectValue
      }
    },
    getSelectValue(data) {
      this.$emit('getSelectValue', data)
    },
    /*  creatResizeOb() {
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
        this.$refs[this.selectedChart][0].operation(operation, content)
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
    /*    changeSelectLegend(item) {
      this.dataArray.forEach(unit => {
        unit.data.data.forEach(dataItem => {
          if (dataItem.name == item.name) {
            this.$refs[unit.id][0].changeSelectLegend(item.name)
          }
        })
      })
      if (this.selectedLegendList.includes(item.name)) {
        this.selectedLegendList = this.selectedLegendList.filter(legend => legend != item.name)
      } else {
        this.selectedLegendList.push(item.name)
      }
    }, */
    handleChange() {
      if (!this.selectedChart.length) {
        return alert('请选择图表')
      }
    },
    // 联动图谱同步响应点击事件
    connectEvent(key, data) {
      this.chartList.forEach(item => {
        if (item.code !== key) {
          console.log(this.$refs[item.code])
          console.log(this.$refs[item.code][0].myChartInstance)
          this.$refs[item.code][0].myChartInstance.chartClickEvent(data)
        }
      })
    },
    // 点击趋势上的波形点信息方法
    getWavePointer(key, data, ctrlKey) {
      // 没按ctrl键，清除其他图谱所有选中点
      if (!ctrlKey) {
        this.chartList.forEach(item => {
          if (item.code !== key) {
            this.$refs[item.code][0].clearWavePointer()
            this.waveInfoDataListObj[item.code] = []
          }
        })
      }
      this.waveInfoDataListObj[key] = data
    },
    getImgData(data) {
      this.$emit('getImgData', data)
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
      font-weight: bold;
      line-height: 30px;
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
}
</style>
