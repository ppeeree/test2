<template>
  <div class="IES_tree_control">
    <div class="item_content_2">
      <div class="item_content_2_title">
        <div class="item_content_2_layout">
          <label style="width: 60px; text-align: right">风场：</label>
          <el-select v-model="windparkSelected" placeholder="请选择">
            <el-option
              v-for="item in userDeptTree"
              :key="item.id"
              :label="item.name"
              :value="item.id"
            >
            </el-option>
          </el-select>
        </div>
        <div class="item_content_2_layout">
          <!--  <i
            class="el-icon-refresh"
            title="刷新"
            @click="getWindParkCurrentTree"
            style="
              margin-right: 10px;
              font-weight: bolder;
              cursor: pointer;
              font-size: 18px;
              color: #000;
            "
          ></i> -->
          <label>报告时间：</label>
          <el-date-picker
            align="right"
            unlink-panels
            range-separator="~"
            start-placeholder="开始"
            end-placeholder="结束"
            ref="time"
            v-model="monthPickerValue"
            type="daterange"
            value-format="YYYY-MM-DD"
            :shortcuts="pickerShortcuts"
            prefix-icon=""
            clear-icon=""
            @change="getWindParkCurrentTree"
          >
          </el-date-picker>
        </div>
      </div>
      <div class="item_content_2_content">
        <el-tree
          ref="diagnosticReportTree"
          :data="diagnosticReportTreeList"
          :props="diagnosticReportTreeProps"
          node-key="id"
          :current-node-key="diagnosticReportTreeCurrentNodeKey"
          highlight-current
          :expand-on-click-node="false"
          default-expand-all
          @node-click="diagnosticReportTreeClick"
          v-loading="diagnosticReportTreeLoading"
          element-loading-background="#eee"
          element-loading-text="正在加载中"
        >
          <!--    getTime(data.createdTime)  -->
          <template #default="{ data }">
          <div :class="['custom-tree-node']">
            <span>{{ data.name || data.createdTime }}</span>
            <div
              v-if="data.status"
              class="tree-node-state"
              :title="eventTypeEnum[data.status]"
              :style="{ background: levelColorEnum[data.status] }"
            />
            <i
              style="font-size: 16px; margin-left: 5px"
              v-if="data.type == 'WindturbineReport' && data.isCorrelationWindpark"
              title="已关联"
              class="el-icon-link"
            ></i>
            <template v-if="data.type == 'WindturbineReport' && !data.isCorrelationWindpark">
              <i
                title="删除"
                @click.stop="deleteTurbineReport(data)"
                class="el-icon-remove-outline icon-font-btn"
              ></i>
            </template>
          </div>
          </template>
        </el-tree>
      </div>
    </div>
    <div class="item_content_3">
      <div class="item_content_2_title">
        <span class="title">{{
          selectedNode.type == 'WindPark' ? '风场历史报告' : '机组分析记录'
        }}</span>
        <div class="item_content_2_layout" v-if="selectedNode.type == 'Windturbine'">
          <i
            class="el-icon-refresh"
            title="刷新"
            @click="getWindParkCurrentTree"
            style="
              margin-right: 10px;
              font-weight: bolder;
              cursor: pointer;
              font-size: 18px;
              color: #000;
            "
          ></i>
          <el-date-picker
            v-model="analysisRecordsTime"
            type="daterange"
            align="right"
            range-separator="~"
            value-format="YYYY-MM-DD"
            :clearable="false"
            :shortcuts="pickerShortcuts"
            unlink-panels
            start-placeholder="开始"
            end-placeholder="结束"
            ref="time"
            prefix-icon=""
            clear-icon=""
            @change="handleAnalysisTimeChange"
          />
        </div>
      </div>
      <div class="item_content_2_content">
        <el-tree
          v-if="selectedNode.type == 'Windturbine'"
          :data="treeData"
          node-key="id"
          default-expand-all
          :props="diagnosticReportTreeProps"
          show-checkbox
          ref="tree"
          :default-checked-keys="defaultCheckedKeys"
          v-loading="treeDataLoading"
          element-loading-background="#eee"
          element-loading-text="正在加载中"
          @check-change="handleCheckChange"
        />
        <el-tree
          v-if="selectedNode.type == 'WindPark'"
          ref="historyReportTree"
          :data="historyReportTreeList"
          :props="diagnosticReportTreeProps"
          node-key="id"
          :current-node-key="historyReportTreeCurrentNodeKey"
          highlight-current
          :expand-on-click-node="false"
          default-expand-all
          @node-click="historyReportTreeClick"
          v-loading="historyReportTreeLoading"
          element-loading-background="#eee"
          element-loading-text="正在加载中"
        >
          <template #default="{ data }">
          <div :class="['custom-tree-node']">
            <span
              >{{ data.name
              }}{{ data.type == 'WindTurbineReport' ? '&nbsp;&nbsp;' + data.createTime : '' }}</span
            >
            <div
              v-if="data.status"
              class="tree-node-state"
              :title="eventTypeEnum[data.status]"
              :style="{ background: levelColorEnum[data.status] }"
            />
          </div>
          </template>
        </el-tree>
      </div>
    </div>
  </div>
