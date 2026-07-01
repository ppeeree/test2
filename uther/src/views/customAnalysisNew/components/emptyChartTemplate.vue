<template>
  <div
    class="colbox"
    :data-key="chartType"
    :id="itemId"
    ref="colbox"
    style="width: 100%; height: 100%"
  >
    <h3>
      <el-popover
        v-if="isHiddenAddrecord"
        popper-class="optionList"
        style="background: #fff; height: 300px; overflow: auto"
        placement="bottom"
        :visible="visible"
        @update:visible="visible = $event"
      >
        <el-tree
          :data="options"
          :props="{
            children: 'children',
            label: 'label'
          }"
          accordion
          @node-click="handleNodeClick"
        >
        </el-tree>
        <template #reference>
          <img
          class="menu_img"
          style="cursor: pointer; float: right; margin-left: 10px"
          :src="require('/public/img/menu1.png')"
          title="操作"
          />
        </template>
      </el-popover>
      <i
        style="font-size: 18px; float: right; margin: -2px 0px 0 10px; cursor: pointer"
        v-if="showAddRecordIcon"
        class="el-icon-edit"
        title="添加诊断记录"
        @click="addRecord"
      ></i>
      <slider-set
        v-if="chartType == 'BearingAnalysis'"
        workingUnit="pitchSpeed"
        typeName="转速"
        unit="°/s"
        :initData="initSpeed"
        @submitFilterData="changeWorking"
      >
        <template #content>
          <i
          style="font-size: 18px; float: right; margin: -2px 10px 0 0; cursor: pointer"
          class="el-icon-setting"
          ></i>
        </template>
      </slider-set>
      <unit-change
        :defaultValue="unit"
        @changeUnit="changeUnit"
        v-if="chartType == 'FreqDomain' && unit && unit.length"
      />
      <!--  <filterWave v-if="showFilterWave()" :maxHz="maxHz" @filterChange="filterChange" /> -->
    </h3>
    <div
      @contextmenu.prevent="
        e => {
          return false
        }
      "
      ref="chartBox"
      style="width: 100%; height: 100%"
      v-loading="loading"
      element-loading-text="数据加载中，请稍后..."
      element-loading-spinner="el-icon-loading"
    ></div>
    <div class="noData" v-if="noData">
      {{ chartENZHList[chartType] }}
    </div>
    <hz-btn-list
      :checkedMeasList="chartLegend"
      @handleChangeHzMark="handleChangeHzMark"
      v-if="
        type.indexOf('BVM') < 0 &&
        type.indexOf('TVM') < 0 &&
        chartType == 'FreqDomain' &&
        chartLegend &&
        chartLegend.length == 1
      "
    />
  </div>
