<template>
  <div class="content">
    <!--  <header-element /> -->
    <div class="drone_table">
      <listNavTags
        ref="navTags"
        :defaultWindpark="WindFarm"
        :isNoneWindPark="false"
        :isMapShow="isMapShow"
        @turbineJump="turbineJump"
      ></listNavTags>
    </div>
    <div v-if="isMapShow" class="change_type_btn">
      <img
        src="/img/WindTurbine/icon/changeCardIcon.png"
        title="网格视图"
        class="change_type_icon"
        @click="changeControl"
      />
    </div>
    <!--  && WindFarm && WindFarm.isImageryLayer -->
    <div v-if="!isMapShow" class="change_type_btn">
      <img
        src="/img/WindTurbine/icon/change3DIcon.png"
        title="地图视图"
        class="change_type_icon"
        @click="changeControl"
      />
    </div>
    <vue-cesium
      v-if="isMapShow"
      :isControl="isControl"
      :measureIsShow="false"
      :windparkList="windparkList"
      :userMapInfo="userMapInfo"
      @fMethod="initData"
      @mapWindparkClick="handleOneClick"
      @mapturbineClick="turbineJump"
      @leftDoubleClick="showAllWindparks"
    >
    </vue-cesium>
    <template v-else>
      <windparkCard
        v-if="isControl && windparkList.length"
        :dataList="windparkList"
        @clickevent="jumpWindpark"
      ></windparkCard>
      <windFieldVue
        v-else-if="selectedWindFarmCode"
        :key="windFieldKey"
        style="float: right"
        :selectWind="WindFarm"
      ></windFieldVue>
    </template>

    <div ref="leftBox" :class="isControl || isMapShow ? 'left_box' : 'insertPage'">
      <newLeftCard ref="leftCard" />
    </div>
  </div>
</template>

<script>
import init from './mixins/index'
import pop from './mixins/cesiumPop'
import cesiumPng from './mixins/cesiumPng'
import { defineAsyncComponent } from 'vue'
import { screenSize } from '@/util/transfrom'
import { mapGetters } from 'vuex'
import { getAllWindParkInfo, getEnitiyTree } from '@/api/screen/index'
import { getModelPlanApi } from '@/api/WindTurbine/CenterPartAPI.js'
import { setTheme } from '@/util/util'
import newLeftCard from './component/newleftCard.vue'
import listNavTags from '../WindTurbine/components/listNavTags.vue'
import VueCesium from './component/vueCesium.vue'
import { getPagecomp } from '@/api/equipment/model'
import { setStore } from '@/util/store'
import windparkCard from './component/windparkCards.vue'

const windFarmCach = []
// let firstEnterSelectWindFarm = true

