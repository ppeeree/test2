<template>
  <div class="container_box">
    <div class="windTurbine_state" :style="{ height: parent.isControl ? '80%' : '50%' }">
      <el-col
        class="turbine_item"
        :span="8"
        v-for="item in turbineList"
        :key="item.value"
        @click="clickWindTurbineMonitor(item.value, item.list)"
      >
        <span class="num" :style="changeNumStyle(item.value, item.num)" v-if="item.name == '监控率'"
          >{{ item.num }}%</span
        >
        <span
          class="num"
          style="cursor: pointer"
          :style="changeNumStyle(item.value, item.num)"
          v-else
          >{{ item.num }}</span
        >
        <span class="text"> {{ item.name }} </span>
      </el-col>
    </div>
    <div class="windTurbine_informion" v-if="!parent.isControl">
      <el-col class="pink_item" :span="8" v-for="item in pinkList" :key="item.value">
        <span class="num">{{ item.num }}</span>
        <span class="text"> {{ item.name }}</span>
      </el-col>
    </div>
  </div>
</template>

<script>
import {
  getUserTurbinesInfo,
  getWindTurbineStatistics,
  getWindTurbineMonitorNum
} from '@/api/screen/leftCardApi'
import { defineAsyncComponent } from 'vue'

export default {
  components: {
    Card: defineAsyncComponent(() => import('./card.vue')),
    GaugeCharts: defineAsyncComponent(() => import('../base/gauge-charts.vue')),
    PieDoughnut: defineAsyncComponent(() => import('../base/pie-doughnut.vue')),
    BarTickAlign: defineAsyncComponent(() => import('../base/bar-tickAlign.vue'))
  },
  inject: ['parent'],
  watch: {
    /*  'parent.isControl': {
      handler() {
        this.initData()
      }
    } */
  },
  data() {
    return {
      turbineList: [],
      pinkList: [],
      peiDataIndex: null
    }
  },
  mounted() {
    //  this.initData()
  },
  methods: {
    initData() {
      if (this.parent.isControl) {
        getUserTurbinesInfo({ userID: this.$store.getters.userInfo.user_id }).then(res => {
          if (res.data.code == 200) {
            let turbineList = []
            let data = res.data.data
            turbineList.push(
              {
                name: '风场',
                value: 'windpark',
                num: data.stationNum,
                list: []
              },
              {
                name: '机组',
                value: 'turbine',
                num: data.deviceNum,
                list: []
              },
              {
                name: '监控率',
                value: 'monitorRate',
                num: Math.round(data.stationControlRate * 100) /* .toFixed(2) */
              }
            )
            this.turbineList = turbineList
          }
        })
      } else {
        this.getWindTurbineStatisticsFunc()
        this.getWindTurbineMonitorNumFunc()
      }
    },
    getWindTurbineStatisticsFunc() {
      getWindTurbineStatistics({
        stationId: this.parent.WindFarm.id
      }).then(res => {
        if (res.data.code == 200 && res.data.data[0]) {
          const data = res.data.data[0]
          let pinkList = []
          pinkList.push(
            {
              name: '风机型号',
              value: 'type',
              num: data.childNode2[0] ? data.childNode2[0].name : '--'
            },
            {
              name: '生产厂商',
              value: 'maker',
              num: data.childNode[0] ? data.childNode[0].name : '--'
            },

            {
              name: '运行时长',
              value: 'time',
              num: data.childNode1[0] ? data.childNode1[0].name : '--'
            }
          )
          this.pinkList = pinkList
        } else {
          this.pinkList = []
        }
      })
    },
    getWindTurbineMonitorNumFunc() {
      getWindTurbineMonitorNum({
        stationId: this.parent.WindFarm.id
      }).then(res => {
        if (res.data.code == 200) {
          let turbineList = []
          let data = res.data.data
          turbineList.push(
            {
              name: '监控中',
              value: 'monitoring',
              num: data.monitorNumber,
              list: data.monitorNumberIds
            },
            {
              name: '离线',
              value: 'offline',
              num: data.offlineNumber,
              list: data.offlineNumberIds
            },
            {
              name: '监控率',
              value: 'monitorRate',
              num: Math.round(data.monitoringRate * 100) /* .toFixed(2) */
            }
          )
          this.turbineList = turbineList
        }
      })
    },

    //数字样式
    changeNumStyle(value, num) {
      let numcolor = ''
      if (value == 'offline') {
        if (num == 0) {
          numcolor = '#C2C2C2'
        } else {
          numcolor = '#FF0D52'
        }
      } else if (value == 'monitorRate') {
        if (num == 100) {
          numcolor = '#0DFF7A'
        } else if (num < 85) {
          numcolor = '#FF0D52'
        } else {
          numcolor = '#FFCF0D'
        }
      }
      return { color: numcolor }
    },
    //点击监控数据联动
    clickWindTurbineMonitor(type, list) {
      if (type !== 'monitorRate') {
        this.setSessionItem('changeEntities', '1')
        const ids = list
        this.$cesuimData.popupList.length > 0 &&
          this.$cesuimData.popupList.forEach(item => {
            item.windowClose()
          })
        this.$cesuimData.popupList = []
        this.handleInclude(ids, { isAll: true, bool: false })
        if (this.peiDataIndex === type) {
          this.handleInclude(ids, { isAll: true, bool: true }, true)
          this.setSessionItem('changeEntities', '0')
          return this.passPart()
        }
        this.peiDataIndex = type
      }
    },
    handleInclude(e, all = { isAll: false, bool: true }, isShow) {
      const temp = this.$utils.map.getInclude(
        { isAll: isShow || false, bool: true },
        // eslint-disable-next-line no-useless-escape
        e.map(item => 'entitie\-' + item),
        true
      )
      this.$utils.map.getInclude(
        all,
        // eslint-disable-next-line no-useless-escape
        e.map(item => 'label\-' + item),
        true,
        this.$utils.map.FJLabel
      )
      this.$utils.map.getInclude(
        all,
        // eslint-disable-next-line no-useless-escape
        e.map(item => 'event\-' + item),
        true,
        this.$utils.map.eventIcon
      )
      return temp
    },
    passPart() {
      this.peiDataIndex = null
      this.setSessionItem('icoName', null)
      this.$bus.$emit('pieDoughnut', {
        all: true,
        id: null
      })
      this.$cesuimData.popupList.forEach((item, index) => {
        item.windowClose()
        this.$cesuimData.popupList.splice(index, 1)
      })
    }
  }
}
</script>

