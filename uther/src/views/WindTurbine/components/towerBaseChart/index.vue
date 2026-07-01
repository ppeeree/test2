<template>
  <div class="scatterDiagram" ref="scatterDiagram" v-if="visible">
    <div class="scatterDiagram_title">
      塔筒倾角分布图
      <span @click="closeModal">
        <i class="el-icon-close"></i>
      </span>
    </div>
    <div class="scatterDiagram_content">
      <trend-chart
        chartDomId="scatter"
        titleText="塔筒倾角分布图"
        :dataSourse="dataSourse"
        theme="dark"
        chartType="OA"
        isMultipleCLick="none"
        @clickEvent="trendClickEvent"
        :loading="loading"
      />
    </div>
  </div>
</template>
<script>
import { getEvAnalyzerDataApi } from '@/api/analysis/index.js'
export default {
  components: {
    trendChart: () => import('@/components/diagnosisChart/trend.vue')
  },
  props: {
    trendParams: {
      type: Object,
      require: true,
      default: () => {
        return {}
      }
    },
    filterData: {
      type: Object,
      require: true,
      default() {
        return {}
      }
    }
  },
  data() {
    return {
      loading: false,
      chartData: {},
      visible: false,
      dataSourse: {
        data: [],
        titleText: '倾角分布图'
      }
      //  towerScatterData: []
    }
  },
  inject: ['closeDipModel'],
  watch: {
    filterData: {
      handler(val, old) {
        if (JSON.stringify(val) != JSON.stringify(old) && this.visible) {
          this.getTrendData()
        }
      },
      deep: true
    },
    trendParams: {
      handler() {
        if (this.visible) {
          this.getTrendData()
        }
      },
      deep: true
    }
  },
  created() {},
  mounted() {},
  methods: {
    getTrendData() {
      this.chartData = {}
      let { time, ...others } = this.filterData
      if (!time) {
        return
      }
      this.loading = true
      const { objArr, turbineId } = this.trendParams
      getEvAnalyzerDataApi({
        analyzeWay: 'OA',
        endTime: time[1],
        startTime: time[0],
        evCode: '',
        measlocCode: '',
        eigenValueIds: [],
        windturbineIds: [turbineId],
        wkCond: {
          ...others
        }
      })
        .then(res => {
          if (res.data.code === 200 && res.data.data) {
            if (res.data.data.evdataList && res.data.data.evdataList.length) {
              this.dataSourse.data = this.dealTrendData(res.data.data)
            } else {
              this.$message.warning('无数据')
            }
          }
          this.loading = false
        })
        .catch(error => {
          // this.$message.warning(error)
          this.loading = false
        })
    },
    // 特征值趋势返回值数据处理
    dealTrendData(data) {
      const { evdataList } = data
      let lineDatas = []
      if (evdataList && evdataList.length) {
        evdataList.forEach((element, index) => {
          const {
            windturbineName,
            windParkName,
            evName,
            sampleRate,
            measlocName,
            vdiMax,
            vdiMin,
            evdata,
            evId,
            unitX,
            unitY
          } = element
          let name = `${measlocName}_${windturbineName}_${windParkName}`
          lineDatas.push({
            source: evdata,
            name: name,
            sourceName: name,
            VDI: vdiMin === null ? [] : [vdiMin, vdiMax],
            id: evId + '~' + sampleRate,
            dimensions: [unitX, unitY],
            other: {
              ...element,
              evdata: []
            }
          })
        })
        return lineDatas
      } else {
        return []
      }
    },
    closeModal() {
      this.closeDipModel()
    }
  }
}
</script>
<style lang="scss" scoped>
.scatterDiagram {
  position: absolute;
  left: calc(50% - 250px);
  top: calc(50% - 250px);
  width: 500px;
  height: 500px;
  border: 1px solid #8a989e;
  // background: #052c42;
  z-index: 99;
  .scatterDiagram_title {
    width: 100%;
    height: 40px;
    line-height: 40px;
    background: #0d326a;
    font-size: 18px;
    color: #fff;
    text-align: left;
    padding-left: 15px;
    span {
      font-size: 24px;
      font-weight: bolder;
      cursor: pointer;
      float: right;
      margin-right: 15px;
    }
  }
  .scatterDiagram_content {
    width: 100%;
    height: calc(100% - 40px);
    background: #000;
    backdrop-filter: blur(10px);
    position: relative;
  }
}
</style>
