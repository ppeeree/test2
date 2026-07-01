<template>
  <!-- 测点管理 -->
  <div>
    <timeLine isActive="3" />
    <basic-container class="spot_table">
      <avue-crud
        :option="spotOption"
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
          <el-button type="primary " size="small" plain icon="el-icon-plus" @click="saveComp"
            >新 增
          </el-button>
          <el-button type="danger" size="small" plain icon="el-icon-delete" @click="handleDelete"
            >删 除
          </el-button>
        </template>
        <template slot="menu" slot-scope="{ type, size, row }">
          <el-button icon="el-icon-view" :size="size" :type="type" @click.native="editItem(row)"
            >查看</el-button
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
        <el-form
          :model="addSpotForm"
          label-width="80px"
          :rules="addRules"
          ref="addRuleForm"
          style="padding: 0px 0 10px 27px"
        >
          <div class="seting_title"><img src="/img/equipment/addSpot.png" alt="" /> 创建测点</div>
          <!-- 添加风场和机组 -->
          <turbineSelect
            :addCompForm="addSpotForm"
            :addRules="addRules"
            @selectCompList="selectCompList"
            :allCompList="allallCompData"
          ></turbineSelect>
          <el-form-item label="添加测点：" prop="spotName">
            <el-cascader
              v-model="addSpotForm.spotName"
              :options="CompData"
              :props="compProps"
              @change="handleChange"
              placeholder="请选择测点"
              ref="refSubCat"
              clearable=""
              popper-class="last_check"
            ></el-cascader>
          </el-form-item>
          <el-form-item label="物理量：" style="width: 100%">
            <el-select v-model="addSpotForm.physical" clearable placeholder="请选择物理量">
              <el-option
                v-for="item in unitList"
                :key="item.code"
                :label="item.name"
                :value="item.name"
              >
              </el-option>
            </el-select>
          </el-form-item>
        </el-form>
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
        <span class="no_edit"
          >风场名称：<span class="edit_input">{{ editItemList.windparkName }}</span></span
        >
        <span class="no_edit"
          >机组名称：<span class="edit_input">{{ editItemList.windturbineName }}</span></span
        >
        <span class="no_edit"
          >部件名称：<span class="edit_input">{{ editItemList.allcompName }}</span></span
        >
        <span class="no_edit"
          >测点名称：<span class="edit_input">{{ editItemList.measlocName }}</span
          ><!-- <el-input v-model="editItemList.measlocName"></el-input
        > --></span
        >
      </div>
      <span slot="footer" class="dialog-footer">
        <el-button type="primary" size="small" @click="editRole"
          ><i class="el-icon-circle-plus-outline"></i>保 存</el-button
        >
        <el-button @click="editBox = false" size="small"
          ><i class="el-icon-circle-close"></i>取 消</el-button
        >
      </span>
    </el-dialog>
  </div>
</template>

<script>
import debounce from 'lodash/debounce'
import { mapGetters } from 'vuex'
import timeLine from '../../equipment/component/timeLine.vue'
import turbineSelect from '../../equipment/component/turbineSelect.vue'
import { getUnits } from '@/api/alarmFault/alarmModel'
import { getList, remove, add, update } from '@/api/equipment/spot'
import { getCompApi } from '@/api/equipment/component'
import {
  deleteCompChildren,
  getSearchCompList,
  handlerCompList
} from '../../equipment/component/windList'

