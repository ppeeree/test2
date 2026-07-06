<template>
  <div class="content_fixedBox" v-if="isVisible">
    <div class="content_title">
      <!-- <img :src="imgSrc" alt="comp" /> -->
      <i :class="['icon', 'local', 'local-' + paramsObj.compCode]" style="font-size: 18px"></i>
      {{ title }}
      <span @click="closeModal">
        <i class="el-icon-close"></i>
      </span>
    </div>
    <div class="content_body">
      <div class="content_body_title">
        <el-cascader
          ref="cascader"
          :options="options"
          :props="props"
          placeholder="信号类型 / 波形类型 / 频带类型"
          collapse-tags
          filterable
          clearable
          debounce="800"
          @change="handleCheckChange"
          v-model="selectedValue"
        ></el-cascader>
        <!--  :filter-method="getFilterResult" -->
      </div>
      <div class="eigenvalueList">
        <div
          type=""
          v-for="item in eigenList"
          :key="item.eigenValueId"
          :class="{
            unit_btn: true,
            unit_btn_danger: item.eigenstate == 'danger',
            unit_btn_warning: item.eigenstate == 'warning',
            unit_btn_attention: item.eigenstate == 'attention',
            unit_btn_active: item.eigenValueId == trendParams.eigenValueId
          }"
          @click="getTrend(item, $event)"
        >
          <span class="unit_btn_name">{{ item.eigenValueName }}</span>
          <el-row style="width: 100px">
            <el-col :span="16" style="font-size: 18px">{{ item.eigenValue }}</el-col>
            <el-col :span="8" style="font-size: 12px">{{ item.eigenValueUnit }}</el-col>
          </el-row>
        </div>
      </div>
    </div>
    <!-- <div class="content_body_oil">
      <div v-for="item in oilList" :key="item.title" class="oil_style">
        <div class="oil_style_title">{{ item.title }}</div>
        <div class="oil_style_content">
          <div v-for="i in item.params" :key="i.name" @click="getTrend(i, $event)">
            {{ i.name }}:
            <span>{{ i.value }}</span>
            {{ i.unit }}
          </div>
        </div>
      </div>
    </div> -->
  </div>
</template>
<script>
import { getAllEvListTreeApi, getOptions } from '@/api/WindTurbine/CenterPartAPI.js'
import { entityPartEnum } from '@/util/constant'

