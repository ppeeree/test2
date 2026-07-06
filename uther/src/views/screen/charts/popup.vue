<template>
  <div v-if="show" v-show="dataShow" ref="UI" class="info_boxL">
    <!-- <div class="close" @click="hide">X</div> -->
    <div class="area-title">
      <div class="left_title">
        <!--    <el-image
          src="/img/screen/pop/FJIcon.png"
          fit="cover"
        ></el-image> -->
        <i class="icon local local-turbine"></i>
        <span>{{ WindFarm.name }}_{{ handleTitleName }}</span>
      </div>
      <div class="right_title">
        <span>
          {{ eventTypeEnum[detail.deviceStatus || 'normal'] }}
        </span>
      </div>
    </div>

    <div v-if="handleType" class="body_1">
      <vircualList :items="detailList" :itemHeight="182" :shownumber="1" style="width: 100%">
        <template v-slot="{ item: outItem }">
          <div class="body_bg">
            <div class="body_title">
              <el-image
                :style="GeneratorStelyEnum[GeneratorIndexEnum[outItem.type]]"
                :src="`/img/screen/eventIcon/${GeneratorIndexEnum[outItem.type]}.png`"
                fit="cover"
              ></el-image>
              <span>{{ eventCode[outItem.type] }}</span>
            </div>
            <div class="body_content">
              <vircualList
                :items="outItem.data"
                :itemHeight="33"
                :shownumber="4"
                style="width: 100%"
              >
                <template v-slot="{ item: inItem }">
                  <div
                    class="body_item"
                    :style="handleBodyStyle(inItem.index)"
                    @click="handleBodyItem(inItem, inItem.index)"
                  >
                    <div class="body_item_left">
                      <el-image
                        style="width: 25px; height: 25px"
                        :src="`/img/screen/devicePart/${inItem.entityName}.png`"
                        fit="cover"
                      ></el-image>
                      <span>{{ inItem.eventMessage }}</span>
                    </div>
                    <div class="body_item_left_1">
                      <span v-if="eventPartType === 'eventLevel'" class="mr-40"
                        >{{ inItem.count }}次</span
                      >
                      <span
                        v-else
                        :style="{ color: eventLevelArray[levelImgEnum[item.level] - 1] }"
                        class="mr-40"
                      >
                        {{ eventLevelEnum[levelImgEnum[item.level]] }}
                      </span>
                      <el-image
                        v-if="eventPartType === 'eventLevel'"
                        :style="progressStyleEnum[inItem.status]"
                        :src="`/img/screen/progress/${progressEnum[inItem.status]}.png`"
                        fit="cover"
                      ></el-image>
                      <span v-else>{{ inItem.count }}次</span>
                    </div>
                  </div>
                </template>
              </vircualList>
            </div>
          </div>
        </template>
      </vircualList>
    </div>

    <div v-if="eventPartType === 'eventType'" class="body_5">
      <vircualList :items="handleEventTypeData" :itemHeight="40" :shownumber="5">
        <template v-slot="{ item }">
          <div
            v-if="item.entityName"
            class="body_5_content"
            :style="handleBodyStyle(item.index)"
            @click="handleBodyItem(item, item.index)"
          >
            <div class="body_item_left">
              <el-image
                style="width: 25px; height: 25px"
                :src="`/img/screen/devicePart/${item.entityName}.png`"
                fit="cover"
              ></el-image>
              <span>{{ item.eventMessage }}</span>
            </div>
            <div class="body_item_left_1">
              <span :style="{ color: eventLevelArray[levelImgEnum[item.level] - 1] }">
                {{ eventLevelEnum[levelImgEnum[item.level]] }}
              </span>
              <div>{{ item.count }}次</div>
              <el-image
                :style="progressStyleEnum[item.status]"
                :src="`/img/screen/progress/${progressEnum[item.status]}.png`"
                fit="cover"
              ></el-image>
            </div>
          </div>
        </template>
      </vircualList>
    </div>

    <!-- 默认 -->
    <div v-if="!eventPartType" class="body">
      <div v-if="infoShow" class="body_1">
        <div v-for="item in popIconT" :key="item.id" class="vessel">
          <el-tooltip
            effect="light"
            :enterable="false"
            :content="item.tooltipContent"
            placement="top"
          >
            <el-image
              style="width: 14px; height: 14px; margin-right: 6px"
              :src="item.icon"
              fit="cover"
            ></el-image>
          </el-tooltip>
          <span>
            {{ item.title }}
          </span>
        </div>
      </div>
      <div v-if="partsFailure && handlePartName.length > 0" class="body_2">
        <vircualList :items="handlePartName" :itemHeight="36" :shownumber="3">
          <template v-slot="{ item }">
            <div class="part">
              <div class="row">
                <el-image style="width: 25px; height: 25px" :src="item.src" fit="cover"></el-image>
                <span>{{ item.partName }}</span>
              </div>
              <div class="part_item" :style="{ width: partWidth }">
                <span :style="{ color: levelColorEnum[item.state] }">{{
                  eventTypeEnum[item.state]
                }}</span>
                <span>{{ item.time }}</span>
              </div>
            </div>
          </template>
        </vircualList>
      </div>
      <div v-if="partsCheck" class="body_3">
        <div class="row">
          <el-image
            style="width: 15.12px; height: 13.64px"
            src="/img/screen/pop/容器 60.png"
            fit="cover"
          ></el-image>
          <span>无人机例行巡检</span>
        </div>
        <span>
          {{ '2022-08-22 12:12' }}
        </span>
      </div>
      <div v-if="progressShow" class="body_4">
        <vircualList
          v-if="showProgressEvent"
          :items="progressEvent"
          :itemHeight="20"
          :shownumber="5"
        >
          <template v-slot="{ item }">
            <div
              class="body_4_item"
              :style="handleBodyStyle(item.index)"
              @click="handleBodyItem(item, item.index)"
            >
              <div class="item_content">
                <el-image
                  :style="GeneratorStelyEnum[GeneratorIndexEnum[item.type]]"
                  :src="`/img/screen/eventIcon/${GeneratorIndexEnum[item.type]}.png`"
                  fit="cover"
                ></el-image>
                <el-tooltip
                  effect="light"
                  :disabled="!(item.dec.length > 9)"
                  :content="item.dec"
                  placement="top"
                >
                  <span>{{ handleDecText(item.dec) }}</span>
                </el-tooltip>
              </div>
              <div class="item_content_1">
                <span>{{ item.time }}</span>
                <span>{{ item.eventCount }}次</span>
                <el-image
                  :style="progressStyleEnum[item.progress]"
                  :src="`/img/screen/progress/${progressEnum[item.progress]}.png`"
                  fit="cover"
                ></el-image>
              </div>
            </div>
          </template>
        </vircualList>
        <div v-else class="body_4_nodata">
          <el-image src="/img/screen/pop/nodata.png" fit="cover"></el-image>
          <span> 当前机组无事件信息 </span>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { upObjVal } from '@/util/exp'
