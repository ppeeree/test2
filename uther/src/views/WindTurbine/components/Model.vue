<template>
  <div id="fan_Container">
    <!-- 存放three.js的渲染效果 -->
    <div id="container" ref="container"></div>

    <!-- 卡片DOM -->
    <div ref="markDom" id="markDom" style="height: 100%; width: 100%">
      <valueComponent
        ref="compDom"
        v-bind="$attrs"
        v-if="currentComp !== 'windturbine' && animate"
        :currentComp="currentComp"
        :isStopTimer="isStopTimer"
        @allValueList="allValueList"
      ></valueComponent>
      <div v-if="currentComp == 'windturbine' && animate">
        <turbineCard
          class="turbine_fault_card"
          v-for="item in turbineCompCardData"
          :key="item.pagecompCode"
          :id="item.pagecompCode"
          :style="{ left: item.boxLeft + 'px', top: item.boxTop + 'px' }"
          :data="item"
        ></turbineCard>
        <!-- <noData v-else></noData> -->
      </div>
    </div>

    <!-- 测点DOM -->
    <span
      v-for="item in allCompValueList[currentComp]"
      :key="item.spotId"
      :id="item.spotId"
      class="comp_spot"
      v-show="item.top && item.left"
      :style="{
        top: item.top + 'px',
        left: item.left + 'px'
      }"
    ></span>

    <!-- 顶部按钮 -->
    <!--  <div class="btn_comp">
      <div
        v-for="item in btnNameArr"
        :key="item.index"
        class="btn_change"
        :style="handleMenuBtnStyle(item)"
        @click="clickChangeBtn(item.name, item.isShow)"
      >
        <span class="btn_change_text">{{ setCompBtnName[item.name] }}</span>
      </div>
    </div> -->
  </div>
</template>

<script>
import * as THREE from 'three'
import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls'
import { DRACOLoader } from 'three/examples/jsm/loaders/DRACOLoader'
import { GLTFLoader } from 'three/examples/jsm/loaders/GLTFLoader'
import { TweenMax, Linear } from 'gsap'
import LeaderLine from 'leader-line'
import { CSS2DRenderer, CSS2DObject } from 'three/examples/jsm/renderers/CSS2DRenderer'
// import { DragControls } from 'three/examples/jsm/controls/DragControls.js'
// import { CSS3DRenderer, CSS3DObject } from 'three/examples/jsm/renderers/CSS3DRenderer'

import dayjs from 'dayjs'
import { levelColor, eventTypeEnum } from '@/util/constant'
import {
  getCompStatusApi,
  getModelPlanApi,
  getLatestTurbine
} from '@/api/WindTurbine/CenterPartAPI.js'

import { setStore, getStore } from '@/util/store'
// import { getEnitiyTree } from '@/api/screen/index'
import { mergeSteelList } from '../common'

import valueComponent from '../valueComponent/index.vue'
import noData from '@/components/noData/index.vue'
import turbineCard from '../turbineCard'

let scene = new THREE.Scene()
let renderer = new THREE.WebGLRenderer({
  antialias: true,
  alpha: true,
  logarithmicDepthBuffer: true
})
let cssRenderer = new CSS2DRenderer()
let loader = new GLTFLoader()

let camera, controls
let turbine = new THREE.Object3D()
let allSpot = new THREE.Object3D()
let arrNeedOutLine = []

