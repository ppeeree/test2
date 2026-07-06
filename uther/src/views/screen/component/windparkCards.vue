<template>
  <div class="mainCard" ref="gridWrapper" :style="gridStyle">
    <windTag
      style="float: right; right: 100px; top: -40px"
      :sortOptions="[
        {
          value: 'windparkName',
          label: '风场名称'
        },
        {
          value: 'tubineStatusIndex',
          label: '机组报警数量'
        }
      ]"
      @sortChange="sortChange"
    ></windTag>
    <template v-if="startRender">
      <template v-for="ii in orderDataList" :key="ii.stationID">
        <unit-card
          @clickevent="clickevent"
          :style="ii.style"
          :windparkInfo="ii"
        />
      </template>
    </template>
  </div>
</template>
<script>
import { levelColorEnum } from '@/util/constant.js'
import unitCard from './cardUnit.vue'
import windTag from '@/views/windField/windTag.vue'
export default {
  components: {
    unitCard,
    windTag
  },
  props: {
    dataList: {
      type: Array,
      default: () => []
    }
  },
  computed: {
    // 计算网格样式
    gridStyle() {
      if (this.dataList.length === 0) return {}
      const { cols, rows } = this.calculateGrid(this.dataList.length)
      return {
        gridTemplateColumns: `repeat(${cols}, 1fr)`,
        gridTemplateRows: `repeat(${rows}, 1fr)`
      }
    }
  },
  data() {
    return {
      startRender: false,
      containerWidth: 1500,
      containerHeight: 0,
      levelColorEnum,
      orderDataList: [],
      levelConfig: [
        {
          name: '危险',
          code: 'danger',
          key: 'dangerDeviceNum'
        },
        {
          name: '警告',
          code: 'warning',
          key: 'warningDeviceNum'
        },
        {
          name: '注意',
          code: 'attention',
          key: 'attentionDeviceNum'
        },
        {
          name: '正常',
          code: 'normal',
          key: 'normalDeviceNum'
        }
      ]
    }
  },
  mounted() {
    this.orderDataList = this.dataList
    this.updateContainerSize()
    window.addEventListener('resize', this.updateContainerSize)
  },
  beforeUnmount() {
    window.removeEventListener('resize', this.updateContainerSize)
  },
  methods: {
    sortChange(val, orderVal) {
      if (val == 'windparkName') {
        if (orderVal == 'Order') {
          //升序
          this.orderDataList = this.dataList
        } else {
          //降序
          this.orderDataList = [...this.dataList].reverse()
        }
      } else {
        // 报警数量
        if (orderVal == 'Order') {
          //升序
          this.orderDataList = this.sortByStatus(true)
        } else {
          //降序
          this.orderDataList = this.sortByStatus(false)
        }
      }
    },
    sortByStatus(isDesc = true) {
      let data = this.dataList.sort((a, b) => {
        // 权重数组（降序）
        const wA = [a.dangerDeviceNum, a.warningDeviceNum, a.attentionDeviceNum, a.normalDeviceNum]
        const wB = [b.dangerDeviceNum, b.warningDeviceNum, b.attentionDeviceNum, b.normalDeviceNum]

        // 全 0 沉底
        const allZeroA = wA.every(v => v === 0)
        const allZeroB = wB.every(v => v === 0)
        if (allZeroA && !allZeroB) return 1
        if (!allZeroA && allZeroB) return -1
        if (allZeroA && allZeroB) return 0

        // 逐级比较
        for (let i = 0; i < wA.length; i++) {
          if (wA[i] !== wB[i]) return isDesc ? wA[i] - wB[i] : wB[i] - wA[i]
        }
        return 0
      })
      return data
    },
    clickevent(data) {
      this.$emit('clickevent', data)
    },
    // 更新容器尺寸
    updateContainerSize() {
      this.$nextTick(() => {
        const wrapper = this.$refs.gridWrapper
        if (wrapper) {
          this.containerWidth = wrapper.clientWidth
          this.containerHeight = wrapper.clientHeight
          this.updateItemsStyle()
        }
      })
    },
    // 关键：计算每个块的实际像素尺寸，设置CSS变量
    updateItemsStyle() {
      if (this.dataList.length === 0) return

      const { cols, rows } = this.calculateGrid(this.dataList.length)
      const gap = 10
      const totalGapsX = (cols - 1) * gap
      const totalGapsY = (rows - 1) * gap

      // 计算每个块的实际像素大小
      let blockWidth = (this.containerWidth - totalGapsX) / cols
      let blockHeight = (this.containerHeight - totalGapsY) / rows

      // 限制最大宽高
      blockWidth = Math.min(blockWidth, 300)
      blockHeight = Math.min(blockHeight, 260)

      // 限制最小宽高
      blockWidth = Math.max(blockWidth, 168)
      blockHeight = Math.max(blockHeight, 153)
      // 更新每个 item 的样式（包含CSS变量）
      this.dataList.forEach((item, index) => {
        item.style = {
          //  background: this.colors[index % this.colors.length],
          // 设置CSS变量，供 font-size: calc() 使用
          '--block-width': `${blockWidth}px`,
          '--block-height': `${blockHeight}px`
        }
      })
      this.startRender = true
    },
    // 核心算法：计算最优网格布局
    calculateGrid(count) {
      if (count === 0) return { cols: 0, rows: 0 }

      // 获取容器实际宽高比（减去padding）
      const aspectRatio = this.containerWidth / this.containerHeight

      let bestCols = 1
      let bestRows = count
      let minDiff = Infinity

      // 遍历所有可能的列数，找到最接近容器比例的布局
      for (let cols = 1; cols <= count; cols++) {
        const rows = Math.ceil(count / cols)
        const gridRatio = cols / rows
        const diff = Math.abs(gridRatio - aspectRatio)

        if (diff < minDiff) {
          minDiff = diff
          bestCols = cols
          bestRows = rows
        }
      }

      // 优化：如果最后一行太空，尝试减少列数
      const lastRowItems = count - (bestRows - 1) * bestCols
      if (lastRowItems < bestCols * 0.5 && bestCols > 1) {
        for (let cols = bestCols - 1; cols >= 1; cols--) {
          const rows = Math.ceil(count / cols)
          const lastRow = count - (rows - 1) * cols
          // 最后一行至少填充60%，或者是单行
          if (lastRow >= cols * 0.6 || rows === 1) {
            bestCols = cols
            bestRows = rows
            break
          }
        }
      }

      return { cols: bestCols, rows: bestRows }
    }
  }
}
</script>
<style lang="scss" scoped>
.mainCard {
  float: right;
  width: calc(100% - 470px);
  margin-top: 50px;
  height: 88%;
  position: relative;
  color: white;
  padding: 0 15px 0 15px;
  display: grid;
  // gap: clamp(8px, 0.4vw, 18px);
  row-gap: 10px;
  column-gap: 10px;
  transition: all 0.3s ease;
}
</style>
