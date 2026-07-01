<template>
  <div>
    <div>
      <el-button
        @click="showWaveLine"
        style="float: right; cursor: pointer; margin: 5px"
        size="mini"
        type="primary"
        >波形分析</el-button
      >
      <el-button
        @click="deleteList"
        style="float: right; cursor: pointer; margin: 5px"
        size="mini"
        type="primary"
        >删除选中项</el-button
      >
      <el-button
        @click="clearList"
        style="float: right; cursor: pointer; margin: 5px"
        size="mini"
        type="primary"
        >清空列表</el-button
      >
    </div>
    <el-table
      ref="multipleTable"
      :data="list"
      tooltip-effect="dark"
      style="width: 100%"
      @selection-change="handleSelectionChange"
    >
      <el-table-column type="selection" width="55"> </el-table-column>
      <el-table-column
        v-for="item in tableColumns"
        :key="item.prop"
        :prop="item.prop"
        :label="item.label"
        :width="item.width"
      >
      </el-table-column>
    </el-table>
  </div>
</template>
<script>
export default {
  props: ['list', 'tableColumns'],
  data() {
    return {
      multipleSelection: []
    }
  },
  methods: {
    handleSelectionChange(val) {
      this.multipleSelection = val
    },
    showWaveLine() {
      this.$emit('showWaveLine', this.multipleSelection)
    },
    clearList() {
      this.$emit('changeSelectedList', [])
    },
    deleteList() {
      if (this.multipleSelection.length) {
        let arr = []
        this.list.forEach(item => {
          let index = this.multipleSelection.findIndex(i => i.id == item.id)
          if (index < 0) {
            arr.push(item)
          }
        })
        this.$emit('changeSelectedList', arr)
      } else {
        return this.$message.warning('请勾选需要删除的数据')
      }
    }
  }
}
</script>
