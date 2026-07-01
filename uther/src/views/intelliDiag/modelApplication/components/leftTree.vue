<template>
  <div class="content">
    <el-input placeholder="输入关键字进行过滤" v-model="filterText"> </el-input>
    <el-tree :data="treeData" :props="treeProps" node-key="id" default-expand-all highlight-current ref="tree"
      indent="8" :expand-on-click-node="false" :filter-node-method="filterNode" @node-click="handleNodeClick">
    </el-tree>
  </div>
</template>
<script>
import { getWindParkTreeApi } from '@/api/intelliDiag'
export default {
  components: {},
  data() {
    return {
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
    }
  },
  computed: {},
  created() {
    this.getTreeData()
  },
  methods: {
    /**
     * 核心逻辑：查找并选中第一个叶子节点
     */
    selectFirstLeaf() {
      const firstLeaf = this.findFirstLeaf(this.treeData)
      if (firstLeaf && firstLeaf.id) {
        // setCurrentKey 会触发高亮，且不会触发 node-click 事件
        this.$refs.tree.setCurrentKey(firstLeaf.id)
        // 如果你需要在初始化时也触发业务逻辑（比如加载右侧详情），需要手动 emit
        this.$emit('clickedNode', firstLeaf)
      }
    },

    /**
     * 递归查找第一个叶子节点
     * 定义：没有 children 或者 children 长度为 0 的节点
     */
    findFirstLeaf(nodes) {
      if (!nodes || nodes.length === 0) return null

      for (let i = 0; i < nodes.length; i++) {
        const node = nodes[i]
        // 判断是否为叶子节点
        const isLeaf = !node.children || node.children.length === 0

        if (isLeaf) {
          return node // 找到了，直接返回
        } else {
          // 如果不是叶子，继续往深层找
          const found = this.findFirstLeaf(node.children)
          if (found) return found
        }
      }
      return null
    },

    /**
     * 节点点击事件
     * 只有叶子节点才允许响应业务逻辑
     */
    handleNodeClick(data, node) {
      const isLeaf = !data.children || data.children.length === 0

      if (!isLeaf) {
        // 如果是父级节点（如事业部），阻止选中状态或不做任何反应
        // 注意：el-tree 点击父级可能仍会有蓝色背景，如果需要完全禁止父级变色，需配合 CSS 或 setCurrentKey(null)
        // 这里我们选择不执行业务回调
        return
      }
      this.$emit('clickedNode', data, node)
    },

    getTreeData() {
      getWindParkTreeApi().then(res => {
        const { data } = res.data
        this.treeData = data
        this.$nextTick(() => {
          this.selectFirstLeaf()
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

::v-deep .el-tree--highlight-current .el-tree-node.is-current>.el-tree-node__content {
  background-color: #32a9ff;
  color: #fff;
}

.el-tree {
  width: 100%;
  height: calc(100% - 50px);
  overflow-y: auto;
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
</style>