export default {
  mixins: [init, pop, cesiumPng],
  provide() {
    return {
      showPop: this.handleShowPop,
      parent: this
    }
  },
  components: {
    newLeftCard,
    listNavTags,
    windFieldVue: defineAsyncComponent(() => import('../windField/index.vue')),
    VueCesium,
    windparkCard
  },
  watch: {
    // 监听路由变化
    '$route.query'(newQuery) {
      this.syncStateFromRoute(newQuery)
    },
    userDeptTree() {
      this.syncStateFromRoute(this.$route.query)
    },
    windparkList() {
      this.syncStateFromRoute(this.$route.query)
    }
    /* WindFarm: {
        handler(val) {
        if (!val) return
        this.setSessionItem('selectWindFarm', JSON.stringify(val))
        // 判断在地图模式修改相机
        if (this.isControl) {
          return
        }
        if (this.isMapShow) {
          console.log(1)
          // this.changeFarmVisual(this.WindFarm.id)
            if (val.isImageryLayer) {
            this.changeFarmVisual(this.WindFarm.id)
          } else {
            this.$message.warning('此风场地图资源建设中，敬请期待...')
            this.isMapShow = false
          }
        } 
      },
      deep: true
    }*/
  },
  computed: {
    ...mapGetters(['userInfo', 'userDeptTree']),
    selectedWindFarmCode() {
      return this.getWindFarmCode(this.WindFarm)
    },
    windFieldKey() {
      return this.selectedWindFarmCode || this.$route.query.locationcode || 'empty'
    }
  },
  data() {
    return {
      runServe: '',
      isControl: this.$route.query?.isControl !== undefined ? this.$route.query.isControl : true, // 判断是否为集控级
      isMapShow: false, // 判断是地图模式还是卡片模式，默认卡片模式
      windparkList: [],
      deviceShow: false,
      showPopCard: false,
      row: {},
      sessionArr: Object.freeze([
        'icoName',
        'changeEntities',
        'clickEventPartType',
        'clickChart',
        'tablePopupCell',
        'windowPopup',
        'eventIndexByCondData'
      ]),
      WindFarm: null
    }
  },
  beforeCreate() {
    this.$store.commit('SET_THEME_NAME', 'theme-screen')
    setTheme('theme-screen')
  },
  created() {
    this.getAllWindParkInfoFunc()
    // 自动登录后设备树接口调用
    if (!this.userDeptTree.length) {
      this.$store.dispatch('GetDeptTree', this.userInfo.user_id)
    }
    //==
    this.sessionArr.forEach(item => sessionStorage.removeItem(item))
    this.$cesuimData.handleClearAllData()
    document.documentElement.style.overflow = 'hidden'
    getPagecomp().then(res => {
      setStore({
        name: 'allPagecompList',
        content: JSON.stringify(res.data.data)
      })
    })
  },
  mounted() {
    // 初始化时从路由恢复状态
    this.syncStateFromRoute(this.$route.query)
    this.runServe = process.env.VUE_APP_MODE
    this.$refs.leftBox && screenSize(this.$refs.leftBox)
  },

  beforeUnmount() {
    this.sessionArr.forEach(item => sessionStorage.removeItem(item))
    window.removeEventListener('setItem', () => {})
    let eventBus = [
      'pieDoughnutRe',
      'pieDoughnut',
      'duration',
      'factory',
      'stateTypeClick',
      'leftTalbe',
      'barStacked',
      'levelEvent',
      'progressClick',
      'clickEventType'
    ]
    eventBus.forEach(item => this.$bus.$off(item))
    this.$cesuimData.handleClearAllData()
    document.documentElement.style.overflow = ''
  },

  methods: {
    // 更新路由参数（核心）
    updateRoute(isControl, isMapShow, locationcode) {
      const controlMode = typeof isControl === 'string' ? isControl === 'true' : isControl
      const mapMode = typeof isMapShow === 'string' ? isMapShow === 'true' : isMapShow
      let queryParam = {
        isControl: controlMode,
        isMapShow: mapMode,
        locationcode: controlMode
          ? 'all'
          : locationcode || this.getWindFarmCode(this.WindFarm)
      }
      return this.$router.replace({
        query: {
          ...this.$route.query, // 保留其他参数
          ...queryParam,
          t: Date.now() // 时间戳，强制刷新同路由
        }
      })
    },
    // 从路由参数同步状态
    syncStateFromRoute(query) {
      const { isControl, isMapShow, locationcode } = query
      if (locationcode) {
        // 解析层级栈
        this.isControl = typeof isControl === 'string' ? isControl === 'true' : isControl
        this.isMapShow = typeof isMapShow === 'string' ? isMapShow === 'true' : isMapShow
        if (locationcode == 'all') {
          this.WindFarm = {}
        } else {
          this.WindFarm = this.findWindFarm(locationcode)
        }
      } else {
        this.isControl = this.userInfo.role_name !== 'windpark' ? true : false
        this.isMapShow = false
        this.WindFarm = this.userInfo.role_name !== 'windpark' ? {} : this.normalizeWindFarm(this.userDeptTree[0])
      }
    },
    // 获取所有风场的地理位置坐标列表
    getAllWindParkInfoFunc() {
      getAllWindParkInfo({ userID: this.userInfo.user_id }).then(res => {
        if (res.data.data) {
          this.windparkList = res.data.data //.slice(0, 11)
          this.syncStateFromRoute(this.$route.query)
          /* this.windparkList[0] = {
            ...this.windparkList[0],
            attentionDeviceNum: 2,
            warningDeviceNum: 2
          } */
        }
      })
    },
    handleRight() {
      this.showRightCard = !this.showRightCard
    },
    handleLeft() {
      this.showLeftCard = !this.showLeftCard
    },
    handleDeviceShow() {
      this.deviceShow = !this.deviceShow
    },
    handleShowPop(boolen, row) {
      this.row = row
      this.showPopCard = boolen
      this.$bus.$emit('tablePopupCell', boolen)
    },
    // 双击或者点击全部，重置地图
    /*  showAllWindparks(event) {
      this.isControl = true
      this.WindFarm = null
    }, */
    // 风场卡片切换风场
    jumpWindpark(windpark) {
      const selectedWindpark = this.normalizeWindFarm(windpark)
      this.WindFarm = selectedWindpark
      this.updateRoute(false, this.isMapShow, this.getWindFarmCode(selectedWindpark))
    },
    // 风场视角切换
    changeFarmVisual(windparkId) {
      let id = windparkId || this.getWindFarmCode(this.WindFarm)
      setTimeout(() => {
        let data = this.windparkList.find(i => i.stationID == id)
        this.$utils.map.flyToAngle(
          [Number(data.viewLongitude), Number(data.viewLatitude), Number(data.viewElevation)],
          0,
          -30,
          0
        )
        if (this.$utils.map.FJDataArr.length) {
          this.$utils.map.delFjModel()
        }
        this.initData(id)
      }, 1000)
    },
    // 地图上点击风场事件
    handleOneClick(clickedWindparkId) {
      this.$utils.map.hideMapStatus()
      this.updateRoute(
        false,
        this.isMapShow,
        clickedWindparkId || this.getWindFarmCode(this.WindFarm)
      )
    },
    getDblclickEntity(obj) {
      if (obj) {
        this.turbineJump(obj.entityId)
      }
    },
    // 跳转机组页面
    turbineJump(turbineId) {
      getModelPlanApi({ windturbineId: turbineId }).then(res => {
        let compArr = res.data.data.measloc.sort(function (a, b) {
          return new String(a.sort) - new String(b.sort)
        })
        let noTurbineList = compArr.filter(j => j.deviceModelType !== '整机')
        let pathType = ''
        if (noTurbineList.length < 2) {
          pathType = noTurbineList[0].deviceCode
        } else {
          pathType = compArr[0].deviceCode || 'windturbine'
        }
        this.$router.push({
          path: '/WindTurbine',
          query: {
            turbineId: turbineId,
            type: pathType,
            //  windFarmName: this.WindFarm?.name,
            locationcode: this.getWindFarmCode(this.WindFarm)
          }
        })
      })
    },

    //点击重点事件框的的方法
    /*  showEventDetailCard(clickItem) {
      this.clickEventItem = clickItem
    }, */
    changeSwitchCard(e) {
      this.changeSwitchCardVal = e
    },
    /*  handleTitleClose() {
      sessionStorage.removeItem('tablePopupCell')
      this.showEventDetail = false
      this.clickEventItem = null
      this.changeSwitchCardVal = ''
      this.$bus.$emit('leftTalbe')
      this.$utils.map.initMapStatus()
    }, */
    /* clickOutSide(e) {
      const { buttonSidDom, cardSidDom } = e
      if (buttonSidDom) return
      this.deviceShow = cardSidDom
    }, */
    // 点击右侧事件
    /*  cilckEventListItem(e) {
      this.clearPartStatus()
      this.showEventDetailCard(e)
      const findFJData = this.$utils.map.FJDataArr.find(
        // eslint-disable-next-line no-useless-escape
        item => item.id === 'entitie\-' + e.windTurbineId
      )
      if (!findFJData) return
      const { lng, lat, alt } = this.$utils.map.cartesianTolngLatAlt(findFJData.position._value)
      window.viewer.camera.flyTo({
        destination: window.Cesium.Cartesian3.fromDegrees(
          lng + 0.004937911320850594,
          lat + 0.005166767516649999,
          alt + 400 //218.99764706980164
        ),
        duration: 5,
        orientation: {
          heading: 3.874001322155119,
          pitch: -0.3976787903803052,
          roll: 6.283182116789324
        }
      })
    },
    handleIncludeIndex(e, all = { isAll: false, bool: true }) {
      this.$utils.map.getInclude(
        all,
        // eslint-disable-next-line no-useless-escape
        e.data.ids.map(item => 'entitie\-' + item),
        true
      )
      this.$utils.map.getInclude(
        all,
        // eslint-disable-next-line no-useless-escape
        e.data.ids.map(item => 'label\-' + item),
        true,
        this.$utils.map.FJLabel,
        false
      )
      this.$utils.map.getInclude(
        all,
        // eslint-disable-next-line no-useless-escape
        e.data.ids.map(item => 'event\-' + item),
        true,
        this.$utils.map.eventIcon
      )
      this.$cesuimData.popupList.length > 0 && this.$cesuimData.handleClearPopupList()
    },
    clearPartStatus() {
      this.$bus.$emit('stateTypeClick')
      this.$bus.$emit('progressClick')
      this.$bus.$emit('barStacked')
      this.$bus.$emit('pieDoughnutRe', {
        all: true,
        id: null
      })
      this.$bus.$emit('pieDoughnut', {
        all: true,
        id: null
      })
      this.handleIncludeIndex(
        {
          data: {
            ids: []
          }
        },
        { isAll: true, bool: true }
      )
    },
    dealEventOver() {
      this.$refs['rightCard'].getTableList()
    }, */
    async getWindFarmObj(id) {
      let windObj
      await getEnitiyTree({ stationId: id }).then(res => {
        if (res.data.code == 200) {
          let { childNode, ...others } = res.data.data[0]
          windObj = others
        }
      })
      return windObj
    },
    changeControl() {
      const nextMapShow = !this.isMapShow
      if (nextMapShow) {
        this.updateRoute(true, true, 'all')
        return
      }
      this.updateRoute(this.isControl, false, this.$route.query.locationcode)
    },
    normalizeWindFarm(item) {
      if (!item) return null
      const windFarmCode =
        item.code || item.stationID || item.stationId || item.stationCode || item.id
      return {
        ...item,
        id: item.id || item.stationID || windFarmCode,
        code: item.code || windFarmCode,
        stationID: item.stationID || windFarmCode,
        name: item.name || item.stationName
      }
    },
    getWindFarmCode(item) {
      return item?.code || item?.stationID || item?.stationId || item?.stationCode || item?.id
    },
    findWindFarm(locationcode) {
      const deptMatch = this.userDeptTree.find(
        item =>
          item.id == locationcode ||
          item.code == locationcode ||
          item.stationID == locationcode ||
          item.stationId == locationcode ||
          item.stationCode == locationcode
      )
      if (deptMatch) return this.normalizeWindFarm(deptMatch)

      const stationMatch = this.windparkList.find(
        item =>
          item.stationID == locationcode ||
          item.stationId == locationcode ||
          item.stationCode == locationcode ||
          item.id == locationcode ||
          item.code == locationcode
      )
      if (stationMatch) return this.normalizeWindFarm(stationMatch)

      return {
        id: locationcode,
        code: locationcode,
        stationID: locationcode,
        name: ''
      }
    }
  }
}
</script>

