import { getManualDiagnosisListTreeByWindturbineId } from '@/api/analysis/index'
// import dayjs from 'dayjs'
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
    label: '警告',
    value: 'warning'
  },
  {
    label: '注意',
    value: 'attention'
  },
  {
    label: '危险',
    value: 'danger'
  }
]

export default {
  props: {
    IESummaryconclusionData: Object,
    diagnosticRecordReportVo: null,
    windturbineStateTemp: String,
    diagnosticRecordVo: Array
  },
  data() {
    return {
      filterText: '',
      treeData: [],
      defaultProps: {
        children: 'diagnosticRecordReportByList',
        label: 'label'
      },
      queryTreeData: [],
      windturbineState: 'normal',
      processRecordData: [],
      deviceHealthOpt: defaultDeviceState,
      diagnosticRecordReportData: [],
      processRecordOpt: {
        addBtn: false,
        menu: false,
        addRowBtn: false,
        cellBtn: true,
        column: [
          {
            label: '部件',
            prop: 'compName',
            cell: false,
            formatter: val => {
              return val.compName.split(',').join('_')
            }
          },
          {
            label: '健康状态',
            prop: 'faultLevel',
            slot: true
          },
          {
            label: '诊断结论',
            prop: 'conclusion',
            slot: true,
            cell: true
          },
          {
            label: '维护建议',
            prop: 'advice',
            slot: true,
            cell: true
          }
        ]
      },
      defaultCheckedKeys: [],
      changeFaultTextColor: {
        normal: '#2ED133',
        attention: '#FFE604',
        warning: '#FF6B0E',
        danger: '#FF0F0D'
      },
      defaultDeviceStateTemp: [
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
      ],
      activatBtnType: 'oneMonth',
      treeDataLoading: false
    }
  },
  /*created() {
    this.getHistoryTreeData()
  },*/
  mounted() {
    this.$bus.$on('clearParameter', () => {
      this.treeData = []
      this.processRecordData = []
      this.$bus.$emit('processRecordSubmitLoad', this.processRecordData)
    })
  },
  watch: {
    filterText(val) {
      this.$refs.tree.filter(val)
    },
    /*processRecordData: {
      handler: function (val) {
        let diagnosticRecordReportData = this.extractChildren(val)
        this.diagnosticRecordReportData = diagnosticRecordReportData.map(item => {
          return {
            ...item,
            $cellEdit: true
          }
        })
        console.log(this.diagnosticRecordReportData, 'this.diagnosticRecordReportData')
      },
      deep: true
    },*/
    IESummaryconclusionData: {
      handler: function (val) {
        if (!val?.id) {
          return
        }
        this.getHistoryTreeData()
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
          return (this.windturbineState = 'normal')
        }
        let faultLevelTempL = val.reduce((acc, curr) => {
          acc.push(curr.faultLevel)
          return acc
        }, [])
        this.handleDefaultChangeFaultLevel(faultLevelTempL)
      },
      deep: true,
      immediate: true
    }
  },
  methods: {
    filterNode(value, data) {
      if (!value) return true
      return data.label.indexOf(value) !== -1
    },
    getHistoryTreeData(params) {
      this.treeDataLoading = true
      if (!params) {
        params = {
          windturbineId: this.IESummaryconclusionData.id,
          endTime: this.analysisRecordsTime[1] + ' 23:59:59',
          startTime: this.analysisRecordsTime[0] + ' 00:00:00',
          isShowImages: true
        }
      }
      getManualDiagnosisListTreeByWindturbineId(params).then(result => {
        let treeData = result.data?.data?.length > 0 ? this.updateLabel(result.data.data) : []
        if (treeData.length > 0) {
          treeData.sort(
            (a, b) => new Date(b.label) - new Date(a.label)
          )
        }
        this.treeData = treeData
      }).finally(() => {
        this.treeDataLoading = false
      })
    },
    updateLabel(obj) {
      if (Array.isArray(obj)) {
        obj.forEach(item => this.updateLabel(item))
      } else if (typeof obj === 'object') {
        const keysToCheck = [
          'comp',
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
    handleSubmitLoad() {
      let searchvalues = []
      this.$refs.tree.getCheckedNodes(true).forEach(item => searchvalues.push(item.value))
      this.queryTreeData = this.fuzzyQueryTree(this.treeData, searchvalues)
      this.processRecordData = this.mergeData(cloneDeep(this.queryTreeData))
      this.$bus.$emit('processRecordSubmitLoad', this.processRecordData)
    },
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
          let childrenFindMeasloc = target.find(item => item.label === sourceRecord.label && item.type === 'measloc')
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
      return this.$refs.crud.list
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
        defaultCheckedKeysTempValue && defaultCheckedKeysTemp.push(defaultCheckedKeysTempValue.value)
      })
      this.defaultCheckedKeys = defaultCheckedKeysTemp
    },
    handleChangeFaultLevel(value) {
      if (value === 'danger') {
        this.windturbineState = 'danger'
      } else if (value === 'warning') {
        this.windturbineState = 'warning'
      } else if (value === 'attention') {
        this.windturbineState = 'attention'
      } else if (value === 'normal') {
        this.windturbineState = 'normal'
      }
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
      this.windturbineState = highestWeightElement
    },
    cellStyle({ row, columnIndex }) {
      if (columnIndex == 1) {
        return {
          color: this.changeFaultTextColor[row.faultLevel]
        }
      }
    }
  }
}