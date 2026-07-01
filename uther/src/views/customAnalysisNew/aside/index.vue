/* 新接口 */
<template>
  <div class="aside-content">
    <!--  <h4>设备树</h4> -->
    <div class="tree_content">
      <div class="tree_tool">
        <el-input
          placeholder="输入关键字进行过滤"
          v-model="filterText"
          @keyup.enter="searchTree"
        >
        </el-input>
        <i class="el-icon-search" title="搜索" style="margin: 0 5px" @click="searchTree"></i>
        <i class="el-icon-refresh" @click="refreshTree" title="重置" alt="pic" />
      </div>
      <div class="tree_div">
        <el-tree
          :data="treeData"
          show-checkbox
          :props="defaultProps"
          node-key="id"
          :filter-node-method="filterNode"
          highlight-current
          ref="tree"
          indent="8"
          :default-checked-keys="currentNodeId"
          @node-expand="handleNodeExpand"
          :default-expanded-keys="defaultExpandedNodes"
        >
          <template #default="{ node, data }">
            <p :class="['custom-tree-node']">
            <b class="inlineBlock" style="margin-right: 4px">
              <i :class="['icon', 'local', getClass(data)]"></i>
            </b>
            {{ node.label }}
            <b class="inlineBlock" v-if="data.status && data.status != ''" style="margin-left: 5px">
              <span
                :title="eventTypeEnum[data.status.toLowerCase()]"
                :style="{
                  background: levelColor[data.status.toLowerCase()],
                  display: 'inline-block',
                  width: '8px',
                  height: '14px'
                }"
              ></span>
            </b>
            <!--   <i
              class="el-icon-document"
              v-if="node.level == 4 && data.isDiagnosticResults"
              title="诊断记录"
              @click.stop="getHistory($event, data)"
              :style="{
                color: levelColor[data.diagnosticStatus],
                marginLeft: '5px'
              }"
            /> -->
            </p>
          </template>
        </el-tree>
      </div>
    </div>
    <!--  <template>
      <history-card v-show="visible" :requestParam="cardInfo" ref="historyCard" />
    </template> -->
    <!-- 过滤条件 -->
    <div class="filter_content">
      <p>
        <label> 时间：</label>
        <dateRange ref="datarange" />
      </p>
      <p>
        <label style="width: 55px">
          <input
            style="float: left; margin: -5px 3px 5px 0"
            type="checkbox"
            v-model="checked"
          />转速：</label
        >
        <el-input
          class="inline-input"
          v-model="inputMin"
          type="number"
          size="mini"
          placeholder="下限"
          :disabled="!checked"
        ></el-input>
        <span style="margin: 0 5px">-</span>
        <el-input
          class="inline-input"
          v-model="inputMax"
          type="number"
          size="mini"
          placeholder="上限"
          :disabled="!checked"
        ></el-input
        >rpm
      </p>
      <div style="width: 100%; text-align: center">
        <el-button type="primary" size="mini" @click="getTreeCheckedData">数据查询</el-button>
      </div>
    </div>
  </div>
