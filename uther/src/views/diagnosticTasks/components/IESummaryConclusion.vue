<template>
  <div class="IES">
    <div class="IES_tree_content">
      <IESTreeControl
        ref="IESTreeControl"
        @handleDialogVisible="handleDialogVisible"
        @clickedNode="clickedNode"
        @changeCheckedRecord="changeCheckedRecord"
      />
    </div>
    <el-main
      v-loading="mainLoading"
      element-loading-background="rgba(0, 0, 0, 0.5)"
      element-loading-text="报告正在加载中"
      class="IES_content"
    >
      <!--  <template v-if=""> -->

      <div class="body_content" v-show="pageContent == 'turbine'">
        <parametersMea ref="parametersMea" />
        <div class="body_content_down">
          <analysisProcess ref="analysisProcess" style="width: 63%" />
          <turbine-conclusion ref="turbineConclusion" style="width: 37%" />
        </div>
        <el-button
          title="新增报告"
          type="primary"
          class="btn_blue_comp save_btn"
          size="small"
          icon="el-icon-plus"
          style="width: 96px; margin-right: 20px"
          :loading="submitLoading"
          @click="handleSaveReport"
          >保存</el-button
        >
      </div>
      <!--   </template> -->
      <windparkReport
        v-show="pageContent == 'windpark'"
        ref="windparkReport"
        @initLeftTree="updateLeftWindparkTree"
      ></windparkReport>
      <div
        v-show="pageContent == ''"
        style="color: #999; text-align: left; padding: 20px; line-height: 40px"
      >
        <p>
          首先，在左上角选择要做报告的风场！<br />
          然后，在左侧设备树选择要查看的数据！<br />
          其中点击风场，主视图区域会默认显示风场报告，根据需求，勾选机组列表选项，进行汇总生成风场报告。点击左下角“风场历史报告”的树节点，可以查看对应的报告。<br />
          点击机组，主视图区域会默认显示机组报告，用户可以操作左下角“机组分析记录”，进行诊断分析记录的增删修改等。<br />
          点击机组日期，可以查看历史报告。
        </p>
      </div>
    </el-main>
  </div>
</template>

<script>
import {
  getTurbineReportDetail, // 获取机组报告详情
  saveTurbineReportDetail, // 保存机组报告
  getTurbineConclusion, // 获取机组诊断结论
  getWindParkReportDetail, // 获取风场新增报告详情
  getWindParkExitReportDetail // 获取风场已有报告的详情
} from '@/api/analysis/workPlatform'
import dayjs from 'dayjs'
import windparkReport from './windparkReport/index.vue'
import turbineConclusion from './turbineConclusion.vue'

