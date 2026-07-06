<template>
  <ul class="cssNav" ref="closePopover">
    <li class="wind_farm_tag" v-if="userInfo.role_name != 'windpark'">
      <span
        :style="{
          cursor: 'pointer',
          color: '#1fffff',
          fontWeight: 'bold'
        }"
        @click="jumpControlPage"
        >总览</span
      >
    </li>
    <!-- 风场页面页签 -->
    <template v-if="!isNoneWindPark && !parent.isControl">
      <!-- 风场 -->
      <li class="wind_farm_tag">
        <span
          :style="{
            cursor: 'default',
            color: '#1fffff',
            fontWeight: 'bold'
          }"
          @click="jumpWindpark"
          >{{ selectWind.name }}</span
        >
        <el-popover placement="bottom" width="auto" trigger="click">
          <div class="more_wind_card">
            <div
              class="wind_item"
              v-for="item in windOptions"
              :key="item.id || item.stationID || item.code"
              @mouseenter="mouseWind = item"
              @mouseleave="mouseWind = ''"
              @click="WindChange(item)"
              :style="{
                background:
                  getWindName(item) == getWindName(selectWind) || getWindName(item) == getWindName(mouseWind)
                    ? 'rgba(71, 86, 128, 0.5)'
                    : 'transparent'
              }"
            >
              {{ getWindName(item) }}
            </div>
          </div>
          <template #reference>
            <span class="wind_dropdown_arrow"></span>
          </template>
        </el-popover>
      </li>

      <!-- 机组 -->
      <li class="wind_farm_tag" v-show="isControl">
        <span> 全部 </span>
        <el-popover placement="bottom" width="auto" trigger="click">
          <div class="more_turbine_card">
            <span
              class="turbine_item"
              v-for="item in turbineList"
              :key="item"
              @mouseenter="mouseTurbine = item"
              @mouseleave="mouseTurbine = ''"
              @click="TurbineChange(item)"
              @dblclick="getdbClickEntity(item)"
              :style="{
                background:
                  item.windturbineName == windTurbineName ||
                  item.windturbineName == mouseTurbine.windturbineName
                    ? 'rgba(31, 255, 255, 0.2)'
                    : 'transparent',
                borderColor:
                  item.windturbineName == windTurbineName ||
                  item.windturbineName == mouseTurbine.windturbineName
                    ? 'rgba(31, 255, 255, 1)'
                    : 'transparent'
              }"
            >
              <span :style="{ background: levelColor[item.windturbineStatus] }"></span>
              {{ item.windturbineName }}
            </span>
          </div>
          <template #reference>
            <span class="wind_dropdown_arrow"></span>
          </template>
        </el-popover>
      </li>
    </template>

    <!-- 部件页面页签 -->
    <template v-else-if="isNoneWindPark">
      <!-- 风场 -->
      <li class="wind_farm_tag">
        <span
          :style="{
            cursor: 'pointer',
            color: 'rgba(31, 255, 255, 1)',
            fontWeight: 'bold'
          }"
          @click="jumpWindpark"
          >{{ selectWind.name }}</span
        >
        <el-popover placement="bottom" width="auto" trigger="click">
          <div class="more_wind_card">
            <div
              class="wind_item"
              v-for="item in windOptions"
              :key="item.id || item.stationID || item.code"
              @mouseenter="mouseWind = item"
              @mouseleave="mouseWind = ''"
              @click="WindChange(item)"
              :style="{
                background:
                  getWindName(item) == getWindName(selectWind) || getWindName(item) == getWindName(mouseWind)
                    ? 'rgba(71, 86, 128, 0.5)'
                    : 'transparent'
              }"
            >
              {{ getWindName(item) }}
            </div>
          </div>
          <template #reference>
            <i
              class="el-icon-arrow-down"
              style="cursor: pointer; color: rgba(255, 255, 255, 0.5); font-size: 12px"
            ></i>
          </template>
        </el-popover>
      </li>

      <!-- 机组 -->
      <li class="wind_farm_tag">
        <span
          :style="{
            color: 'rgba(31, 255, 255, 1)',
            fontWeight: 'bold'
          }"
          >{{ windTurbineName }}</span
        >
        <el-popover placement="bottom" width="auto" trigger="click">
          <div class="more_turbine_card">
            <span
              class="turbine_item"
              v-for="item in turbineList"
              :key="item"
              @mouseenter="mouseTurbine = item"
              @mouseleave="mouseTurbine = ''"
              @click="TurbineChange(item)"
              :style="{
                background:
                  item.windturbineName == windTurbineName ||
                  item.windturbineName == mouseTurbine.entityName
                    ? 'rgba(31, 255, 255, 0.2)'
                    : 'transparent',
                borderColor:
                  item.windturbineName == windTurbineName ||
                  item.windturbineName == mouseTurbine.entityName
                    ? 'rgba(31, 255, 255, 1)'
                    : 'transparent'
              }"
            >
              <span :style="{ background: levelColor[item.windturbineStatus] }"></span>
              {{ item.windturbineName }}
            </span>
          </div>
          <template #reference>
            <i
              class="el-icon-arrow-down"
              style="cursor: pointer; color: rgba(255, 255, 255, 0.5); font-size: 12px"
            ></i>
          </template>
        </el-popover>
      </li>

      <!-- 聚合部件 -->
      <li class="wind_farm_tag" v-if="isNoneWindPark">
        <span style="color: #1fffff; font-weight: bold">{{ selectPagecomp.entityName }}</span>

        <el-popover placement="bottom" width="auto" trigger="click">
          <div class="more_wind_card">
            <div
              class="wind_item"
              v-for="item in pagecompList"
              :key="item"
              @mouseenter="mousePagecomp = item"
              @mouseleave="mousePagecomp = ''"
              @click="pagecompChange(item)"
              :style="{
                background:
                  item.type == selectedComp || item.name == mousePagecomp.name
                    ? 'rgba(71, 86, 128, 0.5)'
                    : 'transparent'
              }"
            >
              {{ item.name }}
            </div>
          </div>
          <template #reference>
            <i
              class="el-icon-arrow-down"
              style="
                cursor: pointer;
                color: rgba(255, 255, 255, 1);
                font-size: 12px;
                font-weight: bold;
              "
            ></i>
          </template>
        </el-popover>
      </li>
    </template>
  </ul>