</template>
<script>
// import _ from 'lodash'
import { levelColor, entityPartEnum, eventTypeEnum } from '@/util/constant'
// import historyCard from './card.vue'
import dateRange from './timeRange.vue'
import { analyzerListApiNew } from '@/api/analysis/index.js'
import { mapGetters } from 'vuex'
import dayjs from 'dayjs'
export default {
  components: {
    //  historyCard,
    dateRange
  },
  data() {
    return {
      checked: false,
      levelColor,
      eventTypeEnum,
      defaultExpandedNodes: [],
      currentNodeId: null,
      defaultProps: {
        children: 'children',
        label: 'name',
        key: 'id'
        // isLeaf: 'leaf'
      },
      treeData: [],
      filterText: '',
      visible: false,
      cardInfo: {
        windturbineIds: '',
        measlocId: ''
      },
      menuVisible: false,
      waveInfo: [],
      maps: new Map(),
      inputMin: '',
      inputMax: '',
      autoTimeRange: null,
      autoTimeRangeSignature: '',
      currentQuerySignature: ''
    }
  },
  computed: {
    ...mapGetters(['userInfo'])
  },
  watch: {
    // 2. 监听路由变化（处理浏览器前进/后退或直接修改URL的情况）
    /*    $route: {
      handler(newRoute) {
        this.initFromUrl()
      },
      immediate: true // 立即执行一次
    } */
    /*   filterText(val) {
      this.$refs.tree.filter(val)
    } */
  },
  mounted() {
    this.initTree()
    if (sessionStorage.getItem('checkData')) {
      const { measlocId, acqTime } = JSON.parse(sessionStorage.getItem('checkData'))
      this.currentNodeId = [measlocId] // 默认选中当前测点
      // 展开节点
      this.defaultExpandedNodes = [measlocId] //Array.from(checkedNode, i => i.id)
      this.$nextTick(() => {
        setTimeout(() => {
          this.getTreeCheckedData()
        }, 1500)
      })
    }
  },
  updated() {},
  beforeUnmount() {},
  methods: {
    initFromUrl() {
      // 从 query 中获取 id，假设参数名为 nodeId
      const { id, acqTime } = this.$route.query
      if (id) {
        this.currentNodeId = id
      } else {
        this.currentNodeId = null
      }
    },
    // 获取设备树选中的数据
    getTreeCheckedData() {
      if (
        this.inputMin != '' &&
        this.inputMax != '' &&
        Number(this.inputMin) > Number(this.inputMax)
      ) {
        return this.$message.warning('下限值不能大于上限值！')
      }
      // 获取全选中node
      // 获取半选中node
      let checkedData = this.$refs.tree.getCheckedNodes()
      if (!checkedData.length) {
        return this.$message.warning('请勾选需要分析的特征值或者测点位置！')
      }
      /*   完善部件专项分析默认显示逻辑，大部件全选中 */
      const halfCheckedData = this.$refs.tree.getHalfCheckedNodes()
      let { resultObj, paramList, tabList } = this.getCheckedData(checkedData, halfCheckedData)
      const selectionSignature = this.getSelectionSignature(checkedData, halfCheckedData)
      // 单独处理时间范围，如果结束时间是当前日期，则取当前点击按钮时候的时分秒
      let timeRange = []
      let time = this.resolveQueryTimeValue(this.$refs['datarange'].timeValue, selectionSignature)
      if (time[1] == dayjs().format('YYYY-MM-DD')) {
        timeRange[0] = dayjs(time[0]).format('YYYY-MM-DD 00:00:00')
        timeRange[1] = dayjs().format('YYYY-MM-DD HH:mm:ss')
      } else {
        timeRange[0] = dayjs(time[0]).format('YYYY-MM-DD 00:00:00')
        timeRange[1] = dayjs(time[1]).format('YYYY-MM-DD 23:59:59')
      }
      this.currentQuerySignature = selectionSignature
      this.$emit('nodeClick', {
        ...resultObj,
        paramList,
        tabList,
        filterParam: {
          timeValue: timeRange,
          wkCond: {
            ROTSPEED_MCS: this.checked
              ? [
                  this.inputMin ? Number(this.inputMin) : -Infinity,
                  this.inputMax ? Number(this.inputMax) : Infinity
                ]
              : [-Infinity, Infinity]
          }
        }
      })
    },
    getDefaultTimeRange() {
      return [dayjs().subtract(1, 'year').format('YYYY-MM-DD'), dayjs().format('YYYY-MM-DD')]
    },
    getSelectionSignature(checkedData, halfCheckedData) {
      return []
        .concat(checkedData || [], halfCheckedData || [])
        .map(item => `${item.type || ''}:${item.id || ''}`)
        .sort()
        .join('|')
    },
    normalizeDateRange(range) {
      if (!Array.isArray(range) || range.length < 2 || !range[0] || !range[1]) {
        return []
      }
      return range.map(item => dayjs(item).format('YYYY-MM-DD'))
    },
    isSameDateRange(left, right) {
      const a = this.normalizeDateRange(left)
      const b = this.normalizeDateRange(right)
      return a.length === 2 && b.length === 2 && a[0] === b[0] && a[1] === b[1]
    },
    resolveQueryTimeValue(time, selectionSignature) {
      const currentRange = this.normalizeDateRange(time)
      if (!currentRange.length) {
        return this.getDefaultTimeRange()
      }
      const isUsingOtherSelectionAutoRange =
        this.autoTimeRange &&
        this.isSameDateRange(currentRange, this.autoTimeRange) &&
        this.autoTimeRangeSignature &&
        this.autoTimeRangeSignature !== selectionSignature
      if (!isUsingOtherSelectionAutoRange) {
        return currentRange
      }

      const defaultRange = this.getDefaultTimeRange()
      this.autoTimeRange = null
      this.autoTimeRangeSignature = ''
      if (this.$refs['datarange']) {
        this.$refs['datarange'].timeValue = [...defaultRange]
      }
      return defaultRange
    },
    shouldUseAutoTimeRange(time) {
      const currentRange = this.normalizeDateRange(time)
      if (!currentRange.length) {
        return true
      }
      if (this.isSameDateRange(currentRange, this.getDefaultTimeRange())) {
        return true
      }
      return Array.isArray(this.autoTimeRange) && this.isSameDateRange(currentRange, this.autoTimeRange)
    },
    setTimeRange(range, force = false) {
      if (!Array.isArray(range) || range.length < 2 || !range[0] || !range[1]) {
        return
      }
      const nextRange = range.map(item => dayjs(item).format('YYYY-MM-DD'))
      const current = this.$refs['datarange'] ? this.$refs['datarange'].timeValue : []
      if (!force && !this.shouldUseAutoTimeRange(current)) {
        return
      }
      this.autoTimeRange = nextRange
      this.autoTimeRangeSignature = this.currentQuerySignature
      if (this.$refs['datarange']) {
        this.$refs['datarange'].timeValue = [...nextRange]
      }
    },
    getCheckedData(checkedArr, halfArr, levelType) {
      let arr = [].concat(checkedArr, halfArr)
      if (levelType) {
        let result = []
        arr.forEach(item => {
          if (item.type == levelType) {
            result.push(item.id)
          }
        })
        return result
      } else {
        let resultObj = { turbine: [], comp: [], measloc: [] }
        let paramList = []
        let tabList = new Set()
        // let defalutOpenTabList = new Set()
        let keyValue = {
          turbine: 'turbine',
          component: 'comp',
          measloc: 'measloc'
        }
        arr.forEach(item => {
          let obj = {
            id: item.id
          }
          resultObj[keyValue[item.type]] ? resultObj[keyValue[item.type]].push(obj) : null
          if (item.type == 'bigComponent' && item.code !== 'TVM_STE') {
            tabList.add(item.code)
          } else if (
            item.type == 'component' &&
            (item.code == 'TVM_STE_FDN' || item.code == 'TVM_STE_TOP' || item.code == 'TVM_CBF')
          ) {
            tabList.add(item.code)
          }
          if (item.type == 'measloc') {
            paramList.push({
              windturbineID: item.parent.turbineId,
              measLoctionID: item.id
            })
          }
        })
        /**  checkedArr.forEach(item => {
          if (item.type == 'bigComponent') {
            defalutOpenTabList.add(item.code)
          }
          if (item.type == 'measloc') {
            paramList.push({
              windturbineID: item.parent.turbineId,
              measLoctionID: item.id
            })
          }
        })*/
        return {
          resultObj,
          paramList,
          tabList: [...tabList]
          //  defalutOpenTabList: [...defalutOpenTabList]
        }
      }
    },

    getClass(data) {
      if (data.type == 'windpark') {
        return `local-location`
      }
      if (data.code == 'TVM_CBF' || data.code == 'SSD' || data.code == 'STL') {
        return `local-TOWPL`
      } else if (data.code == 'TVM_STE_TOP') {
        return `local-TOWTOP`
      } else if (data.code == 'TVM_STE_FDN') {
        return `local-TOWFDN`
      } else if (data.code == 'TVM_FLG_GAP') {
        return `local-TOWFL`
      } else if (data.code in entityPartEnum) {
        let code = !isNaN(parseFloat(data.code.slice(-1))) ? data.code.slice(0, -1) : data.code
        return `local-${code}`
      } else if (data.type == 'turbine') {
        return `local-turbine`
      } else if (data.type == 'sampleRate') {
        return `local-acqData`
      } else if ('signalTypeCode' in data) {
        return `local-link`
      } else {
        return `local-mark`
      }
    },
    searchTree() {
      this.$refs.tree.filter(this.filterText)
    },
    /*  filterNode(value, data) {
      if (!value) return true
      return data.name.indexOf(value) !== -1
    }, */
    filterNode(value, data, node) {
      if (!value) return true
      let parentNode = node.parent
      let labels = [data.name]
      let level = 1
      while (level < node.level) {
        labels = [...labels, parentNode.data.name]
        parentNode = parentNode.parent
        level++
      }
      return labels.some(label => label.includes(value))
    },
    /*  filterNode(value, data, node) {
      if (!value) {
        //如果数据为空，则返回true,显示所有的数据项
        node.expanded = false
        return true
      }
      // 查询列表是否有匹配数据

      // let val = value.toLowerCase()
      return this.chooseNode(value, data, node) // 调用过滤二层方法
    }, */
    chooseNode(value, data, node) {
      if (data.name.indexOf(value) !== -1) {
        return true
      }
      const level = node.level
      // 如果传入的节点本身就是一级节点就不用校验了
      if (level === 1) {
        return false
      }

      // 先取当前节点的父节点
      let parentData = node.parent
      // 遍历当前节点的父节点
      let index = 0
      while (index < level - 1) {
        // 如果匹配到直接返回
        if (parentData.data.name.indexOf(value) !== -1) {
          return true
        }
        // 否则的话再往上一层做匹配
        parentData = parentData.parent
        index++
      }
      // 没匹配到返回false

      return false
    },

    //接口获取数据
    initTree(callback) {
      analyzerListApiNew({
        userId: this.userInfo?.user_id || ''
      }).then(res => {
        if (res.data.code === 200) {
          let treeData = res.data.data
          treeData.forEach(item => {
            if (item.children && item.children.length > 0) {
              item.children.forEach(child => {
                this.traverse(child, { turbineId: child.id, name: child.name })
              })
            }
          })
          this.treeData = this.freezeForTree(treeData)
          if (callback) callback()
        }
      })
    },
    freezeForTree(list) {
      const walk = arr =>
        arr.forEach(n => {
          n.$treeNodeId = 0 // 占坑
          if (Array.isArray(n.children)) walk(n.children)
        })
      walk(list)
      return Object.freeze(list) // 整体 freeze
    },
    traverse(node, parent) {
      // 添加父级信息
      node.parent = parent
      // 如果当前节点有子节点，则递归遍历子节点
      if (node.children && node.children.length > 0) {
        node.children.forEach(child => {
          this.traverse(child, parent)
        })
      }
    },
    refreshTree() {
      this.filterText = ''
      let checkedNode = this.$refs.tree.getCheckedNodes()
      this.initTree(() => {
        this.$nextTick(() => {
          this.defaultExpandedNodes = Array.from(checkedNode, i => i.id)
          this.$refs.tree.setCheckedNodes(checkedNode)
        })
      })
    },
    getHistory(e, data) {
      const { clientX, clientY } = e
      this.$refs.historyCard.$el.style.left = clientX + 'px'
      this.$refs.historyCard.$el.style.top = clientY + 'px'
      this.visible = true
      this.cardInfo = {
        // windturbineIds: data.info.windturbineId,
        measlocId: data.id,
        diagnostiCrecordTime: data.diagnostiCrecordTime,
        diagnosticonclusion: data.diagnosticonclusion,
        diagnosticStatus: data.diagnosticStatus
      }
      window.addEventListener('click', () => {
        this.visible = false
        window.removeEventListener('click', () => {
          this.visible = false
        })
      })
    },
    handleNodeExpand(data, node, component) {
      if (node.level === 2) {
        this.expandAllChildren(node) // 展开所有子节点
      }
    },
    expandAllChildren(nodes) {
      if (nodes.childNodes?.length) {
        for (let i = 0; i < nodes.childNodes.length; i++) {
          nodes.childNodes[i].expand()
          if (nodes.childNodes[i].childNodes?.length) {
            this.expandAllChildren(nodes.childNodes[i])
          }
        }
      }
    }
  }
}
</script>

