<template>
  <div class="header">
    <div class="nav-position">
      <div
        v-for="(item, index) in items"
        :key="index"
        :class="[
          item.code !== 'system' ? 'nav-bg' : 'system_nav',
          index > items.length - 3 ? 'nav-bg2' : ''
        ]"
        :style="{
          left: `${120 * index + 20}px`,
          right: `${120 * (items.length - index) + 140}px`
        }"
      >
        <i
          class="el-icon-setting"
          title="系统管理"
          v-if="item.code == 'system'"
          @click="openMenu(item)"
        ></i>
        <span
          v-else-if="item.code == 'screen' || item.code == 'basicConfig' || item.code == 'analysis'"
          @click="openMenu(item)"
          >{{ generateTitle(item) }}</span
        >
        <el-popover placement="bottom" trigger="hover" v-else popper-class="tag_style">
          <span v-for="ele in menu" :key="ele.code" class="list_item">
            <div
              v-if="validatenull(ele[childrenKey]) && vaildRoles(ele)"
              @click="openMenu(ele, item)"
            >
              {{ ele.name }}
            </div>
          </span>
          <span @mouseenter="openMenu(item)" slot="reference">{{ generateTitle(item) }}</span>
        </el-popover>
      </div>
    </div>

    <div class="showTime">
      <span class="date">
        <span>{{ nowTime }}</span>
      </span>
    </div>
    <div class="title-content">
      <el-image src="/img/bg/icon.png" fit="cover" />
      <h2>{{ websiteTitleName }}</h2>
    </div>
    <el-popover
      :visible-arrow="false"
      placement="bottom"
      trigger="click"
      popper-class="user_popper"
    >
      <div class="user-info">
        <div class="info-item">
          <el-image style="width: 20px" src="/img/login/userIcon1.png" fit="cover" />
          <span>{{ userInfo.nick_name }}</span>
        </div>
        <span class="line" />
        <div
          :class="['info-item', { 'info-item_active': showNoChange }]"
          style="width: 93%; margin-left: 2px"
          @click="showNoChange = true"
        >
          <el-image style="width: 20px" src="/img/login/logout.png" fit="cover" />
          <span>{{ $t('navbar.logOut') }}</span>
        </div>
      </div>
      <el-image
        slot="reference"
        src="/img/login/userIcon2.png"
        fit="cover"
        class="user"
        style="width: 24px"
      />
    </el-popover>
    <!-- 确定退出登录框 -->
    <el-dialog
      custom-class="log_out"
      :visible.sync="showNoChange"
      width="17%"
      :modal-append-to-body="false"
    >
      <span>
        <span style="float: left; margin-right: 7px"
          ><i class="el-icon-question" style="color: #e6a23c; font-size: 20px"></i
        ></span>
        <span>退出系统，是否继续?</span>
      </span>
      <span slot="footer" class="dialog-footer">
        <el-button @click="showNoChange = false">取 消</el-button>
        <el-button type="primary" @click="goLogin">确 定</el-button>
      </span>
    </el-dialog>
  </div>
</template>
<script>
import dayjs from 'dayjs'
import { setTheme } from '@/util/util'
import { mapGetters } from 'vuex'
import { resetRouter } from '@/router/router'
import cloneDeep from 'lodash/cloneDeep'
import { info } from '@/api/system/tenant'
import { getTopUrl } from '@/util/util'

