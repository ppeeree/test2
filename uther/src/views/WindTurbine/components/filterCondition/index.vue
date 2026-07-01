<template>
  <div class="filter_block">
    <timeComp :time="formData.time" :timeSelect="timeSelect" @timeChange="timeChange"></timeComp>
    <div class="working_block">
      <span style="float: left; line-height: 31px" v-show="this.apiList.length">工况：</span>
      <div
        class="block_btn"
        :style="{ cursor: item.value !== '' ? 'pointer' : 'not-allowed' }"
        v-for="item in apiList"
        :key="item.code"
      >
        <slider-set
          :workingList="item.value"
          :workingUnit="item.code"
          :typeName="item.name"
          :unit="item.unit"
          :initData="[]"
          @submitFilterData="changeWorking"
        />
      </div>
    </div>
  </div>
</template>
<script>
import sliderSet from './sliderSet'
import dayjs from 'dayjs'
// import { getWkRangeApi } from '@/api/analysis/index.js'
import isEqual from 'lodash/isEqual'
import timeComp from '@/components/timeComp/time.vue'

/* const DEFAULTFROMDE = {
  time: [],
  speed: [],
  power: []
} */

export default {
  components: {
    sliderSet,
    timeComp
  },
  props: {
    //工况数据
    apiList: {
      type: String,
      require: true,
      default: ''
    },
    //当前分析机组
    turbineIdList: {
      type: String,
      require: true,
      default: ''
    }
    /*   filterData: {
      type: Object,
      default: () => {}
    } */
  },
  data() {
    return {
      timeSelect: false, //设置时间选择器禁选变量
      // apiList: [], //存放接口数据
      formData: {},
      defaultValue: {},
      time: ''
    }
  },
  watch: {
    apiList: {
      handler() {
        this.getWkRangeApiFunc()
      }
    }
  },
  created() {},
  mounted() {
    this.getWkRangeApiFunc()
    // console.log('初始化参数', this.formData)
    // this.$emit('changeFunc', this.formData)
    // this.formData = JSON.parse(JSON.stringify(this.filterData))
  },
  destroyed() {
    if (!isEqual(this.defaultValue, this.formData)) {
      this.formData = JSON.parse(JSON.stringify(this.defaultValue))
      this.$emit('changeFunc', this.formData)
    }
  },
  methods: {
    getWkRangeApiFunc() {
      let result = {}
      this.apiList.forEach(item => {
        let range = item.value.split('-')
        let code = item.code
        this.$set(result, code, range)
      })

      let endTime
      let startTime
      if (this.turbineIdList.length == 0) {
        this.timeSelect = true
      } else {
        this.timeSelect = false
      }
      endTime = dayjs().format('YYYY-MM-DD 23:59:59')
      startTime = dayjs().subtract(3, 'months').format('YYYY-MM-DD 00:00:00')

      this.defaultValue = {
        time: [startTime, endTime],
        ...result
      }
      this.formData = {
        time: [startTime, endTime],
        ...result
      }
      this.$emit('changeFunc', this.formData)
    },
    changeWorking(data) {
      const { down, up } = data
      let code = data.code
      this.$set(this.formData, code, [down, up])
      this.$emit('changeFunc', this.formData)
    },
    timeChange(time) {
      this.formData.time = time
      this.$emit('changeFunc', this.formData)
    }
  }
}
</script>
<style lang="scss">
.filter_block {
  // width: 100%;
  height: 38px;
  border-radius: 5px;
  padding: 3px 10px 3px 80px !important;
  // background: linear-gradient(270deg, #0e496c 82%, rgba(14, 73, 108, 0) 100%);
  .filter_block_timePic {
    float: left;
    margin: 5px 10px 0 0;
  }
  .working_block {
    color: #fff;
    float: left;
    margin-left: 10px;
  }
  .el-range-editor {
    width: 340px;
    float: left;
    border-radius: 5px;
    cursor: pointer;
  }
  .el-range-editor.el-input__inner {
    border: 1px solid #255873;
    box-shadow: none;
    background: #fff;
    height: 30px;
    line-height: 30px;
    color: #000;
    .el-icon-time {
      width: 0;
      &::before {
        content: '';
        position: absolute;
        right: 10px;
        top: 6px;
        width: 15px;
        height: 15px;
        background: url('../../../../../public/img/WindTurbine/dropIcon.png') 100% 100%;
      }
    }
    .el-range__close-icon {
      line-height: 20px;
    }
  }
  .el-range-editor.is-disabled input {
    padding-left: 0px;
  }
  .el-date-editor .el-range-input {
    color: #000;
    width: 43%;
  }
  .el-date-editor .el-range-separator {
    color: #000;
    width: 8%;
  }
  .date_pop_model {
    ::v-deep .el-input__inner {
      color: #999;
      border: 1px solid #dcdfe6;
      background: #fff;
      box-shadow: none;
    }
    ::v-deep .el-date-range-picker__content.is-left {
      border-right: none !important;
    }
  }
}

