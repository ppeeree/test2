<template>
  <div class="right_mini">
    <div class="bigSize_btn">
      <span @click="changeRightSize"><i class="el-icon-d-arrow-left" title="展开侧边栏"></i></span>
    </div>
    <ul
      v-if="newsList.length"
      class="newlist_ul"
      @mouseenter="stopInterval"
      @mouseout="startInterval"
    >
      <li v-for="(item, index) in newsList" :key="index" :class="{ 'animate-up': animateUp }">
        <span :style="{ color: eventLevelArray[item.degree] }">
          {{ eventLevelArrayName[item.degree] }}
        </span>
        <span>
          <img
            style="float: left; margin-top: 5px; margin-right: 5px; width: 20px; height: 20px"
            :src="requireImgType(item.type)"
          />
          <b style="font-weight: normal">{{ GeneratorEnum[item.type] }}</b>
        </span>
        <span>{{ item.message }}</span>
        <span>{{ item.time }}</span>
        <span>{{ item.status }}</span>
      </li>
    </ul>
    <div v-else class="nodata">暂无事件</div>
  </div>
</template>
<script>
import { GeneratorEnum, eventStatusEnum, eventLevelEnum, compSimpleCode } from '@/util/constant.js'
//引入接口
import { getEntityEventStatisticApi } from '@/api/WindTurbine/RightPartAPI.js'
export default {
  props: {
    entityId: {
      type: String,
      default: '',
      require: true
    },
    currentComp: {
      type: String,
      default: 'turbine',
      require: true
    }
  },
  data() {
    return {
      selectedComp: '',
      animateUp: false,
      timer: null, // 定时器
      timerAxios: null,
      eventLevelEnum,
      GeneratorEnum,
      eventStatusEnum,
      compSimpleCode,
      eventLevelArray: {
        first: '#FFF287',
        second: '#F5B270',
        third: '#E85E51',
        fourth: '#DC1034'
      },
      eventLevelArrayName: {
        first: '一级',
        second: '二级',
        third: '三级',
        fourth: '四级'
      },
      newsList: []
    }
  },
  watch: {
    entityId: {
      handler() {
        this.getTableData()
      }
    }
  },
  created() {
    this.timerAxios = setInterval(this.getTableData, 60 * 1000)
    this.getTableData()
  },
  mounted() {
    this.timer = setInterval(this.scrollAnimate, 2500)
  },
  methods: {
    changeRightSize() {
      this.$emit('changeRightSize')
    },
    scrollAnimate() {
      this.animateUp = true
      setTimeout(() => {
        this.newsList.push(this.newsList[0])
        this.newsList.shift()
        this.animateUp = false
      }, 2000)
    },
    stopInterval() {
      this.timer && clearInterval(this.timer)
      this.timer = null
    },
    startInterval() {
      if (this.timer) {
        clearInterval(this.timer)
        this.timer = null
      }
      this.timer = setInterval(this.scrollAnimate, 2500)
    },
    requireImgType(id) {
      const note = {
        health: 3,
        inspection: 1,
        work: 4
      }
      return require(`../img/eventTypeTable/${note[id]}.svg`)
    },
    getTableData() {
      this.timer && clearInterval(this.timer)
      this.timer = null
      let entityType = this.compSimpleCode[this.currentComp]
      /* getEntityEventStatisticApi({ id: this.entityId, type: entityType }).then(res => {
        if (res.data.code === 200) {
          this.newsList = res.data.data.eventInfo
          this.timer = setInterval(this.scrollAnimate, 2500)
        }
      }) */
    }
  },
  beforeUnmount() {
    clearInterval(this.timer)
    clearInterval(this.timerAxios)
    this.timer = null
    this.timerAxios = null
  }
}
</script>
<style scoped lang="scss">
.right_mini {
  float: right;
  position: fixed;
  top: 8%;
  left: calc(100% - 460px);
  width: 460px;
  height: 35px;
  border-radius: 5px 0px 0px 5px;
  background: rgba(5, 46, 64, 0.7);
  box-sizing: border-box;
  border: 1px solid #1c8c93;
  overflow: hidden;
  .bigSize_btn {
    height: 40px;
    text-align: center;
    float: left;
    width: 50px;
    span {
      display: inline-block;
      width: 40px;
      height: 25px;
      margin-top: 4px;
      border-radius: 4px;
      background: #255873;
      color: #1fffff;
      line-height: 25px;
      border: 1px solid #1c8c93;
      cursor: pointer;
      &:hover {
        background: #33b98a;
        border: 1px solid #1c8c93;
      }
    }
  }
  .newlist_ul {
    float: left;
    width: calc(100% - 50px);
    height: 100%;
    overflow: hidden;
    li {
      font-size: 12px;
      color: #fff;
      line-height: 35px;
      padding-left: 10px;
      display: flex;
      flex-direction: row;
      justify-content: space-around;
    }
    .animate-up {
      transition: all 2s ease-in-out;
      transform: translateY(-35px); //向上滚动
    }
  }
  .nodata {
    width: 100%;
    text-align: center;
    color: #909399;
    font-size: 12px;
  }
}
</style>
