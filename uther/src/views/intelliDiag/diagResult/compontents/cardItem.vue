<template>
  <div class="stat-item">
    <div class="icon-circle" :style="{ background: bgColor }">
      <i :class="['icon', 'local', icon]" style="font-size: 20px"></i>
    </div>
    <div class="stat-info">
      <div class="label" :style="{ color: fontColor || '#909399' }">{{ titleText }}</div>
      <div class="num">{{ value }} <span class="unit">台</span></div>
    </div>
  </div>
</template>
<script>
export default {
  props: {
    value: {
      type: Number,
      default: 0
    },
    titleText: {
      type: String,
      default: ''
    },
    icon: {
      type: String,
      default: ''
    },
    bgColor: {
      type: String,
      default: ''
    },
    fontColor: {
      type: String,
      default: ''
    }
  },
  data() {
    return {}
  }
}
</script>
<style lang="scss" scoped>
.stat-item {
  display: flex;
  align-items: center;
  padding: 10px;
  border-radius: 6px;
  transition: all 0.3s;

  /* 运维时限的高亮框样式 */
  .highlight-box {
    background-color: #fef0f0; /* 浅红背景 */
    border: 1px solid #fde2e2;
  }

  .icon-circle {
    width: 40px;
    height: 40px;
    border-radius: 50%;
    display: flex;
    justify-content: center;
    align-items: center;
    color: #fff;
    font-size: 24px;
    margin-right: 15px;
  }

  .stat-info .num {
    font-size: 24px;
    font-weight: bold;
    color: #131414;
    line-height: 30px;
  }

  .stat-info .label {
    font-size: 14px;
    color: #606266;
    margin-top: 5px;
    line-height: 30px;
  }

  .unit {
    font-size: 12px;
    margin-left: 14px;
    color: #767676;
  }
}
</style>
