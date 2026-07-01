<template>
  <!-- style="height: auto; marginheight: 90vh; overflow: auto" -->
  <!-- @scroll="handleScroll" -->
  <div class="list-view" :style="{ height: viewHeight }">
    <div class="list-view-phantom" :style="{ height: contentHeight }"></div>
    <div ref="content" class="list-view-content">
      <div
        v-for="(item, index) in data"
        :key="index"
        :style="{ height: itemHeight + 'px' }"
        class="list-view-item"
      >
        <slot :item="item" :index="index" />
      </div>
    </div>
  </div>
</template>

<script>
import throttle from 'lodash/throttle'

export default {
  name: 'ListView',
  props: {
    itemHeight: {
      type: Number,
      default: 34
    },
    items: {
      type: Array,
      required: true
    },
    shownumber: {
      type: Number,
      default: 6
    }
  },
  data() {
    return {
      visibleData: [],
      data: [],
      start: 0,
      end: 6
    }
  },
  computed: {
    contentHeight() {
      return this.data.length * this.itemHeight + 'px'
    },
    viewHeight() {
      return this.shownumber * this.itemHeight + 'px'
    }
  },
  watch: {
    items: {
      handler: function (val) {
        if (!val || val.length === 0) return (this.data = [])
        this.data = val
        /*   this.$nextTick(() => {
          this.updateVisibleData(false)
        }) */
      },
      deep: true,
      immediate: true
    },
    shownumber: {
      handler: function (val) {
        this.end = val
      },
      immediate: true
    }
  },
  methods: {
    updateVisibleData: throttle(function (scrollTop) {
      if (scrollTop) {
        const visibleCount = Math.ceil(this.$el.clientHeight / this.itemHeight) // 取得可见区域的可见列表项数量
        this.start = Math.floor(scrollTop / this.itemHeight) // 取得可见区域的起始数据索引
        this.end = this.start + visibleCount // 取得可见区域的结束数据索引
      } else if (scrollTop === 0) {
        this.start = 0
        this.end = this.shownumber
      }
      this.visibleData = this.data.slice(this.start, this.end) // 计算出可见区域对应的数据，让 Vue.js 更新
      this.$refs.content.style.webkitTransform = `translate3d(0, ${
        this.start * this.itemHeight
      }px, 0)` // 把可见区域的 top 设置为起始元素在整个列表中的位置（使用 transform 是为了更好的性能）
    }, 100),
    handleScroll() {
      const scrollTop = this.$el.scrollTop
      this.updateVisibleData(scrollTop)
    }
  }
}
</script>

<style lang="less" scoped>
.list-view {
  overflow: auto;
  position: relative;
}
.list-view-phantom {
  position: absolute;
  left: 0;
  top: 0;
  right: 0;
  z-index: -1;
}
.list-view-content {
  left: 0;
  right: 0;
  top: 0;
  position: absolute;
}
</style>
