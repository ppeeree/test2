<template>
  <div class="containter_box">
    <div class="merge_header">报告管理</div>
    <basic-container class="main_table">
      <avue-crud
        :option="tableOption"
        :table-loading="loading"
        :data="data"
        ref="crud"
        v-model="form"
        v-model:page="page"
        :before-open="beforeOpen"
        @search-change="searchChange"
        @search-reset="searchReset"
        @current-change="currentChange"
        @size-change="sizeChange"
        @refresh-change="refreshChange"
        @on-load="onLoad"
      >
        <!--   <template slot="menuLeft">
          <el-button
            size="small"
            @click="
              () => {
                uploadVisible = true
              }
            "
            type="primary"
            icon="el-icon-upload2"
            >报告上传</el-button
          >
        </template> -->
        <template #menu="{ row }">
          <el-button
            type="text"
            icon="el-icon-download"
            size="small"
            @click.stop="downloadReport(row)"
            >报告下载
          </el-button>
          <el-button type="text" icon="el-icon-delete" size="small" @click.stop="deleteReport(row)"
            >删除报告
          </el-button>
        </template>
      </avue-crud>
    </basic-container>
  </div>
</template>

<script>
import dayjs from 'dayjs'
import { getList, addReport, deleteReportApi } from '@/api/reportManage/reportManage'
import { mapGetters } from 'vuex'

export default {
  computed: {
    ...mapGetters(['userDeptTree'])
  },
  data() {
    return {
      uploadVisible: false,
      formData: {
        windpark: ''
      },
      rules: {
        windpark: [{ required: true, message: '请选择风场名称', trigger: 'change' }]
      },
      fileBaseCode: null,
      fileType: 'docx',
      fileList: [],
      loading: false,
      query: {},
      data: [],
      page: {
        total: 0, // 总页数
        currentPage: 1, // 当前页数
        pageSize: 10 // 每页显示多少条
      },
      tableOption: {
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
        selection: false,
        searchShow: true,
        searchMenuSpan: 6,
        emptyBtnText: '重置',
        menuWidth: 400, // 操作栏宽度
        emptyBtnIcon: 'el-icon-refresh',
        column: [
          {
            label: '报告日期',
            prop: 'createTime',
            search: true,
            type: 'date',
            width: 200
          },
          {
            label: '风场',
            prop: 'windParkName',
            search: true,
            type: 'select',
            width: 200,
            dicData: [], //this.userDeptTree,
            props: {
              label: 'name',
              value: 'id'
            }
          },
          {
            label: '报告名称',
            prop: 'reportName',
            search: true,
            type: 'input'
          },
          {
            label: '备注信息',
            prop: 'remark'
          }
        ]
      }
    }
  },
  mounted() {
    const wind = this.findObject(this.tableOption.column, 'windParkName')
    wind.dicData = this.userDeptTree
  },
  methods: {
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
        if (res.data.success) {
          this.data = res.data.data.children
          this.page.total = res.data.data.totalCount
        } else {
          this.$message.error(res.data.msg)
        }
      })
    },
    //表格--搜索
    searchChange(params, done) {
      let newparam = {
        reportTime: params.createTime ? dayjs(params.createTime).format('YYYY-MM-DD') : '', //+ ' 00:00:00',
        reportName: params.reportName,
        windParkId: params.windParkName
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
    // 选择数量多出提示
    /*   handleExceed(files, fileList) {
      this.$message.warning(
        `当前限制选择 3 个文件，本次选择了 ${files.length} 个文件，共选择了 ${
          files.length + fileList.length
        } 个文件`
      )
    }, */
    // 上传文件选中
    // 当开启多选时 filelist有值
    /*  async handleFileChange(file, filelist) {
      const isLt20M = file.size / (1024 * 1024) < 20
      if (!isLt20M) {
        this.$message.error('上传文件大小不能超过 20MB!')
        this.$refs.upload.clearFiles()
        return
      }
      if (this.formData.windpark == '') {
        return this.$message.error('上传文件前请先选择风场！')
      }
      this.allUpload(file.raw)
    }, */
    /*  allUpload(file) {
      let formDatas = new FormData()
      formDatas.append('files', file)
      formDatas.append('windParkId', this.formData.windpark)
      addReport(formDatas)
        .then(res => {
          this.$message.success('上传成功')
          this.uploadVisible = false
          this.$refs.upload.clearFiles()
          this.onLoad(this.page, this.query)
        })
        .catch(err => {})
    }, */
    // 下载报告
    downloadReport(row) {
      let downloadElement = document.createElement('a')
      downloadElement.href =
        '/NetApi/DiagnosticReport/DownloadWindParkReport?reportGuid=' + row.reportGuid
      // let fileName = this.basceList.projectName + '.docx'
      // downloadElement.download = fileName
      document.body.appendChild(downloadElement)
      downloadElement.click()
      document.body.removeChild(downloadElement)
    },
    // 删除报告
    deleteReport(row) {
      this.$confirm('确定删除此报告?', {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning'
      }).then(() => {
        const reportId = row.diagReportGuid || row.DiagReportGuid || row.id || row.reportGuid
        deleteReportApi({ id: reportId }).then(res => {
          if (res.data.success) {
            this.$message.success(res.data.message)
            this.onLoad(this.page, this.query)
          } else {
            this.$message.error(res.data.message)
          }
        })
      })
    }
  }
}
</script>

<style lang="less" scoped>
.main_table {
  width: 100%;
  height: calc(100% - 36px);
  overflow: hidden;
  padding: 15px;
  button {
    cursor: pointer;
  }
  :deep(.avue-crud__menu){
    min-height: 0 !important;
  }
}

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
}
</style>