.filter_block {
  background: linear-gradient(270deg, #0e496c 82%, rgba(14, 73, 108, 0) 100%);
  padding-left: 10px !important;

  .el-range-editor.el-input__inner {
    background: white;
    border-color: transparent;
    margin-left: 35px;
    width: 80px;
    float: right;
  }

  .el-range-editor.el-input__inner .el-icon-time::before {
    content: '\e78f';
    background: none;
    top: -1px;
  }

  .el-date-editor .el-range-separator {
    color: white;
  }

  .block_btn {
    display: inline-block;
    height: 30px;
    width: 30px;
    background: #0e496c;
    border-radius: 5px;
    margin-right: 5px;

    .func_btn {
      background: transparent;
      border: none;
      padding: 0;
      color: #fff;
      line-height: 28px;
      height: 30px;
      padding: 5px 5px 5px 5px;

      &:hover {
        background: #4c84b3;
      }

      img {
        float: left;
      }
    }

    .func_btn_active {
      background: #4c84b3;
    }

    .func_btn_none {
      background: #0e496c !important;
      border: none;
      padding: 0;
      color: #fff;
      line-height: 28px;
      height: 30px;
      padding: 5px 5px 5px 5px;

      &:hover {
        background: #0e496c;
      }

      img {
        float: left;
      }
    }
  }

  .data_picker {
    display: inline-block;
    background: white;
    color: black;
    padding-right: 5px;
    border-radius: 5px;
    position: relative;
    margin-right: -90px;
    z-index: 10;
    height: 30px;
    line-height: 30px;

    .data_picker_btn {
      font-size: 12px;
      cursor: default;
      text-align: center;
      display: table;
      padding-right: 4px;
      padding-left: 5px;
    }

    .el-icon-time {
      margin: 7px 5px 3px 15px;
      float: left;
    }
  }

  .time_filter_block {
    margin-left: 60px;
  }
}
// .mini_filter_block {
//   width: 100%;
//   height: 28px;
//   border-radius: 5px;
//   // padding: 3px 10px 3px 80px !important;
//   background: linear-gradient(270deg, #0e496c 82%, rgba(14, 73, 108, 0) 100%);
//   .filter_block_timePic {
//     float: left;
//     margin: 5px 10px 0 0;
//   }
//   .working_block {
//     color: #fff;
//     float: left;
//     margin-left: 10px;
//   }
//   .el-range-editor {
//     width: 340px;
//     float: left;
//     border-radius: 5px;
//     cursor: pointer;
//   }
//   .el-range-editor.el-input__inner {
//     border: 1px solid #255873;
//     box-shadow: none;
//     background: #fff;
//     height: 30px;
//     line-height: 30px;
//     color: #000;
//     .el-icon-time {
//       width: 0;
//       &::before {
//         content: '';
//         position: absolute;
//         right: 10px;
//         top: 6px;
//         width: 15px;
//         height: 15px;
//         background: url('../../../../../public/img/WindTurbine/dropIcon.png') 100% 100%;
//       }
//     }
//     .el-range__close-icon {
//       line-height: 20px;
//     }
//   }
//   .el-date-editor .el-range-input {
//     color: #000;
//     width: 43%;
//   }
//   .el-date-editor .el-range-separator {
//     color: #000;
//     width: 8%;
//   }
//   .date_pop_model {
//     ::v-deep .el-input__inner {
//       color: #999;
//       border: 1px solid #dcdfe6;
//       background: #fff;
//       box-shadow: none;
//     }
//     ::v-deep .el-date-range-picker__content.is-left {
//       border-right: none !important;
//     }
//   }
// }
</style>
