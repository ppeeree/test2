<template>
  <div class="content-box chart_module" id="rightArea">
    <diagnosticRecord
      v-if="dialogVisible"
      class="lightTheme"
      :dialogVisible="dialogVisible"
      dialogTitle="诊断记录"
      :componentFromCompleteData="recordChartInfo"
      :imgUrl="imgUrlDialog"
      :key="recordChartType"
      :chartType="recordChartType"
      @handleDialogVisible="handleDialogVisibleRec"
    />
    <!-- 主视图 -->
    <div class="rowbox" style="height: 49.5%" key="row0" id="row0">
      <div itemId="row0-0" rowId="row0" style="width: 100%; height: 100%">
        <el-tabs type="border-card" style="width: 100%; height: 100%" v-model="activeName">
          <el-tab-pane
            v-for="item in tabArrList"
            :key="item.key"
            :label="item.label"
            :name="item.key"
            style="width: 100%; height: 100%"
          >
            <keep-alive>
              <component
                style="padding: 0 10px"
                :is="item.components"
                :type="item.key"
                :ref="item.key"
                v-if="item.key === activeName"
                :treeDataAndFilterP="treeDataAndFilterP"
                :waveDetailInfoList="waveInfoList"
                @changeSelectedEigenValue="changeSelectedEigenValue"
                @getWavePointer="
                  data => {
                    getWavePointerTab1(data)
                  }
                "
                @getImgData="getImgData"
                @syncDataTimeRange="syncDataTimeRange"
              />
            </keep-alive>
          </el-tab-pane>
        </el-tabs>
      </div>
    </div>
    <Splitter key="splitH0" splitType="horizontal" id="splitH0" :limit="{}" style="flex: 0 0 4px" />
    <!-- 辅助视图 可配置保存-->
    <template v-for="(item, index) in downLayout">
      <div
        class="rowbox"
        :style="{
          flex: `0 0 ${item.rowHeight + '%'}`
          // height: item.rowHeight + '%'
        }"
        :key="index + 1"
        :id="'row' + (index + 1)"
      >
        <template v-for="(ii, i) in item.charts">
          <emptyChart
            :type="activeName"
            :key="'row' + activeName + '-' + i"
            :itemId="'row' + (index + 1) + '-' + i"
            :ref="'row' + (index + 1) + '-' + i"
            :rowId="'row' + (index + 1)"
            :chartInitType="ii.chartType || 'empty'"
            :initWidth="ii.chartWidth + '%'"
            :acqPointerInfoList="acqPointerInfoList"
            :treeDataAndFilterP="treeDataAndFilterP"
            :selectedEigenValue="selectedEigenValue"
            @addChart="addInsertChart"
            @removeChart="removeChart"
            @removeRow="removeRow"
            @getImgData="getImgData"
          />
          <Splitter
            :key="'splitV' + i"
            v-if="i !== item.charts.length - 1"
            splitType="vertical"
            :id="'splitV' + (index + 1) + '-' + i"
            :limit="{}"
            style="flex: 0 0 4px"
          />
        </template>
      </div>
      <Splitter
        :key="'splitH' + (index + 1)"
        v-if="index !== downLayout.length - 1"
        splitType="horizontal"
        :id="'splitH' + (index + 1)"
        :limit="{}"
        style="flex: 0 0 4px"
      />
    </template>
    <!--   <template v-for="(item, index) in layout">
      <div
        class="rowbox"
        :style="{
          height: item.rowHeight + '%'
        }"
        :key="index"
        :id="'row' + index"
      >
        <template v-for="(ii, i) in item.charts">
          <div
            v-if="ii.chartType == 'trend'"
            :itemId="'row' + index + '-' + i"
            :rowId="'row' + index"
            :key="i"
            :style="{
              width: ii.chartWidth + '%',
              height: '100%'
            }"
          >
            <el-tabs
              type="border-card"
              style="width: 100%; height: 100%"
              v-model="activeName"
              @tab-click="handleClick"
            >
              <el-tab-pane
                v-for="item in tabArrList"
                :key="item.key"
                :label="item.label"
                :name="item.key"
                style="width: 100%; height: 100%"
              >
                <keep-alive>
                  <component
                    :key="item.key"
                    :is="item.components"
                    v-if="activeName === item.key"
                    :type="item.key"
                    :ref="item.key"
                    :treeDataAndFilterP="treeDataAndFilterP"
                    :waveDetailInfoList="waveInfoList"
                    @changeSelectedEigenValue="changeSelectedEigenValue"
                    @getWavePointer="
                      data => {
                        getWavePointerTab1(data)
                      }
                    "
                    @getImgData="getImgData"
                    @syncDataTimeRange="syncDataTimeRange"
                  />
                </keep-alive>
              </el-tab-pane>
            </el-tabs>
          </div>
          <emptyChart
            v-else
            :type="activeName"
            :key="'row' + activeName + '-' + i"
            :itemId="'row' + index + '-' + i"
            :ref="'row' + index + '-' + i"
            :rowId="'row' + index"
            :chartInitType="ii.chartType || 'empty'"
            :initWidth="ii.chartWidth + '%'"
            :acqPointerInfoList="acqPointerInfoList"
            :treeDataAndFilterP="treeDataAndFilterP"
            :selectedEigenValue="selectedEigenValue"
            @addChart="addInsertChart"
            @removeChart="removeChart"
            @removeRow="removeRow"
            @getImgData="getImgData"
          />
          <Splitter
            :key="'splitV' + i"
            v-if="i !== item.charts.length - 1"
            splitType="vertical"
            :id="'splitV' + index + '-' + i"
            :limit="{
              left: 40,
              right: 40,
              top: 0,
              bottom: 0
            }"
          />
        </template>
      </div>
      <Splitter
        :key="'splitH' + index"
        v-if="index !== layout.length - 1"
        splitType="horizontal"
        :id="'splitH' + index"
        :limit="{
          left: 0,
          right: 0,
          top: 150,
          bottom: 150
        }"
      />
    </template> -->
  </div>
