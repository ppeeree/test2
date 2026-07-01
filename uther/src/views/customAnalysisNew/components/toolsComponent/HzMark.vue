<template>
  <div class="btn_list_content">
    <button
      :class="{
        disableBtn: selectList.indexOf('BSF') < 0 && selectList.indexOf('BPFI') < 0
      }"
      v-if="btnList.includes('SF')"
      size="mini"
      title="边频"
      @click="clickBtn('SF')"
    >
      SF
    </button>
    <template v-for="item in btnList">
      <button
        v-if="item !== 'SF'"
        :key="item"
        :class="{ activeBtn: selectList.indexOf(item) !== -1 }"
        size="mini"
        :title="keyValue[item]"
        @click="clickBtn(item)"
      >
        {{ item }}
        <span v-if="btnIsTrue[item]" class="redDotMark"></span>
      </button>
    </template>
  </div>
</template>
<script>
import { getBearingDiagnoseResultApi } from '@/api/analysis/index.js'
export default {
  props: ['checkedMeasList'],
  data() {
    return {
      btnList: [],
      btnIsTrue: {},
      selectList: [],
      keyValue: {
        BPFI: '内圈',
        SF: '边频',
        RF: '转频',
        BPFO: '外圈',
        FTF: '保持架',
        BSF: '滚动体'
      }
    }
  },
  watch: {
    checkedMeasList: {
      handler(val) {
        this.selectList = []
        this.initBtnList()
      },
      deep: true,
      immediate: true
    }
  },
  mounted() {},
  methods: {
    initBtnList() {
      const { windturbineId, measlocId, time, sampleRate } = this.$parent.chartSource.find(
        i => i.name == this.checkedMeasList[0]
      ).others
      getBearingDiagnoseResultApi({
        WindturbineID: windturbineId,
        MeasLoctionID: measlocId,
        AcqTime: time,
        SampleRate: sampleRate
      }).then(res => {
        this.btnIsTrue = res.data.data
        let arr = []
        this.checkedMeasList.forEach(item => {
          if (item.indexOf('主轴') > -1 || item.indexOf('发电机') > -1) {
            arr = ['SF', 'RF', 'BPFI', 'BPFO', 'FTF', 'BSF']
          }
        })
        this.btnList = arr
      })
    },
    clickBtn(type) {
      if (type == 'SF') {
        this.$emit('handleChangeHzMark', type)
        return
      }
      if (this.selectList.indexOf(type) === -1) {
        this.selectList.push(type)
        this.$emit('handleChangeHzMark', type, 'add')
      } else {
        this.selectList.splice(this.selectList.indexOf(type), 1)
        this.$emit('handleChangeHzMark', type, 'cancel')
      }
    }
  }
}
</script>
<style lang="scss">
.btn_list_content {
  position: absolute;
  right: 4px;
  top: 28px;
  height: auto;
  width: 35px;
  button {
    background: #fff;
    float: left;
    color: #000;
    border: 1px solid #ccc;
    padding: 5px 2px;
    margin: 3px 0;
    width: 100%;
    font-size: 11px;
    border-radius: 2px;
    position: relative;
  }
  .redDotMark {
    position: absolute;
    top: -4px;
    right: -4px;
    width: 8px;
    height: 8px;
    background: red;
    border-radius: 50%;
    display: inline-block;
  }
  .activeBtn {
    background: #409eff;
  }
  .disableBtn {
    border: 1px solid #eee;
    color: #ccc;
    cursor: not-allowed;
  }
}
</style>
