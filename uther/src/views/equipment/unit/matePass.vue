<template>
  <div class="mate_pass">
    <el-row class="mate_title">
      <el-col :span="1">通道</el-col>
      <el-col :span="4">测点</el-col>
      <el-col :span="8">灵敏度系数【mv/ (m/s^2)】</el-col>
      <el-col :span="4">最低偏执电压(V)</el-col>
      <el-col :span="5">最高偏执电压(V)</el-col>
    </el-row>
    <div v-for="(item, index) in selectlist" :key="item.channelNumber" class="mate_item">
      <el-form ref="form" :model="item">
        <span>{{ item.channelNumber }}：</span>
        <el-form-item>
          <el-select
            v-model="item.measlocId"
            placeholder="请匹配测点"
            filterable
            clearable
            @change="idChange"
          >
            <el-option
              v-for="item in selectOption"
              :key="item.measlocId"
              :label="item.measlocName"
              :value="item.measlocId"
            >
            </el-option>
          </el-select>
        </el-form-item>
        <el-form-item>
          <el-input
            v-model="item.sensitivity"
            @input="itemChange"
            oninput="value=value.replace(/[^0-9.]/g,'')"
            :placeholder="index == 0 ? '' : '同上'"
          ></el-input>
        </el-form-item>
        <el-form-item>
          <el-input
            v-model="item.minvolt"
            @input="itemChange"
            oninput="value=value.replace(/[^0-9.]/g,'')"
            :placeholder="index == 0 ? '' : '同上'"
          ></el-input>
        </el-form-item>
        <el-form-item>
          <el-input
            v-model="item.maxvolt"
            @input="itemChange"
            oninput="value=value.replace(/[^0-9.]/g,'')"
            :placeholder="index == 0 ? '' : '同上'"
          ></el-input>
        </el-form-item>
      </el-form>
    </div>
  </div>
</template>

<script>
import { getMeasureListApi } from '@/api/equipment/unit'
export default {
  props: {
    turbineList: {
      type: Array,
      default: () => []
    }
  },
  data() {
    return {
      selectNumber: 16,
      selectOption: []
    }
  },
  computed: {
    selectlist() {
      let list = []
      for (let i = 1; i <= this.selectNumber; i++) {
        list.push({
          channelNumber: i,
          measlocCode: '',
          measlocId: '',
          sensitivity: '',
          minvolt: '',
          maxvolt: ''
        })
      }
      return list
    }
  },
  watch: {
    turbineList: {
      handler(val) {
        this.getMeasureList(val)
      }
    }
  },
  mounted() {},
  methods: {
    getMeasureList(list) {
      getMeasureListApi(list.join(',')).then(res => {
        if (res.data.code == 200) {
          this.selectOption = res.data.data
        }
      })
    },
    idChange() {
      this.$forceUpdate()
      this.selectlist.forEach(item => {
        let obj = this.selectOption.find(j => j.measlocId == item.measlocId)
        if (obj) {
          item.measlocCode = obj.code
        }
      })
      this.$emit('measureList', this.selectlist)
    },
    itemChange() {
      this.$forceUpdate()
      this.$emit('measureList', this.selectlist)
    },
    formValidate() {
      let auth = true
      let newarr = this.selectlist.filter((item, index) => {
        for (let j = 0; j < this.selectlist.length; j++) {
          if (item.measlocId == this.selectlist[j].measlocId && index != j) {
            return item
          }
        }
      })

      let first = this.selectlist[0]

      if (newarr.length > 0 && newarr[0].measlocId !== '') {
        this.$message.error('存在相同测点，请修改！')
        auth = false
      } else if (newarr.length > 0 && newarr.length == 16) {
        this.$message.error('至少输入一个通道！')
        auth = false
      } else if (first.maxvolt == '' || first.minvolt == '' || first.sensitivity == '') {
        this.$message.error('请输入:灵敏度系数/最低偏执电压/最高偏执电压！')
        auth = false
      }

      return auth
    }
  }
}
</script>

<style lang="less" scoped>
@import url('../component/commonStyle.less');
.mate_pass {
  padding: 20px;
  .mate_title {
    text-align: center;
    .el-col {
      margin: 0px;
    }
  }
  .mate_item {
    // display: inline-block;
    margin: 10px;
    span {
      width: 30px;
      display: inline-block;
      text-align: right;
    }
    // ::v-deep .el-input {
    //   width: 19%;
    // }
    ::v-deep .el-input__inner {
      height: 36px;
      width: 140px;
    }
  }
}
.el-col {
  margin-right: 11px;
  margin-bottom: 0px;
}
::v-deep .el-form {
  width: 100% !important;
  padding: 0px !important;
  .el-form-item {
    margin-right: 38px !important;
    .el-form-item__content {
      margin: 0px !important;
    }
  }
}
</style>
