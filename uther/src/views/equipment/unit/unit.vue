<template>
  <div class="ip_border">
    <span class="ip_title">采集单元与机组IP匹配</span>
    <el-row class="ip_content" v-if="list.length">
      <el-col :span="6" style="text-align: center"
        ><span style="opacity: 0.5">机组</span>
        <div v-for="item in list" :key="item.name" class="ip_content_list">
          <el-col :span="14" style="text-align: right; padding-right: 4px" class="name">
            {{ item.name }}</el-col
          >
          <el-col :span="6" class="name">-</el-col>
        </div>
      </el-col>
      <el-col :span="8"
        ><span style="opacity: 0.5">输入IP</span>
        <el-input
          type="textarea"
          :rows="list.length"
          v-model="textarea"
          @input="ipChange"
          resize="none"
        ></el-input
      ></el-col>
    </el-row>
    <noData v-else firstText="请选择机组"></noData>
  </div>
</template>

<script>
import noData from '@/components/noData/index.vue'
export default {
  components: { noData },
  props: {
    list: {
      type: Array,
      default: () => []
    }
  },
  data() {
    return {
      repeatList: [],
      dialogVisible: false,
      textarea: ''
    }
  },
  watch: {
    list(val) {
      if (val.length == 0) {
        this.textarea = ''
      } else {
        this.ipLength(val, this.textarea)
      }
    },
    textarea(newval) {
      this.ipLength(this.list, newval)
    }
  },
  methods: {
    ipLength(windList, ipText) {
      const iplist = ipText.split('\n')
      if (iplist.length > windList.length) {
        const list = iplist.slice(0, windList.length)
        this.textarea = list.join('\n')
        // console.log('输入的长度大于机组长度', list.join('\n'))
      }
    },
    ipChange(val) {
      const list = val.split('\n')

      this.list.forEach((item, index) => {
        item.ip = list[index]
      })

      this.$emit('ipChange', this.list)
    },
    ipListValidate() {
      let auth = true

      let newarr = this.list.filter((item, index) => {
        for (let j = 0; j < this.list.length; j++) {
          if (item.ip == this.list[j].ip && index != j) {
            return item
          }
        }
      })

      let rule =
        /^((25[0-5]|2[0-4]\d|((1\d{2})|([1-9]?\d)))\.){3}(25[0-5]|2[0-4]\d|((1\d{2})|([1-9]?\d)))$/

      if (newarr.length > 0) {
        if (newarr[0].ip !== '') {
          this.$message.error('存在相同IP，请修改！')
          auth = false
        }
      } else if (newarr.length == 0) {
        let nameList = []
        this.list.forEach(item => {
          if (!rule.test(item.ip)) {
            nameList.push(item.name)
          }
        })
        if (nameList.length !== 0) {
          this.$message.error(nameList.join('、') + '机组IP格式不正确，请重新输入！')
          auth = false
        }
      }

      return auth
    }
  }
}
</script>

<style lang="less" scoped>
@import url('../component/commonStyle.less');
.ip_border {
  height: 200px;
  width: 780px;
  margin: 0 20px 22px 20px;
  border: 1px dashed rgba(72, 137, 217, 0.5);
  border-radius: 5px;
  .ip_title {
    height: 18px;
    width: 170px;
    text-align: center;
    position: relative;
    top: -10px;
    left: 29px;
    display: inline-block;
    background: rgba(4, 17, 33, 1);
  }
  .ip_content {
    // opacity: 0.5;
    overflow-y: auto;
    height: 180px;
    &::-webkit-scrollbar-thumb {
      border-radius: 0px;
      background: rgba(71, 86, 128, 0.5);
    }

    &::-webkit-scrollbar-track {
      border-radius: 0;
      background: rgba(0, 0, 0, 0.5);
    }
    .ip_content_list {
      .name {
        height: 28px;
        line-height: 28px;
        margin: 5px 0px;
        color: white;
      }
    }
    ::v-deep .el-textarea {
      .el-textarea__inner {
        line-height: 38px;
        padding-top: 0px;
      }
    }
  }
}
</style>
