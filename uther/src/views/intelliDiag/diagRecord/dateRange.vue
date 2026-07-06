<template>
  <el-date-picker
    align="right"
    unlink-panels
    range-separator="~"
    start-placeholder="开始"
    end-placeholder="结束"
    ref="time"
    v-model="timeValue"
    type="daterange"
    value-format="YYYY-MM-DD"
    :shortcuts="pickerShortcuts"
    prefix-icon=""
    clear-icon=""
  >
  </el-date-picker>
</template>
<script>
import dayjs from 'dayjs'
import customParseFormat from 'dayjs/plugin/customParseFormat'
dayjs.extend(customParseFormat)
export default {
  props: {
    defaultTime: {
      type: Array,
      require: false,
      default: () => []
    }
  },
  data() {
    return {
      pickerShortcuts: [
        {
          text: '近7天',
          value: () => {
            const end = new Date()
            const start = new Date()
            start.setTime(start.getTime() - 3600 * 1000 * 24 * 7)
            return [start, end]
          }
        },
        {
          text: '近30天',
          value: () => {
            const end = new Date()
            const start = new Date()
            start.setTime(start.getTime() - 3600 * 1000 * 24 * 30)
            return [start, end]
          }
        }
      ],
      timeValue: [dayjs().subtract(30, 'day').format('YYYY-MM-DD'), dayjs().format('YYYY-MM-DD')]
    }
  },
  mounted() {
    if (this.defaultTime.length > 0) {
      this.timeValue = this.defaultTime
    }
  },
  methods: {}
}
</script>
<style lang="scss" scoped>
.inline-input {
  width: 100px;
  :deep(.el-input__inner){
    padding: 0;
    text-align: center;
  }
}
input {
  height: 30px;
  line-height: 30px;
}
:deep(.el-range-editor){
  width: 200px;
  height: 30px;
  line-height: 30px;
}
:deep(.el-date-editor){
  .el-range-separator {
    color: #303133;
    padding: 0;
  }
  .el-range-input {
    width: 45%;
  }
  .el-range__icon {
    display: none;
  }
  .el-range__close-icon {
    display: none;
  }
}
</style>