</template>

<script>
import {
  getWindTurbineReportTree, // 获取风场对应的最新的机组树
  deleteTurbineReport, // 删除报告
  getTurbineAnalysisTree, // 获取分析记录树
  getWindparkHistoryTree // 获取风场历史报告树
} from '@/api/analysis/workPlatform'
import { levelColorEnum, eventTypeEnum } from '@/util/constant'
import treeMixins from '../mixins/tree'
import { mapGetters } from 'vuex'
import dayjs from 'dayjs'
import debounce from 'lodash/debounce'

export default {
  mixins: [treeMixins],
  props: {},
  data() {
    return {
      isTouchCheck: false,
      selectedNode: {},
      eventTypeEnum,
      levelColorEnum,
      windparkSelected: '',
      diagnosticReportTreeList: [],
      diagnosticReportTreeProps: {
        label: 'name',
        children: 'children',
        value: 'id'
      },
      historyReportTreeList: [],
      historyReportTreeLoading: false,
      treeData: [],
      treeDataLoading: false,
      defaultCheckedKeys: [],
      monthPickerValue: [
        dayjs().subtract(1, 'month').format('YYYY-MM-DD'),
        dayjs().format('YYYY-MM-DD')
      ],
      pickerShortcuts: [
        {
          text: '最近一周',
          value: () => {
            const end = new Date()
            const start = new Date()
            start.setTime(start.getTime() - 3600 * 1000 * 24 * 7)
            return [start, end]
          }
        },
        {
          text: '最近一个月',
          value: () => {
            const end = new Date()
            const start = new Date()
            start.setTime(start.getTime() - 3600 * 1000 * 24 * 30)
            return [start, end]
          }
        },
        {
          text: '最近三个月',
          value: () => {
            const end = new Date()
            const start = new Date()
            start.setTime(start.getTime() - 3600 * 1000 * 24 * 90)
            return [start, end]
          }
        }
      ],
      analysisRecordsTime: [
        dayjs().subtract(1, 'month').format('YYYY-MM-DD'),
        dayjs().format('YYYY-MM-DD')
      ],
      diagnosticReportTreeLoading: false
    }
  },
  watch: {
    windparkSelected: {
      handler: function (val) {
        if (!val) {
          return
        }
        this.selectedNode = {}
        this.getWindParkCurrentTree()
      },
      immediate: true
    },
    monthPickerValue: {
      handler: function (val) {
        if (!val?.length) {
          return
        }
        this.analysisRecordsTime = val
      },
      immediate: true
    }
  },
  computed: {
    ...mapGetters(['userDeptTree'])
  },
  mounted() {
    //  this.windparkSelected = this.userDeptTree[0].id
  },
  methods: {
    handleCheckChange: debounce(function () {
      if (!this.isTouchCheck) return
      let checkedKeys = this.$refs.tree.getCheckedKeys()
      let halfCheckedKeys = this.$refs.tree.getHalfCheckedKeys()
      this.$emit('changeCheckedRecord', checkedKeys.concat(halfCheckedKeys))
    }, 300),
    getTime(time) {
      return dayjs(time).format('YYYY-MM-DD')
    },
    deleteTurbineReport(data) {
      deleteTurbineReport({ reportGuid: data.id }).then(res => {
        if (res.data.success) {
          this.$message.success(res.data.message)
          // 删除成功，初始化左侧上面的树接口
          this.selectedNode = {}
          this.getWindParkCurrentTree()
        } else {
          this.$message.error(res.data.message)
          return
        }
      })
    },
    handleDialogVisible() {
      this.$emit('handleDialogVisible', true)
    },
    getWindParkCurrentTree(checkedId) {
      if (checkedId && Array.isArray(checkedId)) {
        this.selectedNode = {}
      }
      this.diagnosticReportTreeLoading = true
      getWindTurbineReportTree({
        windParkID: this.windparkSelected,
        startTime: this.monthPickerValue[0],
        endTime: this.monthPickerValue[1]
      })
        .then(result => {
          this.diagnosticReportTreeList = [result.data.data]
          this.$nextTick(() => {
            if (checkedId && !Array.isArray(checkedId)) {
              this.diagnosticReportTreeCurrentNodeKey = checkedId
              this.$refs.diagnosticReportTree.setCurrentKey(checkedId)
              this.diagnosticReportTreeClick({ type: 'WindturbineReport', id: checkedId })
            } else {
              let ids = this.extractIds(result.data.data)
              if (this.selectedNode.id && ids.includes(this.selectedNode.id)) {
                this.diagnosticReportTreeCurrentNodeKey = this.selectedNode.id
                this.$refs.diagnosticReportTree.setCurrentKey(
                  this.diagnosticReportTreeCurrentNodeKey
                )
              } else {
                this.$parent.pageContent = ''
              }
            }
          })
        })
        .finally(() => {
          this.diagnosticReportTreeLoading = false
        })
    },
    diagnosticReportTreeClick(data, node) {
      this.selectedNode = data
      // 查看或者新增风场报告
      if (data.type == 'WindPark') {
        // 更新下面的风场历史报告树, 右侧显示风场报告信息
        this.$emit('clickedNode', {
          clicked: 'windpark',
          deviceId: data.id
        })
        this.getWindparkHistoryTree()
      } else if (data.type == 'Windturbine') {
        // 更新下面的机组分析记录树，并加载机组分析记录及机组结论
        this.getTurbineAnalysisTree()
      } else if (data.type == 'WindturbineReport') {
        // 点击机组历史报告，右侧显示报告详情
        this.$emit('clickedNode', {
          clicked: 'report',
          deviceId: data.id
        })
      }
    },
    handleAnalysisTimeChange() {
      /*  let data = this.$refs.diagnosticReportTree.getCurrentNode()
      console.log(data) */
      this.getTurbineAnalysisTree()
    },
    getWindparkHistoryTree(id) {
      this.historyReportTreeLoading = true
      getWindparkHistoryTree({
        windParkId: this.selectedNode.id,
        startTime: this.monthPickerValue[0],
        endTime: this.monthPickerValue[1]
      })
        .then(res => {
          this.historyReportTreeList = res.data.data
          this.$nextTick(() => {
            if (id) {
              this.historyReportTreeCurrentNodeKey = id
              this.$refs.historyReportTree.setCurrentKey(id)
              this.historyReportTreeClick({ type: 'WindParkReport', id: id })
            }
          })
        })
        .finally(() => {
          this.historyReportTreeLoading = false
        })
    },
    historyReportTreeClick(data, node) {
      // 查看风场报告
      if (data.type == 'WindParkReport') {
        this.$emit('clickedNode', {
          clicked: 'windparkReport',
          deviceId: data.id
        })
      } else if (data.type == 'WindTurbineReport') {
        // 查看机组报告
        this.$emit('clickedNode', {
          clicked: 'report',
          deviceId: data.id
        })
      }
    },
    getTurbineAnalysisTree() {
      this.isTouchCheck = false
      this.treeDataLoading = true
      getTurbineAnalysisTree({
        windturbineId: this.selectedNode.id,
        startTime: this.analysisRecordsTime[0],
        endTime: this.analysisRecordsTime[1]
      })
        .then(res => {
          if (res.data.data.name == null) {
            this.treeData = []
            this.$emit('clickedNode', {
              clicked: 'turbine',
              deviceId: this.selectedNode.id,
              analysisData: [],
              turbineInfo: this.selectedNode
            })
          } else {
            this.treeData = [res.data.data]
            this.$nextTick(() => {
              this.defaultCheckedKeys = this.extractIds(res.data.data)
              this.$nextTick(() => {
                this.isTouchCheck = true
              })
            })
            this.$emit('clickedNode', {
              clicked: 'turbine',
              deviceId: this.selectedNode.id,
              analysisData: res.data.data,
              turbineInfo: this.selectedNode
            })
          }
        })
        .finally(() => {
          this.treeDataLoading = false
        })
    },
    extractIds(obj) {
      let ids = []
      // 检查对象是否有id属性
      if (obj.id) {
        ids.push(obj.id)
      }
      if (obj.children) {
        obj.children.forEach(item => {
          ids = ids.concat(this.extractIds(item))
        })
      }
      return ids
    }
  }
}
</script>

