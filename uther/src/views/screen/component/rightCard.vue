<template>
  <div class="content-right">
    <el-card class="box-card" :body-style="{ padding: '10px' }">
      <template #header>
        <div class="title">
        <div class="top_title">
          <span class="titleChart">综合健康指数及排名</span>
          <span class="title_buttom" style="width: calc(100% - 196px)"></span>
        </div>
        <iconChart
          :style="handleIconBorder('heatMap')"
          @click="handleOpenChart('heatMap')"
        ></iconChart>
        </div>
      </template>
      <div style="width: 450px">
        <div class="ranking-item">
          <gauge :gaugeSeretData="gaugeSeretData"></gauge>
          <div class="title">
            <div>
              {{ '' }}
            </div>
            <div @click="handleSort">正序/倒序</div>
          </div>
          <div class="bar-progres" style="height: 835px" v-if="devicesRealTime.length">
            <vircualList :items="devicesRealTime" :itemHeight="34.5" :shownumber="24">
              <template v-slot="{ item }">
                <div
                  :class="[activeNum === item.windturbineId ? 'active-box' : '']"
                  @click="handlerIndex(item)"
                >
                  <el-tooltip
                    effect="light"
                    :content="item.spaceName"
                    placement="top"
                    :enterable="false"
                  >
                    <span>{{ item.spaceName }}</span>
                  </el-tooltip>
                  <el-progress
                    :percentage="handlePercentageData(item)"
                    :stroke-width="12"
                    :format="format"
                    define-back-color="#445461"
                    :color="handleCustomColor(item.data[0].index)"
                    style="width: 78%"
                  ></el-progress>
                </div>
              </template>
            </vircualList>
          </div>
          <div style="height: 835px" v-else>
            <noData noteText="暂无数据" firstText="" />
          </div>
        </div>
        <div style="display: none">
          <div style="position: relative; top: -15px">
            <div class="top_title" style="margin-top: 10px">
              <span class="titleChart">全流程事件跟踪</span>
              <span class="title_buttom" style="width: calc(100% - 163px)"></span>
            </div>
            <iconChart
              :style="[{ left: '10px' }, handleIconBorder('area')]"
              @click="handleOpenChart('area')"
            ></iconChart>
          </div>
          <eventTracking :eventNum="eventNum" />
          <div
            class="charts-class"
            :style="{ opacity: peiData1.length && peiData2.length ? '1' : '0.3' }"
          >
            <!-- :radiusGrid="{ center: ['34%', '50%'] }" -->
            <pie-ring
              unique="first"
              :color="['#FFF287', '#F5B270', '#E85E51', '#DC1034']"
              :peiData="peiData1"
              width="210px"
              height="120px"
              @clickPartEvent="clickPartEvent"
            ></pie-ring>
            <pie-ring
              unique="second"
              :color="['#F76560', '#7A72FF', '#E72EFF']"
              :peiData="peiData2"
              width="210px"
              height="120px"
              seriesName="事件类型"
              @clickPartEvent="clickPartEvent"
            ></pie-ring>
            <!-- <pie-ring
            unique="third"
            :color="['#00E5FF', '#0195FF', '#004CFF']"
            :peiData="peiData3"
            @clickPartEvent="clickPartEvent"
          ></pie-ring> -->
          </div>
          <left-table :tableListData="tableListData" v-on="$attrs" />
        </div>
      </div>
    </el-card>
    <div ref="heatMap" v-if="heatMap" class="card-1">
      <span class="span-title">健康指数发展趋势</span>
      <i @click="handleOpenChart('heatMap')" class="el-icon-close" />
      <vircualList
        v-if="!showHeatMapLegend"
        :items="heatMapLegend"
        :itemHeight="37"
        :shownumber="8"
      >
        <template v-slot="{ item }">
          <heatMap-dens
            :key="item.value"
            :unique="item.value"
            :legend="[item.label]"
            :seriesData="heatMapSeriesData[item.value]"
          />
        </template>
      </vircualList>
      <noData v-else firstText="暂无数据" noteText="请联系管理员检查数据采集服务或者网络链接" />
    </div>
    <div ref="area" v-if="area" class="card-2">
      <i @click="handleOpenChart('area')" class="el-icon-close" />
      <div class="chart">
        <line-category
          v-if="!showEventSeret"
          :unique="3"
          height="50%"
          title="事件类型发展趋势"
          :legend="eventTypeLegend"
          :seriesData="eventSeretData"
          :xAxisData="xAxisDataEH"
        />
        <noData
          v-else
          firstText="暂无数据"
          noteText="请联系管理员检查数据采集服务或者网络链接"
          style="height: 50%"
        />
        <line-area v-if="!showAreaChart" height="50%" :areaData="AreaChartdata" />
        <noData
          v-else
          firstText="暂无数据"
          noteText="请联系管理员检查数据采集服务或者网络链接"
          style="height: 50%"
        />
      </div>
    </div>
  </div>
