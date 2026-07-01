<!-- 拉索力专项分析 -->
<template>
  <div class="special-analysis" data-key="trend">
    <div :id="type + 'specialAnalysis'" style="position: relative; width: 100%; height: 100%">
      <one-group
        @getWavePointer="getWavePointer"
        :keyid="type"
        :ref="type"
        :titleText="titleText"
        :evIdList="evIdList"
        :setSteelNumber="setSteelNumber"
        :filterParam="filterParam"
        @getImgData="getImgData"
        @getSelectValue="getSelectValue"
      />
    </div>
  </div>
</template>
<script>
import oneGroup from './groupAnalysis.vue'
import splitter from '../splitter.vue'
import { rowResize, colResize, tabName } from '../toolsComponent/tools.js'
import { getGroupTrendData } from '@/api/analysis/index'

export default {
  components: {
    oneGroup,
    splitter
  },
  props: ['type', 'treeDataAndFilterP'],
  watch: {
    treeDataAndFilterP: {
      handler(newVal, oldVal) {
        let newClickedList = newVal?.checkedMeasList || []
        let oldClickedList = oldVal?.checkedMeasList || []
        let newfilterParam = newVal?.filterParam || {}
        let oldfilterParam = oldVal?.filterParam || {}
        if (
          !_.isEqual(newClickedList, oldClickedList) ||
          !_.isEqual(newfilterParam, oldfilterParam)
        ) {
          this.wavePointerDataListObj = {}
          if (!newClickedList.length) {
            return this.$message.warning('请勾选需要查询的数据！')
          }
          if (
            !_.isEqual(newClickedList, oldClickedList) &&
            _.isEqual(newfilterParam, oldfilterParam)
          ) {
            this.$emit('getWavePointer', [])
            this.getGroupList()
          } else if (
            _.isEqual(newClickedList, oldClickedList) &&
            !_.isEqual(newfilterParam, oldfilterParam)
          ) {
            this.filterParam = newfilterParam
          } else if (
            !_.isEqual(newClickedList, oldClickedList) &&
            !_.isEqual(newfilterParam, oldfilterParam)
          ) {
            this.$emit('getWavePointer', [])
            this.getGroupList()
            setTimeout(() => {
              this.$nextTick(() => {
                this.filterParam = newfilterParam
              })
            }, 1000)
          }
        } else {
          // return this.$message.warning('与上次查询数据和条件一致！')
          // 应刚伟要求（采集数据需要实时刷新，手动刷新页面太麻烦），修改成再次请求，刷新页面数据
          this.$emit('getWavePointer', [])
          this.filterParam = newfilterParam
        }
      },
      deep: true,
      immediate: true
    }
  },
  data() {
    return {
      wavePointerDataListObj: {},
      evIdList: [],
      setSteelNumber: {}, //{windtrurbineID:cbfNumber}
      titleText: '',
      filterParam: {}
    }
  },
  created() {},
  mounted() {
    this.filterParam = this.treeDataAndFilterP.filterParam
    this.titleText = tabName[this.type] + '趋势分析'
    this.$nextTick(() => {
      setTimeout(() => {
        colResize(this.type + 'specialAnalysis')
      }, 100)
    })
  },
  methods: {
    getSelectValue(val) {
      this.$emit('changeSelectedEigenValue', val)
    },
    getGroupList() {
      const { checkedMeasList } = this.treeDataAndFilterP
      getGroupTrendData({
        GTCAttributes: checkedMeasList,
        GTCType: this.type
      }).then(res => {
        if (res.data.code === 200) {
          if (!res.data.data.length) return
          let arr = []
          let setSteelNumber = {}
          res.data.data.forEach((i, index) => {
            if (!setSteelNumber[checkedMeasList[index].windturbineID]) {
              setSteelNumber[checkedMeasList[index].windturbineID] = i.cbfNumber
            }
            i.children.forEach(j => {
              arr = arr.concat(j.ids)
            })
          })
          this.evIdList = arr
          this.setSteelNumber = setSteelNumber
        }
      })
    },
    getWavePointer(key, data) {
      this.wavePointerDataListObj[key] = data
      this.$emit('getWavePointer', [
        ...new Set(Object.values(this.wavePointerDataListObj).flatMap(val => val))
      ])
    },
    currentWaveInfoDataListOut() {
      this.$emit('getWavePointer', [
        ...new Set(Object.values(this.wavePointerDataListObj).flatMap(val => val))
      ])
    },
    initSelectedEigenValue() {
      if (this.groupList && this.groupList.length) {
        this.$emit(
          'changeSelectedEigenValue',
          this.$refs[this.groupList[0].key][0].getSelctedChartEigenValue()
        )
      }
    },
    getImgData(data) {
      this.$emit('getImgData', data)
    }
  }
}
</script>
<style lang="scss" scoped>
.special-analysis {
  width: 100%;
  height: 100%;
  position: relative;
  background: #fff;
}
</style>
