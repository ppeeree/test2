<template>
  <div class="page_content" ref="fan">
    <!-- 顶部 -->
    <!--   <header-element /> -->

    <!-- 导航栏 -->
    <div
      class="drone_table"
      :style="{
        left: isLeftMiniSize ? 'calc(5% - -5px)' : 'calc(24% - -5px)'
      }"
    >
      <listNavTags
        :windTurbineId="windTurbineId"
        :windTurbineName="windTurbineName"
        :selectedComp="selectedComp"
        :selectPagecomp="selectPagecomp"
        @changeCurrentModel="changeCurrentModel"
        @ClickEntityWind="ClickEntityWind"
      ></listNavTags>
    </div>

    <!-- 健康状态 -->
    <div class="health_status" :style="{ left: isLeftMiniSize ? 'calc(5.5%)' : 'calc(24.5%)' }">
      <span
        class="scada_content"
        v-show="scadaSwitchShow && selectedComp == 'windturbine'"
        :style="{ color: selectedComp !== 'TWW' ? 'white' : '#676767' }"
      >
        SCADA:
        <el-switch
          v-model="isSCADA"
          active-color="#10D33E"
          inactive-color="#999999"
          :disabled="selectedComp == 'TWW'"
        >
        </el-switch
      ></span>
    </div>

    <!-- SCADA数据 -->
    <!-- <scadaValue v-show="isSCADA" :windTurbine="windTurbineId"></scadaValue> -->

    <!-- 模型 -->
    <Model
      class="model"
      ref="turbineModel"
      :changeSize="isLeftMiniSize"
      :isDipTrendShow="isDipTrendShow"
      :entityId="currentCompId"
      :entityStatus="entityStatus"
      :windTurbine="windTurbineId"
      :isStopTimer="isTrendShow"
      :trendParams="trendParams"
      :currentComp="selectedComp"
      @clickEventComp="clickEventComp"
      @dealEventOver="dealEventOver"
      @changeCurrentModel="changeCurrentModel"
      @changeSizeRemove="changeSizeRemove"
      @clickValueCard="clickValueCard"
      @clickTitleIcon="clickTitleIcon"
      @getUpdateValue="getUpdateValue"
    />

    <!-- 左边框 -->
    <div class="left_content" ref="leftBox" :style="{ display: show }">
      <!-- 机组信息按钮 -->
      <div class="infromation_btn">
        <basicInfo
          :currentComp="selectedComp"
          :windTurbineId="windTurbineId"
          :entityId="currentCompId"
          @initCompId="initCompId"
        />
      </div>
      <mini-left-part
        v-if="isLeftMiniSize"
        :windTurbineId="windTurbineId"
        :entityId="currentCompId"
        :currentComp="selectedComp"
      ></mini-left-part>
    </div>
    <!-- 右边框 -->
    <div class="right_content" id="right" ref="rightBox">
      <div
        :class="isRightMiniSize ? 'update_time' : 'update_time_mini'"
        :style="{ right: isRightMiniSize ? '20px' : '480px' }"
      >
        <label class="timetitle_block">数据更新时间：</label>
        <span class="time_block">{{ updateTime || '--' }}</span>
      </div>
    </div>

    <!-- 特征值工况过滤 -->
    <div class="filter_term">
      <trend-filter
        v-if="trendParams.length || isDipTrendShow"
        @changeFunc="updataTrendParam"
        :apiList="allWorkingList"
        :turbineIdList="windTurbineId"
        ref="workfilter"
      ></trend-filter>
    </div>
    <!-- 特征值趋势 -->
    <analysis-trend
      ref="trendModel"
      v-if="isTrendShow"
      :selectedComp="selectedComp"
      :windTurbineId="windTurbineId"
      :trendParams="trendParams"
      :filterData="filterData"
      @changeAnalysisMode="changeAnalysisMode"
    />
    <!--  塔基倾角散点图 -->
    <towerScatter :trendParams="dipTrendParams" :filterData="filterData" ref="tower_scatter" />
    <!-- 更多特征值 -->
    <!-- <moreEigenvalue :trendParams="trendParams" ref="moreEigenvalue" @clickValue="clickValueCard" /> -->
  </div>
</template>

