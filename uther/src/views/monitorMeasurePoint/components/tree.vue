<template>
  <div style="width: 100%; height: 100%">
    <h4>监测方案</h4>
    <div class="tree_block">
      <el-tree
        ref="treeCard"
        node-key="label"
        :data="list"
        :props="defaultProps"
        :default-expand-all="true"
        :expand-on-click-node="false"
        :highlight-current="true"
        @node-click="handleNodeClick"
      >
        <span
          class="custom-tree-node"
          slot-scope="{ data }"
          style="width: 100%; height: 30px"
          @mouseenter="mouseEnter(data)"
          @mouseleave="mouseLeave(data)"
        >
          <!-- <el-tooltip effect="light" :content="data.name" placement="bottom"> -->
          <div
            style="
              width: 85%;
              display: inline-block;
              white-space: nowrap;
              overflow: hidden;
              text-overflow: ellipsis;
            "
          >
            {{ data.name }}
          </div>
          <!-- </el-tooltip> -->
          <span
            style="
              width: 20px;
              height: 20px;
              line-height: 30px;
              display: inline-block;
              position: relative;
              bottom: 10px;
            "
            @click.stop="deleteNode(data)"
            ><i v-show="data.mouseEnter" @click.stop="deleteNode(data)" class="el-icon-remove"></i
          ></span>
        </span>
      </el-tree>
    </div>
    <div class="foot_btn">
      <el-button size="small" type="primary" icon="el-icon-plus" @click="addPlan">新增</el-button>
    </div>
  </div>
</template>

<script>
import { remove } from '@/api/equipment/monitor'
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
      defaultProps: {
        label: 'name',
        children: 'children'
      }
    }
  },
  mounted() {},
  methods: {
    handleNodeClick(code) {
      if (!code.children) {
        this.$emit('clickedPlan', code.name, code)
      } else {
        this.$message({
          type: 'warning',
          message: '请选择监测部件查看！'
        })
      }
    },
    addPlan() {
      this.$emit('clickedPlan', 'plan')

      this.$refs.treeCard.setCurrentKey(null)
    },

    mouseEnter(data) {
      this.$set(data, 'mouseEnter', true)
    },
    mouseLeave(data) {
      this.$set(data, 'mouseEnter', false)
    },
    deleteNode(data) {
      if (data.children) {
        this.$confirm('确定删除该方案?', {
          confirmButtonText: '确定',
          cancelButtonText: '取消',
          type: 'warning'
        })
          .then(() => {
            console.log('点击删除', data)
            remove(data.children[0].id)
          })
          .then(() => {
            this.$message({ type: 'success', message: '删除方案成功！' })
            this.$emit('initTreeData')
          })
      } else {
        this.$message({ type: 'warning', message: '请选择方案进行删除！' })
      }
    }
  }
}
</script>

<style lang="less" scoped>
h4 {
  padding-left: 15px;
  color: #fff;
  font-size: 14px;
  font-weight: 600;
  height: 28px;
  line-height: 28px;
}
.tree_block {
  height: 90%;
  // max-height: calc(100% - 90px);
  overflow-x: hidden;
  overflow-y: auto;
  font-size: 14px;
  margin-top: 10px;
  padding-left: 10px;
}
.foot_btn {
  width: 100%;
  text-align: center;
}

::v-deep .el-tree-node {
  line-height: 30px;
}
::v-deepel-tree-node__content {
  line-height: 30px;
  height: 30px;
}
::v-deep .custom-tree-node {
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
</style>