export default {
  props: {
    trendParams: {
      type: Object,
      require: true
    }
  },
  data() {
    return {
      entityPartEnum,
      isVisible: false,
      eigenList: [],
      selectedValue: '',
      paramsObj: {},
      title: '',
      value: [],
      imgSrc: '',
      props: { multiple: true, label: 'name', value: 'code', children: 'eigenList' },
      options: [
        {
          value: 1,
          label: '全部'
        },
        {
          value: 17,
          label: '加速度',
          children: [
            {
              value: 18,
              label: '时域',
              children: [{ value: 19, label: '通频' }]
            }
          ]
        }
      ]
      /* oilList: [
        {
          title: '粘度',
          params: [
            {
              name: '运动粘度1',
              value: '233',
              unit: 'mm^2'
            },
            {
              name: '运动粘度2',
              value: '233',
              unit: 'mm^2'
            },
            {
              name: '运动粘度3',
              value: '233',
              unit: 'mm^2'
            }
          ]
        },
        {
          title: '磨粒数',
          params: [
            {
              name: '运动粘度4',
              value: '233',
              unit: 'mm^2'
            },
            {
              name: '运动粘度5',
              value: '233',
              unit: 'mm^2'
            },
            {
              name: '运动粘度6',
              value: '233',
              unit: 'mm^2'
            }
          ]
        }
      ] */
    }
  },
  mounted() {
    //  this.getInitOption()
  },
  methods: {
    getInitOption() {
      getOptions().then(res => {
        if (res.data.code == 200) {
          this.options = res.data.data
        }
      })
    },
    /* getFilterResult(node, keyword) {
      console.log(node)
      this.selectedValue = keyword
      this.getAllEvList({ search: keyword })
    }, */
    init(params) {
      this.title = params.measlocName
      this.paramsObj = params
      this.getInitOption()
      this.isVisible = true
      this.getAllEvList({})
    },
    getAllEvList(params) {
      getAllEvListTreeApi({ ...params, measlocId: this.paramsObj.measlocId }).then(res => {
        if (res.data.code === 200) {
          this.eigenList = res.data.data
        }
      })
    },
    handleCheckChange(data) {
      this.selectedValue = data
      if (data.length) {
        let selected = []
        data.forEach(element => {
          element.length == 3 && selected.push(element[2])
        })
        let selectedStr = selected.join(',')
        this.getAllEvList({ lev1Code: selectedStr })
      } else {
        this.getAllEvList({})
      }
    },
    getTrend(data, e) {
      this.$emit('clickValue', { ...data, ...this.paramsObj }, e)
    },
    closeModal() {
      this.isVisible = false
    }
  }
}
</script>
<style lang="scss" scoped>
.content_fixedBox {
  position: absolute;
  right: 20px;
  top: 200px;
  width: 360px;
  height: calc(100% - 300px);
  background: #26334d;
  border-radius: 10px;
  border: 1px solid #a3b0db;
  z-index: 99;
  padding: 5px 0;
  .content_title {
    width: 100%;
    height: 40px;
    line-height: 40px;
    color: #fff;
    padding: 0 15px;
    background: #26334d;
    border-bottom: 1px solid #a3b0db;
    img {
      float: left;
      margin-right: 8px;
      margin-top: 8px;
    }
    span {
      font-size: 24px;
      font-weight: bolder;
      cursor: pointer;
      float: right;
      margin-right: 15px;
    }
  }
  :deep(.el-cascader){
    width: 100%;
    // border: 1px solid #546992;
    .el-input {
      background: transparent;
    }
    .el-input__inner {
      background: #061932 !important;
      border: 1px solid #546992;
      box-shadow: none;
      color: #c8d1ed;
      border-radius: 4px;
    }

    .el-cascader__search-input {
      background: transparent;
      color: #c8d1ed;
    }
  }
  .content_body_oil {
    padding: 5px 10px;
    height: auto;
    max-height: calc(100% - 50px);
    overflow-x: hidden;
    overflow-y: auto;
    .oil_style {
      width: 100%;
      height: auto;
      color: #fff;
      margin-top: 8px;
      .oil_style_title {
        width: 100%;
        font-size: 14px;
      }
      .oil_style_content {
        display: flex;
        flex-direction: row;
        justify-content: space-between;
        flex-flow: row wrap;
        div {
          display: inline-block;
          background: #3a455a;
          padding: 8px 15px;
          font-size: 12px;
          margin: 5px 0;
          border-radius: 3px;
          cursor: pointer;
          span {
            font-size: 14px;
            margin: 0 5px;
            font-weight: 700;
          }
        }
      }
    }
  }
  .content_body {
    padding: 5px 10px;
    height: calc(100% - 40px);
  }
  .eigenvalueList::-webkit-scrollbar {
    width: 6px;
  }
  .eigenvalueList::-webkit-scrollbar-track {
    background-color: #3e5369;
  }
  .eigenvalueList::-webkit-scrollbar-thumb {
    box-shadow: inset 0 0 6px #d2e5f1;
  }
  .eigenvalueList {
    width: 100%;
    max-height: calc(100% - 50px);
    height: auto;
    overflow-x: hidden;
    overflow-y: auto;
    margin-top: 5px;
    .unit_btn {
      width: 90%;
      height: 30px;
      line-height: 30px;
      border-radius: 2px;
      background: #3a455a;
      margin: 8px auto;
      color: #fff;
      padding: 0 10px;
      display: flex;
      flex-direction: row;
      justify-content: space-between;
      cursor: pointer;
      &:hover {
        border: 2px solid #0bffe7;
      }
      .unit_btn_name {
        font-size: 12px;
      }
      // b {
      //   font-size: 18px;
      //   span {
      //     font-size: 12px;
      //   }
      // }
    }
    .unit_btn_active {
      border: 2px solid #0bffe7;
    }
    .unit_btn_danger {
      background: rgba(250, 81, 81, 0.6);
    }
    .unit_btn_attention {
      background: rgba(255, 213, 0, 0.6);
    }
    .unit_btn_warning {
      background: rgba(255, 107, 14, 0.6);
    }
  }
}
</style>
