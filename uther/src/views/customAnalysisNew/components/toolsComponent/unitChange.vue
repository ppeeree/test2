<template>
  <el-popover
    popper-class="setUnitPop"
    placement="bottom"
    title="单位切换"
    width="200"
    trigger="click"
    :visible="visible"
    @update:visible="visible = $event"
  >
    <template #reference>
      <i
        style="float: right; font-size: 18px; cursor: pointer"
        class="el-icon-setting"
        title="单位切换"
      ></i>
    </template>
    <div class="content">
      <p style="width: 100%; text-align: left">
        X轴：<br />
        <el-radio-group ref="radio" v-model="selectedx">
          <el-radio label="Hz">Hz</el-radio>
          <el-radio label="CPM">CPM</el-radio>
          <!--  <el-radio label="对数">对数</el-radio> -->
        </el-radio-group>
      </p>
      <p style="width: 100%; text-align: left">
        Y轴：<br />
        <el-radio-group ref="radio1" v-model="selectedy">
          <el-radio label="m/s^2">m/s^2</el-radio>
          <el-radio label="g">g</el-radio>
        </el-radio-group>
      </p>
      <div class="footer">
        <el-button
          size="mini"
          type="text"
          @click="visible = false"
          style="border: 1px solid white; border-radius: 4px; margin-right: 10px"
          >取消</el-button
        >
        <el-button
          type="primary"
          size="mini"
          @click="submitData"
          style="background: #1b84ee; border-radius: 4px"
          >确定</el-button
        >
      </div>
    </div>
  </el-popover>
</template>
<script>
export default {
  props: {
    defaultValue: {
      type: Array,
      require: false
    }
  },
  data() {
    return {
      selectedx: '',
      selectedy: '',
      visible: false
    }
  },
  mounted() {
    this.removeRadioAriaHidden()
    this.selectedy = this.defaultValue[1] || 'm/s^2'
    this.selectedx = this.defaultValue[0] || 'Hz'
  },
  methods: {
    removeRadioAriaHidden() {
      this.$nextTick(() => {
        const radioGroups = [this.$refs.radio, this.$refs.radio1]
        radioGroups.forEach(group => {
          const root = group && (group.$el || group)
          if (!root) return
          root.querySelectorAll('input[type="radio"]').forEach(input => {
            input.removeAttribute('aria-hidden')
          })
        })
      })
    },
    submitData() {
      this.$emit('changeUnit', [this.selectedx, this.selectedy])
      this.visible = false
    }
  }
}
</script>
<style lang="scss">
.setUnitPop {
  width: 200px;
  height: auto;
  .el-popover__title {
    height: 25px;
    line-height: 25px;
    background: #d1eaff;
    padding-left: 8px;
    font-size: 14px;
    margin-bottom: 0;
  }
  .content {
    overflow: hidden;
    background: #fff;
  }
  p {
    padding: 5px 10px;
  }
  .el-radio {
    color: #666;
    float: left;
    margin-right: 10px;
  }
  .footer {
    width: 100%;
    padding: 10px;
    text-align: center;
    button {
      margin: 0 10px;
    }
  }
}
</style>
