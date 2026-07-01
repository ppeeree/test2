<template>
  <div class="value_card_border" :style="{ width: styleConfig.boxWidth + 'px' }">
    <div class="value_card">
      <!-- 标题 -->
      <div
        class="value_title"
        :style="{
          color: levelColor[cardInfo.evSummaryStatus] || 'white'
        }"
      >
        <i
          :class="[
            'icon',
            'local',
            `local-${
              !isNaN(parseFloat(cardInfo.evCardTitleIconCode.slice(-1)))
                ? cardInfo.evCardTitleIconCode.slice(0, -1)
                : cardInfo.evCardTitleIconCode
            }`
          ]"
          style="position: relative; top: -5px; margin-right: 5px; font-weight: normal"
        ></i>
        <el-tooltip
          effect="dark"
          :content="cardInfo.evCardTitle"
          placement="top"
          v-if="cardInfo.evCardTitle.length * 15 > styleConfig.boxWidth - 35"
        >
          <span class="value_title_content">{{ cardInfo.evCardTitle }}</span>
        </el-tooltip>
        <span v-else class="value_title_content">{{ cardInfo.evCardTitle }}</span>
      </div>
      <!-- 详细特征值 -->
      <div style="padding-bottom: 5px">
        <!-- 特征值 -->
        <el-row
          v-if="activeList.length"
          class="value_content_box"
          :style="{ maxHeight: styleConfig.maxHeight + 'px' }"
        >
          <el-col
            :span="24 / styleConfig.colCount"
            style="padding: 0 4px"
            v-for="(item, index) in activeList"
            :key="index"
            @mouseenter.native="mouseInterValue = item.evID"
            @mouseleave.native="mouseInterValue = ''"
            @click.native="clickValueItem(cardInfo, item, $event, activeList)"
          >
            <p class="value_content" :style="setVlaueBorderColor(item)">
              <span
                v-if="cardInfo.isShowCircle && cardInfo.isShowRadar"
                class="circel_num"
                :title="item.circleName"
                :style="{
                  color: levelColor[item.evStatus],
                  borderColor: levelColor[item.evStatus] || 'white'
                }"
                >{{ item.circleName }}</span
              >
              <template v-else>
                {{ item.evName }}
              </template>
              <!--  //needSci(item.evValue) ? item.evValue.toExponential() : item.evValue -->
              <span
                :style="{
                  color: levelColor[item.evStatus]
                }"
                >{{ item.evCode == 'BAF' ? Math.round(item.evValue) : keep4(item.evValue) }}</span
              >
              <b class="unit"> {{ item.evUnit }}</b>
            </p>
          </el-col>
        </el-row>
        <noData v-else style="height: 100px"></noData>
        <!-- 倾角分布图 -->
        <div class="tower_basice" v-show="cardInfo.evCardTitle.indexOf('塔基') !== -1">
          <span
            class="tower_base_btn"
            @click="showAnylizeCard(activeList, $event, 'dip')"
            :style="{
              background: isAnylizeCard
                ? 'linear-gradient(0deg, #008E8E 0%, #017C7C 100%)'
                : 'linear-gradient(0deg, #4D5869 0%, #434D5D 100%)',
              borderColor: isAnylizeCard ? '#1FFFFF' : '#4087E7'
            }"
          >
            <span class="tower_btn_text">塔基倾角分布图</span></span
          >
        </div>
        <slot name="content"></slot>
      </div>
    </div>
  </div>
</template>

<script>
import { levelColor } from '@/util/constant.js'
import noData from '@/components/noData/index.vue'

export default {
  components: {
    noData
  },
  inject: ['clickValueCard', 'closeDipModel'],
  props: {
    styleConfig: {
      type: Object,
      require: true,
      default: () => {}
    },

    cardInfo: {
      type: Object,
      require: true,
      default: () => {}
    },
    isStopTimer: {
      type: Boolean,
      default: false
    },
    isDipTrendShow: {
      type: Boolean,
      require: true
    },
    trendParams: {
      type: Object,
      require: true
    }
  },
  data() {
    return {
      isShowMore: false,
      levelColor,
      isAnylizeCard: false, //是否倾角分布图
      mouseInterValue: '' //鼠标移入的特征值
    }
  },
  watch: {
    isDipTrendShow: {
      handler(val) {
        if (!val) {
          this.isAnylizeCard = false
        }
      }
    }
  },
  computed: {
    activeList() {
      let list = this.cardInfo.evList.filter(i => i.evID)
      return list
    }
  },
  mounted() {},
  destroyed() {
    this.isAnylizeCard = false
  },
  methods: {
    // 点击特征值
    clickValueItem(obj, item, event, activeList) {
      // console.log('点击特征值', obj, activeList)
      this.clickValueCard({ ...obj, ...item }, event)
    },
    //点击倾角分布图
    showAnylizeCard(dipList, e, name) {
      this.isAnylizeCard = !this.isAnylizeCard
      if (this.isAnylizeCard) {
        this.clickValueCard(dipList, e, name)
      } else {
        this.closeDipModel()
      }
    },

    //样式
    setVlaueBorderColor(item) {
      return {
        borderColor:
          this.mouseInterValue == item.evID ||
          (this.trendParams &&
            this.trendParams.find(j => j.evID == item.evID) &&
            this.isStopTimer == true)
            ? '#0BFFE7'
            : '#4d5869'
      }
    },
    // 只要小数点后面 ≥5 位就返回 true
    needSci(n) {
      return /\.\d{5,}/.test(n.toString())
    },
    keep4(num) {
      // 20260325 修改为4位小数
      const str = String(num)
      const pos = str.indexOf('.')
      // 没有小数点，或小数位 ≤4，直接返回
      if (pos === -1 || str.length - pos - 1 <= 4) return str
      return Number(num).toFixed(4)
    }
  }
}
</script>

