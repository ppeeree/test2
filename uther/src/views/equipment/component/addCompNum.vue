<template>
  <div style="width: 100%">
    <el-form-item
      v-for="(item, index) in paramList"
      :key="index"
      :label="paramList.length == 1 ? '参数： ' : '参数(' + Number(index + 1) + ')：'"
    >
      <el-input
        v-model="paramList[index].key"
        placeholder="参数名称"
        @change="nameTransform"
        @blur="handlerBlur"
      ></el-input>
      &nbsp; &nbsp; & &nbsp; &nbsp;
      <el-input
        v-model="paramList[index].value"
        placeholder="参数数值"
        type="number"
        @change="numTransform"
        @blur="handlerBlur"
      ></el-input>
    </el-form-item>
    <div class="add_parameter_btn" @click="addComp"><i class="el-icon-plus"></i> 添加参数</div>
  </div>
</template>
<script>
export default {
  data() {
    return {
      paramList: [
        {
          key: '',
          value: ''
        }
      ]
    }
  },
  mounted() {},
  methods: {
    addComp() {
      this.paramList.push({
        key: '',
        value: ''
      })
    }
  }
}
</script>
<style lang="less" scoped>
::v-deep .el-input {
  width: 35%;
}
</style>
