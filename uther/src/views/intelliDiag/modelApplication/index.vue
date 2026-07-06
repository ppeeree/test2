<template>
  <div class="containter_box">
    <div class="merge_header">模型应用</div>
    <div class="merge_content">
      <left-tree class="leftTree" @clickedNode="getList" ref="tree"></left-tree>
      <Splitter splitType="vertical" id="areaSplit" style="flex: 0 0 6px" :limit="{}" />
      <div class="content_right">
        <div style="width: 100%; height: calc(100% - 35px); padding: 0 15px">
          <h4>
            {{ selectedWindpark.name }} - 机组模型应用列表
            <el-button type="primary" @click="addBinding">绑定模型</el-button>
          </h4>
          <!-- 表格 -->
          <el-table
            ref="multipleTable"
            :data="tableData"
            border
            stripe
            size="mini"
            row-key="windTurbineID"
            style="width: 100%"
            height="95%"
            @row-click="rowClick"
            @selection-change="handleSelectionChange"
          >
            <!-- 1. 批量操作勾选列 -->
            <el-table-column type="selection" width="55" align="center"></el-table-column>
            <el-table-column prop="windTurbineName" label="机组名称" width="100" />
            <el-table-column
              prop="highSpeedShaftGearCount"
              label="平行级高速轴齿轮齿数"
              width="150"
            />
            <el-table-column prop="ratedGeneratorSpeed" label="高速轴额定转速" width="150" />
            <el-table-column prop="ratedSpeed" label="发电机额定转速" width="150" />
            <el-table-column prop="ndeModel" label="发电机驱动端轴承型号" width="150" />
            <el-table-column prop="deeModel" label="发电机非驱动端轴承型号" width="170" />
            <el-table-column prop="linkModels" label="应用诊断模型">
              <template #default="scope">
                {{ scope.row.linkModels.join('、') }}
              </template>
            </el-table-column>
            <el-table-column label="操作" width="100" align="center">
              <template #default="scope">
                <el-button type="text" size="mini" @click="editBinding(scope.row)"
                  >修改绑定</el-button
                >
              </template>
            </el-table-column>
          </el-table>
        </div>
      </div>
    </div>
    <!-- 模型绑定弹窗 -->
    <model-bind-dialog
      v-model:visible="dialogVisible"
      :row-data="currentRow"
      :model-tree="modelTree"
      @save="handleSaveBinding"
    />
  </div>
</template>
<script>
import { setTheme } from '@/util/util'
import leftTree from './components/leftTree.vue'
import modelBindDialog from './components/modelBindDialog.vue'
import Splitter from '@/components/splitter/index.vue'
import { applyModelApi, getModelApplyListApi, getModelTreeApi } from '@/api/intelliDiag'
export default {
  components: { leftTree, Splitter, modelBindDialog },
  data() {
    return {
      dialogVisible: false,
      currentRow: {},
      checkedRows: [],
      tableData: [],
      loading: false,
      modelTree: [],
      selectedWindpark: {}
    }
  },

  created() {
    this.$store.commit('SET_THEME_NAME', 'theme-white')
    setTheme('theme-white')
    this.getModelTreeData()
  },
  mounted() {},
  methods: {
    getModelTreeData() {
      getModelTreeApi().then(res => {
        const { success, message, data } = res.data
        if (success) {
          this.modelTree = data
        } else {
          this.$message.error(message)
        }
      })
    },
    addBinding() {
      if (this.checkedRows.length === 0) {
        this.$message.error('请选择机组')
        return
      }
      this.currentRow = {}
      this.dialogVisible = true
    },
    editBinding(row) {
      this.dialogVisible = true
      this.currentRow = row
    },
    // 处理保存绑定
    handleSaveBinding(bindIDList) {
      this.dialogVisible = false
      let windtuyineID = this.currentRow.windTurbineID
        ? [this.currentRow.windTurbineID]
        : this.checkedRows.map(item => item.windTurbineID)
      applyModelApi({
        ModelIDs: bindIDList,
        WindturbineIDs: windtuyineID
      }).then(res => {
        const { success, message } = res.data
        if (success) {
          this.$message.success('绑定成功')
          this.getList()
        } else {
          this.$message.error(message)
        }
      })
    },
    // 获取列表数据
    getList(data) {
      this.loading = true
      if (data?.id) {
        this.selectedWindpark = data
      }
      getModelApplyListApi({ windParkID: this.selectedWindpark.id })
        .then(res => {
          const { success, message, data } = res.data
          if (success) {
            this.tableData = data
          } else {
            this.$message.error(message)
          }
        })
        .finally(() => {
          this.loading = false
        })
    },
    rowClick(row) {
      this.$refs.multipleTable.toggleRowSelection(row)
    },
    handleSelectionChange(val) {
      this.checkedRows = val
    }
  }
}
</script>
<style scoped lang="scss">
.containter_box {
  width: 100%;
  height: 100%;
  .merge_header {
    height: 33px;
    margin-bottom: 3px;
    font-size: 16px;
    font-weight: bold;
    line-height: 33px;
    padding: 0 17px;
    position: relative;
    &::after {
      content: '';
      width: 100%;
      height: 3px;
      background: #ccc;
      position: absolute;
      left: 0;
      bottom: 0;
    }
  }
  .merge_content {
    width: 100%;
    height: calc(100% - 33px);
    display: flex;
    .leftTree {
      flex: 0 0 280px;
      overflow: hidden;
      height: 100%;
    }

    .content_right {
      flex: 1 1 0%;
      overflow: hidden;
      height: 100%;
      h3 {
        width: 100%;
        height: 30px;
        line-height: 30px;
        font-size: 14px;
        padding: 0 15px;
        position: relative;
        font-weight: normal;
        &::after {
          content: '';
          width: 100%;
          height: 2px;
          background: #ccc;
          position: absolute;
          left: 0;
          bottom: 0;
        }
      }
      h4 {
        width: 100%;
        height: 30px;
        line-height: 30px;
        font-size: 16px;
        margin: 10px 0;
        padding: 0 15px;
        color: #333b69;
        .el-button {
          float: right;
          right: 20px;
          top: 10px;
          cursor: pointer;
        }
      }
    }
  }
  :deep(.avue-crud__menu){
    display: none !important;
  }
}
</style>
<style lang="scss">
.white-popover {
  background-color: #fff;
  padding: 10px;
}
</style>
