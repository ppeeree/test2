<template>
  <div class="top-class" ref="UI">
    <el-card class="box-card" @mouseenter="handleEnter" @mouseleave="handleLeave">
      <template #header>
        <div class="clearfix">
        <el-image
          :src="requireImgType(row.eventType)"
          fit="scale-down"
          class="fex-image"
        ></el-image>
        <span>{{ GeneratorEnum[row.eventType] }}事件</span>
        <span>_{{ row.eventSites }}_</span>
        <span>【{{ '海上风场' }}】</span>
        </div>
      </template>
      <el-scrollbar>
        <div class="content-body">
          <span class="title-event">报警事件</span>
          <div v-for="item in event.warningEvent" :key="item.id" class="fel-content">
            <span>{{ item.eventReasonType }}</span>
            <span :style="{ color: colorEventLevel(item.eventLevel) }">
              {{ eventLevelEnum[item.eventLevel] }}
            </span>
            <span>发生时间：{{ item.eventCreatTime }}</span>
          </div>
          <span class="title-event">机组事件近期履历</span>
          <div v-for="item in event.recentlyEvent" :key="item.id" class="fel-content">
            <span>{{ item.eventReasonType }}</span>
            <span :style="{ color: colorEventLevel(item.eventLevel) }">
              {{ eventLevelEnum[item.eventLevel] }}
            </span>
            <span>发生时间：{{ item.eventCreatTime }}</span>
          </div>
        </div>
      </el-scrollbar>
    </el-card>
  </div>
</template>

<script>
import { GeneratorEnum } from '@/util/constant.js'
import { eventLevelEnum } from '@/util/constant.js'
import { mapGetters } from 'vuex'

export default {
  props: {
    row: {
      type: Object,
      default: () => ({
        id: 9,
        eventLevel: 1,
        eventType: 1,
        eventBlong: 'HS0010期',
        eventSites: '机组-01#',
        eventReason: '叶片巡检无人机缺电',
        eventCreatTime: '2022-07-18 15:47:30',
        eventStatus: 1,
        eventUpdateTime: ''
      })
    },
    event: {
      type: Object,
      default: () => ({
        warningEvent: [
          {
            id: 1,
            eventReasonType: '叶片断裂',
            eventLevel: 1,
            eventCreatTime: '2022-07-18 15:47:30'
          },
          {
            id: 2,
            eventReasonType: '叶片断裂',
            eventLevel: 1,
            eventCreatTime: '2022-07-18 15:47:30'
          }
        ],
        recentlyEvent: [
          {
            id: 1,
            eventReasonType: '叶片断裂',
            eventLevel: 1,
            eventCreatTime: '2022-07-18 15:47:30'
          },
          {
            id: 2,
            eventReasonType: '叶片断裂',
            eventLevel: 1,
            eventCreatTime: '2022-07-18 15:47:30'
          },
          {
            id: 3,
            eventReasonType: '叶片断裂',
            eventLevel: 1,
            eventCreatTime: '2022-07-18 15:47:30'
          },
          {
            id: 4,
            eventReasonType: '叶片断裂',
            eventLevel: 1,
            eventCreatTime: '2022-07-18 15:47:30'
          },
          {
            id: 5,
            eventReasonType: '叶片断裂',
            eventLevel: 1,
            eventCreatTime: '2022-07-18 15:47:30'
          }
        ]
      })
    }
  },
  inject:['showPop'],
  data() {
    return {
      GeneratorEnum,
      eventLevelEnum,
      eventLevelArray: ['#DC1034', '#E85E51', '#F5B270', '#FFF287']
    }
  },
  watch: {
    row: {
      handler(val) {
        this.setUI(val.eventLevel)
      },
      deep: true
    }
  },
  computed: mapGetters(['getTimeOut']),
  methods: {
    requireImgType(id) {
      if (id == undefined) return
      return require(`../img/eventType/${id}.svg`)
    },
    setUI(id) {
      this.$refs.UI.style.setProperty('--color', this.colorEventLevel(id))
    },
    colorEventLevel(id) {
      return this.eventLevelArray[id - 1]
    },
    handleEnter() {
      clearTimeout(this.getTimeOut)
    },
    handleLeave() {
      this.showPop(false)
    }
  }
}
</script>

<style lang="less" scoped>
.top-class {
  --color: #dc1034;
  .box-card {
    width: 316px;
    height: 236px;
    overflow-y: auto;
    :deep(.el-card__header){
      background-color: var(--color);
      border-bottom: none;
      .clearfix {
        font-family: SourceHanSansCN-Regular;
        font-size: 0.9rem;
        font-weight: normal;
        line-height: 17px;
        letter-spacing: 0em;
        color: #000;
        display: flex;
        flex-direction: row;
        align-items: center;
        .fex-image {
          width: 20px;
          height: 20px;
          color: #000;
          margin-right: 5px;
        }
      }
    }
    .content-body {
      font-family: SourceHanSansCN-Regular;
      font-size: 12px;
      font-weight: normal;
      line-height: 17px;
      letter-spacing: 0em;
      color: white;
      // .title-event + div {
      //   margin-top: 10px;
      // }
      .title-event {
        font-family: SourceHanSansCN-Bold;
        font-size: 14px;
        font-weight: bold;
        line-height: 17px;
        letter-spacing: 0em;
        color: rgba(255, 255, 255, 0.85);
        position: relative;
        left: 22px;
        &::after {
          content: '';
          display: block;
          position: relative;
          width: 12px;
          height: 12px;
          border-radius: 50%;
          background-color: var(--color);
          right: 22px;
          bottom: 15px;
        }
      }
      .fel-content {
        display: flex;
        flex-direction: row;
        align-items: center;
        justify-content: space-between;
        margin-bottom: 10px;
      }
    }
  }
}

:deep(.el-card__body){
  padding: 16px;
}
:deep(.el-scrollbar__bar.is-horizontal){
  display: none;
}
:deep(.el-card){
  background-color: rgba(4, 17, 33, 0.5);
  backdrop-filter: blur(15px);
  border: none;
}
</style>
