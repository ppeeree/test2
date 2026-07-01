<template>
  <basic-container class="tree_table">
    <span class="table_title">{{ treeClick.windturbineName }}</span>
    <div style="float: right; margin: 15px 0px">
      <el-button
        type="primary"
        size="small"
        plain
        @click="removeItem(tableData)"
        style="margin-right: 25px"
        >解除应用
      </el-button>
      <el-button type="primary" size="small" plain @click="addBox = true">模型应用 </el-button>
    </div>
    <el-table
      ref="multipleTable"
      :data="tableData"
      tooltip-effect="dark"
      style="width: 100%"
      height="calc(100% - 42px)"
      @selection-change="handleSelectionChange"
    >
      <el-table-column type="index" width="50" label="#"> </el-table-column>
      <el-table-column prop="windparkName" label="风场"> </el-table-column>
      <el-table-column prop="windturbineName" label="机组"> </el-table-column>
      <el-table-column prop="alarmModelName" label="报警模型"> </el-table-column>
      <el-table-column fixed="right" label="操作">
        <template slot-scope="scope">
          <el-button @click="removeItem(scope.row)" type="text" size="small">解除应用</el-button>
        </template>
      </el-table-column>
    </el-table>

    <el-dialog
      title="选择绑定报警定义"
      append-to-body
      :visible.sync="addBox"
      width="450px"
      custom-class="tree_add_card"
      style="margin-top: 25vh"
    >
      <div class="dialod_content">
        <el-form
          :model="addModelForm"
          label-width="120px"
          :rules="addRules"
          ref="addRuleForm"
          style="padding: 0px 27px 10px 5px"
        >
          <el-form-item label="风场名称：" prop="windparkName">
            <el-select
              v-model="addModelForm.windparkName"
              placeholder="请选择风场"
              @change="windParkChange"
            >
              <el-option
                v-for="item in windList"
                :key="item.id"
                :label="item.name"
                :value="item.id"
              ></el-option>
            </el-select>
          </el-form-item>
          <el-form-item label="机组名称：" prop="windturbineName">
            <el-select
              filterable
              clearable
              multiple
              collapse-tags
              placeholder="请选择机组"
              v-model="addModelForm.windturbineName"
              @change="selectChange"
            >
              <el-option label="全选" value="selectAll" v-if="turbineList.length > 0"></el-option>
              <el-option
                v-for="item in turbineList"
                :key="item.entityId"
                :label="item.entityName"
                :value="item.entityId"
              >
              </el-option>
            </el-select>
          </el-form-item>
          <el-form-item label="报警模型选择：" prop="alarmType">
            <el-select
              v-model="addModelForm.alarmType"
              placeholder="请选择报警模型"
              @change="compIndexChange"
            >
              <el-option
                v-for="item in allModelList"
                :key="item.id"
                :label="item.modelName"
                :value="item.id"
              ></el-option>
            </el-select>
          </el-form-item>
        </el-form>
      </div>
      <span slot="footer" class="dialog-footer" style="right: 25%">
        <el-button type="primary" @click="submitRole"
          ><i class="el-icon-circle-plus-outline"></i>保 存</el-button
        >
        <el-button @click="addBox = false"><i class="el-icon-circle-close"></i>取 消</el-button>
      </span></el-dialog
    >
  </basic-container>
</template>

<script>
import debounce from 'lodash/debounce'
import { getStore } from '@/util/store.js'
// import { getWindParkList, getTurbineList } from '../../equipment/component/windList'
import { add, remove } from '@/api/alarmFault/alarmTree'
import multipleSelect from './multipleSelect.vue'
import { mapGetters } from 'vuex'

