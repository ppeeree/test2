<template>
  <div class="canvas_container">
    <!-- 存放three.js的渲染效果 -->
    <div class="container" ref="container"></div>
  </div>
</template>

<script>
import * as THREE from 'three'
import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls'
import { DRACOLoader } from 'three/examples/jsm/loaders/DRACOLoader'
import { GLTFLoader } from 'three/examples/jsm/loaders/GLTFLoader'
import { TransformControls } from 'three/examples/jsm/controls/TransformControls.js'

import { saveMark, getDeviceApi } from '@/api/equipment/monitor'

let scene = new THREE.Scene()
let renderer = new THREE.WebGLRenderer({
  antialias: true,
  alpha: true,
  logarithmicDepthBuffer: true
})

let camera, controls
let loader = new GLTFLoader()
let Model = new THREE.Object3D() // 模型

let transformHelperObjects = []
// let dragObj = []
const point = new THREE.Vector3()
const raycaster = new THREE.Raycaster()
const pointer = new THREE.Vector2()
let transformControl
const ARC_SEGMENTS = 200
let splines = []

export default {
  name: 'model',
  props: {
    showType: {
      type: String,
      default: ''
    }
  },
  watch: {},
  data() {
    return {
      interObj: '', //鼠标选中的模型
      markPosition: [], //增加测点的数组
      pagecompList: [] //所有的聚合部件
    }
  },
  computed: {},
  created() {
    this.getAllPagecompList()
  },
  mounted() {
    this.init()
    this.render()

    window.addEventListener('resize', this.onWindowResize.bind(this))
    this.onWindowResize()

    this.$bus.$on('changeObjPosition', val => {
      this.interObj.position.set(val[0], val[1], val[2])
      this.updateSplineOutline()
    })
  },
  beforeDestroy() {
    transformHelperObjects = []
    scene.children = []
    Model.children = []
  },
  methods: {
    init() {
      this.dom = this.$refs['container']

      this.initCamera()
      this.initLight()
      this.initRender()
      this.initControls()

      scene.add(Model)

      this.dom.addEventListener('pointermove', e => {
        this.onPointerMove(e)
      })
    },
    initCamera() {
      let sizeDom = document.getElementsByClassName('avue-contail')[0]
      camera = new THREE.PerspectiveCamera(
        45,
        sizeDom.offsetWidth / sizeDom.offsetHeight,
        0.01,
        10000
      )
      camera.position.set(25, 25, 25)
    },
    initLight() {
      const axes = new THREE.AxesHelper(50)
      scene.add(axes)

      const directLight = new THREE.DirectionalLight('#ffffff', 0.5)
      const directLight1 = new THREE.DirectionalLight('#ffffff', 0.5)
      const ambientLight = new THREE.AmbientLight('#ffffff', 0.5)

      directLight.position.set(15, 15, 15)
      directLight1.position.set(-15, -15, 15)
      ambientLight.position.set(0, 0, -5)

      scene.add(directLight, directLight1, ambientLight)
    },
    initRender() {
      renderer.setSize(this.dom.clientWidth, this.dom.clientHeight)
      renderer.outputEncoding = THREE.sRGBEncoding
      this.dom.appendChild(renderer.domElement)
    },
    initControls() {
      controls = new OrbitControls(camera, renderer.domElement)

      transformControl = new TransformControls(camera, renderer.domElement)
      /*transformControl.addEventListener('change', () => {
        console.log(1)
        // this.render()
        // controls.enabled = false
      })*/
      transformControl.addEventListener('dragging-changed', function (event) {
        controls.enabled = !event.value
      })

      transformControl.addEventListener('objectChange', param => {
        this.$bus.$emit('mouseObjPosition', this.interObj)
        // console.log('更新连线', scene.children)
        this.updateSplineOutline()
      })
      scene.add(transformControl)
    },
    // 鼠标选中模型
    onPointerMove(event) {
      pointer.x = (event.clientX / window.innerWidth) * 2 - 1
      pointer.y = -(event.clientY / window.innerHeight) * 2 + 1

      raycaster.setFromCamera(pointer, camera)

      const intersects = raycaster.intersectObjects(transformHelperObjects, false)

      if (intersects.length > 0) {
        const object = intersects[0].object
        this.interObj = object

        this.$bus.$emit('mouseObjPosition', object)
        // this.outlineObj([object])
        this.changeObjColor(object)

        if (object !== transformControl.object) {
          transformControl.attach(object)
          controls.enabled = false
        }
      }
    },
    //选中模型变色
    changeObjColor(obj) {
      transformHelperObjects.forEach(item => {
        if (item.name.indexOf('spot') !== -1) {
          if (item.name == obj.name) {
            obj.material.color.set(0xff0f0d)
          } else {
            item.material.color.set(0x95b9f0)
          }
        } else if (item.name.indexOf('card' !== -1)) {
          let textObj = this.markPosition.find(j => item.name.replace('card', '') == j.fCode)
          let aloneCanvas
          if (item.name == obj.name) {
            aloneCanvas = this.creatCanvas(textObj, '#ff0f0d')
          } else {
            aloneCanvas = this.creatCanvas(textObj, '#0077ff')
          }

          item.material = new THREE.MeshFaceMaterial(aloneCanvas)
        }
      })
    },
    // 更新连线
    updateSplineOutline() {
      for (let k = 0; k < splines.length; k++) {
        const spline = splines[k]
        const splineMesh = spline.mesh
        const position = splineMesh.geometry.attributes.position
        for (let i = 0; i < ARC_SEGMENTS; i++) {
          const t = i / (ARC_SEGMENTS - 1)
          spline.getPoint(t, point)
          position.setXYZ(i, point.x, point.y, point.z)
        }
        position.needsUpdate = true
      }
    },
    render() {
      requestAnimationFrame(this.render.bind(this))
      controls.update()
      renderer.render(scene, camera)
    },
    // 适应屏幕变化
    onWindowResize() {
      let sizeDom = document.getElementsByClassName('avue-contail')[0]
      let width = sizeDom.offsetWidth
      let height = sizeDom.offsetHeight
      camera.aspect = width / height
      camera.updateProjectionMatrix()
      renderer.setSize(width, height)
    },

    changeModel(list) {
      // console.log('加载模型地址', list)
      if (list.length) {
        list.forEach(item => {
          let newPath = item.path.replace(new RegExp('amp;'), '')
          loader
            .setDRACOLoader(new DRACOLoader().setDecoderPath('/js/draco/gltf/'))
            .load(newPath, function (gltf) {
              if (item.scale) {
                gltf.scene.scale.set(item.scale[0], item.scale[1], item.scale[2])
              }
              if (item.position) {
                gltf.scene.position.set(item.position[0], item.position[1], item.position[2])
              }
              if (item.rotate) {
                gltf.scene.rotation.set(item.rotate[0], item.rotate[1], item.rotate[2])
              }
              gltf.scene.visible = item.isVisible

              Model.add(gltf.scene)
            })
        })
        let { cameraPosition, wholePosition } = list[0]

        Model.position.set(wholePosition[0], wholePosition[1], wholePosition[2])
        camera.position.set(cameraPosition[0], cameraPosition[1], cameraPosition[2])
      }
      Model.scale.set(0.01, 0.01, 0.01)
    },

    addMarkModel(list) {
      this.markPosition = list
      // console.log('canvas中的添加mark数组', list)
      list.forEach(ele => {
        let position = []

        let obj1 = this.creatSpot(ele, ele.index)
        let obj2 = this.creatHtml(ele, ele.index)

        setTimeout(() => {
          position.push(obj1.position)
          position.push(obj2.position)
          this.creatLine(position)
        }, 2000)
      })
    },
    creatSpot(item, index) {
      let positionArr = item.spot
      const spherGeometry = new THREE.SphereGeometry(1, 32, 16)
      const spherMaterial = new THREE.MeshLambertMaterial({ color: '#95b9f0' })
      const sphere = new THREE.Mesh(spherGeometry, spherMaterial)
      sphere.position.set(positionArr[0], positionArr[1], positionArr[2])

      if (this.showType == '机舱' || this.showType == '变桨与偏航') {
        sphere.scale.set(0.02, 0.02, 0.02)
      } else {
        sphere.scale.set(0.1, 0.1, 0.1)
      }

      transformHelperObjects.push(sphere)
      sphere.name = 'spot' + item.fCode
      scene.add(sphere)
      return sphere
    },
    creatCanvas(item, color) {
      let title = item.name

      const canvas = document.createElement('canvas')
      canvas.width = 256
      canvas.height = 256
      const context = canvas.getContext('2d')
      context.fillStyle = color
      context.fillRect(0, 0, 256, 256)
      context.fillStyle = '#ffff00'

      let titleArr = title.split('&')
      let upHeight = 0
      for (let i = 0; i < titleArr.length; i++) {
        context.font = '20px Airal'

        const fontHeight = (titleArr[i].length + 1) * 20
        if (fontHeight > 240) {
          let string = [titleArr[i].substring(0, 11), titleArr[i].substring(11, titleArr[i].length)]

          string.forEach(ele => {
            context.fillText(ele, 20, 30 + upHeight)
            upHeight += 40
          })
        } else {
          context.fillText(titleArr[i], 20, 40 + upHeight)
          upHeight += 40
        }
      }

      const canvasTexture = new THREE.CanvasTexture(canvas)

      const aloneMesh = new THREE.MeshPhongMaterial({ color: '#0077ff' })
      const canvasMesh = new THREE.MeshPhongMaterial({
        map: canvasTexture,
        transparent: true,
        opacity: 1
      })
      const aloneCanvas = [
        canvasMesh,
        aloneMesh,
        aloneMesh,
        aloneMesh,
        aloneMesh,
        aloneMesh,
        aloneMesh,
        aloneMesh
      ]

      return aloneCanvas
    },
    creatHtml(item, index) {
      let positionArr = item.card

      const aloneCanvas = this.creatCanvas(item, '#0077ff') //卡片材质

      let spherGeometry = new THREE.BoxBufferGeometry(0.01, 15, 15)
      const sphere = new THREE.Mesh(spherGeometry, new THREE.MeshFaceMaterial(aloneCanvas))

      if (this.showType == '塔筒' || this.showType == '机舱') {
        sphere.rotation.set(0, -Math.PI / 2, 0)
      } else if (this.showType == '变桨与偏航') {
        sphere.rotation.set(0, -Math.PI / 2, -Math.PI / 2)
      }
      sphere.position.set(positionArr[0], positionArr[1], positionArr[2])
      if (this.showType == '机舱') {
        sphere.scale.set(0.02, 0.02, 0.02)
      } else if (this.showType == '整机') {
        sphere.scale.set(0.4, 0.4, 0.4)
      } else if (this.showType == '变桨与偏航') {
        // sphere.scale.set(0.1, 0.1, 0.1)
        sphere.scale.set(0.02, 0.02, 0.02)
      } else {
        sphere.scale.set(0.2, 0.2, 0.2)
      }

      transformHelperObjects.push(sphere)
      sphere.name = 'card' + item.fCode
      scene.add(sphere)

      return sphere
    },
    creatLine(position) {
      var curve = new THREE.CatmullRomCurve3(position)
      var points = curve.getPoints(ARC_SEGMENTS)
      var geometry = new THREE.BufferGeometry().setFromPoints(points)
      var material = new THREE.LineBasicMaterial({ color: 0xff0000 })

      curve.mesh = new THREE.LineSegments(geometry, material)
      scene.add(curve.mesh)
      splines.push(curve)
    },

    // 保存
    savePrograme(param) {
      scene.children.forEach(item => {
        if (item.isMesh || item.name !== '') {
          param.measlocPositionList.forEach(ele => {
            let objName = ''
            if (this.showType == '整机' || this.showType == '机舱') {
              objName = ele.measlocCode.join('&')
            } else {
              objName = ele.measlocCode.slice(0, ele.measlocCode.length - 1).join('&')
            }

            if (item.name.indexOf('spot') !== -1) {
              let newObjName = item.name.replace('spot', '')
              if (newObjName == objName) {
                ele.spot = [item.position.x, item.position.y, item.position.z]
              }
            } else if (item.name.indexOf('card') !== -1) {
              let newObjName = item.name.replace('card', '')
              if (newObjName == objName) {
                ele.cardPosition = [item.position.x, item.position.y, item.position.z]
              }
            }
          })
        }
      })
      console.log('点击保存设备', this.showType, param)

      saveMark({ ...param }).then(res => {
        if (res.data.code === 200) {
          this.$message({
            type: 'success',
            message: '测点信息保存成功！'
          })
        }
      })
    },

    // 清空测点数据
    clearModel() {
      Model.children = []
      scene.children.forEach(item => {
        if (item.isMesh || item.name !== '') {
          scene.remove(item)
        }
      })

      transformHelperObjects.forEach(item => {
        transformControl.detach(item)
        scene.remove(item)
      })
      splines.forEach(item => {
        scene.remove(item.mesh)
      })
      transformHelperObjects = []
    },

    // 获取现有的所有聚合部件
    getAllPagecompList() {
      getDeviceApi().then(res => {
        if (res.data.code === 200) {
          this.pagecompList = res.data.data
        }
      })
    }
  }
}
</script>
<style lang="scss" scoped>
.canvas_container {
  width: 100%;
  height: 100%;
}
.container {
  width: 100%;
  height: 100%;
  // overflow: hidden;
  // background: #eee;
  display: inline-block;
  top: -15%;
  left: -4%;
  position: relative;
}
.info {
  display: inline-block;
  // top: 0;
  position: relative;
  top: -100%;
  left: 300px;
  width: 100px;
  height: 100px;
  font-size: 14px;
  background: blueviolet;
}
</style>