</template>
<script>
import {
  TimeDomainChart,
  ScatterChart,
  HistogramChart,
  DipAngleChart,
  SpectrumChart,
  // WaterFallChart,
  RPMWave,
  AttachResultLineChart
} from 'acharts'
import {
  getEvAnalyzerDataApi,
  getRF,
  getBeaSFaultFrequency,
  downloadData,
  getSideFrequency
} from '@/api/analysis/index.js'
import { getEigenWaveTrendApi } from '@/api/WindTurbine/CenterPartAPI.js'
import * as echarts from 'echarts'
import dayjs from 'dayjs'
import isEqual from 'lodash/isEqual'
import differenceWith from 'lodash/differenceWith'
import uniqWith from 'lodash/uniqWith'
import { downloadTxt } from '@/util/util'
import {
  chartENZHList,
  unitChangeRecomputedData,
  typeList,
  deepFreeze
} from './toolsComponent/tools'
import SliderSet from './toolsComponent/sliderSet.vue'
import { openWindow } from '@/util/util.js'
import unitChange from './toolsComponent/unitChange.vue'
import HzBtnList from './toolsComponent/HzMark.vue'
import filterWave from './toolsComponent/filterWave.vue'
// 提取常量，避免每次渲染重新创建数组
const EIGEN_TYPE_LIST = ['RCA', 'PCA', 'WCA', 'OA', 'DA']
const HIDDEN_EDIT_TYPES = ['Waterfall', 'RCA', 'PCA', 'WCA', 'OA', 'DA']
export default {
  props: {
    rowId: {
      type: String
    },
    itemId: {
      type: String
    },
    chartInitType: {
      type: String
    },
    initWidth: {
      type: String
    },
    acqPointerInfoList: {
      type: Array,
      default: () => []
    },
    ref: {
      type: String
    },
    treeDataAndFilterP: {
      type: Object,
      default: () => {}
    },
    selectedEigenValue: {
      type: Array,
      default: () => []
    },
    type: {
      type: String
    },
    isHiddenAddrecord: {
      type: Boolean,
      default: true
    }
  },

  components: {
    SliderSet,
    unitChange,
    HzBtnList,
    filterWave
  },
  data() {
    return {
      visible: false,
      // 滤波设置
      maxHz: 0,
      //转速设置
      initSpeed: [],
      chartENZHList,
      loading: false,
      chartType: 'empty',
      noData: true,
      menuList: [
        {
          key: 'left',
          value: '左边'
        },
        {
          key: 'right',
          value: '右边'
        }
      ],
      optionsProps: {
        value: 'key',
        label: 'value',
        multiple: false,
        expandTrigger: 'hover',
        checkStrictly: true
      },
      // typeList: typeList,
      myChart: null,
      myChartInstance: null,
      chartSource: [],
      chartData: {
        data: [],
        dimensions: [], // X维度unit or name ,Y维度unit or name
        titleText: '时域分析',
        yAxisType: 'value',
        xAxisType: 'value',
        xInterval: null
      },
      chartLegend: [],
      options: typeList
    }
  },
  watch: {
    selectedEigenValue: {
      handler(val) {
        if (EIGEN_TYPE_LIST.includes(this.chartType)) {
          this.chartSource = Object.freeze([])
          this.getEvAnalyzerDataApiFunc()
        }
      },
      deep: true,
      immediate: true
    },
    chartType: {
      // 图谱类型
      handler(val) {
        if (!val) {
          return
        }
        this.loading = true
        this.chartSource = Object.freeze([])
        if (EIGEN_TYPE_LIST.indexOf(val) < 0) {
          if (this.acqPointerInfoList.length) {
            this.handleRequstData(this.acqPointerInfoList)
          } else {
            this.loading = false
            return
          }
        } else {
          this.getEvAnalyzerDataApiFunc()
        }
      }
    },
    // 趋势相关分析
    treeDataAndFilterP: {
      handler(newVal, oldVal) {
        if (EIGEN_TYPE_LIST.indexOf(this.chartType) < 0) {
          return
        }
        let newClickedEvList = newVal?.checkedMeasList || []
        let oldClickedEvList = oldVal?.checkedMeasList || []
        let newfilterParam = newVal?.filterParam || {}
        let oldfilterParam = oldVal?.filterParam || {}
        if (
          isEqual(newClickedEvList, oldClickedEvList) &&
          isEqual(newfilterParam, oldfilterParam)
        ) {
          return
        }
        if (EIGEN_TYPE_LIST.includes(this.chartType)) {
          this.getEvAnalyzerDataApiFunc()
        }
      },
      immediate: true,
      deep: true
    },
    // 波形相关分析
    acqPointerInfoList: {
      handler: function (newVal, oldVal) {
        this.changeWavePointer(newVal, oldVal)
      },
      deep: true
    },
    'myChartInstance.selectedLegend': {
      handler(val) {
        if (this.chartType == 'FreqDomain') {
          this.myChartInstance &&
            this.myChartInstance.removeAllFRMarkLine(
              ['SF', 'RF', 'BPFI', 'BPFO', 'FTF', 'BSF'],
              val
            )
          this.chartLegend = val
        }
      }
    }
  },
  computed: {
    // 优化：将模板中复杂的判断提取到计算属性中，利用缓存提高性能
    showAddRecordIcon() {
      return (
        this.chartSource?.length &&
        !HIDDEN_EDIT_TYPES.includes(this.chartType) &&
        this.isHiddenAddrecord
      )
    }
  },
  created() {},
  mounted() {
    this.chartType = this.chartInitType
    this.$refs.colbox.style.width = this.initWidth
    this.creatResizeOb()
  },
  beforeUnmount() {
    this.removeResizeOb()
    this.initChartInst()
  },
  methods: {
    handleNodeClick(data) {
      if (!data.eventName) return
      this.visible = false
      this[data.eventName](data.value)
    },
    changeWavePointer(newVal, oldVal) {
      if (EIGEN_TYPE_LIST.indexOf(this.chartType) > -1) {
        return
      }
      if (!newVal.length) {
        this.chartSource = Object.freeze([])
        this.initChart()
        return
      }
      // 取消
      let dArr1 = differenceWith(oldVal, newVal, isEqual)
      // 新增
      let dArr = differenceWith(newVal, oldVal, isEqual)
      if (dArr1.length) {
        // delete
        let data = this.chartSource.slice()
        let newData = []
        newVal.forEach(item => {
          let unit1 = []
          if (this.chartType !== 'Spd') {
            let expstr = item.slice(item.indexOf('&&') + 2)
            unit1 = data.filter(i => i.id.indexOf(expstr) > -1)
          } else {
            unit1 = data.filter(i => i.id == item.split('&&')[0] + '&&' + item.split('&&')[2])
          }
          // 双采集，一个特征值点对应多条波形，unit1/unit2设置为数组
          unit1.length ? (newData = newData.concat(unit1)) : null
        })
        this.chartSource = deepFreeze(newData) // data
        !dArr.length && this.initChart()
      }
      // 新增
      if (dArr.length) {
        // add
        this.handleRequstData(dArr)
      }
    },
    changeChartType(value) {
      this.chartType = value
    },
    removeResizeOb() {
      if (this.resizeOb) {
        this.resizeOb.disconnect()
        this.resizeOb = null
        clearTimeout(this.resizeTimer)
      }
    },
    initChartInst() {
      // 清理包装器实例（必须先于 dispose，否则 DOM 已被移除）
      if (this.myChartInstance) {
        this.myChartInstance.destroyedInstance()
        this.myChartInstance = null
      }
      // 清理图表实例
      if (this.myChart) {
        this.myChart.dispose()
        this.myChart = null
      }
    },
    creatResizeOb() {
      // 清理旧的观察器
      this.removeResizeOb()

      // 创建防抖函数
      const resizeHandler = () => {
        clearTimeout(this.resizeTimer)
        this.resizeTimer = setTimeout(() => {
          if (this.myChartInstance) {
            requestAnimationFrame(() => {
              try {
                this.myChartInstance.resize()
              } catch (error) {
                console.warn('Resize error:', error)
              }
            })
          }
        }, 100)
      }
      this.resizeOb = new ResizeObserver(resizeHandler)
      this.resizeOb.observe(this.$refs.chartBox)
    },
    removeChart() {
      this.$emit('removeChart', this.rowId, this.itemId)
    },
    removeRow() {
      this.$emit('removeRow', this.rowId)
    },
    addChart(position) {
      this.$emit('addChart', position, this.rowId, this.itemId)
    },
    initChart() {
      this.loading = false
      this.initChartInst()
      if (!this.chartSource.length) {
        this.noData = true
        return
      } else {
        this.noData = false
      }
      if (!this.myChart) {
        this.myChart = echarts.init(this.$refs.chartBox, null, {
          renderer: 'canvas',
          useDirtyRect: true,
          useHandler: 'proxy' // 5.4.0+ 内置委托，减少事件监听器数量
        })
      }
      // if (!this.myChartInstance) {
      switch (this.chartType) {
        case 'BearingAnalysis':
          this.myChartInstance = new AttachResultLineChart(
            this.myChart,
            {
              ...this.chartData,
              data: this.chartSource,
              titleText: chartENZHList[this.chartType]
            },
            this.datazoomEvent.bind(this),
            { isDataDown: false }
          )
          break
        case 'TimeDomain':
        case 'Order':
        case 'OrderEnvelope':
        case 'Envelope':
          this.myChartInstance = new TimeDomainChart(
            this.myChart,
            {
              ...this.chartData,
              data: this.chartSource,
              titleText: chartENZHList[this.chartType]
            },
            this.datazoomEvent.bind(this),
            { isDataDown: false }
          )
          break
        case 'RCA':
        case 'PCA':
        case 'WCA':
          this.myChartInstance = new ScatterChart(this.myChart, {
            ...this.chartData,
            data: this.chartSource,
            titleText: chartENZHList[this.chartType]
          })
          break
        case 'FreqDomain':
        case 'OrderSpectrum':
        case 'OrderEnvelopeSpectrum':
        case 'Cepstrum':
        case 'EnvelopeSpectrum':
          this.myChartInstance = new SpectrumChart(
            this.myChart,
            {
              ...this.chartData,
              data: this.chartSource,
              titleText: chartENZHList[this.chartType]
            },
            this.datazoomEvent.bind(this),
            { isDataDown: false }
          )
          break
        /*   case 'Waterfall':
          this.myChartInstance = new WaterFallChart(this.myChart, {
            ...this.chartData,
            yAxisType: 'category',
            xAxisType: 'value',
            zAxisType: 'value',
            titleText: '瀑布图',
            dimensions: [this.chartData.dimensions[0], '', this.chartData.dimensions[1]]
          })
          break */
        case 'Spd':
          this.myChartInstance = new RPMWave(this.myChart, {
            ...this.chartData,
            data: this.chartSource,
            yAxisType: 'value',
            xAxisType: 'value',
            titleText: '转速波形'
          })
          break
        case 'DA': // 分布
          this.myChartInstance = new HistogramChart(this.myChart, {
            ...this.chartData,
            data: this.chartSource,
            titleText: chartENZHList[this.chartType]
          })
          break
        /*    case 'OA': // 倾覆
          this.myChartInstance = new DipAngleChart(this.myChart, {
            ...this.chartData,
            dimensions: ['X倾角°', 'Y倾角°'],
            titleText: chartENZHList[this.chartType]
          })
          break */
      }

      /* } else {
        this.noData = true
      } */
      /*}  else {
        this.myChartInstance.updateData({
          ...this.chartData,
          data: this.chartSource,
          titleText: chartENZHList[this.chartType]
        })
      } */
    },
    // 采集时间波形点相关的数据请求
    async handleRequstData(pointerList) {
      this.loading = true
      let uniqList
      if (this.chartType == 'Spd') {
        uniqList = uniqWith(
          pointerList,
          (arr, acc) =>
            arr.split('&&')[0] === acc.split('&&')[0] && arr.split('&&')[2] == acc.split('&&')[2]
        )
      } else {
        uniqList = uniqWith(pointerList, (arr, acc) => arr === acc)
      }
      await Promise.allSettled(
        uniqList.map(ele =>
          getEigenWaveTrendApi({
            deviceID: ele.split('&&')[0],
            acqTime: ele.split('&&')[2],
            measlocId: ele.split('&&')[1] /* + ele.split('&&')[1] */,
            waveCategory: this.chartType,
            sampleRate: ele.split('&&')[3] || '',
            takeDataVOS: true,
            dataZoomXValue: '',
            takeFilterWaveData:
              this.chartType == 'FreqDomain' &&
              (ele.split('&&')[1].includes('GBX') ||
                ele.split('&&')[1].includes('GEN') ||
                ele.split('&&')[1].includes('MST'))
                ? false
                : true
          })
        )
      )
        .then(res => {
          let success = res.filter(i => i.status === 'fulfilled')
          if (!success.length) {
            this.loading = false
            return
          }
          let middle = success.flatMap(item => item.value.data.data)
          let chartDataList = this.chartSource.slice()
          let dimensionsX = [],
            dimensionsY = []
          middle.forEach((item, i) => {
            const {
              measlocId,
              waveDefId,
              time,
              measlocName,
              windturbineName,
              windturbineId,
              windParkName,
              unitX,
              unitY,
              dataVOS,
              dataKv,
              sampleRate
            } = item
            if (dataVOS.length) {
              let { fault, modeAlgorithm } = dataKv
              this.initSpeed = modeAlgorithm
                ? [modeAlgorithm.pitchSpeedDown, modeAlgorithm.pitchSpeedUp]
                : []
              let name =
                this.chartType == 'Spd'
                  ? `${windturbineName}_${windParkName}_${time}`
                  : `${measlocName}_${sampleRate + 'Hz_'}${windturbineName}_${windParkName}_${time}`
              let id =
                this.chartType == 'Spd'
                  ? `${windturbineId}&&${time}`
                  : `${measlocId}&&${time}&&${sampleRate}`
              this.maxHz = Math.max(this.maxHz, sampleRate)
              chartDataList.push({
                name,
                id,
                time: time,
                source: dataVOS, // 对大数据应用采样
                dimensions: [unitX, unitY],
                markList: fault,
                others: {
                  ...item,
                  dataVOS: []
                }
              })
            }
            i == 0 || dimensionsX.indexOf(unitX) < 0 ? dimensionsX.push(unitX) : null
            i == 0 || dimensionsY.indexOf(unitY) < 0 ? dimensionsY.push(unitY) : null
          })
          // 去掉因为请求滞后导致的不存在的数据
          let newData = []
          if (this.chartType == 'Spd') {
            // 转速波形id = 机组id + 采集时间唯一
            let uniqListIds = uniqWith(
              this.acqPointerInfoList,
              (arr, acc) =>
                arr.split('&&')[0] === acc.split('&&')[0] &&
                arr.split('&&')[2] == acc.split('&&')[2]
            )
            uniqListIds.forEach(item => {
              let unit1 = chartDataList.find(
                i => i.id == item.split('&&')[0] + '&&' + item.split('&&')[2]
              )
              unit1 && newData.push(unit1)
            })
          } else {
            this.acqPointerInfoList.forEach(item => {
              let arr = item.split('&&') // leng=3 特征值传参过来的 length==4 波形列表or波形索引传参过来的
              let unit1 = chartDataList.filter(i =>
                arr.length == 3
                  ? i.id.indexOf(arr[1] + '&&' + arr[2]) > -1
                  : item.indexOf(i.id) > -1
              )
              unit1.length ? (newData = newData.concat(unit1)) : null
            })
          }
          this.chartSource = deepFreeze(newData) // 时间排序 dateSort(newData, 'time', false) //chartDataList
          this.chartData.dimensions = [
            dimensionsX.length == 1 ? dimensionsX[0] : '',
            dimensionsY.length == 1 ? dimensionsY[0] : ''
          ]
          // 单位转换只针对[m/s^2, Hz]进行转换
          this.unit =
            this.chartData.dimensions[0] == 'Hz' && this.chartData.dimensions[1] == 'm/s^2'
              ? this.chartData.dimensions
              : []
          this.initChart()
        })
        .finally(() => {})
    },
    // 趋势相关数据接口
    getEvAnalyzerDataApiFunc() {
      if (this.selectedEigenValue.length == 0) {
        this.initChart()
        return
      }
      this.loading = true
      let idArr = this.selectedEigenValue
      let { filterParam, checkedMeasList } = this.treeDataAndFilterP
      let { timeValue, wkCond } = filterParam
      if (!timeValue.length) {
        timeValue = [
          dayjs().subtract(90, 'day').format('YYYY-MM-DD 00:00:00'),
          dayjs().format('YYYY-MM-DD 23:59:59')
        ]
      }
      Promise.allSettled(
        idArr.map(ele =>
          getEvAnalyzerDataApi({
            analyzeWay: this.chartType,
            endTime: timeValue[1],
            startTime: timeValue[0],
            eigenValueIds: [ele],
            windturbineIds: /* this.chartType != 'OA' ? [] : */ [ele],
            wkCond
          })
        )
      ).then(res => {
        let success = res.filter(i => i.status === 'fulfilled')
        if (!success.length) {
          this.loading = false
          return
        }
        const middle = success.flatMap(item => item.value.data.data)
        let chartDataList = []
        middle.forEach(item => {
          if (item.evdataList?.length) {
            chartDataList = [...chartDataList, ...this.dealTrendData(item)]
          }
        })
        this.chartSource = deepFreeze([...chartDataList])
        this.initChart()
      })
    },
    // 特征值趋势返回值数据处理
    dealTrendData(data) {
      const { evdataList } = data
      let lineDatas = []
      evdataList.length &&
        evdataList.forEach((element, index) => {
          const {
            windturbineName,
            windturbineId,
            windParkName,
            evName,
            measlocName,
            evdata,
            evId,
            sampleRate,
            unitX,
            unitY
          } = element
          let name = `${evName}_${
            sampleRate < 0 ? '' : sampleRate + 'Hz_'
          }${measlocName}_${windturbineName}_${windParkName}`
          lineDatas.push({
            source: evdata,
            name: this.chartType == 'OA' ? `${windturbineName}_${windParkName}` : name,
            id:
              this.chartType == 'OA'
                ? windturbineId
                : evId + (sampleRate < 0 ? '' : '~' + sampleRate), //
            dimensions: [unitX, unitY]
          })
        })
      return lineDatas
    },
    openPage() {
      let encodedObject = encodeURIComponent(
        JSON.stringify({
          type: this.type,
          chartType: this.chartType,
          acqPointerInfoList: this.acqPointerInfoList,
          treeDataAndFilterP: this.treeDataAndFilterP
        })
      )
      let url = `#/analysis/bigchart?data=${encodedObject}`
      openWindow(url, '_blank', 1200, 600)
    },
    addRecord() {
      // 设定规则：只能给一条曲线增加分析记录
      if (this.myChartInstance.selectedLegend.length != 1) {
        return this.$message.warning('请选择一条曲线进行分析！')
      }
      let reader = new FileReader()
      let data = this.myChartInstance.imageBase64Data(true)
      reader.readAsDataURL(data)
      let recordChartInfo = this.chartSource.find(
        item => item.name == this.myChartInstance.selectedLegend[0]
      ).others
      reader.onload = e => {
        this.$emit('getImgData', {
          data: e.target.result,
          recordChart: chartENZHList[this.chartType],
          isOpen: true,
          recordChartInfo
        })
      }
    },
    // 轴承分析，修改转速
    changeWorking({ code, up, down }) {
      if (!this.chartSource.length) {
        return
      }
      this.loading = true
      Promise.allSettled(
        this.chartSource.map(ele =>
          getEigenWaveTrendApi({
            // windturbineId: ele.id.split('&&')[0],
            acqTime: ele.id.split('&&')[1],
            // measlocCode: ele.id.split('&&')[1],
            measlocId: ele.id.split('&&')[0],
            sampleRate: ele.id.split('&&')[2] || '',
            waveCategory: this.chartType,
            takeDataVOS: true,
            pitchSpeedUp: up,
            pitchSpeedDown: down,
            takeFilterWaveData: true
          })
        )
      )
        .then(res => {
          let success = res.filter(i => i.status === 'fulfilled')
          let infoList = []
          const middle = success.flatMap(item => item.value.data.data)
          middle.forEach(item => {
            const {
              measlocName,
              windturbineName,
              windParkName,
              measlocId,
              waveDefId,
              time,
              dataKv,
              sampleRate
            } = item
            const { fault } = dataKv
            if (fault && fault.length) {
              let id = `${measlocId}&&${time}&&${sampleRate}`
              let name = `${measlocName}_${
                sampleRate + 'Hz_'
              }${windturbineName}_${windParkName}_${time}`
              infoList.push({
                id,
                name,
                markList: fault
              })
            }
          })
          this.myChartInstance.updatedMarkInfo(infoList)
          this.loading = false
        })
        .finally(() => {})
    },

    // 区间放大缩小数据请求
    datazoomEvent(datazoom) {
      const { start, end, startValue, endValue } = datazoom
      let ids = this.chartSource.map(item => item.id)
      Promise.allSettled(
        ids.map(ele =>
          getEigenWaveTrendApi({
            acqTime: ele.split('&&')[1],
            measlocId: ele.split('&&')[0],
            sampleRate: ele.split('&&')[2],
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
          let newData = []
          middle.forEach(item => {
            const { time, waveDefId, sampleRate, measlocId, windturbineId, dataVOS } = item
            if (dataVOS.length) {
              let id =
                this.chartType == 'Spd'
                  ? `${windturbineId}&&${time}`
                  : `${measlocId}&&${time}&&${sampleRate}`
              this.chartSource.forEach(i => {
                if (i.id == id) {
                  newData.push({
                    ...i,
                    source: dataVOS // 对数据应用采样
                  })
                }
              })
            }
          })
          this.chartSource = deepFreeze(newData)
          this.myChartInstance.respondDatazoomData(datazoom, {
            ...this.chartData,
            data: this.chartSource
          })
        })
        .finally(() => {})
    },
    // 修改单位
    changeUnit(val) {
      if (JSON.stringify(this.unit) == JSON.stringify(val)) {
        return
      } else {
        this.unit = val
        // 数据重计算，重新渲染chartSource
        /*  let chartData = cloneDeep(this.chartData)
        chartData.data = chartData.data.map(item => {
          return {
            ...item,
            source: unitChangeRecomputedData(val, item.source)
          }
        }) */
        let chartData = this.chartSource.map(item => {
          return {
            ...item,
            source: unitChangeRecomputedData(val, item.source) // 对转换后的数据应用采样
          }
        })
        this.chartSource = deepFreeze(chartData)
        this.chartData.dimensions = val
        this.myChartInstance.unitChangeRespondData({
          ...this.chartData,
          data: this.chartSource
        })
      }
    },
    // 数据下载
    downloadData() {
      if (this.chartSource.length == 0) {
        return this.$message.warning('暂无数据！')
      }
      this.$message.warning('数据获取中，请稍后......')
      let originalWaveformDataRequestList = []
      this.chartSource.forEach(ele => {
        originalWaveformDataRequestList.push({
          acqTime: ele.id.split('&&')[1],
          measlocId: ele.id.split('&&')[0],
          waveDefId: ele.id.split('&&')[2],
          waveCategory: this.chartType
        })
      })
      downloadData({ originalWaveformDataRequestList }).then(res => {
        if (res) {
          let temp = res.headers['content-disposition'].split(';')[1].split('=')[1]
          //对文件名乱码转义
          let fileName = decodeURI(temp)
          downloadTxt(res.data, fileName)
          this.$message.success('数据下载成功！')
        } else {
          this.$message.error('数据下载失败！')
        }
      })
    },
    handleChangeHzMark(type, operate) {
      let { windturbineId, measlocId, time, sampleRate } = this.chartSource.find(
        i => i.name == this.chartLegend[0]
      ).others
      if (operate == 'add') {
        if (type == 'RF') {
          getRF({
            WindturbineID: windturbineId,
            AcqTime: time,
            MeasLoctionID: measlocId,
            SampleRate: sampleRate
          }).then(res => {
            if (res.data.success) {
              const { rotorFrequencyDoubling } = res.data.data
              if (rotorFrequencyDoubling.length) {
                /*  let arr = Array.from(rotorFrequencyDoubling, item => ({
                label: item.multipleLabel,
                value: item.frequencyValue
              }))
               this.myChartInstance.myYB.creatYBGroup('BeiPinYB', this.chartLegend, arr) */
                let arr = Array.from(rotorFrequencyDoubling, item => item.frequencyValue)
                this.myChartInstance.addMarkLine(arr, 'RF')
              } else {
                this.$message.warning('无数据！')
              }
            } else {
              this.$message.error(res.data.message)
            }
          })
        } else {
          getBeaSFaultFrequency({
            WindturbineID: windturbineId,
            MeasLoctionID: measlocId,
            AcqTime: time,
            BearFaultFrequencyType: type,
            SampleRate: sampleRate
          }).then(res => {
            if (res.data.success) {
              const { bearFaultFrequencyDoubling } = res.data.data
              if (bearFaultFrequencyDoubling.length) {
                let arr = Array.from(bearFaultFrequencyDoubling, item => item.frequencyValue)
                this.myChartInstance.addMarkLine(arr, type)
              } else {
                this.$message.warning('无数据！')
              }
            } else {
              this.$message.error(res.data.message)
            }
          })
        }
      } else if (operate == 'cancel') {
        this.myChartInstance.removeMarkLine(type)
      } else {
        // SF要根据选中的标记线单独判断
        if (
          !this.myChartInstance.clickedMarklineInfo.name ||
          !(
            this.myChartInstance.clickedMarklineInfo.name == 'BSF' ||
            this.myChartInstance.clickedMarklineInfo.name == 'BPFI'
          )
        ) {
          return this.$message.warning('请先点击图表中内圈或者保持架的标记线！')
        }
        const { name, value } = this.myChartInstance.clickedMarklineInfo
        let options = this.myChart.getOption()
        let { id, markLine, markPoint } = options.series.find(i => i.name == this.chartLegend[0])
        let exitList = markLine.data.filter(i => i.name == name + '_' + 'SF' + '_' + value)
        if (exitList.length) {
          let others = markLine.data.filter(i => i.name != name + '_' + 'SF' + '_' + value)
          let othersPoint = markPoint.data.filter(i => i.name != name + '_' + 'SF' + '_' + value)
          this.myChart.setOption({
            series: [
              {
                id,
                markLine: {
                  animation: false,
                  symbol: 'none',
                  data: [...others]
                },
                markPoint: {
                  data: [...othersPoint]
                }
              }
            ]
          })
        } else {
          getSideFrequency({
            WindturbineID: windturbineId,
            MeasLoctionID: measlocId,
            AcqTime: time,
            FaultFrequency: this.myChartInstance.clickedMarklineInfo.value,
            BearFaultFrequencyType: this.myChartInstance.clickedMarklineInfo.name,
            SampleRate: sampleRate
          }).then(res => {
            if (res.data.success) {
              if (res.data.data.length) {
                res.data.data.splice(3, 1)
                this.myChartInstance.addMarkLine(res.data.data, type)
              } else {
                this.$message.warning('无数据！')
              }
            } else {
              this.$message.error(res.data.message)
            }
          })
        }
      }
    },

    showFilterWave() {
      return (
        this.chartSource.length &&
        (this.chartType == 'TimeDomain' ||
          this.chartType == 'Order' ||
          this.chartType == 'OrderEnvelope' ||
          this.chartType == 'Envelope')
      )
    }
    // 数据采样方法：对大数据集进行采样，减少渲染压力
    /*   sampleData(data, maxPoints = 2000) {
      if (!data || data.length <= maxPoints) {
        return data
      }

      const step = Math.ceil(data.length / maxPoints)
      const sampled = []

      for (let i = 0; i < data.length; i += step) {
        sampled.push(data[i])
      }

      return sampled
    } */
  }
}
</script>
<style lang="scss" scoped>
.colbox {
  // border: 1px solid #000;
  text-align: center;
  height: 100%;
  position: relative;
  //padding-top: 15px;
  background: #fff;
  border-radius: 5px;
  h3 {
    position: absolute;
    right: 5px;
    top: 3px;
    z-index: 20;
    width: auto;
    height: 28px;
    padding: 5px;
  }
  .noData {
    width: 100%;
    height: 100%;
    position: absolute;
    top: 0;
    left: 0;
    font-size: 40px;
    color: #eee;
    text-align: center;
    display: flex; /*实现垂直居中*/
    align-items: center; /*实现水平居中*/
    justify-content: center;
    text-align: justify;
    margin: 0 auto;
    overflow: hidden;
    word-break: break-word;
  }
}
.dropDown {
  border-top: 1px solid #ccc;
}
.optionList {
  .el-tree {
    background: #fff;
    height: auto;
    max-height: 300px;
    overflow-y: auto;
  }
}
</style>
