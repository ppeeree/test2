<template>
  <div class="mini_fault">
    <div class="fault_text" :style="{ display: controlDetail }">
      <div class="now_faule">
        当前故障：{{ this.nowfault }}
        <span class="now_fault_level" :style="getMiniLevelColor(this.nowlevel)">
          {{ classLevelEnum[this.nowlevel] }}
        </span>
        <faultPart></faultPart>
      </div>

      <div class="history_fault">
        历史故障：
        <faultPart></faultPart>
        <faultPart></faultPart>
      </div>
    </div>
  </div>
</template>

<script>
import faultPart from '../leftComp/faultPart.vue'
import { classLevelEnum } from '@/util/constant'

export default {
  components: { faultPart },
  props: [],
  data() {
    return {
      classLevelEnum,
      nowfault: '',
      nowlevel: '',
      controlDetail: 'none'
    }
  },
  watch: {},
  created() {},
  mounted() {},
  methods: {
    getMiniLevelColor(level) {
      if (level == 1) {
        return { color: '#FF9B9B' }
      } else if (level == 2) {
        return { color: '#FF5D5D' }
      } else if (level == 3) {
        return { color: '#FF0000' }
      }
    },
    //控制下面详情的展示
    setDetailFault(obj) {
      this.nowfault = obj.depict
      this.nowlevel = obj.level
      this.controlDetail = 'block'
    }
  }
}
</script>

<style lang="less" scoped>
.mini_fault {
  .fault_type {
    .mini_fault_level {
      font-size: 12px;
      margin-left: 40px;
      float: right;
      margin-right: 20px;
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
  .fault_text {
    .now_faule {
      font-size: 12px;
      margin-left: 10px;
      .now_fault_level {
        margin-left: 20px;
      }
    }
    .history_fault {
      font-size: 12px;
      margin-left: 10px;
      margin-top: 10px;
    }
  }
}
</style>
