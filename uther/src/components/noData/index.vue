<template>
  <div class="noData_content">
    <div>
      <img src="/img/noData.png" title="no data" alt="pic" />
      <p>{{ firstText }}</p>
      <p>{{ noteText }}</p>
    </div>
  </div>
</template>
<script>
export default {
  props: {
    noteText: {
      type: String,
      require: false,
      default: ''
    },
    firstText: {
      type: String,
      require: false,
      default: '暂无数据'
    }
  }
}
</script>

<style lang="scss" scoped>
.noData_content {
  width: 100%;
  height: 100%;
  text-align: center;
  display: flex;
  flex-direction: row;
  justify-content: center;
  align-items: center;
  line-height: 20px;
  p {
    text-align: center;
    line-height: 20px;
    color: #909399;
    font-size: 12px;
  }
}
</style>