<style lang="less" scoped>
@import url('./style/minxs.less');
.content {
  position: absolute;
  margin-top: 0px;
  width: 100%;
  height: 100%;
  .rigth-box {
    position: absolute;
    top: 7%;
    left: 75.5%;
    transform-origin: 0px 0px 0px;
  }
  .left_box {
    position: absolute;
    top: 0px;
    transform-origin: 0px 0px 0px;
    z-index: 10;
  }
  .insertPage {
    position: absolute;
    transform-origin: 0px 0px 0px;
    z-index: 10;
  }
  .pop-card {
    position: absolute;
    z-index: 99;
    right: 25%;
    bottom: 7%;
  }
  .import-event {
    position: absolute;
    bottom: 0;
    // left: 34%;
    left: calc(50% - 255px);
    // z-index: 10;
  }
  .event-detail {
    z-index: 11;
    position: absolute;
    display: inline-block;
    left: 53%;
    // 13%
    top: 10%;
    width: 420px;
  }
  #info_article {
    position: absolute;
    z-index: 10;
    display: none;
    width: 297px;
    height: 30px;
    border-radius: 6px;
    background: #fff287;
    backdrop-filter: blur(10px);
    box-shadow: 0px 0px 0px 0px rgba(0, 0, 0, 0.3);
    padding: 8px 0;
    .box {
      display: flex;
      flex-direction: row;
      font-size: 12px;
      font-weight: normal;
      line-height: 17px;
      letter-spacing: 0em;
      color: #000000;
      justify-content: space-around;
    }
  }

  #label {
    width: 51.03px;
    height: 25.89px;
    background: rgba(255, 255, 255, 0.2);
    backdrop-filter: blur(10px);
    display: flex;
    flex-direction: row;
    align-items: center;
    #colorLabel {
      width: 20%;
      height: 25.89px;
      background: #ff6b0e;
    }
    .text {
      font-size: 12px;
      font-weight: bold;
      line-height: 17px;
      letter-spacing: 0em;
      color: #ffffff;
      display: flex;
      justify-content: center;
      flex-direction: row;
      width: 80%;
    }
  }

  -moz-user-select: -moz-none;
  -moz-user-select: none;
  -o-user-select: none;
  -khtml-user-select: none;
  -webkit-user-select: none;
  -ms-user-select: none;
  user-select: none;
}
.navigate {
  position: absolute;
  height: 35px;
  top: 7%;
  left: 25%;
  z-index: 10;
}
:deep(img){
  -webkit-user-drag: none;
}
.drone_table {
  position: absolute;
  top: 13px;
  left: 485px;
  z-index: 10;
}
</style>
