<template>
  <div style="width: 100%; height: 100%">
    <slot :waveInfo="waveInfo"></slot>
    <div
      ref="chartBox"
      :style="{
        width: widthSize ? widthSize + 'px' : '100%',
        height: heightSize ? heightSize + 'px' : '100%'
      }"
      @contextmenu.prevent="
        e => {
          return false
        }
      "
      v-loading="loading"
      element-loading-text="数据加载中，请稍后..."
      element-loading-spinner="el-icon-loading"
    ></div>
  </div>
</template>
<script>
import debounce from 'lodash/debounce'
import uniqWith from 'lodash/uniqWith'
import { TimeDomainChart, SpectrumChart } from 'acharts'
import { getEigenWaveTrendApi } from '@/api/WindTurbine/CenterPartAPI.js'
import { getEchoChartDataApi } from '@/api/basicConfig/dau.js'
import * as echarts from 'echarts'
import { chartENZHList } from '@/views/customAnalysisNew/components/toolsComponent/tools'
export default {
  props: {
    requestParam: {
      type: Array,
      required: true,
      default() {
        return []
      }
    },
    chartType: {
      type: String,
      required: true,
      default: 'TimeDomain'
    },
    widthSize: {
      type: Number,
      required: false
    },
    heightSize: {
      type: Number,
      required: false
    },
    requestApi: {
      type: Boolean,
      required: false,
      default: false
    },
    theme: {
      type: String,
      required: false,
      default: 'dark'
    },
    isShowTitle: {
      type: String,
      required: false,
      default: 'show'
    },
    isDataDown: {
      type: Boolean,
      required: false,
      default: false
    }
  },
  data() {
    return {
      loading: false,
      waveInfo: {},
      // myChart: null,
      myChartInstance: null,
      chartData: {
        data: [],
        dimensions: [], // X维度unit or name ,Y维度unit or name
        titleText: '时域分析',
        yAxisType: 'value',
        xAxisType: 'value',
        xInterval: null
      }
    }
  },
  watch: {
    requestParam: {
      handler(val) {
        this.getChartData(val)
      },
      deep: true
    }
  },
  // 优化：增加防抖后的 resize 方法，提升窗口缩放时的性能
  created() {
    this.debouncedResize = debounce(() => {
      if (this.myChartInstance) {
        this.myChartInstance.resize()
      }
    }, 300)
  },
  mounted() {
    this.getChartData(this.requestParam)
  },
  beforeDestroy() {
    this.removeResizeOb()
    this.initChartInst()
  },
  methods: {
    creatResizeOb() {
      this.removeResizeOb()
      this.resizeOb = new ResizeObserver(entries => {
        this.debouncedResize()
      })
      // 指定观察dom
      this.resizeOb.observe(this.$refs.chartBox)
    },
    removeResizeOb() {
      if (this.resizeOb) {
        this.resizeOb.disconnect()
        this.resizeOb = null
      }
    },
    initChartInst() {
      if (this.myChartInstance) {
        this.myChartInstance.destroyedInstance()
        this.myChartInstance = null
      }
    },
    initChart() {
      this.loading = false
      this.initChartInst()
      this.removeResizeOb()
      if (!this.chartData.data.length) {
        this.noData = true
        return
      } else {
        this.noData = false
      }
      // this.myChart = echarts.init(this.$refs.chartBox)
      switch (this.chartType) {
        case 'TimeDomain':
          this.myChartInstance = new TimeDomainChart(
            echarts.init(this.$refs.chartBox),
            { ...this.chartData, titleText: chartENZHList[this.chartType] + '分析' },
            this.requestApi ? null : this.datazoomEvent.bind(this),
            {
              theme: this.theme || 'dark',
              isSetY: false,
              isSetX: false,
              isAddNote: false,
              isDataDown: this.isDataDown,
              isStacked: false,
              isShowTitle: this.isShowTitle == 'show' ? true : false,
              isFullData: this.requestApi ? true : false
            }
          )
          break
        case 'FreqDomain':
          this.myChartInstance = new SpectrumChart(
            echarts.init(this.$refs.chartBox),
            {
              ...this.chartData,
              titleText: chartENZHList[this.chartType] + '分析'
            },
            this.requestApi ? null : this.datazoomEvent.bind(this),
            {
              theme: this.theme || 'dark',
              isSetY: false,
              isSetX: false,
              isAddNote: false,
              isDataDown: this.isDataDown,
              isStacked: false
            },
            this.isShowTitle
          )
          break
      }
      this.creatResizeOb()
    },
    // 波形数据接口
    async getChartData(param) {
      if (!param || !param.length) {
        this.loading = false
        return
      }
      this.loading = true
      let uniqList = uniqWith(
        param,
        (arr, acc) =>
          arr.other.measlocId === acc.other.measlocId &&
          arr.coord[0] === acc.coord[0] &&
          arr.other.sampleRate === acc.other.sampleRate
      )
      let api = this.requestApi ? getEchoChartDataApi : getEigenWaveTrendApi
      await Promise.allSettled(
        uniqList.map(ele =>
          api({
            waveCategory: this.chartType, //FreqDomain
            deviceID: ele.other.windturbineId,
            sampleRate: ele.other.sampleRate,
            acqTime: ele.coord[0],
            measlocId: ele.other.measlocId,
            dataZoomXValue: '',
            takeDataVOS: true,
            takeFilterWaveData: true
          })
        )
      ).then(res => {
        let success = res.filter(i => i.status === 'fulfilled')
        if (!success.length) {
          this.loading = false
          return
        }
        let middle = success.flatMap(item => item.value.data.data)
        let chartDataSourse = []
        let waveinfo = {}
        let dimensions = []
        middle.forEach(item => {
          const {
            measlocId,
            power,
            rotatespeed,
            sampleRate,
            time,
            measlocName,
            windturbineName,
            windParkName,
            unitX,
            unitY,
            dataVOS,
            samplingtimelength
          } = item
          let name = `${measlocName}_${sampleRate}Hz_${windturbineName}_${windParkName}_${time}`
          let id = `${measlocId}&&${time}&&${sampleRate}`
          chartDataSourse.push({
            name,
            id,
            source: dataVOS,
            dimensions: [unitX, unitY]
          })
          waveinfo = { power, rotatespeed, samplingtimelength, time }
          dimensions = [unitX, unitY]
        })
        this.waveInfo = waveinfo
        this.chartData.data = chartDataSourse
        this.chartData.dimensions = dimensions
        this.initChart()
      })
    },

    // 区间放大缩小数据请求
    datazoomEvent(datazoom) {
      let { start, end, startValue, endValue } = datazoom
      Promise.allSettled(
        this.chartData.data.map(ele =>
          getEigenWaveTrendApi({
            acqTime: ele.id.split('&&')[1],
            measlocId: ele.id.split('&&')[0],
            sampleRate: ele.id.split('&&')[2],
            waveCategory: this.chartType,
            takeDataVOS: true,
            takeFilterWaveData: true,
            dataZoomXValue: start == 0 && end == 100 ? '' : startValue + ',' + endValue
          })
        )
      )
        .then(res => {
          let success = res.filter(i => i.status === 'fulfilled')
          const middle = success.flatMap(item => item.value.data.data)
          let chartDataList = this.chartData.data
          middle.forEach(item => {
            const { time, sampleRate, measlocId, windturbineId, dataVOS } = item
            if (dataVOS.length) {
              let id =
                this.chartType == 'Spd'
                  ? `${windturbineId}&&${time}`
                  : `${measlocId}&&${time}&&${sampleRate}`
              chartDataList.forEach(i => {
                if (i.id == id) {
                  i.source = dataVOS
                }
              })
            }
          })
          this.chartData.data = chartDataList
          this.myChartInstance.respondDatazoomData(datazoom, this.chartData)
        })
        .finally(() => {})
    }
  }
}
</script>
<style lang="scss" scoped></style>
