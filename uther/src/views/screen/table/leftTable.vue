<template>
  <div class="table-wrapper" :style="handleTableStyle">
    <el-table
      ref="tableWrapper"
      :data="visibleList"
      :max-height="handleTableHeight"
      :height="handleTableHeight"
      :header-cell-style="{ background: '#114379', color: '#fff' }"
      :row-class-name="tableRowClassName"
      :row-style="rowStyle"
      @row-click="handleRowClick"
      style="font-size: 10px"
      :cell-style="{
        paddingTop: handleCellStyle,
        paddingBottom: handleCellStyle
      }"
    >
      <template slot="empty">
        <noData noteText="" firstText="机组暂无事件" />
      </template>
      <el-table-column prop="eventLevel" label="等级" align="center" width="50">
        <template slot-scope="scope">
          <span :style="{ color: eventLevelArray[scope.row.eventLevel - 1] }">
            {{ eventLevelEnum[scope.row.eventLevel] }}
          </span>
        </template>
      </el-table-column>
      <el-table-column
        prop="eventSites"
        label="设备"
        align="center"
        width="80"
        :show-overflow-tooltip="true"
      >
      </el-table-column>
      <el-table-column prop="eventReason" label="事件详情" align="center">
        <template slot-scope="scope">
          <div class="cell-flex">
            <el-image
              style="width: 20px; height: 20px"
              :src="requireImgType(scope.row.eventType)"
              fit="scale-down"
            ></el-image>
            <el-tooltip
              :disabled="scope.row.eventReason.length < 8"
              class="item"
              effect="dark"
              :content="scope.row.eventReason"
              placement="top"
            >
              <span class="cell-span">{{ scope.row.eventReason }}</span>
            </el-tooltip>
          </div>
        </template>
      </el-table-column>
      <el-table-column
        prop="disNowTime"
        label="发生时间"
        width="75"
        align="center"
        :show-overflow-tooltip="true"
      >
        <!-- <template slot-scope="scope">
          <span>{{ handleTime(scope.row.disNowTime) }}</span>
        </template> -->
      </el-table-column>
      <el-table-column prop="eventStatus" label="处理状态" align="center" width="75">
        <template slot-scope="scope">
          <span>{{ eventStatusEnum[scope.row.eventStatus] }}</span>
        </template>
      </el-table-column>
      <template #append>
        <div :style="{ height: `${totalHeight}px` }"></div>
      </template>
    </el-table>
  </div>
</template>

<script>
import { eventStatusEnum } from '@/util/constant'
import { eventLevelEnum, eventLevelColorStyleEnum } from '@/util/constant'
import dayjs from 'dayjs'
import throttle from 'lodash/throttle'

