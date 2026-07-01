<template>
  <div class="left_mini">
    <div class="health_comp">
      <p class="left_block_title">报警状态</p>
      <div v-if="healthTrendData.length">
        <el-popover
          :visible-arrow="false"
          placement="right"
          width="500"
          trigger="click"
          popper-class="extendContent"
        >
          <div class="cardContent">
            <div class="title">
              报警状态趋势
              <div class="subtitle">
                <span v-for="(item, index) in hotmapNote" :key="index">
                  <span class="levelClass" :style="{ background: levelColorEnum[item] }"></span>
                  <span class="spot_text">{{ eventTypeEnum[item] }}</span>
                </span>
              </div>
            </div>
            <el-row style="width: 100%" v-for="(item, ii) in healthTrendData" :key="item.entityId">
              <el-col class="describe" style="marginbottom: 0px; width: 70px; text-align: center">
                <span style="color: #fff; line-height: 38px">{{ item.entityName }}</span>
              </el-col>
              <!--   新接口"item.entityStatus"  -->
              <el-col style="width: 400px">
                <health-hot-map
                  widthSize="400px"
                  :hotMapdata="item.entityStatus"
                  :isShowX="ii === healthTrendData.length - 1"
                  :xTime="hotMapTime"
                ></health-hot-map>
              </el-col>
            </el-row>
          </div>
          <template slot="reference">
            <ul class="imglist_ul">
              <li
                :class="{ active_li: selectedComp === item.entityName }"
                v-for="item in healthTrendData"
                :key="item.entityId"
                :title="item.entityName + ': ' + eventTypeEnum[item.status]"
              >
                <i
                  :class="[
                    'icon',
                    'local',
                    `local-${
                      !isNaN(parseFloat(item.entityType.slice(-1)))
                        ? item.entityType.slice(0, -1)
                        : item.entityType
                    }`
                  ]"
                  :style="{ color: levelColor[item.status], fontSize: '42px' }"
                ></i>
                <span
                  class="cornerMark"
                  :style="{ background: levelColor[item.status] }"
                  v-if="!isNaN(parseFloat(item.entityType.slice(-1)))"
                  >{{ item.entityType.slice(-1) }}</span
                >
              </li>
            </ul>
          </template>
        </el-popover>
      </div>
      <div v-else class="health_info">
        <img src="/img/noData.png" alt="无数据" title="无数据" />
        <p>暂无数据</p>
        <div class="healthMsg"></div>
      </div>
    </div>
  </div>
</template>
<script>
import dayjs from 'dayjs'
import {
  compHealthStateApi,
  falutHistoryInfo,
  failureReportApi
} from '@/api/WindTurbine/LeftPartAPI.js'
// import miniUnitFault from '../leftComp/miniUnitFault.vue'
// import faultCard from '../leftComp/faultPart.vue'
import { eventTypeEnum, levelColorEnum } from '@/util/constant'
// import reportTable from '../charts/reportTable.vue'
import healthHotMap from '../hotMap/healthHotMap.vue'
// import indexTrend from '../charts/indexDevelopTrend.vue'
import noData from '@/components/noData/index.vue'
// import faultImageVue from '../leftComp/compFault.vue'
import { levelColor } from '@/util/constant'

