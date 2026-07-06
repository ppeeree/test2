<template>
  <el-popover placement="bottom" width="320" v-model="visible">
    <div class="popoCont">
      <div class="clearfix">
        <span>{{ typeName }}设置</span>
      </div>
      <div class="secondline">
        <div>
          单位：
          <sapn>{{ unit }}</sapn>
        </div>
        <div class="title_btn">
          <el-button-group>
            <el-button
              size="middle"
              @click="changeType('lower')"
              :style="{
                background:
                  this.rangeValue[0] == this.workingList.split('-')[0] * 1 &&
                  this.rangeValue[1] == this.lowValue[1]
                    ? '#1b84ee'
                    : ''
              }"
              >低</el-button
            >
            <el-button
              size="middle"
              @click="changeType('midel')"
              :style="{ background: this.rangeValue[0] == this.milderValue[0] ? '#1b84ee' : '' }"
              >中</el-button
            >
            <el-button
              size="middle"
              @click="changeType('high')"
              :style="{ background: this.rangeValue[0] == this.heightValue[0] ? '#1b84ee' : '' }"
              >高</el-button
            >
            <el-button
              size="middle"
              @click="changeType('full')"
              :style="{
                background:
                  this.rangeValue[0] == this.workingList.split('-')[0] * 1 &&
                  this.rangeValue[1] == this.heightValue[1]
                    ? '#1b84ee'
                    : ''
              }"
              >全</el-button
            >
          </el-button-group>
        </div>
      </div>
      <div class="block" style="padding: 0 5px; margin-top: 20px">
        <el-slider
          @change="changeSlider"
          :marks="marks"
          v-model="rangeValue"
          range
          :max="this.workingList.split('-')[1]"
          :show-tooltip="false"
        ></el-slider>
      </div>
      <div style="text-align: right; margin: 15px 0 0 0">
        <el-button
          size="mini"
          type="text"
          @click="resetRange"
          style="border: 1px solid white; border-radius: 4px; margin-right: 10px"
          >重置</el-button
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
    <template #reference>
      <el-button
        size="mini"
        :class="workingList !== '' ? ['func_btn', { func_btn_active: visible }] : ['func_btn_none']"
        @click="clickWorkingBtn(typeName)"
        :style="{
          marginRight: '20px',
          cursor: workingList !== '' ? 'pointer' : 'not-allowed'
        }"
      >
        <img :src="getCodeImg(workingUnit)" :title="imgTitle" />
        <!-- <span>{{ typeName }}</span> -->
      </el-button>
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
    workingList: {
      type: String,
      require: false
    },
    workingUnit: {
      type: String,
      require: false
    },
    initData: {
      type: String,
      require: false
    }
  },
  data() {
    return {
      rangeValue: '',
      marks: {
        0: '0',
        3000: {
          label: '3000',
          style: {
            'word-break': 'keep-all'
          }
        }
      },
      changeCodeName: {
        ROTSPEED_MCS: 'speed',
        POWER: 'power',
        SPEED: 'windSpeed',
        WIND_DIRECTION: 'winddirection',
        YAW_ANGLE: 'angle'
      },
      visible: false,
      selectedType: 'full',
      heightValue: [],
      milderValue: [],
      lowValue: []
    }
  },
  computed: {
    imgTitle() {
      let text = ''
      text = this.typeName + '：' + this.rangeValue[0] + '-' + this.rangeValue[1]
      return text
    }
  },
  watch: {
    rangeValue() {
      let obj = {}
      let textobj = {}
      obj[`${this.rangeValue[0]}`] = {
        label: this.rangeValue[0],
        style: {
          top: '-40px',
          color: '#000',
          marginLeft: '13px'
        }
      }
      obj[`${this.rangeValue[1]}`] = {
        label: this.rangeValue[1],
        style: {
          top: '-40px',
          color: '#000'
        }
      }

      textobj[`${this.workingList.split('-')[0]}`] = {
        label: this.workingList.split('-')[0],
        style: {
          top: '-2px',
          color: '#000',
          marginLeft: '13px'
        }
      }
      textobj[`${this.workingList.split('-')[1]}`] = {
        label: this.workingList.split('-')[1],
        style: {
          top: '-2px',
          color: '#000'
        }
      }

      this.marks = {
        ...obj,
        ...textobj
      }
      this.getdiffWorkingList()
    },
    initData() {
      if (this.initData) {
        this.rangeValue = this.initData.length == 0 ? this.workingList.split('-') : this.initData
      }
    }
  },
  mounted() {
    // console.log('当前选中的范围', this.initData)
    if (this.initData) {
      this.rangeValue = this.initData.length == 0 ? this.workingList.split('-') : this.initData
    }
    this.getdiffWorkingList()
  },
  beforeUnmount() {
    this.rangeValue = this.workingList.split('-')
  },
  methods: {
    getdiffWorkingList() {
      let alllist = this.workingList.split('-')
      let upkey, bottomkey
      if (this.typeName == '转速') {
        upkey = 0.94
        bottomkey = 0.7
      } else if (this.typeName == '功率') {
        upkey = 0.85
        bottomkey = 0.26
      }

      this.lowValue = [parseInt(alllist[1] * 0), parseInt(alllist[1] * bottomkey)]
      this.milderValue = [parseInt(alllist[1] * bottomkey), parseInt(alllist[1] * upkey)]
      this.heightValue = [parseInt(alllist[1] * upkey), parseInt(alllist[1] * 1)]
    },
    clickWorkingBtn() {
      if (this.workingList) {
        this.visible = false
      } else {
        this.visible = !this.visible
      }
    },
    //获取工况名称对应图片
    getCodeImg(name) {
      let newName = this.changeCodeName[name]
      return require(`/public/img/WindTurbine/${newName}.png`)
    },
    //切换类型
    changeType(type) {
      this.selectedType = type
      if (type === 'lower') {
        this.rangeValue = this.lowValue
      } else if (type === 'midel') {
        this.rangeValue = this.milderValue
      } else if (type === 'high') {
        this.rangeValue = this.heightValue
      } else if (type === 'full') {
        this.rangeValue = this.workingList.split('-')
      }
    },
    // 点击确实提交
    submitData() {
      if (
        this.rangeValue[0] != '' &&
        this.rangeValue[1] != '' &&
        Number(this.rangeValue[0]) > Number(this.rangeValue[1])
      ) {
        return this.$message.warning('下限值不能大于上限值！')
      }
      this.$emit('submitFilterData', {
        down: this.rangeValue[0],
        up: this.rangeValue[1],
        type: this.selectedType,
        code: this.workingUnit
      })
      this.visible = false
    },
    //
    changeSlider(val) {
      // console.log(val)
      // console.log(this.rangeValue)
    },
    //重置
    resetRange() {
      this.rangeValue = this.workingList.split('-')
    }
  }
  //
}
</script>
<style lang="scss" scoped>
.title_btn {
  .el-button-group {
    margin-left: 20px;
  }
  .el-button--default {
    padding: 5px 10px !important;
  }
}
.secondline {
  margin-top: 5px;
  display: flex;
  flex-direction: row;
  justify-content: space-between;
  justify-items: center;
  align-items: center;
}
.text {
  font-size: 14px;
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
  padding: 10px;
}
.clearfix {
  text-align: center;
  border-bottom: 1px solid rgb(234, 232, 232);
}
.box-card {
  width: 480px;
}
</style>
