<template>
  <div
    style="width: 100%; height: 50px; line-height: 34px; cursor: pointer"
    @click="handleShowContent"
  >
    <div class="dividing_line" style="margin-bottom: 9px" />
    <div style="display: flex; justify-content: space-between">
      <div>
        <span class="first_level_title">{{ title }}</span>
        <slot name="left" />
      </div>
      <i :class="handleIconType" style="font-size: 17px; cursor: pointer" />
    </div>
  </div>
</template>

<script>
export default {
  props: {
    title: String,
    iconTypeProp: {
      type: Boolean,
      default: true
    }
  },
  data() {
    return {
      iconType: true
    }
  },
  mounted() {
    this.iconType = this.iconTypeProp
  },
  computed: {
    handleIconType() {
      return this.iconType ? 'el-icon-arrow-right' : 'el-icon-arrow-down'
    }
  },
  methods: {
    handleShowContent() {
      this.iconType = !this.iconType
      this.$emit('handleShowContent', this.iconType)
    }
  }
}
</script>

<style lang="less" scoped>
@import url('../styles/common.less');
</style>