</template>

<script>
import { windFieldStatusApi } from '@/api/screen/index.js'
import { mapGetters } from 'vuex'
import { getStore } from '@/util/store'
import { levelColor } from '@/util/constant.js'
import { getModelPlanApi } from '@/api/WindTurbine/CenterPartAPI.js'

export default {
  props: {
    windTurbineId: {
      type: String,
      default: ''
    },
    windTurbineName: {
      type: String,
      default: ''
    },
    selectedComp: {
      type: String,
      default: ''
    },
    selectPagecomp: {
      type: Object,
      default: () => {}
    },
    // 判断是否在风场页面
    isNoneWindPark: {
      type: Boolean,
      default: true
    },
    // 在风场页面，判断是否展示机组级
    isControl: {
      type: Boolean,
      default: false
    }
  },
  inject: ['parent'],
  components: {},
  data() {
    return {
      levelColor,
      timer: null,

      selectWind: '', // 选中的风场

      // windList: [], //全部风场列表
      turbineList: [], //全部机组列表
      pagecompList: [], // 全部聚合部件列表

      mouseWind: '', //鼠标移入风场
      mouseTurbine: '', //鼠标移入机组
      mousePagecomp: '', //鼠标移入聚合部件
      doubleClick: false
    }
  },
  watch: {
    windTurbineId: {
      handler() {
        if (this.isNoneWindPark) {
          this.getPagecompByPlan()
        }
      },
      immediate: true
    },
    '$route.query.locationcode'(newQuery) {
      const windObj = this.findWindOption(newQuery)
      if (windObj) {
        this.selectWind = this.normalizeWindOption(windObj)
      }
    },
    windOptions() {
      const windObj = this.findWindOption(this.$route.query.locationcode)
      if (windObj) {
        this.selectWind = this.normalizeWindOption(windObj)
      }
    }
  },
  computed: {
    ...mapGetters(['userInfo', 'userDeptTree', 'permission']),
    windOptions() {
      return this.parent?.windparkList?.length ? this.parent.windparkList : this.userDeptTree
    }
  },
  created() {
    let windFarmID = this.$route.query.locationcode
    let windObj = this.findWindOption(windFarmID)
    this.selectWind = windObj ? this.normalizeWindOption(windObj) : this.normalizeWindOption(this.windOptions[0])
  },
  mounted() {
    if (this.isNoneWindPark) {
      this.initTurbineList(this.selectWind)
      this.getPagecompByPlan()
    } else {
    }
  },
  methods: {
    getWindName(item) {
      return item?.name || item?.stationName || ''
    },
    normalizeWindOption(item) {
      if (!item) return null
      const windFarmCode =
        item.stationID || item.stationId || item.stationCode || item.code || item.id
      return {
        ...item,
        id: item.id || windFarmCode,
        code: item.code || windFarmCode,
        stationID: item.stationID || windFarmCode,
        name: item.name || item.stationName
      }
    },
    findWindOption(locationcode) {
      if (!locationcode) return null
      return this.windOptions.find(
        item =>
          item.id == locationcode ||
          item.code == locationcode ||
          item.stationID == locationcode ||
          item.stationId == locationcode ||
          item.stationCode == locationcode
      )
    },
    getPagecompByPlan(turbineId) {
      let list = []
      let allList = JSON.parse(getStore({ name: 'allPagecompList' }))
      getModelPlanApi({ windturbineId: turbineId || this.windTurbineId }).then(res => {
        let planList = res.data.data?.measloc || []
        if (!planList.length) return
        planList.forEach(item => {
          list.push({
            name: item.deviceModelType,
            typeCode: allList.find(j => j.entityName == item.deviceModelType).entityType
          })
        })
        let noTurbineList = planList.filter(j => j.deviceModelType !== '整机')
        if (noTurbineList.length < 2) {
          list = list.filter(j => j.deviceModelType !== '整机')
        }
      })
      this.pagecompList = list
    },

    getEnitiyTreeData() {
      let selectWindObj = null
      if (!this.userDeptTree) return
      if (this.selectWind && this.selectWind.id) {
        selectWindObj = this.userDeptTree.find(i => i.id == this.selectWind.id)
      }
      if (!selectWindObj) {
        selectWindObj = this.userDeptTree[0]
      }
      if (selectWindObj.childNode) {
        let { childNode, ...others } = selectWindObj
        this.selectWind = others
        this.initTurbineList(this.selectWind)
      }
      // })
    },

    initTurbineList(val) {
      // 新接口 val.code
      const stationId = val?.stationID || val?.stationId || val?.stationCode || val?.code || val?.id
      if (!stationId) {
        this.turbineList = []
        return
      }
      windFieldStatusApi({ stationId }).then(res => {
        if (res.data.code === 200) {
          this.turbineList = res.data.data
        }
      })
    },

    async WindChange(val) {
      const selectedWind = this.normalizeWindOption(val)
      const deptWind = this.userDeptTree.find(
        i => i.id == selectedWind.id || i.code == selectedWind.code || i.stationID == selectedWind.stationID
      )
      const childNode = deptWind?.childNode || []
      const { childNode: _childNode, ...others } = deptWind || selectedWind
      await this.initTurbineList(selectedWind) //childNode
      this.selectWind = this.normalizeWindOption(others)
      if (this.isNoneWindPark) {
        this.$emit('ClickEntityWind', { ...this.selectWind, ...(childNode[0] || {}) })
      } else {
        this.parent.updateRoute(
          this.parent.isControl,
          this.parent.isMapShow,
          this.selectWind.stationID ||
            this.selectWind.stationId ||
            this.selectWind.stationCode ||
            this.selectWind.code ||
            this.selectWind.id
        )
      }
      this.$refs.closePopover.click()
    },

    TurbineChange(val) {
      setTimeout(() => {
        if (!this.doubleClick) {
          if (this.timer == null) {
            let that = this
            if (that.isNoneWindPark) {
              this.$emit('ClickEntityWind', { ...val, ...this.selectWind })
            } /* else {
              this.$emit('getClickEntity', val)
            } */
          } else {
            clearTimeout(this.timer)
            this.timer = null
          }
          this.$refs.closePopover.click()
        }
        this.doubleClick = false
      }, 200)
    },

    getdbClickEntity(val) {
      this.doubleClick = true
      if (this.timer) clearTimeout(this.timer)
      this.$emit('turbineJump', val.windturbineId || val.entityId)
    },

    //2、选中聚合部件改变
    pagecompChange(value) {
      this.$emit('changeCurrentModel', value.typeCode)
      this.$refs.closePopover.click()
    },

    // 跳转风场页面
    jumpWindpark() {
      this.$router.push({
        path: '/screen',
        query: {
          isControl: false,
          isMapShow: false,
          locationcode:
            this.selectWind.stationID ||
            this.selectWind.stationId ||
            this.selectWind.stationCode ||
            this.selectWind.code ||
            this.selectWind.id
        }
      })
    },
    jumpControlPage() {
      // 首页进行地图重置
      if (this.parent?.isMapShow !== undefined) {
        this.parent.updateRoute(true, this.$route.query.isMapShow, 'all')
      } else {
        // 首页重新加载
        this.$router.push({
          path: '/screen'
        })
      }
    }
  }
}
</script>