<style lang="less" scoped>
@import url('../style/minxs.less');
@font-face {
  font-family: 'DS-DIGIB-2';
  src: url('../../WindTurbine/font/DS-DIGIB-2.ttf') format('truetype');
}
.container_box {
  width: 100%;
  height: 220px;
  // padding-top: 15px;
  .windTurbine_state {
    height: 50%;
    padding-top: 7px;
    display: flex;
    .turbine_item {
      // height: 100px;
      height: 100%;
      background-image: url('/img/screen/newPart1/windBigBg.png');
      // margin: 0 8px 0px 10px !important;
      background-repeat: no-repeat;
      background-position: center bottom;
      display: flex;
      flex-direction: column;
      justify-content: center;
      span {
        display: inline-block;
        width: 100%;
        text-align: center;
      }
      .num {
        font-family: 'DS-DIGIB-2';
        font-size: 50px;
        font-weight: bold;
        color: #0dff7a;
      }
      .text {
        font-size: 12px;
        font-weight: 500;
        color: white;
        margin-bottom: 10px;
      }
    }
  }
  .windTurbine_informion {
    width: 100%;
    height: 46%;
    padding-top: 20px;
    display: flex;
    .pink_item {
      height: 100%;
      background-image: url('/img/screen/newPart1/windSmellBg.png');
      // margin: 0 17px 0px 20px !important;
      background-repeat: no-repeat;
      background-position: center bottom;
      display: flex;
      flex-direction: column;
      justify-content: space-around;
      span {
        display: inline-block;
        width: 100%;
        text-align: center;
        color: #abe1ff;
        padding-right: 0px;
      }
      .num {
        font-size: 18px;
        font-weight: 500;
      }
      .text {
        font-size: 12px;
        font-weight: 500;
        margin-bottom: 5px;
      }
    }
  }
}
</style>
