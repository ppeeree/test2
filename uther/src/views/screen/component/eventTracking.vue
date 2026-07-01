<template>
  <div class="tody_class">
    <div class="font_clss">
      <div>
        <span style="font-size: 18px; color: #abe1ff"
          >近 <span style="font-size: 22px"> 24h</span>：
          <span class="num_st">{{ handleDefaultNull(eventNum.total || '--') }}&nbsp;</span>
          件</span
        >
      </div>
      <div class="next_st">
        环比：
        <!-- <i
          class="el-icon-caret-top"
          style="color: #fff; margin-right: 6px; line-height: 22px"
          v-if="eventNum.changeNum > 0"
        ></i>
        <i
          class="el-icon-caret-bottom"
          style="color: #fff; margin-right: 6px; line-height: 22px"
          v-if="eventNum.changeNum < 0"
        ></i>
        <i
          class="el-icon-caret-right"
          style="color: #fff; margin-right: 6px; line-height: 22px"
          v-if="eventNum.changeNum === 0"
        ></i> -->
        <span
          :style="{
            color: '#fff',
            marginRight: '-8px'
          }"
          class="font-24 font-row-line"
          >{{ handleDefaultNull(handleChangeRate) }}
          <span class="font-17"> %， </span>
        </span>
        <span
          class="font-24"
          :style="{
            marginLeft: '3px',
            color: '#fff'
          }"
        >
          {{ handleDefaultNull(handleChangeNum) }}
        </span>
      </div>
    </div>
  </div>
</template>

<script>
import round from 'lodash/round'
export default {
  props: {
    eventNum: {
      type: Object,
      default: () => {}
    }
  },
  computed: {
    handleChangeNum() {
      const changeNum = this.eventNum?.changeNum
      return changeNum === 0
        ? 0
        : !Number(changeNum)
        ? '--'
        : changeNum > 0
        ? `+${changeNum}`
        : changeNum
    },
    handleChangeRate() {
      let changeRate
      // const changeRate = round(this.eventNum?.changeRate)
      if (
        (this.eventNum?.changeRate > -1 || this.eventNum?.changeRate < 1) &&
        this.eventNum?.changeRate
      ) {
        changeRate = parseFloat(this.eventNum?.changeRate).toFixed(1)
      } else {
        changeRate = round(this.eventNum?.changeRate)
      }
      return changeRate === 0
        ? 0
        : !Number(changeRate)
        ? '--'
        : changeRate > 0
        ? `+${changeRate}`
        : changeRate
    },
    handleHiderRate() {
      return this.handleDefaultNull(this.handleChangeRate) == '--'
    }
  },
  methods: {
    handleDefaultNull(defaultValue) {
      return [-1, -1.0, '-1.0', '-1'].includes(defaultValue) ? '--' : defaultValue
    }
  }
}
</script>

<style lang="less" scoped>
.tody_class {
  display: flex;
  flex-direction: row;
  justify-content: center;
  margin-top: -15px;
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
  color: #fff;
  font-weight: 500;
}
.next_st {
  color: #abe1ff;
  font-size: 18px;
  margin-top: 5px;
  // width: 178px;
  display: flex;
  flex-direction: row;
  justify-content: flex-end;
  // align-items: flex-end;
  // margin-left: 3px;
}
.font-24 {
  font-size: 24px;
  font-family: Arial, sans-serif;
}
.font-17 {
  font-size: 17px;
  font-family: Arial, sans-serif;
}
.font-row-line {
  display: flex;
  flex-direction: row;
  align-items: baseline;
}
</style>
