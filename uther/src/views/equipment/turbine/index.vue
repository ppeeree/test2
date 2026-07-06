<template>
  <div>
    <timeLine isActive="1" />
    <basic-container>
      <avue-crud
        :option="option"
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
      >
        <template slot="empty">
          <div class="empty-flex-box">
            <el-image src="/img/WindTurbine/empty.png" fit="contain" />
            <span>暂无机组信息，请点击“新增”按钮进行配置</span>
          </div>
        </template>
        <template slot="menuLeft">
          <el-button
            type="success"
            size="small"
            plain
            icon="el-icon-upload2"
            @click.native="handleUpload"
            >批量上传
          </el-button>
          <el-button type="warning" size="small" plain icon="el-icon-download" @click="handleExport"
            >下载模板
          </el-button>
        </template>
        <template slot-scope="{ row }" slot="operationalDate">
          <span>{{ handleTime(row.operationalDate) }}</span>
        </template>
      </avue-crud>
      <el-dialog
        title="批量上传"
        append-to-body
        :visible.sync="uploadVisible"
        v-if="uploadVisible"
        v-dialogDrag
        width="555px"
      >
        <avue-form
          ref="uploadFormTi"
          v-model="uploadForm"
          :option="uploadOpt"
          class="upload-form"
        ></avue-form>
        <el-upload
          ref="upload"
          action="/api/wtphm-service/newFan/upload"
          :file-list="fileList"
          :auto-upload="false"
          accept=".xlsx,.xls"
          :multiple="true"
          :http-request="allUpload"
          :on-change="onUploadChange"
          class="upload-file"
        >
          <el-button slot="trigger" size="small" type="primary">选取文件</el-button>
          <el-button
            style="margin-left: 10px"
            size="small"
            type="success"
            :loading="btnLoading"
            class="upload-submit"
            @click="submitUpload('uploadFormTi')"
            >上传到服务器</el-button
          >
        </el-upload>
      </el-dialog>
    </basic-container>
  </div>
</template>

<script>
import { getLazyList } from '@/api/system/dept'
import {
  detailFan,
  deleteFan,
  // getProvince,
  insertFan,
  updateFanDelete,
  downTemplate,
  upload
} from '@/api/equipment/turbine'
import NProgress from 'nprogress'
import 'nprogress/nprogress.css'
import { downloadXls } from '@/util/util'
import { getRouterPath } from '@/util/exp'
import isArray from 'lodash/isArray'
import cloneDeep from 'lodash/cloneDeep'
import timeLine from '../component/timeLine.vue'
import { mapGetters } from 'vuex'

const numberValidatePass = (rule, value, callback) => {
  let reg = /^[0-9]*$/
  let n = reg.test(value)
  if (!n) {
    return callback(new Error('为数字'))
  }
  return callback()
}

const ChineseValidatePass = (rule, value, callback) => {
  let reg = /[\u4e00-\u9fa5]/
  let n = reg.test(value)
  if (n) {
    return callback(new Error('不含中文'))
  }
  return callback()
}

