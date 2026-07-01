/*@author:hugaiping @time:2025/9/3 @desc:滤波设置*/
<template>
  <el-popover
    placement="bottom"
    popper-class="setUnitPop"
    title="滤波设置"
    width="300"
    :visible="visible"
    @update:visible="visible = $event"
  >
    <div class="pop_content_custom">
      <p class="title_btn">
        {{ selectedType === 'bandpass' ? '带通' : selectedType === 'lowpass' ? '低通' : '高通' }}

        <el-button
          :class="selectedType === 'lowpass' ? 'active' : ''"
          type=""
          size="mini"
          @click="changeType('lowpass')"
        >
          <img title="低通" :src="require('/public/img/limitdown.png')" />
        </el-button>
        <el-button
          :class="selectedType === 'highpass' ? 'active' : ''"
          type=""
          size="mini"
          @click="changeType('highpass')"
        >
          <img title="高通" :src="require('/public/img/limitup.png')" />
        </el-button>
        <el-button
          :class="selectedType === 'bandpass' ? 'active' : ''"
          type=""
          size="mini"
          @click="changeType('bandpass')"
        >
          <img title="带通" :src="require('/public/img/limit.png')" />
        </el-button>
      </p>
      <el-row class="content">
        <el-col :span="12">fpass1</el-col>
        <el-col :span="12">fpass2</el-col>
      </el-row>
      <el-row class="content" style="margin-bottom: 10px">
        <el-col :span="10" class="content_input">
          <input
            :disabled="selectedType === 'lowpass'"
            v-model="minNum"
            type="number"
            maxlength="5"
            placeholder="--"
          />
        </el-col>
        <el-col :span="2" class="content_Symbol">~</el-col>
        <el-col :span="10" class="content_input">
          <input
            :disabled="selectedType === 'highpass'"
            v-model="maxNum"
            type="number"
            maxlength="5"
            placeholder="--"
            :max="maxDefaultNum"
          />
        </el-col>
        <el-col :span="2" class="content_Symbol">Hz</el-col>
      </el-row>
      <div class="note_warning" v-if="isShowWarning1">fpass2数值需大于fpass1数值，请重新设置！</div>
      <div class="note_warning" v-if="isShowWarning2">
        fpass2数值不能大于（采集频率/2.56计算所得的数值），请重新设置！
      </div>
      <div style="text-align: center; margin: 0">
        <el-button size="mini" style="margin-right: 10px" type="text" @click="visible = false"
          >取消</el-button
        >
        <el-button type="primary" size="mini" @click="submitData">确定</el-button>
      </div>
    </div>
    <template #reference>
      <img
        style="cursor: pointer; float: right"
        title="滤波"
        :src="require('/public/img/limitlogo.png')"
      />
    </template>
  </el-popover>
</template>

<script>
export default {
  props: {
    maxHz: {
      type: Number,
      default: 3000
    }
  },
  data() {
    return {
      visible: false,
      minNum: 1,
      maxNum: 3000,
      selectedType: 'bandpass',
      isShowWarning2: false,
      isShowWarning1: false
    }
  },
  watch: {
    minNum: function (newVal) {
      if (Number(newVal) > Number(this.maxNum)) {
        this.isShowWarning2 = false
        this.isShowWarning1 = true
      }
    },
    maxNum: function (newVal) {
      if (Number(newVal) > this.maxDefaultNum) {
        this.isShowWarning1 = false
        this.isShowWarning2 = true
      } else if (Number(newVal) < Number(this.minNum)) {
        this.isShowWarning1 = true
        this.isShowWarning2 = false
      } else {
        this.isShowWarning1 = false
        this.isShowWarning1 = false
      }
    }
  },
  computed: {
    maxDefaultNum: function () {
      return Math.ceil(Number(this.maxHz) / 2.56)
    }
  },
  mounted() {
    this.maxNum = this.maxDefaultNum
  },
  methods: {
    //切换类型
    changeType(type) {
      this.selectedType = type
      if (type === 'lowpass') {
        this.minNum = 0 //-Infinity
        this.maxNum = this.maxDefaultNum
      } else if (type === 'highpass') {
        this.minNum = 0
        this.maxNum = this.maxDefaultNum
      } else {
        this.minNum = 0
        this.maxNum = this.maxDefaultNum
      }
    },
    // 点击确实提交
    submitData() {
      /* if (this.minNum != '' && this.maxNum != '' && Number(this.minNum) > Number(this.maxNum)) {
        this.isShowWarning1 = true
        return
      }
      if (Number(this.maxNum) > Number((this.maxHz / 2.56).toFixed(2))) {
        this.isShowWarning2 = true
        return
      } */
      if (!this.isShowWarning1 && !this.isShowWarning2) {
        this.$emit('submitFilterData', {
          down: this.minNum,
          up: this.maxNum,
          type: this.selectedType
        })
        this.visible = false
      } else {
        return this.$message.error('请检查输入的数值')
      }
    }
  }
}
</script>

<style lang="scss" scoped>
.pop_content_custom {
  background: #fff;
  width: 100%;
  height: auto;
  padding: 10px 15px;

  .title_btn {
    margin-bottom: 10px;
    .el-button--mini {
      border: none;
      float: right;
      padding: 3px 5px !important;
    }
    .active {
      background: #eee;
    }
  }
  .note_warning {
    color: red;
    font-size: 12px;
    margin: 5px 0 15px 0;
  }
  .content {
    margin: 0;
    width: 100%;
    .content_Symbol {
      height: 30px;
      line-height: 30px;
      text-align: center;
    }
    .el-col {
      margin-bottom: 0px;
    }
    input {
      width: 100%;
      height: 30px;
      line-height: 30px;
      border: 1px solid #dcdfe6;
      border-radius: 5px;
      padding: 0 10px;
      &:hover {
        background-color: #fff !important;
        border-radius: 4px !important;
        border-color: #dcdfe6 !important;
        color: #606266 !important;
        -webkit-box-shadow: none;
        box-shadow: none;
        background: none;
        height: 30px;
        line-height: 30px;
      }
    }
  }
}
</style>