</template>

<script>
import { getWindTurbineIndexRankingStatistics, getEventTrend } from '@/api/screen/rightCardApi'
import { getClassificationResults } from '@/api/screen/index'
import find from 'lodash/find'
import filter from 'lodash/filter'
import isArray from 'lodash/isArray'
import cloneDeep from 'lodash/cloneDeep'
import uniq from 'lodash/uniq'
import CesuimCanvas from '@/util/cesuimCanvas'
import { HandleEventTrackingInfoData, FetchData } from '../mixins/handleTableData'
import dayjs from 'dayjs'
import { createEnum } from '@/util/exp'
import { defineAsyncComponent } from 'vue'

const syncEum = {
  eventLevel: {
    key: 'levelData',
    val: createEnum({
      first: 1,
      second: 2,
      third: 3,
      fourth: 4
    })
  },
  eventType: {
    key: 'typeData',
    val: createEnum({
      health: 3,
      inspection: 1,
      work: 4
    })
  },
  eventStatus: {
    key: 'statusData',
    val: createEnum({
      done: 1,
      notdone: 0
    })
  }
}

const eventTypeNum = {
  3: 'health',
  1: 'inspection',
  4: 'work'
}

export default {
  components: {
    noData: defineAsyncComponent(() => import('@/components/noData/index.vue')),
    GaugeBasic: defineAsyncComponent(() => import('../base/gauge-basic.vue')),
    BarStacked: defineAsyncComponent(() => import('../base/bar-stacked.vue')),
    PieRing: defineAsyncComponent(() => import('../base/pie-ring.vue')),
    leftTable: defineAsyncComponent(() => import('../table/leftTable.vue')),
    heatMapDens: defineAsyncComponent(() => import('../base/heatMap-dens.vue')),
    lineCategory: defineAsyncComponent(() => import('../base/line-category.vue')),
    lineArea: defineAsyncComponent(() => import('../base/line-area.vue')),
    gauge: defineAsyncComponent(() => import('../charts/gauge.vue')),
    iconChart: defineAsyncComponent(() => import('../charts/iconChart.vue')),
    vircualList: defineAsyncComponent(() => import('./vircualList.vue')),
    eventTracking: defineAsyncComponent(() => import('./eventTracking.vue'))
  },

  data() {
    return {
      num: 0,
      tableListData: [],
      tableListDataCopy: [],
      peiData1: [],
      peiData2: [],
      peiData3: [],
      heatMap: false,
      area: false,
      eventTypeLegend: ['工作事件', '巡检事件', '健康事件'],
      eventSeretData: [],
      activeNum: '',
      gaugeSeretData: {},
      devicesRealTime: [],
      heatMapSeriesData: [],
      heatMapLegend: [],
      reClickPart: '',
      eventNum: {},
      xAxisDataEH: [],
      AreaChartdata: {}
    }
  },
  watch: {
    heatMap: {
      handler: function (val) {
        if (val) {
          const localValue =
            JSON.parse(sessionStorage.getItem('selectWindFarm'))?.id ||
            JSON.parse(localStorage.getItem('saber-userInfo'))?.content.dept_id ||
            null
          this.getWindTurbine(
            localValue,
            dayjs().subtract(90, 'day').format('YYYY-MM-DD HH:mm:ss'),
            dayjs().format('YYYY-MM-DD HH:mm:ss')
          )
        }
      }
    }
  },
  computed: {
    showHeatMapLegend() {
      return this.heatMapLegend.length === 0
    },
    showEventSeret() {
      return this.eventSeretData.length === 0
    },
    showAreaChart() {
      return JSON.stringify(this.AreaChartdata) === '{}'
    }
  },
  created() {
    this.init()
    this.getTableList()
  },
  mounted: function () {
    this.$bus.$on('progressClick', () => {
      this.activeNum = ''
    }),
      window.addEventListener('setItem', () => {
        if (sessionStorage.getItem('clickChart') === 'leftChart') {
          const key = ['heatMap', 'area']
          key.forEach(item => {
            if (this[item]) {
              this.handleOpenChart(item)
            }
          })
        }
      })
  },
  methods: {
    async init() {
      const localValue =
        JSON.parse(sessionStorage.getItem('selectWindFarm'))?.id ||
        JSON.parse(localStorage.getItem('saber-userInfo'))?.content.dept_id ||
        null
      this.getWindTurbine(localValue)
      this.getEventTrend(localValue)
    },
    async getEventTrend(localValue) {
      const params = {
        id: localValue,
        endTime: dayjs().format('YYYY-MM-DD HH:mm:ss'),
        startTime: dayjs().subtract(3, 'month').format('YYYY-MM-DD HH:mm:ss'),
        type: 'WINDPARK'
      }
      const { data: data } = await getEventTrend(params)
      const { time, seretData } = this.handleEquipmentHealth(data)
      this.AreaChartdata = this.handleAreaChart(data)
      this.eventSeretData = seretData
      this.xAxisDataEH = time
    },
    handleEquipmentHealth(data) {
      if (!data.data?.data) return { time: [], seretData: [] }
      const seretData = [
        {
          title: '工作事件',
          color: '#E72EFF',
          data: []
        },
        {
          title: '巡检事件',
          color: '#F76560',
          data: []
        },
        {
          title: '健康事件',
          color: '#7A72FF',
          data: []
        }
      ]
      const seretNum = {
        workCount: 0,
        inspectionCount: 1,
        healthCount: 2
      }
      const time = []
      data.data.data.forEach(item => {
        for (const key in item) {
          if (Object.hasOwnProperty.call(item, key)) {
            if (key === 'time') {
              time.push(dayjs(item[key]).format('MM-DD'))
            }
            typeof seretNum[key] === 'number' && seretData[seretNum[key]].data.push(item[key])
          }
        }
      })
      return { time, seretData }
    },
    handleAreaChart(data) {
      let firstCountList = [],
        secondCountList = [],
        thirdCountList = [],
        fourthCountList = [],
        timeList = []
      data.data?.data &&
        data.data.data.forEach(({ firstCount, secondCount, thirdCount, fourthCount, time }) => {
          let sum = firstCount + secondCount + thirdCount + fourthCount
          firstCountList.push(Math.floor((firstCount / sum) * 1000) / 1000)
          secondCountList.push(Math.floor((secondCount / sum) * 1000) / 1000)
          thirdCountList.push(Math.floor((thirdCount / sum) * 1000) / 1000)
          fourthCountList.push(Math.floor((fourthCount / sum) * 1000) / 1000)
          timeList.push(dayjs(time).format('MM-DD'))
        })
      return {
        seriesData: [
          {
            name: '一级',
            value: firstCountList
          },
          {
            name: '二级',
            value: secondCountList
          },
          {
            name: '三级',
            value: thirdCountList
          },
          {
            name: '四级',
            value: fourthCountList
          }
        ],
        timeList
      }
    },
    getTableList() {
      const localValue =
        JSON.parse(sessionStorage.getItem('selectWindFarm'))?.id ||
        JSON.parse(localStorage.getItem('saber-userInfo'))?.content.dept_id ||
        null
      let fetchData = new FetchData(localValue, 'WINDPARK')
      fetchData.getFetchData.then(res => {
        if (!res.data?.data) return
        const { levelData, typeData, statusData, eventInfo, eventNum } = res.data.data
        this.eventNum = eventNum
        const trackingInfoData = new HandleEventTrackingInfoData()
        trackingInfoData.tableData = eventInfo
        trackingInfoData.levelData = levelData
        trackingInfoData.eventType = typeData
        trackingInfoData.statusData = statusData
        this.peiData1 = trackingInfoData.levelData
        this.peiData2 = trackingInfoData.eventType
        this.peiData3 = trackingInfoData.statusData
        this.tableListData = trackingInfoData.tableData
        this.tableListDataCopy = cloneDeep(trackingInfoData.tableData)
      })
    },
    handlerIndex(item) {
      this.$bus.$emit('pieDoughnutRe', {
        all: true,
        id: null
      })
      this.$bus.$emit('leftTalbe')
      this.$bus.$emit('pieDoughnut', {
        all: true,
        id: null
      })
      this.$bus.$emit('levelEvent', {
        all: true,
        id: null
      })
      this.$bus.$emit('barStacked')
      this.handleInclude(null, { isAll: true, bool: true })
      this.activeNum = item.windturbineId
      // doSomethings
      // eslint-disable-next-line no-useless-escape
      const itemFJ = find(this.$utils.map.FJDataArr, ['id', 'entitie\-' + item.windturbineId])
      const { lng, lat, alt } = this.$utils.map.cartesianTolngLatAlt(itemFJ.position._value)
      window.viewer.camera.flyTo({
        destination: window.Cesium.Cartesian3.fromDegrees(
          lng + 0.004937911320850594,
          lat + 0.005166767516649999,
          alt + 400 //218.99764706980164
        ),
        duration: 5,
        orientation: {
          heading: 3.874001322155119,
          pitch: -0.3976787903803052,
          roll: 6.283182116789324
        }
      })
    },
    handleInclude(e, all = { isAll: false, bool: true }) {
      this.$utils.map.getInclude(all, [], true)
      this.$utils.map.getInclude(all, [], true, this.$utils.map.FJLabel, false)
      this.$utils.map.getInclude(all, [], true, this.$utils.map.eventIcon)
      this.$cesuimData.popupList.length > 0 && this.$cesuimData.handleClearPopupList()
      this.setSessionItem('changeEntities', '0')
    },
    handleOpenChart(key) {
      if (this[key]) {
        this.$refs[key].classList.contains('openShow') &&
          this.$refs[key].classList.remove('openShow')
        this.$refs[key].classList.add('closeHide')
        setTimeout(() => (this[key] = !this[key]), 300)
      } else {
        this[key] = !this[key]
        this.$nextTick(() => {
          this.setSessionItem('clickChart', 'rightChart')
          this.$refs[key].classList.contains('closeHide') &&
            this.$refs[key].classList.remove('closeHide')
          this.$refs[key].classList.add('openShow')
        })
      }
    },
    handleSort() {
      this.devicesRealTime = this.devicesRealTime.reverse()
    },
    async getWindTurbine(stationId, startTime, endTime) {
      const { data: res } = await getWindTurbineIndexRankingStatistics(
        stationId,
        startTime,
        endTime
      )
      if (!res.data) return
      res.data[0]?.index === -1 ? this.handleTrends(res.data) : this.handleGauge(res.data)
    },
    handleGauge(data) {
      if (!data || data.length === 0) {
        return
      }
      this.gaugeSeretData = {
        healthValue: data.length ? data[0].index : '--',
        indexNum: data.length ? data[0].riseIndex : '--',
        currentComp: 'wind'
      }
      this.devicesRealTime = data.length ? data[0].childNode : []
    },
    handleTrends(data) {
      const sendData = []
      const legend = []
      data[0].childNode.forEach((o, outIndex) => {
        const obj = {
          label: o.spaceName,
          value: outIndex
        }
        legend.push(obj)
        o.data.forEach(({ time, index }) => {
          const arrItem = []
          arrItem.push(0)
          arrItem.push(dayjs(time).format('YYYY-MM-DD'))
          arrItem.push(index)
          if (!isArray(sendData[outIndex])) {
            sendData[outIndex] = []
            sendData[outIndex].push(arrItem)
          } else {
            sendData[outIndex].push(arrItem)
          }
        })
      })
      this.heatMapLegend = legend
      this.heatMapSeriesData = sendData
    },
    handleCustomColor(num) {
      if (num === 100.0) {
        return '#36FA3C'
      } else if (num > 60.0) {
        return '#FFE604'
      } else if (num > 30.0) {
        return '#FF6B0E'
      } else if (num >= 0.0) {
        return '#FF0F0D'
      }
    },
    format(percentage) {
      return percentage === 100 ? '100' : `${percentage}`
    },
    handlePercentageData(item) {
      return item.data[0].index < 0 ? 0 : item.data[0].index
    },
    clickPartEvent(e) {
      if (!e || this.reClickPart === e?.name) {
        this.changeEventIcon([], {}, true)
        this.setSessionItem('clickEventPartType', '')
        this.reClickPart === e?.name &&
          this.$bus.$emit('levelEvent', {
            all: true,
            id: null
          })
        this.reClickPart = ''
        e &&
          this.handleIncludeIndex(
            {
              data: {
                ids: []
              }
            },
            { isAll: true, bool: true }
          )
        return (this.tableListData = this.tableListDataCopy)
      }
      this.setSessionItem(
        'clickEventPartType',
        JSON.stringify({
          type: e.data.key,
          value: e.data.key === 'eventType' ? eventTypeNum[e.data[e.data.key]] : e.data[e.data.key]
        })
      )
      this.getClassificationResults({ type: e.data.key, value: e.data[e.data.key] })
      const key = e.data.key
      this.tableListData = this.tableListDataCopy.filter(item => item[key] === e.data[key])
      this.handleIncludeIndex(this.handleTableEntity(this.tableListData))
      this.reClickPart = e.name
    },
    changeEventIcon(dataMap, obj, isAllClear) {
      if (isAllClear) {
        this.$utils.map.eventIcon.forEach(item => {
          item.billboard.image = item.description._value.copyImg
        })
      } else {
        let cesuimCanvas = new CesuimCanvas(window.Cesium, window.viewer)
        dataMap.forEach(item => {
          const entitieEvnet = filter(this.$utils.map.eventIcon, o => o.id === item.id)
          const params = [obj.value, item.eventLevel, item.eventCount, 1, obj.type]
          cesuimCanvas.handleEventIconImg(...params).then(res => {
            entitieEvnet[0].billboard.image = res
          })
        })
      }
    },
    handleTableEntity(data) {
      const e = {
        data: {
          ids: []
        }
      }
      e.data.ids = uniq(data.map(({ windTurbineId }) => windTurbineId))
      return e
    },
    handleIconBorder(e) {
      return {
        top: '-4px',
        border: `1px solid ${this[e] ? 'rgb(31, 255, 255)' : 'transparent'}`
      }
    },
    handleIncludeIndex(e, all = { isAll: false, bool: true }) {
      this.$utils.map.getInclude(
        all,
        // eslint-disable-next-line no-useless-escape
        e.data.ids.map(item => 'entitie\-' + item),
        true
      )
      this.$utils.map.getInclude(
        all,
        // eslint-disable-next-line no-useless-escape
        e.data.ids.map(item => 'label\-' + item),
        true,
        this.$utils.map.FJLabel,
        false
      )
      this.$utils.map.getInclude(
        all,
        // eslint-disable-next-line no-useless-escape
        e.data.ids.map(item => 'event\-' + item),
        true,
        this.$utils.map.eventIcon
      )
      this.$cesuimData.popupList.length > 0 && this.$cesuimData.handleClearPopupList()
    },
    async getClassificationResults(obj) {
      const { key, val } = syncEum[obj.type]
      const params = {
        type: key,
        value: val[obj.value]
      }
      const { data } = await getClassificationResults(params)
      this.setSessionItem('eventIndexByCondData', JSON.stringify(JSON.stringify(data.data)))
      this.handleEventIcon(data.data, { type: obj.type, value: val[obj.value] })
    },
    handleEventIcon(data, obj) {
      const changeEventEntities = data.map(({ eventCount, eventLevel, windturbineId }) => {
        return {
          eventCount,
          eventLevel,
          // eslint-disable-next-line no-useless-escape
          id: 'event\-' + windturbineId
        }
      })
      this.changeEventIcon(changeEventEntities, obj, false)
    },
    handleWindFarmRequset() {
      this.init()
      this.getTableList()
    }
  }
}
</script>

