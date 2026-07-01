<template>
  <!-- 设置右边信息框 -->
  <div id="right">
    <!-- 右侧标题 -->
    <div class="right_title">
      <span class="right_arrow" v-if="currentComp !== 'turbine'">
        <span @click="changeRightSize"
          ><i class="el-icon-d-arrow-right" title="收起侧边栏"></i
        ></span>
      </span>
      <div class="top_title">
        <span class="titleChart">全流程事件跟踪</span>
        <span class="title_buttom"></span>
      </div>
    </div>
    <!-- 事件汇总 -->
    <div style="height: calc(50% - 53px)">
      <!-- 今日已发生事件 -->
      <eventTracking :eventNum.sync="eventNum" />
      <!-- 饼状图 -->
      <div
        class="charts_class"
        :style="{ opacity: peiData1.length && peiData2.length ? '1' : '0.3' }"
      >
        <pie-ring
          unique="first"
          :peiData="peiData1"
          :color="['#FFF287', '#F5B270', '#E85E51', '#DC1034']"
          height="122px"
          width="210px"
          @clickPartEvent="clickPartEvent"
        ></pie-ring>
        <pie-ring
          unique="second"
          :peiData="peiData2"
          height="122px"
          width="210px"
          seriesName="事件类型"
          :color="['#F76560', '#7A72FF', '#E72EFF']"
          @clickPartEvent="clickPartEvent"
        ></pie-ring>
        <!-- <pie-ring
          unique="third"
          width="160px"
          height="122px"
          :peiData="peiData3"
          :color="['#00E5FF', '#0195FF', '#004CFF']"
          :legendGrid="{ top: '75%' }"
          :radiusGrid="{ radius: ['32%', '55%'] }"
          @clickPartEvent="clickPartEvent"
        ></pie-ring> -->
      </div>
      <!-- 列表 -->
      <event-table
        :tableListData="tableListData"
        style="padding-left: 15px"
        :currentComp="currentComp"
        fatherComp="turbine"
        v-on="$listeners"
        v-bind="$attrs"
      ></event-table>
    </div>

    <!-- 事件发展趋势 -->
    <div style="height: 48%">
      <div class="title_class">
        <div class="smell_line"></div>
        <span>发展趋势</span>
      </div>

      <!-- 发展趋势 -> 类型发展趋势 -->
      <div class="title_class_part">事件类型发展趋势</div>
      <div class="classTrend">
        <chart
          v-if="JSON.stringify(workTrendOption) !== '{}'"
          :chartData="workTrendOption"
          width="100%"
          height="100%"
        />
        <div class="nodata" v-else>
          <noData noteText="机组暂无事件" firstText="" />
        </div>
      </div>
      <!-- 发展趋势 -> 等级发展趋势  -->
      <div class="title_class_part">事件等级发展趋势</div>
      <div class="kindTrend">
        <chart
          v-if="JSON.stringify(areaChartOption) !== '{}'"
          :chartData="areaChartOption"
          width="100%"
          height="100%"
        />
        <div class="nodata" v-else>
          <noData noteText="机组暂无事件" firstText="" />
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import * as echarts from 'echarts'
import chart from '@/components/charts/baseChart'
import { getEventTrendApi } from '@/api/WindTurbine/RightPartAPI.js'
import { HandleEventTrackingInfoData, FetchData } from '../../screen/mixins/handleTableData'
import dayjs from 'dayjs'
// import PieRing from '../charts/pie-ring.vue'
import PieRing from '@/views/screen/base/pie-ring.vue'
import eventTable from '@/views/screen/table/leftTable.vue'
import { compSimpleCode } from '@/util/constant'
import cloneDeep from 'lodash/cloneDeep'
import noData from '@/components/noData/index.vue'
import eventTracking from '@/views/screen/component/eventTracking.vue'

