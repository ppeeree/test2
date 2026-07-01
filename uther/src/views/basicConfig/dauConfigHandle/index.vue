<template>
  <el-main>
    <div class="merge_header">测点通道配置</div>
    <div style="padding-left: 10px; margin: 10px 0">
      <el-button type="primary" size="small" @click="settingCard = !settingCard">
        <i class="el-icon-circle-plus-outline" style="padding-right: 7px"></i>新增配置
      </el-button>
      <el-button style="margin: 0 10px" type="danger" size="small" @click="deleteData">
        <i class="el-icon-remove-outline" style="padding-right: 7px"></i>批量删除
      </el-button>
    </div>
    <el-table
      ref="multipleTable"
      size="mini"
      :data="tableData"
      height="82%"
      style="width: calc(100% - 30px); margin-left: 15px"
      :key="Math.random()"
      border
      :highlight-current-row="true"
      :header-cell-style="{ backgroundColor: '#F2F6FC', color: '#909399', textAlign: 'center' }"
      @selection-change="handleSelectionChange"
      class="tableLimit"
    >
      <el-table-column type="selection" width="55"></el-table-column>
      <el-table-column prop="stationName" label="风场名称"></el-table-column>
      <el-table-column prop="deviceName" label="机组名称"></el-table-column>
      <el-table-column prop="measLocName" label="测量位置" :cell-class-name="'table-cell-wrap'">
      </el-table-column>
      <el-table-column prop="chno" label="通道地址"></el-table-column>
      <el-table-column fixed="right" label="操作" width="240">
        <template slot-scope="scope">
          <el-button class="table_btn" @click="editData(scope.row)" type="text" size="small">
            <i class="el-icon-edit"></i> 修改
          </el-button>
          <el-button class="table_btn" @click="deleteData(scope.row)" type="text" size="small">
            <i class="el-icon-delete"></i> 删除
          </el-button>
        </template>
      </el-table-column>
    </el-table>
    <el-pagination
      @size-change="handleSizeChange"
      @current-change="handleCurrentChange"
      :current-page="currentPage"
      :page-sizes="[10, 20, 30, 50, 100]"
      :page-size="currentSize"
      layout="total, sizes, prev, pager, next, jumper"
      :total="totalNum"
    >
    </el-pagination>

    <!--新增-->
    <el-dialog
      title="新增测点映射配置"
      @before-close="beforeClose('ruleForm')"
      append-to-body
      :visible.sync="settingCard"
      width="1100px"
      destroy-on-close
      v-dialogDrag
      class="avue-dialog"
    >
      <el-form :model="ruleForm" ref="ruleForm" label-width="0px">
        <p class="list_title">风场ID映射</p>
        <div class="list_block">
          <div class="list_row"><span>风场名称</span><span>众芯ID</span><span>源ID</span></div>
          <div class="list_row">
            <span>
              <el-form-item prop="stationID" :rules="rules.stationID">
                <el-select size="small" v-model="ruleForm.stationID" @change="windparkSelectChange">
                  <el-option
                    v-for="item in userDeptTree"
                    :key="item.id"
                    :label="item.name"
                    :value="item.id"
                  >
                  </el-option>
                </el-select>
              </el-form-item>
            </span>
            <span>{{ ruleForm.stationID }}</span>
            <span>
              <el-form-item prop="mapStationID" :rules="rules.mapStationID">
                <el-input
                  size="small"
                  style="width: 140px"
                  v-model="ruleForm.mapStationID"
                  @blur="changemapStationID"
                ></el-input>
              </el-form-item>
            </span>
          </div>
        </div>

        <p class="list_title">机组ID映射</p>
        <div class="list_block list_dau">
          <div class="list_row list_row_dau">
            <span>机组名称</span><span>众芯ID</span><span>源ID</span>
          </div>
          <div class="list_row list_row_dau">
            <span>机组名称</span><span>众芯ID</span><span>源ID</span>
          </div>
          <div class="list_row list_row_dau" v-for="item in turbineList" :key="item.entityId">
            <span>{{ item.entityName }}</span>
            <span>{{ item.entityId }}</span>
            <span>
              <el-form-item :prop="`deviceID.${item.entityId}`" :rules="rules.deviceID">
                <el-input
                  size="small"
                  style="width: 140px"
                  v-model="ruleForm['deviceID'][item.entityId]"
                ></el-input>
              </el-form-item>
            </span>
          </div>
        </div>

        <p class="list_title">
          测点映射<b style="margin-left: 20px; color: #ff0000; font-size: 12px"
            >(注意：混塔-振温一体采集器输出通道+100)</b
          >
        </p>
        <div class="list_block list_dau">
          <div class="list_row list_row_dau"><span>测量位置名称</span><span>通道地址</span></div>
          <div class="list_row list_row_dau"><span>测量位置名称</span><span>通道地址</span></div>
          <div class="list_row list_row_dau" v-for="item in locationOptions" :key="item.key">
            <span>{{ item.value }}</span>
            <span>
              <el-form-item label="" :prop="`mapChno.${item.key}`" :rules="rules.chno">
                <el-input-number
                  :min="1"
                  :step="1"
                  :precision="0"
                  size="mini"
                  v-model="ruleForm['mapChno'][item.key]"
                  style="width: 100px"
                ></el-input-number>
              </el-form-item>
            </span>
          </div>
        </div>
      </el-form>
      <span slot="footer" class="dialog-footer">
        <el-button @click="settingCard = false">
          <i class="el-icon-circle-close" style="padding-right: 7px"></i>
          取 消
        </el-button>
        <el-button type="primary" @click.native="addConfig('ruleForm')">
          <i class="el-icon-circle-plus-outline" style="padding-right: 7px"></i>确 定
        </el-button>
      </span>
    </el-dialog>

    <!--修改-->
    <el-dialog
      title="修改测点映射配置"
      @before-close="beforeClose('editRuleForm')"
      append-to-body
      :visible.sync="editCard"
      width="500px"
      destroy-on-close
      v-dialogDrag
      class="avue-dialog"
    >
      <el-form :model="editRuleForm" :rules="rules" ref="editRuleForm" label-width="120px">
        <el-form-item label="风场：" prop="stationID">
          <el-input v-model="editRuleForm.stationName" type="text" disabled></el-input>
        </el-form-item>

        <el-form-item label="机组：" prop="deviceID">
          <el-input v-model="editRuleForm.deviceName" type="text" disabled></el-input>
        </el-form-item>

        <el-form-item label="测量位置：" prop="measLocName">
          <el-input v-model="editRuleForm.measLocName" type="text"></el-input>
        </el-form-item>

        <el-form-item label="通道地址：" prop="chno">
          <el-input-number
            controls-position="right"
            style="margin-right: 5px; width: 120px"
            :min="1"
            :step="1"
            :precision="0"
            v-model="editRuleForm.chno"
            placeholder=""
          ></el-input-number>
        </el-form-item>
      </el-form>
      <span slot="footer" class="dialog-footer">
        <el-button @click="editCard = false">
          <i class="el-icon-circle-close" style="padding-right: 7px"></i>
          取 消
        </el-button>
        <el-button type="primary" @click.native="addConfig('editRuleForm')">
          <i class="el-icon-circle-plus-outline" style="padding-right: 7px"></i>确 定
        </el-button>
      </span>
    </el-dialog>
  </el-main>
