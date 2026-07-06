<template>
  <div class="event_main">
    <div
      class="event_main_title"
      :style="{
        background:
          newClickItem.degree == 'fourth'
            ? '#DC1034'
            : newClickItem.degree == 'third'
            ? '#E85E51'
            : newClickItem.degree == 'second'
            ? '#F5B270'
            : '#FFF287'
      }"
    >
      <div>
        <span style="margin-left: 20px">
          <span style="float: left; margin-top: 5px; margin-right: 8px; margin-left: -7px"
            ><img :src="getEventDetailTitleImg(newClickItem.type)"
          /></span>
          <span>
            {{ titleDegree[newClickItem.degree] }}{{ GeneratorEnum[newClickItem.type] }}事件
          </span>
        </span>
        <i @click="$emit('handleTitleClose', true)" class="el-icon-close" />
      </div>
    </div>
    <!-- 事件详情组件 -->
    <div class="event_detail">
      <div class="event_comp_title">事件详情</div>
      <div style="width: 97%">
        <eventDetail
          :data="eventDetailData"
          :clickEventItem="newClickItem"
          :isDeal="eventTrackData.length == 0 ? false : true"
        ></eventDetail>
      </div>
    </div>
    <!-- 事件处理组件 -->
    <div class="event_deal">
      <div class="event_comp_title">事件处理</div>
      <div style="width: 97%">
        <eventDeal
          v-bind="$attrs"
          :clickEventItem="newClickItem"
          :trackdData="eventTrackData"
          @getEventAllData="getEventAllData"
        ></eventDeal>
      </div>
    </div>
    <!-- 后续跟踪组件 -->
    <div class="event_track">
      <div class="event_comp_title">后续跟踪</div>
      <div style="width: 97%">
        <eventTrack :data="eventTrackData"> </eventTrack>
      </div>
    </div>
  </div>
</template>

<script>
import eventDetail from './eventDetail.vue'
import eventDeal from './eventDeal.vue'
import eventTrack from './eventTrack.vue'
import { getEventDetailApi } from '@/api/WindTurbine/eventAPI.js'
import { GeneratorEnum, eventStatusEnum } from '@/util/constant.js'
import { createEnum } from '@/util/exp'

export default {
  components: { eventDetail, eventDeal, eventTrack },
  props: {
    clickEventItem: {
      type: Object,
      require: true,
      default() {
        return {}
      }
    }
  },

  data() {
    return {
      eventDetailData: '', //存储事件详情数组
      eventTrackData: '', //存储事件追踪数组
      newClickItem: '',
      GeneratorEnum,
      eventStatusEnum
    }
  },
  provide() {
    return {
      getEventAllData: this.getEventAllData
    }
  },
  watch: {
    clickEventItem: {
      handler(val) {
        this.newClickItem = this.changeKey(val)
        // console.log('点击事件详情', this.newClickItem)
        if (this.newClickItem) {
          this.getEventAllData(this.newClickItem)
        }
      },
      deep: true,
      immediate: true
    }
  },
  computed: {
    titleDegree() {
      return createEnum({
        first: '一级',
        second: '二级',
        third: '三级',
        fourth: '四级'
      })
    },
    changeStatus() {
      return createEnum({
        0: 'notdone',
        1: 'done'
      })
    },
    changeType() {
      return createEnum({
        3: 'health',
        1: 'inspection',
        4: 'work'
      })
    },
    changeDegree() {
      return createEnum({
        1: 'first',
        2: 'second',
        3: 'third',
        4: 'fourth'
      })
    }
  },
  mounted() {},
  methods: {
    //改变键值
    changeKey(data) {
      let newData = {}
      if (data.eventLevel) {
        const {
          eventLevel,
          eventType,
          eventSites,
          eventReason,
          eventCreatTime,
          eventStatus,
          id,
          entityId,
          windTurbineId,
          eventSource,
          faultValue,
          severity,
          compType,
          entityType
        } = data
        let neweventLevel = this.changeDegree[eventLevel]
        let neweventStatus = this.changeStatus[eventStatus]
        let neweventType = this.changeType[eventType]
        newData = {
          degree: neweventLevel,
          entityId: entityId,
          entityName: eventSites,
          eventSource: eventSource,
          faultValue: faultValue,
          id: id,
          message: eventReason,
          severity: severity,
          handleStatus: neweventStatus,
          time: eventCreatTime,
          type: neweventType,
          windTurbineId: windTurbineId,
          compType: compType,
          entityType: entityType
        }
      } else {
        newData = data
      }
      return newData
    },
    //获取事件详情接口
    getEventAllData(eventItem) {
      getEventDetailApi({ ...eventItem }).then(res => {
        if (res.data.code === 200) {
          const data = res.data.data
          this.eventDetailData = data.eventDetail
          this.eventTrackData = data.eventTracking
        }
      })
    },
    getEventDetailTitleImg(type) {
      let reg = /^[0-9]+.?[0-9]*/gi
      reg.test(type) && (type = this.changeType[type])
      if (type) {
        return require(`/public/img/eventMethod/black${type}.png`)
      }
    }
  }
}
</script>

<style lang="less" scoped>
.event_main {
  background: rgba(4, 17, 33, 0.5);
  backdrop-filter: blur(8px);
  position: absolute;
  display: inline-block;
  width: 100%;
  // max-height: 750px;
  // overflow-y: scroll;
  // overflow-x: hidden;
  //标题
  .event_main_title {
    // background: #DC1034;
    height: 28px;
    width: 420px;
    color: rgba(0, 0, 0, 0.7);
    font-size: 12px;
    font-weight: bold;
    line-height: 28px;
    div {
      display: flex;
      flex-direction: row;
      justify-content: space-between;
      align-items: center;
      width: 97%;
      i {
        font-size: 18px;
        cursor: pointer;
        margin-top: -5px;
      }
    }
  }
  //组件标题样式
  .event_comp_title {
    width: 90%;
    height: 33px;
    color: white;
    font-size: 12px;
    font-weight: bold;
    line-height: 34px;
    margin-left: 11px;
  }
  //事件详情
  .event_detail {
    width: 100%;
    // height: 166px;
  }
  //事件处理
  .event_deal {
    width: 100%;
    // height: 166px;
  }
  //后续跟踪
  .event_track {
    width: 100%;
    // height: 150px;
  }
}
</style>