export default {
  components: {
    PieRing,
    eventTable,
    chart,
    noData,
    eventTracking
  },
  props: {
    currentComp: {
      type: String,
      require: true,
      default: 'turbine'
    },
    entityId: {
      type: String,
      default: '',
      require: true
    }
  },
  data() {
    return {
      compSimpleCode,
      timer: null,
      //工作数组
      workTrendData: {},
      workTrendOption: {},
      areaData: {},
      areaChartOption: {},
      //饼状图
      peiData1: [],
      peiData2: [],
      peiData3: [],
      //表格数据
      tableListData: [],
      tableListDataCopy: [],
      //今日发生时间数据
      eventNum: {
        total: '',
        changeNum: '',
        changeRate: ''
      }
    }
  },
  watch: {
    entityId: {
      handler() {
        this.setSessionItem('clickEventPartType', '{}')
        this.init()
      }
    }
  },
  created() {},
  mounted() {
    this.setSessionItem('clickEventPartType', '{}')
    this.init()
    this.timer = setInterval(() => {
      this.init()
    }, 60 * 1000)
  },
  methods: {
    init() {
      let type = this.compSimpleCode[this.currentComp]
      // this.getTrendData(type)
      // this.getStatisticData(type)
    },
    changeRightSize() {
      this.$emit('changeRightSize')
    },
    //圆环的点击事件
    clickPartEvent(e) {
      if (!e) {
        this.reClickPart = ''
        return (this.tableListData = this.tableListDataCopy)
      }
      if (this.reClickPart === e.name) {
        this.reClickPart = ''
        this.$bus.$emit('levelEvent', {
          all: true,
          id: null
        })
        // this.changeEventIcon('eventType', 1)
        this.setSessionItem('clickEventPartType', '{}')
        return (this.tableListData = this.tableListDataCopy)
      }
      /*  JSON.stringify({
        type: e.data.key,
        value: e.data[e.data.key]
      }) */
      this.setSessionItem(
        'clickEventPartType',
        JSON.stringify({
          type: e.data.key,
          value: e.data[e.data.key],
          index: e.dataIndex
        })
      )
      // this.changeEventIcon(e.data.key, e.data[e.data.key])
      const key = e.data.key
      this.tableListData = this.tableListDataCopy.filter(item => item[key] === e.data[key])
      this.reClickPart = e.name
    },
    //风场页的接口数据
    async getStatisticData(entityType) {
      let fetchData = new FetchData(this.entityId, entityType)
      fetchData.getFetchData.then(res => {
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
        const data = JSON.parse(sessionStorage.getItem('clickEventPartType'))
        if (data && data.type) {
          this.handlePieStatus(data)
          this.tableListData = trackingInfoData.tableData.filter(
            item => item[data.type] === data.value
          )
        } else {
          this.tableListData = trackingInfoData.tableData
        }
        this.tableListDataCopy = cloneDeep(trackingInfoData.tableData)
      })
    },
    handlePieStatus(hightLightData) {
      this.$children.forEach(el => {
        // eslint-disable-next-line no-useless-escape
        if (el?.chartId && /^[pie\-]/.test(el.chartId)) {
          if (
            hightLightData &&
            hightLightData.type &&
            el.peiData.length &&
            hightLightData.type == el.peiData[0].key
          ) {
            el.chart.dispatchAction({
              type: 'highlight',
              seriesIndex: 0,
              dataIndex: hightLightData.index
            })
          }
        }
      })
    },
    // 统计数据
    // getStatisticData(entityType) {
    //   // let entityType = this.compName[this.currentComp]
    //   getEntityEventStatisticApi({ id: this.entityId, type: entityType }).then(res => {
    //     if (res.data.code === 200) {
    //       const { eventInfo, eventNum, levelData, statusData, typeData } = res.data.data
    //       //新接口的数据
    //       this.tableListData = eventInfo
    //       this.eventNum = eventNum
    //       this.peiData1 = Array.from(levelData, item => ({
    //         name: item[0],
    //         value: item[1],
    //         icon: 'circle'
    //       }))
    //       this.peiData2 = Array.from(typeData, item => ({
    //         name: item[0],
    //         value: item[1],
    //         icon: 'circle'
    //       }))
    //       this.peiData3 = Array.from(statusData, item => ({
    //         name: item[0],
    //         value: item[1],
    //         icon: 'circle'
    //       }))
    //     }
    //   })
    // },
    // 趋势数据
    getTrendData(entityType) {
      let endTime = dayjs().format('YYYY-MM-DD HH:mm:ss')
      let startTime = dayjs().subtract(3, 'months').format('YYYY-MM-DD HH:mm:ss')
      getEventTrendApi({ id: this.entityId, endTime, startTime, type: entityType }).then(res => {
        if (res.data.code === 200 && res.data.data.data.length) {
          // console.log('趋势数据', res.data.data)
          let arr = res.data.data.data
          let firstCountList = [],
            secondCountList = [],
            thirdCountList = [],
            fourthCountList = [],
            workCountList = [],
            healthCountList = [],
            securityCountList = [],
            inspectionCountList = [],
            timeList = []
          arr.forEach(item => {
            const {
              firstCount,
              secondCount,
              thirdCount,
              fourthCount,
              workCount,
              healthCount,
              securityCount,
              inspectionCount,
              time
            } = item
            let sum = firstCount + secondCount + thirdCount + fourthCount
            firstCountList.push(Math.floor((firstCount / sum) * 1000) / 1000)
            secondCountList.push(Math.floor((secondCount / sum) * 1000) / 1000)
            thirdCountList.push(Math.floor((thirdCount / sum) * 1000) / 1000)
            fourthCountList.push(Math.floor((fourthCount / sum) * 1000) / 1000)
            healthCountList.push(healthCount)
            inspectionCountList.push(inspectionCount)
            workCountList.push(workCount)
            // if (healthCount == 0) {
            //   healthCountList.push('-')
            // } else {
            //   healthCountList.push(healthCount)
            // }
            // if (workCount == 0) {
            //   workCountList.push('-')
            // } else {
            //   workCountList.push(workCount)
            // }
            // if (inspectionCount == 0) {
            //   inspectionCountList.push('-')
            // } else {
            //   inspectionCountList.push(inspectionCount)
            // }
            securityCountList.push(securityCount)

            timeList.push(dayjs(new Date(time)).format('MM-DD'))
          })
          this.workTrendData = {
            seriesData: [
              {
                name: '健康事件',
                value: healthCountList
              },
              /*    {
                name: '安全事件',
                value: securityCountList
              }, */
              {
                name: '巡检事件',
                value: inspectionCountList
              },
              {
                name: '工作事件',
                value: workCountList
              }
            ],
            timeList
          }
          this.areaData = {
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
          this.getClassTrend()
          this.getKindTrend()
        } else {
          this.workTrendData = {}
          this.workTrendOption = {}
          this.areaData = {}
          this.areaChartOption = {}
        }
      })
    },
    //类型发展趋势--折线图
    getClassTrend() {
      this.workTrendOption = {
        tooltip: {
          trigger: 'axis'
        },
        title: {
          text: '事件类型发展趋势',
          textStyle: {
            color: '#FFFFFF',
            fontWeight: 'lighter',
            fontSize: 13,
            lineHeight: 15
          },
          top: 0,
          left: 15,
          show: false
        },
        legend: {
          data: ['工作事件', '巡检事件', '健康事件'],
          textStyle: {
            color: '#FFFFFF'
          },
          right: 10,
          top: 2
        },
        grid: {
          top: 25,
          left: '5%',
          // right: '5%',
          bottom: '7%',
          containLabel: true
        },
        xAxis: {
          type: 'category',
          data: this.workTrendData.timeList,
          splitLine: {
            show: false
          },
          axisLabel: {
            color: '#FFFFFF'
          }
        },
        yAxis: {
          type: 'value',
          splitNumber: 2,
          axisLabel: {
            color: '#FFFFFF'
          },
          //显示分割线
          splitLine: {
            show: true,
            lineStyle: {
              type: 'dashed'
            }
          },
          // 显示y轴线
          axisLine: {
            show: false,
            linestyle: {
              type: 'solid',
              width: 2,
              color: '#657180'
            }
          }
        },
        color: ['#7A72FF', '#F76560', '#E72EFF'],
        series: this.workTrendData.seriesData.map(item => {
          return {
            name: item.name,
            type: 'line',
            // stack: 'Total',
            data: item.value,
            symbol: 'none'
          }
        })
      }
    },
    // 级别发展趋势--面积图
    getKindTrend() {
      this.areaChartOption = {
        color: ['#FFF287', '#F5B270', '#E85E51', '#DC1034'],
        tooltip: {
          trigger: 'axis',
          formatter(params) {
            // console.log(params)
            let str = `${params[0].name}<br />`
            params.forEach(item => {
              str += `${item.marker} ${item.seriesName}: ${(item.value * 100).toFixed(1)}% <br/>`
            })
            return str
          }
          /* axisPointer: {
            type: 'cross',
            label: {
              backgroundColor: '#6a7985'
            }
          } */
        },
        title: {
          text: '事件等级发展趋势',
          textStyle: {
            color: '#FFFFFF',
            fontWeight: 'lighter',
            fontSize: 13,
            lineHeight: 15
          },
          top: 0,
          left: 15,
          show: false
        },
        grid: {
          top: 25,
          left: '3%',
          right: '4%',
          bottom: 10,
          containLabel: true
        },
        legend: {
          data: ['一级', '二级', '三级', '四级'],
          // 修改上面备注框的字体颜色
          textStyle: {
            color: '#FFFFFF',
            fontWeight: 'lighter',
            fontSize: 13,
            lineHeight: 15
          },
          right: 10,
          top: 2,
          icon: 'circle',
          itemWidth: 10,
          itemHeight: 10
        },
        xAxis: [
          {
            type: 'category',
            boundaryGap: false,
            //15个
            data: this.areaData.timeList,
            axisLabel: {
              color: '#FFFFFF'
            }
          }
        ],
        yAxis: [
          {
            axisLabel: {
              // 坐标轴刻度标签的相关设置。
              color: '#FFFFFF', // x轴字体颜色
              formatter(val) {
                return `${val * 100}%`
              }
            },
            /* max: 1, */
            type: 'value',
            splitNumber: 5,
            //显示分割线
            splitLine: {
              show: true,
              lineStyle: {
                type: 'dashed'
              }
            },
            show: true
          }
        ],
        series: [
          {
            name: '一级',
            type: 'line',
            stack: 'Total',
            smooth: true,
            lineStyle: {
              width: 0
            },
            showSymbol: false,
            areaStyle: {
              opacity: 5,
              color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [
                {
                  offset: 0,
                  color: '#FFF287'
                },
                {
                  offset: 1,
                  color: '#FFF287'
                }
              ])
            },
            emphasis: {
              focus: 'series'
            },
            data: this.areaData.seriesData.find(i => i.name === '一级').value
          },
          {
            name: '二级',
            type: 'line',
            stack: 'Total',
            smooth: true,
            lineStyle: {
              width: 0
            },
            showSymbol: false,
            areaStyle: {
              opacity: 5,
              color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [
                {
                  offset: 0,
                  color: '#F5B270'
                },
                {
                  offset: 1,
                  color: '#F5B270'
                }
              ])
            },
            emphasis: {
              focus: 'series'
            },
            data: this.areaData.seriesData.find(i => i.name === '二级').value
          },
          {
            name: '三级',
            type: 'line',
            stack: 'Total',
            smooth: true,
            lineStyle: {
              width: 0
            },
            showSymbol: false,
            areaStyle: {
              opacity: 5,
              color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [
                {
                  offset: 0,
                  color: '#E85E51'
                },
                {
                  offset: 1,
                  color: '#E85E51'
                }
              ])
            },
            emphasis: {
              focus: 'series'
            },
            data: this.areaData.seriesData.find(i => i.name === '三级').value
          },
          {
            name: '四级',
            type: 'line',
            stack: 'Total',
            smooth: true,
            lineStyle: {
              width: 0
            },
            showSymbol: false,
            areaStyle: {
              opacity: 5,
              color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [
                {
                  offset: 0,
                  color: '#DC1034'
                },
                {
                  offset: 1,
                  color: '#DC1034'
                }
              ])
            },
            emphasis: {
              focus: 'series'
            },
            data: this.areaData.seriesData.find(i => i.name === '四级').value
          }
        ]
      }
    }
  },
  beforeDestroy() {
    clearInterval(this.timer)
    this.timer = null
  }
}
</script>