export default {
  components: { multipleSelect },
  data() {
    return {
      addBox: false,
      mulSelectLoading: false,
      query: {},
      treeClick: {}, //左侧树点击
      tableData: [], //表格数据
      windList: [], // 新增--风场列表
      turbineList: [], // 新增--机组列表
      allModelList: [], // 新增--模型列表
      addModelForm: {
        windparkName: '',
        windturbineName: [],
        alarmType: ''
      },
      oldSeleValue: [],
      addRules: {
        windparkName: [{ required: true, message: '请选择风场名称', trigger: 'change' }],
        windturbineName: [{ required: true, message: '请选择机组', trigger: 'change' }],
        alarmType: [{ required: true, message: '请选择报警模型', trigger: 'change' }]
      },

      page: {
        total: 0, // 总页数
        currentPage: 1, // 当前页数
        pageSize: 10 // 每页显示多少条
      },
      Option: {
        height: 'auto',
        calcHeight: 30,
        dialogWidth: 600,
        dialogDrag: true, // 弹窗拖拽
        tip: false,
        border: true,
        dialogClickModal: false,
        addBtn: false,
        editBtn: false,
        index: true,
        delBtn: false,
        selection: true,
        emptyBtnText: '重置',
        emptyBtnIcon: 'el-icon-refresh',
        column: [
          {
            label: '风场',
            prop: 'windparkName'
          },
          {
            label: '机组',
            prop: 'windturbineName'
          },
          {
            label: '报警模型',
            prop: 'alarmModelName'
          }
        ]
      }
    }
  },
  watch: {},
  computed: {
    ...mapGetters(['userDeptTree']),
    allSeleValue() {
      return this.turbineList.map(v => v.entityId)
    }
  },
  mounted() {
    this.windList = this.userDeptTree //getWindParkList()
    this.allModelList = JSON.parse(getStore({ name: 'allModelList' }))
  },
  methods: {
    //左侧树点击节点
    clickItem(val) {
      this.treeClick = val
      this.tableData = []
      this.addModelForm = {
        windparkName: '',
        windturbineName: '',
        alarmType: ''
      }
      if (val.children) {
        this.addModelForm.windparkName = val.windparkId
        this.windparkChange(val.windparkId)
        this.tableData = val.children
      } else {
        let list = []
        list.push(val.windturbineId)
        this.addModelForm.windparkName = val.windparkId
        this.addModelForm.windturbineName = list
        this.tableData.push(val)
      }

      console.log('最终表格数据', val)
    },

    //表格新增风场机组数据
    windparkChange(val) {
      this.turbineList = this.windList.find(item => item.id == val).childNode
    },

    //解除模型
    removeItem(row) {
      let ids = []
      if (row.length) {
        row.forEach(e => {
          ids.push(e.windturbineId)
        })
      } else {
        ids.push(row.windturbineId)
      }

      this.$confirm('确定解除列表中绑定模型?', {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning'
      })
        .then(() => {
          return remove(ids.join(','))
        })
        .then(() => {
          this.$emit('getTree')
          this.$message({
            type: 'success',
            message: '模型解除成功!'
          })
        })
    },

    //新建保存按钮
    submitRole: debounce(function () {
      this.$refs['addRuleForm'].validate(valid => {
        if (valid) {
          let { windparkName, windturbineName, alarmType } = this.addModelForm
          const param = {
            alarmModelId: alarmType,
            windparkId: windparkName,
            windturbineId: windturbineName.filter(j => j !== 'selectAll').join(',')
          }

          add({ ...param }).then(res => {
            this.addBox = false
            if (res.data.code == 200) {
              this.$message({ type: 'success', message: '模型应用成功！' })
              this.$emit('getTree')
            }
          })
        }
      })
    }, 700),

    //全选
    selectChange(val) {
      const oldVal = this.oldSeleValue.length === 1 ? this.oldSeleValue[0] : []
      if (val.includes('selectAll')) {
        this.addModelForm.windturbineName = Object.assign([], this.allSeleValue)
        this.addModelForm.windturbineName.push('selectAll')
      }
      if (oldVal.includes('selectAll') && !val.includes('selectAll'))
        this.addModelForm.windturbineName = []
      if (oldVal.includes('selectAll') && val.includes('selectAll')) {
        const index = val.indexOf('selectAll')
        val.splice(index, 1)
        this.addModelForm.windturbineName = val
      }
      if (!oldVal.includes('selectAll') && !val.includes('selectAll')) {
        if (val.length === this.allSeleValue.length) {
          this.addModelForm.windturbineName.push('selectAll')
        }
      }
      this.oldSeleValue[0] = this.addModelForm.windturbineName
    }
  }
}
</script>

<style lang="less" scoped>
@import url('./style.less');
.tree_table {
  height: calc(100% - 14px);
  width: calc(100% - 430px);
  float: right;
  ::v-deep .el-card.is-always-shadow {
    height: 100%;
    .el-card__body {
      height: 100%;
    }
  }
  .table_title {
    font-size: 14px;
    font-weight: bold;
    color: white;
  }
}
::v-deep .el-table__empty-block {
  height: auto !important;
}
.tree_add_card {
  margin-top: 38vh !important;
  ::v-deep .el-dialog {
    margin-top: 38vh !important;
    .el-dialog__body {
      .dialod_content {
        .el-form-item {
        }
      }
    }
    .el-dialog__footer {
      .dialog-footer {
        right: 20%;
      }
    }
  }
}
::v-deep .el-form-item__label {
  text-align: right;
}
::v-deep .avue-crud__left {
  position: relative;
  right: -82%;
}
.el-table::before {
  height: 0;
}
::v-deep .el-table__fixed-right-patch,
.el-table .el-table__cell.gutter {
  background-color: #2a65ae;
}
</style>
