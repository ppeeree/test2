<template>
  <p>
    <label style="width: 55px">
      <input
        style="float: left; margin: -5px 3px 5px 0"
        type="checkbox"
        v-model="checked"
      />转速：</label
    >
    <el-input
      class="inline-input"
      v-model="inputMin"
      type="number"
      size="mini"
      placeholder="下限"
      :disabled="!checked"
    ></el-input>
    <span style="margin: 0 10px">-</span>
    <el-input
      class="inline-input"
      v-model="inputMax"
      type="number"
      size="mini"
      placeholder="上限"
      :disabled="!checked"
    ></el-input
    >rpm
  </p>
</template>
<script>
export default {
  data() {
    return {
      inputMin: '',
      inputMax: '',
      checked: false
    }
  },
  methods: {
    getWorkRange() {
      if (
        this.inputMin != '' &&
        this.inputMax != '' &&
        Number(this.inputMin) > Number(this.inputMax)
      ) {
        return this.$message.warning('下限值不能大于上限值！')
      }
      let data = this.checked
        ? [
            this.inputMin ? Number(this.inputMin) : -Infinity,
            this.inputMax ? Number(this.inputMax) : Infinity
          ]
        : [-Infinity, Infinity]
      return data
    }
  }
}
</script>
<style lang="scss" scoped>
p {
  display: inline-block;
  width: 300px;
  font-size: 12px;
  text-align: left;
  color: #000;
  label {
    display: inline-block;
    width: 40px;
  }
  .inline-input {
    width: 100px;
    :deep(.el-input__inner){
      padding: 0;
      text-align: center;
    }
  }
  input {
    height: 30px;
    line-height: 30px;
  }
  :deep(.el-range-editor){
    width: 200px;
    height: 30px;
    line-height: 30px;
  }
  :deep(.el-date-editor){
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
</style>
