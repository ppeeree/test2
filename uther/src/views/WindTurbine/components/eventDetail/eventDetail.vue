<template>
  <div class="event_detail_card">
    <div v-for="(item, index) in data" :key="index">
      <div class="event_detail_item">
        发生部件：
        <span class="happen_comp" @click="goCompLink()">
          {{ newClickItem.entityName }}_{{ entityPartEnum[newClickItem.entityType] }}
        </span>
      </div>
      <div class="event_detail_item">
        事件信息：
        <i :class="['icon', 'local', 'local-' + item.happenComp]"></i>
        <span>{{ item.eventMessage }}</span>
      </div>
      <div class="event_detail_item">
        发生时间：<span style="margin-right: 18%">{{ item.hourAgo }}</span> 持续时间：<span>{{
          item.duration
        }}</span>
      </div>
      <el-row class="event_detail_hotMap">
        <el-col :span="4" style="line-height: 66px">发生频次：</el-col>
        <el-col :span="20">
          <!-- 示例 -->
          <span style="font-size: 10px; margin-left: 56%"
            >低 <span class="tooltip_line"></span>高</span
          >
          <!-- 热力图 -->
          <rowHot v-if="item.happenFre.length" :chartData="item.happenFre"></rowHot>
          <span class="none_hotMap_data" v-else>暂无数据</span>
          <!-- 热力图时间 -->
          <div class="detail_hotMap_time">
            <span style="float: left; margin-top: 5px">{{ item.startTime }}</span>
            <span style="float: right; margin-right: 5%; margin-top: 5px">{{ item.endTime }}</span>
          </div>
        </el-col>
      </el-row>
      <div class="event_detail_item">
        处理状态：
        <img
          style="margin-left: 5px; margin-right: 5px; position: relative; top: 3px"
          :src="setDoneStatus(isDeal)"
        />
        {{ isDeal ? '已处理' : '未处理' }}
      </div>
    </div>
  </div>
</template>

<script>
import rowHot from '../../hotMap/eventRowHot.vue'
import { entityPartEnum, eventStatusEnum, compSimpleCode } from '@/util/constant'
export default {
  components: {
    rowHot
  },
  props: {
    data: {
      type: Object,
      default: () => {}
    },
    clickEventItem: {
      type: Object,
      require: true,
      default() {
        return {}
      }
    },
    isDeal: {
      type: Boolean,
      default: false
    }
  },
  data() {
    return {
      entityPartEnum,
      eventStatusEnum,
      compSimpleCode,
      newClickItem: '',
      newHotMapData: [],
      setCompName: {
        机组: 'turbine',
        机舱: 'engine',
        塔筒: 'tower',
        风轮: 'blade',
        WINDTURBINE: 'turbine',
        NAC: 'engine',
        TOW: 'tower',
        ROT: 'blade'
      }
    }
  },
  watch: {
    clickEventItem: {
      handler(val) {
        this.newClickItem = val
      },
      immediate: true
    }
  },
  mounted() {},
  methods: {
    goCompLink() {
      this.$router.push({
        path: '/WindTurbine',
        query: {
          turbineId: this.newClickItem.windTurbineId,
          type: this.compSimpleCode[this.newClickItem.entityType]
        }
      })
    },
    setDoneStatus(value) {
      let index = value ? 'doneEvent' : 'undoneEvent'
      return require(`/public/img/WindTurbine/icon/${index}.png`)
    }
  }
}
</script>

<style lang="less" scoped>
.event_detail_card {
  background: rgba(255, 255, 255, 0.1);
  color: white;
  font-size: 12px;
  font-weight: 500;
  margin-left: 15px;
  .event_detail_item {
    margin-left: 10px;
    height: 30px;
    line-height: 30px;
    width: 100%;
    display: inline-block;
    .happen_comp {
      cursor: pointer;
      color: #3ebbff;
      &:hover {
        border-bottom: 1px solid #3ebbff;
      }
    }
  }
  .event_detail_hotMap {
    width: 100%;
    margin-left: 10px;
    height: 60px;
    margin-top: 10px;
    margin-bottom: 22px;
    .none_hotMap_data {
      background: rgba(0, 0, 0, 0.5);
      height: 18px;
      width: 275px;
      position: absolute;
      left: 22%;
      color: white;
      text-align: center;
    }
    .tooltip_line {
      height: 6px;
      width: 90px;
      display: inline-block;
      margin: 0 5px;
      background: linear-gradient(270deg, #ff1a6d 0%, #3546fe 100%);
    }
    .detail_hotMap_time {
      font-size: 11px;
      width: 100%;
    }
  }
}
</style>
