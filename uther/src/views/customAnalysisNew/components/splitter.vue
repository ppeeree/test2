<template>
  <div
    :id="id"
    :class="[
      'x-splitter splitter',
      splitType == 'horizontal' ? 'x-splitter-horizontal' : 'x-splitter-vertical'
    ]"
    @mousedown="addEvent"
  ></div>
</template>
<script>
import { mouseEvent } from './toolsComponent/tools.js'
export default {
  props: ['splitType', 'id', 'fatherId', 'limit'],
  mounted() {},
  methods: {
    addEvent(e) {
      mouseEvent(e, this.id, this.splitType, this.limit, this.fatherId)
    }
  }
}
</script>
<style lang="scss" scoped>
.x-splitter {
  background: #eee;
  position: absolute;
  z-index: 999;
}
.x-splitter-horizontal {
  cursor: row-resize;
  height: 4px;
  width: 100%;
  left: 0;
}
.x-splitter-vertical {
  cursor: col-resize;
  width: 4px;
  height: 100%;
  top: 0;
}
</style>
