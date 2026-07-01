<template>
  <div class="windField_card">
    <!-- :windData="windData" -->
    <windTag
      style="float: right; right: 100px; top: 10px"
      :sortOptions="[
        {
          value: 'windturbineName',
          label: '机组名称'
        },
        {
          value: 'tubineStatusIndex',
          label: '报警状态'
        },
        {
          value: 'statusLastTime',
          label: '更新时间'
        }
      ]"
      @sortChange="sortChange"
    ></windTag>

    <div
      class="turbine_dom"
      ref="turbineDom"
      v-loading="isCardLoading"
      element-loading-spinner="el-icon-loading"
      element-loading-background="rgba(0, 0, 0, 0.4)"
    >
      <template>
        <turbineCardVue
          v-for="item in cardData"
          :key="item"
          :data="item"
          :style="{ width: boxWidth + 'px', height: boxHeight + 'px' }"
          :boxWidth="boxWidth"
          :turbineNum="turbineNum"
        ></turbineCardVue>

        <NoData
          v-show="!cardData.length"
          style="background-color: rgba(13, 52, 83, 1); margin-top: 10px"
        ></NoData>
      </template>
    </div>
  </div>
</template>

<script>
import { eventTypeEnum, levelColor } from '@/util/constant.js'
import { windFieldStatusApi } from '@/api/screen/index.js'

import turbineCardVue from './turbineCard.vue'
import windTag from './windTag.vue'
import NoData from '@/components/noData' // 暂无数据