export default {
  props: {
    /*  changeSize: {
      type: String,
      require: true,
      default: ''
    }, */
    entityId: {
      type: String,
      default: '',
      require: true
    },
    entityStatus: {
      type: String,
      default: 'normal',
      require: true
    },
    windTurbine: {
      type: String,
      default: '',
      require: true
    },
    isStopTimer: {
      type: Boolean,
      default: false
    },
    currentComp: {
      type: String,
      default: 'windturbine',
      require: true
    }
  },
  components: {
    valueComponent,
    noData,
    turbineCard
  },
  provide() {
    return {
      getCardPosition: this.getCardPosition,
      startHandleValue: this.startHandleValue
    }
  },
  inject: ['reload'],
  data() {
    return {
      levelColor,
      eventTypeEnum,
      dom: null,
      timer: null,

      turbineCompCardData: [], // 整机页面卡片数组

      allLine: [],
      allCompValueList: {}, // 不同页面的连线对象
      allDeviceList: [], // 该机组模型列表
      turbinePlanList: [], // 该机组全部方案
      allPagecompList: [], // 该机组所有聚合部件
      promiseAddModel: [], // 模型是否加载完成
      planPromise: '', // 是否调用成功
      animate: false, // 判断动画是否完成

      statusList: ['danger', 'warning', 'attention', 'nostate'],
      newSetCompBtnName: {
        windturbine: '整机',
        NAC: '机舱',
        TWW: '塔筒',
        ROT: '风轮',
        YPB: '变桨与偏航'
      },

      btnNameArr: [], //顶部按钮
      turbineSpot: [],

      fetchingData: false // 控制是否获取数据的标志位
    }
  },
  watch: {
    currentComp: {
      handler(val) {
        this.deleteLine()
        this.turbineCompCardData = []
        this.allCompValueList = {}

        // console.log('部件改变', val, this.allSpot)
        this.animate = false
        this.changeComp(val)
      },
      deep: true
    },
    windTurbine: {
      handler(val) {
        this.deleteLine()
        this.turbineCompCardData = []
        this.allCompValueList = {}
        // console.log('机组改变', val, this.allCompValueList)
        this.animate = false
        this.initModelPath()
      },
      deep: true
    }
  },
  created() {
    this.allPagecompList = JSON.parse(getStore({ name: 'allPagecompList' }))
  },
  updated() {
    Promise.all(this.promiseAddModel).then(() => {
      let length =
        this.allCompValueList[this.currentComp] && this.allCompValueList[this.currentComp].length
      if (this.allLine.length >= length) {
        this.renderSpot()
      } else {
        this.judgeAddSpotLine(this.currentComp)
      }
    })
  },
  computed: {
    currentCompName() {
      return this.allPagecompList.find(j => j.entityType == this.currentComp).entityName
    }
  },
  mounted() {
    this.init()
    this.render()
    this.initModelPath()

    window.addEventListener('resize', this.onWindowResize.bind(this))
    this.onWindowResize()

    let fdom = document.getElementById('fan_Container')
    fdom.addEventListener('dblclick', this.mouseDBClick.bind(this))
  },
  beforeUnmount() {
    // console.log('页面销毁')
    scene.children = []
    let self = this.$refs['markDom']
    self.parentElement.removeChild(self)
    this.turbineCompCardData = []
    turbine.children = []
    arrNeedOutLine = []
    this.deleteLine()
    this.clearTimer()
  },
  methods: {
    init() {
      this.dom = this.$refs['container']
      // const axes = new THREE.AxesHelper(50)
      // scene.add(axes)

      this.initCamera()
      this.initLight()
      this.initRender()

      //控制器
      controls = new OrbitControls(camera, cssRenderer.domElement)

      turbine.scale.set(0.01, 0.01, 0.01)
      scene.add(turbine)
    },
    initCamera() {
      camera = new THREE.PerspectiveCamera(
        45,
        this.dom.offsetWidth / this.dom.offsetHeight,
        0.001,
        10000
      )
      camera.position.set(25, 25, 25)
    },
    initLight() {
      const directLight = new THREE.DirectionalLight('#ffffff', 0.5)
      const directLight1 = new THREE.DirectionalLight('#ffffff', 0.5)
      const ambientLight = new THREE.AmbientLight('#ffffff', 0.5)

      directLight.position.set(15, 15, 15)
      directLight1.position.set(-15, -15, 15)
      ambientLight.position.set(0, 0, -5)

      scene.add(directLight, directLight1, ambientLight)
    },
    initRender() {
      renderer.setSize(this.dom.offsetWidth, this.dom.offsetHeight)
      renderer.outputEncoding = THREE.sRGBEncoding
      this.dom.appendChild(renderer.domElement)

      let compDiv = this.$refs['markDom']
      let compTag = new CSS2DObject(compDiv)
      scene.add(compTag)

      cssRenderer.setSize(this.dom.offsetWidth, this.dom.offsetHeight)
      cssRenderer.domElement.style.position = 'absolute'
      cssRenderer.domElement.style.top = 0
      this.dom.appendChild(cssRenderer.domElement)
    },
    render() {
      this.renderSpot()
      requestAnimationFrame(this.render.bind(this))
      controls.update()
      cssRenderer.render(scene, camera)
      renderer.render(scene, camera)
    },
    //自适应屏幕
    onWindowResize() {
      let width = this.dom.offsetWidth
      let height = this.dom.offsetHeight
      camera.aspect = width / height
      camera.updateProjectionMatrix()
      renderer.setSize(width, height)
      cssRenderer.setSize(width, height)
    },
    // 清除定时器
    clearTimer() {
      clearInterval(this.timer)
      this.fetchingData = false
      this.timer = null
    },

    //1、获取该机组设备和方案
    initModelPath() {
      this.turbineCompCardData = []
      turbine.children = []
      arrNeedOutLine = []
      this.deleteLine()

      this.planPromise = new Promise(resolve => {
        getModelPlanApi({ windturbineId: this.windTurbine }).then(res => {
          if (res.data.code === 200) {
            this.allDeviceList = res.data.data.model // 全部设备数组
            this.turbinePlanList = res.data.data.measloc
            // .sort(function (a, b) {
            //   return new String(a.sort) - new String(b.sort)
            // })
            // console.log('1、model_初始化方案')
            let isComp = this.turbinePlanList.find(j => j.deviceModelType == this.currentCompName)

            if (!isComp) {
              this.$message({
                type: 'warning',
                message: '该机组没有' + this.newSetCompBtnName[this.currentComp] + '监测！'
              })
              let newType =
                this.turbinePlanList[0].deviceCode !== ''
                  ? this.turbinePlanList[0].deviceCode
                  : this.allPagecompList.find(
                      j => j.entityName == this.turbinePlanList[0].deviceModelType
                    ).entityType
              this.$emit('changeCurrentModel', newType)
            } else {
              this.changeComp(this.currentComp)
            }
            resolve(this.turbinePlanList)
          }
        })
      })
    },
    // 1-2-1、遍历添加模型
    addModel(list) {
      let that = this

      list.forEach(item => {
        let newPath = item.path.replace(new RegExp('amp;'), '')
        let model = new Promise(resolve => {
          loader
            .setDRACOLoader(new DRACOLoader().setDecoderPath('/js/draco/gltf/'))
            .load(newPath, function (gltf) {
              that.changeObjName(gltf.scene, item.name)
              if (item.scale) {
                gltf.scene.scale.set(item.scale[0], item.scale[1], item.scale[2])
              }
              if (item.position) {
                gltf.scene.position.set(item.position[0], item.position[1], item.position[2])
              }
              if (item.rotate) {
                gltf.scene.rotation.set(item.rotate[0], item.rotate[1], item.rotate[2])
              }
              turbine.add(gltf.scene)
              resolve(gltf.scene)
            })
        })
        this.promiseAddModel.push(model)
      })

      Promise.all(this.promiseAddModel).then(() => {
        let length =
          this.allCompValueList[this.currentComp] && this.allCompValueList[this.currentComp].length
        if (this.allLine.length >= length) {
          this.renderSpot()
        } else {
          this.judgeAddSpotLine(this.currentComp)
        }
        // this.settingModelTransparent()
      })
    },
    // 1-2-2、给所有模型增加名称
    changeObjName(obj, strName) {
      let material
      obj.name = strName
      obj &&
        obj.traverse(function (child) {
          if (child.isMesh) {
            child.name = strName
            material = child.material.clone(obj)
          }
        })
      // let cloneObj = obj.clone()
      arrNeedOutLine.push({
        name: strName,
        material: material,
        obj: obj
      })
    },
    // 1-2-3、通过名称查询三维模型
    findNameModel(name) {
      let obj
      arrNeedOutLine.forEach(item => {
        if (name == item.name) {
          obj = item
        }
      })
      return obj
    },
    // 1-2-4、设置模型透明度
    settingModelTransparent() {
      arrNeedOutLine.forEach(item => {
        if (item.name.includes('叶片') || item.name == '机舱' || item.name == '轮毂') {
          //console.log('设置透明度', item)
          item.obj.traverse(function (child) {
            if (child.isMesh) {
              child.material.transparent = true
              child.material.opacity = 0.2
            }
          })
        }
      })
    },

    // 2-1、双击模型
    mouseDBClick(event) {
      event.preventDefault()
      event.stopPropagation()

      let bladeName = [
        '轮毂',
        '叶片一',
        '叶片二',
        '叶片三',
        '变桨轴承一',
        '变桨轴承二',
        '变桨轴承三',
        '叶片前',
        '叶片后'
      ]
      let cabinName = ['齿轮箱', '发电机', '主轴', '机舱支架', '机舱']
      let towerName = [
        '塔顶',
        '塔筒左',
        '塔筒右',
        '塔基',
        '钢塔',
        '钢索左',
        '钢索右',
        '混塔右',
        '混塔左'
      ]

      //定义二维向量用于保存坐标
      const mouse = new THREE.Vector2()
      mouse.x = (event.offsetX / renderer.domElement.offsetWidth) * 2 - 1
      mouse.y = -(event.offsetY / renderer.domElement.offsetHeight) * 2 + 1
      //创建射线
      const raycaster = new THREE.Raycaster()
      raycaster.setFromCamera(mouse, camera)
      let intersects = raycaster.intersectObjects(turbine.children, true)

      if (intersects[0] === undefined) return
      let strSelectName = intersects[0].object.name

      let type
      // console.log('双击模型名称', strSelectName)

      let typeName = ''
      if (bladeName.find(j => j == strSelectName)) {
        typeName = '风轮'
        type = 'ROT'
      } else if (cabinName.find(j => j == strSelectName)) {
        typeName = '机舱'
        type = 'NAC'
      } else if (towerName.find(j => j == strSelectName)) {
        typeName = '塔筒'
        type = 'TWW'
      }
      if (this.turbinePlanList.find(j => j.deviceModelType == typeName)) {
        this.$emit('changeCurrentModel', type)
      } else {
        this.$message({
          type: 'warning',
          message: typeName + '部件没有被监测！'
        })
      }
    },
    // 2-2、切换部件后获取新模型展示列表
    changeComp(comp) {
      this.turbineCompCardData = []
      turbine.children = []
      arrNeedOutLine = []
      this.deleteLine()

      this.animate = false
      let compPath = []

      this.allDeviceList
        .find(j => j.compName == this.newSetCompBtnName[comp])
        .children[0].children[0].propertiesView.forEach(ele => {
          if (ele.isShow) {
            compPath.push(ele)
          }
        })

      let { cameraPosition, wholePosition } = compPath[0]
      this.addModel(compPath)
      TweenMax.to(camera.position, 1.5, {
        x: cameraPosition[0],
        y: cameraPosition[1],
        z: cameraPosition[2],
        repeat: 0
      })

      Promise.all(this.promiseAddModel).then(() => {
        TweenMax.to(turbine.position, 1.5, {
          x: wholePosition[0],
          y: wholePosition[1],
          z: wholePosition[2],
          repeat: 0,
          onComplete: () => {
            // 解决快速切换部件，导致测点卡片错位显示的问题
            if (this.currentComp !== comp) {
              return
            }
            // console.log('3、模型变化方法', comp)
            this.changeCompDetail(compPath)
          }
        })
      })
    },
    // 2-3、切换模型判断+动画效果
    changeCompDetail(data) {
      let action = []
      // console.log('4、动画效果方法', data)
      arrNeedOutLine.forEach(item => {
        let obj = item.obj
        let param = data.find(j => j.name == item.name)
        let p = new Promise(resolve => {
          if (param) {
            obj.visible = param.isVisible

            obj.scale.set(param.scale[0], param.scale[1], param.scale[2])
            TweenMax.to(obj.position, 1.5, {
              x: param.position[0],
              y: param.position[1],
              z: param.position[2],
              repeat: 0
            })
            TweenMax.to(obj.rotation, 1.5, {
              x: param.rotate[0],
              y: param.rotate[1],
              z: param.rotate[2],
              repeat: 0,
              onComplete: () => {
                resolve(obj.position)
              }
            })
          } else {
            obj.visible = false
            resolve(obj.position)
          }
        })
        action.push(p)
      })
      Promise.all([action, this.promiseAddModel]).then(() => {
        this.animate = true
        if (this.timer) {
          this.clearTimer()
        }
        this.getLatestData()
        this.timer = setInterval(() => {
          if (this.fetchData) {
            this.getLatestData()
          }
        }, 60 * 1000)
      })
    },

    // 3-1、获取全部部件颜色
    getCompStatus() {
      // let compList = ['机舱', '风轮', '塔筒']
      if (this.currentComp !== 'windturbine') {
        getCompStatusApi({ entityId: this.windTurbine }).then(res => {
          if (res.data.code === 200) {
            const data = res.data.data.windturbine.chidren || []

            let CompList = data.find(j => j.type == this.currentComp)
              ? data.find(j => j.type == this.currentComp).chidren
              : data

            if (CompList && CompList.length) {
              this.changeListModelColor(CompList)
            }
          }
        })
      }
    },
    // 3-2、改变大部件状态
    changeDigModelColor(name, level) {
      let bigCompName = {
        风轮: [
          '轮毂',
          '叶片一',
          '叶片二',
          '叶片三',
          '变桨轴承一',
          '变桨轴承二',
          '变桨轴承三',
          '叶片前',
          '叶片后'
        ],
        机舱: ['机舱'],
        塔筒: ['塔顶', '塔筒左', '塔筒右', '塔基', '钢塔', '混塔右', '混塔左', '偏航轴承'],
        变桨与偏航: ['变桨轴承一', '变桨轴承二', '变桨轴承三', '偏航轴承']
      }

      let that = this
      let iscolor = that.statusList.find(j => j == level) ? true : false

      // let param = that.findNameModel(name)
      // console.log('获取部件状态', name)

      bigCompName[name].forEach(item => {
        let param = that.findNameModel(item)

        if (param) {
          let obj = param.obj
          let material = param.material

          if (iscolor) {
            obj.traverse(child => {
              if (child.isMesh) {
                child.material.color = new THREE.Color(this.levelColor[level])
                // child.material.color.set(this.setMaterialColor[level])
              }
            })
          } else {
            obj.traverse(child => {
              if (child.isMesh) {
                if (material) {
                  child.material.copy(material)
                }
              }
            })
          }
        }
      })
    },
    //3-2、改变小部件状态
    changeListModelColor(list) {
      //在小部件变色前，把所有大部件变色取消
      /* let bigCompList = [
        {
          name: '风轮',
          level: ''
        },
        {
          name: '机舱',
          level: ''
        },
        {
          name: '塔筒',
          level: ''
        }
      ]
      bigCompList.forEach(item => {
        this.changeDigModelColor(item.name, item.level)
      }) */

      let that = this
      let newList = []
      if (that.currentComp == 'tower') {
        newList = mergeSteelList(list) //对钢索变色数据进行聚合
      } else {
        newList = list
      }

      list &&
        list.forEach(item => {
          let param = that.findNameModel(item.compName)
          let isColor = that.statusList.find(j => j == item.compState) ? true : false

          if (param) {
            let obj = param.obj
            let material = param.material

            if (isColor) {
              obj.traverse(child => {
                if (child.isMesh) {
                  child.material.color = new THREE.Color(this.levelColor[item.compState])
                  // child.material.color.set(that.setMaterialColor[item.datas.entityStatus])
                }
              })
            } else {
              obj.traverse(child => {
                if (child.isMesh) {
                  child.material.copy(material)
                }
              })
            }
          }
        })
    },

    // 4、获取实时特征值数据
    getLatestData() {
      getLatestTurbine({ deviceID: this.windTurbine, pageCompType: this.currentComp }).then(res => {
        if (res.data.code === 200) {
          let result = res.data.data
          // 取部件最近采集时间作为页面右下角的更新时间逻辑-byHGP
          let time = null
          if (this.currentComp !== 'windturbine') {
            this.$refs['compDom'] && this.$refs['compDom'].getEigenValueData(result)
            this.changeListModelColor(result) // 模型添加变色
            let times = result.map(i => {
              return dayjs(i.evSummaryStatusTime).valueOf()
            })
            if (times.length) {
              time = dayjs(Math.max(...times)).format('YYYY-MM-DD HH:mm:ss')
            }
          } else {
            this.handlerTurbineCardList(result.pagecompList)
            time = result.turbineStateTime
          }
          this.$emit('getUpdateValue', time)
        }
      })
    },
    // 4-1、对整机的数据做处理
    handlerTurbineCardList(cardList) {
      let positionArr = []
      cardList.forEach(item => {
        let itemObj = this.turbineCompCardData.find(j => j.pagecompCode == item.pagecompCode)
        if (itemObj) {
          itemObj.measlocList = item.measlocList
        } else {
          const obj = this.getCardPosition(item.compList[0].cardPosition)
          this.turbineCompCardData.push({
            ...item,
            boxLeft: obj.cardLeft,
            boxTop: obj.cardTop
          })
        }
        positionArr.push({
          title: item.pagecompName,
          state: 'unknown',
          spot: item.compList[0].spotPosition,
          cardId: item.pagecompCode,
          spotId: item.pagecompName
        })
      })
      this.allValueList('windturbine', positionArr)
    },

    //修改三维数据为二维
    getCardPosition(card) {
      const halfWidth = this.dom.offsetWidth / 2
      const halfHeight = this.dom.offsetHeight / 2

      let threePosition = new THREE.Vector3(card[0], card[1], card[2])
      let vector = threePosition.clone().project(camera)

      let cardTop = Math.round(-vector.y * halfHeight + halfHeight)
      let cardLeft = Math.round(vector.x * halfWidth + halfWidth)
      return { cardTop, cardLeft }
    },
    //4-2-1、根据状态获取测点和连线颜色
    getModelLineColor(state) {
      return this.levelColor[state] || 'white'
    },
    // 4-1-1、页面展示卡片数据
    allValueList(name, list) {
      this.deleteLine()
      Promise.all([this.planPromise, this.promiseAddModel]).then(reslot => {
        const arr = JSON.parse(JSON.stringify(list))
        // 增加数据变化判断，没有变化则不重新渲染
        if (JSON.stringify(this.allCompValueList) == JSON.stringify(arr)) {
          return
        }
        this.allCompValueList[name] = arr
        this.judgeAddSpotLine(this.currentComp)
      })
    },
    // 4-1、遍历卡片数据，新增测点
    judgeAddSpotLine(name) {
      this.deleteLine()
      this.allCompValueList[name] &&
        this.allCompValueList[name].forEach(val => {
          this.addSpot(val)
        })
      scene.add(allSpot)
    },
    // 4-2、增加测点
    addSpot(val) {
      const { state, spot } = val
      const color = this.getModelLineColor(state)

      //测点
      const spherGeometry = new THREE.SphereGeometry(1, 32, 16)
      const spherMaterial = new THREE.MeshLambertMaterial({ color: new THREE.Color(color) })
      const circle = new THREE.Mesh(spherGeometry, spherMaterial)

      if (this.currentComp == 'NAC' || this.currentComp == 'YPB') {
        circle.scale.set(0.02, 0.02, 0.02)
      } else {
        circle.scale.set(0.1, 0.1, 0.1)
      }

      circle.position.set(spot[0], spot[1], spot[2])

      //测点坐标转化为二维坐标
      this.getSpot2DPosition(circle.position, val, false)

      val.obj = circle //把测点对象增加到数组中

      allSpot.add(circle)
    },
    // 4-3、获取测点的二维坐标
    getSpot2DPosition(pos, val, isRender) {
      let vector = pos.clone().project(camera)

      let halfWidth = this.dom.offsetWidth / 2
      let halfHeight = this.dom.offsetHeight / 2

      val.top = Math.round(-vector.y * halfHeight - 28 + halfHeight)
      val.left = Math.round(vector.x * halfWidth + halfWidth)

      let card = document.getElementById(val.cardId)
      let spot = document.getElementById(val.spotId)
      // console.log('10、添加连线', card, spot)
      if (card && spot) {
        if (!isRender) {
          this.addLine(val.state, val)
        } else {
          if (this.allLine && this.allLine.length) {
            this.allLine.forEach(item => {
              item && item.position()
            })
          }
        }
      }
    },
    // 4-4、根据测点DOM和卡片DOM的id创建连线
    addLine(state, val) {
      let lineDirection = {}
      let card = document.getElementById(val.cardId)
      let spot = document.getElementById(val.spotId)
      if (this.currentComp == 'NAC') {
        card.offsetTop > spot.offsetTop
          ? (lineDirection = {
              startSocket: 'top', //在指引线开始的地方从元素左侧开始
              endSocket: 'bottom'
            })
          : (lineDirection = {
              startSocket: 'bottom', //在指引线开始的地方从元素左侧开始
              endSocket: 'top'
            })
      } else if (this.currentComp == 'TWW') {
        card.offsetLeft > spot.offsetLeft
          ? (lineDirection = {
              startSocket: 'left', //在指引线开始的地方从元素左侧开始
              endSocket: 'right'
            })
          : (lineDirection = {
              startSocket: 'right', //在指引线开始的地方从元素左侧开始
              endSocket: 'left'
            })
      }
      let styleOption = {
        color: this.getModelLineColor(state),
        startPlug: 'disc',
        endPlug: 'disc',
        plugSize: 10,
        endPlugSize: 1,
        size: 2,
        /* startSocket: 'right', //在指引线开始的地方从元素左侧开始
        endSocket: 'end', */
        path: 'grid',
        grid: { step: 10 }
      }
      let showEffectName = 'fade'
      let animOptions = {
        duration: 3, //持续时长
        timing: 'ease-in' // 动画函数
      }

      // console.log('10、添加连线', card, spot)
      if (card && spot) {
        let line = new LeaderLine(card, spot, {
          ...styleOption,
          ...lineDirection
        })
        line.show(showEffectName, animOptions)
        this.allLine.push(line)
      }
    },
    // 4-5、页面每帧都重新获取测点位置
    renderSpot() {
      // console.log('9、runder改变')
      Promise.all(this.promiseAddModel).then(() => {
        this.allCompValueList[this.currentComp] &&
          this.allCompValueList[this.currentComp].forEach(item => {
            this.getSpot2DPosition(item.obj.position, item, true)
          })
      })
    },
    // 4-6、删除所有页面创建连线和测点
    deleteLine() {
      allSpot.children = [] //删除页面添加的测点
      scene.remove(allSpot)

      if (this.allLine.length) {
        this.allLine.forEach(item => {
          item.remove()
        })
      }
      this.allLine = []
    }

    /** x: +,
     *  y: +高,
     *  z: +右
     * */

    //健康状态图片改变
    /*changeHealthLevelImg(level) {
      if (this.statusList.find(j => j == level)) {
        return require(`/public/img/WindTurbine/spotStation/${level}Health.png`)
      } else {
        return require('/public/img/WindTurbine/healthMark.png')
      }
    },*/
    /*/点击按钮
    clickChangeBtn(name, isShow) {
      if (!isShow) {
        return
      }
      let id = ''
      if (name == 'turbine') {
        id = ''
      } else if (name == 'blade') {
        id = this.windBlade
      } else if (name == 'engine') {
        id = this.windEngine
      } else if (name == 'tower') {
        id = this.windTower
      }
      this.$emit('changeCurrentModel', name, id)
    },*/
    /*/ 页面模型切换
    changeModel(name) {
      //部件页面切换，模型全部展示
      deleteBladeModel.traverse(function (child) {
        child.visible = true
      })
      deleteCabinModel.traverse(function (child) {
        child.visible = true
      })
      deleteTowerModel.traverse(function (child) {
        child.visible = true
      })

      if (name == 'turbine') {
        this.reduce()
      } else if (name == 'blade') {
        this.enlargeBlade()
      } else if (name == 'engine') {
        this.enlargeEngine()
      } else if (name == 'tower') {
        this.enlargeTower()
      }
    },*/

    /* getPartData() {
      let objEnum = {
        TWW: 'tower',
        NAC: 'engine',
        ROT: 'blade'
      }
      let butArrayDef = [
        {
          name: 'turbine',
          isShow: true,
          index: 0
        },
        {
          name: 'blade',
          isShow: false,
          index: 1
        },
        {
          name: 'engine',
          isShow: false,
          index: 2
        },
        {
          name: 'tower',
          isShow: false,
          index: 3
        }
      ]
      getEnitiyTree({ windTurbineId: this.$route.query.turbineId })
        .then(result => {
          result.data.data[0] &&
            result.data.data[0].childNode[0].childNode.forEach(item => {
              let obj = butArrayDef.find(o => o.name === objEnum[item.entityType])
              butArrayDef[obj.index].isShow = true
            })
        })
        .finally(() => {
          this.btnNameArr = butArrayDef
        })
    }, */
    /*  handleMenuBtnStyle(item) {
      if (item.isShow) {
        return {
          backgroundImage:
            this.currentComp === item.name
              ? 'url(' + `/img/WindTurbine/background/windTurbineBtnClick.png` + ')'
              : 'url(' + `/img/WindTurbine/background/windTurbineBtn.png` + ')'
        }
      } else {
        return {
          backgroundImage: "url('/img/WindTurbine/background/windTurbineBtnDis.png')",
          cursor: 'not-allowed',
          opacity: 0.2
        }
      }
    } */
  }
}
</script>