export default {
  components: { timeLine },
  data() {
    return {
      form: {},
      uploadForm: {},
      query: {},
      loading: true,
      uploadVisible: false,
      page: {
        pageSize: 10,
        currentPage: 1,
        total: 0
      },
      fileList: [],
      option: {
        height: 'auto',
        calcHeight: 80,
        tip: false,
        simplePage: false,
        searchShow: true,
        searchMenuSpan: 6,
        tree: false,
        border: true,
        index: true,
        selection: false,
        viewBtn: false,
        dialogDrag: true,
        // dialogWidth: 900,
        dialogClickModal: false,
        emptyBtnText: '重置',
        emptyBtnIcon: 'el-icon-refresh',
        // dialogType: 'drawer',
        dialogType: 'drawer',
        dialogDirection: 'ttb',
        emptyText: '暂无机组信息，请点击“新增”按钮进行配置',
        column: []
      },
      data: [],
      isViewOrEdit: false,
      uploadOpt: {
        submitBtn: false,
        emptyBtn: false,
        labelWidth: 0,
        column: [
          {
            label: '风场名称',
            prop: 'deptCode',
            type: 'cascader',
            props: {
              label: 'fullName',
              // deptCode
              value: 'id'
            },
            rules: [
              {
                type: 'array',
                required: true,
                message: '请选择风场名称',
                trigger: 'change'
              }
            ],
            dicData: [],
            checkStrictly: true,
            filterable: true
          }
        ]
      },
      btnLoading: false,
      tableAllColumn: [
        {
          label: '风场名称',
          prop: 'deptCode',
          type: 'cascader',
          span: 12,
          search: true,
          hide: false,
          display: false,
          props: {
            label: 'fullName',
            // deptCode
            value: 'id'
          },
          rules: [
            {
              type: 'array',
              required: true,
              message: '请选择单位名称',
              trigger: 'change'
            }
          ],
          dicData: [],
          checkStrictly: true,
          editDisabled: true,
          filterable: true,
          overHidden: true,
          showAllLevels: false
        },
        {
          label: '风机名称',
          prop: 'spaceName',
          span: 12,
          search: true,
          /*type: 'select',
          props: {
            label: 'spaceName',
            value: 'spaceName'
          },
          dicData: [],*/
          overHidden: true,
          display: false,
          rules: [
            {
              required: true,
              message: '请输入风机名称',
              trigger: 'blur'
            },
            { min: 1, max: 50, message: '长度在 1 到 50 个字符', trigger: 'blur' }
          ]
        },
        {
          label: '风机号',
          prop: 'fanNumber',
          search: true,
          overHidden: true,
          display: false,
          span: 12
          /*type: 'select',
          props: {
            label: 'fanNumber',
            value: 'fanNumber'
          },
          dicData: []*/
          /* rules: [
            {
              required: true,
              message: '请输入风机号',
              trigger: 'blur'
            }, { min: 1, max: 50, message: '长度在 1 到 50 个字符', trigger: 'blur' }
          ] */
        },
        {
          label: '经度',
          prop: 'longitude',
          type: 'number',
          overHidden: true,
          span: 12,
          display: false,
          rules: [
            {
              required: true,
              message: '请输入经度',
              trigger: 'blur'
            }
            // { min: 1, max: 20, message: '长度在 1 到 20 个字符', trigger: 'blur' },
            // { type: 'number', message: '必须为数字值'}
          ]
        },
        {
          label: '纬度',
          prop: 'latitude',
          overHidden: true,
          type: 'number',
          display: false,
          span: 12,
          rules: [
            {
              required: true,
              message: '请输入纬度',
              trigger: 'blur'
            }
            // { min: 1, max: 20, message: '长度在 1 到 20 个字符', trigger: 'blur' },
            // { type: 'number', message: '必须为数字值'}
          ]
        },
        {
          label: '高程',
          prop: 'elevation',
          overHidden: true,
          type: 'number',
          display: false,
          span: 12,
          rules: [
            {
              required: true,
              message: '请输入高程',
              trigger: 'blur'
            }
            // { min: 1, max: 20, message: '长度在 1 到 20 个字符', trigger: 'blur' },
            // { type: 'number', message: '必须为数字值'}
          ]
        },
        {
          label: '风机型号',
          prop: 'fanModel',
          overHidden: true,
          display: false,
          search: true,
          span: 12,
          blur: ({ column, value }) => this.fanModelBlur({ column, value }),
          /* type: 'select',
          props: {
            label: 'fanModel',
            value: 'fanModel'
          },
          dicData: [],*/
          rules: [
            {
              required: true,
              message: '请输入风机型号',
              trigger: 'blur'
            },
            { min: 1, max: 20, message: '长度在 1 到 20 个字符', trigger: 'blur' }
          ]
        },
        {
          label: '风机生产厂家',
          width: 100,
          labelWidth: 100,
          prop: 'fanManufacturer',
          overHidden: true,
          display: false,
          span: 12,
          rules: [
            {
              required: true,
              message: '请输入风机生产厂家',
              trigger: 'blur'
            },
            { min: 1, max: 50, message: '长度在 1 到 50 个字符', trigger: 'blur' }
          ]
        },
        {
          label: '容量/MW',
          prop: 'capacity',
          overHidden: true,
          type: 'number',
          display: false,
          span: 12,
          rules: [
            {
              required: true,
              message: '请输入容量',
              trigger: 'blur'
            }
            // { min: 1, max: 20, message: '长度在 1 到 20 个字符', trigger: 'blur' },
            // { type: 'number', message: '必须为数字值'}
          ]
        },
        {
          label: '风机高度(M)',
          prop: 'fanHeight',
          overHidden: true,
          display: false,
          type: 'number',
          labelWidth: 100,
          span: 12,
          rules: [
            /*{
              required: true,
              message: '请输入风机高度',
              trigger: 'blur'
            }, { min: 1, max: 20, message: '长度在 1 到 20 个字符', trigger: 'blur' }*/
            // { type: 'number', message: '必须为数字值', trigger: 'blur'}
          ]
        },
        {
          label: '叶片长度(M)',
          prop: 'bladeLength',
          labelWidth: 100,
          overHidden: true,
          type: 'number',
          display: false,
          span: 12,
          rules: [
            /*{
              required: true,
              message: '请输入叶片长度',
              trigger: 'blur'
            }, { min: 1, max: 20, message: '长度在 1 到 20 个字符', trigger: 'blur' }*/
            // { type: 'number', message: '必须为数字值', trigger: 'blur'}
          ]
        },
        {
          label: '叶片生产厂家',
          width: 100,
          labelWidth: 100,
          overHidden: true,
          display: false,
          prop: 'bladeManufacturer',
          span: 12
          /* rules: [
            {
              required: true,
              message: '请输入叶片生产厂家',
              trigger: 'blur'
            }, { min: 1, max: 50, message: '长度在 1 到 50 个字符', trigger: 'blur' }
          ] */
        },
        /*{
          label: '省份',
          prop: 'regionName',
          type: 'select',
          overHidden: true,
          span: 12,
          rules: [
            {
              required: true,
              message: '请选择省份',
              trigger: 'change'
            }
          ],
          dicData: [],
          editDisabled: true
        },*/
        {
          label: '投运时间',
          prop: 'operationalDate',
          width: 100,
          span: 12,
          type: 'date',
          valueFormat: 'yyyy-MM-dd',
          overHidden: true,
          display: false,
          rules: [
            {
              required: true,
              message: '请选择投运时间',
              trigger: 'change'
            }
          ]
        },
        {
          label: '监测方案',
          width: 100,
          labelWidth: 100,
          overHidden: true,
          display: false,
          prop: 'measureTypeName',
          span: 12
          /* rules: [
            {
              required: true,
              message: '请输入叶片生产厂家',
              trigger: 'blur'
            }, { min: 1, max: 50, message: '长度在 1 到 50 个字符', trigger: 'blur' }
          ] */
        }
      ],
      tableAllColumnGroup: {
        group: [
          {
            label: '基本信息',
            prop: 'info',
            icon: 'el-icon-comp-diy',
            column: [
              {
                label: '风场名称',
                prop: 'deptCode',
                type: 'cascader',
                span: 12,
                search: true,
                hide: false,
                props: {
                  label: 'fullName',
                  // deptCode
                  value: 'id'
                },
                rules: [
                  {
                    type: 'array',
                    required: true,
                    message: '请选择单位名称',
                    trigger: 'change'
                  }
                ],
                dicData: [],
                checkStrictly: true,
                editDisabled: true,
                filterable: true,
                overHidden: true,
                showAllLevels: false
              },
              {
                label: '机组名称',
                prop: 'spaceName',
                span: 12,
                search: true,
                /*type: 'select',
                props: {
                  label: 'spaceName',
                  value: 'spaceName'
                },
                dicData: [],*/
                overHidden: true,
                rules: [
                  {
                    required: true,
                    message: '请输入风机名称',
                    trigger: 'blur'
                  },
                  { min: 1, max: 50, message: '长度在 1 到 50 个字符', trigger: 'blur' }
                ]
              },
              {
                label: '机组编码',
                prop: 'spaceCode',
                span: 12,
                search: false,
                overHidden: true,
                editDisabled: true,
                rules: [
                  {
                    required: true,
                    message: '请输入风机编码',
                    trigger: 'blur'
                  },
                  { min: 4, max: 4, message: '长度在 4 个字符', trigger: 'blur' },
                  { validator: numberValidatePass, trigger: 'blur' }
                ]
              },
              {
                label: '机组号',
                prop: 'fanNumber',
                search: true,
                overHidden: true,
                display: false,
                span: 12,
                /*type: 'select',
                props: {
                  label: 'fanNumber',
                  value: 'fanNumber'
                },
                dicData: []*/
                rules: [
                  {
                    required: true,
                    message: '请输机组号',
                    trigger: 'blur'
                  },
                  { min: 1, max: 50, message: '长度在 1 到 50 个字符', trigger: 'blur' },
                  { validator: ChineseValidatePass, trigger: 'blur' }
                ]
              }
            ]
          },
          {
            label: '投运信息',
            prop: 'edit',
            icon: 'el-icon-pinwheel-diy',
            column: [
              {
                label: '经度',
                prop: 'longitude',
                overHidden: true,
                type: 'number',
                span: 12,
                rules: [
                  {
                    required: true,
                    message: '请输入经度',
                    trigger: 'blur'
                  }
                  // { min: 1, max: 20, message: '长度在 1 到 20 个字符', trigger: 'blur' },
                  // { type: 'number', message: '必须为数字值', trigger: 'blur'}
                ]
              },
              {
                label: '纬度',
                prop: 'latitude',
                overHidden: true,
                type: 'number',
                span: 12,
                rules: [
                  {
                    required: true,
                    message: '请输入纬度',
                    trigger: 'blur'
                  }
                  // { min: 1, max: 20, message: '长度在 1 到 20 个字符', trigger: 'blur' },
                  // { type: 'number', message: '必须为数字值', trigger: 'blur'}
                ]
              },
              {
                label: '海拔',
                prop: 'elevation',
                overHidden: true,
                type: 'number',
                span: 12,
                rules: [
                  {
                    required: true,
                    message: '请输入海拔',
                    trigger: 'blur'
                  }
                  // { min: 1, max: 20, message: '长度在 1 到 20 个字符', trigger: 'blur' },
                  // { type: 'number', message: '必须为数字值', trigger: 'blur'}
                ]
              },
              {
                label: '投运时间',
                prop: 'operationalDate',
                width: 100,
                span: 12,
                type: 'date',
                valueFormat: 'yyyy-MM-dd',
                overHidden: true,
                rules: [
                  {
                    required: true,
                    message: '请选择投运时间',
                    trigger: 'change'
                  }
                ]
              }
            ]
          },
          {
            label: '机组属性',
            prop: 'jbxx',
            icon: 'el-icon-box-diy',
            column: [
              {
                label: '机组型号',
                prop: 'fanModel',
                overHidden: true,
                search: true,
                span: 12,
                blur: ({ column, value }) => this.fanModelBlur({ column, value }),
                /* type: 'select',
                props: {
                  label: 'fanModel',
                  value: 'fanModel'
                },
                dicData: [],*/
                rules: [
                  {
                    required: true,
                    message: '请输入风机型号',
                    trigger: 'blur'
                  },
                  { min: 1, max: 20, message: '长度在 1 到 20 个字符', trigger: 'blur' }
                ]
              },
              {
                label: '容量/MW',
                prop: 'capacity',
                overHidden: true,
                type: 'number',
                span: 12,
                rules: [
                  {
                    required: true,
                    message: '请输入容量',
                    trigger: 'blur'
                  }
                  // { min: 1, max: 20, message: '长度在 1 到 20 个字符', trigger: 'blur' },
                  // { type: 'number', message: '必须为数字值', trigger: 'blur'}
                ]
              },
              {
                label: '风机高度(M)',
                prop: 'fanHeight',
                overHidden: true,
                type: 'number',
                labelWidth: 100,
                span: 12,
                rules: [
                  /*{
                    required: true,
                    message: '请输入风机高度',
                    trigger: 'blur'
                  }, { min: 1, max: 20, message: '长度在 1 到 20 个字符', trigger: 'blur' }*/
                ]
              },
              {
                label: '风机生产厂家',
                width: 100,
                labelWidth: 100,
                prop: 'fanManufacturer',
                overHidden: true,
                span: 12,
                rules: [
                  {
                    required: true,
                    message: '请输入风机生产厂家',
                    trigger: 'blur'
                  }
                  // { min: 1, max: 50, message: '长度在 1 到 50 个字符', trigger: 'blur' }
                ]
              },
              {
                label: '叶片长度(M)',
                prop: 'bladeLength',
                labelWidth: 100,
                type: 'number',
                overHidden: true,
                span: 12,
                rules: [
                  /*{
                    required: true,
                    message: '请输入叶片长度',
                    trigger: 'blur'
                  }, { min: 1, max: 20, message: '长度在 1 到 20 个字符', trigger: 'blur' }*/
                ]
              },
              {
                label: '叶片生产厂家',
                width: 100,
                labelWidth: 100,
                overHidden: true,
                prop: 'bladeManufacturer',
                span: 12
                /* rules: [
                  {
                    required: true,
                    message: '请输入叶片生产厂家',
                    trigger: 'blur'
                  }, { min: 1, max: 50, message: '长度在 1 到 50 个字符', trigger: 'blur' }
                ] */
              },
              {
                label: '测量方案',
                width: 100,
                labelWidth: 100,
                overHidden: true,
                prop: 'measureTypeId',
                type: 'select',
                display: true,
                span: 12,
                props: {
                  label: 'label',
                  value: 'value'
                },
                dicUrl: '/api/wtphm-service/DeviceMeasloc/getModelMeaslocConf',
                dicFormatter: res =>
                  res.data.map(({ solutionName, children }) => {
                    return {
                      label: solutionName,
                      value: children?.length > 0 ? children[0]?.id || '' : ''
                    }
                  }),
                rules: [
                  {
                    required: true,
                    message: '请选择测量方案',
                    trigger: 'change'
                  }
                ]
              },
              {
                label: '测量方案',
                width: 100,
                labelWidth: 100,
                overHidden: true,
                prop: 'measureTypeName',
                display: false,
                span: 12,
                disabled: true,
                placeholder: ' '
              }
            ]
          }
        ]
      }
    }
  },
  watch: {
    'form.deptCode': {
      handler: function (val) {
        if (!val || this.isViewOrEdit) return
        this.handleDeptCodeValidate(val, 'crud')
      }
    },
    'uploadForm.deptCode': {
      handler: function (val) {
        if (!val) return
        this.handleDeptCodeValidate(val, 'uploadFormTi')
      }
    },
    uploadVisible: {
      handler: function () {
        this.fileList = []
        this.uploadForm = {}
      }
    }
  },
  created() {
    this.getLazyList()
  },
  computed: {
    changDeptCode() {
      return this.form.deptCode
    },
    ...mapGetters(['userInfo', 'permission']),
    permissionList() {
      return {
        addBtn: this.vaildData(this.permission.equipment_turbine_add, false),
        viewBtn: this.vaildData(this.permission.equipment_turbine_view, false),
        delBtn: this.vaildData(this.permission.equipment_turbine_delete, false),
        editBtn: this.vaildData(this.permission.equipment_turbine_edit, false)
      }
    }
  },
  methods: {
    fanModelBlur(val) {
      const data = {
        current: 1,
        size: 9999,
        fanModel: val.value
      }
      this.$refs.crud.$refs.dialogForm.$refs.tableForm.allDisabled = true
      detailFan(data)
        .then(res => {
          const notKey = ['spaceName', 'longitude', 'latitude', 'elevation', 'fanNumber']
          let obj = res.data.data.records[0].fanVos[0]
          for (const key in obj) {
            const element = obj[key]
            if (
              Object.hasOwnProperty.call(this.form, key) &&
              this.form[key]?.length === 0 &&
              !notKey.includes(key)
            )
              this.form[key] = element
          }
          if (this.form.deptCode.length === 0) {
            const deptCode = obj.stationId
            let parentShip = getRouterPath(deptCode, 'id', this.tableAllColumn.at(0).dicData)
            this.form.deptCode = parentShip.length === 0 ? deptCode : parentShip
          }
        })
        .finally(() => (this.$refs.crud.$refs.dialogForm.$refs.tableForm.allDisabled = false))
    },
    rowSave(row, done, loading) {
      const deptCodeCopy = cloneDeep(row.deptCode)
      // row.deptCode = row.deptCode.at(-1)
      /*row.spaceGuid = ''
      const targetObj = this.treeToArray(this.tableAllColumn.at(0).dicData)
      if (targetObj.length > 0) {
        row.spaceGuid = targetObj.find(o => o.deptCode === row.deptCode)?.id || ''
      }*/
      row.createUser = this.userInfo?.user_id || ''
      row.stationId = deptCodeCopy.at(-1)
      row.deptCode = ''
      row.fanNumber = row.spaceCode
      console.log('保存机组参数', row)
      insertFan(row)
        .then(() => {
          this.onLoad(this.page)
          this.$message({
            type: 'success',
            message: '操作成功!'
          })
          done()
        })
        .catch(error => {
          console.log(error)
          loading()
        })
        .finally(() => {
          row.deptCode = deptCodeCopy
          row.stationId = ''
        })
    },
    submitUpload(formName) {
      this.$refs[formName].$refs.form.validateField(['deptCode'], valid => {
        if (!valid) {
          this.$refs.upload.submit()
        }
      })
    },
    rowUpdate(row, index, done, loading) {
      const deptCodeCopy = cloneDeep(row.deptCode)
      isArray(deptCodeCopy) && (row.deptCode = deptCodeCopy.at(-1))
      row.createUser = this.userInfo?.user_id || ''
      row.stationId = deptCodeCopy.at(-1)
      row.deptCode = ''
      updateFanDelete(row)
        .then(() => {
          this.onLoad(this.page)
          this.$message({
            type: 'success',
            message: '操作成功!'
          })
          done()
        })
        .catch(error => {
          console.log(error)
          loading()
        })
        .finally(() => {
          row.deptCode = deptCodeCopy
          row.stationId = ''
        })
    },
    rowDel(row) {
      let len = this.data.length
      this.$confirm('确定将选择数据删除?', {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning'
      })
        .then(() => {
          return deleteFan(row.spaceGuid)
        })
        .then(res => {
          len === 1 &&
            this.page.currentPage > 1 &&
            (this.page.currentPage = this.page.currentPage - 1)
          this.onLoad(this.page)
          this.$message({
            type: res.data.code == 200 ? 'success' : 'error',
            message: res.data.msg
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
      if (params?.deptCode) this.query.deptCode = this.query.deptCode.at(-1)
      this.onLoad(this.page, params)
      done()
    },
    currentChange(currentPage) {
      this.page.currentPage = currentPage
    },
    sizeChange(pageSize) {
      this.page.pageSize = pageSize
    },
    refreshChange() {
      this.onLoad(this.page, this.query)
    },
    onLoad(page, params = {}) {
      this.loading = true
      const { currentPage, pageSize } = page
      const data = {
        current: currentPage,
        size: pageSize,
        ...Object.assign(params, this.query)
      }
      detailFan(data).then(res => {
        for (let i = 0; i < res.data.data.records[0].fanVos.length; i++) {
          const element = res.data.data.records[0].fanVos[i]
          element.deptCode = element?.stationId || ''
        }
        this.data = res.data.data.records[0].fanVos
        // this.tableAllColumn.at(1).dicData = this.data
        // this.tableAllColumn.at(2).dicData = this.data
        // this.tableAllColumn.at(6).dicData = this.data
        this.page.total = res.data.data.total
        this.handleTableColumn(res.data.data.records[0])
        this.loading = false
      })
    },
    handleTableColumn(data) {
      data.dataKVos.push({
        code: 'spaceCode',
        name: '机组编码'
      })
      for (let i = 0; i < data.dataKVos.length; i++) {
        const element = data.dataKVos[i]
        if (element.code === 'deptName') {
          element.code = 'deptCode'
          break
        }
      }
      const columnKeys = data.dataKVos.map(({ code }) => code)
      this.option.column = this.tableAllColumn
        .filter(item => {
          const isKeys = columnKeys.includes(item.prop)
          if (isKeys) item.label = data.dataKVos.find(o => o.code === item.prop).name
          return isKeys
        })
        .sort((a, b) => {
          return columnKeys.indexOf(a.prop) - columnKeys.indexOf(b.prop)
        })
      this.option.group = this.tableAllColumnGroup.group
    },
    handleTime(time) {
      return time.split(' ')[0]
    },
    handleExport() {
      this.$confirm('是否导出模板?', '提示', {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning'
      }).then(() => {
        NProgress.start()
        downTemplate().then(res => {
          downloadXls(res.data, '风机模板.xlsx')
          NProgress.done()
        })
      })
    },
    handleUpload() {
      this.uploadVisible = true
    },
    getLazyList() {
      getLazyList().then(res => {
        let data = res.data.data
        this.tableAllColumn.at(0).dicData = data
        this.tableAllColumnGroup.group[0].column.at(0).dicData = data
        this.uploadOpt.column.at(0).dicData = data
      })
      /*getProvince().then(res => {
        this.tableAllColumn.at(12).dicData = res.data.data.map(item => {
          return {
            label: item,
            value: item
          }
        })
      })*/
    },
    beforeOpen(done, type) {
      const isEdit = ['edit', 'view'].includes(type)
      for (let i = 0; i < this.tableAllColumnGroup.group.length; i++) {
        if (this.tableAllColumnGroup.group[i].prop === 'jbxx') {
          for (let j = 0; j < this.tableAllColumnGroup.group[i].column.length; j++) {
            this.tableAllColumnGroup.group[i].column[j].prop === 'measureTypeId' &&
              (this.tableAllColumnGroup.group[i].column[j].display = !isEdit)
            if (
              [
                'measureTypeName',
                'bladeManufacturer',
                'bladeLength',
                'fanManufacturer',
                'fanHeight',
                'capacity',
                'fanModel'
              ].includes(this.tableAllColumnGroup.group[i].column[j].prop)
            )
              this.tableAllColumnGroup.group[i].column[j].display = isEdit
          }
          break
        }
      }
      if (isEdit) {
        this.isViewOrEdit = true
        const deptCode = this.form.deptCode
        let parentShip = getRouterPath(deptCode, 'id', this.tableAllColumn.at(0).dicData)
        this.form.deptCode = parentShip.length === 0 ? deptCode : parentShip
      } else {
        this.isViewOrEdit = false
      }
      done()
    },
    handleDeptCodeValidate(val, refKey) {
      if (val.length) {
        this.$refs[refKey].clearValidate(['deptCode'])
      } else {
        this.$refs[refKey].validateField(['deptCode'])
      }
    },
    allUpload(data) {
      this.btnLoading = true
      let fd = new FormData()
      fd.append('file', data.file)
      fd.append('stationId', this.uploadForm.deptCode.at(-1))
      fd.append('createUser', this.userInfo?.user_id || '')
      upload(fd)
        .then(result => {
          this.fileList.push({
            name: data.file.name
          })
          this.$message[result.data?.data ? 'success' : 'error'](result.data.msg)
          this.onLoad(this.page)
          this.btnLoading = false
          this.uploadVisible = false
          return false
        })
        .catch(() => {
          this.$message.error('数据添加失败')
          this.btnLoading = false
        })
    },
    onUploadChange(file) {
      const isXls = file.name.substring(file.name.lastIndexOf('.') + 1)
      const isLt10M = file.size / (1024 * 10) < 10
      if (!['xlsx', 'xls'].includes(isXls)) {
        this.onRemoveFile(file, 'upload')
        this.$message.error('上传文件只能是 xlsx 格式或xls!')
        return
      }
      if (!isLt10M) {
        this.$message.error('上传文件大小不能超过 10MB!')
        this.onRemoveFile(file, 'upload')
        return
      }
    },
    onRemoveFile(file, refKey) {
      const index = this.$refs[refKey].uploadFiles.findIndex(e => e.uid === file.uid)
      this.$refs[refKey].uploadFiles.splice(index, 1)
    },
    treeToArray(source) {
      let res = []
      source.forEach(item => {
        res.push(item)
        item.children && res.push(...this.treeToArray(item.children))
      })
      return res.map(item => {
        if (item.children) {
          delete item.children
        }
        return item
      })
    }
  }
}
</script>
<style lang="less" scoped>
// @import url('../component/commonStyle.less');
:deep(.avue-form__group .el-col){
  width: 20%;
}
:deep(.el-form-item__content){
  margin-left: 100px !important;
}
:deep(.el-pagination__sizes){
  color: white;
}

:deep(.el-pagination .el-select .el-input .el-input__inner){
  border: 1px white solid;
  background: transparent;
}

:deep(.el-pagination__editor.el-input .el-input__inner){
  border: 1px white solid;
  background: transparent;
}

:deep(.el-table){
  height: 555px !important;
  max-height: 555px !important;
}

.upload-submit {
  position: absolute;
  bottom: 12px;
  left: 40%;
}

.upload-file {
  display: flex;
  align-items: flex-end;
  flex-flow: row-reverse;
  justify-content: space-between;
  margin-bottom: 40px;

  :deep(.el-upload){
    margin-left: 10px;
  }
}

:deep(.el-dialog__body){
  padding: 6px 30px 20px 20px;
}

:deep(.el-dialog){
  position: relative;
  width: 555px;
  top: 34%;
  margin-top: 0 !important;
}

:deep(.el-upload-list){
  width: 100%;

  .el-upload-list__item {
    height: 32px;
    background: rgba(71, 86, 128, 0.3);
    display: flex;
    align-items: center;

    .el-upload-list__item-status-label {
      top: 6px;
    }

    .el-upload-list__item-name {
      padding-left: 10px;
    }

    .el-icon-close {
      top: 10px;
      color: white;
    }
  }
}

:deep(.upload-form){
  .avue-form__group {
    .avue-form__row {
      .el-form-item--small {
        margin-bottom: 6px;
        .el-input__inner {
          width: 415px;
          margin-left: -100px;
        }
        .el-input__suffix {
          left: 285px;
        }
        .el-form-item__error {
          width: 100px;
        }
        .el-form-item__label {
          display: none;
        }
      }
    }

    .no-print {
      display: none;
    }
  }
}

.empty-flex-box {
  display: flex;
  flex-direction: row;
  align-items: center;
  justify-content: center;
  span {
    font-size: 14px;
    font-weight: 350;
    line-height: 17px;
    letter-spacing: 0.05em;
  }
}
</style>
