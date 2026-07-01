<template>
  <!-- 测点状态 -->
  <div>
    <basic-container class="spot_table">
      <!--  :cell-style="cellStyle" -->
      <avue-crud
        :option="spotOption"
        :table-loading="loading"
        :data="data"
        ref="crud"
        v-model="form"
        :page.sync="page"
        :before-open="beforeOpen"
        :span-method="spanMethod"
        @search-change="searchChange"
        @search-reset="searchReset"
        @current-change="currentChange"
        @size-change="sizeChange"
        @refresh-change="refreshChange"
        @on-load="onLoad"
      >
        <template slot="menu" slot-scope="{ row }">
          <span style="color: #409eff; cursor: pointer" @click="getChart(row)">偏置电压趋势图</span>
        </template>
      </avue-crud>
    </basic-container>
    <el-dialog
      title="偏置电压趋势图"
      append-to-body
      :visible.sync="visibel"
      width="750px"
      custom-class="add_card"
    >
      <div class="dialod_content">
        <div style="margin-bottom: 15px">
          <!--  <label style="float: left; margin-right: 10px">时间范围：</label
          > --><!-- <dataComp :time="timeRange" @timeChange="timeChange"></dataComp> -->
          <el-date-picker
            v-model="timeRange"
            type="daterange"
            align="right"
            unlink-panels
            range-separator="-"
            start-placeholder="开始日期"
            end-placeholder="结束日期"
            :picker-options="pickerOptions"
            @change="getTrend"
          >
          </el-date-picker>
          <!--  <el-button style="margin-left: 20px" type="primary" size="small" @click="getTrend"
            >确定</el-button
          > -->
        </div>
        <chart v-if="JSON.stringify(chartData) !== '{}'" :chartData="chartData" height="400px" />
        <div class="nodata" v-else>
          <noData noteText="未查询到数据" firstText="" />
        </div>
      </div>
      <span slot="footer" class="dialog-footer">
        <el-button @click="visibel = false"><i class="el-icon-circle-close"></i>取 消</el-button>
      </span>
    </el-dialog>
  </div>
</template>

<script>
import dayjs from 'dayjs'
import { mapGetters } from 'vuex'
import { getSatusList, getVoltValue } from '@/api/equipment/spot'
import chart from '@/components/charts/baseChart'
import noData from '@/components/noData/index.vue'
// import dataComp from '@/components/timeComp/date.vue'
import { option } from './components/option'

