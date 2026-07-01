<template>
  <basic-container>
    <avue-crud
      :option="Option"
      :table-loading="loading"
      :data="data"
      ref="crud"
      v-model="form"
      :page.sync="page"
      :before-open="beforeOpen"
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
    </avue-crud>
  </basic-container>
</template>

<script>
import { getList } from '@/api/alarmFault/deviceTree'
export default {
  data() {
    return {
      query: {},
      data: [],
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
            label: '故障编号',
            prop: 'windparkName',
            search: true
          },
          {
            label: '故障名称',
            prop: 'windturbineName',
            search: true
          },
          {
            label: '版本号',
            prop: 'allcompName'
          },
          {
            label: '故障排序',
            prop: 'measlocName'
          },
          {
            label: '故障备注',
            prop: 'measlocName'
          }
        ]
      }
    }
  },
  methods: {
    //表格--加载
    onLoad(page, params = {}) {
      //   let obj = {
      //     offset: page.currentPage,
      //     pageSize: page.pageSize,
      //     ...Object.assign(params, this.query)
      //   }
      //   this.loading = true
      /*getList({ ...obj }).then(res => {
        this.loading = false
        if (res.data.code == 200 && res.data.data) {
          const data = res.data.data.data
          this.page.total = res.data.data.totalCount
          data.forEach(item => {
            let { parentCompName, compName } = item
            item.allcompName = parentCompName + '_' + compName
          })
          this.data = data
        }
      })*/
    },
    //表格--搜索
    searchChange(params, done) {
      this.query = params
      this.parentId = ''
      this.page.currentPage = 1
      this.onLoad(this.page, params)
      done()
    },
    //表格--重置
    searchReset() {
      this.query = {}
      this.onLoad(this.page)
    }
  }
}
</script>

<style lang="less" scoped></style>
