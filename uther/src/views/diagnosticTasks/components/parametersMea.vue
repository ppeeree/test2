<template>
  <div class="windturbine_info_st">
    <div v-for="(item, index) in windturbineInfo" :key="index" class="windturbine_info_item">
      <span class="info_label">{{ item.label }}：</span>
      <div class="info_value">
        <span>{{ item.value }}{{ item.unit || '' }}</span>
      </div>
    </div>
  </div>
</template>

<script>
import cloneDeep from 'lodash/cloneDeep'

const defaultWindturbineInfo = [
  {
    label: '风机编号',
    value: '001',
    key: 'windTurbineName'
  },
  {
    label: '风场名称',
    value: '那仁风电场',
    key: 'windParkName'
  },
  {
    label: '主机厂家',
    value: '厂家',
    key: 'manufactory'
  },
  {
    label: '风机型号',
    value: 'SL1500/82',
    key: 'windTurbineType'
  },
  {
    label: '传动形式及传动比',
    value: '双馈/大连重工1:104.125',
    key: 'transmissionFormAndRatio'
  },
  {
    label: '样本数据发电机转速',
    value: '1860.33',
    unit: 'rpm',
    key: 'sampleDataSpeed'
  },
  {
    label: '发电机额定转速',
    value: '1800',
    unit: 'rpm',
    key: 'ratedGeneratorSpeed'
  }
]

export default {
  components: {},
  props: {},
  data() {
    return {
      windturbineInfo: cloneDeep(defaultWindturbineInfo)
    }
  },
  watch: {},
  mounted() {
    /*  this.$bus.$on('acqTimeReport', params => {})
    this.$bus.$on('clearParameter', () => {
      this.windturbineInfo = cloneDeep(defaultWindturbineInfo)
    })
    this.$bus.$on('acqTimeList', params => {}) */
  },
  methods: {
    initData(data) {
      this.windturbineInfoOriginal = data
      for (let i = 0; i < this.windturbineInfo.length; i++) {
        const key = this.windturbineInfo[i].key
        if (data.windTurbine.hasOwnProperty(key)) {
          this.windturbineInfo[i].value = data.windTurbine[key]
        }
      }
      // console.log(this.windturbineInfo)
    }
  }
}
</script>
<style lang="less" scoped>
.windturbine_info_st {
  display: flex;
  flex-direction: row;
  justify-content: space-around;
  align-items: center;
  width: 100%;
  border-bottom: 2px solid #ccc;
  padding: 10px 15px;
  .windturbine_info_item {
    display: flex;
    flex-direction: row;
    align-items: center;
    .info_label {
      font-size: 14px;
      font-weight: normal;
      color: #000;
    }
    .info_value {
      font-size: 14px;
      font-weight: bolder;
      color: #000;
    }
  }
}
</style>
