<template>
  <div class="content">
    <el-input placeholder="输入关键字进行过滤" v-model="filterText"> </el-input>
    <el-tree :data="treeData" :props="treeProps" node-key="id" show-checkbox default-expand-all highlight-current
      ref="tree" indent="8" :expand-on-click-node="true" :filter-node-method="filterNode"
      @node-expand="handleNodeExpand">
    </el-tree>

    <!-- 过滤条件 -->
    <div class="filter_content">
      <p>
        <label> 时间：</label>
        <dateRange ref="datarange" />
      </p>
      <div style="width: 100%; text-align: center">
        <el-button type="primary" size="mini" @click="getTreeCheckedData">数据查询</el-button>
      </div>
    </div>
  </div>
</template>
<script>
import dateRange from '../../diagRecord/dateRange.vue'
import { getWindParkTreeApi } from '@/api/intelliDiag'
export default {
  components: {
    dateRange
  },
  data() {
    return {
      dataDays: '30',
      filterText: '',
      treeData: [],
      treeProps: {
        children: 'children',
        label: 'name'
      }
    }
  },
  watch: {
    filterText(val) {
      this.$refs.tree.filter(val)

      this.$nextTick(() => {
        if (!val) {
          // 筛选内容为空时，全选中
          this.checkAllNodes()
        } else {
          // 筛选有内容时，清除选中
          this.clearCheckedNodes()
        }
      })
    }
  },
  computed: {
    // 计算属性：自动获取所有节点的 id
    allCheckedKeys() {
      const getAllKeys = data => {
        let keys = []
        data.forEach(item => {
          keys.push(item.id)
          if (item.children && item.children.length > 0) {
            keys = keys.concat(getAllKeys(item.children))
          }
        })
        return keys
      }
      return getAllKeys(this.treeData)
    }
  },
  created() {
    this.getTreeData()
  },
  methods: {
    // 新增：全选所有节点
    checkAllNodes() {
      if (this.$refs.tree && this.treeData.length) {
        this.$refs.tree.setCheckedKeys(this.allCheckedKeys, false)
        // 全选后触发一次数据查询，保持状态同步
        this.getTreeCheckedData()
      }
    },
    // 新增：清空选中节点
    clearCheckedNodes() {
      if (this.$refs.tree) {
        this.$refs.tree.setCheckedKeys([], false)
      }
    },
    getTreeData() {
      getWindParkTreeApi().then(res => {
        const { data } = res.data
        this.treeData = data
        this.$nextTick(() => {
          // 初始化时全选
          this.checkAllNodes()
        })
      })
    },

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
      // return data.name.indexOf(value) !== -1
    },
    handleNodeExpand(data, node, component) {
      this.expandAllChildren(node) // 展开所有子节点
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
    },
    getTreeCheckedData() {
      // 获取全选中的节点
      let checkedData = this.$refs.tree.getCheckedNodes()
      let ids = []
      checkedData.forEach(item => {
        if (item.type == 'station') {
          ids.push(item.id)
        }
      })
      this.$emit('checkData', ids, this.$refs.datarange.timeValue)
    },
    getCheckedNodesFirstLevelNames() {
      // 1. 获取所有完全选中的节点
      const checkedNodes = this.$refs.tree.getCheckedNodes()
      // 2. 获取所有半选中的节点
      const halfCheckedNodes = this.$refs.tree.getHalfCheckedNodes()
      // 3. 合并两个数组
      const allSelectedNodes = checkedNodes.concat(halfCheckedNodes)
      // 4. 筛选出根节点 (level === 1)
      const rootNodes = allSelectedNodes.filter(node => node.type === 'center')

      // 如果只需要根节点的 ID，可以进一步处理：
      const rootNodeNames = rootNodes.map(node => node.name)
      return rootNodeNames
    }
  }
}
</script>
<style lang="scss" scoped>
.content {
  width: 100%;
  height: 100%;
  padding: 10px;
}

:deep(.el-tree--highlight-current .el-tree-node.is-current>.el-tree-node__content){
  background-color: #32a9ff;
  color: #fff;
}

.el-tree {
  width: 100%;
  height: calc(100% - 150px);
  overflow-y: auto;
  border-bottom: 2px solid #eee;
}

.el-radio {
  color: #606266;
}

.filter_content {
  width: 100%;
  height: 100px;
  line-height: 35px;
  padding-top: 10px;

  p {
    width: 100%;
    font-size: 12px;
    text-align: left;
    margin: 5px 0;
    display: inline-block;

    input {
      height: 30px;
      line-height: 30px;
    }

    :deep(.el-range-editor){
      width: calc(100% - 40px);
      height: 30px;
      line-height: 30px;
    }

    :deep(.el-date-editor){
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
</style>
