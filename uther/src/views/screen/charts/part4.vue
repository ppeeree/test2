<template>
  <div>
    <div class="content">
      <div class="card-position">
        <div v-for="(item, index) in healthyArray" :key="item.lable">
          <!--  @click.native="handlerIndex(index, item.ids, levelColorEnum[item.statusName])" -->
          <card
            :title="item.statusName"
            :num="item.deviceCount"
            :type="levelColorEnum[item.statusCode]"
            :class="[activeNum === index ? 'active-box' : '', 'item-card']"
          ></card>
        </div>
      </div>
    </div>
    <div class="chart-title">具体分布</div>
    <bar-detail v-if="isChartShow" :dataList="dataList"></bar-detail>
    <div class="nodata" v-else>
      <noData noteText="暂无数据" firstText="" />
    </div>
  </div>
</template>

<script>
import { levelColorEnum } from '@/util/constant'
import { healthStatusStatistic } from '@/api/screen/leftCardApi'
import func from '@/util/func.js'
import { defineAsyncComponent } from 'vue'

export default {
  components: {
    noData: defineAsyncComponent(() => import('@/components/noData/index.vue')),
    Card: defineAsyncComponent(() => import('./card.vue')),
    BarDetail: defineAsyncComponent(() => import('../base/bar-detail.vue'))
  },
  inject: ['parent'],
  data() {
    return {
      healthyArray: [],
      levelColorEnum,
      stateColer: Object.freeze(['#FF0F0D', '#FFE604', '#FF6B0E', '#2ED133', '#8D8D8D']),
      activeNum: null,
      dataList: [],
      isChartShow: false,
      healthBgTitle: '健康指数'
    }
  },
  mounted: function () {
    //this.init()
    this.$bus.$on('stateTypeClick', () => {
      this.activeNum = null
    })
  },
  methods: {
    async init() {
      this.dataList = []
      this.healthyArray = [] // cloneDeep(defaultHealthyArray)
      this.healthStatusStatistic()
    },
    async healthStatusStatistic() {
      let params = this.parent.isControl
        ? { userID: this.$store.getters.userInfo.user_id }
        : {
            stationId: this.parent.WindFarm.id
          }
      const { data: res } = await healthStatusStatistic(params)
      this.healthyArray = res.data.dayStatusList
      // 新接口 res.data?.weekStatusList
      if (
        res.data &&
        !func.isUndefined(res.data.weekStatusList) &&
        res.data.weekStatusList.length
      ) {
        this.isChartShow = true
        // 新接口res.data.weekStatusList
        this.dataList = res.data.weekStatusList
      } else {
        this.isChartShow = false
      }
    },
    handlerIndex(index, ids, stateColer) {
      this.$bus.$emit('pieDoughnutRe', {
        all: true,
        id: null
      })
      this.$bus.$emit('leftTalbe')
      this.$bus.$emit('progressClick')
      this.$bus.$emit('pieDoughnut', {
        all: true,
        id: null
      })
      this.$bus.$emit('levelEvent', {
        all: true,
        id: null
      })
      this.$bus.$emit('barStacked')
      this.setSessionItem('changeEntities', '1')
      // doSomethings
      if (this.activeNum === index) {
        this.activeNum = null
        this.setSessionItem('changeEntities', '0')
        return this.handleInclude(ids, stateColer, { isAll: true, bool: true })
      }
      this.activeNum = index
      // this.$utils.map.initMapStatus()
      this.handleInclude(ids, stateColer)
    },
    handleInclude(ids, stateColer, all = { isAll: false, bool: true }) {
      this.$utils.map.getInclude(
        all,
        // eslint-disable-next-line no-useless-escape
        ids.map(item => 'entitie\-' + item),
        true
      )
      this.$utils.map.getInclude(
        all,
        // eslint-disable-next-line no-useless-escape
        ids.map(item => 'label\-' + item),
        true,
        this.$utils.map.FJLabel,
        false
      )
      this.$utils.map.getInclude(
        all,
        // eslint-disable-next-line no-useless-escape
        ids.map(item => 'event\-' + item),
        true,
        this.$utils.map.eventIcon
      )
      this.$cesuimData.popupList.length > 0 && this.$cesuimData.handleClearPopupList()
    }
  }
}
</script>

<style lang="less" scoped>
@import url('../style/minxs.less');
.content {
  display: flex;
  flex-direction: row;
  align-items: center;
  // position: relative;
  // right: 12px;
  justify-content: center;
  .card-position {
    display: flex;
    flex-direction: row;
    // position: relative;
    // top: 12px;
    // right: 21px;
    // margin-top: 25px;
    margin-left: 8px;
    // width: 375px;
    justify-content: space-around;
    width: 100%;
    padding: 0 10px;
    div {
      width: 85px;
      cursor: pointer;
      .item-card {
        padding: 7px 7px 7px 2px;
      }
    }
    div + div {
      margin-left: 5px;
    }
  }
  .chart-title {
    color: #fff;
    font-size: 12px;
    padding-left: 20px;
    line-height: 20px;
  }
}

.active-box {
  background: #0094c5;
  box-sizing: border-box;
  // border: 1px solid #1fffff;
}
.nodata {
  height: 150px;
}
</style>
