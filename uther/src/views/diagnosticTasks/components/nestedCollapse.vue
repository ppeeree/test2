<!-- NestedCollapse.vue -->
<template>
  <div class="analysis_process">
    <h3>分析记录</h3>
    <div class="analysis_content">
      <el-collapse v-if="treeData.length" v-model="collapseExpanded">
        <el-collapse-item v-for="item in treeData" :key="item.label" :name="item.label">
          <template slot="title">
            <div style="display: flex; flex-direction: row; align-items: center">
              <!--  <span v-if="handleFormateTitle(item)" class="formate_title">{{
                handleFormateTitle(item)
              }}</span>
              <i
                v-else
                :class="[
                  'icon',
                  'local',
                  `local-${
                    !isNaN(parseFloat(entityPartEnum[item.label].slice(-1)))
                      ? entityPartEnum[item.label].slice(0, -1)
                      : entityPartEnum[item.label]
                  }`
                ]"
              ></i> -->
              <span>{{ item.label }}</span>
            </div>
          </template>
          <!--  <nested-collapse
            v-if="handleHideOrShow(item)"
            :items="item.measList"
            :name="name + '-' + index"
          /> -->
          <el-collapse-item v-for="(ii, i) in item.measList" :key="ii.label" :name="ii.label">
            <template slot="title">
              <div style="display: flex; flex-direction: row; align-items: center">
                <span>{{ ii.label }}</span>
              </div>
            </template>
            <div
              v-for="content in ii.children"
              :key="content.analyzerRecordId"
              class="content_collapse"
            >
              <el-image style="width: 100%" :src="content.image" />
              <div class="input_content">
                <div class="input_content_title">
                  分析描述：<span>{{ content.recordTime }}</span>
                </div>
                <el-input
                  type="textarea"
                  :key="content.analyzerRecordId"
                  :autosize="{ minRows: 1, maxRows: 2 }"
                  placeholder="请输入内容"
                  v-model="formData[content.analyzerRecordId]"
                  class="input_textarea"
                />
              </div>
            </div>
          </el-collapse-item>
        </el-collapse-item>
      </el-collapse>
      <div
        style="color: #999; width: 100%; height: auto; text-align: center; margin-top: 30%"
        v-else
      >
        无分析记录
      </div>
    </div>
  </div>
</template>

<script>
// import { entityPartEnum, typeImg } from '@/util/constant'
export default {
  name: 'nestedCollapse',
  props: {
    items: {
      type: Array,
      default: () => []
    },
    name: {
      type: String,
      default: '1'
    }
  },
  watch: {
    items: {
      handler(val) {
        // this.formData = this.extractChildren(val)
      }
    }
  },
  data() {
    return {
      originalData: [],
      treeData: [],
      formData: {},
      collapseExpanded: []
      // entityPartEnum
    }
  },
  mounted() {
    // this.formData = this.extractChildren(this.items)
  },
  methods: {
    dealWithData(dataList) {
      this.$set(this, 'formData', {})
      const newTreeData = dataList.reduce((acc, item) => {
        // 检查部件对象是否已存在
        let obj = acc.find(c => c.label === item.compName)
        if (!obj) {
          obj = { label: item.compName, measList: [] }
          acc.push(obj)
        }
        // 检查测点对象是否已存在
        let measLoc = obj.measList.find(s => s.label === item.measlocName)
        if (!measLoc) {
          measLoc = { label: item.measlocName, children: [] }
          obj.measList.push(measLoc)
        }
        // 将item添加到对应的测点对象里面
        measLoc.children.push(item)
        this.$set(this.formData, item.analyzerRecordId, item.description)
        return acc
      }, [])
      return newTreeData
    },
    initData(data) {
      this.treeData = []
      this.originalData = []
      this.collapseExpanded = []
      const { analyzerRecords } = data
      if (analyzerRecords.length == 0) {
        this.$set(this, 'formData', {})
        return
      }
      this.treeData = this.dealWithData(analyzerRecords)
      this.originalData = _.cloneDeep(analyzerRecords)
      let collapseExpanded = []
      analyzerRecords.forEach(item => {
        collapseExpanded.push(item.compName)
        collapseExpanded.push(item.measlocName)
      })
      this.collapseExpanded = [...new Set(collapseExpanded)]
    },
    changeDataSourse(idList) {
      let arr = []
      this.originalData.forEach(item => {
        if (idList.includes(item.analyzerRecordId)) {
          arr.push(item)
        }
      })
      this.treeData = this.dealWithData(arr)
    }
  }
}
</script>

<style lang="less" scoped>
.analysis_process {
  width: 100%;
  height: 100%;
  overflow: hidden;
  padding: 10px 15px;
  h3 {
    width: 100%;
    font-size: 14px;
  }
}
.analysis_content {
  flex: 1;
  background: #f6f6f6;
  margin-left: 10px;
  height: calc(100% - 25px);
  overflow-x: hidden;
  overflow-y: auto;
  // padding: 7px 48px 7px 23px;
  .dividing_line {
    width: 100%;
    margin-bottom: 7px;
  }
  .label_span_title {
    margin-right: 8px;
    margin-left: 10px;
  }
  .device_state_select {
    --inputTextColor: '#C0C4CC';
    ::v-deep .el-input__inner {
      // background: black;
      border: 1px solid #bebebe;
      color: var(--inputTextColor) !important;
    }
    ::v-deep .el-input__suffix-inner {
      display: none;
    }
  }

  .avue_crud_st {
    width: 1330px;
    ::v-deep .avue-crud__menu {
      min-height: 11px;
    }
    ::v-deep .el-table::before {
      background-color: transparent;
    }
    ::v-deep .inputColor {
      .el-input__inner {
        color: inherit;
      }
    }
  }
}
.content_collapse {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: space-around;
  padding: 10px;
  .input_content {
    display: flex;
    flex-direction: column;
    width: 100%;
    justify-content: space-around;
    margin: 5px 0;
    .input_content_title {
      font-size: 14px;
      font-weight: normal;
      letter-spacing: 0em;
      color: rgb(51, 49, 49);
      span {
        float: right;
        margin-right: 15px;
      }
    }

    .input_textarea {
      width: 100%;
      ::v-deep .el-textarea__inner {
        color: #000;
        background: #fff;
      }
    }
  }
}
::v-deep .el-collapse-item__header {
  height: 40px;
  font-size: 14px;
  font-weight: bold;
  color: #000;
  background: transparent;
}
::v-deep .el-collapse-item__content {
  background: transparent;
  .el-collapse-item__header {
    background: transparent;
  }
}
::v-deep .el-collapse-item__wrap {
  background: transparent;
}
.formate_title {
  font-weight: 400;
  font-size: 14px;
  color: #000;
}
::v-deep .el-image__inner {
  max-width: 100%;
  height: auto;
  width: auto;
}
</style>
