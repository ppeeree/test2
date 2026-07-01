<template>
  <div class="avue-sidebar">
    <!-- :class="[{ 'avue-breadcrumb--active': isCollapse }]" -->
    <!-- <logo></logo> -->
    <div v-if="showCollapse" class="collapse">
      <i
        :class="`el-icon-d-arrow-${isCollapse ? 'right' : 'left'}`"
        style="color: rgba(255, 255, 255, 0.69); font-size: 18px; cursor: pointer"
        @click="setCollapse"
      ></i>
    </div>
    <el-scrollbar style="height: calc(100% - 90px)">
      <!-- <div v-if="validatenull(menu)"
           class="avue-sidebar--tip">{{$t('menuTip')}}
      </div> -->
      <el-menu
        unique-opened
        :default-active="nowTagValue"
        mode="vertical"
        :show-timeout="200"
        :collapse="keyCollapse"
      >
        <sidebar-item
          :menu="menu"
          :screen="screen"
          first
          :props="website.menu.props"
          :collapse="keyCollapse"
        ></sidebar-item>
      </el-menu>
    </el-scrollbar>
  </div>
</template>

<script>
import { mapGetters, mapState } from 'vuex'
import logo from '../logo'
import sidebarItem from './sidebarItem'

export default {
  name: 'sidebar',
  components: { sidebarItem, logo },
  inject: ['index'],
  data() {
    return {}
  },
  created() {
    // this.index.openMenu()
  },
  computed: {
    ...mapGetters(['website', 'menu', 'tag', 'keyCollapse', 'screen', 'menuId', 'isCollapse']),
    ...mapState({
      showCollapse: state => state.common.showCollapse
    }),
    nowTagValue: function () {
      return this.$router.$avueRouter.getValue(this.$route)
    }
  },
  methods: {
    setCollapse() {
      this.$store.commit('SET_COLLAPSE')
    }
  }
}
</script>
<style lang="scss" scoped>
.collapse {
  height: 40px;
  display: flex;
  flex-direction: row;
  align-items: center;
  justify-content: center;
  padding: 14px 0 0 0;
}
::v-deep .el-scrollbar__wrap {
  background: transparent !important;
}
</style>