export default {
  components: {
    // miniUnitFault,
    // faultCard,
    // reportTable,
    healthHotMap,
    //  indexTrend,
    noData
    // faultImageVue
  },
  props: {
    entityId: {
      type: String,
      require: true
    },
    currentComp: {
      type: String,
      default: 'turbine',
      require: true
    },
    windTurbineId: {
      type: String,
      default: ''
    }
  },
  data() {
    return {
      fetchingData1: false, // 控制是否获取数据的标志位
      eventTypeEnum,
      levelColorEnum,
      reportData: [],
      levelColor: levelColor,
      hotmapNote: [],
      healthTrendData: [],
      hotMapTime: [],
      selectedComp: '',
      timer: null,
      compList: [],
      faultInfoList: [],
      clickedfault: []
    }
  },
  watch: {
    entityId: {
      handler(val) {
        this.healthTrendData = []
        this.init()
      }
    }
  },
  mounted() {
    this.init()
    this.fetchingData1 = true
    this.timer = setInterval(() => {
      if (this.fetchingData1) {
        this.init()
      }
    }, 60 * 1000)
  },

  beforeDestroy() {
    clearInterval(this.timer)
    this.fetchingData1 = false // 停止获取数据
    this.timer = null
  },
  methods: {
    setDetailFault(obj) {
      if (this.clickedfault.length && this.clickedfault.some(item => item.faultId == obj.faultId)) {
        this.clickedfault = this.clickedfault.filter(item => item.faultId != obj.faultId)
      } else {
        this.clickedfault.push(obj)
      }
      if (
        this.faultInfoList.length &&
        this.faultInfoList.some(item => item.faultId == obj.faultId)
      ) {
        return
      } else {
        const { faultId, faultTime, entityId } = obj
        falutHistoryInfo({ faultId, faultTime, entityId }).then(res => {
          if (res.data.code === 200 && res.data.data.length) {
            const { hisFaultData, nowFaultData } = res.data.data[0]
            this.faultInfoList.push({
              ...obj,
              currentFaultInfo: nowFaultData,
              faultHisInfo: hisFaultData
            })
          }
        })
      }
    },

    init() {
      let endTime = dayjs().format('YYYY-MM-DD HH:mm:ss')
      let startTime = dayjs().subtract(3, 'months').format('YYYY-MM-DD HH:mm:ss')
      compHealthStateApi({
        entityId: this.windTurbineId,
        entityCode: this.currentComp,
        endTime,
        startTime
      }).then(res => {
        if (res.data.data.length) {
          let { data } = res.data
          this.hotmapNote = [...new Set(data.flatMap(item => item.status || []))]
          this.healthTrendData = data
          let timeArr = data[0].entityStatus.time // 新接口 data[0].entityStatus.time
          let xTime = []
          timeArr.forEach(item => {
            let time = dayjs(new Date(item)).format('MM-DD')
            xTime.push(time)
          })
          this.hotMapTime = xTime
        } else {
          this.healthTrendData = []
          this.hotMapTime = []
        }
      })
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
        this.reportData = result
        // this.nowReportData = []
      })
    },
    compHealthState(e, obj) {},

    //改变颜色
    getNumColor(level) {
      if (level == 100) {
        // 正常
        return { background: '#2ED133' }
      } else if (level < 100 && level >= 60) {
        return { background: '#FFE604' } // 注意
      } else if (level < 60 && level >= 30) {
        return { background: '#FF6B0E' } //警告
      } else if (level == '') {
        return { background: '#ADAFB2' }
      } else if (level < 30) {
        return { background: '#FF0F0D' }
      }
    }
  }
}
</script>
<style scoped lang="scss">
@font-face {
  font-family: 'DS-DIGIB-2';
  src: url('../font/DS-DIGIB-2.ttf') format('truetype');
}
.cardContent {
  overflow: hidden;
  width: 100%;
  height: auto;
  .title {
    .subtitle {
      float: right;
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
    }
  }
}

