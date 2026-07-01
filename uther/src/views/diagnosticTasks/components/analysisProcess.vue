<template>
  <div class="analysis_process">
    <h3>分析过程记录</h3>
    <div class="analysis_content">
      <nested-collapse ref="nestedCollapse" :items.sync="processRecordData" />
    </div>
  </div>
</template>

<script>
import { getManualDiagnosisListTreeByWindturbineId } from '@/api/analysis/index'
import dayjs from 'dayjs'
import { v4 as uuidv4 } from 'uuid'
import uniqBy from 'lodash/uniqBy'
import cloneDeep from 'lodash/cloneDeep'
import isPlainObject from 'lodash/isPlainObject'

const defaultDeviceState = [
  {
    label: '正常',
    value: 'normal'
  },
  {
    label: '注意',
    value: 'attention'
  },
  {
    label: '警告',
    value: 'warning'
  },
  {
    label: '危险',
    value: 'danger'
  }
]
let copyTreeData = []
let submitLoadOn = false

export default {
  components: {
    NestedCollapse: () => import('./nestedCollapse.vue'),
    titleMeasion: () => import('./titleMeasion.vue')
  },
  props: {
    IESummaryconclusionData: Object,
    diagnosticRecordReportVo: null,
    windturbineStateTemp: String,
    diagnosticRecordVo: Array
  },
  data() {
    return {
      // 部件状态存储
      isRecomputer: true,
      tableData: [1],
      ruleForm: {
        conclusion: '',
        advice: ''
      },
      filterText: '',
      treeData: [],
      queryTreeData: [],
      windturbineState: 'normal',
      processRecordData: [],
      deviceHealthOpt: defaultDeviceState,
      diagnosticRecordReportData: [],
      defaultCheckedKeys: [],
      changeFaultTextColor: {
        normal: '#2ED133',
        attention: '#FFE604',
        warning: '#FF6B0E',
        danger: '#FF0F0D'
      },
      activatBtnType: 'oneMonth',
      levelNum: {
        normal: 1,
        attention: 2,
        warning: 3,
        danger: 4
      }
    }
  },
  /*created() {
    this.getHistoryTreeData()
  },*/
  mounted() {
    this.$bus.$on('processRecordSubmitLoad', params => {
      submitLoadOn = true
      this.processRecordData = params
    })
    // 初始机组状态
    let color = this.changeFaultTextColor[this.windturbineState]
    let dom = document.getElementById('faultLevelInput')
    dom?.style.setProperty('--inputTextColor', color)
  },
  watch: {
    filterText(val) {
      this.$refs.tree.filter(val)
    },
    processRecordData: {
      handler: function (val) {
        let diagnosticRecordReportData = this.extractChildren(val)
        this.diagnosticRecordReportData = diagnosticRecordReportData.map(item => {
          return {
            ...item,
            $cellEdit: true
          }
        })
        if (submitLoadOn) {
          let diagnosticRecordReportDataTemp = this.diagnosticRecordReportData.sort(
            (a, b) => new Date(b.acqTime) - new Date(a.acqTime)
          )
          if (diagnosticRecordReportDataTemp.length > 0) {
            this.$bus.$emit('acqTimeReport', diagnosticRecordReportDataTemp.at(-1).acqTime)
          }
        }
        submitLoadOn = false
      },
      deep: true
    },
    IESummaryconclusionData: {
      handler: function (val) {
        if (!val?.id) {
          return
        }
        // this.getHistoryTreeData()
        let hasReportList = JSON.parse(sessionStorage.getItem('windturbineIdHasReportList'))
        if (hasReportList == 1) {
          return
        }
        // this.windturbineState = val.status
      },
      deep: true,
      immediate: true
    },
    diagnosticRecordReportVo: {
      handler: function (val) {
        // if (!val?.length) {
        //   return
        // }
        this.processRecordData = val
      }
    },
    windturbineStateTemp: {
      handler: function (val) {
        console.log('turbinestate', val)
        if (!val) {
          return
        }
        this.windturbineState = val
      }
    },
    diagnosticRecordVo: {
      handler: function (val) {
        if (!val?.length) {
          return (this.defaultCheckedKeys = [])
        }
        this.handleMarkerTree()
      },
      deep: true,
      immediate: true
    },
    treeData: {
      handler: function (val) {
        if (!val?.length) {
          return (this.defaultCheckedKeys = [])
        }
        this.handleMarkerTree()
      },
      deep: true,
      immediate: true
    },
    windturbineState: {
      handler: function (val) {
        let color = this.changeFaultTextColor[val]
        let dom = document.getElementById('faultLevelInput')
        dom?.style.setProperty('--inputTextColor', color)
      },
      immediate: true
    },
    diagnosticRecordReportData: {
      handler: function (val) {
        if (!val?.length) {
          this.windturbineState = 'normal'
          // this.$bus.$emit('acqTimeList', [])
          return
        }
        if (this.isRecomputer) {
          // 取最高状态
          let compList = {}
          this.clearFormData()
          val.map(item => {
            const { compName, faultLevel } = item
            if (compList[compName]) {
              if (this.levelNum[compList[compName]] < this.levelNum[faultLevel]) {
                compList[compName] = faultLevel
                this.ruleForm[compName] = faultLevel
              }
            } else {
              compList[compName] = faultLevel
              this.ruleForm[compName] = faultLevel
            }
          })
          // 自行拼接所有诊断记录中的维修建议和诊断结论
          let conclusionSummary = ''
          let adviceSummary = ''
          let num1 = 1
          let num2 = 1
          val.forEach(item => {
            const { conclusion, advice } = item
            if (conclusion.length) {
              conclusionSummary += num1 + '. ' + conclusion + ';\n'
              num1++
            }
            if (advice.length) {
              adviceSummary += num2 + '. ' + advice + ';\n'
              num2++
            }
          })
          this.ruleForm.conclusion = conclusionSummary.slice(0, -2)
          this.ruleForm.advice = adviceSummary.slice(0, -2)
          // ====完成
          let faultLevelTempL = val.reduce((acc, curr) => {
            acc.push(curr.faultLevel)
            return acc
          }, [])
          this.handleDefaultChangeFaultLevel(faultLevelTempL)
        }
        this.isRecomputer = true
      },
      deep: true
      // immediate: true
    }
  },
  methods: {
    clearFormData() {
      this.ruleForm = {
        conclusion: '',
        advice: ''
      }
    },
    // 已有报告的机组初始化状态
    initTurbineStatus(data) {
      this.isRecomputer = false
      this.clearFormData()
      const {
        recordIdAndMeasDataParse,
        recordIdAndMeasData,
        maintenanceAdvice,
        diagnosisTime,
        analyzerConclusion,
        windturbineState,
        windturbineId,
        windturbineName
      } = data
      this.ruleForm.conclusion = analyzerConclusion.replaceAll('\\n', '\n')
      this.ruleForm.advice = maintenanceAdvice.replaceAll('\\n', '\n')
      this.windturbineState = windturbineState
      const { entityStatusList } = recordIdAndMeasDataParse
      if (entityStatusList.length) {
        entityStatusList.forEach(item => {
          this.ruleForm[item.compName] = item.status
        })
      }
      console.log(this.ruleForm)
    },
    filterNode(value, data) {
      if (!value) return true
      return data.label.indexOf(value) !== -1
    },
    getHistoryTreeData(params) {
      if (!params) {
        params = {
          windturbineId: this.IESummaryconclusionData.id,
          endTime: dayjs().format('YYYY-MM-DD HH:mm:ss'),
          startTime: dayjs().subtract(1, 'month').format('YYYY-MM-DD HH:mm:ss'),
          isShowImages: true
        }
      }
      getManualDiagnosisListTreeByWindturbineId(params).then(result => {
        this.treeData = result.data?.data?.length > 0 ? this.updateLabel(result.data.data) : []
        copyTreeData = cloneDeep(this.treeData)
      })
    },
    updateLabel(obj) {
      if (Array.isArray(obj)) {
        obj.forEach(item => this.updateLabel(item))
      } else if (typeof obj === 'object') {
        const keysToCheck = [
          'subComp',
          'conclusion',
          'measlocName',
          'recordTime',
          'diagnosticRecordGroupByTimeList'
        ]
        const idKey = 'id'
        const shouldUpdate = key => obj.hasOwnProperty(key) && !obj.hasOwnProperty(idKey)
        keysToCheck.forEach(key => {
          if (shouldUpdate(key)) {
            if (key === 'diagnosticRecordGroupByTimeList') {
              obj.diagnosticRecordReportByList = obj[key]
            } else {
              obj.label = obj[key]
              obj.value = uuidv4()
            }
            if (key === 'measlocName') {
              obj.children = obj.diagnosticRecordReportByList
              obj.type = 'measloc'
              delete obj.diagnosticRecordReportByList
            }
            if (key === 'conclusion') {
              obj.type = 'conclusion'
            }
            delete obj[key]
          }
        })
        Object.values(obj).forEach(value => this.updateLabel(value))
      }
      return obj
    },
    /*  handleSubmitLoad() {
      console.log('vue载入信息')
      let searchvalues = []
      this.$refs.tree.getCheckedNodes(true).forEach(item => searchvalues.push(item.value))
      this.queryTreeData = this.fuzzyQueryTree(this.treeData, searchvalues)
      this.processRecordData = this.mergeData(cloneDeep(this.queryTreeData))
    }, */
    fuzzyQueryTree(arr, values) {
      if (!Array.isArray(arr) || arr.length === 0) {
        return []
      }
      if (!Array.isArray(values)) {
        values = [values]
      }
      let result = []
      arr.forEach(item => {
        if (values.includes(item.value)) {
          const diagnosticRecordReportByList = this.fuzzyQueryTree(
            item.diagnosticRecordReportByList,
            values
          )
          const obj = { ...item, diagnosticRecordReportByList }
          result.push(obj)
        } else {
          if (item.diagnosticRecordReportByList && item.diagnosticRecordReportByList.length > 0) {
            const diagnosticRecordReportByList = this.fuzzyQueryTree(
              item.diagnosticRecordReportByList,
              values
            )
            const obj = { ...item, diagnosticRecordReportByList }
            if (diagnosticRecordReportByList && diagnosticRecordReportByList.length > 0) {
              result.push(obj)
            }
          }
        }
      })
      return result
    },
    mergeData(input) {
      const output = []
      function mergeDiagnosticRecords(source, target) {
        for (const sourceRecord of source) {
          let targetRecord = target.find(item => item.label === sourceRecord.label)
          if (!targetRecord) {
            targetRecord = { ...sourceRecord }
            target.push(targetRecord)
          }
          let childrenFindMeasloc = target.find(item => item.type === 'measloc')
          if (childrenFindMeasloc?.children && sourceRecord?.children) {
            childrenFindMeasloc.children.push(...sourceRecord.children)
            childrenFindMeasloc.children = uniqBy(childrenFindMeasloc.children, 'id')
          }
          if (
            sourceRecord.diagnosticRecordReportByList &&
            sourceRecord.diagnosticRecordReportByList.length > 0
          ) {
            mergeDiagnosticRecords(
              sourceRecord.diagnosticRecordReportByList,
              targetRecord.diagnosticRecordReportByList
            )
          }
        }
      }
      for (const inputData of input) {
        mergeDiagnosticRecords(inputData.diagnosticRecordReportByList, output)
      }
      return output
    },
    extractChildren(data) {
      let result = []
      function traverse(node) {
        if (node.children) {
          result.push(...node.children)
          node.children.forEach(child => traverse(child))
        }
        if (node.diagnosticRecordReportByList) {
          node.diagnosticRecordReportByList.forEach(child => traverse(child))
        }
      }
      data.forEach(item => traverse(item))
      return result
    },
    crudList() {
      return this.diagnosticRecordReportData
      // return this.$refs.crud.list
    },
    handleChangeTreeData(type) {
      this.activatBtnType = type
      if (type === 'oneMonth') {
        this.treeData = cloneDeep(copyTreeData)
      } else if (type === 'new') {
        this.treeData = [
          cloneDeep(copyTreeData)
            .sort((a, b) => new Date(b.label) - new Date(a.label))
            .at(0)
        ]
      }
    },
    handleTestData() {
      // console.log(this.$refs.nestedCollapse)
      // console.log(this.processRecordData, 'processRecordData')
      console.log(this.treeData, 'this.treeData')
      console.log(this.findValueById(this.treeData, 18), 'findValueById')
    },
    findValueById(jsonObj, targetId) {
      let result = null
      let found = false
      function traverse(obj) {
        if (found) return
        if (Array.isArray(obj)) {
          for (let i = 0; i < obj.length; i++) {
            traverse(obj[i])
          }
        } else if (isPlainObject(obj)) {
          if (obj.hasOwnProperty('children') && obj?.children.length > 0) {
            let findTargetId = obj.children.find(item => item.id === targetId)
            if (findTargetId) {
              result = obj
              found = true
              return
            } else {
              traverse(obj.children)
            }
          } else if (
            obj.hasOwnProperty('diagnosticRecordReportByList') &&
            obj?.diagnosticRecordReportByList.length > 0
          ) {
            traverse(obj.diagnosticRecordReportByList)
          }
        }
      }
      traverse(jsonObj)
      return result
    },
    handleMarkerTree() {
      if (!this.diagnosticRecordVo?.length) {
        return (this.defaultCheckedKeys = [])
      }
      if (!this.treeData?.length) {
        return (this.defaultCheckedKeys = [])
      }
      this.defaultCheckedKeys = []
      // this.$refs.tree.setCheckedKeys([])
      let defaultCheckedKeysTemp = []
      this.diagnosticRecordVo.forEach(item => {
        const defaultCheckedKeysTempValue = this.findValueById(this.treeData, item.id)
        defaultCheckedKeysTemp.push(defaultCheckedKeysTempValue.value)
      })
      this.defaultCheckedKeys = defaultCheckedKeysTemp
    },
    handleChangeFaultLevel(value, val) {
      let stateslIst = []
      Object.keys(this.ruleForm).forEach(item => {
        if (item !== 'advice' && item !== 'conclusion') {
          stateslIst.push(this.ruleForm[item])
        }
      })
      this.handleDefaultChangeFaultLevel(stateslIst)
      this.$set(this.tableData, 0, { id: stateslIst.join('') })
    },
    handleDefaultChangeFaultLevel(elements) {
      const weights = {
        danger: 4,
        attention: 2,
        warning: 3,
        normal: 1
      }
      const highestWeightElement = elements.reduce((highestElement, currentElement) => {
        if (weights[currentElement] > weights[highestElement]) {
          return currentElement
        }
        return highestElement
      })
      console.log(highestWeightElement)
      this.windturbineState = highestWeightElement
    },
    cellStyle({ row, column, rowIndex, columnIndex }) {
      if (rowIndex == 1 && columnIndex != 0) {
        return {
          color: this.changeFaultTextColor[row.faultLevel]
        }
      }
    }
  }
}
</script>

