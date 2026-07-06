<template>
  <transition name="fade">
    <div
      ref="loading"
      class="loading-tag"
      :class="{'show':isShow,noShow:!isShow} "
      v-show="isShow"
    >
      <div
        class="loading-gif"
        style="margin-bottom: 5px;"
      ><img
          :src="LoadingGif"
          width="100"
        ></div>
    </div>
  </transition>
</template>
<script>
import loadgingGif_dark from './loading_dark.gif'
import loadgingGif_light from './loading_light.gif'
import { mapGetters } from 'vuex'
export default {
  props: ['show'],
  data() {
    return {
      LoadingGif: ''
    }
  },
  computed: {
    ...mapGetters(['theme']),
    isShow: {
      get: function () {
        return this.show
      },
      set: function (newValue) {
        return newValue
      }
    }
  },
  created() {
    this.LoadingGif = parseInt(this.theme) === 1 ? loadgingGif_light : loadgingGif_dark
    // this.LoadingGif = loadgingGif
    // 设置初始化监听是否错误，错误即提示出来
    let that = this
    window.addEventListener('message', function (messageEvent) {
      if (messageEvent.data == 'loadging') {
        // 取消loading
        that.isShow = false
        return false
      }
      if (messageEvent.data == 'showLoading') {
        // 添加loading
        that.isShow = true
        return false
      }
    })
  },
  updated() {
  },
  mounted() {
  }
}
</script>
<style lang="scss" scoped>
.fade-enter-active,
.fade-leave-active {
  // transition: opacity 0.5s;
}
.fade-enter, .fade-leave-to /* .fade-leave-active below version 2.1.8 */ {
  opacity: 0;
}
div.show {
  opacity: 0.8;
  text-align: center;
  // -webkit-transition: all 1.5s;
  // -moz-transition: all 1.5s;
  // transition: all 1.5s;
}
div.loading-tag {
  text-align: center;
  position: absolute;
  width: 100%;
  height: 100%;
  left: 0;
  right: 0;
  bottom: 0;
  top: 0;
  // background: #242c49;
  z-index: 999;
  opacity: 0.8;
  display: flex;
  // flex: 1;
  flex-direction: column;
  align-items: center;
  justify-content: center;
}
div.noShow {
  // -webkit-transition: all 1.5s;
  // -moz-transition: all 1.5s;
  // transition: all 1.5s;
  opacity: 0;
}
</style>

