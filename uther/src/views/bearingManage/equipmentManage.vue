<template>
  <basic-container>
    <avue-crud
      :option="option"
      :table-loading="loading"
      :data="data"
      ref="crud"
      v-model="form"
      :search="query"
      :permission="permissionList"
      :before-close="beforeClose"
      @row-del="rowDel"
      @row-update="rowUpdate"
      @row-save="rowSave"
      @search-change="searchChange"
      @search-reset="searchReset"
      @current-change="currentChange"
      @size-change="sizeChange"
      @on-load="onLoad"
    >
      <template slot-scope="scope" slot="speed">
        <el-input
          type="number"
          @change="handleChange($event, scope.row)"
          v-model="scope.row.speed"
        ></el-input>
      </template>
    </avue-crud>
  </basic-container>
</template>

<script>
import { add, getList, remove, update, getBearingVendorApi } from '@/api/equipment/bearing'
import { mapGetters } from 'vuex'
import { accMul } from '@/util/fengji'
import website from '@/config/website'

export default {
  data() {
    return {
      compList: [],
      form: {},
      query: {},
      loading: true,
      page: {
        pageSize: 10,
        currentPage: 1,
        total: 0
      },
      option: {
        tip: false,
        simplePage: true,
        searchShow: true,
        searchMenuSpan: 6,
        tree: true,
        border: true,
        index: true,
        viewBtn: true,
        dialogWidth: 900,
        labelWidth: 150,
        dialogClickModal: false,
        emptyBtnText: '重置',
        emptyBtnIcon: 'el-icon-refresh',
        column: [
          {
            label: '轴承厂家',
            prop: 'factory',
            type: 'select',
            search: true,
            dicData: [],
            allowCreate: true,
            filterable: true,
            // cascader: ['model'],
            span: 12,
            props: {
              label: 'factory',
              value: 'factory'
            },
            change: val => {
              this.changeModelDicData(val.value)
            },
            rules: [
              {
                required: true,
                message: '请选择或输入轴承厂家',
                trigger: 'change'
              }
            ]
          },
          {
            label: '轴承型号',
            prop: 'model',
            type: 'select',
            search: true,
            span: 12,
            allowCreate: true,
            filterable: true,
            dicData: [],
            props: {
              label: 'label',
              value: 'value'
            },
            rules: [
              {
                required: true,
                message: '请选择或输入轴承型号',
                trigger: 'change'
              }
            ]
          },
          {
            label: '节圆直径(mm)',
            prop: 'pitchDiameter',
            type: 'number',
            span: 12,
            search: false,
            rules: [
              {
                required: true,
                message: '请输入节圆直径',
                trigger: 'blur'
              }
            ]
          },
          {
            label: '滚动体直径(mm)',
            prop: 'rollerDiameter',
            type: 'number',
            span: 12,
            search: false,
            rules: [
              {
                required: true,
                message: '请输入滚动体直径',
                trigger: 'blur'
              }
            ]
          },
          {
            label: '滚动体个数',
            prop: 'rollingElementSize',
            type: 'number',
            span: 12,
            search: false,
            rules: [
              {
                required: true,
                message: '请输入滚动体个数',
                trigger: 'blur'
              }
            ]
          },
          {
            label: '接触角(°)',
            prop: 'contactAngle',
            type: 'number',
            span: 12,
            search: false,
            rules: [
              {
                required: true,
                message: '请输入接触角',
                trigger: 'blur'
              }
            ]
          },
          {
            label: '外圈故障频率(BPOF:Hz)',
            prop: 'bpof',
            addDisplay: false,
            editDisplay: false
          },
          {
            label: '内圈故障频率(BPIF:Hz)',
            prop: 'bpif',
            addDisplay: false,
            editDisplay: false
          },
          {
            label: '滚动体故障频率(BSF:Hz)',
            prop: 'bsf',
            addDisplay: false,
            editDisplay: false
          },
          {
            label: '保持架故障频率(FTF:Hz)',
            prop: 'ftf',
            addDisplay: false,
            editDisplay: false
          },
          {
            label: '转速(RPM)',
            prop: 'speed',
            cell: true,
            type: 'number',
            addDisplay: false,
            editDisplay: false
          }
        ]
      },
      data: []
    }
  },
  computed: {
    ...mapGetters(['userInfo', 'permission']),
    permissionList() {
      return {
        addBtn: this.vaildData(this.permission.equipmentManage_add, false),
        viewBtn: this.vaildData(this.permission.equipmentManage_view, false),
        delBtn: this.vaildData(this.permission.equipmentManage_delete, false),
        editBtn: this.vaildData(this.permission.equipmentManage_edit, false)
      }
    }
  },
  created() {
    this.initData()
  },
  methods: {
    initData() {
      getBearingVendorApi().then(res => {
        this.compList = res.data.data
        const column = this.findObject(this.option.column, 'factory')
        column.dicData = res.data.data
        // this.$refs.crud.resetFields()
      })
    },
    changeModelDicData(value) {
      let unitObj = this.compList.find(item => item.factory == value)
      // this.modelList = unitObj && unitObj.modelList?.length ? unitObj.modelList : []
      const column = this.findObject(this.option.column, 'model')
      column.dicData =
        unitObj && unitObj.modelList?.length
          ? Array.from(unitObj.modelList, i => ({ label: i, value: i }))
          : []
      if (this.form.model && column.dicData.find(i => i.value == this.form.model)) {
        return
      } else {
        this.form.model = ''
      }
    },
    handleChange(value, row) {
      let coefficient = value / 60 // 系数
      let { bpif1, bpof1, bsf1, ftf1, factory, model } = row
      this.data = this.data.map(item => {
        if (item.factory == factory && item.model == model) {
          return {
            ...item,
            bpif: accMul(bpif1, coefficient),
            bpof: accMul(bpof1, coefficient),
            bsf: accMul(bsf1, coefficient),
            ftf: accMul(ftf1, coefficient)
          }
        } else {
          return item
        }
      })
    },
    rowSave(row, done, loading) {
      add(row).then(
        () => {
          this.onLoad(this.page)
          this.$message({
            type: 'success',
            message: '操作成功!'
          })
          done()
        },
        error => {
          window.console.log(error)
          loading()
        }
      )
    },
    rowUpdate(row, index, done, loading) {
      update(row).then(
        () => {
          this.onLoad(this.page)
          this.$message({
            type: 'success',
            message: '操作成功!'
          })
          done()
        },
        error => {
          window.console.log(error)
          loading()
        }
      )
    },
    rowDel(row) {
      this.$confirm('确定将选择数据删除?', {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning'
      })
        .then(() => {
          const { factory, model } = row
          return remove({ factory, model })
        })
        .then(() => {
          this.onLoad(this.page)
          this.initData(this.form.id)
          this.$message({
            type: 'success',
            message: '操作成功!'
          })
        })
    },

    searchReset() {
      this.query = {}
      this.onLoad(this.page)
    },
    searchChange(params, done) {
      this.query = params
      this.page.currentPage = 1
      this.onLoad(this.page, params)
      done()
    },
    beforeClose(done, type) {
      if (['add', 'edit'].includes(type)) {
        this.initData(this.form.id)
      }
      done()
    },
    currentChange(currentPage) {
      this.page.currentPage = currentPage
    },
    sizeChange(pageSize) {
      this.page.pageSize = pageSize
    },

    onLoad(page, params = {}) {
      this.loading = true
      getList({
        offset: page.currentPage,
        pageSize: page.pageSize,
        ...Object.assign(params, this.query)
      }).then(res => {
        this.data = Array.from(res.data.data.data, item => ({
          ...item,
          bpif1: item.bpif,
          bpof1: item.bpof,
          bsf1: item.bsf,
          ftf1: item.ftf
        }))
        this.loading = false
        this.page.total = res.data.data.totalCount
        /*   this.$nextTick(() => {
          this.$refs.crud.dicInit('cascader')
        }) */
      })
    }
  }
}
</script>
