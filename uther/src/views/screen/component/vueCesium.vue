<template>
  <div ref="viewerContainer" class="viewer">
    <!--  @move-end="cameraMoveEnd" -->
    <!--  @LEFT_DOUBLE_CLICK="onDoubleClick" -->
    <vc-viewer
      ref="vcViewer"
      :logo="false"
      :context-options="contextOptions"
      :animation="animation"
      :should-animate="shouldAnimate"
      :order-independent-translucency="false"
      @ready="onViewerReady"
      @LEFT_CLICK="onClick"
      @MOUSE_MOVE="onhover"
      @move-end="cameraMoveEnd"
    >
      <!-- 测绘工具 -->
      <vc-measure-distance
        ref="measureDistance"
        :clamp-to-ground="clampToGround"
        :remove-last-position="removeLastPosition"
        @activeEvt="activeEvt"
        @measureEvt="measureEvt"
        @movingEvt="movingEvt"
      />
      <vc-measure-area
        ref="measureArea"
        :clamp-to-ground="clampToGround"
        :remove-last-position="removeLastPosition"
        @activeEvt="activeEvt"
        @measureEvt="measureEvt"
        @movingEvt="movingEvt"
      />
      <vc-measure-height
        ref="measureHeight"
        @activeEvt="activeEvt"
        @measureEvt="measureEvt"
        @movingEvt="movingEvt"
      />
      <!-- 在线地图 -->
      <!--  地名标注层 -->
      <!--   <vc-layer-imagery :alpha="1" :brightness="1" :contrast="1" :sort-order="20">
        <vc-provider-imagery-tianditu
          map-style="cva_w"
          token="436ce7e50d27eede2f2929307e6b33c0"
        ></vc-provider-imagery-tianditu>
      </vc-layer-imagery> -->
      <!-- 影像层 -->
      <!--   <vc-layer-imagery :alpha="1" :brightness="1" :contrast="1" :sort-order="10">
        <vc-provider-imagery-tianditu
          map-style="img_w"
          token="436ce7e50d27eede2f2929307e6b33c0"
        ></vc-provider-imagery-tianditu>
      </vc-layer-imagery> -->

      <!-- 地形在线 -->
      <!--   <vc-provider-terrain-arcgis-tiled-elevation
        url="https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer"
      ></vc-provider-terrain-arcgis-tiled-elevation>
      <vc-navigation :options="navOptions" /> -->
      <slot name="pop" />
      <slot name="png" />
      <!-- 弹窗用 infobox -->
    </vc-viewer>
    <!--   v-if="popup.show" -->
    <div v-if="popup.show" class="my_popupContent" :style="popup.style">
      <unit-card :windparkInfo="hoverWindparkInfo" />
    </div>
    <div v-if="measureIsShow" class="measure">
      <div>
        <el-button size="small" @click="toggle('measureDistance')">
          {{ distanceMeasuring ? '停止' : '距离' }}
        </el-button>
      </div>
      <div>
        <el-button size="small" @click="toggle('measureArea')">
          {{ areaMeasuring ? '停止' : '面积' }}
        </el-button>
      </div>
      <div>
        <el-button size="small" @click="toggle('measureHeight')">
          {{ heightMeasuring ? '停止' : '高度' }}
        </el-button>
      </div>
      <div>
        <el-button size="small" @click="clear">清除</el-button>
      </div>
      <div>
        <span style="color: white">贴地</span>
        <el-switch v-model="clampToGround" />
      </div>
    </div>
  </div>
