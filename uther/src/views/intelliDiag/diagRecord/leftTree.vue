<template>
  <div class="content">
    <el-input placeholder="输入关键字进行过滤" v-model="filterText"> </el-input>
    <!-- @node-expand="handleNodeExpand" -->
    <el-tree :data="treeData" :props="treeProps" node-key="id" show-checkbox highlight-current ref="tree" indent="8"
      :expand-on-click-node="true" :filter-node-method="filterNode" :default-expanded-keys="defaultExpandedKeys">
    </el-tree>

    <!-- 过滤条件 -->
    <div class="filter_content">
      <el-radio v-model="radio" label="1">最近一次故障记录</el-radio>
      <el-radio v-model="radio" label="2">
        <p>
          <label> 时间：</label>
          <dateRange ref="datarange" />
        </p>
      </el-radio>
      <div style="width: 100%; text-align: center">
        <el-button type="primary" size="mini" @click="getTreeCheckedData">数据查询</el-button>
      </div>
    </div>
  </div>
</template>
<script>
import dateRange from './dateRange.vue'
import { getWindTurbineTreeApi } from '@/api/intelliDiag'
export default {
  components: {
    dateRange
  },
  data() {
    return {
      radio: '1',
      filterText: '',
      treeData: [],
      defaultExpandedKeys: [],
      treeProps: {
        children: 'children',
        label: 'name'
      }
    }
  },
  watch: {
    filterText(val) {
      // 1. 执行过滤
      this.$refs.tree.filter(val)

      // 2. 清空选中状态 (需求：过滤时，设备树选中全部置空)
      // 注意：如果希望过滤后保持全选或重新全选，可以注释掉下面这行，或者改为调用 checkAllNodes()
      // 根据通常交互习惯，过滤后往往需要用户重新确认选择，所以这里保留清空逻辑。
      // 如果需求是“过滤后依然保持全选可见节点”，则需修改此处的逻辑。
      this.clearCheckedNodes()

      // 3. 当过滤条件清空时，重置树的展开状态为默认（第二级）并重新全选
      if (!val) {
        this.$nextTick(() => {
          this.resetTreeToDefaultLevel()
          // 可选：清空过滤后是否自动重新全选？
          // 如果希望清空过滤后自动恢复全选，请解开下面这行的注释
          // this.checkAllNodes()
        })
      }
    }
  },
  created() {
    this.getTreeData()
  },
  methods: {
    // 新增：获取所有节点的 key
    getAllNodeKeys(data, keys = []) {
      if (!data || !Array.isArray(data)) return keys
      data.forEach(item => {
        keys.push(item.id)
        if (item.children && item.children.length > 0) {
          this.getAllNodeKeys(item.children, keys)
        }
      })
      return keys
    },
    // 新增：全选所有节点
    checkAllNodes() {
      if (!this.$refs.tree || !this.treeData.length) return
      const allKeys = this.getAllNodeKeys(this.treeData)
      // setCheckedKeys 第一个参数是 key 数组，第二个参数 false 表示不递归处理半选状态（直接全选）
      this.$refs.tree.setCheckedKeys(allKeys, false)
    },
    // 新增：清空树节点选中状态
    clearCheckedNodes() {
      if (this.$refs.tree) {
        this.$refs.tree.setCheckedKeys([], false)
      }
    },
    getTreeData() {
      getWindTurbineTreeApi().then(res => {
        const { data } = res.data
        this.treeData = data
        this.$nextTick(() => {
          // 1. 重置展开层级
          this.resetTreeToDefaultLevel()
          // 2. 默认全部选中
          // this.checkAllNodes()
        })
      })
    },
    // 重置树展开到指定层级（默认第二级）
    resetTreeToDefaultLevel(level = 2) {
      const tree = this.$refs.tree
      if (!tree || !tree.store) return

      const nodes = tree.store.nodesMap
      if (!nodes) return

      Object.values(nodes).forEach(node => {
        if (node.level <= level) {
          node.expand()
        } else {
          node.collapse()
        }
      })
    },
    getLevelKeys(nodes, level, result) {
      if (level <= 0 || !nodes || nodes.length === 0) return

      nodes.forEach(node => {
        result.push(node.id)
        if (level > 1 && node.children && node.children.length > 0) {
          this.getLevelKeys(node.children, level - 1, result)
        }
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
    },
    handleNodeExpand(data, node, component) {
      this.expandAllChildren(node)
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
        if (item.type == 'device') {
          ids.push(item.id)
        }
      })
      this.$emit('clickedNode', ids, this.radio, this.$refs.datarange.timeValue)
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