<style lang="scss" scoped>
.aside-content {
  min-width: 30px;
  height: 100%;
  padding: 10px 0;
  text-align: center;
  display: flex;
  flex-direction: column;
  h4 {
    padding-bottom: 10px;
  }
  .tree_tool {
    width: 100%;
    height: 40px;
    display: flex;
    flex-direction: row;
    align-items: center;
    justify-items: center;
    justify-content: space-between;
    img {
      width: 14px;
      cursor: pointer;
      margin: 0 5px;
    }
    i {
      font-size: 16px;
      font-weight: bolder;
      cursor: pointer;
      margin: 0 5px;
      &:hover {
        color: #409eff;
      }
    }
    span {
      width: 24px;
      cursor: pointer;
      margin-left: 8px;
      img {
        width: 24px;
        float: left;
        margin-top: -5px;
      }
    }
  }
  .tree_content {
    width: 100%;
    flex: 1;
    display: flex;
    flex-direction: column;
    overflow: hidden;
    .tree_div {
      flex: 1;
      width: 100%;
      overflow-y: auto;
      overflow-x: auto;
      padding-bottom: 10px;
      background: #f9f9f9;
    }
  }
  .filter_content {
    width: 100%;
    height: 100px;
    p {
      width: 100%;
      font-size: 12px;
      text-align: left;
      margin: 5px 0;
      label {
        display: inline-block;
        width: 40px;
      }
      .inline-input {
        width: calc(50% - 50px);
        ::v-deep .el-input__inner {
          padding: 0;
          text-align: center;
        }
      }
      input {
        height: 30px;
        line-height: 30px;
      }
      ::v-deep .el-range-editor {
        width: calc(100% - 40px);
        height: 30px;
        line-height: 30px;
      }
      ::v-deep .el-date-editor {
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
    }
  }
  ::v-deep .el-tree {
    width: 100%;
    color: #606266 !important;
    margin: 10px 0 0 0 !important;

    .el-tree-node__content {
    }
    .edit_icon {
      display: none;
      font-size: 14px;
      margin: 0 3px;
    }
  }
  .custom-tree-node {
    width: 100%;
    text-align: left;
    height: 24px;
    font-size: 14px;
    display: flex;
    flex-direction: row;
    justify-content: left;
    align-items: center;
    .inlineBlock {
      width: auto;
      height: 24px;
      display: inline-flex;
      justify-content: center;
      align-items: center;
      font-weight: normal;
    }
    span {
      font-size: 14px;
    }
  }
  .custom-tree-node span {
    font-size: 14px;
  }
  .menublock {
    position: fixed;
    z-index: 9999;
    width: 150px;
    height: auto;
    line-height: 28px;
    font-size: 12px;
    background: #fff;
    padding: 5px 0;
    border: 1px solid #ccc;
    li {
      cursor: pointer;
      &:hover {
        background: #eee;
      }
    }
  }
}
</style>
