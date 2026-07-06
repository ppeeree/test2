<template>
  <div class="event_deal_card">
    <div class="none_add_deal" v-if="part == 'noneAdd'">
      <span class="none_add_text">
        <span
          style="
            float: left;
            margin-left: calc(50% - 50px);
            line-height: 91px;
            height: 100%;
            cursor: pointer;
          "
          @click="switchCard('add')"
          ><img src="/img/WindTurbine/icon/eventAdd.png"
        /></span>
        <span style="font-size: 16px; margin-left: 10px">添加处理方式</span>
      </span>
    </div>
    <eventDealPart
      @switchCard="switchCard"
      v-else
      v-bind="$attrs"
    ></eventDealPart>
    <!-- </div> -->
  </div>
</template>

<script>
import eventDealPart from './eventDealPart.vue'
export default {
  components: { eventDealPart },
  data() {
    return {
      part: 'noneAdd',
      cardShow: 'show'
    }
  },

  mounted() {},
  methods: {
    // 切换卡片
    switchCard(name) {
      this.$emit('changeSwitchCard', name)
      this.part = name
    },
    closeDetailCard() {
      this.cardShow = 'noShow'
    }
  }
}
</script>

<style lang="less" scoped>
.event_deal_card {
  color: white;
  width: 100%;
  max-height: 335px;
  overflow-y: hidden;
  overflow-x: hidden;
  //状态为已处理
  .deal {
    margin-bottom: 10px;
  }
  //状态为未处理
  // .un_deal {
  //未处理样式
  .none_add_deal {
    margin-left: 15px;
    background: rgba(255, 255, 255, 0.1);
    color: white;
    height: 80px;
    width: 100%;
    .none_add_text {
      line-height: 80px;
      font-size: 14px;
    }
  }
  // }
}
</style>
