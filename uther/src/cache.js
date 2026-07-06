import store from './store'

export default {
  beforeRouteEnter(to, from, next) {
    next(() => {
      const avueView = document.getElementById('avue-view')
      if (avueView && to.meta.savedPosition) {
        avueView.scrollTop = to.meta.savedPosition
      }
    })
  },
  beforeRouteLeave(to, from, next) {
    const avueView = document.getElementById('avue-view')
    if (from && from.meta.keepAlive) {
      if (avueView) {
        from.meta.savedPosition = avueView.scrollTop
      }
      const result = this.$route.meta.keepAlive === true && store.state.tags.tagList.some(ele => {
        return ele.value === this.$route.fullPath
      })
      if (this.$vnode && !result) {
        from.meta.savedPosition = 0
      }
    }
    next()
  },
}
