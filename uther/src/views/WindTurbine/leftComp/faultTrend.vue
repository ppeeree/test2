<template>
  <div class="failure_trend">
    <!-- 内容 -->
    <span class="spot">
      <span class="spot_normal" v-for="(item, index) in unitFuilureLevel" :key="index">
        <span class="levelClass" :style="{ background: item.color }"></span>
        <span class="spot_text">{{ item.name }}</span>
      </span>
    </span>
    <template v-if="dataArr.length">
      <div class="blade" v-for="item in dataArr" :key="item.compId">
        <span class="title" v-if="item.faultDataVo.length !== 0">
          <i :class="['icon', 'local', 'local-' + entityPartEnum[item.compName]]"></i>
          <span>{{ item.compName }}</span>
        </span>
        <div class="blade_hotmap">
          <div v-for="(i, index) in item.faultDataVo" :key="index">
            <el-row v-if="i.faultName">
              <el-col span="4" class="hotMap_text">{{ i.faultName }}</el-col>
              <el-col span="20" style="height: 35px">
                <rowHot
                  :chartData="i"
                  unique="bladeone"
                  v-if="i.faultName"
                  :name="i.faultName"
                ></rowHot>
              </el-col>
            </el-row>
          </div>
        </div>
      </div>
    </template>
    <div class="nodata" v-else>
      <noData noteText="" />
    </div>
    <!-- <div class="hot_time">
      <span class="time_num" v-for="(item, index) in hotMapTime" :key="index">{{ item }}</span>
    </div> -->
  </div>
</template>

<script>
import { compFaultHisStateApi } from '@/api/WindTurbine/LeftPartAPI.js'
import rowHot from '../hotMap/rowHot.vue'
import find from 'lodash/find'
import dayjs from 'dayjs'
import { entityPartEnum } from '@/util/constant.js'
import noData from '@/components/noData/index.vue'

export default {
  props: ['showKey'],
  components: { rowHot, noData },
  data() {
    return {
      entityPartEnum,
      dataArr: [],
      //故障数据处理
      bladeData: [],
      cabinData: [],
      towerData: [],
      hotMapTime: [],
      unitFuilureLevel: [
        /*  { name: '无故障', color: '#2ed133' }, */ //'#FCCECE' },
        { name: '轻微', color: '#FF9B9B' },
        { name: '中等', color: '#FF5D5D' },
        { name: '严重', color: '#FF0000' }
      ]
    }
  },
  created() {},
  mounted() {},
  methods: {
    // 获取机组故障信息接口
    init(data) {
      // let timeArr = []
      this.dataArr = []
      let endTime = dayjs().format('YYYY-MM-DD HH:mm:ss')
      let startTime = dayjs().subtract(3, 'months').format('YYYY-MM-DD HH:mm:ss')
      compFaultHisStateApi({ ...data, endTime, startTime }).then(res => {
        if (res.data.code == 200) {
          const data = res.data.data
          let judge = 0
          data.forEach(item => {
            if (item.faultDataVo.length) {
              judge = 1
            }
          })
          if (judge == 0) {
            this.dataArr = []
          } else {
            this.dataArr = data
          }
        }
      })
    },
    //新数据处理
    bladeFaultMaker() {
      let nameArr = []
      this.bladeFaultData.forEach(item => {
        let tranform = item.faultDetail
        const exite = find(nameArr, o => o === tranform)
        if (!exite) {
          let obj = {
            name: '',
            data: []
          }
          nameArr.push(tranform)
          obj.name = tranform
          obj.data.push(item)
          this.bladeData.push(obj)
        } else {
          const index = nameArr.findIndex(o => o == tranform)
          this.bladeData[index].data.push(item)
        }
      })
    },
    cabinFaultMaker() {
      let nameArr = []
      this.cabinFaultData.forEach(item => {
        let tranform = item.faultDetail
        const exite = find(nameArr, o => o === tranform)
        if (!exite) {
          let obj = {
            name: '',
            data: []
          }
          nameArr.push(tranform)
          obj.name = tranform
          obj.data.push(item)
          this.cabinData.push(obj)
        } else {
          const index = nameArr.findIndex(o => o == tranform)
          this.cabinData[index].data.push(item)
        }
      })
    },
    towerFaultMaker() {
      let nameArr = []
      this.towerFaultData.forEach(item => {
        let tranform = item.faultDetail
        const exite = find(nameArr, o => o === tranform)
        if (!exite) {
          let obj = {
            name: '',
            data: []
          }
          nameArr.push(tranform)
          obj.name = tranform
          obj.data.push(item)
          this.towerData.push(obj)
        } else {
          const index = nameArr.findIndex(o => o == tranform)
          this.towerData[index].data.push(item)
        }
      })
    }
  }
}
</script>

<style lang="less" scoped>
.failure_trend {
  // height: 566px;
  padding: 0 15px;
  width: 500px;
  height: auto;
  max-height: 360px;
  overflow-x: hidden;
  overflow-y: auto;
  &::-webkit-scrollbar {
    width: 5px;
  }
  &::-webkit-scrollbar-track {
    background-color: #3e5369;
  }
  &::-webkit-scrollbar-thumb {
    box-shadow: inset 0 0 6px #d2e5f1;
  }
  // z-index: 200;
}
.title {
  height: 23px;
  color: #fff;
  font-size: 14px;
  display: block;
  margin: 5px 10px;
  border-bottom: 1px solid #255873;
  span {
    margin-left: 5px;
  }
}
.hotMap_text {
  color: white;
  font-size: 12px;
  margin-top: 5px;
}
.blade {
  margin-bottom: 10px;
}
.nodata {
  width: 100%;
  height: 200px;
}
.blade_hotmap {
  margin-left: 10px;
}
.cabin_hotmap {
  margin-left: 10px;
}
.tower_hotmap {
  margin-left: 10px;
}

// 热力图上的点
.spot {
  position: absolute;
  top: 10px;
  right: 70px;
}
.levelClass {
  height: 8px;
  width: 8px;
  border-radius: 50%;
  display: inline-block;
  margin-left: 10px;
}
.spot_text {
  color: white;
  font-size: 12px;
  margin-left: 10px;
}
.hot_time {
  height: 13px;
  width: calc(80%);
  color: white;
  margin-left: calc(18%);
  font-size: 12px;
  .time_num {
    margin-right: calc(6.5%);
  }
}
</style>