export default {
  components: {
    timeLine,
    turbineSelect
  },
  data() {
    return {
      editBox: false,
      addBox: false,
      loading: true,
      query: {},
      selectOption: {}, //下拉筛选数组
      data: [],
      selectionList: [], //表格选中的数据
      allallCompData: [],
      unitList: [], //物理量数组
      CompData: [], //根据机组筛选部件及部件下测点
      editItemList: [], //点击编辑的数据
      page: {
        total: 0, // 总页数
        currentPage: 1, // 当前页数
        pageSize: 10 // 每页显示多少条
      },
      spotOption: {
        // menu: false, // 不显示操作列
        height: 'auto',
        calcHeight: 30,
        dialogWidth: 600,
        dialogDrag: true, // 弹窗拖拽
        tip: false,
        border: true,
        dialogClickModal: false,
        addBtn: false,
        editBtn: false,
        //  viewBtn: true,
        index: true,
        delBtn: false,
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
            },
            dicData: [],
            filterable: true
          },
          {
            label: '部件名称',
            prop: 'allcompName',
            search: true,
            type: 'cascader',
            props: {
              label: 'name',
              value: 'name'
            },
            dicData: [],
            filterable: true
          },
          {
            label: '测点名称',
            prop: 'measlocName',
            search: true,
            type: 'select',
            props: {
              label: 'name',
              value: 'name'
            },
            filterable: true,
            dicData: []
          }
        ]
      },
      addSpotForm: {
        windparkName: '',
        turbineName: '',
        spotName: '',
        physical: ''
      },
      addRules: {
        windparkName: [{ required: true, message: '请选择风场名称', trigger: 'change' }],
        turbineName: [{ required: true, message: '请选择机组名称', trigger: 'change' }],
        spotName: [{ required: true, message: '请选择测点', trigger: 'change' }]
      },
      compProps: {
        value: 'name',
        label: 'name',
        expandTrigger: 'hover',
        multiple: true
      },

      AllMeaslocList: [] // 全部部件及测点列表
    }
  },
  watch: {
    addBox: {
      handler() {
        if (!this.addBox) {
          this.addSpotForm = {
            windparkName: '',
            turbineName: '',
            spotName: '',
            physical: ''
          }
          // this.turbineNameList = []
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
        ids.push(ele.measlocId)
      })
      return ids.join(',')
    }
  },
  mounted() {
    const wind = this.findObject(this.spotOption.column, 'windparkName')
    wind.dicData = this.userDeptTree
  },
  beforeCreate() {
    getCompApi().then(res => {
      this.AllMeaslocList = res.data.data
    })
  },
  methods: {
    windparkChange(val) {
      const turbineId = this.findObject(this.spotOption.column, 'windturbineName')
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
          data.forEach(item => {
            let { parentCompName, compName } = item
            item.allcompName = compName // parentCompName + '_' + compName
          })
          this.data = data

          const option = res.data.data.searchOptList
          /* const wind = this.findObject(this.spotOption.column, 'windparkName')
          wind.dicData = option.windParkList */

          /*     const turbine = this.findObject(this.spotOption.column, 'windturbineName')
          turbine.dicData = option.windturbineList */

          const comp = this.findObject(this.spotOption.column, 'allcompName')
          comp.dicData = option.compList

          const spot = this.findObject(this.spotOption.column, 'measlocName')
          spot.dicData = option.measlocList

          getCompApi().then(res => {
            this.allallCompData = handlerCompList(JSON.parse(JSON.stringify(res.data.data)))
            this.allCompData = res.data.data
            deleteCompChildren(this.allCompData)
            /* let searchArr = getSearchCompList(this.allCompData, option.compList)
            const comp = this.findObject(this.spotOption.column, 'allcompName')
            comp.dicData = searchArr */
          })
        }
      })
    },
    //表格--搜索
    searchChange(params, done) {
      let newparam = {
        windParkId: params.windparkName,
        windturbineId: params.windturbineName,
        compName: params.allcompName
          ? params.allcompName[params.allcompName.length - 1]
          : undefined,
        measlocName: params.measlocName
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
    //表格--更新
    editItem(row) {
      this.editBox = true
      this.editItemList = JSON.parse(JSON.stringify(row))
    },
    //编辑弹框 -- 保存按钮
    editRole() {
      this.editBox = false
      let param = this.handlerEditList(this.editItemList)

      update({ ...param }).then(
        () => {
          this.$message({
            type: 'success',
            message: '操作成功!'
          })
          this.onLoad(this.page)
        },
        error => {
          window.console.log(error)
        }
      )
    },
    //编辑--保存数据修改
    handlerEditList(obj) {
      let {
        windparkId,
        windturbineId,
        measlocId,
        parentCompName,
        parentCompCode,
        compName,
        sectionName,
        orientation
      } = obj

      let param = {
        windParkId: windparkId,
        windturbineId: windturbineId,
        measLocVibrationList: [],
        oldMeaslocVibrationIds: [measlocId]
      }

      let obj1 = this.AllMeaslocList.find(j => j.name == parentCompName)
      let obj2 = obj1.children.find(j => j.name == compName)
      let obj3, obj4

      if (obj2.children[0].key == 'section') {
        if (sectionName) {
          obj3 = obj2.children.find(j => j.code == sectionName)
        }
        if (orientation) {
          obj4 = obj3.children.find(j => j.code == orientation)
        }
      } else {
        obj2.children.forEach(item => {
          item.children.forEach(item1 => {
            obj3 = item1.children.find(j => j.code == sectionName)
            obj4 = obj3.children.find(j => j.code == orientation)
          })
        })
      }

      // console.log('中间参数', obj1, obj2, obj3, obj4)

      let measlocParam = {
        compCodeRelationship: [parentCompCode, obj2.code],
        compNameRelationship: [parentCompName, compName],
        orientationCode: orientation ? obj4.code : '',
        orientationName: orientation ? obj4.name : '',
        sectionCode: sectionName ? obj3.code : '',
        sectionName: sectionName ? obj3.name : '',
        measlocVibrationName: obj.measlocName
      }

      param.measLocVibrationList.push(measlocParam)
      // console.log('更新接口参数', param)
      return param
    },

    //获取物理量
    getUnit() {
      getUnits().then(res => {
        if (res.data.code === 200) {
          this.unitList = res.data.data
        }
      })
    },

    //表格--选中
    selectionChange(list) {
      this.selectionList = list
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
            message: '测点删除成功!'
          })
        })
    },
    //表格--新增按钮
    saveComp() {
      this.addBox = true
      this.getUnit()
    },
    /*handlerCompList(list) {
      list.forEach(item => {
        if (item.children && item.children.length) {
          this.handlerCompList(item.children)
        } else {
          item.children = this.handlerMeasureList(item.name, item.measureList)
        }
      })
      return list
    },
    //表格--新增按钮--数组处理
    handlerMeasureList(name, list) {
      let Slist = []
      let Olist = []
      list &&
        list.forEach(ele => {
          if (ele.key == 'section') {
            Slist.push({ code: ele.code, key: ele.key, name: ele.name, children: [] })
          } else {
            Olist.push({ code: ele.code, key: ele.key, name: ele.name })
          }
        })

      if (Olist.length) {
        Slist.forEach(item => {
          item.children.push(...Olist)
        })
      }

      return Slist
    },*/

    //新增--保存按钮
    submitRole: debounce(function () {
      this.$refs['addRuleForm'].validate(valid => {
        if (valid) {
          const param = this.handleAddParam()
          console.log('新增测点处理结果', param)
          add({ ...param }).then(res => {
            if (res.data.code == 200) {
              this.addBox = false
              this.$message({ type: 'success', message: '测点添加成功！' })
              this.onLoad(this.page)
            }
          })
        }
      })
    }, 700),
    //新建--保存按钮--数据处理1
    handleAddParam() {
      let { spotName, turbineName, windparkName, physical } = this.addSpotForm

      let param = {
        windParkId: windparkName,
        windturbineIds: turbineName,
        measLocVibrationList: []
      }

      spotName.forEach(item => {
        let obj = this.handleMeasLocVibrationList(item)
        obj.standardUnitCode = physical
          ? this.unitList.find(j => j.name == physical).code
          : undefined
        obj.standardUnitName = physical ? physical : undefined
        param.measLocVibrationList.push(obj)
      })

      return param
    },
    //新建--保存按钮--数据处理2
    handleMeasLocVibrationList(val) {
      let obj = {
        compCodeRelationship: [],
        compNameRelationship: [],
        sectionCode: '',
        sectionName: '',
        orientationCode: '',
        orientationName: ''
      }
      console.log(val, this.CompData)
      let obj1 = this.CompData.find(j => j.name == val[0]) // 实体部件code
      let obj2 = obj1.children.find(j => j.name == val[1])
      let obj3 = obj2.children.find(j => j.name == val[2])
      let obj4 = {}
      if (val[3]) {
        obj4 = obj3.children.find(j => j.name == val[3])
      }

      let list = [obj1, obj2, obj3, obj4]
      console.log(list)
      list.forEach(ele => {
        if (ele.key == 'comp') {
          obj.compNameRelationship.push(ele.name)
          obj.compCodeRelationship.push(ele.code)
        } else if (ele.key == 'section') {
          obj.sectionCode = ele.code
          obj.sectionName = ele.name
        } else if (ele.key == 'orientation') {
          obj.orientationCode = ele.code
          obj.orientationName = ele.name
        }
      })

      return obj
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
    },
    selectCompList(arr) {
      /* const result = this.AllMeaslocList.flatMap(parent =>
        parent.children
          .filter(child => arr.some(target => target.value === child.value))
          .map(child => ({ parent, child }))
      ) */
      /* this.AllMeaslocList.forEach(pagecompItem => {
        arr.forEach(compObj => {
          const obj = pagecompItem.children.find(o => o.code == compObj.code)
          if(obj){

          }
        })
      }) */
      const result = []
      this.AllMeaslocList.forEach(parent => {
        parent.children.forEach(child => {
          const match = arr.find(target => target.code === child.code)
          if (match) {
            result.push(child)
          }
        })
      })

      console.log('获取的部件列表', arr, this.AllMeaslocList, result)

      this.CompData = result
    }
  }
}
</script>