export default {
  data() {
    return {
      nowTime: '',
      showUserInfo: false,
      showNoChange: false, //控制弹框显示变量
      items: [], // == titleNavTo
      currentTopMenu: '',
      websiteTitleName: ''
    }
  },
  computed: {
    ...mapGetters(['userInfo', 'roles', 'menu', 'themeName', 'tagWel'])
  },
  created() {
    this.getMenu()
    let domain = getTopUrl()
    info(domain).then(res => {
      const data = res.data
      this.websiteTitleName = data.data?.websiteTitleName || ''
      this.setLocalstorageItem('websiteCofigTemp', JSON.stringify(data.data))
    })
  },
  mounted() {
    setTheme(this.themeName)
    setInterval(() => {
      this.nowTime =
        dayjs().format('YYYY') +
        '年' +
        dayjs().format('MM') +
        '月' +
        dayjs().format('DD') +
        '日 ' +
        dayjs().format('HH:mm:ss')
    }, 1000)
  },
  methods: {
    /* openPage() {
      window.open('http://192.168.124.11:5150', '_blank')
    }, */
    vaildRoles(item) {
      item.meta = item.meta || {}
      return item.meta.roles ? item.meta.roles.includes(this.roles) : true
    },
    childrenKey() {
      return this.props.children || this.config.propsDefault.children
    },
    getMenu() {
      this.$store.dispatch('GetTopMenu').then(res => {
        let items = cloneDeep(res)
        this.items = items
      })
    },
    generateTitle(item) {
      let name = this.$router.$avueRouter.generateTitle(item.name, (item.meta || {}).i18n)
      return name
    },
    logout() {
      this.$confirm(this.$t('logoutTip'), this.$t('tip'), {
        confirmButtonText: this.$t('submitText'),
        cancelButtonText: this.$t('cancelText'),
        type: 'warning'
      }).then(() => {
        this.$store.dispatch('LogOut').then(() => {
          resetRouter()
          this.$router.push({ path: '/login' })
        })
      })
    },
    goLogin() {
      this.showNoChange = false
      this.$store.dispatch('LogOut').then(() => {
        // resetRouter()
        this.$router.push({ path: '/login' })
      })
    },
    // navTo(path) {
    //   if (path === '/system') {
    //     path = this.tagWel.value || '/equipment/turbine/index'
    //   }
    //   if (this.$route.path === path || !path) return
    //   this.$router.push({ path })
    // },

    openMenu(item, parent) {
      /* if (JSON.stringify(this.currentTopMenu) == JSON.stringify(item)) {
        return
      } else {
        this.currentTopMenu = cloneDeep(item)
      } */
      this.currentTopMenu = cloneDeep(item)
      if (this.$route.path !== item.path) {
        // 不同应用场景，使用不同主题色
        let themeVal = 'system'
        /*if (item.code === 'analysis') {
          themeVal = 'theme-analysis'
          this.$store.commit('SET_THEME_NAME', 'theme-analysis')
        } else if (item.code === 'system') {
          themeVal = 'theme-default'
          this.$store.commit('SET_THEME_NAME', 'theme-default')
        } else if (item.code === 'report') {
          themeVal = 'theme-analysis'
          this.$store.commit('SET_THEME_NAME', 'theme-analysis')
        } else if (item.code === 'screen') {
          themeVal = 'theme-screen'
          this.$store.commit('SET_THEME_NAME', 'theme-screen')
        }*/
        if (item.code === 'system') {
          themeVal = 'theme-default'
          this.$store.commit('SET_THEME_NAME', 'theme-default')
          setTheme(themeVal)
        }
        if (parent) {
          // console.log('parent', parent)
          this.setLocalstorageItem(
            'offTags',
            !['screen', 'analysis', 'intelliDiag', 'report', 'basicConfig', 'opsSystem'].includes(
              parent.code
            )
          )
        } else {
          this.setLocalstorageItem(
            'offTags',
            !['screen', 'analysis', 'intelliDiag', 'report', 'basicConfig', 'opsSystem'].includes(
              item.code
            )
          )
        }
        this.openNewMenu(item)
      } else {
        this.$message({
          message: '已经在该页面，请重新选择！',
          type: 'warning'
        })
      }
    },
    openNewMenu(item = {}) {
      this.$store.dispatch('GetMenu', item.id).then(data => {
        if (data.length !== 0) {
          this.$router.$avueRouter.formatRoutes(data, true)
        }
        if (item.code == 'screen') {
          return this.$router.push('/screen')
        } else if (item.code == 'basicConfig') {
          return this.$router.push('/basicConfig/dataDownLoad/index')
        } else if (item.code == 'analysis') {
          return this.$router.push('/customAnalysisNew/index')
        } else {
          //当点击顶部菜单后默认打开第一个菜单
          if (!this.validatenull(item)) {
            let itemActive = {},
              childItemActive = 0
            if (item.path) {
              itemActive = item
            } else {
              return
            }
            this.$store.commit('SET_MENU_ID', item)
            if (itemActive.code == 'wave') {
              let param = {
                evcode: '',
                measlocCode: '',
                windturbineIds: '',
                work: '',
                pointer: ''
              }
              this.$router.push({
                path: this.$router.$avueRouter.getPath(
                  {
                    name: itemActive.label || itemActive.name,
                    src: itemActive.path
                  },
                  itemActive.meta
                ),
                query: {
                  content: JSON.stringify(param)
                }
              })
            } else if (itemActive.code == 'one') {
              const routeUrl = this.$router.resolve({
                path: itemActive.path
              })
              window.open(routeUrl.href, '_blank')
            } else {
              this.$router.push({
                path: this.$router.$avueRouter.getPath(
                  {
                    name: itemActive.label || itemActive.name,
                    src: itemActive.path
                  },
                  itemActive.meta
                )
              })
            }
          }
        }
      })
    }
    /*  handleWebsitTitle() {
      const websiteCofigTemp = JSON.parse(localStorage.getItem('websiteCofigTemp')) || {}
      this.websiteTitleName = websiteCofigTemp?.websiteTitleName || ''
    } */
  }
}
</script>
<style lang="scss" scoped>
.header {
  position: absolute;
  height: 68px;
  background-image: url('/img/WindTurbine/background/titleBg.png');
  background-position: center;
  background-repeat: no-repeat;
  width: 100%;
  z-index: 12;
  margin-top: -2px;
  .nav-position {
    height: 0;
    .nav-bg {
      background-image: url('/img/login/nav2.png');
      background-position: center;
      background-repeat: no-repeat;
      /* width: 100%; */
      width: 107.72px;
      height: 30.7px;
      position: absolute;
      top: 14px;
      left: 32px;
      text-align: center;
      cursor: pointer;
      span {
        font-size: 15px;
        font-weight: bold;
        line-height: 35px;
        letter-spacing: 0px;
        color: #ffffff;
      }
    }
    .system_nav {
      .el-icon-setting {
        position: relative;
        display: inline-block;
        float: right;
        top: 23px;
        right: 270px;
        cursor: pointer;
        color: white;
      }
    }
  }
  .nav-bg2 {
    background-image: url('/img/login/nav.png') !important;
    left: unset !important;
    right: 320px;
  }
  .title-content {
    display: flex;
    flex-direction: row;
    justify-content: center;
    align-items: center;
    h2 {
      color: #ffffff;
      font-size: 2.8vh;
      line-height: 68px;
      letter-spacing: 3px;
      margin-left: 10px;
    }
    ::v-deep .el-image {
      width: 24px;
      height: 27px;
    }
  }
  .weather {
    position: absolute;
    right: 9.8vh;
    top: 1.75vh;
    font-size: 1.575vh;
    // color: rgb(255 255 255);
    display: -webkit-box;
    display: -ms-flexbox;
    display: flex;
    -webkit-box-align: center;
    -ms-flex-align: center;
    align-items: center;
    width: 22.75vh;
    -webkit-box-pack: space-evenly;
    -ms-flex-pack: space-evenly;
    justify-content: space-between;
    div {
      display: flex;
      align-items: center;
      img {
        //  width: 2.975vh;
      }
      span {
        margin-left: 4px;
        display: inline-block;
      }
    }
    span {
      display: inline-block;
    }
    .tem {
      margin: 0 0.175vh 0 0.35vh;
    }
  }

  .showTime {
    position: absolute;
    top: 23px;
    right: 80px;
    color: #fff;
    display: -webkit-box;
    display: -ms-flexbox;
    display: flex;
    .time {
      font-size: 0.49vh;
      margin-right: 0.135vh;
    }
    .date {
      span {
        display: block;
        &:nth-child(1) {
          font-size: 15px;
          text-align: right;
          line-height: 15px;
        }
        &:nth-child(2) {
          font-size: 0.245vh;
        }
      }
    }
  }
  .user {
    /*  position: relative;
    right: 4%;
    bottom: 72%; */
    float: right;
    right: 10px;
    top: -50px;
    cursor: pointer;
  }
}
.user_popper {
  .user-info {
    width: 120px;
    background-color: #fff;
    position: absolute;
    right: 0.3%;
    top: 80%;
    display: flex;
    flex-direction: column;
    padding: 5px;
    img {
      width: 20px;
    }
    .line {
      height: 1px;
      background-color: rgba(255, 255, 255, 0.5);
      flex-grow: 0.1;
    }
    .info-item {
      /*    flex-direction: row;
      display: flex;
      align-items: center;
      justify-content: space-evenly;
      justify-items: flex-start; */
      width: 100%;
      margin: 3px 0;
      padding: 5px 10px;
      color: #000;
      font-size: 12px;
      cursor: pointer;
      &:hover {
        background: #cde2f9;
        border-radius: 2px;
      }
      .el-image {
        float: left;
        margin-top: -2px;
        margin-right: 10px;
      }
    }
    .info-item_active {
      background: #cde2f9;
      border-radius: 2px;
    }
  }
}
//弹框样式修改
::v-deep .el-dialog__body {
  height: 100px;
  color: white;
}
.tag_style {
  .list_item {
    color: white;
    text-align: center;
    font-size: 14px;
    font-weight: normal;
    div {
      height: 34px;
      line-height: 34px;
      background: rgba(4, 17, 33, 1);
      cursor: pointer;
      &:hover {
        background: #18222c;
        color: #409eff;
      }
    }
  }
}
</style>