<style lang="less" scoped>
@import url('../styles/common.less');
@fontColor: #000;
.IES_tree_control {
  display: flex;
  flex-direction: column;
  width: 100%;
  height: 100%;
  .item_content_1 {
    width: 100%;
    height: 40px;
    //  background: #323232;
    margin-bottom: 5px;
    padding: 8px;
    :deep(.el-input__inner){
      background: #fff;
      height: 28px;
    }
    :deep(.el-input__prefix){
      top: -6px;
    }
  }
  .item_content_2 {
    width: 100%;
    height: 382px;
    background: #ecf0f7;
    margin-bottom: 3px;
    padding: 5px 10px;
    .item_content_2_title {
      /*   padding-top: 5px;
      padding-left: 10px;
      padding-right: 14px; */
      .title {
        color: @fontColor;
        font-size: 14px;
        font-weight: 500;
        line-height: 30px;
      }
      .item_content_2_layout {
        display: flex;
        flex-direction: row;
        font-size: 12px;
        align-items: center;
        justify-content: left; //space-between;
        margin-top: 7px;
        :deep(.el-input__inner){
          width: 260px;
          height: 28px;
          background: #fff;
        }
        :deep(.el-input__prefix){
          top: -6px;
        }

        :deep(.el-month-table td.today .cell){
          color: @fontColor;
          font-weight: 700;
        }
        :deep(.el-month-table td.current:not(.disabled) .cell){
          color: #409eff !important;
        }
      }
    }
    .item_content_2_content {
      padding: 5px 0;
      width: 327px;
      height: 276px;
      opacity: 1;
      background: #fff;
      overflow-y: auto;
      margin-top: 7px;
    }
    .btn_pos {
      position: absolute;
      right: 9px;
      margin-top: 4px;
    }
  }
  .item_content_3 {
    flex: 1;
    background: #ecf0f7;
    padding: 5px 10px;
    overflow: hidden;
    .item_content_2_title {
      /* padding-top: 5px;
      padding-left: 10px;
      padding-right: 14px; */
      .title {
        color: @fontColor;
        font-size: 14px;
        font-weight: 500;
        line-height: 30px;
      }
      .item_content_2_layout {
        display: flex;
        flex-direction: row;
        align-items: center;
        justify-content: space-between;
        margin-top: 7px;
        :deep(.el-input__inner){
          height: 28px;
          background: #fff;
        }
        :deep(.el-input__prefix){
          top: -6px;
        }
      }
    }
    .item_content_2_content {
      width: 327px;
      height: calc(100% - 150px);
      opacity: 1;
      background: #fff;
      overflow-y: auto;
      margin-top: 7px;
    }
    .btn_pos {
      position: relative;
      right: 10px;
      float: right;
      top: -37px;
    }

    :deep(.el-icon-date:before){
      position: absolute;
      top: -2px;
    }
  }
}

