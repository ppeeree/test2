<template>
  <basic-container>
    <avue-crud
      :option="option"
      :table-loading="loading"
      :data="data"
      ref="crud"
      v-model="form"
      :page.sync="page"
      :permission="permissionList"
      :before-open="beforeOpen"
      :before-close="beforeClose"
      @row-del="rowDel"
      @row-update="rowUpdate"
      @row-save="rowSave"
      @search-change="searchChange"
      @search-reset="searchReset"
      @current-change="currentChange"
      @size-change="sizeChange"
      @refresh-change="refreshChange"
      @on-load="onLoad"
    >
      <template slot-scope="scope" slot="imgBaseList">
        <el-image
          v-for="item in scope.row.imgBaseList"
          :key="item.id"
          style="width: 50px; height: 50px; margin: 0 10px; cursor: pointer"
          :src="item.scaleImgBase64"
          :preview-src-list="['/imgFile/' + item.imgPath]"
        >
        </el-image>
      </template>
      <template slot-scope="scope" slot="imgBaseListForm">
        <el-upload
          action="#"
          ref="upload"
          list-type="picture-card"
          :file-list="fileList"
          :auto-upload="false"
          :on-change="handleChange"
        >
          <i slot="default" class="el-icon-plus"></i>
          <div slot="file" slot-scope="{ file }">
            <img class="el-upload-list__item-thumbnail" :src="file.url" alt="" />
            <span class="el-upload-list__item-actions">
              <span class="el-upload-list__item-preview" @click="handlePictureCardPreview(file)">
                <i class="el-icon-zoom-in"></i>
              </span>
              <span
                v-if="!disabled"
                class="el-upload-list__item-delete"
                @click="handleRemove(file)"
              >
                <i class="el-icon-delete"></i>
              </span>
            </span>
          </div>
        </el-upload>
      </template>
    </avue-crud>
    <el-dialog append-to-body :visible.sync="dialogVisible" destroy-on-close>
      <img width="100%" :src="dialogImageUrl" alt="" />
    </el-dialog>
  </basic-container>
</template>

<script>
import {
  getBearingFaultLevelApi,
  /*getBearingFaultTypeApi, */
  getFaultCompApi,
  addFault,
  getFaultList,
  removeFault,
  updateFault
} from '@/api/equipment/bearing'
import { mapGetters } from 'vuex'
import func from '@/util/func'
import website from '@/config/website'