<style lang="scss">
/* @import url('../../equipment/component/commonStyle.less');
.el-pagination .el-select .el-input .el-input__inner {
  border: 1px white solid;
  background: transparent;
  color: white;
}
.el-pagination .el-input__inner,
.el-pagination .el-input__inner:hover {
  border: 1px white solid;
  background: transparent;
} */

.spot_table {
  .el-form-item__label {
    width: 100px !important;
  }
  .el-form-item__content {
    margin-left: 100px !important;
  }
  .el-card + .el-card {
    margin-top: -16px;
  }
  .avue-crud__left {
    padding-left: 20px;

    .el-button {
      margin-right: 20px !important;
    }

    .el-button--info {
      width: 80px;
    }
  }
  .avue-form__group .el-col {
    width: 20%;
  }
  .avue-form__menu--center .el-button {
    position: relative;
    top: -5px;
    left: 5%;
  }
}

.el-button--primary {
  background: #0861aa !important;
  border-color: #0861aa !important;
  color: white !important;

  &:hover {
    background: #1c98ff !important;
    border-color: #1c98ff !important;
  }
}
.add_card {
  padding-bottom: 15px;

  .el-dialog__header {
    .el-dialog__title {
      font-size: 16px;
    }
  }

  .el-dialog__body {
    padding: 10px 20px;
    color: white;
    max-height: 620px;
    overflow-y: auto;
    //overflow-x: hidden;//解决下拉框显示不全问题
    .el-cascader {
      width: 268%;
    }
    .el-form {
      width: 93%;
      padding: 21px 0 10px 27px;

      .el-form-item {
        display: inline-block;
        margin-right: 40px;
      }

      .el-form-item__content {
        line-height: 35px;
      }

      .el-input__inner {
        height: 35px;
      }
    }
  }

  .el-dialog__footer {
    height: 40px;
    padding: 0px;

    .dialog-footer {
      position: relative;
      bottom: 0px;
      right: 40%;

      .el-button {
        width: 110px;
        height: 36px;
        padding: 0px;
        line-height: 36px;

        i {
          margin-right: 5px;
        }
      }
    }
  }
}

.last_check {
  li[aria-haspopup='true'] {
    .el-checkbox {
      display: none;
    }
  }
}

.seting_title {
  width: 100%;
  left: 0%;
  margin-bottom: 20px;
  font-size: 14px;
  height: 30px;
  line-height: 30px;
  border-bottom: 1px solid rgba(72, 137, 217, 0.5);
  position: relative;
  padding-left: 10px;

  img {
    padding-right: 5px;
  }
}
.dialod_content {
  color: #fff;
  .no_edit {
    line-height: 36px;
    margin-top: 20px;
    width: 48%;
    display: inline-block;

    .edit_title {
      width: 100px;
      display: inline-block;
      text-align: right;
    }

    .edit_input {
      height: 36px;
      display: inline-block;
      width: 260px;
      border-radius: 4px;
      background-color: rgba(71, 86, 128, 0.3);
      padding-left: 15px;
    }

    .el-input {
      height: 36px;
      width: 260px;

      .el-input__inner {
        height: 36px;
      }
    }
  }
}
</style>
