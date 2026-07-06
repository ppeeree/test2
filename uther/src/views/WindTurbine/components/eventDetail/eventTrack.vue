<template>
  <div class="event_track_card" :style="{ height: data.length ? '150px' : '98px' }">
    <el-timeline v-if="data.length" :reverse="true">
      <el-timeline-item
        v-for="(item, index) in activities"
        :key="index"
        :timestamp="item.timestamp"
        placement="top"
      >
        <span style="float: right; margin-right: 23px; margin-top: -25px">
          <img :src="item.imgUrl" style="margin-right: 10px" />{{ item.content }}</span
        >
        <el-row>
          <el-col :span="6">维修建议：</el-col>
          <el-col :span="18"> {{ item.maintainAdvice }}</el-col>
        </el-row>
        <el-row>
          <el-col :span="6">维修反馈：</el-col>
          <el-col :span="18">{{ item.maintainFeedback }}</el-col>
        </el-row>
      </el-timeline-item>
    </el-timeline>
    <div v-else>
      <img
        style="float: left; margin-left: 50px; margin-top: 20px; margin-right: 20px"
        src="/img/WindTurbine/icon/eventResult.png"
      />
      <span style="font-size: 14px; color: white; line-height: 98px"
        >当前事件未处理，请添加处理方式</span
      >
    </div>
  </div>
</template>

<script>
export default {
  props: {
    handleTime: {
      type: String,
      default: '',
      require: true
    },
    handleMethod: {
      type: String,
      default: '',
      require: true
    },
    data: {
      type: Object,
      default: () => {}
    }
  },
  data() {
    return {
      //存放时间轴的数组
      activities: [],
      //枚举改变传递过来的选项
      changeMethod: {
        1: '运维巡检',
        2: '无人机自主巡检'
      },
      methodImg: {
        1: 'yunwei',
        2: 'wurenji',
        运维巡检: 'yunwei',
        无人机自主巡检: 'wurenji',
        运维检修: 'yunwei'
      }
    }
  },
  watch: {
    data: {
      handler(val) {
        this.getTimeLineData(val)
      }
    }
  },
  mounted() {
    this.getTimeLineData(this.data)
  },
  methods: {
    //将获取到的数据存放到数组中
    getTimeLineData(list) {
      // debugger
      let arr = []
      if (list) {
        list.forEach(item => {
          let element = {
            timestamp: this.timeChange(item.disposalTime),
            content: item.disposalWay,
            imgUrl: require(`/public/img/eventMethod/${this.methodImg[item.disposalWay]}.png`),
            trackingResults: item.trackingResults,
            faultDesc: item.faultDesc,
            maintainAdvice: item.maintainAdvice
          }
          arr.push(element)
        })
      } else {
        return
      }
      this.activities = arr
    },
    //时间格式转化
    timeChange(oldTime) {
      let data = new Date(oldTime)
      let y = data.getFullYear()
      let m = data.getMonth() + 1
      m = m < 10 ? '0' + m : m
      let d = data.getDate()
      d = d < 10 ? '0' + d : d
      let h = data.getHours()
      let minute = data.getMinutes()
      minute = minute < 10 ? '0' + minute : minute
      let second = data.getSeconds()
      second = second < 10 ? '0' + second : second
      let nowTime = y + '-' + m + '-' + d + ' ' + h + ':' + minute + ':' + second
      return nowTime
    }
  }
}
</script>

<style lang="less" scoped>
.event_track_card {
  background: rgba(255, 255, 255, 0.1);
  width: calc(100% - 15px);
  // height: 150px;
  max-height: 150px;
  overflow: hidden;
  text-overflow: ellipsis;
  margin-left: 15px;
  overflow-x: hidden;
  margin-bottom: 10px;
}
:deep(.el-timeline-item){
  margin-left: 30px;
  margin-top: 10px;
}
:deep(.el-timeline-item__content){
  color: white;
}
:deep(.el-timeline-item__timestamp.is-top){
  color: white;
  font-size: 14px;
}
</style>
