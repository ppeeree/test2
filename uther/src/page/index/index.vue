<template>
  <div class="avue-contail" :class="{ 'avue--collapse': isCollapse }">
    <headerElement></headerElement>
    <div class="avue-header">
      <!-- 顶部导航栏 -->
    </div>
    <div class="avue-layout">
      <!-- 判断是否展示： v-if="!getCurrentRoute"  -->
      <div class="avue-left" v-if="!getCurrentRoute">
        <!-- 左侧导航栏 -->
        <sidebar />
      </div>
      <!-- 添加导航栏样式  -->
      <div class="avue-main" :style="handleAloneForDiag">
        <!-- 顶部标签卡 -->
        <tags v-show="offTags" />
        <transition name="fade-scale">
          <search class="avue-view" v-show="isSearch"></search>
        </transition>
        <!-- 主体视图层 -->
        <div :style="handleViewMainStyle" id="avue-view" v-show="!isSearch">
          <keep-alive>
            <router-view class="avue-view" v-if="$route.meta.keepAlive" />
          </keep-alive>
          <router-view class="avue-view" v-if="!$route.meta.keepAlive" />
        </div>
      </div>
    </div>
    <div class="avue-shade" @click="showCollapse"></div>
  </div>
</template>

<script>
import { mapGetters } from 'vuex'
import tags from './tags'
import search from './search'
import top from './top/'
import sidebar from './sidebar/'
import admin from '@/util/admin'
import { validatenull } from '@/util/validate'
import { calcDate } from '@/util/date.js'
import { getStore } from '@/util/store.js'
import headerElement from '@/components/header/index'

export default {
  components: {
    top,
    tags,
    search,
    sidebar,
    headerElement
  },
  name: 'index',
  provide() {
    return {
      index: this
    }
  },
  data() {
    return {
      //搜索控制
      isSearch: false,
      //刷新token锁
      refreshLock: false,
      //刷新token的时间
      refreshTime: '',
      offTags: true
    }
  },
  created() {
    //实时检测刷新token
    this.refreshToken()
    this.handleTagsShow()
    window.addEventListener('setLocalItem', () => this.handleTagsShow())
  },
  mounted() {
    this.init()
  },
  computed: {
    ...mapGetters(['isMenu', 'isLock', 'isCollapse', 'website', 'menu', 'tag']),
    handleViewMainStyle() {
      return {
        height: `calc(100% - ${this.offTags ? 30 : 0}px)`,
        marginLeft: `${this.offTags ? 10 : 0}px`,
        marginRight: '10px',
        width: '100%'
      }
    },
    getCurrentRoute() {
      return /customAnalysis|intelliDiag|WindTurbine|screen|diagnosisTool|diagnosticTasks|reportManage|basicConfig|dataVolumeCheck/i.test(
        this.$route.path
      )
    },
    handleAloneForDiag() {
      if (!this.getCurrentRoute) {
        return
      }
      return {
        width: '100%',
        height: '100%',
        left: '0px'
      }
    }
  },
  methods: {
    showCollapse() {
      this.$store.commit('SET_COLLAPSE')
    },
    // 初始化
    init() {
      this.$store.commit('SET_SCREEN', admin.getScreen())
      window.onresize = () => {
        setTimeout(() => {
          this.$store.commit('SET_SCREEN', admin.getScreen())
        }, 0)
      }
      //  this.$store.dispatch('FlowRoutes').then(() => {})
    },
    // 定时检测token
    refreshToken() {
      this.refreshTime = setInterval(() => {
        const token =
          getStore({
            name: 'token',
            debug: true
          }) || {}
        const date = calcDate(token.datetime, new Date().getTime())
        if (validatenull(date)) return
        if (date.seconds >= this.website.tokenTime && !this.refreshLock) {
          this.refreshLock = true
          this.$store
            .dispatch('refreshToken')
            .then(() => {
              this.refreshLock = false
            })
            .catch(() => {
              this.refreshLock = false
            })
        }
      }, 10000)
    },
    handleTagsShow() {
      this.offTags = JSON.parse(localStorage.getItem('offTags'))
      if (this.offTags) {
        this.$store.commit('DEL_TAG_OTHER')
      } else {
        this.$store.commit('DEL_ALL_TAG')
      }
    }
  }
}
</script>
<style lang="scss" scoped>
.avue-layout {
  width: 100%;
  height: calc(100% - 58px);
  margin-top: -10px;
  .avue-view {
    width: 100%;
    height: 100%;
    //overflow: hidden;
    // position: absolute;
  }
}
:deep(.header){
  position: relative;
  margin-top: 0px;
  z-index: 2000;
  // .title-content {
  //   h2 {
  //     font-size: 5.8vh;
  //   }
  // }
  // .showTime {
  //   .date {
  //     font-size: 3.8vh;
  //     span:nth-child(1) {
  //       font-size: 3vh;
  //       text-align: right;
  //       line-height: 7vh;
  //     }
  //   }
  // }
}
:deep(.avue-sidebar){
  margin-top: 50px;
  background: linear-gradient(
    180deg,
    rgba(13, 52, 83, 0) 0%,
    rgba(13, 52, 83, 0.57) 19%,
    rgba(13, 52, 83, 0.95) 100%
  );
  backdrop-filter: blur(10px);

  border-right: 1px solid;
  border-image: linear-gradient(
      180deg,
      rgba(255, 255, 255, 0) 0%,
      #00ffff 20%,
      rgba(255, 255, 255, 0) 99%
    )
    1 1 1 1;
}
:deep(.avue-sidebar .el-menu-item.is-active),
.avue-sidebar .el-submenu__title.is-active {
  background-color: #11508b;
}
:deep(.avue-sidebar .el-menu-item.is-active:before),
.avue-sidebar .el-submenu__title.is-active:before {
  content: '';
  top: 0;
  left: 0;
  bottom: 0;
  width: 4px;
  background: #1ad5dc;
  position: absolute;
}
#avue-view {
  // background-color: rgba(13, 52, 83, 0.6);
  // backdrop-filter: blur(10px);
  // box-shadow: 5px 5px 5px 0px rgba(0, 0, 0, 0.35), inset 0px 5px 5px 0px rgba(0, 0, 0, 0.35);
}
</style>