import {
  iconStyleEnum,
  levelColor,
  eventTypeEnum,
  GeneratorStelyEnum,
  healthEventStelyEnum,
  entityPartEnum,
  levelColorEnum,
  GeneratorIndexEnum,
  progressStyleEnum,
  progressEnum,
  eventCode,
  eventLevelColorStyleEnum,
  eventIndexEnum,
  eventLevelEnum
} from '@/util/constant'
import isEqual from 'lodash/isEqual'
import filter from 'lodash/filter'
import omit from 'lodash/omit'
import cloneDeep from 'lodash/cloneDeep'
import { defineAsyncComponent } from 'vue'
export default {
  components: {
    vircualList: defineAsyncComponent(() => import('../component/vircualList.vue'))
  },
  props: {
    show: {
      type: Boolean,
      default: true
    }
  },
  data() {
    return {
      popIconT: [
        {
          id: 0,
          icon: '/img/screen/pop/brand.png',
          title: '风力发电机',
          tooltipContent: '品牌'
        },
        {
          id: 1,
          icon: '/img/screen/pop/model.png',
          title: '1500-P3',
          tooltipContent: '型号'
        },
        {
          id: 2,
          icon: '/img/screen/pop/time.png',
          title: '03 年 04 月',
          tooltipContent: '投运时间'
        }
      ],
      progressEvent: [],
      popTitle: '_#001',
      detail: {
        name: '',
        part: [],
        deviceStatus: '',
        popIconT: [],
        width: '50%'
      },
      eventTypeEnum,
      infoShow: false,
      partsFailure: true,
      partsCheck: false,
      progressShow: false,
      GeneratorStelyEnum,
      healthEventStelyEnum,
      levelImgEnum: {
        first: 1,
        second: 2,
        third: 3,
        fourth: 4
      },
      GeneratorIndexEnum,
      eventLevelColorStyleEnum,
      eventCode,
      progressEnum,
      progressStyleEnum,
      levelColorEnum,
      eventPartType: '',
      clickNumIndex: null,
      entitieId: '',
      detailList: [],
      dataShow: true,
      WindFarm: {},
      getSS: {},
      eventLevelArray: Object.freeze(['#FFF287', '#F5B270', '#E85E51', '#DC1034']),
      eventLevelEnum,
      partWidth: '50%'
    }
  },
  created() {
    this.handleSSEvent()
  },
  mounted() {
    this.handleFarm()
    window.addEventListener('setItem', () => {
      this.handleSSEvent()
      this.handleFarm()
    })
  },
  watch: {
    'detail.deviceStatus': {
      handler: function (val) {
        this.$refs.UI.style.setProperty('--titleBgColor', levelColor[val || 'normal'])
      }
    },
    'detail.structure': {
      handler: function (val) {
        const { infoShow, partsFailure, partsCheck, progressShow } = val
        this.infoShow = infoShow
        this.partsFailure = partsFailure
        this.partsCheck = partsCheck
        this.progressShow = progressShow
      }
    },
    'detail.popIconT': {
      handler: function (val) {
        upObjVal(this.popIconT, val)
      },
      deep: true
    },
    'detail.progressEvent': {
      handler: function (val) {
        if (val?.length === 0 || !val) return
        this.progressEvent = val.map((item, index) => {
          const { acqTime, dispose, name, type } = item
          return {
            type,
            dec: name,
            time: acqTime,
            progress: dispose,
            index,
            ...omit(item, ['acqTime', 'dispose', 'name', 'type'])
          }
        })
      },
      deep: true
    },
    'detail.entitieId': {
      handler: function (val) {
        this.entitieId = val
      }
    },
    'detail.width': {
      handler: function (val) {
        if (!val) return
        this.partWidth = val
      }
    },
    getSS: {
      handler: function (val) {
        JSON.stringify(val) !== '{}' && this.getClassificationResults()
      },
      immediate: true
    },
    eventPartType: {
      handler: function () {
        this.detailList = []
      }
    }
  },
  computed: {
    handleTitleName() {
      // eslint-disable-next-line no-useless-escape
      return this.detail.name.replace(/entitie[\-]+?/g, '')
    },
    handlePartName() {
      if (this.detail.part.length === 0) return []
      return this.detail.part.map(({ code, state, acqTime }, index) => {
        const name = entityPartEnum[code]
        return {
          id: index,
          src: `/img/screen/devicePart/${code}.png`,
          partName: name,
          style: iconStyleEnum[name],
          state,
          time: acqTime
        }
      })
    },
    handleType() {
      return ['eventLevel', 'eventStatus'].includes(this.eventPartType)
    },
    showProgressEvent() {
      return this.progressEvent.length > 0
    },
    handleEventTypeData() {
      return this.detailList[0]?.data || []
    }
  },
  methods: {
    handleBodyItem(item, index) {
      let eventInfoVo
      if (item?.eventInfoVo) {
        eventInfoVo = cloneDeep(item.eventInfoVo)
      } else {
        eventInfoVo = cloneDeep(item)
      }
      let obj = {
        type: eventInfoVo.type,
        degree: !/\d/.test(eventInfoVo.degree)
          ? eventInfoVo.degree
          : eventIndexEnum[eventInfoVo.degree],
        ...omit(eventInfoVo, ['type', 'degree'])
      }
      if (obj.acqTimeCopy) {
        obj.time = obj.acqTimeCopy
      }
      if (!obj?.message) {
        obj.message = obj.dec
      }
      if (!obj?.handleStatus) {
        obj.handleStatus = obj.progress
      }
      this.$bus.$emit('clickEventType', obj)
      this.clickNumIndex = index
    },
    handleBodyStyle(index) {
      return index === this.clickNumIndex
        ? {
            borderRadius: '2px',
            opacity: 1,
            background: 'rgba(138, 170, 234, 0.5)'
          }
        : {}
    },
    handleTypeSS() {
      window.addEventListener('setItem', () => {
        if (!sessionStorage.getItem('clickEventPartType')) return (this.eventPartType = '')
        const getSS = JSON.parse(sessionStorage.getItem('clickEventPartType'))
        this.eventPartType = getSS.type
        if (sessionStorage.getItem('tablePopupCell') === 'false') this.clickNumIndex = null
      })
    },
    handleSSEvent() {
      if (!sessionStorage.getItem('clickEventPartType')) return (this.eventPartType = '')
      this.getSS = JSON.parse(sessionStorage.getItem('clickEventPartType'))
      this.eventPartType = this.getSS.type
      if (sessionStorage.getItem('tablePopupCell') === 'false') this.clickNumIndex = null
    },
    getClassificationResults() {
      const data = JSON.parse(JSON.parse(sessionStorage.getItem('eventIndexByCondData')))
      if (!data) return
      this.clickNumIndex = null
      // eslint-disable-next-line no-useless-escape
      const item = filter(
        data,
        o => o.windturbineId === this.entitieId.replace(/entitie[\-]+?/g, '')
      )
      if (item.length === 0) {
        this.dataShow = false
        this.detailList = []
        return
      }
      this.dataShow = item[0].detailList.length === 0 ? false : true
      this.detailList = item[0].detailList.map((outItem, outIndex) => {
        return {
          ...omit(outItem, ['data']),
          index: outIndex,
          data: outItem.data.map((inItem, inIndex) => {
            return {
              ...inItem,
              index: inIndex
            }
          })
        }
      })
    },
    handleDecText(dec) {
      return dec.length > 9 ? dec.slice(0, 9) + '...' : dec
    },
    handleFarm() {
      let param = JSON.parse(sessionStorage.getItem('selectWindFarm'))
      param && !isEqual(this.WindFarm, param) && (this.WindFarm = param)
    }
  }
}
</script>