export default {
  data() {
    return {
      form: {},
      fileList: [],
      compList: [],
      dialogVisible: false,
      disabled: false,
      dialogImageUrl: '',
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
        dialogClickModal: false,
        emptyBtnText: '重置',
        emptyBtnIcon: 'el-icon-refresh',
        column: [
          {
            label: '部件',
            prop: 'entityTypeCode',
            type: 'select',
            search: true,
            width: '150',
            // dicUrl: '/api/wtphm-service/bearing/fault/deviceComponents?isExistenceComponents=true',
            cascader: ['faultNameType'],
            span: 24,
            dicData: [],
            props: {
              label: 'name',
              value: 'code'
            },
            rules: [
              {
                required: true,
                message: '请选择故障部件',
                trigger: 'blur'
              }
            ]
          },
          {
            label: '故障类型',
            prop: 'faultNameType',
            type: 'select',
            span: 12,
            width: 200,
            dicUrl: `/api/wtphm-service/bearing/faultKnowledge/faultTypeList?entityTypeCode={{key}}`,
            allowCreate: true,
            filterable: true,
            search: true,
            // overHidden: true,
            dicFormatter: res => {
              return res.data.map(item => {
                return {
                  label: item,
                  value: item
                }
              })
            },
            rules: [
              {
                required: true,
                message: '请选择或输入故障类型',
                trigger: 'blur'
              }
            ]
          },
          {
            label: '故障程度',
            prop: 'faultLevelCode',
            search: true,
            width: 100,
            // dicUrl: '/api/wtphm-service/bearing/fault/severityDefList',
            type: 'select',
            dicData: [],
            span: 12,
            props: {
              label: 'name',
              value: 'code'
            },
            rules: [
              {
                required: true,
                message: '请选择故障程度',
                trigger: 'change'
              }
            ]
          },
          {
            label: '维护建议',
            prop: 'maintenanceProposals',
            type: 'textarea',
            span: 24,
            // overHidden: true,
            rules: [
              {
                required: true,
                message: '请输入维护建议',
                trigger: 'blur'
              }
            ]
          },
          {
            label: '参考照片',
            slot: true,
            formslot: true,
            prop: 'imgBaseList',
            span: 24,
            row: true,
            width: 300,
            rules: [
              {
                required: false,
                message: '请上传图片',
                trigger: 'change'
              }
            ]
            /*   propsHttp: {
              res: 'data',
              url: 'link'
              // name: 'originalName'
            } */
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
        addBtn: this.vaildData(this.permission.faultManage_add, false),
        viewBtn: this.vaildData(this.permission.faultManage_view, false),
        delBtn: this.vaildData(this.permission.faultManage_delete, false),
        editBtn: this.vaildData(this.permission.faultManage_edit, false)
      }
    }
  },
  created() {
    // 新增弹窗的部件列，与搜索的部件列 下拉内容不同
    this.initFaultCompList()
    getFaultCompApi().then(res => {
      this.compList = res.data.data
    })
    getBearingFaultLevelApi().then(res => {
      const column = this.findObject(this.option.column, 'faultLevelCode')
      column.dicData = res.data.data
    })
  },
  mounted() {},
  methods: {
    initData() {
      const column = this.findObject(this.option.column, 'entityTypeCode')
      column.dicData = this.compList
    },
    initFaultCompList() {
      getFaultCompApi({ isExistenceComponents: true }).then(res => {
        const column = this.findObject(this.option.column, 'entityTypeCode')
        column.dicData = res.data.data
      })
    },
    handleChange(file, fileList) {
      this.fileList = fileList
    },
    rowSave(row, done, loading) {
      let formData = func.toFormData(row)
      this.fileList.length
        ? this.fileList.forEach(file => {
            formData.append('imgFile', file.raw)
          })
        : null
      addFault(formData).then(
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
      let formData = func.toFormData(row)
      let imgList = []
      this.fileList.length
        ? this.fileList.forEach(file => {
            if (file.raw) {
              formData.append('imgFile', file.raw)
            } else {
              let id = file.bigImg.replace('/imgFile/', '')
              imgList.push(id)
            }
          })
        : null
      formData.append('retentionImgFile', imgList)
      updateFault(formData).then(
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
          return removeFault(row.id)
        })
        .then(() => {
          this.onLoad(this.page)
          this.fileList = []
          this.initFaultCompList()
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
    beforeOpen(done, type) {
      if (['add', 'edit'].includes(type)) {
        this.initData(this.form.id)
        if (type == 'edit' && this.form.imgBaseList.length) {
          this.fileList = Array.from(this.form.imgBaseList, item => ({
            name: item.id,
            url: item.scaleImgBase64,
            bigImg: '/imgFile/' + item.imgPath
          }))
        }
      }
      done()
    },
    beforeClose(done, type) {
      console.log(type)
      if (['add', 'edit'].includes(type)) {
        this.fileList = []
        this.initFaultCompList()
      }
      done()
    },
    currentChange(currentPage) {
      this.page.currentPage = currentPage
    },
    sizeChange(pageSize) {
      this.page.pageSize = pageSize
    },
    // 重置
    refreshChange() {
      this.onLoad(this.page, this.query)
    },
    // 页面加载
    onLoad(page, params = {}) {
      this.loading = true
      getFaultList({
        offset: page.currentPage,
        pageSize: page.pageSize,
        ...Object.assign(params, this.query)
      }).then(res => {
        this.data = res.data.data.data
        this.loading = false
        this.page.total = res.data.data.totalCount
      })
    },
    // 图片预览
    handlePictureCardPreview(file) {
      this.dialogImageUrl = file.bigImg || file.url
      this.dialogVisible = true
    },
    // 图片删除
    handleRemove(file) {
      this.fileList = this.fileList.filter(i => i.uid !== file.uid)
    }
  }
}
</script>
