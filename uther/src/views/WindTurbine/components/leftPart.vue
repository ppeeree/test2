<template>
  <div id="left">
    <!-- 左边标题 -->
    <div class="left_title">
      <div class="top_title">
        <span class="titleChart">{{ compNameEnum[currentComp] }}健康</span>
        <span class="title_buttom_line"></span>
      </div>
    </div>
    <!-- 一、机组健康 -->
    <div class="left_top">
      <gauge :gaugeSeretData="{ healthValue, indexNum, currentComp }" />
      <!-- 按钮 -->
      <iconChart
        :style="[handleIconBorder('healthHis')]"
        @click.native="showTypeTrend('healthHis')"
      ></iconChart>
      <!-- 按钮弹框：健康指数发展趋势 -->
      <div ref="healthHis" class="index_card" v-if="popupName == 'healthHis'">
        <div class="type_card_title">
          <span class="type_card_text">健康指数发展趋势</span>
          <i class="el-icon-close" @click="closeModel('healthHis')"></i>
        </div>
        <indexTrend
          v-if="this.timeData.length"
          :typeTrendShow="popupName == 'healthHis' ? 'block' : 'none'"
          :timeData="timeData"
          :numData="numData"
        ></indexTrend>
        <div class="nodata" v-else>
          <noData noteText="" />
        </div>
      </div>
    </div>
    <!-- 二、部件健康趋势 -->
    <div class="healthTrend">
      <div class="title_class">
        <span>部件健康趋势</span>
        <div class="smell_line" style="width: calc(100% - 120px)"></div>
      </div>
      <div class="healthTrend_content" v-if="healthStatuesComp.length">
        <!-- 热力图示意点 -->
        <div class="healthSpot">
          <span v-for="(item, index) in hotmapNote" :key="index">
            <span class="levelClass" :style="{ background: item.color }"></span>
            <span class="spot_text">{{ item.name }}</span>
          </span>
        </div>
        <!-- 热力图内容 -->
        <el-row style="width: 100%" v-for="(item, index) in healthStatuesComp" :key="item.entityId">
          <el-col span="4" class="hotMap_describe">
            <i :class="['icon', 'local', 'local-' + item.entityType]"></i>
            <span class="hotMap_text">{{ item.entityName }}</span>
          </el-col>
          <el-col span="19">
            <health-hot-map
              :hotMapdata="item.value"
              :isShowX="index === healthStatuesComp.length - 1"
              :xTime="hotMapTime"
            ></health-hot-map>
          </el-col>
        </el-row>
      </div>
      <div class="nodata" v-else>
        <noData noteText="" />
      </div>
    </div>
    <!-- 三、部件故障 -->
    <div class="comp_fault">
      <div class="title_class">
        <span>部件故障</span>
        <div class="smell_line"></div>
      </div>
      <div class="unit_fault">
        <div class="unit_failure_text" v-if="faultData.length">
          <div class="unit_blade" v-for="(item, index) in this.faultData" :key="index">
            <div class="unit_title">
              <i :class="['icon', 'local', 'local-' + entityPartEnum[item.compName]]"></i>
              <span class="unit_title_text">{{ item.compName }}</span>
            </div>
            <!-- 故障内容 -->
            <div class="unit_health" v-if="!item.faultList.length">
              <img class="unit_health_img" src="/img/WindTurbine/healthState.png" />
              <div class="unit_health_text">该部件健康</div>
            </div>
            <div
              v-else
              v-for="(i, index) in item.faultList"
              :key="index"
              style="cursor: pointer; margin: 7px"
              @click="showFaultCard(i)"
              :class="i.faultId === clickedfault.faultId ? 'activeFault' : null"
            >
              <el-row gutter="5" style="cursor: pointer">
                <el-col span="18">
                  <span class="text_name">{{ i.faultName }}</span>
                </el-col>
                <el-col span="6">
                  <span
                    :style="{
                      color: falutLevelColor[i.faultLevel],
                      fontSize: '12px'
                    }"
                  >
                    {{ classLevelEnum[i.faultLevel] }}
                  </span>
                </el-col>
              </el-row>
            </div>
          </div>
        </div>
        <div class="nodata" v-else>
          <noData noteText="" />
        </div>
      </div>
      <!-- 点击故障出现的DOM框 -->
      <div
        id="faultCard"
        class="fault_card"
        :style="{ display: controlFaultCard && popupName == 'fault' ? 'block' : 'none' }"
      >
        <div class="fault_title">
          <!-- 当前故障：历史故障数据 -->
          <span class="title_text">
            {{ clickedfault.faultName }}
            <b
              :style="{
                marginLeft: '10px',
                color: falutLevelColor[clickedfault.faultLevel]
              }"
            >
              {{ classLevelEnum[clickedfault.faultLevel] }}
            </b>
          </span>
          <i class="el-icon-close" @click="noneFaultShow"></i>
        </div>
        <faultCard :faultHisInfo="faultHisInfo" :currentFaultInfo="currentFaultInfo"></faultCard>
      </div>
      <!-- 部件故障旁边的按钮 -->
      <iconChart
        :style="[{ top: '9%' }, handleIconBorder('historyState')]"
        @click.native="showTypeTrend('historyState')"
      ></iconChart>
      <!-- 点击按钮：故障发展趋势 -->
      <div
        ref="historyState"
        class="unit_trend_card"
        :style="{ display: popupName == 'historyState' ? 'block' : 'none' }"
      >
        <!-- 标题 -->
        <div class="type_card_title">
          <span class="type_card_text">故障发展趋势</span>
          <i class="el-icon-close" @click="closeModel('historyState')"></i>
        </div>
        <!-- 内容 -->
        <div class="unit_text" style="width: 500px">
          <hot-card
            ref="historyState_trend"
            :showKey="popupName == 'historyState' ? 'block' : 'none'"
          ></hot-card>
        </div>
      </div>
    </div>
    <!-- 四、诊断报告 -->
    <div class="diagnostic_report">
      <!-- 标题 -->
      <div class="title_class">
        <span>诊断报告</span>
        <div class="smell_line"></div>
      </div>
      <!-- 内容 -->
      <report-table
        class="table_content"
        :reportData="nowReportData"
        :heightSize="currentComp !== 'turbine' ? 'calc(100% - 40px)' : '100%'"
      ></report-table>
    </div>
  </div>
