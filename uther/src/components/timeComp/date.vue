<template>
  <div class="time_filter_block">
    <div class="data_picker" :style="{ width: timetext.length > 5 ? '85%' : '80%' }">
      <i class="el-icon-time" style="margin-left: 10px"></i>
      <div class="data_picker_btn">{{ this.timetext }}</div>
    </div>
    <el-date-picker
      v-model="time"
      type="daterange"
      :picker-options="pickerOptions"
      range-separator="~"
      start-placeholder="起始日期"
      end-placeholder="结束日期"
      align="left"
      value-format="yyyy-MM-dd"
      popper-class="date_pop_model"
      :clearable="false"
      :style="{ width: timetext.length > 5 ? '225px' : '125px' }"
    >
    </el-date-picker>
  </div>
</template>
<script>
import dayjs from 'dayjs'
export default {
  props: {
    time: {
      type: Array,
      require: true
    },
    timeSelect: {
      type: Boolean,
      require: true,
      default: false
    }
  },
  data() {
    return {
      pickerOptions: {
        shortcuts: [
          {
            text: '一天内',
            onClick(picker) {
              const end = dayjs().format('YYYY-MM-DD')
              const start = dayjs().subtract(1, 'day').format('YYYY-MM-DD')
              picker.$emit('pick', [start, end])
            }
          },
          {
            text: '三天内',
            onClick(picker) {
              const end = dayjs().format('YYYY-MM-DD')
              const start = dayjs().subtract(3, 'day').format('YYYY-MM-DD')
              picker.$emit('pick', [start, end])
            }
          },
          {
            text: '近一周',
            onClick(picker) {
              const end = dayjs().format('YYYY-MM-DD')
              const start = dayjs().subtract(7, 'day').format('YYYY-MM-DD')
              picker.$emit('pick', [start, end])
            }
          },
          {
            text: '近两周',
            onClick(picker) {
              const end = dayjs().format('YYYY-MM-DD')
              const start = dayjs().subtract(14, 'day').format('YYYY-MM-DD')
              picker.$emit('pick', [start, end])
            }
          },
          {
            text: '近一个月',
            onClick(picker) {
              const end = dayjs().format('YYYY-MM-DD')
              const start = dayjs().subtract(1, 'months').format('YYYY-MM-DD')
              picker.$emit('pick', [start, end])
            }
          },
          {
            text: '近三个月',
            onClick(picker) {
              const end = dayjs().format('YYYY-MM-DD')
              const start = dayjs().subtract(3, 'months').format('YYYY-MM-DD')
              picker.$emit('pick', [start, end])
            }
          },
          {
            text: '近半年',
            onClick(picker) {
              const end = dayjs().format('YYYY-MM-DD')
              const start = dayjs().subtract(6, 'months').format('YYYY-MM-DD')
              picker.$emit('pick', [start, end])
            }
          },
          {
            text: '近一年',
            onClick(picker) {
              const end = dayjs().format('YYYY-MM-DD')
              const start = dayjs().subtract(12, 'months').format('YYYY-MM-DD')
              picker.$emit('pick', [start, end])
            }
          }
        ]
      }
    }
  },
  watch: {
    time: {
      handler(val) {
        if (val) {
          this.$emit('timeChange', val)
          //计算在时间上显示的文本
          let nowData = dayjs().format('YYYY-MM-DD')
          let text = dayjs(nowData).diff(dayjs(val[0]), 'days')
          let timeeText = ''
          if (nowData == val[1]) {
            timeeText = ''
            if (text == 1) {
              timeeText = '近一天'
            } else if (text == 3) {
              timeeText = '近三天'
            } else if (text == 7) {
              timeeText = '近一周'
            } else if (text == 14) {
              timeeText = '近两周'
            } else if (text < 33 && text > 27) {
              timeeText = '近一个月'
            } else if (text < 93 && text > 88) {
              timeeText = '近三个月'
            } else if (text < 186 && text > 177) {
              timeeText = '近半年'
            } else if (text < 366 && text > 364) {
              timeeText = '近一年'
            } else {
              timeeText = val[0] + '~' + val[1]
            }
          } else {
            timeeText = val[0] + '~' + val[1]
          }
          this.timetext = timeeText
        }
      },
      deep: true,
      immediate: true
    }
  }
}
</script>

<style lang="scss" scoped>
.time_filter_block {
  float: left;
  .data_picker {
    height: 33px;
    position: relative;
    z-index: 10;
    margin-left: 1px;
    margin-bottom: 1px;
    border-radius: 5px;
    line-height: 35px;
    width: 85%;
    .el-icon-time {
      position: relative;
      top: -2px;
    }
    .data_picker_btn {
      display: inline-block;
      margin-left: 10px;
      position: relative;
      top: -2px;
    }
  }
}
::v-deep .el-input__inner {
  top: -36px;
}
::v-deep .el-icon-date:before {
  content: '\e790';
  position: absolute;
  right: 11px;
  top: -2px;
  cursor: pointer;
}
</style>