<style lang="less" scoped>
* {
  margin: 0;
  padding: 0;
}
#fan_Container {
  // overflow: hidden;
  width: 100%;
  height: 100%;
}
#container {
  width: 100%;
  height: 100%;
  position: absolute;
  top: -28px;
  // overflow: hidden;
}

.turbine_fault_card {
  display: inline-block;
  position: absolute;
  // border: 1px solid red;
}
/* .cabin_card {
  width: 349px;
  height: 328px;
  display: inline-block;
  position: absolute;
  top: 180px;
  left: 500px;
  // border: 1px solid red;
}

.tower_card {
  width: 349px;
  height: 328px;
  display: inline-block;
  position: absolute;
  top: 430px;
  left: 1050px;
  // border: 1px solid red;
} */
.comp_spot {
  // border: 1px solid green;
  height: 1px;
  width: 1px;
  display: inline-block;
  position: absolute;
  transition: top 1ms linear, left 1ms linear;
  // transition: all 100ms linear;
  overflow: hidden;
}

//不同视图的模型
.cabin {
  position: absolute;
  display: inline-block;
}
.blade {
  position: absolute;
  display: inline-block;
}
.tower {
  position: absolute;
  display: inline-block;
}

//按钮样式
.btn_comp {
  position: absolute;
  display: inline-block;
  top: 12%;
  left: 39.5%;
  // height: 47px;
  .btn_change {
    height: 30px;
    width: 80px;
    display: inline-block;
    margin-right: 35px;
    background-size: 100% 100%;
    background-repeat: no-repeat;
    box-sizing: border-box;
    backdrop-filter: blur(7px);
    text-align: center;
    cursor: pointer;
  }
  .btn_change_text {
    color: white;
    font-size: 16px;
    font-weight: bold;
    line-height: 30px;
  }
}
</style>
