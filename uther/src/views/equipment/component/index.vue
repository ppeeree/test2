<template>
  <!-- 部件管理 -->
  <div ref="comp">
    <timeLine isActive="2" />
    <basic-container>
      <avue-crud
        :option="compOption"
        :table-loading="loading"
        :data="data"
        ref="crud"
        v-model="form"
        :page.sync="page"
        :before-open="beforeOpen"
        :permission="permissionList"
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
          <el-button
            type="primary"
            v-if="permissionList.addBtn"
            size="small"
            plain
            icon="el-icon-plus"
            @click="saveComp"
            >新 增
          </el-button>
          <el-button
            v-if="permissionList.delBtn"
            type="danger"
            size="small"
            plain
            icon="el-icon-delete"
            @click="handleDelete"
            >删 除
          </el-button>
        </template>
        <template slot-scope="scope" slot="details">
          <span
            class="setting_style"
            @click="handlerDetail(scope.row.partConfigList, scope.row.compName)"
          >
            <i class="el-icon-setting"></i>{{ scope.row.details }}</span
          >
        </template>
      </avue-crud>
    </basic-container>
    <!-- 部件属性 -->
    <el-dialog
      title="部件属性"
      append-to-body
      :visible.sync="setingBox"
      width="500px"
      custom-class="seting_card"
    >
      <div class="seting_title">
        {{ setingParam.title }}
      </div>
      <el-row v-if="setingParam.upItem.length" class="seting_div">
        <el-col
          :span="12"
          v-for="(item, index) in setingParam.upItem"
          :key="index"
          class="seting_text"
          >{{ item.name }}{{ item.value }}</el-col
        >
      </el-row>
      <noData v-else style="margin-top: 20px"></noData>
      <div class="seting_title" style="margin-top: 10px">
        <i class="el-icon-coin" style="margin-right: 5px"></i>参数
      </div>
      <el-row v-if="setingParam.downItem.length" class="seting_div">
        <el-col
          :span="12"
          v-for="(item, index) in setingParam.downItem"
          :key="index"
          class="seting_text"
          >{{ item.name }}：{{ item.value }}</el-col
        >
      </el-row>
      <noData v-else style="margin-top: 20px"></noData>
    </el-dialog>
    <!-- 新增弹框 -->
    <el-dialog
      title="新 增"
      append-to-body
      :visible.sync="addBox"
      width="850px"
      custom-class="add_card"
    >
      <div class="dialod_content">
        <el-form
          :model="addCompForm"
          label-width="80px"
          :rules="addRules"
          ref="addRuleForm"
          style="padding: 0px 0 10px 27px"
        >
          <div class="seting_title"><img src="/img/equipment/addSpot.png" alt="" />创建部件</div>
          <!-- 添加风场和机组 -->
          <turbineSelect :addCompForm="addCompForm" :addRules="addRules"></turbineSelect>
          <el-form-item label="添加部件：" prop="compNames">
            <el-cascader
              v-model="addCompForm.compNames"
              :options="compData"
              :props="compProps"
              @change="compChange"
              filterable
            ></el-cascader>
          </el-form-item>
          <div class="seting_title"><i class="el-icon-coin"></i> 部件属性</div>
          <span v-if="compAttitList.length">
            <el-form-item
              :label="item.metaModelKeyName + '：'"
              v-for="item in compAttitList"
              :key="item.metaModelKeyValue"
            >
              <el-select
                v-model="addCompForm[item.metaModelKeyValue]"
                :placeholder="'请选择' + item.metaModelKeyName"
              >
                <el-option
                  v-for="ele in item.list"
                  :key="ele.dataItemValue"
                  :label="ele.dataItemValue"
                  :value="ele.dataItemValue"
                ></el-option>
              </el-select>
            </el-form-item>
          </span>
          <noData v-else style="height: 100px"></noData>
          <!-- 参数： -->
          <!-- <div v-show="compModelList.length">
            <el-form-item
              :label="compModelList.length == 1 ? '参数： ' : '参数(' + (Number(index) + 1) + ')：'"
              v-for="(item, index) in compModelList"
              :key="index"
            >
              <span class="meta_input">{{ item.metaModelKeyName }}</span>
              &nbsp; &nbsp;&nbsp; &nbsp; & &nbsp; &nbsp;&nbsp; &nbsp;
              <span class="meta_input">{{ item.dataItemValue }}</span>
            </el-form-item>
          </div> -->
        </el-form>
      </div>

      <span slot="footer" class="dialog-footer">
        <el-button type="primary" @click="submitRole"
          ><i class="el-icon-circle-plus-outline"></i>保 存</el-button
        >
        <el-button @click="addBox = false"><i class="el-icon-circle-close"></i>取 消</el-button>
      </span>
    </el-dialog>
  </div>
