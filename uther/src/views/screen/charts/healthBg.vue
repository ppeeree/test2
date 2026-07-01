<template>
  <div class="content">
    <span v-if="!!title">{{ title }}</span>
    <div class="content-img" :style="{ backgroundImage: handleBgOrFont.imgUrl }">
      <span :style="{ color: handleBgOrFont.fontColor }">{{ num }}</span>
    </div>
  </div>
</template>

<script>
export default {
  props: {
    num: {
      type: Number,
      default: 0
    },
    title: {
      type: String,
      default: ''
    }
  },
  computed: {
    handleBgOrFont() {
      let level = this.num
      if (level > 75.0) {
        return {
          imgUrl: `url('/img/screen/health/${1}.png')`,
          fontColor: '#36FA3C'
        }
      } else if (level > 50.0) {
        return {
          imgUrl: `url('/img/screen/health/${2}.png')`,
          fontColor: '#FFE604'
        }
      } else if (level > 25.0) {
        return {
          imgUrl: `url('/img/screen/health/${3}.png')`,
          fontColor: '#FF6B0E'
        }
      } else if (level >= 0.0) {
        return {
          imgUrl: `url('/img/screen/health/${4}.png')`,
          fontColor: '#FF0F0D'
        }
      }
    }
  }
}
</script>

<style lang="less" scoped>
@font-face {
  font-family: 'DS-DIGIB-2';
  src: url('../../WindTurbine/font/DS-DIGIB-2.ttf') format('truetype');
}
.content {
  display: flex;
  flex-direction: column;
  span:first-child {
    font-size: 10px;
    font-weight: 500;
    line-height: 30px;
    letter-spacing: 0em;
    color: #1fffff;
  }
  .content-img {
    background-image: url('/img/screen/health/1.png');
    //   background-color: red;
    background-size: 100%, 100%;
    background-repeat: no-repeat;
    width: 60.57px;
    height: 40.01px;
    display: flex;
    justify-content: center;
    align-items: center;
    span {
      font-family: 'DS-DIGIB-2';
      font-size: 40px;
      font-weight: bold;
    }
  }
}
</style>
