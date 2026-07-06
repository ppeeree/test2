<template>
  <div class="dialog_content" ref="dialog_content" v-drag>
    <div class="dialog_header">
      特征值趋势图
      <span @click="closeModal">
        <i class="el-icon-close"></i>
      </span>
      <b @click="enlarge" v-if="!isFull">
        <i class="el-icon-full-screen"></i>
      </b>
      <b @click="narrow" v-if="isFull">
        <i class="el-icon-copy-document"></i>
      </b>
    </div>
    <div class="dialog_body">
      <trend-chart
        :chartDomId="`${trendParams[0].eigenValueId}`"
        class="trend_anlysis"
        titleText="特征值趋势"
        :dataSourse="dataSourse"
        theme="dark"
        chartType="TA"
        :isMultipleCLick="selectedComp == 'NAC' ? 'single' : 'none'"
        @clickEvent="trendClickEvent"
        :loading="loading"
      />
    </div>
  </div>
</template>
<script>
import { getEvAnalyzerDataApi } from '@/api/analysis/index.js'
import { dealTrendData } from '@/util/transfrom.js'
import { defineAsyncComponent } from 'vue'
export default {
  components: {
    trendChart: defineAsyncComponent(() => import('@/components/diagnosisChart/trend.vue'))
  },
  props: {
    trendParams: {
      type: Array,
      require: true,
      default() {
        return []
      }
    },
    filterData: {
      type: Object,
      require: true,
      default() {
        return {}
      }
    },
    selectedComp: {
      type: String,
      require: true,
      default: 'turbine'
    },
    positionXY: {
      type: Object,
      require: true,
      default() {
        return {
          left: 0,
          top: 0
        }
      }
    },
    windTurbineId: {
      type: String,
      default: ''
    }
  },
  data() {
    return {
      dataSourse: {
        data: [],
        xType: 'time',
        yType: 'value',
        titleText: '趋势分析'
      },
      myChart: null,
      myChartInstance: null,
      loading: false,
      index: 0,
      isFull: false,
      positionNum: {},
      size1: { width: 700, height: 350 }
    }
  },
  watch: {
    trendParams: {
      handler() {
        this.getTrendDataFunc(() => {})
      },
      deep: true
    },
    filterData: {
      handler(val, old) {
        if (JSON.stringify(val) != JSON.stringify(old)) {
          this.getTrendDataFunc()
        }
      },
      deep: true,
      immediate: false
    },
    positionXY: {
      handler() {
        this.setPosition()
      },
      deep: true
    }
  },
  mounted() {
    this.getTrendDataFunc()
    this.$nextTick(() => {
      this.setPosition()
    })
  },
  methods: {
    setPosition() {
      const dom = this.$refs['dialog_content']
      dom.style.left = this.positionXY.left + 'px'
      dom.style.top = this.positionXY.top + 'px'
    },
    enlarge() {
      this.isFull = true
      const dom = this.$refs['dialog_content']
      this.positionNum = {
        left: dom.style.left,
        top: dom.style.top
      }
      let windowWidth = document.documentElement.clientWidth || document.body.clientWidth
      dom.style.left = 100 + 'px'
      dom.style.top = 100 + 'px'
      let newWidth = windowWidth - 200
      dom.style.width = newWidth + 'px'
    },
    narrow() {
      this.isFull = false
      const dom = this.$refs['dialog_content']
      dom.style.left = this.positionNum.left
      dom.style.top = this.positionNum.top
      let newWidth = 700
      dom.style.width = newWidth + 'px'
    },
    judgeInitPosition(left, top) {
      let newLeft, newTop
      let bodyWidth = document.body.clientWidth
      let bodyHeight = document.body.clientHeight
      // console.log(this.$refs)
      const dom = this.$refs['dialog_content']
      let domWidth = dom.clientWidth
      let domHeight = dom.clientHeight
      if (left + domWidth / 2 + 10 > bodyWidth) {
        newLeft = left - domWidth
      } else if (left - domWidth / 2 - 10 < 0) {
        newLeft = left + domWidth
      } else {
        newLeft = left - domWidth / 2
      }
      if (top + domHeight + 10 > bodyHeight) {
        newTop = top - domHeight - 10
      } else {
        newTop = top + 10
      }
      dom.style.left = newLeft + 'px'
      dom.style.top = newTop + 'px'
    },
    // close
    closeModal() {
      this.$emit('changeAnalysisMode')
    },

    // 特征值趋势接口
    getTrendDataFunc(callback) {
      this.loading = true
      const { time, ...others } = this.filterData
      if (!time) {
        return
      }
      const eigenValueIds = this.trendParams.map(item => {
        return item.evID
      })
      getEvAnalyzerDataApi({
        analyzeWay: 'TA',
        eigenValueIds: eigenValueIds,
        endTime: time[1],
        startTime: time[0],
        measlocCode: '',
        evCode: '',
        windturbineIds: [],
        wkCond: {
          ...others
        }
      }).then(res => {
        if (res.data.code == 200 && res.data.data) {
          let arr = dealTrendData(res.data.data)
          this.dataSourse.data = arr
          if (!this.dataSourse.data.length) {
            this.$message({
              message: '未查询到数据',
              type: 'warning',
              duration: 1000
            })
          }
          if (callback) {
            callback()
          }
          this.loading = false
        }
      })
    },
    trendClickEvent(data /* symbolArr, clickedObj, type */) {
      this.$emit('getWave', data /* { symbolArr, clickedObj, type } */)
    }
  }
}
</script>
<style lang="scss" scoped>
.dialog_content {
  position: absolute;
  z-index: 1000;
  width: 700px;
  height: 382px;
  border: 1px solid #8a989e;
  border-radius: 2px;
  .dialog_header {
    width: 100%;
    height: 40px;
    line-height: 40px;
    color: #fff;
    padding: 0 15px;
    background: #0d326a;
    span {
      font-size: 24px;
      font-weight: bolder;
      cursor: pointer;
      float: right;
      margin-right: 15px;
    }
    b {
      font-size: 24px;
      font-weight: bolder;
      cursor: pointer;
      float: right;
      margin-right: 15px;
    }
  }
  .dialog_body::-webkit-scrollbar {
    width: 5px;
  }
  .dialog_body::-webkit-scrollbar-track {
    background-color: #3e5369;
  }
  .dialog_body::-webkit-scrollbar-thumb {
    box-shadow: inset 0 0 6px #d2e5f1;
  }
  .dialog_body {
    width: 100%;
    height: calc(100% - 40px);
    overflow-x: hidden;
    overflow-y: auto;
    position: relative;
    // height: auto;
    background: #232121;
    .left_chartTool {
      position: absolute;
      top: 5px;
      right: 10px;
      z-index: 10;
    }

    .content_box_bottom {
      width: 100%;
      height: 40px;
      line-height: 20px;
      padding: 0px 10px;
      /*  background: rgba(0, 43, 88, 0.4);
      backdrop-filter: blur(10px); */
      background: #252526;
      ul {
        color: #c2c2c2;
        width: 100%;
        height: 100%;
        overflow-x: hidden;
        overflow-y: auto;
        li {
          cursor: pointer;
          padding: 0 8px;
          color: #e2e2e2;
          font-size: 12px;
          float: left;
          &:hover {
            background: #333947;
            border-radius: 6px;
          }
          span {
            display: inline-block;
            width: 10px;
            height: 6px;
            margin-right: 5px;
            opacity: 0.4;
            margin-bottom: 2px;
          }
        }
        .active_li {
          color: #fff;
          span {
            opacity: 1;
          }
        }
      }
    }
    .trend_anlysis {
      width: 100%;
      height: calc(100% - 40px);
    }
    .wave_title {
      width: 100%;
      height: 30px;
      color: #fff;
      text-align: center;
      background: #061932;
      line-height: 30px;
      font-size: 16px;
    }
    :deep(.linePage){
      background: transparent;
    }
    :deep(.chartbox){
      /*       background: rgba(0, 43, 88, 0.4) !important;
      backdrop-filter: blur(10px); */
    }
  }
}
</style>
