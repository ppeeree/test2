<template>
  <div class="report_table" style="width: 100%; margin-top: 7px; height: calc(100% - 40px)">
    <el-table
      v-show="reportData.length"
      :data="reportData"
      :height="heightSize"
      :max-height="heightSize"
      :header-cell-style="{
        background: '#114379',
        color: '#fff',
        left: 'center',
        'text-align': 'center'
      }"
      :row-class-name="tableRowClassName"
      :row-style="rowStyle"
      :cell-style="{ padding: '0px', 'text-align': 'center', cursor: 'default' }"
      style="font-size: 12px"
    >
      <el-table-column
        prop="reportTime"
        label="时间"
        class="time"
        width="70"
        :show-overflow-tooltip="true"
      >
        <template slot-scope="scope">
          <span>{{ scope.row.reportTime }}</span>
        </template>
      </el-table-column>
      <el-table-column prop="reportHealth" label="部件健康" class="health" width="180px">
        <template slot-scope="scope">
          <span
            v-for="item in scope.row.compList"
            :key="item.code"
            :style="{ color: item.color, margin: '0 3px' }"
          >
            <i :class="['icon', 'local', 'local-' + item.code]"></i>
            <span style="margin: 0 1px">{{ item.levelZh }}</span>
          </span>
        </template>
      </el-table-column>
      <el-table-column prop="person" label="诊断工程师" class="name" width="100px">
        <template slot-scope="scope">
          <span class="nowTime">
            <span>{{ scope.row.person }}</span>
          </span>
        </template>
      </el-table-column>
      <el-table-column>
        <img
          style="margin-bottom: -4px; cursor: pointer"
          src="/img/WindTurbine/icon/fileIcon.png"
          @click="handleRowClick"
        />
      </el-table-column>
    </el-table>

    <el-row class="no_data_text">
      <!-- <el-col
        :span="24"
        v-for="item in textNoDataList"
        :key="item"
        :style="{
          background: item % 2 == 0 ? 'rgba(255, 255, 255, 0.1)' : 'rgba(255, 255, 255, 0.2)',
          height: '25px',
          margin: '0px'
        }"
      ></el-col> top: -127px"-->
      <no-data
        style="height: 120px; width: 100%; position: relative"
        noteText=""
        firstText="暂无报告"
      ></no-data>
    </el-row>
  </div>
</template>

<script>
import { GeneratorEnum, eventStatusEnum } from '@/util/constant.js'
import noData from '@/components/noData/index.vue'

export default {
  components: { noData },
  props: {
    heightSize: {
      type: String,
      default: '100%',
      require: false
    },
    reportData: {
      type: Object,
      default: () => {},
      require: true
    }
  },
  data() {
    return {
      eventLevelArray: ['#2ED133', '#FFE604', '#FF6B0E', '#FF0F0D'],
      currentRowId: null,
      textNoDataList: [1, 2, 3, 4, 5]
    }
  },
  computed: {
    GeneratorEnum() {
      return GeneratorEnum
    },
    eventStatusEnum() {
      return eventStatusEnum
    }
  },
  mounted() {},
  methods: {
    rowStyle({ row }) {
      if (this.currentRowId === row.id) {
        return {
          'background-color': 'rgba(45, 86, 134, 1) !important'
        }
      }
    },
    disClearPatch() {
      this.currentRowId = null
    },
    tableRowClassName({ rowIndex }) {
      return rowIndex % 2 === 0 ? 'dobule-cell-table' : 'solo-cell-table'
    },
    handleRowClick() {
      // console.log('点击图标成功')
      window.open('http://mozilla.github.io/pdf.js/web/compressed.tracemonkey-pldi-09.pdf')
    },
    heardClass() {
      return 'text-align: center; background: #114379, color: #fff'
    }
  }
}
</script>

<style lang="less" scoped>
.no_data_text {
  height: 100%;
  width: 100%;
}
::v-deep .el-table__empty-block {
  display: none;
}
.dialog_content {
  height: 500px;
  width: 100%;
}
.blade_form {
  margin-right: 10px;
}
.cabin_form {
  margin-right: 10px;
}
.time {
  width: 10%;
}
.health {
  width: 20%;
}
::v-deep .el-table {
  // width: 100%;
  box-sizing: border-box;
  &::before {
    height: 0px;
  }
}
.el-table--border::after,
.el-table--group::after {
  width: 0px;
  height: 0px;
}

.report_table ::v-deep .el-table--fit {
  padding: 0px;
}
.report_table ::v-deep .el-table,
.el-table__expanded-cell {
  background-color: transparent;
}

.report_table ::v-deep .el-table tr {
  background-color: transparent;
}
.report_table ::v-deep .el-table--enable-row-transition .el-table__body td,
.el-table .cell {
  background-color: transparent;
  line-height: 18px;
}
::v-deep .el-table .row {
  height: 12px;
}

.report_table {
  .el-table {
    color: #fff;
    font-size: 0.7rem;
    ::v-deep tr {
      &:hover > td {
        cursor: pointer;
        background-color: #2d5686;
      }
    }
    ::v-deep .el-table__cell {
      border-bottom: none !important;
    }
    .cell-flex {
      display: flex;
      span {
        margin-left: 5px;
      }
    }
  }
  ::-webkit-scrollbar {
    display: none;
  }
}

::v-deep .el-table__body {
  -webkit-border-horizontal-spacing: 0px; // 水平间距
  -webkit-border-vertical-spacing: 0px; // 垂直间距 设置的是行间距
}

::v-deep .el-table {
  .dobule-cell-table {
    background-color: rgba(255, 255, 255, 0.2) !important;
  }
  .solo-cell-table {
    background-color: rgba(255, 255, 255, 0.1) !important;
  }
}
::v-deep .el-table,
.el-table__expanded-cell,
.el-table th.el-table__cell {
  border: none;
}
</style>