export default {
  components: { /* dataComp,  */ chart, noData },
  computed: {
    ...mapGetters(['userDeptTree'])
  },
  data() {
    return {
      pickerOptions: {
        shortcuts: [
          {
            text: '最近一周',
            onClick(picker) {
              const end = new Date()
              const start = new Date()
              start.setTime(start.getTime() - 3600 * 1000 * 24 * 7)
              picker.$emit('pick', [start, end])
            }
          },
          {
            text: '最近一个月',
            onClick(picker) {
              const end = new Date()
              const start = new Date()
              start.setTime(start.getTime() - 3600 * 1000 * 24 * 30)
              picker.$emit('pick', [start, end])
            }
          },
          {
            text: '最近三个月',
            onClick(picker) {
              const end = new Date()
              const start = new Date()
              start.setTime(start.getTime() - 3600 * 1000 * 24 * 90)
              picker.$emit('pick', [start, end])
            }
          }
        ]
      },
      titleText: '',
      chartData: {},
      timeRange: [dayjs().subtract(1, 'months').format('YYYY-MM-DD'), dayjs().format('YYYY-MM-DD')],
      visibel: false,
      loading: false,
      chartLoading: false,
      query: {},
      selectOption: {}, //下拉筛选数组
      data: [],
      selectionList: [], //表格选中的数据
      allCompData: [], //所有部件数据
      allallCompData: [],
      unitList: [], //物理量数组
      CompData: [], //根据机组筛选部件
      editItemList: [], //点击编辑的数据
      page: {
        total: 0, // 总页数
        currentPage: 1, // 当前页数
        pageSize: 20 // 每页显示多少条
      },
      spotOption: {
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
        emptyBtnText: '重置',
        emptyBtnIcon: 'el-icon-refresh',
        column: [
          {
            label: '风场名称',
            prop: 'windParkName',
            search: true,
            type: 'select',
            cascader: ['windturbineName'],
            dicData: [],
            props: {
              label: 'name',
              value: 'id'
            },
            overHidden: true,
            change: this.windparkChange
          },
          {
            label: '机组名称',
            prop: 'windturbineName',
            search: true,
            type: 'select',
            dicData: [],
            props: {
              label: 'entityName',
              value: 'entityId'
            },
            filterable: true
          },
          { label: 'IP', prop: 'ip' },
          { label: '通道号', prop: 'channelNumber' },
          {
            label: '测点名称',
            prop: 'measlocName'
          },
          {
            label: '偏置电压',
            prop: 'voltValue'
          },
          {
            label: '状态',
            prop: 'senorAlarm',
            search: true,
            type: 'select',
            dicUrl: `api/wtphm-service/analyzerData/sensordcdata/sensoralarmStatus`,
            dicFormatter: res => {
              return res.data
            },
            props: {
              label: 'label',
              value: 'value'
            }
          },
          {
            label: '更新时间',
            prop: 'updateTime'
          }
        ]
      }
    }
  },

  mounted() {
    const wind = this.findObject(this.spotOption.column, 'windParkName')
    wind.dicData = this.userDeptTree
  },
  methods: {
    windparkChange(val) {
      const turbineId = this.findObject(this.spotOption.column, 'windturbineName')
      turbineId.dicData = this.userDeptTree.find(i => i.id == val.value).childNode
    },
    spanMethod({ row, column, rowIndex }) {
      if (column.property === 'windParkName') {
        if (rowIndex > 0 && row.windParkName === this.data[rowIndex - 1].windParkName) {
          return {
            rowspan: 0,
            colspan: 1
          }
        }

        let rowspan = 1
        for (let i = rowIndex + 1; i < this.data.length; i++) {
          if (this.data[i].windParkName === row.windParkName) {
            rowspan++
          } else {
            break
          }
        }
        return {
          rowspan: rowspan,
          colspan: 1
        }
      } else if (column.property === 'windturbineName') {
        if (rowIndex > 0 && row.windturbineName === this.data[rowIndex - 1].windturbineName) {
          return {
            rowspan: 0,
            colspan: 1
          }
        }

        let rowspan = 1
        for (let i = rowIndex + 1; i < this.data.length; i++) {
          if (this.data[i].windturbineName === row.windturbineName) {
            rowspan++
          } else {
            break
          }
        }
        return {
          rowspan: rowspan,
          colspan: 1
        }
      } else if (column.property === 'ip') {
        if (rowIndex > 0 && row.ip === this.data[rowIndex - 1].ip) {
          return {
            rowspan: 0,
            colspan: 1
          }
        }

        let rowspan = 1
        for (let i = rowIndex + 1; i < this.data.length; i++) {
          if (this.data[i].ip === row.ip) {
            rowspan++
          } else {
            break
          }
        }
        return {
          rowspan: rowspan,
          colspan: 1
        }
      }
      // 其他列不进行合并
      return {
        rowspan: 1,
        colspan: 1
      }
    },
    getChart(row) {
      this.visibel = true
      this.titleText = row.measlocName + ' -- 通道' + row.channelNumber
      this.measlocId = row.measlocId
      this.getTrend(row)
    },
    getTrend() {
      this.chartLoading = true
      getVoltValue({
        endTime: dayjs(this.timeRange[1]).format('YYYY-MM-DD'),
        startTime: dayjs(this.timeRange[0]).format('YYYY-MM-DD'),
        measlocId: this.measlocId
      }).then(res => {
        if (res.data.code == 200) {
          const { voltMaximumValue, voltMinimumValue, voltValueList } = res.data.data
          this.chartData = {
            ...option,
            title: {
              ...option.title,
              text: this.titleText
            },
            series: [
              {
                type: 'line',
                id: this.titleText,
                name: '直流偏置电压',
                data: voltValueList,
                symbol: 'emptyCircle',
                symbolSize: 10,
                markLine: {
                  label: {
                    show: false,
                    color: '#fff',
                    position: 'middle',
                    fontFamily: 'Microsoft YaHei'
                  },
                  data: [
                    {
                      name: '最低配置电压',
                      yAxis: voltMinimumValue,
                      lineStyle: {
                        color: '#FFE604',
                        type: 'solid'
                      },
                      emphasis: {
                        label: {
                          show: true,
                          formatter: '{b}: {c}'
                        }
                      }
                    },
                    {
                      name: '最高配置电压',
                      yAxis: voltMaximumValue,
                      lineStyle: {
                        color: '#FF0F0D',
                        type: 'solid'
                      },
                      emphasis: {
                        label: {
                          show: true,
                          formatter: '{b}: {c}'
                        }
                      }
                    }
                  ]
                }
              }
            ]
          }
        }
      })
    },
    //表格--加载
    onLoad(page, params = {}) {
      let obj = {
        offset: page.currentPage,
        pageSize: page.pageSize,
        ...Object.assign(params, this.query)
      }
      this.loading = true
      getSatusList({ ...obj }).then(res => {
        this.loading = false
        if (res.data.code == 200 && res.data.data) {
          const data = res.data.data.records || []
          this.page.total = res.data.data.total || 0
          data.forEach(item => {
            let { parentCompName, compName } = item
            item.allcompName = parentCompName + '_' + compName
          })
          this.data = data
        }
      })
    },
    //表格--搜索
    searchChange(params, done) {
      let newparam = {
        windPark: params.windParkName,
        windturbineId: params.windturbineName,
        senorAlarm: params.senorAlarm
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

      let obj1 = this.allCompData.find(j => j.name == parentCompName)
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
      this.CompData = arr
    }
  }
}
</script>

<style lang="scss">
/* @import url('../equipment/component/commonStyle.less');
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
</style>