.left_mini {
  position: relative;
  width: 88px;
  height: calc(100vh - 100px);
  margin-top: 10px;
  background: rgba(13, 52, 83, 0.6);
  backdrop-filter: blur(7px);
  padding: 20px 0;
  /** .bigSize_btn {
    height: 40px;
    text-align: center;
    span {
      display: inline-block;
      width: 40px;
      height: 25px;
      margin-top: 8px;
      border-radius: 4px;
      background: #255873;
      color: #1fffff;
      line-height: 25px;
      border: 1px solid #1c8c93;
      cursor: pointer;
      &:hover {
        background: #33b98a;
        border: 1px solid #1c8c93;
      }
    }
  }*/
  .cornerMark {
    // display: inline-block;
    position: absolute;
    left: 12px;
    bottom: 10px;
    font-size: 12px;
    border-radius: 50%;
    margin-top: 30px;
    width: 16px;
    height: 16px;
    text-align: center;
    line-height: 16px;
    color: #fff;
  }
  .report_block {
    margin-top: 50px;
    text-align: center;
    position: absolute;
    width: 100%;
    bottom: 30px;
    left: 0;
    cursor: pointer;
    i {
      font-size: 32px;
      color: #d1efff;
    }
    p {
      font-size: 14px;
      padding: 10px 0;
      color: #c1e7ff;
    }
  }
  .health_val {
    width: 100%;
    text-align: center;
    margin: 15px 0;
    margin-bottom: 100px;
    cursor: pointer;
    .health_val_num {
      position: relative;
      span {
        display: inline-block;
        width: 54px;
        height: 54px;
        // background: #0066ff;
        filter: blur(15px);
        text-align: center;
      }
      b {
        position: absolute;
        left: 0;
        display: block;
        width: 100%;
        text-align: center;
        top: 4px;
        font-size: 49px;
        color: #fff;
        font-weight: 500;
        font-family: 'DS-DIGIB-2';
      }
    }
  }
  .left_block_title {
    text-align: center;
    color: #c1e7ff;
    font-size: 14px;
    margin: 10px 0;
  }
  .imglist_ul {
    width: 100%;
    cursor: pointer;
    &:hover {
      background: #011120;
    }
    li {
      width: 66px;
      height: 66px;
      line-height: 66px;
      margin: 20px auto;
      background: transparent;
      text-align: center;
      border-radius: 5px;

      position: relative;
    }
    .active_li {
      background: #246398;
      border: 1px solid #25faec;
      border-radius: 11px;

      /*  background: url('../../../../public/img/WindTurbine/comp_bg.png');
      background-size: 80px 80px;
      background-position: 0 -5px;
      background-repeat: no-repeat; */
      /*   background: #246398;
      border: 1px solid #25faec;
      border-radius: 11px;
      position: relative;
      &::after {
        content: '';
        position: absolute;
        background: #25faec;
        filter: blur(10px);
        width: 40px;
        height: 40px;
        z-index: 1;
        left: 0;
        top: 0;
      } */
    }
  }
  .health_info {
    width: 100%;
    text-align: center;
    img {
      margin-top: 30px;
      width: 60px;
    }
    p {
      font-size: 12px;
      color: #909399;
      margin-bottom: 20px;
    }
    .healthMsg {
      width: 12px;
      margin: 10px auto;
      height: auto;
      font-size: 12px;
      color: #909399;
    }
  }
  .fault_type {
    .mini_fault_level {
      font-size: 10px;
      margin-left: 40px;
      float: right;
      margin-right: 20px;
    }
    li {
      padding: 2px 5px;
      margin-top: 3px;
      .falut_his {
        padding: 0 5px;
      }
    }
    .active_fault {
      background: rgba(255, 255, 255, 0.1) !important;
      border-radius: 5px;
    }
  }
  .fault_line {
    width: 90%;
    height: 3px;
    background: #38495d;
    margin-left: 5%;
    margin-top: 15px;
    margin-bottom: 10px;
  }
}
.popup_model {
  display: none;
  position: absolute;
  left: 95px;
  border-radius: 10px;
  background: #21344b;
  color: white;
  width: 400px;
  z-index: 10;
  .fault_title {
    font-size: 14px;
    padding: 10px 15px;
    background: #264c7b;
    border-top-right-radius: 10px;
    border-top-left-radius: 10px;
    height: 39px;
    .hotMap_img {
      width: 25px;
      float: left;
      margin-top: -2px;
      margin-left: -5px;
      margin-right: 10px;
    }
    .el-icon-close {
      float: right;
      margin-left: 40px;
      cursor: pointer;
    }
  }
  .popup_content {
    overflow-x: hidden;
    overflow-y: auto;
    height: 400px;
    padding: 0 5px 10px 5px;
    &::-webkit-scrollbar {
      width: 5px;
    }
    &::-webkit-scrollbar-track {
      background-color: #3e5369;
    }
    &::-webkit-scrollbar-thumb {
      box-shadow: inset 0 0 6px #d2e5f1;
    }
  }
  .fault_current {
    margin-left: 0px;
    margin-top: 10px;
    padding-top: 6px;
    color: white;
    font-size: 14px;
    border-top: 2px solid #38495d;
    .title_text {
      font-size: 12px;
    }
  }
}

.fault_comp {
  position: absolute;
  display: inline-block;
  height: 66px;
  width: 66px;
  border: 1px solid;
  top: 90%;
  left: 10px;
  background-image: url('/img/WindTurbine/faultCompIcon.png');
  background-position: center;
  background-repeat: no-repeat;
  cursor: pointer;
  border-radius: 11px;
  // &:hover {
  //   background-color: rgba(41, 64, 88, 0.6);
  // }
  // &:active {
  //   background-color: #246398;
  // }
}
</style>