<style lang="less" scoped>
@import url('../styles/common.less');
.analysis_process {
  width: 100%;
  height: 100%;
  overflow: hidden;
  padding: 10px 15px;
  h3 {
    width: 100%;
    font-size: 14px;
  }
  ::v-deep .el-form-item {
    margin-bottom: 0;
    .el-textarea {
      background: #fff;
    }
  }
  ::v-deep .el-input__inner {
    color: inherit;
  }
  .history_tree {
    width: 301px;
    padding: 7px;
    background: #323232;
    ::v-deep .el-input {
      width: 100%;
      height: 28px;
      border-radius: 5px;
      opacity: 1;
      background: #1d1c1c;
      margin-bottom: 10px;
      .el-input__inner {
        height: 28px;
      }
      .el-input__prefix {
        height: 28px;
        top: -5px;
      }
    }
    .btn_combination {
      display: flex;
      flex-direction: row;
      width: 100%;
      justify-content: space-between;
    }
    ::v-deep .el-tree {
      .el-tree-node__expand-icon {
        font-size: 20px;
      }
      .el-checkbox__inner {
        background: #d8d8d8;
        border: 1px solid #d8d8d8;
      }
      .el-checkbox__input.is-checked .el-checkbox__inner,
      .el-checkbox__input.is-indeterminate .el-checkbox__inner {
        background-color: #409eff !important;
        border-color: #409eff !important;
      }
    }
    .lod_btn {
      width: 100%;
      text-align: center;
      margin-top: 36px;
      margin-bottom: 8px;
    }
  }
  .analysis_content {
    flex: 1;
    background: #f6f6f6;
    margin-left: 10px;
    height: calc(100% - 25px);
    overflow-x: hidden;
    overflow-y: auto;
    // padding: 7px 48px 7px 23px;
    .dividing_line {
      width: 100%;
      margin-bottom: 7px;
    }
    .label_span_title {
      margin-right: 8px;
      margin-left: 10px;
    }
    .device_state_select {
      --inputTextColor: '#C0C4CC';
      ::v-deep .el-input__inner {
        // background: black;
        border: 1px solid #bebebe;
        color: var(--inputTextColor) !important;
      }
      ::v-deep .el-input__suffix-inner {
        display: none;
      }
    }

    .avue_crud_st {
      width: 1330px;
      ::v-deep .avue-crud__menu {
        min-height: 11px;
      }
      ::v-deep .el-table::before {
        background-color: transparent;
      }
      ::v-deep .inputColor {
        .el-input__inner {
          color: inherit;
        }
      }
    }
  }
}
</style>