<script>
import { getWkRangeApi } from '@/api/analysis/index.js'
import { setTheme } from '@/util/util'
import { getStore } from '@/util/store'
import { levelColor, eventTypeEnum } from '@/util/constant'
import { mapGetters } from 'vuex'

import Model from './components/Model.vue'
import miniLeftPart from './components/miniLeftPart'
import trendFilter from './components/filterCondition/index.vue'
import analysisTrend from './components/trendAnalysis/index'
import basicInfo from './basicInfo/index'
import towerScatter from './components/towerBaseChart/index.vue'
import navTags from '@/page/index/navTags.vue'
import listNavTags from './components/listNavTags.vue'
import faultImageVue from './leftComp/compFault.vue'

export default {
  components: {
    Model,
    miniLeftPart,
    trendFilter,
    basicInfo,
    analysisTrend,
    towerScatter,
    navTags,
    listNavTags,
    faultImageVue
  },
  provide() {
    return {
      reload: this.reload,
      clickValueCard: this.clickValueCard,
      closeDipModel: this.closeDipModel
    }
  },
  data() {
    return {
      isFirst: true, // 路由开关
      levelColor,
      eventTypeEnum,
      updateTime: '',
      isRouterAlive: true,
      selectedComp: '',
      currentCompId: '',
      isLeftMiniSize: true, // 左侧size
      isRightMiniSize: true, // 右侧size
      isTrendShow: false, // 特征值趋势是否显示
      isDipTrendShow: false,
      trendParams: [],
      dipTrendParams: {},
      filterData: {},
      /*  windEngine: '',
      windBlade: '',
      windTower: '', */
      isSCADA: false,
      scadaSwitchShow: false,
      entityStatus: '--',
      windTurbineName: '', // 机组名字
      windTurbineId: '', //机组对象
      selectPagecomp: '', // 选中部件
      /* entityListSelect: {
        entityName: ''
      }, */
      setCompBtnName: {
        turbine: '整机',
        engine: '机舱',
        tower: '塔筒',
        blade: '风轮'
      },

      currentEntityCompStates: [],
      showEventDetail: '',
      allWorkingList: [], //工况接口数据
      clickEventItem: '', // 存储点击重点事件的对象
      pagecompData: [], //机组的聚合部件

      isNoFaultData: false, //部件故障
      compFaultList: []
    }
  },
  computed: {
    ...mapGetters(['userDeptTree'])
  },
  watch: {
    '$route.query': {
      handler(val, oldVal) {
        this.changeRoutePath(val, oldVal)
        /* if (this.isFirst) {
          this.isFirst = false
          this.changeRoutePath(val, oldVal)
        } else {
          return
        } */
      }
    },
    windTurbineId: {
      handler() {
        this.trendParams = []
        this.dipTrendParams = {}
      }
    }
  },
  beforeCreate() {
    this.$store.commit('SET_THEME_NAME', 'theme-screen')
    setTheme('theme-screen')
  },
  created() {
    this.pagecompData = JSON.parse(getStore({ name: 'allPagecompList' }))
    this.initPage()
    //this.getAllPagecomp()
  },
  mounted() {},
  beforeDestroy() {},
  methods: {
    initPage() {
      let { turbineId, type } = this.$route.query
      this.windTurbineId = turbineId
      this.selectedComp = type
      this.selectPagecomp = this.pagecompData.find(j => j.entityType == type)
      this.windTurbineName = this.getTurbineNameByID(turbineId)
      if (type === 'windturbine') {
        this.currentCompId = turbineId
      } else {
        this.currentCompId = turbineId + type
      }
    },
    getTurbineNameByID(turbineId) {
      let name = ''
      this.userDeptTree.forEach(element => {
        let obj = element.childNode.find(o => o.entityId == turbineId)
        if (obj) {
          name = obj.entityName
        }
      })
      return name || ''
    },

    /* getUnit(turbineId) {
      let obj = {}
      getUnitApi({
        windTurbineId: turbineId,
        entityType: 'windturbine'
      }).then(res => {
        const { entityInfo, entityName } = Array.isArray(res.data.data)
          ? res.data.data[0]
          : res.data.data
        if (entityInfo && entityInfo.length) {
          obj = {
            turbineName: entityName
          }
        } else {
          alert('无数据返回！')
        }
      })
      return obj
    }, */
    //获取工况数据
    getWorking() {
      getWkRangeApi({ windturbineIds: this.windTurbineId }).then(res => {
        if (res.data.code == 200) {
          this.allWorkingList = res.data.data
        }
      })
    },
    initCompId(turbineName) {
      /* compIds.forEach(item => {
        if (item.key === 'NAC') {
          this.windEngine = item.value
        } else if (item.key === 'ROT') {
          this.windBlade = item.value
        } else {
          this.windTower = item.value
        }
      }) */
      // this.entityListSelect.entityName = turbineName
      // this.windTurbineName = turbineName
    },
    /* getEntityStatus(state, time, compStatus) {
      this.entityStatus = state
      this.currentEntityCompStates = compStatus
    }, */
    // 特征值趋势等是否显示
    changeAnalysisMode() {
      this.isTrendShow = !this.isTrendShow
      if (!this.isTrendShow) {
        this.trendParams = []
      }
    },
    // 切换部件
    changeCurrentModel(type) {
      // console.log('2、index_切换部件')
      this.entityStatus = ''
      this.updateTime = ''
      this.trendParams = []
      /*   this.selectedComp = type
      if (type !== 'windturbine') {
        this.currentCompId = this.windTurbineId + type
      } else {
        this.currentCompId = this.windTurbineId
      }
      this.selectPagecomp = this.pagecompData.find(j => j.entityType == type) */
      this.$router.replace({
        query: { ...this.$route.query, turbineId: this.windTurbineId, type }
      })
    },
    //点击设备树拿到机组数据
    ClickEntityWind(obj) {
      this.entityStatus = '--'
      this.updateTime = ''
      this.currentEntityCompStates = []
      /*   this.windTurbineId = obj.entityId || obj.windturbineId
      this.windTurbineName = obj.entityName || obj.windturbineName
      this.currentCompId = this.windTurbineId + this.selectedComp */
      this.$router.replace({
        query: {
          locationcode: obj.id,
          turbineId: obj.entityId || obj.windturbineId,
          type: this.selectedComp
        }
      })
    },
    //跳转地址改变的方法
    changeRoutePath(val, olderVal) {
      this.isTrendShow = false
      this.closeDipModel()
      if (val.turbineId === olderVal.turbineId && val.type !== olderVal.type) {
        const { type } = val
        this.selectedComp = type
        this.selectPagecomp = this.pagecompData.find(j => j.entityType == type)
        if (type !== 'windturbine') {
          this.currentCompId = this.windTurbineId + type
        } else {
          this.currentCompId = this.windTurbineId
        }
      } else if (val.turbineId != olderVal.turbineId) {
        this.initPage(val)
      }
    },

    changeRightSize() {
      this.isRightMiniSize = !this.isRightMiniSize
    },

    // 页面加载
    reload() {
      this.isRouterAlive = false
      this.$nextTick(function () {
        this.isRouterAlive = true
        // console.log('页面刷新成功')
      })
    },

    //点击测点信息框方法
    clickValueCard(obj, event, name) {
      if (name) {
        // 倾角分布图
        this.isDipTrendShow = true
        this.isTrendShow = false
        this.$refs['tower_scatter'].visible = true
        this.dipTrendParams = {
          turbineId: this.windTurbineId,
          left: event.clientX,
          top: event.clientY,
          objArr: obj
        }
        // this.$refs['tower_scatter'].init(this.trendParams)
      } else {
        let value = this.trendParams.find(j => j.evID == obj.evID)
        if (value) {
          this.trendParams = this.trendParams.filter(j => j != value)
        } else {
          this.trendParams.push({
            turbineId: this.windTurbineId,
            ...obj,
            left: event.clientX,
            top: event.clientY
          })
        }
        this.isTrendShow = true
        if (this.isDipTrendShow) {
          this.closeDipModel()
        }
        if (this.trendParams.length == 0) {
          this.changeAnalysisMode()
        }
      }
    },

    updataTrendParam(param) {
      this.filterData = { ...param }
    },

    closeDipModel() {
      // console.log('关闭弹框', this.isDipTrendShow)
      this.isDipTrendShow = false
      this.$refs['tower_scatter'].visible = false
    },
    dealEventOver() {
      this.$refs['right_part'].init()
    },

    // 判断是否有scada数据
    judgeScada(val) {
      this.scadaSwitchShow = val
    },
    getUpdateValue(time) {
      // console.log(time == 'Invalid date')
      if (time !== '' && time !== 'null' && time !== 'Invalid date') {
        this.updateTime = time
      } else {
        this.updateTime = '--'
      }
    }
  }
}
</script>

