<template>
  <!-- 采集单元管理 -->
  <div>
    <timeLine isActive="4" />
    <basic-container>
      <avue-crud
        :option="unitOption"
        :table-loading="loading"
        :data="data"
        ref="crud"
        v-model="form"
        :page.sync="page"
        :before-open="beforeOpen"
        @row-del="rowDel"
        @row-save="rowSave"
        @row-update="rowUpdate"
        @search-change="searchChange"
        @search-reset="searchReset"
        @current-change="currentChange"
        @size-change="sizeChange"
        @refresh-change="refreshChange"
        @on-load="onLoad"
        @selection-change="selectionChange"
      >
        <template slot="menuLeft">
          <el-button type="primary " size="small" plain icon="el-icon-plus" @click="addBox = true"
            >新 增
          </el-button>
          <el-button type="danger" size="small" plain icon="el-icon-delete" @click="handleDelete"
            >删 除
          </el-button>
        </template>
        <template slot="menu" slot-scope="{ row }">
          <span class="edit">
            <span class="editImg" />
            <span @click="editItem(row)" class="editBtn">编 辑</span></span
          >
        </template>
      </avue-crud>
    </basic-container>
    <el-dialog
      title="新 增"
      append-to-body
      :visible.sync="addBox"
      width="850px"
      custom-class="add_card"
    >
      <div class="dialod_content">
        <div class="seting_title"><img src="/img/equipment/addUnit.png" alt="" />创建采集单元</div>
        <el-form :model="addUnitForm" label-width="80px" :rules="addRules" ref="addRuleForm">
          <turbineSelect
            :addCompForm="addUnitForm"
            :addRules="addRules"
            @turbineChange="turbineChange"
          ></turbineSelect>
          <el-form-item label="采集单元类型：" prop="unitType">
            <el-select v-model="addUnitForm.unitType" placeholder="请选择采集单元类型">
              <el-option
                v-for="item in typeList"
                :key="item.value"
                :label="item.name"
                :value="item.value"
              >
              </el-option>
            </el-select>
          </el-form-item>
        </el-form>
        <unitIp ref="ipUnit" :list="turbineNameList" @ipChange="ipChange"></unitIp>
        <div class="seting_title"><img src="/img/equipment/mateSpot.png" alt="" />通道匹配测点</div>
        <matePassVue
          ref="matePass"
          :turbineList="addUnitForm.turbineName"
          @measureList="measureList"
        ></matePassVue>
      </div>

      <span slot="footer" class="dialog-footer">
        <el-button type="primary" @click="submitRole"
          ><i class="el-icon-circle-plus-outline"></i>保 存</el-button
        >
        <el-button @click="addBox = false"><i class="el-icon-circle-close"></i>取 消</el-button>
      </span>
    </el-dialog>
    <el-dialog
      title="编 辑"
      append-to-body
      :visible.sync="editBox"
      width="850px"
      custom-class="edit_card"
    >
      <div class="dialod_content">
        <div class="seting_title">
          <img src="/img/equipment/addUnit.png" alt="" />采集单元基本信息
        </div>
        <span v-for="upItem in editInformation[0]" :key="upItem" class="no_edit">
          <span class="edit_title"> {{ upItem.name }}：</span
          ><span class="edit_input">{{ upItem.value }}</span>
        </span>
        <div class="seting_title" style="margin-top: 17px">
          <img src="/img/equipment/mateSpot.png" alt="" />通道测点信息
        </div>
        <span v-for="downItem in editInformation[1]" :key="downItem" class="no_edit">
          <span class="edit_title"> {{ downItem.name }}：</span
          ><el-input v-model="downItem.value"></el-input>
        </span>
      </div>
      <span slot="footer" class="dialog-footer">
        <el-button type="primary" @click="editRole"
          ><i class="el-icon-circle-plus-outline"></i>保 存</el-button
        >
        <el-button @click="editBox = false"><i class="el-icon-circle-close"></i>取 消</el-button>
      </span>
    </el-dialog>
  </div>
</template>

<script>
import debounce from 'lodash/debounce'
import { mapGetters } from 'vuex'
import timeLine from '../component/timeLine.vue'
import unitIp from './unit.vue'
import matePassVue from './matePass.vue'
import { getList, add, remove, update } from '@/api/equipment/unit'
import turbineSelect from '../component/turbineSelect.vue'