export default {
  components: { turbineCardVue, windTag, NoData },
  props: {
    selectWind: {
      type: Object,
      default: () => {}
    }
  },
  data() {
    return {
      eventTypeEnum,
      levelColor,

      isCardLoading: false,

      timer: null,

      selectStatus: '',
      windData: [], //风场列表
      pankData: [], //风场信息

      cardData: [], //展示的卡片数据
      allCardData: [], //全部数据
      boxWidth: '250',
      boxHeight: '145',

      turbineNum: '',

      turbineStatus: [
        {
          level: 'danger',
          list: [],
          num: 0
        },
        {
          level: 'warning',
          list: [],
          num: 0
        },
        {
          level: 'attention',
          list: [],
          num: 0
        },
        {
          level: 'normal',
          list: [],
          num: 0
        },
        {
          level: 'nostate',
          list: [],
          num: 0
        }
      ]
    }
  },
  watch: {
    selectWind: {
      handler(val) {
        if (val && val.id) {
          /*    if (this.timer) {
            this.clearTimer()
          } */
          this.getTurbineData(this.selectWind)
          /*  this.timer = setInterval(() => {
            this.getTurbineData(this.selectWind)
          }, 60 * 1000) */
        }
      },
      immediate: true
    }
  },
  created() {},
  mounted() {},
  beforeDestroy() {
    this.clearTimer()
  },
  methods: {
    clearTimer() {
      clearInterval(this.timer)
      this.timer = null
    },

    // 1.2、卡片+风场信息
    getTurbineData(val) {
      this.isCardLoading = true
      this.selectStatus = ''
      // console.log(val)
      // 获取中间卡片数据 新版本接口传val.code
      windFieldStatusApi({ stationId: val.code }).then(res => {
        if (res.data.code === 200) {
          this.allCardData = res.data.data.map(item => ({
            ...item,
            healthStatusEntityVo: item.healthStatusEntityVo.filter(
              child => child.entityId.trim() !== ''
            )
          }))

          this.cardData = this.handlerTurbineStatusIndex(res.data.data)

          this.turbineNum = this.cardData.length

          this.handlerCardSize()

          this.handlerTurbineStatus(this.cardData)

          this.sortChange('windturbineName', 'Order')
          this.isCardLoading = false
        }
      })

      /*  // 获取风场信息
      getWindTurbineStatistics().then(res => {
        if (res.data.code === 200) {
          const data = res.data.data
          if (data.length) {
            this.pankData = [
              {
                name: '风机型号',
                value: 'type',
                num: data[0].childNode2[0].name
              },
              {
                name: '生产厂商',
                value: 'maker',
                num: data[0].childNode[0].name
              },

              {
                name: '运行时长',
                value: 'time',
                num: data[0].childNode1[0].name
              }
            ]
          } else {
            this.pankData = []
          }
        }
      }) */
    },
    // 2、处理卡片尺寸
    handlerCardSize() {
      if (!this.$refs['turbineDom']) return
      let width = this.$refs['turbineDom'] && this.$refs['turbineDom'].offsetWidth
      let height = this.$refs['turbineDom'] && this.$refs['turbineDom'].offsetHeight
      this.$refs['turbineDom'].style.overflow = 'hidden'
      let wnum, hnum
      if (this.turbineNum <= 24) {
        wnum = 6
        hnum = 4
      } else if (this.turbineNum > 24 && this.turbineNum <= 35) {
        wnum = 7
        hnum = 5
      } else if (this.turbineNum > 35 && this.turbineNum <= 56) {
        wnum = 8
        hnum = 7
      } else if (this.turbineNum > 56 && this.turbineNum <= 70) {
        wnum = 10
        hnum = 7
      } else if (this.turbineNum > 70 && this.turbineNum <= 88) {
        wnum = 11
        hnum = 8
      } else {
        wnum = 10
        hnum = 8
        width -= 10
        this.$refs['turbineDom'].style.overflow = 'auto'
      }

      this.boxWidth = (width - wnum * 10) / wnum
      this.boxHeight = (height - hnum * 10) / hnum
    },

    // 处理状态
    handlerTurbineStatusIndex(arr) {
      let param = ['unknown', 'normal', 'attention', 'warning', 'danger']
      arr.map(obj => {
        let index = param.indexOf(obj.windturbineStatus)
        obj.tubineStatusIndex = index !== -1 ? index : 0
        // if (index !== -1) {
        //   obj.tubineStatusIndex = index
        // } else {
        //   obj.tubineStatusIndex = 0
        // }
        return obj
      })
      return arr
    },

    // 3、汇总状态
    handlerTurbineStatus(data) {
      this.turbineStatus = [
        {
          level: 'danger',
          list: [],
          num: 0
        },
        {
          level: 'warning',
          list: [],
          num: 0
        },
        {
          level: 'attention',
          list: [],
          num: 0
        },
        {
          level: 'normal',
          list: [],
          num: 0
        },
        {
          level: 'nostate',
          list: [],
          num: 0
        }
      ]
      data.forEach(item => {
        let obj = this.turbineStatus.find(j => j.level == item.windturbineStatus)
        if (obj) {
          obj.list.push(item)
          obj.num = obj.num + 1
        } else {
          let nostateObj = this.turbineStatus.find(j => j.level == 'nostate')
          nostateObj.list.push(item)
          nostateObj.num = nostateObj.num + 1
        }
      })
    },

    // 4、排序改变
    sortChange(val, orderVal) {
      if (orderVal == 'Order') {
        this.cardData = this.sortObject(this.cardData, val)
      } else {
        this.cardData = this.noneSortObject(this.cardData, val)
      }
    },
    // 4.1、从小到大
    sortObject(array, key) {
      return array.sort(function (a, b) {
        let x = a[key]
        let y = b[key]
        return x < y ? -1 : x > y ? 1 : 0
      })
    },
    // 4.2、从大到小
    noneSortObject(array, key) {
      return array.sort(function (a, b) {
        let x = a[key]
        let y = b[key]
        return x > y ? -1 : x < y ? 1 : 0
      })
    }

    // 5、根据状态筛选机组
    /*   selectTurbineStatus(val) {
      if (this.selectStatus == val.level) {
        this.selectStatus = ''
        this.cardData = this.allCardData
      } else {
        this.selectStatus = val.level
        this.cardData = val.list
      }
    }, */
  }
}
</script>

<style lang="less" scoped>
.windField_card {
  width: calc(100% - 470px); //100vw;;
  height: 100%;
  display: inline-block;
  position: relative;
  color: white;
  padding: 0 15px 0 15px;
  .turbine_dom {
    height: calc(100% - 130px);
    width: calc(100% + 10px);
    margin-top: 55px;
  }
}
</style>
