<template>
  <div class="containter_box">
    <div class="merge_header">模型配置</div>
    <div class="merge_content">
      <left-tree class="leftTree" @clickedNode="initTable" :tree-data="treeData" ref="leftTree"></left-tree>
      <Splitter splitType="vertical" id="areaSplit" style="flex: 0 0 6px" :limit="{}" />
      <div class="content_right">
        <div style="width: 100%; height: calc(100% - 35px); padding: 0 15px">
          <h4>
            诊断模型列表
            <el-button type="primary" @click="openEdit()">新增模型</el-button>
          </h4>
          <!-- 表格 -->
          <el-table ref="table" :data="tableData" border stripe size="small" style="width: 100%" height="85%">
            <el-table-column prop="name" label="模型名称" />
            <el-table-column prop="modelType" label="专病类型" />
            <el-table-column fixed="right" label="操作">
              <template #default="scope">
                <el-button type="text" size="small" @click="openEdit(scope.row)">编辑</el-button>
                <el-button type="text" size="small" @click="deleteModel(scope.row)">删除</el-button>
              </template>
            </el-table-column>
          </el-table>
        </div>
      </div>
    </div>
    <model-dialog ref="addModel" v-model:show="dialogVisible" @success="handleSuccess"></model-dialog>
  </div>
</template>
<script>
import { setTheme } from '@/util/util'
import leftTree from './components/leftTree.vue'
import modelDialog from './components/modelDialog.vue'
import Splitter from '@/components/splitter/index.vue'
import { getModelTreeApi, deleteModelApi } from '@/api/intelliDiag'
export default {
  components: { leftTree, Splitter, modelDialog },
  data() {
    return {
      dialogVisible: false, // 控制弹窗显示的变量
      currentRow: null,
      tableData: [],
      treeData: [],
      loading: false
    }
  },

  created() {
    this.$store.commit('SET_THEME_NAME', 'theme-white')
    setTheme('theme-white')
  },
  mounted() {
    this.initData()
  },
  methods: {
    openEdit(row) {
      let modelTypeInfo = {}
      if (row) {
        modelTypeInfo.id = row.modelTypeCode
        modelTypeInfo.name = row.modelType
      } else {
        let { modelType, modelTypeCode, name, id } = this.$refs.leftTree.getCurrentNode()
        modelTypeInfo.name = modelType || name
        modelTypeInfo.id = modelTypeCode || id
      }
      // 获取默认参数
      this.$refs.addModel.initData(modelTypeInfo, row)
      this.dialogVisible = true
    },
    handleSuccess(data) {
      this.initData()
    },
    initTable(nodeData) {
      if (nodeData) {
        let { children } = nodeData
        if (children.length) {
          this.tableData = children
        } else {
          this.tableData = [{ ...nodeData }]
        }
      } else {
        this.tableData = this.treeData[0].children
      }
    },
    initData() {
      getModelTreeApi().then(res => {
        const { success, message, data } = res.data
        if (success) {
          this.treeData = data.map(item => ({
            ...item,
            children: item.children?.map(child => ({
              ...child,
              modelType: item.name,
              modelTypeCode: item.id
            }))
          }))
          // this.initTable()
        } else {
          this.$message.error(message)
        }
      })
    },
    deleteModel(row) {
      this.$confirm('此操作将永久删除该模型, 是否继续?', '提示', {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning'
      }).then(() => {
        deleteModelApi({ modelID: row.id }).then(res => {
          const { success, message } = res.data
          if (success) {
            this.$message.success('删除成功')
            this.initData()
          } else {
            this.$message.error(message)
          }
        })
      }).catch(() => {
        this.$message({
          type: 'info',
          message: '已取消删除'
        })
      })
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
