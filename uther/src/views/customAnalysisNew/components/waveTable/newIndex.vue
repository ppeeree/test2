<!-- 增加购物车功能  -->
<template>
  <div
    class="colbox block_content"
    data-key="trend"
    :id="itemId"
    ref="tableContent"
    style="width: 100%; height: 100%"
  >
    <div class="block_left" ref="block_left">
      <p style="overflow: hidden">
        <span
          style="font-weight: bolder; float: left; color: #000; line-height: 35px; font-size: 14px"
          >波形列表</span
        >

        <el-button
          @click="showWaveLine"
          style="float: right; cursor: pointer; margin: 5px"
          size="mini"
          type="primary"
          >波形分析</el-button
        >
        <el-button
          @click="expandChange"
          style="float: right; cursor: pointer; margin: 5px"
          size="mini"
          type="primary"
          >全部展开/折叠</el-button
        >
        <el-button
          @click="dialogVisible = true"
          style="float: right; cursor: pointer; margin: 5px"
          size="mini"
          type="primary"
          >波形分析表</el-button
        >
        <el-button
          @click="addAnalysisList"
          style="float: right; cursor: pointer; margin: 5px"
          size="mini"
          type="primary"
          >加入分析列表</el-button
        >
      </p>
      <div style="overflow: hidden; box-shadow: 0px 0px 5px #e2dfdf">
        <ux-grid
          ref="multipleTable"
          row-key="id"
          tooltip-effect="dark"
          style="width: 100%"
          :tree-config="{
            lazy: true,
            children: 'children',
            hasChild: 'hasChildren',
            expandAll: false,
            trigger: '',
            loadMethod: loadChildrenMethod,
            iconOpen: 'el-icon-arrow-down',
            iconClose: 'el-icon-arrow-up'
          }"
          size="mini"
          :border="true"
          @select="selectedRow"
          :checkbox-config="{ trigger: '', checkField: 'checked', highlight: true }"
          :height="tableHeight"
          v-loading="loading"
          element-loading-text="数据加载中，请稍后......"
          element-loading-spinner="el-icon-loading"
        >
          <ux-table-column type="checkbox" width="120" fixed="left" tree-node> </ux-table-column>
          <ux-table-column
            v-for="item in tableColumns"
            :key="item.prop"
            :title="item.label"
            :field="item.prop"
            :sortable="item.sortable || false"
            :width="item.width || ''"
          >
          </ux-table-column>
        </ux-grid>
      </div>
    </div>
    <el-dialog
      title="分析列表"
      :modal="false"
      v-drag
      :visible="dialogVisible"
      width="1200px"
      @update:visible="dialogVisible = $event"
    >
      <selected-data
        @changeSelectedList="changeSelectedList"
        @showWaveLine="showWaveLine"
        :list="tableList"
        :tableColumns="tableColumns"
      ></selected-data>
    </el-dialog>
  </div>