<style lang="less" scoped>
.bgitem() {
  border-radius: 2px;
  opacity: 1;
  background: rgba(138, 170, 234, 0.5);
}

.info_boxL {
  --titleBgColor: #2ed133;

  position: absolute;
  z-index: 10;
  //   display: none;
  width: 316px;
  //   height: 264.69px;
  // border: 1px solid #38e1ff;
  background: rgba(4, 17, 33, 0.5);
  border-radius: 6px;
  // box-shadow: 0 0 10px 2px #29baf1;
  // animation: slide 1.5s;
  .area-title {
    height: 28.34px;
    width: 100%;
    border-radius: 6px 6px 0px 0px;
    background: var(--titleBgColor);
    display: flex;
    flex-direction: row;
    font-size: 12px;
    font-weight: bold;
    line-height: 17px;
    letter-spacing: 0em;
    color: rgba(0, 0, 0, 0.6);
    align-items: center;
    justify-content: space-between;
    .left_title {
      display: flex;
      flex-direction: row;
      align-items: center;
      margin-left: 6px;
      span {
        margin-left: 8px;
      }
    }
    .right_title {
      margin-right: 6px;
    }
  }
  .close {
    position: absolute;
    color: #fff;
    top: 1px;
    right: 10px;
    text-shadow: 2px 2px 2px #022122;
    cursor: pointer;
    animation: fontColor 1s;
    margin-top: 5px;
  }
  .body {
    height: 89%;
    display: flex;
    flex-direction: column;
    justify-content: space-around;
    align-items: center;
    align-items: center;
    .body_1 {
      margin: 6px 0;
      width: 300px;
      height: 36px;
      border-radius: 3px;
      background: rgba(255, 255, 255, 0.1);
      display: flex;
      flex-direction: row;
      font-size: 12px;
      font-weight: bold;
      line-height: 17px;
      letter-spacing: 0em;
      color: #ffffff;
      justify-content: space-around;
      .vessel {
        display: flex;
        flex-direction: row;
        align-items: center;
      }
    }
    .body_2 {
      margin: 6px 0;
      width: 100%;
      display: flex;
      justify-content: center;
      :deep(.list-view){
        width: 300px;
        overflow-y: overlay;
        &::-webkit-scrollbar {
          width: 5px;
        }
        .list-view-content {
          div + div {
            margin-top: 5px;
          }
          div {
            .part {
              width: 300px;
              height: 36px;
              border-radius: 3px;
              background: rgba(255, 255, 255, 0.1);
              display: flex;
              flex-direction: row;
              justify-content: space-between;
              font-size: 12px;
              font-weight: bold;
              line-height: 17px;
              letter-spacing: 0em;
              color: #ffffff;
              align-items: center;
              .row {
                display: flex;
                flex-direction: row;
                align-items: center;
                margin-left: 10px;
                span {
                  margin-left: 6px;
                }
              }
              .part_item {
                width: 50%;
                display: flex;
                flex-direction: row;
                justify-content: space-between;
                align-items: center;
                margin-right: 14px;
              }
            }
          }
        }
      }
    }
    .body_3 {
      margin: 6px 0;
      width: 300px;
      height: 36px;
      border-radius: 3px;
      background: rgba(255, 255, 255, 0.1);
      display: flex;
      flex-direction: row;
      justify-content: space-around;
      font-size: 12px;
      font-weight: bold;
      line-height: 17px;
      letter-spacing: 0em;
      align-items: center;
      .row {
        display: flex;
        flex-direction: row;
        align-items: center;
        span {
          color: #f5b270;
          margin-left: 6px;
        }
      }
      span {
        color: white;
      }
    }
    .body_4 {
      margin: 6px 0;
      // height: 92.19px;
      display: flex;
      flex-direction: column;
      justify-content: space-around;
      width: 300px;
      border-radius: 3px;
      background: rgba(255, 255, 255, 0.1);
      align-items: center;

      font-size: 12px;
      font-weight: normal;
      line-height: 20px;
      letter-spacing: 0px;
      color: #ffffff;
      :deep(.list-view){
        width: 296px;
        position: relative;
        left: 3px;
        .list-view-content {
          div + div {
            margin-top: 7px;
          }
          div {
            .body_4_item {
              display: flex;
              flex-direction: row;
              justify-content: space-between;
              align-items: center;
              margin-right: 6px;
              padding: 3px 6px 3px 6px;
              cursor: pointer;
              .item_content {
                display: flex;
                flex-direction: row;
                span {
                  margin-left: 6px;
                }
              }
              .item_content_1 {
                margin-top: 0px;
                display: flex;
                flex-direction: row;
                align-items: center;
                span {
                  margin-right: 12px;
                  &:first-child {
                    margin-right: 12px;
                  }
                }
              }
              &:hover {
                .bgitem();
              }
            }
          }
        }
        &::-webkit-scrollbar {
          width: 4px;
        }
      }
      .body_4_nodata {
        height: 100px;
        width: 296px;
        display: flex;
        flex-direction: row;
        justify-content: center;
        align-items: center;
        span {
          font-size: 14px;
          font-weight: normal;
          line-height: 15px;
          letter-spacing: 0px;
          margin-left: 10px;
        }
      }
    }
  }
  .body_1 {
    height: 89%;
    display: flex;
    flex-direction: column;
    justify-content: space-around;
    align-items: center;
    margin-top: 7px;

    :deep(.list-view){
      &::-webkit-scrollbar {
        width: 5px;
      }
      .body_bg {
        display: flex;
        flex-direction: column;
        width: 100%;
        font-size: 12px;
        font-weight: normal;
        line-height: 20px;
        letter-spacing: 0px;
        color: #ffffff;
        .body_title {
          display: flex;
          flex-direction: row;
          align-items: center;
          margin-left: 15px;
          // margin-top: 7px;
          span {
            margin-left: 7px;
          }
        }
        .body_content {
          display: flex;
          flex-direction: column;
          border-radius: 3px;
          opacity: 1;
          background: rgba(255, 255, 255, 0.1);
          margin: 8px;
          align-items: center;
          max-height: 177px;
          overflow-y: auto;
          .list-view {
            &::-webkit-scrollbar {
              width: 5px;
            }
            .list-view-content {
              .list-view-item {
                display: flex;
                justify-content: center;
                &:hover {
                  .bgitem();
                }
                .body_item {
                  display: flex;
                  flex-direction: row;
                  height: 100%;
                  width: 100%;
                  justify-content: space-between;
                  align-items: center;
                  padding: 4px;
                  cursor: pointer;
                  .body_item_left {
                    display: flex;
                    flex-direction: row;
                    align-items: center;
                    span {
                      margin-left: 5px;
                    }
                  }
                  .body_item_left_1 {
                    display: flex;
                    flex-direction: row;
                    align-items: center;
                    .mr-40 {
                      margin-right: 40px;
                    }
                  }
                }
              }
            }
          }
        }
      }
    }
  }
  .body_5 {
    height: 205px;
    :deep(.list-view){
      &::-webkit-scrollbar {
        width: 4px;
      }
      .list-view-content {
        div {
          .body_5_content {
            display: flex;
            flex-direction: row;
            font-size: 12px;
            font-weight: normal;
            line-height: 20px;
            letter-spacing: 0px;
            color: #ffffff;
            align-items: center;
            justify-content: space-between;
            border-radius: 3px;
            opacity: 1;
            background: rgba(255, 255, 255, 0.1);
            padding: 7px;
            margin: 7px;
            cursor: pointer;
            .body_item_left {
              display: flex;
              flex-direction: row;
              align-items: center;
              span {
                margin-left: 5px;
              }
            }
            .body_item_left_1 {
              display: flex;
              flex-direction: row;
              align-items: center;
              width: 35%;
              justify-content: space-between;
            }
            &:hover {
              .bgitem();
            }
          }
        }
      }
    }
  }
  :deep(.el-progress-bar__outer){
    background-color: rgba(0, 0, 0, 0.3);
  }
}
</style>
