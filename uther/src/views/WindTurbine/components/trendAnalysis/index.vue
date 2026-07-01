<template>
  <div class="cms_model">
    <eigen-trend
      v-if="trendParams.length"
      id="trendDom"
      ref="trendDom"
      class="trendDom"
      :trendParams="trendParams"
      :filterData="filterData"
      :selectedComp="selectedComp"
      @getWave="getWave"
      :positionXY="trendPosition"
      v-on="$listeners"
      v-bind="$attrs"
      @changeAnalysisMode="changeAnalysisMode"
    />
    <template>
      <wave-trend
        ref="waveDom"
        class="waveDom"
        :trendParams="trendParams"
        :waveParams="waveParams"
        :filterData="filterData"
        :selectedComp="selectedComp"
        :positionXY="wavePosition"
        @getPosition="waveInitPosition"
      />
    </template>
  </div>
</template>
<script>
import waveTrend from './waveCurrent.vue'
import EigenTrend from './eigenTrend.vue'
// import dayjs from 'dayjs'
export default {
  components: {
    waveTrend,
    EigenTrend
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
    }
  },
  data() {
    return {
      index: 0,
      isFull: false,
      positionNum: {},
      waveParams: [],
      isWaveShow: false,
      trendPosition: {
        left: 0,
        top: 0
      },
      wavePosition: {
        left: 0,
        top: 0
      }
    }
  },
  watch: {
    /* trendParams: {
      handler(val) {
        this.$refs['waveDom'].visible = false
        this.judgeInitPosition(val.left, val.top)
      },
      deep: true
    } */
  },
  mounted() {
    this.$nextTick(() => {
      this.judgeInitPosition(this.trendParams[0].left, this.trendParams[0].top)
    })
  },

  methods: {
    getWave(param) {
      this.waveParams = param
      //  this.$refs['waveDom'].getwave(param)
    },
    judgeInitPosition(left, top) {
      let newLeft, newTop
      let bodyWidth = document.body.clientWidth
      let bodyHeight = document.body.clientHeight
      const dom = this.$refs['trendDom']
      let domWidth = dom.$el.clientWidth
      let domHeight = dom.$el.clientHeight
      if (left + domWidth / 2 + 10 > bodyWidth) {
        newLeft = left - domWidth
      } else if (left - domWidth / 2 - 10 < 0) {
        newLeft = left
      } else {
        newLeft = left - domWidth / 2
      }
      if (top + domHeight + 10 > bodyHeight) {
        newTop = top - domHeight - 10
      } else {
        newTop = top + 10
      }
      this.trendPosition = {
        left: newLeft,
        top: newTop
      }
    },
    waveInitPosition() {
      /*  let left = this.trendParams.left
      let top = this.trendParams.top */
      const { left, top } = this.trendPosition
      let newLeft, newTop, newWaveLeft, newWaveTop
      let bodyWidth = document.body.clientWidth
      let bodyHeight = document.body.clientHeight
      const dom = this.$refs['trendDom']
      const waveDom = this.$refs['waveDom']
      let domWidth = dom.$el.clientWidth
      let offsetLeft = dom.$el.offsetLeft
      let offsetTop = dom.$el.offsetTop
      // let domHeight = dom.$el.clientHeight
      let waveDomWidth = waveDom.$el.clientWidth
      let waveDomHeight = waveDom.$el.clientHeight
      if (bodyWidth - (domWidth + offsetLeft + 30) > waveDomWidth) {
        newLeft = offsetLeft
        newWaveLeft = domWidth + offsetLeft + 20
      } else {
        newLeft = bodyWidth - domWidth - waveDomWidth - 50
        newWaveLeft = bodyWidth - waveDomWidth - 10
      }
      /*   if (left + (domWidth + waveDomWidth + 30) / 2 + 10 > bodyWidth) {
        newLeft = bodyWidth - domWidth - waveDomWidth - 30
        newWaveLeft = bodyWidth - domWidth + 30
      } else if (left - (domWidth + waveDomWidth + 30) / 2 - 10 < 0) {
        newLeft = left
        newWaveLeft = left + domWidth + 30
      } else {
        newLeft = left - domWidth / 2
        newWaveLeft = left + domWidth / 2 + 30
      } */
      if (offsetTop + waveDomHeight + 30 > bodyHeight) {
        newWaveTop = bodyHeight - waveDomHeight - 30
      } else {
        newWaveTop = offsetTop
      }
      this.trendPosition = {
        left: newLeft,
        top: offsetTop
      }
      this.wavePosition = {
        left: newWaveLeft,
        top: newWaveTop
      }
    },
    // close
    changeAnalysisMode() {
      this.$emit('changeAnalysisMode')
    }
  }
}
</script>
<style lang="scss" scoped>
.cms_model {
  position: absolute;
  width: 100%;
  height: 100%;
  pointer-events: none;
  left: 0;
  top: 0;
  .trendDom {
    pointer-events: auto;
  }
  .waveDom {
    width: auto;
    pointer-events: auto;
  }
}
</style>