export default {
  components: { noData: () => import('@/components/noData/index.vue') },
  props: {
    tableListData: {
      type: Array,
      default: () => [],
      require: true
    },
    //判断父组件是哪个
    fatherComp: {
      type: String,
      default: 'windFarm',
      require: true
    },
    showEventDetail: {
      type: Boolean
      // default:false
    },
    itemHeight: {
      type: Number,
      default: 48.17
    },
    showItemNumber: {
      type: Number,
      default: 9
    }
  },
  inject: ['showPop'],
  data() {
    return {
      tableData: [],
      eventLevelEnum,
      eventLevelArray: ['#FFF287', '#F5B270', '#E85E51', '#DC1034'],
      timeOutNum: null,
      currentRowId: null,
      setNewTypeName: {
        1: 'patroEvent',
        3: 'healthEvent',
        4: 'workEvent'
      },
      totalHeight: 0,
      visibleList: [],
      scrollTop: 0,
      start: 0
    }
  },
  computed: {
    eventStatusEnum() {
      return eventStatusEnum
    },
    isComponents() {
      return this.fatherComp == 'turbine'
    },
    handleTableHeight() {
      return this.isComponents ? 'calc(100% - 40px)' : '424px'
    },
    handleCellStyle() {
      return this.isComponents ? '4px' : ''
    },
    handleTableStyle() {
      return {
        width: '450px',
        height: this.isComponents ? ' calc(100% - 115px)' : ''
      }
    },
    eventLevelColorStyleEnum() {
      return eventLevelColorStyleEnum
    }
  },
  watch: {
    tableListData: {
      handler(value) {
        this.$nextTick(() => {
          this.tableData = value
          this.totalHeight = value.length * this.itemHeight
          this.visibleList = this.tableData.slice(this.start, this.start + this.showItemNumber)
        })
      },
      deep: true,
      immediate: true
    },
    showEventDetail: {
      handler(val) {
        if (!val) {
          this.currentRowId = null
        }
      }
    }
  },
  mounted: function () {
    this.$bus.$on('leftTalbe', () => {
      this.disClearPatch()
    }),
      this.$refs.tableWrapper.bodyWrapper.addEventListener('scroll', e =>
        this.changeScroll(e.target.scrollTop)
      )
  },
  methods: {
    handleRowClick(row) {
      //设置点击出现事件详情弹框
      if (row) {
        this.$emit('cilckEventListItem', row)
      }
      if (this.currentRowId === row.indexId) return
      this.currentRowId = row.indexId
    },
    rowStyle({ row }) {
      const styleColor = {
        'background-color': 'rgba(31, 255, 255, 0.4) !important'
      }

      if (row.eventLevel === 4) {
        return (styleColor['background-color'] = 'rgba(220, 16, 52, 0.4) !important')
      }
      if (this.currentRowId === row.indexId) {
        return styleColor
      }
    },
    disClearPatch() {
      this.currentRowId = null
    },
    requireImgType(id) {
      if (!id) return
      return require(`/public/img/eventMethod/${this.setNewTypeName[id]}.png`)
    },
    tableRowClassName({ rowIndex }) {
      return rowIndex % 2 === 0 ? 'dobule-cell-table' : 'solo-cell-table'
    },
    mouseEnter(row) {
      if (this.fatherComp !== 'turbine') {
        this.timeOutNum && clearTimeout(this.timeOutNum)
        this.showPop(true, row)
      }
    },
    mouseLeave() {
      if (this.fatherComp !== 'turbine') {
        this.timeOutNum = setTimeout(() => {
          this.showPop(false)
        }, 800)
        this.$store.commit('SET_TIME', this.timeOutNum)
      }
    },
    handleTime(time) {
      return dayjs(time).fromNow()
    },
    changeScroll: throttle(function (val) {
      this.$nextTick(() => {
        this.start = Math.floor(val / this.itemHeight)
        this.visibleList = this.tableData.slice(this.start, this.start + this.showItemNumber)
        const top = this.start * this.itemHeight
        this.$refs.tableWrapper.$el.getElementsByClassName(
          'el-table__body'
        )[0].style.top = `${top}px`
      })
    }, 100)
  }
}
</script>
<style lang="less" scoped>
.table-wrapper ::v-deep .el-table--fit {
  padding: 0px;
}
.table-wrapper ::v-deep .el-table,
.el-table__expanded-cell {
  background-color: transparent;
}

.table-wrapper ::v-deep .el-table tr {
  background-color: transparent;
}
.table-wrapper ::v-deep .el-table--enable-row-transition .el-table__body td,
.el-table .cell {
  background-color: transparent;
}
//去掉底下边框
::v-deep .el-table {
  // width: 100%;
  box-sizing: border-box;
  &::before {
    height: 0px;
  }
}

.table-wrapper {
  overflow: hidden;
  .el-table {
    color: #fff;
    font-size: 0.7rem;
    ::v-deep tr {
      &:hover > td {
        cursor: pointer;
        background-color: transparent;
        border-top: 2px solid #4abaff;
        border-bottom: 2px solid #4abaff;
        &:first-child {
          border-left: 2px solid #4abaff;
        }
        &:last-child {
          border-right: 2px solid #4abaff;
        }
      }
    }
    ::v-deep .el-table__cell {
      border-bottom-color: transparent;
    }
    .cell-flex {
      display: flex;
      flex-direction: row;
      justify-content: flex-start;
      align-items: center;
      .cell-span {
        display: inline-block;
        width: calc(100% - 25px);
        text-overflow: ellipsis;
        white-space: nowrap;
        overflow: hidden;
        margin-left: 5px;
        text-align: left;
      }
    }
  }
  ::-webkit-scrollbar {
    // display: none;
    width: 7px;
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
::v-deep .el-empty__description {
  margin-top: 0px;
}
::v-deep .el-table__empty-text {
  line-height: 29px;
}
.event-level-img {
  position: relative;
  top: 6px;
}
::v-deep .el-table__body-wrapper {
  overflow-y: auto;
  .el-table__body {
    position: absolute;
  }
}
::v-deep .el-table .el-table__cell.is-center,
.el-table td.el-table__cell,
.el-table th.el-table__cell.is-leaf {
  border-color: transparent;
}
::v-deep .el-table td.el-table__cell,
.el-table th.el-table__cell.is-leaf {
  border-bottom: none;
}
::v-deep .el-table__row td {
  border-left: none;
}
::v-deep .el-table th.gutter {
  // display: none;
  background: rgb(17, 67, 121);
}
</style>
