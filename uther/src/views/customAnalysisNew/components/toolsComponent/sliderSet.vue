<template>
  <el-popover
    placement="bottom"
    width="320"
    :visible="visible"
    @update:visible="visible = $event"
  >
    <div class="popoCont">
      <div class="clearfix">
        <span>{{ typeName }}设置</span>
      </div>
      <div style="margin: 10px">
        <label>范围：</label>
        <el-input
          class="inline-input"
          v-model="inputMin"
          type="number"
          size="mini"
          placeholder="下限"
        ></el-input>
        <span style="margin: 0 10px">-</span>
        <el-input
          class="inline-input"
          v-model="inputMax"
          type="number"
          size="mini"
          placeholder="上限"
        ></el-input>
        <sapn style="margin: 0 0 0 10px">{{ unit }}</sapn>
      </div>

      <div style="text-align: right; margin: 15px 0 0 0">
        <el-button
          size="mini"
          type="text"
          ref="button1"
          @click="visible = false"
          style="border: 1px solid white; border-radius: 4px; margin-right: 10px"
          >取消</el-button
        >
        <el-button
          type="primary"
          size="mini"
          ref="button2"
          @click="submitData"
          style="background: #1b84ee; border-radius: 4px"
          >确定</el-button
        >
      </div>
    </div>
    <template #reference>
      <slot v-if="$slots.content" name="content"></slot>
      <img
        v-else
        style="width: 20px; margin-right: 10px; cursor: pointer"
        :src="getCodeImg(workingUnit)"
        :title="imgTitle"
      />
    </template>
  </el-popover>
</template>
<script>
export default {
  props: {
    typeName: {
      type: String,
      require: false
    },
    unit: {
      type: String,
      require: false
    },
    workingUnit: {
      type: String,
      require: false
    },
    initData: {
      type: Array,
      require: false
    }
  },
  data() {
    return {
      inputMin: '',
      inputMax: '',
      changeCodeName: {
        ROTSPEED_MCS: 'speed',
        POWER: 'power',
        SPEED: 'windSpeed',
        WIND_DIRECTION: 'winddirection',
        YAW_ANGLE: 'angle'
      },
      visible: false
    }
  },
  computed: {
    imgTitle() {
      let text = ''
      text = this.typeName + '：' + this.inputMin + '-' + this.inputMax
      return text
    }
  },
  watch: {
    initData: {
      handler(val) {
        if (val.length) {
          this.inputMin = val[0]
          this.inputMax = val[1]
        }
      },
      deep: true
    }
  },
  mounted() {
    this.$refs.button1.$el.removeAttribute('aria-hidden')
    this.$refs.button2.$el.removeAttribute('aria-hidden')
  },
  unmounted() {},
  methods: {
    getCodeImg(name) {
      let newName = this.changeCodeName[name]
      return require(`/public/img/${newName}.png`)
    },

    // 点击确实提交
    submitData() {
      if (
        this.inputMin != '' &&
        this.inputMax != '' &&
        Number(this.inputMin) > Number(this.inputMax)
      ) {
        return this.$message.warning('下限值不能大于上限值！')
      }
      this.$emit('submitFilterData', {
        down: this.inputMin ? Number(this.inputMin) : -Infinity,
        up: this.inputMax ? Number(this.inputMax) : Infinity,
        code: this.workingUnit
      })
      this.visible = false
    }
  }
  //
}
</script>
<style lang="scss" scoped>
.text {
  font-size: 14px;
}
.inline-input {
  width: 80px;
}
// .func_btn {
//   background: transparent;
//   border: none;
//   padding: 0;
//   color: #fff;
//   line-height: 28px;
//   height: 30px;
//   padding: 5px 5px 5px 5px;
//   // cursor: pointer;
//   &:hover {
//     background: #1b84ee;
//   }
//   img {
//     float: left;
//     // width: 15px;
//     // margin: 5px 5px 0 0;
//   }
// }
// .func_btn_active {
//   background: #1b84ee;
// }
.item {
  margin-bottom: 18px;
}
.popoCont {
  background: #fff;
  overflow: hidden;
  padding-bottom: 10px;
}
.clearfix {
  text-align: left;
  background: #d1eaff;
  padding: 5px 10px;
}
.box-card {
  width: 480px;
}
:deep(.el-slider__marks-text){
  width: 36px;
}
</style>