</template>
<script>
import { mapGetters } from 'vuex'
import 'vue-cesium/lib/vc-navigation.css'
import 'vue-cesium/lib/style.css'
import isEqual from 'lodash/isEqual'
import { getUserMapInfo } from '@/api/screen/index'
import unitCard from './cardUnit.vue'
export default {
  components: {
    unitCard
  },
  props: {
    measureIsShow: {
      type: Boolean,
      default: false
    },
    isControl: {
      type: Boolean,
      default: true
    },
    windparkList: {
      type: Array,
      default: () => []
    },
    userMapInfo: {
      type: Object,
      default: () => {}
    }
  },
  inject: ['parent'],
  watch: {
    windparkList: {
      handler(newVal, olderVal) {
        if (isEqual(newVal, olderVal)) return
        this.initWindparkMarks()
      },
      deep: true
    },
    isControl: {
      handler() {
        this.initMapView()
      }
    },
    'parent.WindFarm'() {
      this.initMapView()
    }
  },
  computed: {
    ...mapGetters(['userInfo'])
  },
  data() {
    return {
      popup: { show: false, style: {} },
      hoverWindparkInfo: {},
      contextOptions: {
        webgl: {
          preserveDrawingBuffer: true,
          alpha: true
        }
      },
      navOptions: {
        enableCompass: false, // 指南针
        enableZoomControl: false, // 缩放
        enableDistanceLegend: true, // 比例尺
        enableLocationBar: true, // 经纬度位置
        enableCompassOuterRing: true,
        enablePrintView: false, // 打印当前视角
        enableMyLocation: false // 定位当前位置
      },
      // 测量
      distanceMeasuring: false,
      areaMeasuring: false,
      heightMeasuring: false,
      clampToGround: false,
      removeLastPosition: true,
      shouldAnimate: true,
      animation: false,
      timer: null,
      siteInfoDom: {},
      siteInfoPosition: {},
      rightClickSiteBillboardId: '',
      popTitle: '',
      mapStyle: '6',
      ltype: '0',
      alpha: 1,
      brightness: 1,
      contrast: 1,
      projectionTransforms: {
        form: 'BD09',
        to: 'WGS84'
      }
    }
  },
  mounted() {
    // eslint-disable-next-line no-unused-vars
    this.$refs.vcViewer.createPromise.then(({ Cesium, viewer }) => {
      // 获取集控风场的经纬度创建风场广告牌
      //  this.$emit('fMethod')
      this.initMapView()
    })
  },
  destroyed() {
    // this.$refs.vcViewer.unload() // vue-cesium 1.x 提供的卸载
    setTimeout(() => {
      this.$utils.map.unInit()
    }, 1000)
  },
  methods: {
    async onViewerReady({ Cesium, viewer }) {
      // 禁用相机惯性
      viewer.scene.screenSpaceCameraController.enableInertia = false
      // ==end==
      this.$utils.map.initConfig(Cesium, viewer)
    },
    async initMapView() {
      // console.log('initMapView')
      if (window.viewer) {
        if (this.$utils.map.FJDataArr?.length) {
          this.$utils.map.delFjModel() // 先清除风机实体
        }
        if (this.isControl) {
          // 默认进来是集控云南地图
          getUserMapInfo({ userID: this.userInfo.user_id }).then(res => {
            if (res.data.data) {
              if (this.$utils.map.markList?.length) {
                this.$utils.map.resetMapStatus(res.data.data)
              } else {
                this.$utils.map.initAniMat(res.data.data, () => {
                  this.initWindparkMarks()
                })
              }
            }
          })
        } else {
          this.parent.changeFarmVisual()
        }
      }
    },
    async initWindparkMarks() {
      if (this.windparkList.length === 0) return
      if (this.$utils.map.markList.length) return
      this.windparkList.forEach(r => {
        this.$utils.map.addImgAndLabel({
          baseInfo: {
            id: r.stationID
          },
          positions: [Number(r.longitude), Number(r.latitude), 20],
          name: r.stationName,
          img: '/img/screen/windparkicon.png',
          imgSize: [19, 23],
          offset: 20,
          flag: 'windpark',
          isDistanceDisplayCondition: true
        })
      })
    },
    onhover(e) {
      const pickedFeature = window.viewer.scene.pick(e.endPosition)
      if (
        window.Cesium.defined(pickedFeature) &&
        pickedFeature.id &&
        pickedFeature.id._flag === 'windpark'
      ) {
        // 世界坐标 → 屏幕坐标
        const winPos = window.Cesium.SceneTransforms.wgs84ToWindowCoordinates(
          window.viewer.scene,
          pickedFeature.id._position._value
        )
        this.hoverWindparkInfo = {
          ...this.windparkList.find(item => item.stationID === pickedFeature.id._id),
          style: {
            '--block-width': `220px`,
            '--block-height': `150px`
          }
        }
        this.popup = {
          show: true,
          style: {
            left: winPos.x + 180 + 'px',
            top: winPos.y + 50 + 'px',
            transform: 'translate(-50%,-100%)'
          }
        }
      } else {
        this.popup.show = false
      }
    },
    cameraMoveEnd() {
      this.getcameraPosInfo()
    },
    // 获取相机位置，姿态等
    getcameraPosInfo() {
      const cameraPoston = {
        position: window.viewer.camera.position,
        head: window.viewer.scene.camera.heading,
        pitch: window.viewer.scene.camera.pitch,
        roll: window.viewer.scene.camera.roll
      }
      /*console.log("🚀 ~ file: vueCesium.vue:184 ~ getcameraPosInfo ~ this.$utils.map.cartesianTolngLatAlt(window.viewer.camera.position):", this.$utils.map.cartesianTolngLatAlt(window.viewer.camera.position))*/
      // this.$emit('cameraMoveEnd', cameraPoston)
    },
    /*  doubleClick(e) {
      if (this.timer) {
        this.$emit('leftDoubleClick', e)
        // 取消上次延时未执行的方法
        window.clearTimeout(this.timer)
      }
    }, */
    onClick(e) {
      if (this.timer) {
        window.clearTimeout(this.timer)
        this.timer = null
      }
      const pickedFeature = window.viewer.scene.pick(e.position)
      if (window.Cesium.defined(pickedFeature) && pickedFeature.id) {
        if (pickedFeature.id._flag === 'windpark') {
          this.$emit('mapWindparkClick', pickedFeature.id._id)
          /*     this.$utils.map.AreaMap.show = false
          this.$utils.map.AreaMapBorder.show = false
          this.$utils.map.markList.forEach(e => (e.show = false)) */
        } else if (pickedFeature.id._flag === 'fjModel') {
          this.$emit('mapturbineClick', pickedFeature.id._id.split('-')[1])
        }
        return
      }
    },
    toggle(type) {
      this.$refs[type].measuring = !this.$refs[type].measuring
    },
    clear() {
      this.$refs.measureDistance.clear()
      this.$refs.measureArea.clear()
      this.$refs.measureHeight.clear()
    },
    activeEvt(_) {
      this[_.type] = _.isActive
    },
    measureEvt(result) {
      console.log(result)
    },
    movingEvt(position) {
      console.log(position)
    }
  }
}
</script>
<style lang="scss" scoped>
.my_popupContent {
  position: absolute;
  padding: 10px;
  pointer-events: auto;
  width: 280px;
  height: 150px;
  z-index: 999;
}
@keyframes fontColor {
  0% {
    color: #ffffff00;
    text-shadow: 1px 1px 5px #00252000;
  }
  40% {
    color: #ffffff00;
    text-shadow: 1px 1px 5px #00252000;
  }
  100% {
    color: #ffffff;
    text-shadow: 1px 1px 5px #002520d2;
  }
}

@keyframes slide {
  0% {
    border: 1px solid #38e1ff00;
    background-color: #38e1ff00;
    box-shadow: 0 0 10px 2px #29baf100;
  }

  100% {
    border: 1px solid #38e1ff;
    background-color: #38e1ff4a;
    box-shadow: 0 0 10px 2px #29baf1;
  }
}
@keyframes area {
  0% {
    width: 0%;
  }
  25% {
    width: 0%;
  }
  100% {
    width: 95%;
  }
}
#cesiumContainer {
  background: url('/img/skyBoxImg.png') no-repeat;
  background-size: 100% 100%;
}
.viewer {
  height: 100vh;
  position: relative;
}
.measure {
  position: absolute;
  top: 20px;
  left: 10px;
  display: flex;
  align-items: center;
}
.measure div {
  margin-right: 5px;
}
.measure button {
  background: #41404e;
  border-color: #41404e;
  color: #9d9f9e;
}
::v-deep .el-switch.is-checked .el-switch__core {
  border-color: #41404e !important;
  background-color: #41404e !important;
}
::v-deep .vc-location-distance {
  z-index: 11;
  right: 470px !important;
  button {
    display: none;
  }
}
</style>
