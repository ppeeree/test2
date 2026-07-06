<template>
  <div class="dialog_content" ref="dialog_content" v-drag v-if="visible && selectedComp == 'NAC'">
    <div class="dialog_header">
      {{
        trendParams && trendParams.length
          ? trendParams[0].evCardTitle || trendParams[0].measlocName
          : ''
      }}
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
      <chart-dom
        ref="TimeDomain"
        chartType="TimeDomain"
        :widthSize="size2.width"
        :heightSize="size2.height"
        :requestParam="requestParam"
        v-slot="{ waveInfo }"
      >
        <!--   <div style="color: #fff; font-size: 12px; padding: 3px 10px">
          <span>
            采集时间：
            <b>{{ waveInfo.time }}</b>
          </span>
        </div> -->
      </chart-dom>
      <chart-dom
        ref="FreqDomain"
        chartType="FreqDomain"
        :widthSize="size3.width"
        :heightSize="size3.height"
        :requestParam="requestParam"
      >
      </chart-dom>
    </div>
  </div>
</template>
<script>
import { defineAsyncComponent } from 'vue'

export default {
  components: {
    chartDom: defineAsyncComponent(() => import('@/components/diagnosisChart/wave.vue'))
    // chartDom: () => import('./wave/index.vue')
  },
  props: {
    trendParams: {
      type: Array,
      require: true,
      default() {
        return []
      }
    },
    waveParams: {
      type: [],
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
    }
  },
  data() {
    return {
      isFull: false,
      visible: false,
      positionNum: {},
      size2: { width: 700, height: 530 },
      size3: { width: 700, height: 530 },
      measlocName: '',
      requestParam: []
    }
  },
  watch: {
    positionXY: {
      handler(val) {
        this.setPosition()
      },
      deep: true
    },
    waveParams: {
      handler(val) {
        this.changeWave(val)
      },
      deep: true,
      immediate: true
    }
  },
  mounted() {},
  beforeUnmount() {},
  methods: {
    changeWave(param) {
      if (!param.length) {
        this.visible = false
        return
      }
      if (this.selectedComp !== 'NAC') {
        return
      }
      this.visible = true
      this.$nextTick(() => {
        this.$emit('getPosition')
        this.setPosition()
      })
      this.requestParam = param
      /*  const { coord, name } = clickedObj
      let arr = name.split('&&')[0].split('&')
      this.$set(this.requestParam, 0, {
        acqTime: coord[0],
        measlocId: '',
        windturbineId: arr[0],
        measlocCode: arr[1]
      }) */
    },
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
      dom.style.height = 'calc(100% - 120px)'
      this.size2.width = newWidth
      this.size3.width = newWidth
    },
    narrow() {
      this.isFull = false
      const dom = this.$refs['dialog_content']
      dom.style.left = this.positionNum.left
      dom.style.top = this.positionNum.top
      let newWidth = 700
      dom.style.width = newWidth + 'px'
      dom.style.height = '530px'
      this.size2.width = newWidth
      this.size3.width = newWidth
    },
    // close
    closeModal() {
      this.visible = false
      // this.$emit('changeAnalysisMode')
    }

    // 过滤波形频率接口
    /*  filterFreWave(param) {
      const { up, down, type } = param
      const { turbineId } = this.trendParams
      postFilterFrequency({
        a: down,
        b: up,
        filterPassType: type,
        acqTime: '2022-10-29 19:41:37', //param.value[0],
        measlocId: 'TWP001001NACMSRR0OC',
        windturbineId: turbineId,
        measlocCode: ''
        // evId: 'TWP001001NACGBXHSSA0OC&&0-5000_aCF'
      }).then(res => {
        if (res.data.code === 200 && res.data.data.length) {
          this.syWaveData = {
            ...this.syWaveData,
            source: res.data.data.dataVOS
          }
        } else {
          this.$message.warning('过滤失效，未返回本次频率范围的波形数据！')
        }
      })
    }, */
    // 故障频率接口
    /*    getDamageFailureDataFunc(param) {
      getDamageFailureDatas({
        ...param,
        id: 1
      }).then(res => {
        if (res.data.code === 200) {
          this.$refs['fftwave'].watchFaultData(res.data.data)
        } else {
          this.$message.warning('无数据')
        }
      })
    } */
  }
}
</script>
<style lang="scss" scoped>
.dialog_content {
  position: absolute;
  z-index: 1000;
  width: 670px;
  height: 530px;
  border: 1px solid #b9cdd4;
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
    max-height: calc(100% - 40px);
    overflow-x: hidden;
    overflow-y: auto;
    height: auto;
    background: #232121;
    position: relative;
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
      background: #252526;
      /*  background: rgba(0, 43, 88, 0.4) !important;
      backdrop-filter: blur(10px); */
    }
    :deep(.table_info){
      background: #252526;
      /*  background: rgba(0, 43, 88, 0.4) !important;
      backdrop-filter: blur(10px); */
    }
  }
}
</style>
