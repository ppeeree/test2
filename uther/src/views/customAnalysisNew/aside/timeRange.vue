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
    value-format="yyyy-MM-dd"
    :picker-options="pickerOptions"
    prefix-icon=""
    clear-icon=""
  >
  </el-date-picker>
</template>
<script>
import dayjs from 'dayjs'
import customParseFormat from 'dayjs/plugin/customParseFormat'
import { getHasDataDates } from '@/api/analysis/index.js'
dayjs.extend(customParseFormat)
export default {
  props: {
    defaultTime: {
      type: Array,
      require: false,
      default: () => []
    },
    deviceList: {
      type: Array,
      require: false,
      default: () => []
    }
  },
  data() {
    let that = this
    return {
      hasDataDateList: [],
      pickerOptions: {
        cellClassName: date => {
          if (that.hasDataDateList.includes(dayjs(date).format('YYYY-MM-DD'))) {
            return 'badge'
          }
        },
        shortcuts: [
          {
            text: '最近三天',
            onClick(picker) {
              const end = new Date()
              const start = new Date()
              start.setTime(start.getTime() - 3600 * 1000 * 24 * 3)
              picker.$emit('pick', [start, end])
            }
          },
          {
            text: '最近一周',
            onClick(picker) {
              const end = new Date()
              const start = new Date()
              start.setTime(start.getTime() - 3600 * 1000 * 24 * 7)
              picker.$emit('pick', [start, end])
            }
          },
          {
            text: '最近一个月',
            onClick(picker) {
              const end = new Date()
              const start = new Date()
              start.setTime(start.getTime() - 3600 * 1000 * 24 * 30)
              picker.$emit('pick', [start, end])
            }
          },
          {
            text: '最近三个月',
            onClick(picker) {
              const end = new Date()
              const start = new Date()
              start.setTime(start.getTime() - 3600 * 1000 * 24 * 90)
              picker.$emit('pick', [start, end])
            }
          }
        ]
      },
      timeValue: [
        /*  dayjs().subtract(6, 'month').format('YYYY-MM-DD 00:00:00'),
        dayjs().format('YYYY-MM-DD 23:59:59') */
        dayjs().subtract(1, 'year').format('YYYY-MM-DD'),
        dayjs().format('YYYY-MM-DD')
      ]
    }
  },
  mounted() {
    if (this.defaultTime.length > 0) {
      this.timeValue = this.defaultTime
    }
    let unwatch = null
    // 监听活动日历 月份发生变化后需要刷新日历中活动的标志
    this.$nextTick(() => {
      const datePicker = this.$refs.time // 获取组件
      // pickerVisible为true时picker才展示出来
      datePicker.$watch('pickerVisible', newValue => {
        if (newValue) {
          // 打开了日期选择面板，获取picker组件
          const picker = datePicker.picker
          this.hasDataDateList = []
          this.$nextTick(() => {
            this.getActivityListByDate(picker.leftLabel)
            this.getActivityListByDate(picker.rightLabel)
            // this.getActivityListByDate(picker.leftLabel, picker.rightLabel)
          })
          if (picker) {
            unwatch = picker.$watch('leftLabel', newDate => {
              // 切换月份后的日期 后续使用该日期进行相关数据查询
              this.$nextTick(() => {
                this.getActivityListByDate(newDate)
              })
            })
            unwatch = picker.$watch('rightLabel', newDate => {
              // 切换月份后的日期 后续使用该日期进行相关数据查询
              this.$nextTick(() => {
                this.getActivityListByDate(newDate)
              })
            })
          }
        } else {
          if (unwatch) {
            // 移除监听效果 防止重复监听 多次触发回调
            unwatch()
          }
        }
      })
    })
  },
  methods: {
    getActivityListByDate(date, date2) {
      let months = []
      if (date) {
        // 监听面板点击切换, 获取当前面板月份，当前月，前后各一个月
        months.push(dayjs(date, 'YYYY年M月').format('YYYY-MM'))
      }
      if (date2) {
        months.push(dayjs(date2, 'YYYY年M月').format('YYYY-MM'))
      }
      let checkedData = this.$parent.$refs.tree.getCheckedNodes()
      if (!checkedData.length) {
        return
      }
      let checkedTurbines = this.$parent.getCheckedData(
        checkedData,
        this.$parent.$refs.tree.getHalfCheckedNodes(),
        'turbine'
      )
      /*   this.deviceList.forEach(item => {
        checkedTurbines.add(item.windturbineID)
      }) */
      getHasDataDates({
        months: [...new Set(months)].join(','),
        deviceID: checkedTurbines.join(',')
      }).then(res => {
        if (res.data.data.length) {
          let arr = [...new Set([].concat(this.hasDataDateList, res.data.data))]
          this.hasDataDateList = arr
        }
      })
    }
  }
}
</script>
<style lang="scss" scoped>
.inline-input {
  width: 100px;
  ::v-deep .el-input__inner {
    padding: 0;
    text-align: center;
  }
}
input {
  height: 30px;
  line-height: 30px;
}
::v-deep .el-range-editor {
  width: 200px;
  height: 30px;
  line-height: 30px;
}
::v-deep .el-date-editor {
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
