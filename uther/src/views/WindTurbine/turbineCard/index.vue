<template>
  <div class="click_card">
    <!-- 标题 -->
    <div class="click_card_title">
      <i
        :class="['icon', 'local', 'local-' + this.compCode[data.pagecompCode]]"
        style="margin-right: 10px"
      ></i>
      <span class="title_name">{{ data.pagecompName }}</span>
      <!-- <span class="title_level">{{ eventTypeEnum[data.compHealthStatus] }}</span> -->
    </div>
    <div class="click_card_main" v-if="data.compList.length">
      <div
        class="card_comp_item"
        v-for="(item, index) in data.compList"
        :key="index"
        :style="{ color: levelColor[item.compState] }"
      >
        <i
          :class="[
            'icon',
            'local',
            `local-${
              !isNaN(parseFloat(item.compCode.slice(-1)))
                ? item.compCode.slice(0, -1)
                : item.compCode
            }`
          ]"
        ></i>
        <span
          class="cornerMark"
          :style="{ background: levelColor[item.compState] }"
          v-if="!isNaN(parseFloat(item.compCode.slice(-1)))"
          >{{ item.compCode.slice(-1) }}</span
        >
        {{ item.compName }}
      </div>
      <!-- <div v-for="(item, index) in data.faultCompList" :key="index">
        <eigenvalue
          :title="item.faultCompName"
          :data="item.faultNameList"
          :level="item.faultCompStatus"
        ></eigenvalue>
      </div> -->
    </div>
    <noData v-else style="height: 106px"></noData>
  </div>
</template>

<script>
import eigenvalue from './eigenvalue.vue'
import { eventTypeEnum, levelColorEnum, entityPartEnum } from '@/util/constant.js'
import { levelColor } from '@/util/constant'
import noData from '@/components/noData/index.vue'

export default {
  props: {
    data: {
      type: Object,
      default: () => {}
    }
  },
  components: {
    eigenvalue,
    noData
  },
  data() {
    return {
      levelColor: levelColor,
      eventTypeEnum,
      levelColorEnum,
      entityPartEnum,
      compCode: { windturbine: 'turbine', NAC: 'NAC', TWW: 'TOW', ROT: 'ROT' }
    }
  },
  mounted() {}
}
</script>

<style lang="less" scoped>
.click_card {
  width: 349px;
  height: 150px;
  border: 1px solid #939393;
  border-radius: 10px;
  background-color: rgba(0, 0, 0, 0.2);
  backdrop-filter: blur(10px);
  .click_card_title {
    background-color: #2a65ae;
    width: 100%;
    height: 40px;
    line-height: 40px;
    border-radius: 10px 10px 0px 0px;
    font-size: 16px;
    color: white;
    display: inline-block;
    font-weight: bold;
    padding-left: 5px;
    .title_level {
      float: right;
      margin-right: 20px;
      line-height: 45px;
    }
  }
  .click_card_main {
    max-height: 260px;
    width: 100%;
    overflow-y: auto;
    overflow-x: hidden;
    .card_comp_item {
      margin: 8px 10px;
      font-size: 14px;
      font-weight: bold;
      .cornerMark {
        width: 10px;
        height: 10px;
        line-height: 10px;
        color: #fff;
        border-radius: 50%;
        position: relative;
        left: -22px;
        bottom: -3px;
        font-size: 9px;
        /* margin-top: 30px; */
        text-align: center;
        display: inline-block;
      }
    }
  }
}
</style>
