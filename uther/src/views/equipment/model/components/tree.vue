<template>
  <el-tree
    ref="tree"
    :data="list"
    node-key="label"
    :props="defaultProps"
    :default-expand-all="true"
    :expand-on-click-node="false"
    :highlight-current="true"
    @node-click="handleNodeClick"
    :current-node-key="current"
    class="type_tree"
  >
    <span
      class="custom-tree-node"
      slot-scope="{ data }"
      style="width: 100%; height: 30px; line-height: 30px"
      @mouseenter="mouseEnter(data)"
      @mouseleave="mouseLeave(data)"
    >
      <span>{{ data.label }}</span>
      <span class="delete_icon" @click.stop="deleteNode(data)"
        ><i v-show="data.mouseEnter" @click.stop="deleteNode(data)" class="el-icon-remove"></i
      ></span>
    </span>
  </el-tree>
</template>

<script>
import { remove } from '@/api/equipment/model'
export default {
  props: {
    list: {
      type: Array,
      default: () => []
    }
  },
  data() {
    return {
      showModelList: [],
      defaultProps: {}
    }
  },
  mounted() {},
  methods: {
    handleNodeClick(code) {
      if (!code.children) {
        this.$emit('handlerTreeClick', code)
      } else {
        this.$message({
          type: 'warning',
          message: '请选择型号查看！'
        })
      }
    },
    mouseEnter(data) {
      this.$set(data, 'mouseEnter', true)
    },
    mouseLeave(data) {
      this.$set(data, 'mouseEnter', false)
    },

    deleteNode(data) {
      this.$confirm('确定删除该型号?', {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning'
      })
        .then(() => {
          let id = this.handlerDeleteId(data)
          remove(id)
        })
        .then(() => {
          this.$message({ type: 'success', message: '删除厂商成功' })
          this.$emit('initTreeData')
        })
    },
    handlerDeleteId(data) {
      let id = ''
      if (data.children) {
        data.children.forEach(item => {
          if (item.children) {
            item.children.forEach(ite => {
              id = ite.id
            })
          } else {
            id = item.id
          }
        })
      } else {
        id = data.id
      }
      return id
    },

    setNoneHeightLight() {
      this.$refs.tree.setCurrentKey(null)
    },
    setHeightLight(code) {
      this.$refs.tree.setCurrentKey(code.toString())
    }
  }
}
</script>

<style lang="less" scoped>
.type_tree {
  margin-top: 5px;
  font-size: 14px;
  height: 86%;
  overflow: auto;
  .delete_icon {
    color: white;
    display: inline-block;
    margin-left: 5px;
  }

  :deep(.el-tree-node__content){
    height: 30px !important;
  }
}
</style>