<style lang="less" scoped>
/* 右边框 */
#right {
  // position: absolute;
  // right: 10px;
  // top: 7%;
  width: 460px;
  height: 100%;
  // height: calc(100.5vh - 100px);
  background: rgba(41, 64, 88, 0.6);
  backdrop-filter: blur(7px);
  z-index: 999;
  .right_title {
    width: 100%;
    padding: 25px 15px 0 0px;
    .right_arrow {
      height: 40px;
      text-align: center;
      position: absolute;
      top: 0;
      left: 10px;
      span {
        display: inline-block;
        width: 80px;
        height: 25px;
        margin-top: 8px;
        border-radius: 4px;
        background: #255873;
        color: #1fffff;
        line-height: 25px;
        border: 1px solid #1c8c93;
        cursor: pointer;
        margin-left: 20px;
        &:hover {
          background: #33b98a;
          border: 1px solid #1c8c93;
        }
      }
    }
    /* 两边标题样式 */
    .top_title {
      overflow: hidden;
      line-height: 24px;
      &::before {
        float: right;
        width: 10px;
        height: 20px;
        background: #1fffff;
        content: '';
        margin-top: 2px;
      }
      .titleChart {
        color: #1fffff;
        font-size: 18px;
        height: 30px;
        float: right;
        margin-right: 10px;
      }
      .title_buttom {
        height: 4px;
        width: calc(100% - 160px);
        background-image: url('/img/WindTurbine/line/title_line.png');
        display: inline-block;
        background-size: 100%, 100%;
        background-repeat: no-repeat;
        box-sizing: border-box;
      }
    }
  }
}

