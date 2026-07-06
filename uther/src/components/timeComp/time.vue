<template>
  <div class="time_filter_block">
    <div class="data_picker">
      <i class="el-icon-time"></i>
      <div class="data_picker_btn">{{ this.timetext }}</div>
      <!-- <br style="clear: both" /> -->
    </div>
    <el-date-picker
      v-model="localTime"
      type="datetimerange"
      :picker-options="pickerOptions"
      range-separator="~"
      start-placeholder="起始日期"
      end-placeholder="结束日期"
      align="left"
      value-format="yyyy-MM-dd HH:mm:ss"
      :default-time="['00:00:00', '23:59:59']"
      popper-class="date_pop_model"
      :clearable="false"
      :disabled="timeSelect"
    ></el-date-picker>
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
              const end = dayjs().format('YYYY-MM-DD 23:59:59')
              const start = dayjs().subtract(1, 'day').format('YYYY-MM-DD 00:00:00')
              // start.setTime(start.getTime() - 3600 * 1000 * 24 * 1)
              picker.$emit('pick', [start, end])
            }
          },
          {
            text: '三天内',
            onClick(picker) {
              const end = dayjs().format('YYYY-MM-DD 23:59:59')
              const start = dayjs().subtract(3, 'day').format('YYYY-MM-DD 00:00:00')
              picker.$emit('pick', [start, end])
            }
          },
          {
            text: '最近一周',
            onClick(picker) {
              const end = dayjs().format('YYYY-MM-DD 23:59:59')
              const start = dayjs().subtract(7, 'day').format('YYYY-MM-DD 00:00:00')
              picker.$emit('pick', [start, end])
            }
          },
          {
            text: '最近一个月',
            onClick(picker) {
              const end = dayjs().format('YYYY-MM-DD 23:59:59')
              const start = dayjs().subtract(1, 'months').format('YYYY-MM-DD 00:00:00')
              picker.$emit('pick', [start, end])
            }
          },
          {
            text: '最近三个月',
            onClick(picker) {
              const end = dayjs().format('YYYY-MM-DD 23:59:59')
              const start = dayjs().subtract(3, 'months').format('YYYY-MM-DD 00:00:00')
              picker.$emit('pick', [start, end])
            }
          }
        ]
      }
    }
  },
  computed: {
    localTime: {
      get() {
        return this.time
      },
      set(value) {
        this.$emit('update:time', value)
        this.$emit('timeChange', value)
      }
    }
  },
  watch: {
    time: {
      handler(val) {
        if (val) {
          this.$emit('timeChange', val)
          //计算在时间上显示的文本
          let nowData = dayjs().format('YYYY-MM-DD 23:59:59')
          let text = dayjs(nowData).diff(dayjs(val[0]), 'days')
          let timeeText = ''
          // console.log('时间差', text)
          if (nowData == val[1]) {
            timeeText = ''
            if (text == 1) {
              timeeText = '近一天'
            } else if (text == 3) {
              timeeText = '近三天'
            } else if (text == 7) {
              timeeText = '近一周'
            } else if (text < 33 && text > 27) {
              timeeText = '近一个月'
            } else if (text < 93 && text > 88) {
              timeeText = '近三个月'
            } else {
              timeeText = val[0] + '~' + val[1]
            }
            // switch (text) {
            //   case 1:
            //     timeeText = '近一天'
            //     break
            //   case 3:
            //     timeeText = '近三天'
            //     break
            //   case 7:
            //     timeeText = '近一周'
            //     break
            //   case 30:
            //     timeeText = '近一个月'
            //     break
            //   case 31:
            //     timeeText = '近一个月'
            //     break
            //   case 90:
            //     timeeText = '近三个月'
            //     break
            //   case 89:
            //     timeeText = '近三个月'
            //     break
            //   case 92:
            //     timeeText = '近三个月'
            //     break
            //   default:
            //     timeeText = val[0] + '~' + val[1]
            // }
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
}
</style>