<style lang="less" scoped>
.page_content {
  -moz-user-select: -moz-none;
  -moz-user-select: none;
  -o-user-select: none;
  -khtml-user-select: none;
  -webkit-user-select: none;
  -ms-user-select: none;
  user-select: none;
  width: 100vw;
  height: 100vh;
  position: absolute;
  // background: url('/img/WindTurbine/background/bg.png');
  overflow: hidden;
  /*  background-size: 100%, 100%; */
  background-repeat: no-repeat;
  box-sizing: border-box;
  //导航栏
  .drone_table {
    position: absolute;
    top: 13px;
    z-index: 10;
  }
  //健康状态
  .health_status {
    position: absolute;
    top: calc(12.5%);
    z-index: 9;
    .scada_content {
      font-size: 15px;
      margin-left: 12px;
      ::v-deep .el-switch {
        height: 15px;
        width: 25px;
        margin-bottom: 2px;
        .el-switch__core {
          width: 25px !important;
          height: 15px;
        }
        .el-switch__core:after {
          background-color: #10172d;
          width: 12px;
          height: 12px;
          top: 0px;
        }
      }
      ::v-deep .el-switch.is-checked .el-switch__core::after {
        margin-left: -13px;
      }
    }
  }
  // 设备列表按钮
  .normal_left_arrow {
    position: absolute;
    top: 8%;
    left: 69.5%;
    background: url('/img/WindTurbine/background/turbineBar.png');
    background-size: 100%, 100%;
    background-repeat: no-repeat;
    box-sizing: border-box;
    width: 104px;
    height: 36px;
    z-index: 10;
    .normal_left_arrow_text {
      line-height: 36px;
      color: #1fffff;
      font-size: 18px;
      margin-left: 18px;
      font-weight: bold;
      cursor: pointer;
    }
  }
  // 设备列表卡片
  .device_card {
    position: absolute;
    z-index: 1000;
    top: 10%;
    margin-left: calc(100% - 1070px);
  }
  //左边框
  .left_content {
    position: absolute;
    left: 0;
    top: 0;
    width: auto;
    height: calc(100% - 80px);
    z-index: 10;
  }
  // 右边框
  .right_content {
    z-index: 10;
    position: absolute;
    right: 10px;
    top: 7%;
    width: auto;
    height: calc(100% - 80px);
  }
  .update_time {
    color: #fff;
    position: absolute;
    right: 20px;
    bottom: 0px;
    font-size: 16px;
    width: 380px;
    img {
      float: left;
      margin-top: 3px;
      margin-right: 0px;
    }
    .timetitle_block {
      display: inline-block;
    }
    .time_block {
      display: inline-block;
    }
    label {
      margin-left: 10px;
      margin-right: 5px;
    }
  }
  .update_time_mini {
    color: #fff;
    position: absolute;
    bottom: 0px;
    width: 150px;
    font-size: 16px;
    line-height: 25px;
    label {
      margin-right: 5px;
    }
    img {
      float: left;
      margin-top: 3px;
      margin-right: 5px;
    }
    .timetitle_block {
      display: block;
    }
    .time_block {
      display: block;
    }
  }
  .model {
    z-index: 1;
  }
}

.infromation_btn {
  position: absolute;
  top: calc(100% - 50px);
  left: 105%;
  width: 30px;
  height: 30px;
  cursor: pointer;
}
.infromation_btn_none {
  position: absolute;
  top: calc(95%);
  left: 99px;
  width: 30px;
  height: 30px;
  cursor: pointer;
}

.filter_term {
  position: absolute;
  right: 0;
  top: 15px;
  color: #fff;
  z-index: 10;
}

//事件详情
.event_detail {
  position: absolute;
  display: inline-block;
  top: 12.5%;
  width: 420px;
  z-index: 11;
}
// 重点事件
.import_event {
  // z-index: 999;
  position: absolute;
  display: inline-block;
  top: 94%;
  left: 37%;
}
</style>
