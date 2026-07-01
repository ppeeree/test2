<template>
  <div class="right">
    <div class="right_content">
      <!-- 折叠面板 -->
      <div class="report_information">
        <el-collapse v-model="activeName">
          <el-collapse-item title="基本信息" name="1">
            <el-row>
              <el-col :span="16" class="other_report">
                <div class="unit_param" v-for="key in Object.keys(baseInfoKeys)" :key="key">
                  <template v-if="key == '监测时间'">
                    <span>{{ key }}：</span>
                    <el-date-picker
                      align="right"
                      unlink-panels
                      range-separator="~"
                      start-placeholder="开始"
                      end-placeholder="结束"
                      ref="time"
                      :clearable="false"
                      v-model="monitorTime"
                      type="daterange"
                      value-format="yyyy-MM-dd"
                      prefix-icon=""
                      :disabled="isShowExitReport"
                      clear-icon=""
                    >
                    </el-date-picker>
                  </template>
                  <template v-else>
                    <span>{{ key }}：</span>
                    <b :title="basceList[baseInfoKeys[key]]">{{
                      basceList[baseInfoKeys[key]] || '--'
                    }}</b>
                  </template>
                </div>
              </el-col>
              <el-col :span="8" class="input_report">
                <div class="input_title">项目描述：</div>
                <el-input
                  type="textarea"
                  :rows="4"
                  placeholder="项目描述"
                  v-model="projectDesc"
                  resize="none"
                  :disabled="isShowExitReport"
                >
                </el-input> </el-col
            ></el-row>
          </el-collapse-item>
        </el-collapse>
      </div>
      <!-- 下方表格 -->
      <div class="detail_analysis">
        <!--  勿删 -->
        <!-- <div>
          机组诊断情况汇总
          <el-button
            style="float: right"
            type="primary"
            size="small"
            title="对风场下的正常机组后台自动生成“机组诊断”报告，刷新左侧树，查看生成情况，可能需要一段时间！完成后点击风场可进行查看。"
            @click="getNormalTurbineDiag"
            >自动生成正常机组报告</el-button
          >
        </div> -->
        <div class="detail_analysis_header">
          <h3>机组诊断情况汇总</h3>
          <div class="detail_analysis_header_right">
            <p>
              报告月份：
              <el-date-picker
                v-model="reportTime"
                type="month"
                placeholder="选择月份"
                format="yyyy 年 MM 月"
                :clearable="false"
                :disabled="isShowExitReport"
              >
              </el-date-picker>
            </p>
            <p>
              机组<b>{{ basceList.windTurbineCount || 0 }}</b
              >台
            </p>
            <p>
              <el-button
                @click="filterTableData(0)"
                :class="{ active: filterStatus == 0 }"
                :disabled="isShowExitReport"
                size="mini"
                plain
                >已诊断</el-button
              ><b>{{ basceList.diagnosisWindturbineCount || 0 }}</b
              >台
            </p>
            <p>
              <el-button
                @click="filterTableData(1)"
                :class="{ active: filterStatus == 1 }"
                :disabled="isShowExitReport"
                size="mini"
                plain
                >未诊断</el-button
              ><b>{{ basceList.noDiagnosisWindturbineCount || 0 }}</b
              >台
            </p>
          </div>
        </div>
        <el-table
          ref="multipleTable"
          :data="tableData"
          tooltip-effect="light"
          style="width: 100%"
          @selection-change="handleSelectionChange"
          @select="handleSelecRow"
          :span-method="objectSpanMethod"
          :cell-style="setBgColor"
          max-height="520"
          size="small"
          row-key="diagRecordGuid"
        >
          <el-table-column
            :selectable="checkSelectable"
            :reserve-selection="true"
            type="selection"
            width="55"
            prop="diagRecordGuid"
          >
          </el-table-column>
          <el-table-column prop="windturbineName" label="机组" width="120"> </el-table-column>
          <el-table-column prop="diagnosisTime" label="诊断时间" width="180"> </el-table-column>
          <el-table-column prop="status" label="健康状态" width="80">
            <template slot-scope="scope">
              <span
                :style="{
                  display: 'inline-block',
                  width: '100%'
                }"
                >{{ eventTypeEnum[scope.row.status] }}</span
              >
            </template>
          </el-table-column>
          <el-table-column prop="compName" label="部件" width="120"> </el-table-column>
          <el-table-column prop="compStatus" label="部件状态" width="80">
            <template slot-scope="scope">
              <span
                :style="{
                  display: 'inline-block',
                  width: '100%'
                }"
                >{{ eventTypeEnum[scope.row.compStatus] }}</span
              >
            </template>
          </el-table-column>
          <el-table-column prop="diagnosisConclusion" label="诊断结论" show-overflow-tooltip>
          </el-table-column>
          <el-table-column prop="maintainAdvice" label="维护建议" show-overflow-tooltip>
          </el-table-column>
          <el-table-column prop="runingAdvice" label="运行建议"> </el-table-column>
        </el-table>
      </div>
    </div>
    <div class="right_bottom" v-if="!isShowExitReport">
      <el-button icon="el-icon-plus" :loading="submitLoading" @click="checkReport"
        >生成报告</el-button
      >
    </div>
    <closeDialog
      :dialogVisible="closeIndex"
      :reportName="getTitle()"
      @closeDialog="closeIndex = false"
      @dialogStoreRepote="storeReport"
    />
  </div>
  <!-- <noData v-else :listText="noDataText ? '请在任务列表中选择一条任务' : '该机组暂无数据'"></noData> -->