</template>
<script>
import { setTheme } from '@/util/util'
import { mapGetters } from 'vuex'
import {
  getMeasIDByStationIDApi,
  getHADUConfigListApi,
  addHADUConfigApi,
  updateHADUConfigApi,
  deleteHADUConfigApi
} from '@/api/basicConfig/dau.js'
export default {
  data() {
    return {
      searchOption: [],
      totalNum: 0, // 所有个数
      tableData: [],
      currentPage: 1, // 当前表格在第几页
      currentSize: 30, // 当前也共有几条数据
      settingCard: false, //新建弹框是否展示

      selectTaskList: [], // 选中行

      // windparkList: [], //风场列表
      turbineList: [], // 该风场下的机组列表
      locationOptions: [],

      rules: {
        stationID: [{ required: true, message: '！', trigger: 'change' }],
        mapStationID: [{ required: true, message: '！', trigger: 'change' }],
        measLocName: [{ required: true, message: '请输入测量位置名称', trigger: 'change' }],
        chno: [{ required: true, message: '!', trigger: 'change' }],
        deviceID: [{ required: true, message: '!', trigger: 'change' }]
      },
      ruleForm: {
        stationID: '',
        mapStationID: '',
        deviceID: {},
        mapChno: {}
      },
      editCard: false,
      editRuleForm: {
        stationName: '',
        deviceName: '',
        stationID: '',
        deviceID: '',
        measLocID: '',
        measLocName: '',
        chno: ''
      }
    }
  },
  computed: {
    ...mapGetters(['userDeptTree'])
  },
  created() {
    this.$store.commit('SET_THEME_NAME', 'theme-white')
    setTheme('theme-white')
  },
  mounted() {
    this.ruleForm.stationID = this.userDeptTree[0].id
    this.windparkSelectChange(this.userDeptTree[0].id)
    this.getTableDatas()
  },
  methods: {
    // 获取风场下测点列表接口
    getMealDByStationId() {
      getMeasIDByStationIDApi({
        stationId: this.ruleForm.stationID.toString()
      }).then(data => {
        this.locationOptions = data.data.data
        data.data.data.forEach((i, index) => {
          this.$set(this.ruleForm.mapChno, i.key, index + 1)
        })
      })
    },
    // 修改风场源ID，机组源ID也进行改变
    changemapStationID() {
      if (this.ruleForm.stationID !== this.ruleForm.mapStationID) {
        Object.keys(this.ruleForm.deviceID).forEach(key => {
          this.ruleForm.deviceID[key] = key.replace(
            new RegExp(this.ruleForm.stationID, 'g'),
            this.ruleForm.mapStationID
          )
        })
      }
    },

    // 2.1、获取表格数据
    getTableDatas() {
      getHADUConfigListApi({
        current: this.currentPage,
        size: this.currentSize
      })
        .then(data => {
          this.tableData = data.data.data
          this.totalNum = data.data.totalCount
        })
        .catch(error => console.error('Unable to get items.', error))
    },

    //

    // 3、表格：新增配置
    addConfig(formName) {
      this.$refs[formName].validate(valid => {
        if (valid) {
          let param = {}
          if (formName == 'editRuleForm') {
            param = this.editRuleForm
            this.editCard = false
          } else {
            let deviceIDList = Object.keys(this[formName].deviceID).map(item => {
              return `${item},${this[formName].deviceID[item]}`
            })
            let measLocList = Object.keys(this[formName].mapChno).map(item => {
              return `${item},${this[formName].mapChno[item]}`
            })
            param = {
              stationID: this[formName].stationID,
              mapStationID: this[formName].mapStationID,
              deviceIDList: deviceIDList,
              measLocList: measLocList
            }
            this.settingCard = false
          }
          this.addConfigApi(param, formName == 'editRuleForm')
        }
      })
    },

    // 3.1、新增、编辑
    addConfigApi(param, isEdit) {
      if (isEdit) {
        updateHADUConfigApi(param).then(data => {
          if (data.data.success) {
            this.$message.success('操作成功！')
          }
          this.getTableDatas() // 在请求成功后调用 getTableDatas 方法
        })
      } else {
        addHADUConfigApi(param).then(data => {
          if (data.success) {
            this.$message.success('操作成功！')
          }
          this.getTableDatas() // 在请求成功后调用 getTableDatas 方法
        })
      }
    },

    // 4.1、表格：删除
    deleteData(row) {
      this.$confirm('确定将选择数据删除?', {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning'
      }).then(() => {
        this.deleteTaskApi(row)
      })
    },

    // 4.2、删除数据接口
    deleteTaskApi(data) {
      let measIDs = ''
      if (data && data.measLocID) {
        measIDs = data.measLocID
      } else {
        measIDs = Array.from(this.selectTaskList, i => i.measLocID).join(',')
      }
      deleteHADUConfigApi({
        measIDs: measIDs.toString()
      })
        .then(data => {
          if (data.data.success) {
            this.$message({ type: 'success', message: '删除成功!' })
            this.getTableDatas()
          } else {
            this.$message({ type: 'danger', message: '删除失败！' })
          }
        })
        .catch(error => console.error('Unable to get items.', error))
    },

    // 5.1、选中风场改变
    windparkSelectChange(val) {
      this.ruleForm.mapStationID = val
      this.turbineList = this.userDeptTree.find(i => i.id == val).childNode
      this.$set(this.ruleForm, 'deviceID', {})
      this.turbineList.forEach(item => {
        this.$set(this.ruleForm.deviceID, item.entityId, item.entityId)
      })
      this.getMealDByStationId()
    },

    // 5.2、多选框改变
    handleSelectionChange(val) {
      this.selectTaskList = val
    },

    // 5.3、分页size改变
    handleSizeChange(val) {
      this.currentSize = val
      this.getTableDatas()
    },
    // 5.4、分页改变
    handleCurrentChange(val) {
      this.currentPage = val
      this.getTableDatas()
    },

    // 点击编辑
    editData(rowData) {
      this.editCard = true
      this.editRuleForm = rowData
    }
  }
}
</script>
<style lang="scss" scoped>
::v-deep .el-checkbox__label {
  color: #909399 !important;
}
.el-main {
  height: 100%;
  width: 100%;
  padding: 0;
  overflow: unset;
  color: #000;
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
}

