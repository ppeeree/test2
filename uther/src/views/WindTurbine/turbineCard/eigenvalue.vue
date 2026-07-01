<template>
  <div class="content">
    <!-- 标题 -->
    <div class="content_title" v-if="data.length">
      <span style="float: left; margin-top: 6px"
        ><img style="width: 20px" :src="requireCompentImg(title, level)"
      /></span>
      <span class="content_title_text">{{ title }}</span>
    </div>
    <!-- 内容 -->
    <div style="margin-left: 39px">
      <ul>
        <li v-for="(faultitem, faultindex) in data" :key="faultindex" style="margin-bottom: 10px">
          <!-- 部件损伤 -->
          <span class="damage_title">{{ faultitem.faultName }}</span>
          <!-- 严重等级 -->
          <span
            class="level_color"
            :style="{
              color: falutLevelColor[faultitem.faultLevel]
            }"
          >
            {{ compLevel[faultitem.faultLevel] }}
          </span>
          <!-- 小三角 -->
          <i
            v-if="thisIndex.indexOf(faultindex) != -1"
            class="el-icon-caret-bottom"
            @click="showDetailcard(faultindex)"
          ></i>
          <i v-else class="el-icon-caret-right" @click="showDetailcard(faultindex)"></i>
          <!-- 特征值框 -->
          <div
            v-show="thisIndex.indexOf(faultindex) != -1"
            v-for="(detailItem, detailIndex) in faultitem.measlocList"
            :key="detailIndex"
          >
            <span class="damage_title">
              <img src="/img/WindTurbine/littleSpot.png" class="damage_title_img" />
              {{ detailItem.measlocName }}
            </span>
            <!-- 特征值数字 -->
            <el-row style="width: 95%">
              <div v-for="(numItem, numIndex) in detailItem.evDatasList" :key="numIndex">
                <el-col
                  :span="numItem.eigenValueCode.length > 6 ? '17' : '12'"
                  class="data_right"
                  :style="{ background: changeItemBackgroundColor(numItem.eigenValueStatus) }"
                >
                  <span class="damage_unit">
                    {{ numItem.eigenValueName }}：
                    <span class="damage_num">{{ parseFloat(numItem.eigenValue).toFixed(2) }}</span>
                    {{ numItem.eigenValueUnit }}
                  </span>
                </el-col>
              </div>
            </el-row>
          </div>
        </li>
      </ul>
    </div>
  </div>
</template>

<script>
import { falutLevelColor } from '@/util/constant.js'
export default {
  props: {
    title: {
      type: String,
      default: '',
      require: true
    },
    data: {
      type: Array,
      default: () => {},
      require: true
    },
    level: {
      type: String,
      default: '',
      require: true
    }
  },
  data() {
    return {
      falutLevelColor,
      showDetail: 'none',
      //对数据进行分层处理
      faultNameArr: [],
      eigenData: [],
      eigenValueNameArr: [],
      valueNum: [],
      thisIndex: [],
      compLevel: {
        severe: '严重',
        moderate: '中等',
        mild: '轻微'
      },
      setNum: {
        danger: '3',
        warning: '2',
        attention: '1',
        normal: '0'
      },
      compIcon: {
        叶片: 'blade',
        主轴: 'mainbearing',
        发电机: 'alternator',
        齿轮箱: 'gearbox',
        塔基: 'towerbase'
      }
    }
  },
  inject: ['clickValueCard'],
  mounted() {
    // console.log('卡片数据', this.data)
  },
  methods: {
    //改变部件图片
    requireCompentImg(title, level) {
      let name = this.compIcon[title]
      let num = this.setNum[level]
      if (num !== undefined) {
        let string = name + num
        return require(`/public/img/compIcon/${string}.png`)
      }
    },
    showDetailcard(index) {
      if (this.thisIndex.indexOf(index) == -1) {
        this.thisIndex.push(index)
      } else {
        this.thisIndex = this.thisIndex.filter(item => item != index)
      }
    },
    changeItemBackgroundColor(level) {
      let color
      if (level == 'attention') {
        color = 'rgba(255, 230, 4, 0.5)'
      } else if (level == 'danger') {
        color = 'rgba(250, 81, 81, 0.5)'
      } else {
        color = 'rgba(71, 86, 128, 0.5)'
      }
      return color
    }
  }
}
</script>

<style lang="less" scoped>
li {
  list-style: unset;
}
.content {
  color: white;
  font-family: 'SourceHanSansCN';
  .content_title {
    color: white;
    padding: 10px 10px 5px 10px;
    font-size: 14px;
    img {
      float: left;
      margin-top: -3px;
    }
    .content_title_text {
      margin-left: 7px;
    }
  }
  .damage_title {
    font-size: 12px;
    line-height: 17px;
  }
  /* 等级颜色 */
  .level_color {
    position: absolute;
    right: 7%;
    font-size: 12px;
    font-family: 'SourceHanSansCN-blod';
  }
  .el-icon-caret-bottom {
    float: right;
    margin-right: 6px;
    cursor: pointer;
  }
  .el-icon-caret-right {
    float: right;
    margin-right: 6px;
    cursor: pointer;
  }
  .damage_title_img {
    width: 5px;
    height: 5px;
    margin-left: 7px;
    margin-right: 6px;
  }
  .data_right {
    height: 28px;
    margin-top: 5px;
    margin-left: 10px;
    padding-left: 5px;
  }
  .damage_unit {
    height: auto;
    font-size: 10px;
    line-height: 28px;
    text-align: center;
    .damage_num {
      height: auto;
      font-size: 14px;
      line-height: 28px;
      font-family: 'SourceHanSansCN-blod';
      font-weight: bold;
    }
  }
}
</style>