<style lang="less" scoped>
.value_card_border {
  border: 2px solid #4d5869;
  position: relative;
  backdrop-filter: blur(5px);

  .value_card {
    overflow: hidden;
    margin: 3px;
    background-color: rgba(77, 88, 105, 0.5);
    .value_title {
      height: 37px;
      font-size: 14px;
      font-weight: bold;
      line-height: 37px;
      background-color: rgba(77, 88, 105, 0.5);
      padding-left: 5px;
      border-bottom: 2px solid #4d5869;
      .value_title_content {
        width: calc(100% - 35px);
        display: inline-block;
        height: 30px;
        text-overflow: ellipsis;
        overflow: hidden;
        white-space: nowrap;
      }
      .fold_button {
        color: white;
        height: 25px;
        width: 25px;
        display: inline-block;
        background: rgba(30, 38, 52, 0.5);
        border-radius: 4px;
        cursor: pointer;
        line-height: 25px;
        float: right;
        margin-right: 5%;
        margin-top: 5px;
        .el-icon-caret-bottom {
          margin-left: 5px;
        }
        .el-icon-caret-top {
          margin-left: 5px;
        }
      }
    }
    .value_content_box {
      width: 100%;
      overflow-y: auto;
      overflow-x: hidden;
      margin-top: 3px;
      // padding: 0 2px;
    }
    .value_content {
      color: white;
      height: 35px;
      /*  display: inline-block; */
      background-color: rgba(77, 88, 105, 0.3294);
      border: 2px solid #4d5869;
      font-size: 10px;
      // margin: 5px 2px 5px 0px;
      line-height: 27px;
      // padding: 0 2px;
      cursor: pointer;
      position: relative;
      text-align: center;
      .circel_num {
        display: inline-block;
        width: 23px;
        height: 23px;
        border-radius: 50%;
        border: 1px solid;
        position: relative;
        font-size: 12px;
        font-weight: bolder;
        text-align: center;
        line-height: 23px;
        white-space: nowrap; /* 不换行 */
        overflow: hidden; /* 溢出部分裁掉 */
        text-overflow: ellipsis; /* 用 … 代替被裁掉的文本 */
        margin-bottom: 3px;
        vertical-align: bottom;
      }
      span {
        font-size: 14px;
        font-weight: 700;
        margin-left: 2px;
        margin-right: 0px;
      }
      .vdiIcon {
        position: absolute;
        right: 5px;
        top: 2px;
      }
      /*  .unit {
        position: absolute;
        right: 2px;
        bottom: 1px;
      } */
      // &:hover {
      //   border-color: rgba(31, 255, 255, 1);
      // }
    }
    .tower_basice {
      border-top: 2px solid black;
      text-align: center;
      height: 45px;
      .tower_base_btn {
        color: white;
        display: inline-block;
        height: 28px;
        width: 155px;
        margin: 11px 0 9px 0;
        border-radius: 5px;
        cursor: pointer;
        text-align: center;
        border-bottom: 2px solid;
        .tower_btn_text {
          line-height: 28px;
          font-size: 12px;
        }
      }
    }
    .more_value_btn {
      height: 12px;
      background-color: rgba(77, 88, 105, 0.6);
      border: 1px solid rgba(77, 88, 105, 0.6);
      border-radius: 2px;
      margin: 2px 5px;
      cursor: pointer;
      .el-icon-more {
        height: 7px;
        color: white;
        line-height: 7px;
        position: relative;
        top: -5px;
        left: calc(50% - 9px);
      }
      &:hover {
        border-color: #1fffff;
      }
    }
  }
  .fault_card {
    margin: 5px 3px 3px 3px;
    color: white;
    background-color: rgba(77, 88, 105, 0.5);
    border-top: 2px solid #4d5869;
    padding: 5px 8px;
    p {
      font-size: 14px;
      font-weight: bold;
      line-height: 20px;
    }
    ::v-deep .el-collapse {
      border-color: transparent;
      .el-collapse-item__header {
        background-color: transparent;
        color: white;
        height: 20px;
        border-color: transparent;
      }
      .el-collapse-item__wrap {
        border-color: transparent;
      }
    }
    .el-col {
      text-overflow: ellipsis;
      overflow: hidden;
      white-space: nowrap;
      height: 20px;
      padding: 0px;
      margin: 0px;
      line-height: 20px;
    }
  }
  .fold_button {
    height: 25px;
    width: 25px;
    display: inline-block;
    background: rgba(30, 38, 52, 0.5);
    border-radius: 4px;
    cursor: pointer;
    line-height: 25px;
    margin-right: 13px;
    margin-top: 5px;
    .el-icon-caret-bottom {
      margin-left: 5px;
    }
    .el-icon-caret-top {
      margin-left: 5px;
    }
  }
}
</style>