<style lang="less" scoped>
.cssNav {
  width: 100%;
  display: inline-block;
  display: -webkit-box;
  display: -ms-flexbox;
  li {
    position: relative;
    flex-wrap: nowrap;
    // padding-left: 5px;
    color: #fff;
    background: #102947;
    height: 35px !important;
    line-height: 35px !important;
    padding-left: 5px;
    &::after {
      content: '';
      position: absolute;
      right: -15px;
      top: 0;
      z-index: 10;
      display: block;
      border-top: 17px solid transparent;
      border-bottom: 18px solid transparent;
      border-left: 15px solid #102947;
    }
    &::before {
      content: '';
      position: absolute;
      left: -15px;
      top: 0;
      display: block;
      border-top: 17px solid #102947;
      border-bottom: 18px solid #102947;
      border-right: 15px solid transparent;
      transform: rotateY(180deg);
    }
    &:first-child::before {
      display: none;
    }
    /* &:last-child {
      color: #1fffff;
      font-weight: bold;
    } */
    span {
      font-size: 18px;
      line-height: 28px;
      letter-spacing: 0em;
      color: rgba(31, 255, 255, 1);
      font-weight: 350;
      margin: 5px;
    }
  }
  li + li {
    margin-left: 30px;
  }
}

.wind_farm_tag {
  display: flex;
  align-items: center;
  .wind_inform {
    position: relative;
    left: 3px;
    top: 4px;
    cursor: pointer;
    display: inline-block;
    width: 35px;
    height: 20px;
  }
  .wind_dropdown_arrow {
    width: 0;
    height: 0;
    margin-left: 4px;
    margin-right: 5px;
    cursor: pointer;
    border-left: 5px solid transparent;
    border-right: 5px solid transparent;
    border-top: 6px solid rgba(255, 255, 255, 0.65);
  }
}

.more_wind_card {
  width: auto;
  max-height: 220px !important;
  background-color: rgba(9, 39, 76, 0.6) !important;
  backdrop-filter: blur(10px);
  overflow-y: auto;
  padding: 10px;
  .wind_item {
    height: 36px;
    line-height: 36px;
    padding-left: 10px;
    font-size: 14px;
    color: white;
    cursor: pointer;
  }
}

.more_turbine_card {
  width: 440px !important;
  max-height: 250px !important;
  background-color: rgba(9, 39, 76, 0.6) !important;
  backdrop-filter: blur(10px);
  overflow-y: auto;
  padding: 10px;
  .turbine_item {
    display: inline-block;
    width: 67px;
    height: 35px;
    line-height: 35px;
    text-align: center;
    color: white;
    border-radius: 5px;
    margin: 0 5px;
    cursor: pointer;
    border: 1px solid;
    span {
      height: 10px;
      width: 10px;
      border-radius: 50%;
      display: inline-block;
    }
  }
}
</style>