</template>

<script>
import {
  numTrendApi,
  compHealthStateApi,
  // unitFailureApi,
  failureReportApi,
  compFaultStateApi,
  falutHistoryInfo
} from '@/api/WindTurbine/LeftPartAPI.js'
import reportTable from '../charts/reportTable.vue'
import hotCard from '../leftComp/faultTrend.vue'
import healthHotMap from '../hotMap/healthHotMap.vue'
import faultCard from '../leftComp/faultPart.vue'
// import healthIndex from '../charts/healthIndex.vue'
import indexTrend from '../charts/indexDevelopTrend.vue'
import dayjs from 'dayjs'

import {
  classLevelEnum,
  eventTypeEnum,
  levelColor,
  compIconImgList,
  entityPartEnum,
  falutLevelColor,
  compSimpleCode,
  compNameEnum
} from '@/util/constant'
import noData from '@/components/noData/index.vue'
import gauge from '@/views/screen/charts/gauge.vue'
import iconChart from '../../screen/charts/iconChart.vue'
const dicData = {
  turbine: 'windturbine',
  engine: 'NAC',
  tower: 'TWW',
  blade: 'ROT',
  YPB: 'YPB'
}
export default {
  components: {
    reportTable,
    hotCard,
    healthHotMap,
    faultCard,
    indexTrend,
    noData,
    gauge,
    iconChart
  },
  props: {
    currentComp: {
      type: String,
      default: 'turbine',
      require: true
    },
    entityId: {
      type: String,
      default: '',
      require: true
    },
    windTurbineId: {
      type: String,
      default: '',
      require: true
    }
  },
  data() {
    return {
      classLevelEnum,
      entityPartEnum,
      falutLevelColor,
      compSimpleCode,
      compNameEnum,
      timer: null,
      popupName: '',
      num: 0,
      healthValue: '',
      //机组健康按钮接口数组
      numData: [],
      timeData: [],
      nowReportData: [],
      //热力图数据
      healthStatuesComp: [],
      hotMapTime: [],
      hotmapNote: [
        { name: '正常', color: '#2ED133' },
        { name: '注意', color: '#FFE604' },
        { name: '警告', color: '#FF6B0E' },
        { name: '危险', color: '#FF0F0D' },
        { name: '无状态', color: '#4E6278' }
      ],

      faultData: [], //机组故障数据
      controlFaultCard: false,
      clickedfault: {},
      // omitSpotList: ['1', '2', '3', '4', '5'],
      faultHisInfo: [],
      currentFaultInfo: [],
      indexNum: 0,
      nowEnterPart: ''
    }
  },
  watch: {
    entityId: {
      handler() {
        this.controlFaultCard = false
        this.clickedfault = {}
        this.popupName = ''
        this.init()
      }
    },
    currentComp: {
      handler() {
        this.controlFaultCard = false
        this.popupName = ''
        this.clickedfault = {}
      }
    }
  },
  computed: {},
  created() {},
  mounted() {
    this.init()
    this.timer = setInterval(() => {
      this.init()
    }, 60 * 1000)
  },
  methods: {
    init() {
      // this.popupName = ''
      this.unitFailureData()
      this.failureReportData()
      this.compHealthState()
      this.getHealthValue()
    },
    // 收起侧边栏
    changeLeftSize() {
      this.$emit('changeLeftSize')
    },
    //故障报告
    failureReportData() {
      failureReportApi({ entityId: this.entityId }).then(res => {
        const data = res.data.data.data
        let result = []
        data.forEach(item => {
          let { time, diagEngineer, rotor, nacelle, tower } = item
          let obj = {
            reportTime: time,
            compList: [
              {
                code: 'ROT', // 风轮
                levelZh: rotor,
                color: levelColor[eventTypeEnum[rotor]]
              },
              {
                code: 'NAC', // 机舱
                levelZh: nacelle,
                color: levelColor[eventTypeEnum[nacelle]]
              },
              {
                code: 'TOW', // 塔筒
                levelZh: tower,
                color: levelColor[eventTypeEnum[tower]]
              }
            ],
            person: diagEngineer
          }
          result.push(obj)
        })
        this.nowReportData = result
        // this.nowReportData = []
      })
    },
    //机组故障
    unitFailureData() {
      let entityType = this.compSimpleCode[this.currentComp]
      /* compFaultStateApi({ entityId: this.entityId, entityType }).then(res => {
        this.faultData = res.data.data
      }) */
    },
    //新热力图趋势接口
    compHealthState() {
      let timeArr = []
      let endTime = dayjs().format('YYYY-MM-DD HH:mm:ss')
      let startTime = dayjs().subtract(3, 'months').format('YYYY-MM-DD HH:mm:ss')
      compHealthStateApi({
        entityId: this.windTurbineId,
        entityCode: dicData[this.currentComp],
        endTime,
        startTime
      }).then(res => {
        if (res.data.code == 200 && res.data.data.length) {
          let { data, entityStatus, statusTime } = res.data.data // 新接口res.data.data
          // console.log('热力图数据', data)
          let unitList = []

          let compStatusList = []
          data.forEach(item => {
            const { entityId, entityName, entityType, entityStatus } = item
            unitList.push({
              entityId,
              entityName,
              entityType: entityType == 'TWW' ? 'TOW' : entityType,
              value: entityStatus[0]
            })
            compStatusList.push({
              entityId,
              entityName,
              entityType,
              entityStatus: entityStatus[0].status[entityStatus[0].status.length - 1]
            })
            timeArr = entityStatus[0].time
          })
          let xTime = []
          timeArr.forEach(item => {
            //if (index % 15 === 0) {
            let time = dayjs(new Date(item)).format('MM-DD')
            xTime.push(time)
            /// }
          })
          this.hotMapTime = xTime
          this.healthStatuesComp = unitList

          // console.log('热力图数据', this.healthStatuesComp)
          this.$emit('getEntityStatus', entityStatus, statusTime, compStatusList)
        } else {
          this.hotMapTime = []
          this.healthStatuesComp = []
        }
      })
    },
    //指数趋势接口
    getHealthValue() {
      numTrendApi({ entityId: this.entityId }).then(res => {
        if (res.data.code == 200 && res.data.data.length) {
          const eventNum = res.data.data[0]
          const { annulus, healthIndex } = eventNum
          this.healthValue = healthIndex
          this.indexNum = annulus
          /* if (eventNum.length == 2) {
            this.healthValue = parseInt(eventNum[1].healthIndex)
            this.indexNum = this.healthValue - parseInt(eventNum[0].healthIndex)
          } else {
            this.healthValue = parseInt(eventNum[0].healthIndex)
            this.indexNum = this.healthValue
          } */
          // console.log(this.indexNum)
        } else {
          this.healthValue = '--'
          this.indexNum = '--'
        }
        // console.log(this.healthValue)
      })
    },
    showTypeTrend(type) {
      if (this.controlFaultCard) {
        this.clickedfault = {}
        this.controlFaultCard = false
      }
      if (this.popupName === type) {
        this.popupName = ''
      } else {
        this.popupName = type
        /*  this.$refs[type].classList.remove('closeCard')
        this.$refs[type].classList.add('openCard') */
        if (type == 'healthHis') {
          this.getHealthTrend()
        } else {
          this.$refs['historyState_trend'].init({
            entityId: this.entityId,
            entityType: this.compSimpleCode[this.currentComp]
          })
        }
      }
    },
    getHealthTrend() {
      let endTime = dayjs().format('YYYY-MM-DD HH:mm:ss')
      let startTime = dayjs().subtract(3, 'months').format('YYYY-MM-DD HH:mm:ss')
      numTrendApi({ entityId: this.entityId, startTime, endTime }).then(res => {
        if (res.data.code == 200 && res.data.data.length) {
          const eventNum = res.data.data
          let dataList = [],
            timeList = []
          eventNum.forEach(item => {
            // timeList.push(dayjs(new Date(item.time)).format('YYYY-MM-DD'))
            timeList.push(item.time)
            dataList.push(item.healthIndex)
          })
          this.numData = dataList
          this.timeData = timeList
        } else {
          this.numData = []
          this.timeData = []
        }
      })
    },
    closeModel() {
      this.popupName = ''
      /*  this.$refs[type].classList.remove('openCard')
      this.$refs[type].classList.add('closeCard') */
      /* setTimeout(() => (this.popupName = ''), 300) */
    },
    getHotmapImg(img) {
      let string = compIconImgList[img]
      // console.log('获取图片icon', string)
      return require(`/public/img/WindTurbine/${string}.png`)
    },
    showFaultCard(obj) {
      if (this.clickedfault && this.clickedfault.faultId === obj.faultId) {
        this.clickedfault = {}
        this.controlFaultCard = false
        this.popupName = ''
      } else {
        this.popupName = 'fault'
        this.clickedfault = {
          ...obj
        }
        this.faultHisInfo = []
        this.currentFaultInfo = []
        const { faultId, faultTime, entityId } = obj
        falutHistoryInfo({ faultId, faultTime, entityId }).then(res => {
          if (res.data.code === 200 && res.data.data.length) {
            const { hisFaultData, nowFaultData } = res.data.data[0]
            this.faultHisInfo = hisFaultData
            this.currentFaultInfo = nowFaultData
            this.controlFaultCard = true
          }
        })
      }
    },
    noneFaultShow() {
      this.controlFaultCard = false
      this.clickedfault = {}
    },
    //鼠标移入
    enterBtn(type) {
      this.nowEnterPart = type
    },
    //鼠标溢出
    leaveBtn(type) {
      this.nowEnterPart = type
    },
    handleIconBorder(e) {
      return {
        border: `1px solid ${this.popupName == e ? 'rgb(31, 255, 255)' : 'transparent'}`
      }
    }
  },
  beforeDestroy() {
    clearInterval(this.timer)
    this.timer = null
  }
}
</script>