</template>
<script>
import { getLayoutNameList } from '@/api/analysis/index.js'
import Splitter from '@/components/splitter/index.vue'
import { rowResize, colResize } from '@/components/splitter/index.js'
import emptyChart from '../components/emptyChartTemplate.vue'
import { h, render } from 'vue'
import diagnosticRecord from '@/components/diagnosisRecord/index.vue'
import trend from '../components/trend/index.vue'
import specialAnalysis from '../components/specialAnalysis/index.vue'
import towerAnalysis from '../components/towerAnalysis/index.vue'
import cableAnalysis from '../components/cableAnalysis/index.vue'
import { getId, tabName } from '../components/toolsComponent/tools.js'
import waveTable from '../components/waveTable/table1.vue'
import { mapState } from 'vuex'

export default {
  components: {
    Splitter,
    emptyChart,
    trend,
    diagnosticRecord,
    waveTable,
    specialAnalysis,
    towerAnalysis,
    cableAnalysis
  },
  props: {
    /*  layout: {
      type: Array,
      require: true,
      default: () => {
        return []
      }
    }, */
    mainViewOption: {
      type: Object,
      require: true,
      default: () => {
        return {}
      }
    }
  },
  data() {
    return {
      activeName: 'Trend',
      acqPointerInfoList: [],
      dialogVisible: false,
      imgUrlDialog: null,
      recordChartType: '',
      recordChartInfo: {},
      eigenTypeList: ['trend', 'waveIndex', 'Spd', 'RCA', 'PCA', 'WCA', 'OA', 'DA'],
      tabArrList: [],
      selectedEigenValue: [],
      dynamicUnmountMap: {},
      chartSplitterMap: {},
      layoutResizeFrame: null,
      downLayout: [
        {
          key: 'rowbox',
          rowHeight: 49.5,
          charts: [
            {
              key: 'colbox',
              chartType: 'TimeDomain',
              chartWidth: 49.65
            },
            {
              key: 'colbox',
              chartType: 'FreqDomain',
              chartWidth: 49.65
            }
          ]
        }
      ]
    }
  },
  watch: {
    activeName: {
      handler(val, oldVal) {
        // 切换后清除实例方案，之前的波形图谱重置
        this.acqPointerInfoList = []
        this.selectedEigenValue = []
        if (val !== 'waveTable') {
          this.$nextTick(() => {
            const activeRef = this.getComponentRef(val)
            if (activeRef) {
              activeRef.currentWaveInfoDataListOut()
              activeRef.initSelectedEigenValue()
            }
          })
          //
        } else {
          this.$nextTick(() => {
            const waveTableRef = this.getComponentRef('waveTable')
            waveTableRef && waveTableRef.showWaveLine()
          })
        }
        this.$emit('changeActiveTab', val)
        // this.getLayoutContent()
        if (val == 'TVM_STE_FDN') {
          this.downLayout = [
            {
              key: 'rowbox',
              rowHeight: 49.5,
              charts: [
                {
                  key: 'colbox',
                  chartType: 'TimeDomain',
                  chartWidth: 100
                }
              ]
            }
          ]
        } else {
          this.downLayout = [
            {
              key: 'rowbox',
              rowHeight: 49.5,
              charts: [
                {
                  key: 'colbox',
                  chartType: 'TimeDomain',
                  chartWidth: 49.65
                },
                {
                  key: 'colbox',
                  chartType: 'FreqDomain',
                  chartWidth: 49.65
                }
              ]
            }
          ]
        }
      }
    },
    downLayout: {
      handler(val) {
        this.scheduleLayoutResize(true)
      },
      deep: true
    },
    mainViewOption: {
      handler(val) {
        // console.log(val)
        const { paramList, tabList, filterParam } = val
        let arr = [
          {
            key: 'Trend',
            label: '趋势分析',
            components: trend
          },
          {
            key: 'waveTable',
            label: '波形列表',
            components: waveTable
          }
        ]
        if (tabList && tabList.length) {
          tabList.forEach(item => {
            arr.push({
              key: item,
              label: tabName[item] + '趋势', //'专项分析',
              components: item.includes('TVM_STE_FDN')
                ? towerAnalysis
                : item.includes('TVM_CBF')
                ? cableAnalysis
                : specialAnalysis
            })
          })
          let arrList = ['waveTable', 'Trend', ...tabList]
          if (this.activeName.length && arrList.includes(this.activeName)) {
            this.activeName = this.activeName
          } else {
            this.activeName = tabList[0]
          }
        } else {
          this.activeName = this.activeName == 'Trend' ? 'Trend' : 'waveTable'
        }
        this.tabArrList = arr
        this.treeDataAndFilterP = { checkedMeasList: paramList, filterParam }
      },
      deep: true,
      immediate: true
    }
  },
  computed: {
    ...mapState({
      userInfo: state => state.user.userInfo
    }),
    waveInfoList() {
      if (!this.acqPointerInfoList.length) {
        return []
      } else {
        const refComponents = Object.values(this.$refs).flatMap(item =>
          Array.isArray(item) ? item : [item]
        )
        const dynamicComponents = Object.values(this.dynamicUnmountMap)
          .map(item => item.proxy)
          .filter(Boolean)
        let waveInstanc = refComponents
          .concat(dynamicComponents)
          .find(item => item && item.chartType && this.eigenTypeList.indexOf(item.chartType) < 0)
        let waveDatas = waveInstanc && waveInstanc.chartData ? waveInstanc.chartData.data : []
        return waveDatas.map(i => {
          return i.others
        })
      }
    }
    /*  acqPointerInfoList() {
      return [...new Set([].concat(this.tab1AcqPointer, this.tab2AcqPointer))]
    } */
  },
  created() {},
  mounted() {
    setTimeout(() => {
      this.scheduleLayoutResize(true)
    }, 500)
  },
  updated() {},
  beforeUnmount() {
    if (this.layoutResizeFrame) {
      cancelAnimationFrame(this.layoutResizeFrame)
      this.layoutResizeFrame = null
    }
    Object.keys(this.dynamicUnmountMap).forEach(refName => {
      this.unmountDynamicComponent(refName)
    })
    this.acqPointerInfoList = []
    this.selectedEigenValue = []
  },
  methods: {
    scheduleLayoutResize(isReposition = false) {
      this.$nextTick(() => {
        if (this.layoutResizeFrame) {
          cancelAnimationFrame(this.layoutResizeFrame)
        }
        this.layoutResizeFrame = requestAnimationFrame(() => {
          this.layoutResizeFrame = null
          rowResize('rightArea', isReposition)
          colResize('rightArea', isReposition)
        })
      })
    },
    scheduleRowColResize(rowId, isReposition = false) {
      this.$nextTick(() => {
        if (this.layoutResizeFrame) {
          cancelAnimationFrame(this.layoutResizeFrame)
        }
        this.layoutResizeFrame = requestAnimationFrame(() => {
          this.layoutResizeFrame = null
          colResize(rowId, isReposition)
        })
      })
    },
    getComponentRef(refName) {
      if (this.dynamicUnmountMap[refName]) {
        return this.dynamicUnmountMap[refName].proxy
      }
      const ref = this.$refs[refName]
      return Array.isArray(ref) ? ref[0] : ref
    },
    mountDynamicComponent(component, props, listeners, refName) {
      const container = document.createElement('div')
      const vnode = h(component, {
        ...props,
        ...listeners
      })
      if (this.$?.appContext) {
        vnode.appContext = this.$.appContext
      }
      render(vnode, container)
      const proxy = vnode.component?.proxy
      const instance = {
        $el: container.firstElementChild,
        proxy,
        unmount: () => {
          render(null, container)
        }
      }
      if (refName) {
        this.dynamicUnmountMap[refName] = instance
      }
      return instance
    },
    unmountDynamicComponent(refName) {
      const instance = this.dynamicUnmountMap[refName]
      if (!instance) return
      instance.unmount()
      delete this.dynamicUnmountMap[refName]
    },
    destroyComponentRef(refName) {
      if (this.chartSplitterMap[refName]) {
        this.unmountDynamicComponent(this.chartSplitterMap[refName])
        delete this.chartSplitterMap[refName]
      }
      if (this.dynamicUnmountMap[refName]) {
        this.unmountDynamicComponent(refName)
        return
      }
      const instance = this.getComponentRef(refName)
      if (instance) {
        instance.removeResizeOb && instance.removeResizeOb()
        instance.initChartInst && instance.initChartInst()
      }
      delete this.$refs[refName]
    },
    addRowChart() {
      let rowId = getId()
      const initRowSliptInstance = this.mountDynamicComponent(
        Splitter,
        {
          splitType: 'horizontal',
          id: 'splitH' + rowId,
          limit: {
            left: 0,
            right: 0,
            top: 150,
            bottom: 150
          },
          style: {
            flex: '0 0 4px'
          }
        },
        {},
        'splitH' + rowId
      )
      const placeholder = document.getElementById('rightArea')
      placeholder.appendChild(initRowSliptInstance.$el)
      let rowNode = document.createElement('div')
      rowNode.classList.add('rowbox')
      rowNode.id = rowId
      this.$nextTick(() => {
        const { initChartInstance } = this.getAddInstance(rowId, rowNode)
        rowNode.appendChild(initChartInstance.$el)
        placeholder.appendChild(rowNode)
        this.scheduleLayoutResize()
      })
    },
    // 全局的新增图表，在row最后追加
    addChart(rowIndex) {
      let rowDoms = document.getElementsByClassName('rowbox')
      let placeholder = rowDoms[rowIndex]
      let rowId = placeholder.id
      const { initRowSliptInstance, initChartInstance } = this.getAddInstance(rowId, placeholder)
      placeholder.appendChild(initRowSliptInstance.$el)
      placeholder.appendChild(initChartInstance.$el)
      this.scheduleRowColResize(rowId)
    },

    /**
     * positon:left/right
     */
    addInsertChart(position, rowId, itemId) {
      let placeholder = document.getElementById(rowId)
      let itemDom = document.getElementById(itemId)
      const { initRowSliptInstance, initChartInstance } = this.getAddInstance(rowId, placeholder)
      if (position == 'left') {
        placeholder.insertBefore(initChartInstance.$el, itemDom)
        placeholder.insertBefore(initRowSliptInstance.$el, itemDom)
      } else {
        placeholder.insertBefore(initChartInstance.$el, itemDom.nextElementSibling)
        placeholder.insertBefore(initRowSliptInstance.$el, itemDom.nextElementSibling)
      }
      this.scheduleRowColResize(rowId)
    },

    getAddInstance(rowId, rowDom) {
      let colId = getId()
      let that = this
      const chartRef = rowId + '-' + 'col' + colId
      const splitterRef = `${rowId}-split-${colId}`
      const initChartInstance = this.mountDynamicComponent(
        emptyChart,
        {
          acqPointerInfoList: that.acqPointerInfoList,
          treeDataAndFilterP: that.treeDataAndFilterP,
          selectedEigenValue: that.selectedEigenValue,
          itemId: chartRef,
          rowId: rowId,
          chartInitType: 'empty',
          type: that.activeName
        },
        {
          onRemoveChart: (rowId, itemId) => {
            that.removeChart(rowId, itemId)
          },
          onRemoveRow: rowId => {
            that.removeRow(rowId)
          },
          onAddChart: (position, rowId, itemId) => {
            that.addInsertChart(position, rowId, itemId)
          },
          /* updateWaveInfo: data => {
            that.updateWaveInfo(data)
          }, */
          onGetImgData: data => {
            that.getImgData(data)
          }
        },
        chartRef
      )
      /* initChartInstance.$on('removeChart', (rowId, itemId) => {
        this.removeChart(rowId, itemId)
      })*/
      const initRowSliptInstance = this.mountDynamicComponent(
        Splitter,
        {
          splitType: 'vertical',
          id: `${getId()}`,
          // id: `${rowId.replace(/row/, 'splitV')}-${children.length + 1}`,
          limit: {
            left: 40,
            right: 40,
            top: 0,
            bottom: 0
          },
          style: {
            flex: '0 0 4px'
          }
        },
        {},
        splitterRef
      )
      this.chartSplitterMap[chartRef] = splitterRef
      return {
        initRowSliptInstance,
        initChartInstance
      }
    },

    removeChart(rowId, itemId) {
      let rowDom = document.getElementById(rowId)
      // 判断删除的chart是否为当前行唯一一个，如果是，则执行删除行逻辑
      let long = rowDom.querySelectorAll('.colbox').length
      if (long == 1) {
        this.removeRow(rowId)
        return
      }
      let removeDom = document.getElementById(itemId)
      const splitterRef = this.chartSplitterMap[itemId]
      if (removeDom?.previousElementSibling) {
        rowDom.removeChild(removeDom.previousElementSibling)
      } else {
        removeDom?.nextElementSibling ? rowDom.removeChild(removeDom.nextElementSibling) : null
      }
      rowDom.removeChild(removeDom)
      this.$nextTick(() => {
        this.scheduleRowColResize(rowId)
      })
      splitterRef && this.destroyComponentRef(splitterRef)
      delete this.chartSplitterMap[itemId]
      this.destroyComponentRef(itemId)
    },
    removeRow(rowId) {
      this.$emit('deleteRow')
      let parentDom = document.getElementById('rightArea')
      let removeDom = document.getElementById(rowId)
      if (removeDom?.previousElementSibling) {
        parentDom.removeChild(removeDom.previousElementSibling)
      } else {
        removeDom?.nextElementSibling ? parentDom.removeChild(removeDom.nextElementSibling) : null
      }
      parentDom.removeChild(removeDom)
      this.scheduleLayoutResize()
      // 清除实例
      let refschildren = Object.keys(this.$refs)
        .concat(Object.keys(this.dynamicUnmountMap))
        .filter((i, index, arr) => i.indexOf(rowId) > -1 && arr.indexOf(i) === index)
      refschildren.forEach(refname => {
        this.destroyComponentRef(refname)
      })
      Object.keys(this.chartSplitterMap).forEach(chartRef => {
        if (chartRef.indexOf(rowId) > -1) {
          delete this.chartSplitterMap[chartRef]
        }
      })
    },

    getWavePointerTab1(data) {
      this.acqPointerInfoList = data
    },
    syncDataTimeRange(range) {
      this.$emit('syncDataTimeRange', range)
    },
    changeSelectedEigenValue(data) {
      if (data.length) {
        this.selectedEigenValue = data.split(',')
      } else {
        this.selectedEigenValue = []
      }
    },
    /*  getWavePointerTab2(data) {
      this.tab2AcqPointer = data
    }, */
    /* updateWaveInfo(data) {
      this.waveInfoList = [...new Set(data)]
    }, */
    getImgData({ data, recordChart, isOpen, recordChartInfo }) {
      this.imgUrlDialog = data
      this.dialogVisible = isOpen
      this.recordChartType = recordChart
      this.recordChartInfo = recordChartInfo
      // 刷新左侧设备树this.$bus.$emit('waveInfoChange', recordChartInfo)
    },
    handleDialogVisibleRec(val) {
      this.dialogVisible = val
    },
    getLayoutContent() {
      getLayoutNameList({ userName: this.userInfo.user_name, layoutName: this.activeName }).then(
        res => {
          if (res.data.data?.length) {
            this.downLayout = res.data.data[0].children[0].layoutData
          }
        }
      )
    }
  }
}
</script>
<style lang="scss" scoped>
.content-box {
  width: 100%;
  height: 100%;
  overflow: hidden;
  background: #eee;
  display: flex;
  flex-direction: column;
  position: relative;
  .rowbox {
    flex: 1 1 0%;
    width: 100%;
    display: flex;
    height: 100px;
    box-sizing: border-box;
  }
  .colbox {
    // border: 1px solid #000;
    flex: 1 1 0%;
    text-align: center;
    height: 100%;
    background: #fff;
  }
  ::v-deep .el-tabs__content {
    width: 100%;
    height: calc(100% - 30px);
    padding: 0;
  }
  ::v-deep .el-tabs__nav {
    height: 30px;
  }
}
</style>
