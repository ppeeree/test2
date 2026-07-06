<template>
  <div class="tabsNav_content">
    <div class="merge_header">报告工作台</div>
    <div class="tabsNav_content_body">
      <IESummaryConclusion v-model:IESummaryconclusionData="IESummaryconclusionData" />
    </div>
  </div>
</template>

<script>
import IESummaryConclusion from './IESummaryConclusion.vue'
import isEqual from 'lodash/isEqual'
import debounce from 'lodash/debounce'
import { setTheme } from '@/util/util'

// const sessionArr = Object.freeze(['IESummaryconclusionData'])

export default {
  components: {
    IESummaryConclusion
  },
  data() {
    return {
      tabsList: [
        {
          id: 0,
          label: '单机组报告汇总',
          key: 'IESummaryConclusion'
        }
      ],
      selectTabs: ['IESummaryConclusion'],
      componentCache: {},
      activeChart: 'IESummaryConclusion',
      IESummaryconclusionData: null
    }
  },
  beforeCreate() {
    this.$store.commit('SET_THEME_NAME', 'theme-white')
    setTheme('theme-white')
  },
  created() {
    this.IESummaryconclusionData = JSON.parse(sessionStorage.getItem('IESummaryconclusionData'))
  },
  mounted() {
    const that = this
    this.$nextTick(() => {
      for (let i = 0; i < document.querySelectorAll('.avue-view').length; i++) {
        const element = document.querySelectorAll('.avue-view')[i]
        if (element.className === 'avue-view') {
          element.style.height = '100%'
          element.style.overflowY = 'hidden'
          break
        }
      }
    }),
      window.addEventListener('setItem', () => {
        this.handleSessionAssignment(that)
      })
  },
  watch: {
    IESummaryconclusionData: {
      handler: function (val) {
        let IESummaryconclusionPage = this.tabsList.find(o => o.key === 'IESummaryConclusion')
        if (!val) {
          return (IESummaryconclusionPage.label = '')
        }
        IESummaryconclusionPage.label = '单机组报告汇总'
      },
      deep: true
      // immediate: true
    }
  },
  computed: {
    MultipleSelection() {
      const gt = this.selectTabs.length > 1
      const size = gt ? (this.selectTabs.length === 2 ? '100%' : 'calc(50% - 10px)') : '100%'
      const height = gt
        ? this.selectTabs.length === 2
          ? 'calc(50% - 10px)'
          : size
        : 'calc(100% - 10px)'
      return {
        width: size,
        height,
        flexBasis: gt ? size : 'auto'
      }
    }
  },
  methods: {
    isActiveTab(key) {
      return this.selectTabs.includes(key)
    },
    getComponentName(component) {
      // 获取组件名
      return component
    },
    getComponentInstance(component) {
      // 获取组件实例，如果已经缓存过，则直接返回缓存中的实例
      const name = this.getComponentName(component)
      if (!this.componentCache[name]) {
        const instance = this.$options.components[name]
        this.componentCache[name] = instance
      }
      return this.componentCache[name]
    },
    getActiveChart(item) {
      this.getComponentInstance(item)
      if (this.selectTabs.length > 1) {
        this.activeChart = item
      }
    },
    handleCtrlClick(event, key) {
      if (event.button === 0) {
        this.selectTabs = []
        this.selectTabs.push(key)
      }
    },
    handleSessionAssignment: debounce(function (that) {
      const IESummaryconclusionDataTemp = JSON.parse(
        sessionStorage.getItem('IESummaryconclusionData')
      )
      if (!isEqual(that.IESummaryconclusionData, IESummaryconclusionDataTemp)) {
        that.IESummaryconclusionData = IESummaryconclusionDataTemp
      }
    }, 200),
    clickTurbine() {
      this.selectTabs = ['IESummaryConclusion']
      this.activeChart = 'IESummaryConclusion'
    }
  },
  beforeUnmount() {
    sessionStorage.removeItem('IESummaryconclusionData')
  }
}
</script>

<style lang="less" scoped>
@tabItemHeight: 30px;

.tabsNav_content {
  height: 100%;
  background: #ccc;
  .merge_header {
    height: 30px;
    margin-bottom: 3px;
    font-size: 16px;
    font-weight: bold;
    line-height: 30px;
    padding: 0 17px;
  }
  .tabsNav_head {
    height: @tabItemHeight;
    opacity: 1;
    box-sizing: border-box;
    display: flex;
    flex-direction: row;

    position: sticky;
    div {
      width: 138px;
      height: @tabItemHeight;
      text-align: center;
      line-height: @tabItemHeight;

      cursor: pointer;
      span {
        display: inline-block;
        font-size: 15px;
        font-weight: normal;
        letter-spacing: 0em;
        // color: #f7f7f7;
      }
    }
    div + div {
      &::after {
        content: '';
        position: relative;
        width: 1px;
        height: 20px;
        background-color: #616161;
        display: block;
        bottom: 24px;

        cursor: pointer;
      }
    }
  }

  .tabsNav_content_body {
    height: calc(100% - 32px);
    // background-color: #1a1c20;
    overflow-x: hidden;
    overflow-y: auto;
    width: 100%;
    // height: calc(100% - @tabItemHeight);
    // background-color: #1a1c20;
    // display: flex;
    // flex-wrap: wrap;
    // overflow-x: hidden;
    // overflow-y: auto;
    .tabsNav_content_item {
      margin: 5px;
      // background: #252526;
      border: 1px solid transparent;
      div {
        &:first-child {
          height: 100%;
        }
      }
    }
    .tabsNav_content_active {
      border: 1px solid rgb(121, 5, 255);
    }
    &::-webkit-scrollbar-thumb {
      border-radius: 0px;
      background: #696969;
    }
    &::-webkit-scrollbar-track {
      border-radius: 0;
      background: #303030;
    }
  }
}

.active_tabs {
  background-image: url('/img/analysis/bgsele.png');
  background-repeat: no-repeat;
  background-size: auto;

  position: relative;
  top: 4px;
  line-height: 23px !important;
  span {
    font-weight: bold !important;
  }
}
</style>
