<template>
  <div class="content-left">
    <el-card class="box-card" :body-style="{ padding: '10px' }">
      <div slot="header" class="title">
        <div class="top_title">
          <span class="titleChart">风场概况</span>
          <span class="title_buttom_line"></span>
        </div>
      </div>
      <div style="width: 450px" class="item-style">
        <part-1 ref="part1" :isControl="isControl" />
        <div class="top_title">
          <span class="titleChart">机组健康状态</span>
          <span class="title_buttom_line" style="width: calc(100% - 153px)"></span>
        </div>
        <iconChart
          :style="[{ left: '402px' }, handleIconBorder('groupCard')]"
          @click.native="handleOpenChart('groupCard')"
        ></iconChart>
        <part-4 ref="part4" :isControl="isControl" style="position: relative; height: 31%" />
        <!-- 部件健康状态 -->
        <div class="top_title">
          <span class="titleChart">部件报警状态</span>
          <span class="title_buttom_line" style="width: calc(100% - 153px)"></span>
        </div>
        <iconChart
          :style="[{ left: '402px' }, handleIconBorder('stateCard')]"
          @click.native="handleOpenChart('stateCard')"
        ></iconChart>
        <part-5 ref="part5" :isControl="isControl" />
      </div>
    </el-card>
    <div ref="groupCard" v-if="groupCard" class="card-1" style="top: 30%">
      <i @click="handleOpenChart('groupCard')" class="el-icon-close"></i>
      <line-category
        v-if="!showHealthPartSeret"
        :unique="1"
        title="健康状态发展趋势"
        :seriesData="healthPartSeretData"
      />
      <noData v-else firstText="暂无数据" noteText="请联系管理员检查数据采集服务或者网络链接" />
    </div>
    <div ref="stateCard" v-if="stateCard" class="card-2" style="top: 66%">
      <i @click="handleOpenChart('stateCard')" class="el-icon-close"></i>
      <line-chart v-if="partSeretData.series?.length" :chartData="partSeretData" />
      <noData v-else firstText="暂无数据" noteText="请联系管理员检查数据采集服务或者网络链接" />
    </div>
  </div>
</template>

<script>
import { getFaultTrend, healthStatusTrend } from '@/api/screen/leftCardApi'
import { faultTrendEnum } from '@/util/constant'
import { levelColorEnum } from '@/util/constant'
import { mapGetters } from 'vuex'
import part1 from '../charts/newpart1.vue'
import part4 from '../charts/part4.vue'
import part5 from '../charts/newpart5.vue'

