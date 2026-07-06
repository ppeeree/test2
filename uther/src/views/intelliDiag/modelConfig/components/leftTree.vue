<template>
  <div class="content">
    <el-input placeholder="输入关键字进行过滤" v-model="filterText"> </el-input>
    <el-tree :data="treeData" :props="treeProps" node-key="id" highlight-current default-expand-all ref="tree"
      indent="8" :expand-on-click-node="false" :filter-node-method="filterNode" @node-click="clickedNode">
    </el-tree>
  </div>
</template>
<script>
export default {
  props: {
    treeData: {
      type: Array,
      default: () => []
    }
  },
  data() {
    return {
      filterText: '',
      currentNodeKey: null,
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
    },
    treeData: {
      handler(val) {
        if (val.length) {
          this.$nextTick(() => {
            let ids = this.getAllNodeKeys(val)
            if (this.currentNodeKey === null || !ids.includes(this.currentNodeKey.id)) {
              this.currentNodeKey = val[0]
              // 主动调用 setCurrentKey 触发高亮
              if (this.$refs.tree) {
                this.$refs.tree.setCurrentKey(val[0].id)
              }
              this.$emit('clickedNode', val[0])
            } else {
              // 2. 恢复高亮
              this.$nextTick(() => {
                this.$refs.tree.setCurrentKey(this.currentNodeKey.id)
                this.$emit('clickedNode', this.$refs.tree.getCurrentNode())
              })
            }
          })
        }
      },
      immediate: true,
      deep: true
    }
  },
  created() { },
  methods: {
    clickedNode(data, node, component) {
      this.currentNodeKey = data
      this.$emit('clickedNode', data, node)
    },
    getCurrentNode() {
      return this.$refs.tree.getCurrentNode()
    },
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

:deep(.el-tree--highlight-current .el-tree-node.is-current>.el-tree-node__content){
  background-color: #32a9ff;
  color: #fff;
}

.el-tree {
  width: 100%;
  height: calc(100% - 50px);
  overflow-y: auto;
}
</style>