.list_block {
  width: 100%;
  height: auto;
  margin-bottom: 20px;
  overflow: hidden;
}

.list_row {
  width: 100%;
  height: 40px;
  border-bottom: 1px solid #eee;
  display: flex;
  flex-direction: row;
  align-items: center;
  align-content: center;
  justify-items: center;
  justify-content: space-around;
  box-sizing: border-box;
}

.list_row > span {
  display: inline-block;
  width: 33%;
  text-align: center;
}

.list_title {
  width: 100%;
  margin: 0;
  font-weight: bolder;
  margin-bottom: 5px;
  color: #000;
}

.list_row .el-form-item {
  margin-bottom: 0px;
}

.list_row .el-form-item__content {
  margin-left: 0 !important;
}

.list_dau {
  border-top: 1px solid #eee;
  border-left: 1px solid #eee;
  border-bottom: 1px solid #eee;
}

.list_row_dau {
  width: 50%;
  float: left;
  border-right: 1px solid #eee;
}

.list_row_dau > span {
  width: 50%;
}

.el-form-item__error {
  display: none;
}
.el-dialog__header {
  padding: 20px 20px 10px;
}
.el-dialog__body {
  max-height: 70vh;
  overflow: auto;
  padding: 0 20px;
  color: #606266;
  font-size: 14px;
  word-break: break-all;
}
</style>
