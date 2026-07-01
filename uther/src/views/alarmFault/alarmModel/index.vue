<template>
  <div class="alarm_index">
    <div class="alarm_index_up">
      <alarmCard></alarmCard>
      <div class="space_bar" style="top: 3%"></div>
      <alarmTabke></alarmTabke>
    </div>
    <div class="space_bar_transverse"></div>
    <div class="alarm_index_down">
      <treeCard ref="treeTree" @clickTreeItem="clickTreeItem"></treeCard>
      <div class="space_bar" style="top: 3%"></div>
      <treeTable ref="treeTable" @getTree="getTree"></treeTable>
    </div>
  </div>
</template>

<script>
import alarmCard from './alarmCard.vue'
import alarmTabke from './alarmTable.vue'
import treeCard from './treeCard.vue'
import treeTable from './treeTable.vue'

export default {
  components: {
    alarmCard,
    alarmTabke,
    treeCard,
    treeTable
  },
  data() {
    return {}
  },
  methods: {
    clickTreeItem(val) {
      this.$refs.treeTable.clickItem(val)
    },
    getTree() {
      this.$refs.treeTree.getTree()
    }
  }
}
</script>

<style lang="less" scoped>
@import url('./style.less');
.alarm_index {
  height: 100%;
  width: 100%;
  .alarm_index_up {
    height: 380px;
    width: 100%;
    margin-bottom: 10px;
    // border: 1px solid red;
    // background-color: rgba(13, 52, 83, 0.5);
  }
  .space_bar_transverse {
    height: 10px;
    background-color: #131c35;
    width: 102%;
    position: relative;
    left: -10px;
  }
  .alarm_index_down {
    height: calc(100% - 400px);
    width: 100%;
    // border: 1px solid green;
    // background-color: rgba(13, 52, 83, 0.5);
  }
}

.avue-main {
  // background: #252526;
  // height: calc(100% - 48px);
  // margin-top: -20px;

  #avue-view {
    background-color: transparent !important;
  }
}
</style>
