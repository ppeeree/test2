<template>
  <ul class="chart-common-tools">
    <template v-if="selectedChart !== 'track' && selectedChart !== 'distribution'">
      <li title="标记线" class="selectLi" style="position: relative">
        <i class="icon iconfont acharts-markline" @click="clickEvent('isShowMarkline')"></i>
        <div v-show="isShowMarkline" class="diagmodel" style="position: absolute">
          <p title="markline_min" @click="clickItem('min')">
            <i class="icon iconfont acharts-min"></i>最小值
          </p>
          <p title="markline_max" @click="clickItem('max')">
            <i class="icon iconfont acharts-max"></i>最大值
          </p>
          <p title="markline_avg" @click="clickItem('avg')">
            <i class="icon iconfont acharts-avg"></i>平均值
          </p>
          <p title="markline_mid" @click="clickItem('mid')">
            <i class="icon iconfont acharts-mid"></i>中位数
          </p>
          <p title="markline_VDI" @click="clickItem('VDI')">
            <i class="icon iconfont acharts-vdi"></i>报警线
          </p>
        </div>
      </li>
      <li class="filterWaveLi" title="波形点" @click="clickItem('wavepointer')">
        <i class="icon iconfont acharts-filter"></i>
      </li>
      <span class="gap"></span>
    </template>

    <li class="addNote" title="添加备注">
      <i class="icon iconfont acharts-addnote" @click="clickItem('addnote')"></i>
    </li>
    <li title="清空备注">
      <i class="icon iconfont acharts-clearnote" @click="clickItem('clearnote')"></i>
    </li>
    <span class="gap"></span>
    <li title="数据处理" class="selectLi" style="position: relative">
      <i class="icon iconfont acharts-download" @click="clickEvent('isShowDown')"></i>
      <div v-show="isShowDown" class="diagmodel" style="position: absolute">
        <p title="复制图片" @click="clickItem('copy')">
          <i class="icon iconfont acharts-copy"></i>图片复制
        </p>
        <p title="保存图片" @click="clickItem('downpic')">
          <i class="icon iconfont acharts-downpic"></i>图片下载
        </p>
        <p title="数据下载" @click="clickItem('datadown')">
          <i class="icon iconfont acharts-datadown"></i>数据下载
        </p>
      </div>
    </li>
    <li title="重置" @click="clickItem('reset')">
      <i class="icon iconfont acharts-reset"></i>
    </li>
    <span class="gap"></span>
    <li title="添加分析记录" @click="clickItem('addRecord')">
      <i class="icon el-icon-edit" style="font-size: 18px"></i>
    </li>
    <div class="note-detail" v-if="isShowNoteDiag">
      <el-input type="textarea" v-model="textareaContent" placeholder="请输入内容"></el-input>
      <p>
        <el-button size="mini" type="primary" @click="clickItem('confirm')">确定</el-button>
        <el-button size="mini" type="cancel" @click="clickItem('cancel')">取消</el-button>
      </p>
    </div>
  </ul>
</template>
<script>
export default {
  props: {
    optionConfig: {
      type: Object,
      default: () => {
        return {}
      }
    },
    selectedChart: {
      type: String,
      default: ''
    }
  },
  data() {
    return {
      textareaContent: '',
      isShowNoteDiag: false,
      isShowZoom: false,
      isShowMarkline: false,
      isShowDown: false
    }
  },
  updated() {},
  methods: {
    clickEvent(type) {
      let names = ['isShowDown', 'isShowZoom', 'isShowMarkline']
      names.forEach(item => {
        if (item === type) {
          this[item] = !this[item]
        } else {
          this[item] = false
        }
      })
    },
    clickItem(type) {
      this.isShowDown = false
      this.isShowZoom = false
      this.isShowMarkline = false
      if (type == 'cancel') {
        this.isShowNoteDiag = false
        return
      }
      if (type === 'addnote') {
        this.isShowNoteDiag = true
        return
      }
      if (type === 'confirm') {
        if (this.textareaContent.length) {
          this.isShowNoteDiag = false
        } else {
          return alert('请输入备注内容')
        }
      }
      this.$emit('operationChange', type, this.textareaContent)
    }
  }
}
</script>
<style lang="scss" scoped>
.chart-common-tools {
  float: left;
  margin-top: 5px !important;
  margin-left: calc(50% - 80px) !important;
  list-style: none;
  padding: 0;
  font-size: 12px;
  .gap {
    float: left;
    width: 1px;
    height: 20px;
    margin: 1px 5px;
    background: #777575;
  }
  li {
    list-style: none;
    text-align: center;
    line-height: 0;
    float: left;
    margin: 3px 2px;
    cursor: pointer;
    background: transparent;
    color: #444;

    i {
      margin: 0px 2px;
      line-height: 16px;
    }
  }
  .selectLi {
    position: relative;
    list-style: none;
    text-align: center;
    line-height: 0;
    float: left;
    margin: 3px 2px;
    cursor: pointer;
    background: transparent;
    border-radius: 5px;
  }

  .selectLi > .diagmodel {
    position: absolute;
    left: -50px;
    top: 25px;
    width: 120px;
    height: auto;
    overflow: hidden;
    background: #fff;
    z-index: 2;
    box-shadow: 0 2px 12px 0 rgba(0, 0, 0, 0.1);
  }

  .selectLi .diagmodel p {
    padding: 0 10px;
    margin: 3px 0;
    text-align: left;
    width: 100%;
    font-size: 12px;
    height: 24px;
    line-height: 24px;
  }

  .selectLi .diagmodel p:hover {
    background-color: #eee;
  }

  .selectLi .diagmodel p i {
    margin: 0 10px 0 5px;
    font-size: 12px;
  }

  .selectLi > .diagContent {
    position: absolute;
    left: -100px;
    top: 25px;
    width: 200px;
    height: auto;
    overflow: hidden;
    border: none;
    background: #fff;
    z-index: 2;
    line-height: 20px;
    box-shadow: 0 2px 12px 0 rgba(0, 0, 0, 0.1);
  }

  .selectLi .diagContent h3 {
    font-size: 12px;
    height: 24px;
    line-height: 24px;
    padding: 0 10px;
    background-color: #d1eaff;
    text-align: left;
    font-weight: normal;
  }

  .selectLi .diagContent p {
    text-align: center;
    padding: 0 10px;
    margin: 15px 0;
    text-align: left;
    width: 100%;
    font-size: 12px;
    height: auto;
    line-height: 24px;
  }

  .selectLi .diagContent p input {
    display: inline-block;
    width: 60px;
    padding: 3px 10px;
  }

  .selectLi .diagContent p span {
    display: block;
    color: red;
  }

  .selectLi .diagContent div {
    text-align: center;
    width: 100%;
    height: auto;
  }

  .selectLi .diagContent button {
    cursor: pointer;
    margin: 5px;
    background-color: #ffffff;
    border: 1px solid #ccc;
    padding: 5px 10px;
    font-size: 12px;
  }

  .selectLi .diagContent .ybNumConfirm {
    background-color: #409eff;
    color: #fff;
  }
  .activedLi {
    background: #86b5f1;
  }

  .note-detail {
    position: absolute;
    left: calc(50% - 150px);
    top: 100px;
    width: 200px;
    height: auto;
    overflow: hidden;
    border: none;
    background: #fff;
    z-index: 10000;
    line-height: 20px;
    padding: 10px;
    box-shadow: 0 2px 12px 0 rgba(0, 0, 0, 0.1);
    p {
      text-align: center;
      margin: 10px 0;
      button {
        margin: 0 5px;
      }
    }
  }
}
</style>
