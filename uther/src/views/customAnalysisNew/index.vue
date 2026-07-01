<template>
  <div class="content_page">
    <Header
      @addRow="addRow"
      :activeTabName="activeName"
      :rowTotal="rowTotal"
      @addChart="addChart"
      @initLayout="initLayout"
    />
    <div class="downContent">
      <Aside ref="leftSide" id="leftArea" @nodeClick="nodeClick" />
      <Splitter splitType="vertical" id="areaSplit" limit="{}" style="flex: 0 0 4px" />
      <!--   :layout="rightLayout" -->
      <Content
        v-if="layout.length"
        ref="rightContent"
        :key="layoutName"
        @deleteRow="deleteRow"
        :mainViewOption="mainViewOption"
        @changeActiveTab="changeActiveTab"
        @syncDataTimeRange="syncDataTimeRange"
      />
    </div>
  </div>
</template>
<script>
import Aside from './aside/index.vue'
import Header from './header/index.vue'
import Content from './contentBox/index.vue'
import Splitter from '@/components/splitter/index.vue'
import { rowResize, getAnalysisLayout } from '@/components/splitter/index.js'
import { setTheme } from '@/util/util'

export default {
  components: {
    Aside,
    Header,
    Content,
    Splitter
  },
  data() {
    return {
      rowTotal: 0,
      layoutName: '',
      layout: [
        { key: 'leftPart', partWidth: 10.41 },
        {
          key: 'rightPart',
          partWidth: 89,
          rows: [
            {
              key: 'rowbox',
              rowHeight: 49.5,
              charts: [
                {
                  key: 'colbox',
                  chartType: 'trend',
                  timeRange: '',
                  chartWidth: 100
                }
              ]
            },
            {
              key: 'rowbox',
              rowHeight: 49.5,
              charts: [
                {
                  key: 'colbox',
                  chartType: 'TimeDomain',
                  chartWidth: 49.65
                },
                {
                  key: 'colbox',
                  chartType: 'FreqDomain',
                  chartWidth: 49.65
                }
              ]
            }
          ]
        }
      ],
      // 选中的特征值，以特征值趋势的组件展示
      mainViewOption: {}
      // 选中的机组，部件，测点位置，以列表的形式展示
      //  isShowTable: false
    }
  },
  watch: {
    /*  layout: {
      handler() {
        this.$nextTick(() => {
          this.initSize()
        })
      },
      deep: true,
      immediate: true
    } */
  },
  computed: {
    /*  rightLayout() {
      return this.layout[1].rows
    } */
  },
  created() {},
  unmounted() {
    this.mainViewOption = {}
  },
  mounted() {
    this.$store.commit('SET_THEME_NAME', 'theme-white')
    setTheme('theme-white')
    // 根据layout初始化宽度
    this.$nextTick(() => {
      window.addEventListener('resize', this.handleResize)
    })
    this.initSize()
  },
  beforeUnmount() {
    window.removeEventListener('resize', this.handleResize)
  },
  methods: {
    initLayout(layoutName, data) {
      if (this.layoutName == layoutName) {
        return
      }
      this.layoutName = layoutName
      this.layout = JSON.parse(data)
    },
    initSize() {
      /*  this.rowTotal = this.layout[1].rows.length
      const { clientWidth } = document.body
      let leftWidth = (this.layout[0].partWidth * clientWidth) / 100
      this.$refs['leftSide'].$el.style.width = leftWidth + 'px'
      document.getElementById('areaSplit').style.left = leftWidth + 'px'
      this.$refs.rightContent.$el.style.left = leftWidth + 10 + 'px'
      this.$refs.rightContent.$el.style.width = clientWidth - leftWidth - 10 + 'px' */
    },
    addRow() {
      this.rowTotal += 1
      this.$refs['rightContent'].addRowChart()
    },
    deleteRow() {
      this.rowTotal -= 1
    },
    addChart(rowIndex) {
      this.$refs['rightContent'].addChart(rowIndex)
    },
    handleResize() {
      rowResize('rightArea')
    },

    nodeClick(data) {
      this.mainViewOption = data
    },
    syncDataTimeRange(range) {
      this.$refs.leftSide && this.$refs.leftSide.setTimeRange(range)
    },
    // 切换tab, 辅助图视图布局修改
    changeActiveTab(activeName) {
      this.activeName = activeName
      /* let downContent = getAnalysisLayout(activeName)
      this.layout[1].rows.splice(1, 1, downContent) */
    }
  }
}
</script>
<style scoped lang="scss">
.content_page {
  width: 100%;
  height: 100%;
  user-select: text;
  color: #000;
  .downContent {
    height: calc(100% - 33px);
    display: flex;
    position: relative;
    background: #fff;
    width: 100%;
  }
  #leftArea {
    flex: 0 0 260px;
    height: 100%;
    padding: 10px 5px;
    border: 1px solid #ccc;
    border-radius: 5px;
    color: #606266;
    overflow: auto;
  }
  #rightArea {
    flex: 1 1 0;
    height: 100%;
  }
  .colbox {
    position: relative;
  }
}
</style>