</template>
<script>
import { getWaveTableList } from '@/api/analysis/index.js'
import selectedData from './selectedData.vue'
import isEqual from 'lodash/isEqual'
import uniqWith from 'lodash/uniqWith'
import dayjs from 'dayjs'
export default {
  props: ['treeDataAndFilterP'],
  components: { selectedData },
  data() {
    return {
      dialogVisible: false,
      tableList: [],
      isOpen: false, // 默认全部展开
      timeData: '7天',
      noData: false,
      loading: false,
      tableHeight: 360,
      tableColumns: [
        {
          prop: 'name',
          label: '风电场-机组',
          width: '180'
        },
        {
          prop: 'time',
          label: '采集时间',
          width: '160',
          sortable: true
        },
        {
          prop: 'compName',
          label: '部件'
        },
        {
          prop: 'measlocName',
          label: '波形数量/测点位置',
          width: '220'
        },
        {
          prop: 'rotatespeed',
          label: '转速(rpm)',
          width: '',
          sortable: true
        },
        {
          prop: 'rms',
          label: '有效值',
          width: ''
        },
        {
          prop: 'sampleRate',
          label: '采样频率(Hz)',
          width: ''
        },
        {
          prop: 'waveLength',
          label: '波形长度',
          width: ''
        }
      ],
      selectedTableList: []
    }
  },
  watch: {
    treeDataAndFilterP: {
      handler(newVal, oldVal) {
        let newClickedMsList = newVal?.measloc || []
        let oldClickedMsList = oldVal?.measloc || []
        let newfilterParam = newVal?.filterParam || {}
        let oldfilterParam = oldVal?.filterParam || {}
        if (
          isEqual(newClickedMsList, oldClickedMsList) &&
          isEqual(newfilterParam, oldfilterParam)
        ) {
          if (newClickedMsList.length) {
            this.$message.warning('与上次查询条件一致！')
          }
          return
        } else {
          if (!newClickedMsList.length) {
            return this.$message.warning('请勾选需要查询的数据！')
          }
          /* this.$emit('getWavePointer', []) */
          this.initTable(newClickedMsList)
        }
      },
      deep: true,
      immediate: true
    }
  },
  mounted() {
    this.creatResizeOb()
  },
  beforeUnmount() {
    //使用disconnect()方法将containerObserver实例销毁
    if (this.resizeOb) {
      this.resizeOb.disconnect()
      this.resizeOb = null
    }
  },
  methods: {
    // 监听尺寸变化，修改表格高度
    creatResizeOb() {
      if (this.resizeOb) {
        this.resizeOb.disconnect()
        this.resizeOb = null
      }
      this.resizeOb = new ResizeObserver(entries => {
        entries.forEach(entry => {
          let newHeight = entry.contentRect.height
          this.$refs.block_left.style.height = newHeight + 10 + 'px'
          this.tableHeight = newHeight - 40
        })
      })
      // 指定观察dom
      this.resizeOb.observe(this.$refs.tableContent)
    },
    // 折叠
    expandChange() {
      this.isOpen = !this.isOpen
      this.$refs.multipleTable.setAllTreeExpand(this.isOpen)
    },

    // 表格数据初始化
    initTable(clickedMsList) {
      this.tableData = []
      if (!clickedMsList.length) {
        return
      }
      this.loading = true
      let { filterParam } = this.treeDataAndFilterP
      const { timeValue, wkCond } = filterParam
      let ids = Array.from(clickedMsList, item => item.id).join(',')
      let ROTSPEED_MCS = `${
        Number.isFinite(wkCond.ROTSPEED_MCS[0]) ? wkCond.ROTSPEED_MCS[0] : null
      },${Number.isFinite(wkCond.ROTSPEED_MCS[1]) ? wkCond.ROTSPEED_MCS[1] : null}`
      let param = {
        endTime: timeValue[1],
        startTime: timeValue[0],
        spdRange: ROTSPEED_MCS,
        // entityId: type == 'comp' ? id : '',
        measlocId: ids // type == 'msc' ? id : '',
        //  windturbineId: type == 'turbine' ? id : ''
      }
      getWaveTableList(param).then(res => {
        if (res.data.code == 200) {
          let result = []
          // let checkedRowList = []
          let data = res.data.data
          for (let i = 0; i < data.length; i++) {
            let unit = {
              ...data[i],
              measlocName: data[i].measlocSummary,
              id: data[i].windturbineId + data[i].acqTime,
              checked: false,
              time: data[i].acqTime,
              name: data[i].windParkName + '-' + data[i].windturbineName,
              hasChildren: true
            }
            result.push(unit)
          }
          this.tableData = result.reverse()
          this.$refs.multipleTable.reloadData(this.tableData)
          this.loading = false
        }
      })
    },
    // 异步加载子节点
    loadChildrenMethod({ row }) {
      return new Promise((resolve, reject) => {
        let { timeValue, wkCond } = this.treeDataAndFilterP.filterParam
        let ROTSPEED_MCS = `${Number.isFinite(wkCond.ROTSPEED_MCS[0]) ? down : null},${
          Number.isFinite(wkCond.ROTSPEED_MCS[1]) ? up : null
        }`
        let ids = Array.from(this.treeDataAndFilterP.measloc, item => item.id).join(',')
        let param = {
          endTime: timeValue[1],
          startTime: timeValue[0],
          spdRange: ROTSPEED_MCS,
          measlocId: ids,
          acqTime: row.acqTime
        }
        getWaveTableList(param).then(res => {
          let { data } = res.data
          let children = data[0].children.map(ii => {
            return {
              ...ii,
              checked: false, //isCheck,
              id: ii.measlocId + ii.waveDefId + ii.acqTime,
              parentId: ii.windturbineId + ii.acqTime,
              time: '',
              name: '',
              windParkName: data[0].windParkName
            }
          })
          resolve(children)
        })
      })
    },
    // 部件间切换，波形列表保持选中切换前的勾选项
    // 点击波形分析
    showWaveLine(selectedList) {
      let selectList = Array.isArray(selectedList)
        ? selectedList
        : this.$refs.multipleTable.getCheckboxRecords()
      let params = []
      selectList.forEach(i => {
        if (!i.children) {
          params.push(i.windturbineId + '&&' + i.measlocId + '&&' + i.acqTime + '&&' + i.waveDefId)
        }
      })
      this.$emit('getWavePointer', params)
    },
    // 勾选父级先判断是否已经有子级数据，使用setTreeExpand，组件自调用loadChildrenMethod
    selectedRow(selection, row) {
      if (row.children && !row.children.length) {
        this.$refs.multipleTable.setTreeExpand(row, true)
      }
    },

    // 加入分析列表
    addAnalysisList() {
      let selectList = this.$refs.multipleTable.getCheckboxRecords()
      let params = this.tableList
      selectList.forEach(i => {
        if (!i.children) {
          params.push({
            ...i,
            time: i.acqTime,
            name: i.windParkName + '-' + i.windturbineName
          })
        }
      })
      this.tableList = uniqWith(params, (arr, acc) => arr.id === acc.id)
      this.$message.success('添加成功！')
    },
    changeSelectedList(newdArr) {
      this.tableList = newdArr
    }
  }
}
</script>
<style lang="scss" scoped>
.block_content {
  :deep(.el-dialog){
    height: auto !important;
    padding-bottom: 10px;
    max-height: 80%;
    overflow: auto;
    .el-table th.el-table__cell > .cell {
      color: #999;
    }
    .el-table tr {
      color: #999;
    }
  }
}
.colbox {
  text-align: center;
  height: 100%;
  position: relative;
  background: #fff;

  h3 {
    position: absolute;
    right: 10px;
    top: 10px;
    z-index: 20;
    width: auto;
    height: 28px;
  }
  .block_left {
    width: 100%; //calc(100% - 304px);
    height: 100%;
    left: 0;
    position: absolute;
    top: 0;
    padding: 0 10px;
  }
}
.block_btn {
  float: right;
}
</style>