export default {
  components: {
    part1, // () => import('../charts/newpart1.vue'),
    part4, // () => import('../charts/part4.vue'),
    part5, //() => import('../charts/newpart5.vue'),
    lineCategory: () => import('../base/line-category.vue'),
    iconChart: () => import('../charts/iconChart.vue'),
    noData: () => import('@/components/noData/index.vue'),
    lineChart: () => import('@/components/charts/baseChart.vue')
  },
  inject: ['parent'],
  data() {
    return {
      groupCard: false,
      stateCard: false,
      synthesis: false,
      partsLegend: [],
      healthPartsLegend: [],
      dronesOrNestsLegend: Object.freeze(['无人机', '机巢']),
      partSeretData: [],
      healthPartSeretData: [],
      faultTrendEnum
    }
  },
  watch: {
    stateCard: {
      handler: function (val) {
        val && this.getFaultTrend()
      }
    },
    'parent.WindFarm': {
      handler: function (val) {
        this.init()
      },
      deep: true
    }
  },
  computed: {
    ...mapGetters(['userInfo'])
  },
  mounted() {
    window.addEventListener('setItem', () => {
      if (sessionStorage.getItem('clickChart') === 'rightChart') {
        const key = ['groupCard', 'stateCard', 'synthesis']
        key.forEach(item => {
          if (this[item]) {
            this.handleOpenChart(item)
          }
        })
      }
    })
    /*  this.$nextTick(() => {
      this.init()
    }) */
  },

  methods: {
    init() {
      this.healthStatusTrend()
      this.$refs['part1'] && this.$refs['part1'].initData()
      this.$refs['part5'] && this.$refs['part5'].getFaultStatistic()
      this.$refs.part4 && this.$refs.part4.init()
    },
    chartClick(index) {
      console.log(index)
    },
    handleOpenChart(key) {
      if (this[key]) {
        this.$refs[key].classList.contains('openShow') &&
          this.$refs[key].classList.remove('openShow')
        this.$refs[key].classList.add('closeHide')
        setTimeout(() => (this[key] = !this[key]), 300)
      } else {
        this[key] = !this[key]
        this.$nextTick(() => {
          this.setSessionItem('clickChart', 'leftChart')
          this.$refs[key].classList.contains('closeHide') &&
            this.$refs[key].classList.remove('closeHide')
          this.$refs[key].classList.add('openShow')
        })
      }
    },

    async getFaultTrend() {
      let params = this.parent.isControl
        ? { userID: this.userInfo.user_id }
        : {
            stationId: this.parent.WindFarm.id
          }
      const { data: res } = await getFaultTrend(params)
      const { faultStatusList, time } = res.data
      let legendData = [],
        dataList = []
      faultStatusList.forEach((item, index) => {
        const { compName, faultCount } = item
        legendData.push(compName)
        let newData = faultCount.map((i, index) => {
          return [time[index], ...i]
        })
        dataList.push({
          name: compName,
          type: 'line',
          showSymbol: true,
          symbol: 'circle',
          symbolSize: 5,
          smooth: false,
          data: newData
        })
      })
      this.partSeretData = {
        title: {
          text: '部件报警数量发展趋势',
          textStyle: {
            fontSize: '16',
            color: '#fff',
            fontFamily: 'Lato',
            foontWeight: '500'
          },
          top: '5%',
          left: '2%'
        },
        tooltip: {
          trigger: 'item',
          formatter: param => {
            let htmlStr = ''
            const { marker, value, seriesName, name } = param
            htmlStr += `${name}<br/><div>${seriesName}：${value[1]}`
            //  htmlStr += `${marker}${seriesName}：${value[1]}`
            if (value[1]) {
              htmlStr += '<br/><span style="margin-left:10px"></span>'
            }
            if (value[2]) {
              htmlStr += `<span style="margin-right:5px;display:inline-block;width:10px;height:10px;border-radius:5px;background-color:#FF0F0D;"></span>危险：${value[2]}`
            }
            if (value[3]) {
              htmlStr += `<span style="margin-right:5px;display:inline-block;width:10px;height:10px;border-radius:5px;background-color:#ff6b0e;"></span>警告：${value[3]}`
            }
            if (value[4]) {
              htmlStr += `<span style="margin-right:5px;display:inline-block;width:10px;height:10px;border-radius:5px;background-color:#ffe604;"></span>注意：${value[4]}`
            }
            htmlStr += '</div>'
            return htmlStr
          }
        },
        toolbox: {
          show: false
        },
        legend: {
          icon: 'rect',
          itemHeight: 4,
          itemWidth: 20,
          data: legendData,
          textStyle: {
            color: '#fff'
          },
          top: '5%'
        },
        grid: {
          left: '3%',
          right: '4%',
          bottom: '3%',
          containLabel: true
        },
        xAxis: {
          type: 'category',
          boundaryGap: false,
          data: time,
          axisLine: {
            show: true, //隐藏X轴轴线
            lineStyle: {
              color: '#fff'
            }
          },
          axisTick: {
            show: false //隐藏X轴刻度
          },
          axisLabel: {
            show: true,
            textStyle: {
              color: '#fff' //X轴文字颜色
            }
          }
        },
        yAxis: {
          type: 'value',
          axisLine: {
            show: false,
            lineStyle: {
              color: '#fff'
            }
          },
          splitLine: {
            show: true,
            lineStyle: {
              type: 'dashed',
              color: '#9EA3B4',
              opacity: 0.3
            }
          },
          axisLabel: {
            color: '#fff'
          },

          axisTick: {
            show: false
          }
        },
        series: dataList
      }
    },
    handleIconBorder(e) {
      return {
        border: `1px solid ${this[e] ? 'rgb(31, 255, 255)' : 'transparent'}`
      }
    },
    async healthStatusTrend() {
      let params = this.parent.isControl
        ? { userID: this.userInfo.user_id, stationId: '' }
        : {
            userID: '',
            stationId: this.parent.WindFarm.id
          }
      const { data } = await healthStatusTrend(params)
      if (!data.data) return
      this.healthPartSeretData = Array.from(data.data, i => ({
        ...i,
        color: levelColorEnum[i.statusCode]
      }))
    }
  }
}
</script>

<style lang="less" scoped>
@import url('../style/minxs.less');
@chart-width: 840px;
@icon-left: 423px;

.card-chart(@top, @height: 320px) {
  position: absolute;
  top: @top;
  left: 101%;
  width: @chart-width;
  height: @height;
  background: rgba(4, 17, 33, 0.5);
  backdrop-filter: blur(15px);
}

.icon-class() {
  position: absolute;
  left: 96%;
  top: 10px;
  font-size: 16px;
  color: #ffffff;
  cursor: pointer;
  z-index: 10;
}

::v-deep .el-card__header {
  border-bottom: none;
}
::v-deep .el-card__body {
  height: calc(100% - 40px);
}
.item-style {
  height: 100%;
  div:nth-child(4) {
    margin-top: 6px;
  }
}
.content-left {
  overflow: hidden;
  height: 104%;
  .icon-1 {
    left: @icon-left;
    top: 163px;
  }
  .icon-2 {
    left: @icon-left;
  }
  .icon-3 {
    left: @icon-left;
  }
  .card-1 {
    .card-chart(4%);
    i {
      .icon-class();
    }
  }
  .card-2 {
    .card-chart(36%);
    i {
      .icon-class();
    }
  }
  .card-3 {
    .card-chart(68%, 296px);
    i {
      .icon-class();
    }
  }
}
.openShow {
  animation: left 0.3s ease-in both;
}
.closeHide {
  animation: right 0.3s ease-in both;
}
@keyframes left {
  from {
    width: 0;
  }
  to {
    width: @chart-width;
  }
}
@keyframes right {
  from {
    width: @chart-width;
  }
  to {
    width: 0;
  }
}
.title {
  position: relative;
  top: 10px;
}
.title_class {
  // margin-top: 10px;
}
</style>
