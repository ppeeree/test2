<template>
  <div class="content">
    <span class="font-style">近24 h：</span>
    <div class="box">
      <span>{{ num }}</span>
      <span>件</span>
    </div>
  </div>
</template>

<script>
export default {
  props: {
    num: {
      type: Number,
      default: 0
    }
  }
}
</script>

<style lang="less" scoped>
.content {
  display: flex;
  color: #fff;
  flex-direction: column;
  align-items: center;
  height: 60px;
  width: 140px;
  .font-style {
    font-size: 0.7em;
  }
  .box {
    margin-top: 10px;
    span:nth-child(1) {
      font-size: 2.7em;
      color: #00bbbd;
    }
  }
}
</style>
