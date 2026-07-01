<template>
  <div class="main">
    <div v-if="currentFaultInfo && currentFaultInfo.length && currentFaultInfo[0].faultName">
      <div
        class="hisFault_content"
        style="width: 100%; height: auto; margin-bottom: 8px; padding: 5px"
        v-for="item in currentFaultInfo"
        :key="item.faultName"
      >
        <div class="part_one">
          <span class="diagnose_time">诊断时间：{{ item.diagTime }}</span>
        </div>
        <div style="width: 100%; height: 35px">
          <rowHot :chartData="getChartData(item)"></rowHot>
        </div>
        <div class="duration_div">
          <span class="time_quatum"> {{ item.startTime }} ~ {{ item.endTime }} </span>
          <span class="holdTime">持续时间：{{ item.duration }}</span>
        </div>
        <!--   <div class="advice">
          <div class="mainten_advice">维护建议：{{ item.maintAdvice }}</div>
        </div> -->
        <div class="total_number">
          <span class="number">
            <span class="notice" style="background: #ff9b9b"></span>
            <span>轻微：{{ item.mildCount }}次</span>
          </span>
          <span class="number">
            <span class="notice" style="background: #ff5d5d"></span>
            <span>中等：{{ item.moderateCount }}次</span>
          </span>
          <span class="number">
            <span class="notice" style="background: #ff0000"></span>
            <span>严重：{{ item.serverCount }}次</span>
          </span>
        </div>
      </div>
    </div>
    <div v-if="faultHisInfo && faultHisInfo.length && faultHisInfo[0].faultName">
      <div style="font-size: 14px; margin: 0 0 5px 5px">历史故障：</div>
      <div class="hisFault_content">
        <div class="unit_one" v-for="item in faultHisInfo" :key="item.faultName">
          <div class="duration_div">
            <span class="time_quatum"> 故障时间：{{ item.startTime }} ~ {{ item.endTime }} </span>
            <span class="holdTime">持续时间：{{ item.duration }}</span>
          </div>
          <div style="width: 100%; height: 35px">
            <rowHot :chartData="getChartData(item)"></rowHot>
          </div>
          <div class="total_number">
            <span class="number">
              <span class="notice" style="background: #ff9b9b"></span>
              <span>轻微：{{ item.mildCount }}次</span>
            </span>
            <span class="number">
              <span class="notice" style="background: #ff5d5d"></span>
              <span>中等：{{ item.moderateCount }}次</span>
            </span>
            <span class="number">
              <span class="notice" style="background: #ff0000"></span>
              <span>严重：{{ item.serverCount }}次</span>
            </span>
          </div>
          <div class="advice">
            <div class="mainten_advice">维护建议：{{ item.maintAdvice }}</div>
            <div class="mainten_advice">维护反馈：{{ item.maintFeedback }}</div>
          </div>
        </div>
      </div>
    </div>
    <div
      class="nodata"
      v-if="
        currentFaultInfo && currentFaultInfo.length == 0 && faultHisInfo && faultHisInfo.length == 0
      "
    >
      <noData noteText="" />
    </div>
  </div>
</template>

<script>
import rowHot from '../hotMap/rowHot.vue'
import noData from '@/components/noData/index.vue'

export default {
  components: { rowHot, noData },
  props: ['title', 'show', 'faultHisInfo', 'currentFaultInfo'],
  data() {
    return {
      /*   time: '2022-06-21',
      startTime: '2022-04-06',
      endTime: '2022-06-21',
      advice: '停机处理',
      result: '处理成功',
      holdTime: '2月04天',
      diagnoseStartTime: '2022-10-06',
      diagnoseEndTime: '2022-01-06',
      slightNum: '10',
      mediumNum: '20',
      graveNum: '8' */
    }
  },
  watch: {},
  created() {},
  mounted() {
    // console.log('获取展示', this.show)
  },
  methods: {
    getChartData(item) {
      return { faultLevel: item.faultLevelList, faultTime: item.faultTimeList }
    }
  }
}
</script>

<style lang="less" scoped>
.main {
  height: auto;
  // max-height: 360px;
  // overflow-y: auto;
  width: 100%;
  padding: 10px 0;
  // overflow-x: hidden;
  .nodata {
    width: 100%;
    height: 260px;
  }
  .hisFault_content {
    background: #38495d;
    border-radius: 8px;
    width: 100%;
    height: auto;
    padding: 5px;
  }
  .unit_one {
    margin-bottom: 8px;
    &:not(:first-child) {
      border-top: 1px solid rgba(216, 216, 216, 0.1);
    }
  }
  .part_one {
    font-size: 12px;
    line-height: 28px;
    width: 100%;
  }
  .duration_div {
    overflow: hidden;
  }
  .holdTime {
    font-size: 12px;
    line-height: 28px;
    float: right;
  }
  .time_quatum {
    display: block;
    width: 70%;
    text-align: left;
    font-size: 12px;
    float: left;
    line-height: 28px;
  }
  .total_number {
    width: 100%;
    .number {
      font-size: 12px;
      margin-left: 20px;
      .notice {
        width: 10px;
        height: 6px;
        border-radius: 4px;
        display: inline-block;
        margin-right: 5px;
      }
    }
  }
  .advice {
    width: 100%;
    .mainten_advice {
      font-size: 12px;
      line-height: 21px;
      width: 100%;
      margin-top: 8px;
    }
  }
}
</style>