:deep(.el-tree){
  .el-tree-node__expand-icon {
    font-size: 20px;
  }
  .icon-font-btn {
    cursor: pointer;
    margin-left: 80px;
    font-size: 16px;
    color: #000;
    display: none;
    /*  &:hover {
      color: #fff;
    } */
  }

  .custom-tree-node {
    width: 100%;
    font-size: 14px;
    display: flex;
    flex-direction: row;
    align-items: center;
    &:hover {
      .icon-font-btn {
        display: block;
      }
    }
  }
  .tree-node-state {
    width: 7px;
    height: 17px;
    opacity: 1;
    // background: #00ff2b;
    border-radius: 5px;
    margin-left: 8px;
  }
  .el-checkbox__inner {
    border: 1px solid #d8d8d8;
  }
  .el-checkbox__input.is-checked .el-checkbox__inner,
  .el-checkbox__input.is-indeterminate .el-checkbox__inner {
    background-color: #409eff !important;
    border-color: #409eff !important;
  }

  .el-loading-parent--relative {
    height: calc(100% - 10px);
  }
}
:deep(.el-tree--highlight-current .el-tree-node.is-current > .el-tree-node__content){
  background: #32a9ff;
}
:deep(.el-date-editor){
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
</style>
<style lang="less">
.el-tree-node__content {
  height: 30px;
}
</style>
