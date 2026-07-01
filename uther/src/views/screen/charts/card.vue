<template>
  <div class="box-1">
    <div class="vertical" ref="UI">
      <span class="w-front">{{ title }}</span>
    </div>
    <div class="num-layout">
      <span class="font-size">{{ num }}</span>
    </div>
  </div>
</template>

<script>
export default {
  props: {
    title: {
      type: String,
      default: '',
      required: true
    },
    num: {
      type: Number,
      default: 0,
      required: true
    },
    type: {
      type: String,
      default: 'blue'
    }
  },
  mounted() {
    this.setUI()
  },
  computed: {},
  watch: {
    title: {
      handler: function (val) {
        if (val === '无状态') {
          this.$nextTick(() => {
            this.$refs.UI.style.setProperty('--marginLeft', '-36px')
            this.$refs.UI.style.setProperty('--color', '#8D8D8D')
          })
        } /* else {
          this.setUI()
        } */
      },
      immediate: true
    }
  },
  updated() {
    this.setUI()
  },
  methods: {
    setUI() {
      this.$refs.UI.style.setProperty('--color', this.type) //给变量赋值
    }
  }
}
</script>

<style lang="less" scoped>
@import url('../style/minxs.less');
.box-1 {
  //   position: absolute;
  //   left: 64px;
  .vertical {
    --color: #4966d2;
    --marginLeft: -23px;

    text-align: center;
    position: relative;
    left: -10px;

    &::after {
      content: '';
      width: 9px;
      height: 23px;
      background: var(--color);
      position: absolute;
      border-radius: 3px;
      margin-left: var(--marginLeft);
    }
    .w-front {
      position: relative;
      left: 15px;
      font-size: 0.7rem !important;
    }
  }
  .num-layout {
    // width: 100px;
    text-align: center;
    margin-top: 20px;
    .font-size {
      font-family: HarmonyOS_Sans_SC_Medium;
      font-size: 1.5rem;
      font-weight: 500;
      line-height: 22px;
      letter-spacing: 0px;
      color: #ffffff;
    }
  }
  .color {
    margin-left: 5px;
  }
}
</style>
