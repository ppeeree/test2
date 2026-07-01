<template>
  <div class="value_card">
    <div v-for="(item, index) in valueCardArr" :key="index">
      <common-card
        :id="item.evCardTitle"
        class="value_card_style"
        :style="{ top: item.cardTop + 'px', left: item.cardLeft + 'px' }"
        v-bind="$attrs"
        v-on="$listeners"
        :cardInfo="item"
        :styleConfig="handlerCardStyle(item.evCardTitle)"
      >
        <template slot="content">
          <PolarCanvas
            v-if="item.isShowCircle"
            :dataList="item.evList"
            :warningValue="item.evThreshold"
            :towerCurrentComp="handlerCircleStyle(item.evCardTitle)"
            :isShowRadar="item.isShowRadar"
          />
        </template>
      </common-card>
    </div>
  </div>
</template>

<script>
import commonCard from './commonCard.vue'
import PolarCanvas from './distributionCircle.vue'

export default {
  components: {
    commonCard,
    PolarCanvas
  },
  props: {
    currentComp: {
      type: String,
      default: ''
    }
  },
  inject: ['getCardPosition'],
  data() {
    return {
      valueCardArr: [] // 特征值数组
    }
  },
  created() {},
  mounted() {},
  methods: {
    // 1、处理特征值数据
    getEigenValueData(arr) {
      this.valueCardArr = arr.map(i => {
        const { cardTop, cardLeft } = this.getCardPosition(i.evCardPosition)
        return {
          ...i,
          cardTop,
          cardLeft
        }
      })
      let positionArr = arr.map(i => {
        const { evSpotPosition, evSummaryStatus, evCardTitle, evCardTitleIconCode } = i
        return {
          title: evCardTitle,
          state: evSummaryStatus,
          spot: evSpotPosition,
          cardId: evCardTitle,
          spotId: evCardTitle + evCardTitleIconCode
        }
      })
      this.$emit('allValueList', this.currentComp, positionArr)
    },
    // 3.1、获取卡片样式
    handlerCardStyle(name) {
      const valueCardStyle = [
        { name: 'B', boxWidth: '420', colCount: '2', maxHeight: '', isNameCircle: false },
        { name: '变桨', boxWidth: '350', colCount: '2', maxHeight: '', isNameCircle: false },
        { name: '齿轮箱', boxWidth: '220', colCount: '1', maxHeight: '150', isNameCircle: false },
        { name: '发电机', boxWidth: '220', colCount: '1', maxHeight: '150', isNameCircle: false },
        { name: '主轴', boxWidth: '220', colCount: '1', maxHeight: '150', isNameCircle: false },
        { name: '偏航', boxWidth: '350', colCount: '2', maxHeight: '', isNameCircle: false },
        { name: '塔顶', boxWidth: '380', colCount: '2', maxHeight: '', isNameCircle: false },
        { name: '钢索', boxWidth: '540', colCount: '4', maxHeight: '', isNameCircle: true },
        { name: '间隙', boxWidth: '350', colCount: '2', maxHeight: '', isNameCircle: true },
        { name: '螺栓', boxWidth: '350', colCount: '3', maxHeight: '', isNameCircle: true },
        { name: '塔基', boxWidth: '320', colCount: '2', maxHeight: '', isNameCircle: false }
      ]
      let obj = valueCardStyle.find(j => name.includes(j.name))
      return obj ? obj : {}
    },
    // 3.2、获取圆环形式
    handlerCircleStyle(name) {
      let text = ''
      if (name.includes('钢索')) {
        text = 'steel'
      } else if (name.includes('法兰')) {
        text = 'flang'
      } else if (name.includes('偏航')) {
        text = 'yaw'
      } else if (name.includes('变桨')) {
        text = 'pitch'
      }
      return text
    }
  }
}
</script>

<style lang="less" scoped>
.value_card {
  .value_card_style {
    position: absolute;
    display: inline-block;
  }
}
</style>