</template>

<script>
import { saveWindParkDiagReport } from '@/api/analysis/workPlatform.js'
import debounce from 'lodash/debounce'
import cloneDeep from 'lodash/cloneDeep'
import dayjs from 'dayjs'
import { eventTypeEnum, levelColorEnum } from '@/util/constant'
import { creatNormalTurbineReport } from '@/api/analysis/unitMerge.js'
import closeDialog from './closeReport.vue'
// import noData from '../../../diagnosisTool/noData/index.vue'

export default {
  components: {
    //   noData,
    closeDialog
  },
  data() {
    return {
      isShowExitReport: false,
      filterStatus: null,
      reportMonth: '',
      selectionTableList: [],
      tableData: [],
      eventTypeEnum,
      closeIndex: false, //关闭弹框变量
      activeName: '1',
      reportTime: new Date(), // dayjs(new Date()).format('YYYY年MM月'),
      basceList: {}, //详细信息Obj
      monitorTime: [],
      projectDesc: '', //项目描述
      // compTableList: [], //部件表格数据
      noDataText: true,
      loading: false,
      submitLoading: false,
      baseInfoKeys: {
        风场名称: 'name',
        机组数量: 'windTurbineCount',
        风机型号: 'windTurbineType',
        监测对象: 'detectionObject',
        监测方式: 'detectionMethod',
        风场地址: 'windParkAddress',
        投运时间: 'operationlDate',
        传动形式: 'transmissionForm',
        监测设备: 'detectionDevice',
        监测时间: 'detectionDate'
      }
    }
  },
  watch: {},
  methods: {
    filterTableData(val) {
      if (this.filterStatus == val) {
        this.filterStatus = null
        this.tableData = this.basceList.children
      } else {
        this.filterStatus = val
        let isDiagnosis = val == 0 ? true : false
        this.tableData = this.basceList.children.filter(item => item.isDiagnosis == isDiagnosis)
      }
    },
    checkSelectable(row) {
      return row.windturbingReportGuid !== null
    },
    getTitle() {
      return (
        '【众芯汉创】_【振动分析报告】_【' +
        dayjs(this.reportTime).format('YYYY年MM月') +
        this.basceList.name +
        '】'
      )
    },
    initData(data, timeArr) {
      this.isShowExitReport = true
      if (timeArr) {
        this.isShowExitReport = false
        this.monitorTime = timeArr
      }
      const { monitoringTimeEnd, monitoringTimeStart, ...rest } = data
      this.basceList = rest
      this.tableData = cloneDeep(rest.children)
      if (monitoringTimeStart) {
        this.monitorTime = [
          dayjs(monitoringTimeStart).format('YYYY-MM-DD'),
          dayjs(monitoringTimeEnd).format('YYYY-MM-DD')
        ]
      }
      this.$nextTick(() => {
        if (this.isShowExitReport) {
          this.tableData.forEach(row => {
            this.$refs.multipleTable.toggleRowSelection(row, true) // 设置所有行为选中状态
          })
        }
      })
    },
    getNormalTurbineDiag() {
      creatNormalTurbineReport({
        windParkId: this.windParkId,
        timeYYMM: this.currentMonth
      }).then(res => {
        if (res.data.success) {
          this.$message.success(res.data.msg)
        } else {
          this.$message.error(res.data.msg)
        }
      })
    },
    setBgColor({ row, column }) {
      if (column.property === 'status') {
        let color = levelColorEnum[row.status]
        return `background: ${color}`
      }
      if (column.property === 'compStatus') {
        let color = levelColorEnum[row.compStatus]
        return `background: ${color}`
      }
    },
    handleSelecRow(selection, row) {
      // 取消选中
      if (!selection.find(i => i.diagRecordGuid == row.diagRecordGuid)) {
        // 如果还存在选中的相同的报告ID，则取消选中
        let arr = selection.filter(item => item.windturbingReportGuid == row.windturbingReportGuid)
        arr.length
          ? arr.forEach(i => {
              this.$nextTick(() => {
                this.$refs.multipleTable.toggleRowSelection(i, false)
              })
            })
          : null
      }
    },
    // 表格勾选
    handleSelectionChange(selection) {
      this.selectionTableList = selection
    },
    spanMethod(row, rowIndex, key) {
      if (Array.isArray(key)) {
        if (
          rowIndex > 0 &&
          row[key[0]] === this.tableData[rowIndex - 1][key[0]] &&
          row[key[1]] === this.tableData[rowIndex - 1][key[1]]
        ) {
          return {
            rowspan: 0,
            colspan: 1
          }
        }
        let rowspan = 1
        for (let i = rowIndex + 1; i < this.tableData.length; i++) {
          if (
            this.tableData[i][key[0]] === row[key[0]] &&
            this.tableData[i][key[1]] === row[key[1]]
          ) {
            rowspan++
          } else {
            break
          }
        }
        return {
          rowspan: rowspan,
          colspan: 1
        }
      } else {
        if (rowIndex > 0 && row[key] === this.tableData[rowIndex - 1][key]) {
          return {
            rowspan: 0,
            colspan: 1
          }
        }
        let rowspan = 1
        for (let i = rowIndex + 1; i < this.tableData.length; i++) {
          if (this.tableData[i][key] === row[key]) {
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
    },
    // 表格行合并
    objectSpanMethod({ row, column, rowIndex, columnIndex }) {
      if (columnIndex === 0 || columnIndex === 2 || columnIndex === 3 || columnIndex === 8) {
        // 复选框
        if (row.windturbingReportGuid === null) {
          return this.spanMethod(row, rowIndex, 'windturbineId')
        } else {
          return this.spanMethod(row, rowIndex, 'windturbingReportGuid')
        }
      } else if (columnIndex === 1) {
        // 机组
        return this.spanMethod(row, rowIndex, 'windturbineId')
      } else if (columnIndex === 4 || columnIndex === 5) {
        // 部件名称
        if (row.windturbingReportGuid === null) {
          return this.spanMethod(row, rowIndex, ['compName', 'windturbineId'])
        } else {
          return this.spanMethod(row, rowIndex, ['compName', 'windturbingReportGuid'])
        }
      } else {
        return {
          rowspan: 1,
          colspan: 1
        }
      }
    },
    checkReport() {
      let reportTurbineIds = []
      let reportIds = []
      this.selectionTableList.forEach(i => {
        reportTurbineIds.push(i.windturbineId)
        reportIds.push(i.windturbingReportGuid)
      })
      if (!reportTurbineIds.length) {
        return alert('请选择需要生成报告的机组！')
      }
      if (new Set(reportTurbineIds).size !== new Set(reportIds).size) {
        return alert('每台机组只能选择一份诊断记录！')
      }
      this.closeIndex = true
    },
    //保存报告方法
    storeReport: debounce(function (note) {
      let reportIds = []
      this.selectionTableList.forEach(i => {
        if (i && i.windturbineId) {
          reportIds.push({
            WindTurbineReportGuid: i.windturbingReportGuid,
            WindTurbineId: i.windturbineId
          })
        }
      })
      this.submitLoading = true
      let param = {
        WindParkId: this.basceList.id, //风场ID
        WindParkName: this.basceList.name, //风场名称
        ReportMonth: this.reportTime, //报告月份
        MonitoringTimeStart: this.monitorTime[0], //检测时间开始
        MonitoringTimeEnd: this.monitorTime[1], //检测时间结束
        Remark: note, //备注
        ReportName: this.getTitle(),
        ProjectOverview: this.projectDesc, //项目概述
        WindParkDeviceRelations: Array.from(
          new Map(reportIds.map(item => [item.WindTurbineReportGuid, item])).values()
        ) // 去重
      }
      saveWindParkDiagReport(param).then(res => {
        const { message, success, data } = res.data
        if (success) {
          this.$message({
            message: message,
            type: 'success'
          })
          this.submitLoading = false
          this.$emit('initLeftTree', data)
        } else {
          this.$message({
            message: message,
            type: 'error'
          })
        }
      })
    }, 10),
    downloadReport(id) {
      let downloadElement = document.createElement('a')
      downloadElement.href = '/api/wtphm-service/diagnosticRecord/downloadDiagnoseReport?id=' + id
      let fileName = this.basceList.projectName + '.docx'
      downloadElement.download = fileName
      document.body.appendChild(downloadElement)
      downloadElement.click()
      document.body.removeChild(downloadElement)
      //点击下载
      /* downloadDiagnoseReportApi({ id }).then(result => {
        const headers = result.headers
        const contentType = headers['Content-Type']
        const blob = new Blob([result.data], { type: contentType })
        let fileName = this.basceList.projectName + '.docx' //this.rowClickId.name + '.docx' //文件名
        let downloadElement = document.createElement('a')
        let href = window.URL.createObjectURL(blob) //创建下载的链接
        downloadElement.href = href
        downloadElement.download = fileName
        document.body.appendChild(downloadElement)
        downloadElement.click() //点击下载
        document.body.removeChild(downloadElement) //下载完成移除元素
        window.URL.revokeObjectURL(href) //释放掉blob对象
      }) */
    }
  }
}
</script>

<style lang="less" scoped>
.right {
  height: 100%;
  .right_content {
    height: calc(100% - 50px);
    overflow-x: hidden;
    overflow-y: hidden;
    .detail_analysis {
      padding: 10px 20px;
      p {
        font-size: 16px;
        font-weight: bold;
        color: #000;
        margin-bottom: 10px;
      }
      .detail_analysis_header {
        width: 100%;
        height: 30px;
        h3 {
          display: inline-block;
          font-size: 16px;
          font-weight: bold;
          color: #000;
          margin-bottom: 10px;
        }
        .detail_analysis_header_right {
          float: right;

          ::v-deep .el-date-editor {
            width: 150px;
            height: 28px;
            line-height: 28px;
            .el-input__inner {
              height: 100%;
              line-height: 100%;
            }
            .el-input__prefix {
              display: none;
            }
          }
          p {
            float: left;
            font-size: 14px;
            font-weight: normal;
            margin-right: 15px;
            line-height: 30px;
          }
          .el-button {
            margin-left: 10px;
            color: #000;
            background: transparent;
            border-color: #eee;
            padding: 6px 10px;
          }
          .active {
            background: #2e8ded;
          }
          b {
            font-weight: bolder;
            color: #2e8ded;
            margin: 0 5px;
          }
        }
      }
      .detail_analysis_content {
        margin-top: 10px;
      }
    }
    .other_report {
      height: auto;
      .unit_param {
        width: auto;
        font-size: 14px;
        color: #000;
        float: left;
        margin: 0 20px 10px 10px;
        min-width: 30%;
        max-width: 50%;
        overflow: hidden;
        text-overflow: ellipsis;
        white-space: nowrap;
        span {
          display: inline-block;
          width: 70px;
          text-align: right;
          margin-right: 10px;
        }
        b {
          font-weight: bolder;
        }
      }
    }
  }
  ::v-deep .el-date-editor {
    height: 28px;
    line-height: 28px;
    width: 230px;
    .el-range-separator {
      color: #303133;
      padding: 0;
    }
    .el-range-input {
      width: 45%;
    }
    .el-range__icon {
      display: none;
    }
    .el-range__close-icon {
      display: none;
    }
  }
  .report_information {
    ::v-deep .el-collapse {
      .el-collapse-item {
        margin: 0px 20px 0 20px;
        border-bottom: 1px solid rgba(216, 216, 216, 0.2);
        .el-collapse-item__header {
          // background-color: #252526;
          border: none;
          color: #000;
          font-size: 16px;
          font-weight: bold;
          height: 60px;
          line-height: 60px;
        }
        .el-collapse-item__content {
          //background-color: #252526;
          color: #606266;
          /*  .el-input {
            position: relative;
            font-size: 14px;
            display: inline-block;
            width: 180px; //220px;
            // background: #1d1c1c;
            height: 40px;
            border-radius: 5px;
            color: #606266;
            .el-input__inner {
              height: 40px;
              // cursor: default !important;
              &:hover {
                background-color: transparent;
              }
            }
          } */
        }
      }
    }
    .input_report {
      .input_title {
        color: #000;
        font-size: 14px;
        float: left;
      }
      ::v-deep .el-textarea {
        width: calc(100% - 80px) !important;
        float: left;
        .el-textarea__inner {
          // border-color: transparent;
          background: transparent;
          border-radius: 4px;
          line-height: 30px;
          padding: 8px 18px;
        }
      }
    }
  }
}
.right_bottom {
  height: 50px;
  width: 100%;
  // background-color: #252526;
  margin: 0px 0 20px 0;
  .el-button {
    padding: 0;
    left: calc(50% - 40px);
    position: relative;
    height: 35px;
    width: 88px;
    background: #2e8ded;
    margin-right: 40px;
    margin-top: 10px;
    border: none;
    &:hover {
      background: #0126bc;
    }
  }
}
.el-table__row {
  .textline {
    width: 100%;
    text-align: left;
    font-weight: normal;
    font-size: 14px;
    color: #606266;
    white-space: normal !important;
    word-break: break-all !important;
    height: auto;
  }
}
</style>