<style lang="scss" scoped>
@font-face {
  font-family: 'DS-DIGIB-2';
  src: url('../font/DS-DIGIB-2.ttf') format('truetype');
}
/* 左边框 */
#left {
  width: 460px;
  height: 100%;
  background: rgba(41, 64, 88, 0.6);
  z-index: 999;
  backdrop-filter: blur(7px);
  .nodata {
    width: 100%;
    height: calc(100% - 30px);
  }
  .left_title {
    width: 100%;
    padding: 25px 0px 0 15px;
    /*.left_arrow {
      height: 40px;
      text-align: center;
      position: absolute;
      top: 0;
      right: 10px;
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
        margin-right: 20px;
        &:hover {
          background: #33b98a;
          border: 1px solid #1c8c93;
        }
      }
    }*/
    // .top_title {
    overflow: hidden;
    line-height: 24px;
    &::before {
      float: left;
      width: 10px;
      height: 20px;
      background: #1fffff;
      content: '';
      margin-right: 8px;
      margin-top: 2px;
    }
    .titleChart {
      color: #1fffff;
      font-size: 18px;
      height: 30px;
      // width: 100%;
      margin-right: 10px;
      float: left;
    }
    .title_buttom_line {
      height: 4px;
      width: calc(100% - 100px);
      background-image: url('/img/WindTurbine/line/title_line1.png');
      background-size: 100%, 100%;
      background-repeat: no-repeat;
      box-sizing: border-box;
      display: inline-block;
    }
    // }
  }
}
//两个按钮样式
.click_btn_show {
  position: absolute;
  width: 41px;
  height: 23px;
  cursor: pointer;
  right: 10px;
  background: rgba(36, 106, 143, 1);
  border-radius: 5px;
  border: 1px solid;
}
.left_top {
  width: 100%;
  height: 15%;
  position: relative;
  /* 两边大标题样式 */

  .healthRanking {
    position: absolute;
    top: calc(50% - 30px);
    left: 11%;
    width: 88%;
    .health_ranking_num {
      position: absolute;
      top: 11%;
      left: 9%;
      img {
        height: 45px;
        position: absolute;
        top: 60%;
        left: -30%;
      }
    }
    .health_ranking_idnex {
      position: absolute;
      top: 38%;
      left: 59%;
    }
  }
  .index_card {
    position: absolute;
    top: 1%;
    left: 102%;
    width: 500px;
    display: inline-block;
    z-index: 999;
    background: rgba(41, 64, 88, 1);
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
    float: left;
    width: 5px;
    height: 15px;
    background: #1fffff;
    content: '';
    margin-right: 5px;
    margin-top: 3px;
  }
  span {
    display: block;
    float: left;
    margin-right: 10px;
  }
  .smell_line {
    float: left;
    height: 4px;
    width: calc(100% - 90px);
    background-image: url('/img/WindTurbine/line/title_line1.png');
    background-size: 100%, 100%;
    background-repeat: no-repeat;
    box-sizing: border-box;
    margin-top: 10px;
  }
  .img_cube {
    height: 1vh;
    width: 1vh;
    margin-right: 1vh;
  }
}
.smell_line {
  /*  height: 4px;
  width: 100%;
  background-image: url('/img/WindTurbine/line/title_line1.png');
  background-size: 100%, 100%;
  background-repeat: no-repeat;
  box-sizing: border-box;
  margin-top: 5px; */
}
.type_card_title {
  width: 90%;
  height: 37px;
  .type_card_text {
    color: white;
    margin-left: 20px;
    margin-top: 10px;
    line-height: 40px;
    font-size: 12px;
    font-weight: bold;
  }
  .el-icon-close {
    color: white;
    cursor: pointer;
    float: right;
    margin-right: -40px;
    margin-top: 10px;
  }
}
// 热力图
.healthTrend {
  height: 25%;
  .healthTrend_content {
    width: 100%;
    height: calc(100% - 30px);
    overflow-x: hidden;
    overflow-y: auto;
  }
  .healthTrend_content::-webkit-scrollbar {
    width: 5px;
  }
  .healthTrend_content::-webkit-scrollbar-track {
    background-color: #3e5369;
  }
  .healthTrend_content::-webkit-scrollbar-thumb {
    box-shadow: inset 0 0 6px #d2e5f1;
  }
  .hotMap_describe {
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
    color: #fff;
    line-height: 38px;
    text-align: center;
  }
  .hotMap_img {
    width: 25px;
    float: left;
    margin-top: 5px;
    margin-left: 13px;
  }
  .hotMap_text {
    color: #ffffff;
    font-size: 12px;
    margin-left: 5px;
  }
  .el-col {
    margin: 0px;
  }
}