<style lang="less" scoped>
@import url('../style/minxs.less');
@card-right: 102%;
@card-width: 894px;

@font-face {
  font-family: 'DS-DIGIB-2';
  src: url('../../WindTurbine/font/DS-DIGIB-2.ttf') format('truetype');
}

.card-chart(@top, @height: 276.46px) {
  position: absolute;
  top: @top;
  right: @card-right;
  width: @card-width;
  height: @height;
  background: rgba(4, 17, 33, 0.5);
  backdrop-filter: blur(15px);
}

.icon-class() {
  position: absolute;
  right: 2%;
  top: 8px;
  font-size: 21px;
  color: #ffffff;
  cursor: pointer;
  z-index: 10;
}

.openShow {
  animation: left 0.3s ease-in both;
}
.closeHide {
  animation: right 0.3s ease-in both;
}
@keyframes left {
  from {
    width: 0;
  }
  to {
    width: @card-width;
  }
}
@keyframes right {
  from {
    width: @card-width;
  }
  to {
    width: 0;
  }
}

.active-box {
  background: #0094c5;
}

:deep(.el-card__header){
  border-bottom: none;
}
.content-right {
  .box-card {
    // overflow: hidden;
    --color: #ff321b;
    .ranking-item {
      display: flex;
      flex-direction: column;
      position: relative;
      align-items: center;
      // bottom: 28px;
      top: -12px;
      .title {
        font-size: 10px;
        font-weight: bold;
        line-height: 14.02px;
        letter-spacing: 0.35px;
        color: white;
        width: 92%;
        display: flex;
        flex-direction: row;
        justify-content: space-between;
        margin-bottom: 15px;
        margin-top: -11px;
        div:first-child {
          font-size: 12px;
          font-weight: bold;
          line-height: 14.02px;
          letter-spacing: 0.35px;
        }
        div:nth-child(2) {
          cursor: pointer;
        }
      }
      .bar-progres {
        width: 100%;
        display: flex;
        flex-direction: column;
        align-items: center;
        // max-height: 228px;
        .list-view::-webkit-scrollbar {
          width: 5px;
        }
        // .list-wrap {
        //   &::-webkit-scrollbar {
        //     background: transparent;
        //   }
        //   &::-webkit-scrollbar-thumb {
        //     background: transparent;
        //   }
        //   &::-webkit-scrollbar-track-piece {
        //     background: transparent;
        //   }
        // }
        div {
          display: flex;
          flex-direction: row;
          justify-content: space-between;
          align-content: center;
          align-items: center;
          width: 96%;
          height: 40px;
          cursor: pointer;
          padding: 4px 16px 4px 4px;
          :deep(.el-progress-bar__outer){
            background-color: #445461;
          }
          :deep(.el-progress__text){
            color: white;
            font-size: 22px !important;
            font-family: 'DS-DIGIB-2';
          }
          span {
            font-size: 12px;
            font-weight: bold;
            line-height: 17px;
            letter-spacing: 0em;
            color: white;
            width: 21%;
            word-break: break-all;
            text-overflow: ellipsis;
            display: -webkit-box;
            -webkit-box-orient: vertical;
            -webkit-line-clamp: 2;
            overflow: hidden;
            text-align: center;
          }
          // &:first-child {
          //   margin-bottom: 15px;
          // }
        }
        // div + div {
        //   margin-bottom: 15px;
        // }
      }
    }
    .charts-class {
      display: flex;
      flex-direction: row;
      // margin-top: 28px;
      // margin-top: 18px;
      align-items: center;
      justify-content: space-evenly;
    }
    .icon-1 {
      left: 9px;
    }
    .title {
      position: relative;
      top: 10px;
    }
  }
  .card-1 {
    .card-chart(5%, 335px);
    max-height: 335px;
    // overflow-y: auto;
    // text-align: center;
    .span-title {
      font-size: 15px;
      font-weight: bold;
      line-height: 14px;
      letter-spacing: 0.5px;
      color: #ffffff;
      position: relative;
      top: 10px;
      left: 15px;
    }
    i {
      .icon-class();
    }
    i + div {
      margin-top: 18px;
      margin-right: 3px;
    }
    .list-view::-webkit-scrollbar {
      width: 5px;
    }
  }
  .card-2 {
    .card-chart(39%, 608px);
    i {
      .icon-class();
    }
    .chart {
      height: 96%;
      position: relative;
      // top: 4%;
    }
  }
}
.top_title {
  &::before {
    float: right;
  }
  .titleChart {
    float: right;
  }
}
</style>