export default {
  components: { timeLine, unitIp, matePassVue, turbineSelect },
  data() {
    return {
      turbineNameList: [],
      ipList: [],
      unitList: [],
      selectionList: [], //表格选中数组
      editItemList: [], //点击编辑的数据
      editInformation: [], //点击编辑展示数据
      query: {},
      addBox: false,
      editBox: false,
      loading: true,
      page: {
        total: 0, // 总页数
        currentPage: 1, // 当前页数
        pageSize: 10 // 每页显示多少条
      },
      unitOption: {
        height: 'auto',
        calcHeight: 30,
        dialogWidth: 600,
        dialogDrag: true, // 弹窗拖拽
        tip: false,
        border: true,
        dialogClickModal: false,
        // menu: false,
        addBtn: false,
        editBtn: false,
        delBtn: false,
        index: true,
        selection: true,
        emptyBtnText: '重置',
        emptyBtnIcon: 'el-icon-refresh',
        column: [
          {
            label: '风场名称',
            prop: 'windparkName',
            search: true,
            type: 'select',
            props: {
              label: 'name',
              value: 'id'
            },
            dicData: [],
            overHidden: true,
            cascader: ['windturbineName'],
            change: this.windparkChange
          },
          {
            label: '机组名称',
            prop: 'windturbineName',
            search: true,
            type: 'select',
            props: {
              label: 'entityName',
              value: 'entityId'
              /*  label: 'name',
              value: 'value' */
            },
            dicData: [],
            filterable: true
          },
          {
            label: '采集单元名称',
            prop: 'dauName',
            search: true,
            type: 'select',
            props: {
              label: 'name',
              value: 'name'
            },
            dicData: []
          },
          {
            label: '采集单元类型',
            prop: 'dauType',
            search: true,
            type: 'select',
            props: {
              label: 'name',
              value: 'value'
            },
            dicData: [
              {
                name: 'WTPHM.DT',
                value: 'WTPHM.DT'
              },
              {
                name: 'WTPHM.BT',
                value: 'WTPHM.BT'
              }
            ]
          },
          {
            label: 'IP',
            prop: 'ipaddress'
          },
          {
            label: '通道',
            prop: 'channelNumber'
          },
          {
            label: '测点名称',
            prop: 'measlocName'
          }
        ]
      },
      addUnitForm: {
        windparkName: '',
        turbineName: '',
        unitType: ''
      },
      addRules: {
        windparkName: [{ required: true, message: '请选择风场名称', trigger: 'change' }],
        turbineName: [{ required: true, message: '请选择机组名称', trigger: 'change' }],
        unitType: [{ required: true, message: '请选择采集单元类型', trigger: 'change' }]
      },
      typeList: [
        {
          name: 'WTPHM.DT',
          value: 'WTPHM.DT'
        },
        {
          name: 'WTPHM.BT',
          value: 'WTPHM.BT'
        }
      ]
    }
  },
  watch: {
    addBox: {
      handler() {
        if (!this.addBox) {
          this.addUnitForm = {
            windparkName: '',
            turbineName: '',
            unitType: ''
          }
          this.turbineNameList = []
        }
      },
      immediate: true
    }
  },
  computed: {
    ...mapGetters(['userDeptTree']),
    ids() {
      let ids = []
      this.selectionList.forEach(ele => {
        ids.push({ dauId: ele.dauId, channelNumber: ele.channelNumber })
      })
      return ids
    }
  },
  mounted() {
    const wind = this.findObject(this.unitOption.column, 'windparkName')
    wind.dicData = this.userDeptTree
  },
  methods: {
    windparkChange(val) {
      const turbineId = this.findObject(this.unitOption.column, 'windturbineName')
      turbineId.dicData = this.userDeptTree.find(i => i.id == val.value).childNode
    },
    //表格--加载
    onLoad(page, params = {}) {
      let obj = {
        offset: page.currentPage,
        pageSize: page.pageSize,
        ...Object.assign(params, this.query)
      }
      this.loading = true

      getList({ ...obj }).then(res => {
        this.loading = false
        if (res.data.code == 200 && res.data.data) {
          const data = res.data.data.data
          this.page.total = res.data.data.totalCount
          this.data = data

          const option = res.data.data.searchOptList
          /*  const wind = this.findObject(this.unitOption.column, 'windparkName')
          wind.dicData = option.windParkList

          const turbine = this.findObject(this.unitOption.column, 'windturbineName')
          turbine.dicData = option.windturbineList */

          const dauName = this.findObject(this.unitOption.column, 'dauName')
          dauName.dicData = option.devDauList
        }
      })
    },
    //表格--搜索
    searchChange(params, done) {
      let newparam = {
        windParkId: params.windparkName,
        windturbineId: params.windturbineName,
        dauType: params.dauType,
        dauName: params.dauName
      }

      this.query = newparam
      this.page.currentPage = 1
      this.onLoad(this.page, newparam)
      done()
    },
    //表格--重置
    searchReset() {
      this.query = {}
      this.onLoad(this.page)
    },
    //表格--选中
    selectionChange(list) {
      this.selectionList = list
    },
    //表格--删除
    handleDelete() {
      console.log('ids', this.ids)
      if (this.selectionList.length === 0) {
        this.$message.warning('请选择至少一条数据')
        return
      }
      this.$confirm('确定将选择数据删除?', {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning'
      })
        .then(() => {
          return remove({ dauUniteChannels: this.ids })
        })
        .then(() => {
          this.data = []
          this.$refs.crud.refreshTable()
          this.$refs.crud.toggleSelection()
          this.onLoad(this.page)
          this.$message({
            type: 'success',
            message: '采集单元删除成功!'
          })
        })
    },
    //表格--编辑
    editItem(row) {
      this.editBox = true
      this.editItemList = JSON.parse(JSON.stringify(row))
      this.editInformation = [
        [
          {
            name: '风场名称',
            value: row.windparkName
          },
          {
            name: '机组名称',
            value: row.windturbineName
          },
          {
            name: '采集单元类型',
            value: row.dauType
          },
          {
            name: '采集单元名称',
            value: row.dauName
          },
          {
            name: 'IP',
            value: row.ipaddress
          }
        ],
        [
          {
            name: '通道号',
            value: row.channelNumber
          },
          {
            name: '测点名称',
            value: row.measlocName
          },
          {
            name: '灵敏度系数',
            value: row.sensitivity
          },
          {
            name: '最低偏执电压',
            value: row.minvolt
          },
          {
            name: '最高偏执电压',
            value: row.maxvolt
          }
        ]
      ]
    },
    //编辑弹框 -- 保存按钮
    editRole() {
      this.editBox = false
      let list = []
      list.push(this.editItemList)
      update({ dauUniteChannels: list })
        .then(res => {
          if (res.data.code === 200) {
            this.$message({
              type: 'success',
              message: '采集单元更新成功!'
            })
          }
        })
        .then(() => {
          this.data = []
          this.$refs.crud.refreshTable()
          this.$refs.crud.toggleSelection()
          this.onLoad(this.page)
        })
    },

    //新增--保存按钮
    submitRole: debounce(function () {
      let auth = this.$refs.matePass.formValidate()
      let ipAuth = this.$refs.ipUnit.ipListValidate()

      // console.log('IP验证通过?', ipAuth, '测点配置验证通过?', auth)

      if (auth && ipAuth) {
        this.$refs['addRuleForm'].validate(valid => {
          if (valid) {
            const param = this.handleAddParam()
            add({ ...param }).then(res => {
              if (res.data.code == 200) {
                this.addBox = false
                this.$message({ type: 'success', message: '采集单元添加成功！' })
                this.onLoad(this.page)
              }
            })
          }
        })
      }
    }, 700),
    //新增--保存按钮--数据处理1
    handleAddParam() {
      let { unitType, windparkName } = this.addUnitForm

      let param = {
        windParkId: windparkName,
        dauType: unitType,
        dauIPAndWindturbine: [],
        wtphmDauChannel: []
      }

      param.dauIPAndWindturbine = this.ipList.map(item => ({
        windturbineId: item.id,
        windturbineCode: item.code,
        dauIp: item.ip
      }))

      param.wtphmDauChannel = this.unitList

      return param
    },
    //新增 -- 选择的机组数组
    turbineChange(list) {
      this.turbineNameList = list
    },
    //新增 -- 填写的通道匹配测点数组
    measureList(list) {
      let newList = JSON.parse(JSON.stringify(list))
      let newnewList = JSON.parse(JSON.stringify(list))
      let noList = []
      let sList = []
      let maxvoltList = []
      let minvoltList = []

      newList.forEach((item, index) => {
        if (item.sensitivity !== '') {
          sList.push(index)
        }
        if (item.maxvolt !== '') {
          maxvoltList.push(index)
        }
        if (item.minvolt !== '') {
          minvoltList.push(index)
        }
      })

      sList = sList.sort((a, b) => a - b)
      sList.forEach(ele => {
        newList.forEach((item, index) => {
          if (index >= ele) {
            item.sensitivity = newnewList[ele].sensitivity
          }
        })
      })
      maxvoltList = maxvoltList.sort((a, b) => a - b)
      maxvoltList.forEach(ele => {
        newList.forEach((item, index) => {
          if (index >= ele) {
            item.maxvolt = newnewList[ele].maxvolt
          }
        })
      })
      minvoltList = minvoltList.sort((a, b) => a - b)
      minvoltList.forEach(ele => {
        newList.forEach((item, index) => {
          if (index >= ele) {
            item.minvolt = newnewList[ele].minvolt
          }
        })
      })

      newList.forEach(item => {
        if (item.measlocId !== '') {
          noList.push(item)
        }
      })
      this.unitList = noList
    },
    //新增 -- 填写IP
    ipChange(list) {
      this.ipList = list
    }
  }
}
</script>

<style lang="less" scoped>
@import url('../component/commonStyle.less');
// ::v-deep .el-form-item__label {
//   width: 108px !important;
// }

// ::v-deep .el-form-item__content {
//   margin-left: 100px !important;
// }
// ::v-deep .avue-form__group .el-col {
//   width: 20%;
// }
::v-deep .el-dialog .el-dialog__body .el-form .el-form-item {
  margin-right: 30px;
}
::v-deep .el-dialog__body {
  .el-form-item__label {
    text-align: right;
  }
}
</style>
