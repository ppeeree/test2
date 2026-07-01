<template>
  <div style="width: 100%; height: 100%; padding: 0 10px">
    <el-table
      ref="singleTable"
      :data="turbineList"
      height="95%"
      style="width: 100%"
      border
      row-key="guid"
      :header-cell-style="{
        backgroundColor: '#F2F6FC',
        color: '#909399',
        textAlign: 'center'
      }"
      class="tableLimit"
      size="mini"
    >
      <!-- :span-method="objectSpanMethod" -->
      <!-- @filter-change="filterStatus" -->
      <el-table-column prop="deviceName" width="120" label="机组名称"></el-table-column>
      <el-table-column prop="monitorName" label="传感器名称"></el-table-column>
      <el-table-column prop="monitorConnType" label="连接方式"></el-table-column>
      <el-table-column prop="monitorIP" label="IP地址"></el-table-column>

      <el-table-column
        prop="monitorStatus"
        label="状态"
        :filter-method="filterStatus"
        :filters="turbineStatusFilters"
      >
        <template slot-scope="scope">
          <span :style="{ color: scope.row.monitorStatus == '正常' ? '#606266' : '#F56C6C' }">{{
            scope.row.monitorStatus
          }}</span>
        </template>
      </el-table-column>
      <el-table-column prop="data" label="数据"> </el-table-column>
      <el-table-column
        prop="monitorStatusTime"
        sortable
        label="更新时间"
        width="280"
      ></el-table-column>
    </el-table>
  </div>
</template>
<script>
import { getModBusListApi } from '@/api/basicConfig/dau.js'
export default {
  props: {
    selectedWindparkID: {
      type: String,
      required: true
    },
    monitorTypeCode: {
      type: String,
      required: true
    },
    keyId: {
      type: String,
      required: true
    }
  },
  watch: {
    keyId: {
      handler() {
        this.initTable()
      },
      immediate: true
    }
  },
  data() {
    return {
      interTimer: null, //定时器
      turbineList: [],
      turbineStatusFilters: [],
      displayData: []
    }
  },
  mounted() {
    /*  this.interTimer = setInterval(() => {
      this.initTable()
    }, 1000 * 60) */
  },
  destroyed() {
    /*  if (this.interTimer) {
      clearInterval(this.interTimer)
      this.interTimer = null
    } */
  },
  methods: {
    initTable() {
      this.turbineStatusFilters = []
      getModBusListApi({
        stationID: this.selectedWindparkID,
        monitorType: this.monitorTypeCode
      }).then(res => {
        if (res.data.code == 200) {
          this.turbineList = res.data.data
          // 过滤下拉
          let statusList = new Set(Array.from(this.turbineList, item => item.monitorStatus))
          this.turbineStatusFilters = Array.from([...statusList], item => ({
            value: item,
            text: item
          }))
          // 清除过滤
          if (this.$refs.singleTable) {
            this.$refs.singleTable.clearFilter()
          }
        }
      })
    },
    objectSpanMethod({ row, column, rowIndex, columnIndex }) {
      if (columnIndex === 0) {
        if (rowIndex > 0 && row['deviceName'] === this.turbineList[rowIndex - 1]['deviceName']) {
          return {
            rowspan: 0,
            colspan: 1
          }
        }
        let rowspan = 1
        for (let i = rowIndex + 1; i < this.turbineList.length; i++) {
          if (this.turbineList[i]['deviceName'] === row['deviceName']) {
            rowspan++
          } else {
            break
          }
        }
        return {
          rowspan: rowspan,
          colspan: 1
        }
      }
    },
    // 机组状态表格状态列筛选
    /* filterStatus(value) {
      let valueArr = Object.values(value)
      if (valueArr[0].length) {
        this.displayData = this.turbineList.filter(item => {
          console.log(valueArr[0].indexOf(item.status) > -1)
          return valueArr[0].indexOf(item.status) > -1
        })
        console.log(this.displayData)
      } else {
        this.displayData = this.turbineList
        return
      }
    } */
    // 机组状态表格状态列筛选
    filterStatus(value, row) {
      return row.monitorStatus === value
    }
  }
}
</script>
<style lang="scss" scoped>
::v-deep .el-table--mini .el-table__cell {
  padding: 3px 0;
}
</style>