export default {
  components: {
    parametersMea: () => import('./parametersMea.vue'),
    analysisProcess: () => import('./nestedCollapse.vue'),
    IESTreeControl: () => import('./IESTreeControl.vue'),
    windparkReport,
    turbineConclusion
  },
  props: {
    IESummaryconclusionData: Object
  },
  data() {
    return {
      datePickerOpt: [],
      diagnosisTime: dayjs(new Date()).format('YYYY-MM-DD'), //null,
      pageContent: '',
      windparkList: [],
      IESummaryconclusionDataFinal: {},
      mainLoading: false,
      submitLoading: false
    }
  },
  watch: {},
  mounted() {},
  methods: {
    // 左侧设备树修改勾选项
    changeCheckedRecord(ids) {
      this.$refs.analysisProcess.changeDataSourse(ids)
    },
    clickedNode(data) {
      this.mainLoading = true
      const { clicked, deviceId, analysisData, turbineInfo } = data
      if (clicked == 'turbine') {
        this.pageContent = 'turbine'
        getTurbineConclusion({
          windturbineId: deviceId
        })
          .then(res => {
            if (res.data.success) {
              this.$nextTick(() => {
                this.$refs.turbineConclusion.initData(res.data.data)
              })
            } else {
              this.$message.error(res.data.message)
            }
          })
          .finally(() => {
            this.mainLoading = false
          })
        setTimeout(() => {
          this.$refs.analysisProcess.initData({
            analyzerRecords: analysisData.id == null ? [] : this.flattenArray(analysisData)
          })
          this.$refs.parametersMea.initData({
            windTurbine: {
              ...turbineInfo,
              sampleDataSpeed: Math.max(...this.extractProperty(analysisData, 'sampleDataSpeed'))
            }
          })
        }, 100)
      } else if (clicked === 'windpark') {
        this.pageContent = 'windpark'
        let timeArr = this.$refs.IESTreeControl.monthPickerValue
        getWindParkReportDetail({
          windParkID: deviceId,
          startTime: timeArr[0],
          endTime: timeArr[1]
        })
          .then(res => {
            if (res.data.success) {
              this.$refs.windparkReport.initData(res.data.data, timeArr)
            } else {
              this.$message.error(res.data.message)
            }
          })
          .finally(() => {
            this.mainLoading = false
          })
      } else if (clicked === 'report') {
        this.pageContent = 'turbine'
        getTurbineReportDetail({ reportGuid: deviceId })
          .then(res => {
            if (res.data.success) {
              const { analyzerRecords, conclusions, status, runingAdvice, ...others } =
                res.data.data
              this.windTurbineInfo = others
              this.$refs.analysisProcess.initData({
                analyzerRecords
              })
              this.$refs.parametersMea.initData({
                ...others
              })
              this.$refs.turbineConclusion.initData({
                conclusions,
                status,
                runingAdvice
              })
            } else {
              this.$message.error(res.data.message)
            }
          })
          .finally(() => {
            this.mainLoading = false
          })
      } else if (clicked === 'windparkReport') {
        this.pageContent = 'windpark'
        // 获取风场报告详情接口
        getWindParkExitReportDetail({ reportGuid: deviceId })
          .then(res => {
            if (res.data.success) {
              this.$refs.windparkReport.initData(res.data.data)
            } else {
              this.$message.error(res.data.message)
            }
          })
          .finally(() => {
            this.mainLoading = false
          })
      }
    },
    // 工具函数
    // 获取树结构最底层属性值的数组
    extractProperty(obj, property) {
      let values = []
      // 检查对象是否有该属性
      if (property in obj) {
        values.push(obj[property])
      }
      // 遍历下一层
      if (obj.children) {
        obj.children.forEach(item => {
          values = values.concat(this.extractProperty(item, property))
        })
      }
      return values
    },
    // 多层嵌套的数组，进行平铺变成一层对象数组
    flattenArray(obj) {
      let flatArray = []
      let compName, measlocName
      obj.children.forEach(item => {
        compName = item.name
        if (item.children) {
          item.children.forEach(ii => {
            measlocName = ii.name
            let arr = this.getObj(ii.children)
            if (arr.length) {
              arr.forEach(iii => {
                flatArray.push({
                  ...iii,
                  compName,
                  measlocName,
                  recordTime: iii.name,
                  analyzerRecordId: iii.id
                })
              })
            }
          })
        }
      })
      return flatArray
    },
    // 递归返回最底层
    getObj(arr) {
      let result = []
      arr.forEach(item => {
        if (item.children) {
          result = result.concat(this.getObj(item.children))
        } else {
          result.push({ ...item })
        }
      })
      return result
    },

    // 保存机组报告，刷新左侧树
    handleSaveReport() {
      this.submitLoading = true
      let params = {
        windturbineId: '',
        RuningAdvice: '',
        Status: '',
        SampleDataSpeed: 0,
        AnalyzerRecords: [],
        Conclusions: []
      }
      // 机组信息
      let { id, windTurbineID, sampleDataSpeed } =
        this.$refs.parametersMea.windturbineInfoOriginal.windTurbine
      params.windturbineId = id || windTurbineID
      params.SampleDataSpeed = sampleDataSpeed
      // 分析记录
      Object.keys(this.$refs.analysisProcess.formData).forEach(key => {
        params.AnalyzerRecords.push({
          Description: this.$refs.analysisProcess.formData[key],
          AnalyzerRecordId: key
        })
      })
      if (params.AnalyzerRecords.length == 0) {
        this.submitLoading = false
        return this.$message.error('请先添加分析记录！')
      }
      // 诊断结论
      const { runingAdvice, turbinestate, ...others } = this.$refs.turbineConclusion.ruleForm
      params.RuningAdvice = runingAdvice
      params.Status = turbinestate
      Object.keys(others).forEach(key => {
        if (!key.includes('Level')) {
          others[key].forEach(item => {
            params.Conclusions.push({
              ...item,
              CompName: key
            })
          })
        }
      })
      saveTurbineReportDetail(params)
        .then(res => {
          if (res.data.success) {
            this.$message.success('保存成功！')
            // 刷新左侧树
            this.$refs.IESTreeControl.getWindParkCurrentTree(res.data.data)
            // this.clickedNode({ clicked: 'report', deviceId: res.data.data })
          } else {
            this.$message.error('保存失败！')
          }
        })
        .finally(() => {
          this.submitLoading = false
        })
    },
    updateLeftWindparkTree(id) {
      this.$refs.IESTreeControl.getWindParkCurrentTree()
      this.$refs.IESTreeControl.getWindparkHistoryTree(id)
      //  this.clickedNode({ clicked: 'windparkReport', deviceId: id })
    }
  }
}
</script>

<style lang="less" scoped>
@import url('../styles/common.less');
.IES {
  display: flex;
  flex-direction: row;
  width: 100%;
  height: 100%;
  .IES_tree_content {
    width: 350px !important;
    // background: #252526;
    display: flex;
    flex-direction: column;
    position: fixed;
    height: 100%;
    margin-right: 3px;
  }
  .IES_content {
    display: flex;
    flex-direction: column;
    // background-color: #1a1c20;
    flex: 1;
    width: 0%;
    margin-left: 353px;
    height: 100%;
    overflow: hidden;
    .save_btn {
      position: absolute;
      bottom: 10px;
      right: 13%;
    }
    .body_content {
      flex: 1;
      overflow-y: hidden;
      .body_content_down {
        width: 100%;
        height: calc(100% - 40px);
        display: flex;
        flex-direction: row;
        justify-content: left;
        align-items: flex-start;
      }
    }
  }
}

.demo-ruleForm {
  padding: 8px 8px 0px 0px;
  ::v-deep .el-input__inner {
    // background: rgba(39, 39, 39, 0.5);
  }
}
</style>