/* 设置小标题样式 */
.title_class {
  color: #1fffff;
  font-size: 14px;
  line-height: 22px;
  margin-left: 15px;
  overflow: hidden;
  margin-top: 5px;
  &::before {
    float: right;
    width: 5px;
    height: 15px;
    background: #1fffff;
    content: '';
    margin-right: 15px;
    margin-top: 3px;
  }
  span {
    display: block;
    float: left;
    margin-left: 10px;
  }
  .smell_line {
    float: left;
    height: 4px;
    width: calc(100% - 90px);
    background-image: url('/img/WindTurbine/line/title_line.png');
    background-size: 100%, 100%;
    background-repeat: no-repeat;
    box-sizing: border-box;
    margin-top: 10px;
  }
}
.title_class_part {
  width: 100%;
  height: 20px;
  line-height: 20px;
  color: #fff;
  font-size: 12px;
  padding-left: 15px;
  font-weight: bold;
  margin-top: 5px;
}
.tody_class {
  display: flex;
  flex-direction: row;
  justify-content: center;
  margin-top: 7px;
}
.font_clss {
  width: 470px;
  font-family: HarmonyOS_Sans_SC;
  font-size: 1.1rem;
  font-weight: normal;
  line-height: 22px;
  letter-spacing: 0px;
  color: #ffffff;
  display: flex;
  flex-direction: row;
  align-items: center;
  justify-content: space-evenly;
}
.num_st {
  font-size: 1.8rem;
  color: #abe1ff;
  font-weight: 500;
}
.next_st {
  color: var(--color);
  font-size: 0.9rem;
  margin-top: 5px;
  width: 150px;
  display: flex;
  flex-direction: row;
  justify-content: space-between;
  align-items: flex-end;
  // margin-left: 3px;
}
.charts_class {
  display: flex;
  flex-direction: row;
  // margin-top: 12px;
  margin-left: 5px;
  align-items: center;
  justify-content: space-around;
}
.classTrend {
  width: 100%;
  height: calc(47% - 20px);
}
.kindTrend {
  width: 100%;
  height: calc(47% - 20px);
}
</style>
