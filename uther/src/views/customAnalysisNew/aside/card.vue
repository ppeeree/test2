<template>
  <div class="cardContent">
    <div class="cardContent_top">
      <p>
        诊断时间:
        {{ activities.length ? activities[0].recordTime : requestParam.diagnostiCrecordTime }}
      </p>
      <!--   <div>变桨轴承</div> -->
      <p>
        诊断结论：{{
          activities.length ? activities[0].conclusion : requestParam.diagnosticonclusion
        }}_
        <span
          :style="{
            color:
              levelColor[
                activities.length ? activities[0].faultLevel : requestParam.diagnosticStatus
              ]
          }"
          >{{
            levelZh[activities.length ? activities[0].faultLevel : requestParam.diagnosticStatus]
          }}</span
        >
      </p>
    </div>
    <h4>
      诊断历史：
      <span :class="timeRange == 'three' ? 'activeSpan' : ''" @click.stop="changeTimeRange('three')"
        >近三个月</span
      >
      <span :class="timeRange == 'one' ? 'activeSpan' : ''" @click.stop="changeTimeRange('one')"
        >近一个月</span
      >
    </h4>
    <template v-if="activities.length">
      <el-timeline :reverse="reverse">
        <el-timeline-item
          v-for="(activity, index) in activities"
          :key="index"
          :timestamp="activity.recordTime"
          placement="top"
          :color="levelColor[activity.faultLevel]"
        >
          诊断结论：{{ activity.conclusion }}_<sapn
            :style="{ color: levelColor[activity.faultLevel] }"
            >{{ levelZh[activity.faultLevel] }}</sapn
          >
        </el-timeline-item>
      </el-timeline>
    </template>
    <div class="noData" v-else>此时间段内无诊断记录！</div>
  </div>
</template>
<script>
import { getManualDiagnosisList } from '@/api/analysis/index.js'
import dayjs from 'dayjs'
import { levelColor, eventTypeEnum } from '@/util/constant'
import { objectArraySort } from '@/util/utils.js'
export default {
  props: ['requestParam'],
  watch: {
    requestParam: {
      handler() {
        this.getManualDiagnosisListFunc()
      },
      deep: true
    },
    timeRange: {
      handler() {
        this.getManualDiagnosisListFunc()
      }
    }
  },
  data() {
    return {
      levelColor,
      timeRange: 'one',
      reverse: false,
      levelZh: eventTypeEnum,
      activities: []
    }
  },
  methods: {
    changeTimeRange(type) {
      if (this.timeRange == type) {
        return
      }
      this.timeRange = type
    },
    getManualDiagnosisListFunc() {
      let { windturbineIds, measlocId } = this.requestParam
      let param = {
        windturbineIds,
        measlocId,
        endTime: dayjs().format('YYYY-MM-DD HH:mm:ss'),
        startTime:
          this.timeRange == 'one'
            ? dayjs().subtract(1, 'months').format('YYYY-MM-DD HH:mm:ss')
            : dayjs().subtract(3, 'months').format('YYYY-MM-DD HH:mm:ss')
      }
      getManualDiagnosisList(param).then(res => {
        if (res.data.data.length) {
          let result = res.data.data.map(item => {
            return { ...item, recordTime: dayjs(item.recordTime).valueOf() }
          })
          // 返回值按照时间降序排列
          let orderDesc = objectArraySort(result, 'recordTime', 'desc')
          this.activities = Array.from(orderDesc, i => ({
            ...i,
            recordTime: dayjs(i.recordTime).format('YYYY-MM-DD HH:mm:ss')
          }))
        } else {
          this.activities = []
        }
      })
    }
  }
}
</script>
<style lang="scss" scoped>
.cardContent {
  width: 285px;
  height: 290px;
  overflow: auto;
  position: fixed;
  border-radius: 5px;
  padding: 10px;
  z-index: 1000;
  background: #fff;
  font-size: 12px;
  text-align: left;
  line-height: 24px;
  box-shadow: 0px 0px 4px 0px rgba(0, 0, 0, 0.3);
  .cardContent_top {
    p {
      font-size: 14px;
    }
    overflow: hidden;
    margin-bottom: 10px;
  }
  h4 {
    border-top: 1px solid #ccc;
    span {
      display: block;
      float: right;
      margin: 0 10px;
      cursor: pointer;
    }
    .activeSpan {
      color: #0256ff;
    }
  }
  .noData {
    width: 100%;
    height: auto;
    text-align: center;
    padding: 30px 0;
    font-size: 14px;
    color: #ccc;
  }
}
</style>