// 机组故障
.comp_fault {
  width: 100%;
  height: 27%;
  position: relative;
  .unit_fault::-webkit-scrollbar {
    width: 6px;
  }
  .unit_fault::-webkit-scrollbar-track {
    background-color: #3e5369;
  }
  .unit_fault::-webkit-scrollbar-thumb {
    box-shadow: inset 0 0 6px #d2e5f1;
  }
  .unit_fault {
    width: 100%;
    height: calc(100% - 26px);
    padding: 20px 10px 0 10px;
    overflow-x: hidden;
    overflow-y: auto;
    .unit_failure_text {
      color: #ffffff;
      height: 100%;
      width: 100%;
      padding-left: 2px;
      display: flex;
      flex-direction: row;
      justify-content: flex-start;
      align-items: center;
      flex-wrap: wrap;
      .unit_blade {
        width: 128px;
        display: inline-block;
        background: rgba(140, 140, 140, 0.2);
        border-radius: 4px;
        height: 175px;
        margin: 2px 8px;
        overflow: auto;
        &::-webkit-scrollbar {
          display: none;
        }
        .unit_blade_text {
          margin: 7px;
        }
      }
      .unit_health {
        text-align: center;
        margin-top: 20px;
        .unit_health_img {
          height: 60px;
          width: 65px;
          text-align: center;
        }
        .unit_health_text {
          margin: 10px;
          text-align: center;
          font-size: 12px;
        }
      }
      .unit_title {
        margin: 4px;
        border-bottom: 1px solid #d8d8d8;
        .unit_title_text {
          font-size: 12px;
          line-height: 20px;
          margin-left: 5px;
          font-weight: bold;
        }
      }
      .text_name {
        font-size: 12px;
      }
    }
    .all_omit_spot {
      height: 5px;
      width: 120px;
      display: inline-block;
      margin-top: 8px;
      margin-left: calc(40%);
      .omit_spot_show {
        height: 5px;
        width: 5px;
        border-radius: 50%;
        background: #d8d8d8;
        display: inline-block;
        margin-right: 17px;
      }
      .omit_spot_none {
        height: 5px;
        width: 5px;
        border-radius: 50%;
        background: #818181;
        display: inline-block;
        margin-right: 17px;
      }
    }
  }
}
// 点击故障信息
.fault_card {
  position: absolute;
  left: calc(105%);
  top: calc(39%);
  display: inline-block;
  height: auto;
  width: 420px;
  border-radius: 20px;
  background: #21344b;
  color: white;
  z-index: 999;
  padding: 0 15px;
  .fault_title {
    margin-top: 10px;
    color: white;
    font-size: 14px;
    img {
      width: 16px;
      height: 15px;
      margin-right: 10px;
      margin-top: 5px;
    }
    .title_text {
      font-size: 12px;
      font-weight: bold;
    }
    .el-icon-close {
      width: 12px;
      height: 12px;
      // margin-top: -20px;
      cursor: pointer;
      float: right;
      margin-right: 0px;
    }
  }
}
/* 点击机组故障按钮 */
.unit_trend_card {
  width: 500px;
  position: absolute;
  top: 0;
  left: 102%;
  background-color: #294058;
  z-index: 999;
  border-radius: 5px;
}
// 诊断报告
.diagnostic_report {
  height: 25%;
  width: 100%;
  .table_content {
    padding: 10px 15px;
    overflow: hidden;
  }
}
// 热力图上的点
.spot {
  margin-left: 168px;
}
.levelClass {
  height: 8px;
  width: 8px;
  border-radius: 50%;
  display: inline-block;
  margin-left: 10px;
}
.spot_text {
  color: white;
  font-size: 12px;
  margin-left: 10px;
}
.healthSpot {
  margin-left: 33%;
  margin-top: 5px;
  margin-bottom: 5px;
}
//热力图下面文字
.hot_time {
  height: 13px;
  width: calc(83%);
  color: #fff;
  margin-left: calc(17%);
  font-size: 12px;
  .time_num {
    margin-right: calc(6%);
  }
}

.openCard {
  // animation: showCard 0.2s ease-in both;
}
.closeCard {
  // animation: closeCard 0.2s ease-in both;
}
@keyframes showCard {
  from {
    width: 0;
  }
  to {
    width: 500px;
  }
}
@keyframes closeCard {
  from {
    width: 500px;
  }
  to {
    width: 0;
  }
}
.activeFault {
  background: rgba(255, 255, 255, 0.2);
  border-radius: 5px;
}
</style>
