<template>
  <div
    class="colbox block_content"
    data-key="trend"
    :id="itemId"
    ref="tableContent"
    style="width: 100%; height: 100%"
  >
    <div class="block_left" ref="block_left">
      <div style="overflow: hidden; border-bottom: 2px solid #ccc; height: 35px">
        <span
          style="font-weight: bolder; float: left; color: #000; line-height: 35px; font-size: 14px"
          >波形列表
        </span>
        <div class="condition_filter">
          <el-pagination
            :page-size="pageSize"
            @current-change="handleCurrentChange"
            :hide-on-single-page="true"
            small
            :current-page="currentPage"
            :total="dataTotal"
            layout="prev, pager, next"
            style="margin: 0; display: inline-block; float: left"
          >
          </el-pagination>
        </div>
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
      </div>
      <div style="width: 100%; overflow: hidden; box-shadow: 0px 0px 5px #e2dfdf">
        <!--  @cell-click="cellClick" -->
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
          :checkbox-config="{ trigger: 'cell', checkField: 'checked', highlight: true }"
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
  </div>
</template>
<script>
import { getWaveTableList, getWaveTableDetailList } from '@/api/analysis/index.js'
import dayjs from 'dayjs'
import isEqual from 'lodash/isEqual'
export default {
  props: ['treeDataAndFilterP'],
  components: {},
  data() {
    return {
      currentPage: 1,
      pageSize: 50,
      isOpen: false, // 默认全部展开
      noData: false,
      loading: false,
      tableHeight: 360,
      tableColumns: [
        {
          prop: 'name',
          label: '风电场-机组',
          width: '280'
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
          prop: 'rotSpeed',
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
      ]
    }
  },
  watch: {
    treeDataAndFilterP: {
      handler(newVal, oldVal) {
        let newClickedMsList = newVal?.checkedMeasList || []
        let oldClickedMsList = oldVal?.checkedMeasList || []
        let newfilterParam = newVal?.filterParam || {}
        let oldfilterParam = oldVal?.filterParam || {}
        if (
          isEqual(newClickedMsList, oldClickedMsList) &&
          isEqual(newfilterParam, oldfilterParam)
        ) {
          if (newClickedMsList.length) {
            //  this.$message.warning('与上次查询条件一致！')
            // 应刚伟要求（采集数据需要实时刷新，手动刷新页面太麻烦），修改成再次请求，刷新页面数据
            this.$emit('getWavePointer', [])
            this.initTable(newClickedMsList)
          }
          return
        } else {
          if (!newClickedMsList.length) {
            return this.$message.warning('请勾选需要查询的数据！')
          }
          this.$emit('getWavePointer', [])
          this.initTable(newClickedMsList)
        }
      },
      deep: true,
      immediate: true
    }
    /*  getSelectOptionParam: {
      handler(val) {
        this.$emit('getWavePointer', [])
        if (val.length) {
          this.initTable(val)
        }
      },
      deep: true
    } */
  },
  activated() {
    this.creatResizeOb()
  },
  deactivated() {
    if (this.resizeOb) {
      this.resizeOb.disconnect()
      this.resizeOb = null
    }
  },
  computed: {},
  /*  mounted() {
    this.creatResizeOb()
  },
  beforeUnmount() {
    //使用disconnect()方法将containerObserver实例销毁
    if (this.resizeOb) {
      this.resizeOb.disconnect()
      this.resizeOb = null
    }
  }, */
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
    updatePageData() {
      let currentData = this.tableData.slice(
        (this.currentPage - 1) * this.pageSize,
        this.currentPage * this.pageSize
      )
      this.$refs.multipleTable.reloadData(currentData)
    },
    // 分页
    handleCurrentChange(val) {
      this.currentPage = val
      this.updatePageData()
    },
    // 表格数据初始化
    initTable() {
      this.$refs.multipleTable ? this.$refs.multipleTable.reloadData([]) : null
      if (!this.treeDataAndFilterP.checkedMeasList.length) {
        return
      }
      this.loading = true
      let { filterParam } = this.treeDataAndFilterP
      const { timeValue, wkCond } = filterParam
      let ids = Array.from(
        this.treeDataAndFilterP.checkedMeasList,
        item => item.measLoctionID
      ).join(',')
      let ROTSPEED_MCS = `${
        Number.isFinite(wkCond.ROTSPEED_MCS[0]) ? wkCond.ROTSPEED_MCS[0] : null
      },${Number.isFinite(wkCond.ROTSPEED_MCS[1]) ? wkCond.ROTSPEED_MCS[1] : null}`
      let param = {
        endTime: timeValue[1],
        startTime: timeValue[0],
        spdRange: ROTSPEED_MCS,
        measlocId: ids,
        deviceID: '',
        detail: false
      }
      getWaveTableList(param).then(res => {
        if (res.data.code == 200) {
          let result = []
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
          this.tableData = result
          this.dataTotal = result.length
          this.currentPage = 1
          this.updatePageData()
          this.loading = false
        }
      })
    },
    // 异步加载子节点
    loadChildrenMethod({ row }) {
      return new Promise((resolve, reject) => {
        let { timeValue, wkCond } = this.treeDataAndFilterP.filterParam
        let ROTSPEED_MCS = `${
          Number.isFinite(wkCond.ROTSPEED_MCS[0]) ? wkCond.ROTSPEED_MCS[0] : null
        },${Number.isFinite(wkCond.ROTSPEED_MCS[1]) ? wkCond.ROTSPEED_MCS[1] : null}`
        let ids = Array.from(
          this.treeDataAndFilterP.checkedMeasList,
          item => item.measLoctionID
        ).join(',')
        let param = {
          endTime: timeValue[1],
          startTime: timeValue[0],
          spdRange: ROTSPEED_MCS,
          measlocId: ids,
          acqTime: row.acqTime,
          deviceID: row.windturbineId,
          detail: true
        }
        getWaveTableDetailList(param).then(res => {
          let { data } = res.data
          if (data.children && data.children.length) {
            let children = data.children.map(ii => {
              return {
                ...ii,
                checked: false, //isCheck,
                id: ii.measlocId + ii.sampleRate + ii.acqTime,
                parentId: ii.windturbineId + ii.acqTime,
                time: '',
                name: '',
                windParkName: row.windParkName
              }
            })
            resolve(children)
          } else {
            resolve()
          }
        })
      })
    },
    // 部件间切换，波形列表保持选中切换前的勾选项
    // 点击波形分析
    showWaveLine() {
      // this.creatResizeOb()
      let selectList = this.$refs.multipleTable.getCheckboxRecords()
      let params = []
      selectList.forEach(i => {
        if (!i.children) {
          params.push(i.windturbineId + '&&' + i.measlocId + '&&' + i.acqTime + '&&' + i.sampleRate)
        }
      })
      this.$emit('getWavePointer', params)
    },
    // 勾选父级先判断是否已经有子级数据，使用setTreeExpand，组件自调用loadChildrenMethod
    selectedRow(selection, row) {
      if (row.children && !row.children.length) {
        this.$refs.multipleTable.setTreeExpand(row, true)
      } /* else {
        console.log(e)
        console.log(selection)
        console.log(row)
      } */
    }
    /*   cellClick(row, column,cell, event) {
      console.log(row)
      console.log(column)
      console.log(event)
    } */
  }
}
</script>
<style lang="scss" scoped>
.block_content {
  ::v-deep .el-dialog {
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
    height: calc(100% - 10px);
    left: 0;
    position: absolute;
    top: 0;
    padding: 0 10px;
    .condition_filter {
      height: 30px;
      margin-right: 10px;
      width: 80%;
      display: inline-block;
      text-align: right;
      margin-top: 5px;
      p {
        display: inline-block;
        width: 300px;
        font-size: 12px;
        text-align: left;
        color: #000;
        label {
          display: inline-block;
          width: 40px;
        }
        .inline-input {
          width: 100px;
          ::v-deep .el-input__inner {
            padding: 0;
            text-align: center;
          }
        }
        input {
          height: 30px;
          line-height: 30px;
        }
        ::v-deep .el-range-editor {
          width: 200px;
          height: 30px;
          line-height: 30px;
        }
        ::v-deep .el-date-editor {
          .el-range-separator {
            color: #303133;
            padding: 0;
          }
          .el-range-input {
            width: 45%;
          }
          .el-range__icon {
            display: none;
          }
          .el-range__close-icon {
            display: none;
          }
        }
      }
    }
  }
}
.block_btn {
  float: right;
}
</style>