</template>

<script>
import { mapGetters } from 'vuex'
import debounce from 'lodash/debounce'
import timeLine from './timeLine.vue'
import {
  getList,
  add,
  remove,
  getCompParametersApi,
  getModelApi,
  getCompApi
} from '@/api/equipment/component'
import noData from '@/components/noData/index.vue'
import { entityPartEnum } from '@/util/constant.js'
import { getWindParkList, deleteCompChildren, getSearchCompList } from './windList'
import turbineSelect from './turbineSelect.vue'

export default {
  components: { timeLine, noData, turbineSelect },
  data() {
    return {
      entityPartEnum,
      setingBox: false, //属性弹框展示变量
      addBox: false, //新增弹框展示
      loading: true, //表格loading
      data: [], //表格数据
      query: {}, //表格筛选
      selectionList: [], //表格选中的数据
      selectOption: {}, //下拉筛选数组
      compData: [], //全部部件数组
      // compModelList: [], //根据型号拿到参数数组
      compAttitList: [], //部件属性数组
      setingParam: { title: '', upItem: [], downItem: [] }, //属性弹框变量
      page: {
        total: 0, // 总页数
        currentPage: 1, // 当前页数
        pageSize: 10 // 每页显示多少条
      },
      compOption: {
        height: 'auto',
        calcHeight: 30,
        dialogWidth: 600,
        dialogDrag: true, // 弹窗拖拽
        tip: false,
        border: true,
        dialogClickModal: false,
        menu: false,
        index: true,
        addBtn: false,
        delBtn: false,
        selection: true,
        emptyBtnText: '重置',
        emptyBtnIcon: 'el-icon-refresh',
        column: [
          {
            label: '风场名称',
            prop: 'windParkName',
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
            dicData: [],
            filterable: true,
            props: {
              label: 'entityName',
              value: 'entityId'
            }
          },
          {
            label: '部件名称',
            prop: 'compName',
            search: true,
            type: 'select',
            dicData: [],
            props: {
              label: 'value',
              value: 'value'
            }
          },
          {
            label: '制造商',
            prop: 'maderNames'
          },
          {
            label: '部件型号',
            prop: 'compModel'
          },
          {
            label: '详情',
            prop: 'details'
          }
        ]
      },
      addCompForm: {
        windparkName: '',
        turbineName: '',
        compNames: ''
      },
      addRules: {
        windparkName: [{ required: true, message: '请选择风场名称', trigger: 'change' }],
        turbineName: [{ required: true, message: '请先选择风场', trigger: 'change' }],
        compNames: [{ required: true, message: '请选择部件', trigger: 'change' }]
      },
      compProps: {
        value: 'name',
        label: 'name',
        children: 'childNode',
        expandTrigger: 'hover'
      }
    }
  },
  watch: {
    addBox: {
      handler() {
        //弹框关闭，输入数据清空
        if (!this.addBox) {
          for (let i in this.addCompForm) {
            this.addCompForm[i] = ''
          }
          this.compAttitList = []
          // this.compModelList = []
        }
      },
      immediate: true
    }
  },
  computed: {
    ...mapGetters(['userInfo', 'permission', 'userDeptTree']),
    permissionList() {
      return {
        addBtn: this.vaildData(this.permission.equipment_component_add, false),
        viewBtn: this.vaildData(this.permission.equipment_component_view, false),
        delBtn: this.vaildData(this.permission.equipment_component_delete, false),
        editBtn: this.vaildData(this.permission.equipment_component_edit, false)
      }
    },
    ids() {
      let ids = []
      this.selectionList.forEach(ele => {
        ids.push(ele.compId)
      })
      return ids.join(',')
    }
  },
  mounted() {
    const wind = this.findObject(this.compOption.column, 'windParkName')
    wind.dicData = this.userDeptTree
  },
  methods: {
    windparkChange(val) {
      const turbineId = this.findObject(this.compOption.column, 'windturbineName')
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
          data.forEach(element => {
            element.details = '属性'
            if (element.partConfigList.length) {
              element.maderNames = element.partConfigList.find(j => j.name == '厂商')
                ? element.partConfigList.find(j => j.name == '厂商').value
                : ''
              element.compModel = element.partConfigList.find(j => j.name == '型号')
                ? element.partConfigList.find(j => j.name == '型号').value
                : ''
            }
          })
          this.data = data

          this.selectOption = this.handleOptionList(res.data.data.searchOptList)

          /*  const wind = this.findObject(this.compOption.column, 'windParkName')
          wind.dicData = this.selectOption.windParkList

          const turbine = this.findObject(this.compOption.column, 'windturbineName')
          turbine.dicData = this.selectOption.windturbineList */
          const comp = this.findObject(this.compOption.column, 'compName')
          comp.dicData = this.selectOption.compList
          //重构搜索结构
          getCompApi().then(res => {
            let result = res.data.data.map(item => {
              let newChildren = []
              if (item.children && item.children.length) {
                newChildren = item.children.map(i => {
                  return {
                    ...i,
                    children: []
                  }
                })
              }
              return {
                ...item,
                children: newChildren
              }
            })
            this.compData = result
            deleteCompChildren(this.compData)
            /*  let searchArr = getSearchCompList(this.compData, this.selectOption.compList)
            const comp = this.findObject(this.compOption.column, 'compName')
            comp.dicData = searchArr */
          })
        }
      })
    },
    //下拉数据重组
    handleOptionList(param) {
      let { compCompany, compModel, ...other } = param
      let list = []
      let list1 = []
      Object.keys(compCompany).forEach(function (key) {
        compCompany[key].forEach(j => {
          if (!list.find(i => i.value == j.value)) {
            list.push({ name: j.value, value: j.value })
          }
        })
      })
      Object.keys(compModel).forEach(function (key) {
        compModel[key].forEach(j => {
          if (!list1.find(i => i.value == j.value)) {
            list1.push({ name: j.value, value: j.value })
          }
        })
      })

      let newParam = {
        ...other
        // compModel: list1,
        // facturer: list
      }
      return newParam
    },
    //表格--搜索
    searchChange(params, done) {
      let newparam = {
        windParkId: params.windParkName,
        windturbineId: params.windturbineName,
        compName: params.compName ? params.compName[params.compName.length - 1] : undefined
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
    //表格--新增按钮
    saveComp() {
      this.addBox = true
    },
    //表格--删除
    handleDelete() {
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
          return remove({ ids: this.ids })
        })
        .then(() => {
          this.data = []
          this.$refs.crud.refreshTable()
          this.$refs.crud.toggleSelection()
          this.onLoad(this.page)
          this.$message({
            type: 'success',
            message: '部件删除成功!'
          })
        })
    },
    //表格--点击属性
    handlerDetail(param, name) {
      this.setingBox = true
      if (param.length) {
        this.setingParam = {
          title: name,
          upItem: [],
          downItem: []
        }
        param.forEach(item => {
          if (item.name == '厂商') {
            this.setingParam.upItem.push({ name: '制造商：', value: item.value })
          } else if (item.name == '型号') {
            this.setingParam.upItem.push({ name: '部件型号：', value: item.value })
          } else {
            this.setingParam.downItem.push(item)
          }
        })
      } else {
        this.setingParam = { title: name, upItem: [], downItem: [] }
      }
    },

    //新增卡片--部件选择
    compChange(value) {
      getModelApi({ MetaModelName: value[value.length - 1] }).then(res => {
        if (res.data.code == 200) {
          const data = res.data.data
          if (data.length) {
            data.forEach(item => {
              let obj = this.compAttitList.find(j => j.metaModelKeyValue == item.metaModelKeyValue)
              if (!obj) {
                this.$set(this.addCompForm, item.metaModelKeyValue, '')

                this.compAttitList.push({
                  metaModelKeyName: item.metaModelKeyName,
                  metaModelKeyValue: item.metaModelKeyValue,
                  list: [item]
                })
              } else {
                obj.list.push(item)
              }
            })
          }
        }
      })
    },
    //新增卡片--部件型号选择
    // compIndexChange(value) {
    //   getCompParametersApi({ metaDataName: value }).then(res => {
    //     if (res.data.code == 200) {
    //       this.compModelList = res.data.data
    //     }
    //   })
    // },
    //新增卡片--保存按钮
    submitRole: debounce(function () {
      this.$refs['addRuleForm'].validate(valid => {
        if (valid) {
          const param = this.handleAddParam()
          this.addBox = false
          add({ ...param }).then(res => {
            if (res.data.code == 200) {
              this.$message({ type: 'success', message: '部件添加成功！' })
              this.onLoad(this.page)
            }
          })
        }
      })
    }, 700),
    //处理add接口传参
    handleAddParam() {
      let allWindList = getWindParkList()

      let { compNames, windparkName, turbineName, ...other } = this.addCompForm

      let normLists = []
      if (this.compAttitList.length) {
        this.compAttitList.forEach(item => {
          let { metaModelKeyName, metaModelKeyValue } = item
          normLists.push({
            spaceBasicCode: metaModelKeyValue,
            spaceBasicName: metaModelKeyName,
            spaceBasicValue: other[metaModelKeyValue]
          })
        })
      } else {
        normLists = []
      }

      let entityObj = this.handleAddCompList(compNames)
      let windObj = allWindList.find(j => j.id == windparkName)

      let param = {
        entityTypeCode: entityObj.codeList,
        entityType: entityObj.typeList,
        entityName: compNames,
        windParkId: windObj.id,
        windParkCode: windObj.code,
        windturbineIds: turbineName,
        normLists: normLists
      }

      return param
    },
    handleAddCompList(arr) {
      let obj1, obj2, obj3
      obj1 = this.compData.find(j => j.name == arr[0])
      if (arr.length > 1) {
        obj2 = obj1.children.find(j => j.name == arr[1])
      }
      if (arr.length > 2) {
        obj3 = obj2.children.find(j => j.name == arr[2])
      }

      let list = [obj1, obj2, obj3]
      list = list.filter(x => !!x == true || x == 0)

      let codeList = list.map(j => j.code)
      let typeList = list.map(j => j.modelCode)
      return { codeList, typeList }
    },

    //点击刷新按钮触发
    currentChange(currentPage) {
      this.page.currentPage = currentPage
    },
    sizeChange(pageSize) {
      this.page.pageSize = pageSize
    },
    refreshChange() {
      this.onLoad(this.page, this.query)
    }
  }
}
</script>

<style lang="less" scoped>
@import url('./commonStyle.less');

::v-deep .avue-form__group .el-col {
  width: 25%;
}

//属性字体样式
.setting_style {
  color: #409eff;
  text-decoration: underline;
  cursor: pointer;
  .el-icon-setting {
    margin-right: 5px;
  }
  &:hover {
    color: #afcaff;
  }
}

.seting_div {
  position: relative;
  left: 8%;
  width: 85%;
  .seting_text {
    font-size: 14px;
    height: 20px;
    display: inline-block;
    margin-top: 10px;
    line-height: 20px;
  }
}
.meta_input {
  display: inline-block;
  width: 240px;
  height: 36px;
  line-height: 36px;
  background-color: rgba(71, 86, 128, 0.5);
  border-radius: 4px;
  padding-left: 14px;
}
</style>
