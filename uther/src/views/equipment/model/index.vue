<template>
  <div class="fa_container">
    <div class="left_container">
      <span class="left_title">设备模型树</span>
      <treeCard
        ref="leftTree"
        :list="treeData"
        @handlerTreeClick="handlerTreeClick"
        @initTreeData="initTreeData"
      ></treeCard>
      <el-button type="primary" @click="handlerAdd" class="add_btn"
        ><i class="el-icon-plus"></i> 新增设备</el-button
      >
      <!-- <el-button type="primary" class="drawer_btn" @click="showDrawer = true">设备配置</el-button> -->
    </div>

    <div ref="container" class="center_container"></div>

    <el-drawer
      class="right_container"
      :visible.sync="showDrawer"
      :title="isAdd ? '新增' : '编辑'"
      direction="rtl"
      :modal="false"
      :show-close="true"
      :wrapperClosable="true"
    >
      <el-tabs tab-position="left" style="height: 100%">
        <el-tab-pane label="设备">
          <modifyList ref="listCard" :isAdd="isAdd" @handlerListSave="handlerSave"></modifyList>
        </el-tab-pane>
        <el-tab-pane label="参数">
          <modifyModel
            ref="modelCard"
            :treeData="treeData"
            :selectModelList="selectModelList"
            @addModel="addModel"
            @cameraPosition="setCameraPosition"
            @selectPosition="selectPosition"
            @wholePosition="setWholePosition"
            @handlerModelSave="handlerSave"
            @handlerVisible="handlerVisible"
            @changeSelectModel="changeSelectModel"
          ></modifyModel>
        </el-tab-pane>
      </el-tabs>
    </el-drawer>
  </div>
</template>

<script>
import * as THREE from 'three'
import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls'
import { DRACOLoader } from 'three/examples/jsm/loaders/DRACOLoader'
import { GLTFLoader } from 'three/examples/jsm/loaders/GLTFLoader'
import noData from '@/components/noData/index.vue'

import treeCard from './components/tree.vue'
import modifyList from './components/modifyList.vue'
import modifyModel from './components/modifyModel.vue'

import { save, getList, update } from '@/api/equipment/model'

import { findLeafNode } from '../component/windList'

let scene = new THREE.Scene()
let renderer = new THREE.WebGLRenderer({
  antialias: true,
  alpha: true,
  logarithmicDepthBuffer: true
})
let loader = new GLTFLoader()
let turbine = new THREE.Object3D() // 模型
let camera, controls
let arrNeedOutLine = []

export default {
  components: {
    noData,
    treeCard,
    modifyModel,
    modifyList
  },

  data() {
    return {
      dom: null,
      isAdd: true, //判断是否为新增卡片
      showDrawer: false, //展示右侧dom参数
      treeData: [], //左侧树数据
      selectModelList: [],
      treeClickNode: ''
    }
  },

  created() {},
  mounted() {
    this.init()
    this.render()

    this.$refs['container'].addEventListener('click', this.selectModel.bind(this))
    this.initTreeData()

    window.addEventListener('resize', this.onWindowResize.bind(this))
    this.onWindowResize()
  },
  beforeDestroy() {
    this.clearData()
    scene.children = []
  },
  methods: {
    init() {
      this.dom = this.$refs['container']

      let axes = new THREE.AxesHelper(50)
      scene.add(axes)

      this.initCamera()
      this.initLight()
      this.initRender()

      //控制器
      controls = new OrbitControls(camera, renderer.domElement)
      // controls.minDistance = 0
      // controls.maxDistance = 1000

      scene.add(turbine)
    },
    initCamera() {
      let sizeDom = document.getElementsByClassName('avue-contail')[0]

      camera = new THREE.PerspectiveCamera(
        45,
        sizeDom.offsetWidth / sizeDom.offsetHeight,
        0.001,
        10000
      )
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
    },
    render() {
      requestAnimationFrame(this.render.bind(this))
      controls.update()
      renderer.render(scene, camera)
    },
    //自适应屏幕
    onWindowResize() {
      let sizeDom = document.getElementsByClassName('avue-contail')[0]

      let width = sizeDom.offsetWidth
      let height = sizeDom.offsetHeight
      camera.aspect = width / height
      camera.updateProjectionMatrix()
      renderer.setSize(width, height)
    },

    // 2-1、加载模型
    addModel(list) {
      turbine.children = []
      arrNeedOutLine = []
      let that = this
      list &&
        list.forEach(item => {
          let newPath = item.path.replace(new RegExp('amp;'), '')
          loader
            .setDRACOLoader(new DRACOLoader().setDecoderPath('/js/draco/gltf/'))
            .load(newPath, function (gltf) {
              that.changeObjName(gltf.scene, item.name, newPath)
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
            })
        })

      turbine.scale.set(0.01, 0.01, 0.01)

      /*let data = scene.toJSON()
      let eleLink = document.createElement('a')
      eleLink.download = 'data.json'
      eleLink.style.display = 'none'

      // 这里的data换成你想要导出的JavaScript对象
      let blob = new Blob([JSON.stringify(data, undefined, 4)], { type: 'text/json' })
      eleLink.href = URL.createObjectURL(blob)
      document.body.appendChild(eleLink)
      eleLink.click()

      document.body.removeChild(eleLink)*/
    },
    // 2-2、对加载的模型进行命名，并添加到数组中
    changeObjName(obj, strName, path) {
      let material
      obj.name = strName
      obj &&
        obj.traverse(function (child) {
          if (child.isMesh) {
            child.name = strName
            material = child.material.clone(obj)
          }
        })
      arrNeedOutLine.push({
        name: strName,
        path: path,
        obj: obj,
        isClick: false,
        material: material
      })
    },
    // 2-3、单击选中模型
    selectModel(event) {
      event.preventDefault()
      event.stopPropagation()
      const mouse = new THREE.Vector2()
      mouse.x = (event.offsetX / renderer.domElement.offsetWidth) * 2 - 1
      mouse.y = -(event.offsetY / renderer.domElement.offsetHeight) * 2 + 1
      const raycaster = new THREE.Raycaster()
      raycaster.setFromCamera(mouse, camera)
      let intersects = raycaster.intersectObjects(turbine.children, true)

      if (intersects[0] === undefined) return

      let nameObj = arrNeedOutLine.find(j => j.name == intersects[0].object.name)
      nameObj.isClick = !nameObj.isClick

      this.selectModelList = []
      arrNeedOutLine.forEach(item => {
        if (item.isClick) {
          this.selectModelList.push({ name: item.name, path: item.path })
        }
      })

      this.handlerObjColor(intersects[0].object.name)

      // 参数设置初始值
      if (this.$refs['modelCard']) {
        this.$refs['modelCard'].clearData('isChange')
      }
    },
    // 2-4、对选中模型变色
    handlerObjColor() {
      arrNeedOutLine.forEach(item => {
        if (item.isClick) {
          item.obj.traverse(child => {
            if (child.isMesh) {
              child.material.color.set(0x315ebd) //0x95b9f0 //0x3471f5
            }
          })
        } else {
          item.obj.traverse(child => {
            if (child.isMesh) {
              child.material.copy(item.material)
            }
          })
        }
      })
    },
    // 2-5、对选中模型进行不展示操作
    handlerVisible(arr) {
      arrNeedOutLine.forEach(item => {
        item.obj.visible = true
      })

      arr.forEach(item => {
        let newPath = item.replace(new RegExp('amp;'), '')
        let noneShowObj = arrNeedOutLine.find(j => j.path == newPath)
        if (noneShowObj) {
          noneShowObj.obj.visible = false
        }
      })
    },
    // 2-6、选择器中选中的模型变色
    changeSelectModel(list) {
      arrNeedOutLine.forEach(item => {
        let obj = list.find(
          j => j.replace(new RegExp('amp;'), '') == item.path.replace(new RegExp('amp;'), '')
        )
        if (obj) {
          item.isClick = true
          item.obj.traverse(child => {
            if (child.isMesh) {
              child.material.color.set(0x315ebd) //0x95b9f0 //0x3471f5
            }
          })
        } else {
          item.isClick = false
          item.obj.traverse(child => {
            if (child.isMesh) {
              child.material.copy(item.material)
            }
          })
        }
      })
    },

    // 3-1、实时修改相机位置
    setCameraPosition(position) {
      camera.position.set(position[0], position[1], position[2])
    },
    // 3-2、实时修改选中模型位置
    selectPosition(from) {
      let { position, rotate, scale } = from
      arrNeedOutLine.forEach(item => {
        if (item.isClick) {
          item.obj.position.set(position[0], position[1], position[2])
          item.obj.rotation.set(rotate[0], rotate[1], rotate[2])
          item.obj.scale.set(scale[0], scale[1], scale[2])
        }
      })
    },
    //3-3、实时修改整体模型位置
    setWholePosition(pos) {
      turbine.position.set(pos[0], pos[1], pos[2])
    },

    // 4-1、保存按钮
    handlerSave() {
      let listFrom = this.$refs['listCard'].addFrom
      let basiceList = this.$refs['listCard'].paramList
      let requiredList = this.$refs['listCard'].requiredData
      let entityTypeCode = this.$refs['listCard'].entityTypeCode

      let modelFrom = this.$refs['modelCard'].modelForm

      if (listFrom.maker == '' || listFrom.model == '' || listFrom.type == '') {
        this.$message({
          type: 'warning',
          message: '请完成设备信息！'
        })
      } else {
        let type = Array.isArray(listFrom.type)
          ? listFrom.type[listFrom.type.length - 1]
          : listFrom.type

        let param = {
          manufacturer: listFrom.maker,
          deviceModelType: type,
          deviceModelCode: entityTypeCode,
          embodyId: modelFrom.embodyId,
          deviceModel: listFrom.model,
          propertiesBasic: basiceList,
          defaultpropertiesBasic: requiredList,
          propertiesView: []
        }
        param.propertiesView = this.handlerSaveStep()

        console.log('保存参数', param)

        if (this.isAdd) {
          save(param).then(res => {
            if (res.data.code == 200) {
              this.initTreeData()
              turbine.children = []
              this.$message({
                type: 'success',
                message: '该型号保存成功！'
              })
            }
          })
        } else {
          update({ ...param, id: this.treeClickNode.id }).then(res => {
            if (res.data.code == 200) {
              this.initTreeData()
              turbine.children = []
              this.$message({
                type: 'success',
                message: '该型号更新成功'
              })
            }
          })
        }

        setTimeout(() => {
          this.$refs.leftTree.setHeightLight(listFrom.model)

          //调用方法确定点击的节点
          let node = findLeafNode(listFrom.model, this.treeData, 'label')
          this.handlerTreeClick(node)
        }, 900)
      }
    },
    // 4.2、 保存参数修改
    handlerSaveStep() {
      let modelFrom = this.$refs['modelCard'].modelForm
      let allModelList = this.$refs['modelCard'].allModelNameList
      let { cameraPosition, wholePosition } = modelFrom

      let arr = []

      console.log('保存模型的列表', allModelList, arrNeedOutLine)

      // 仅保存页面展示的模型
      arrNeedOutLine.forEach(item => {
        let modelByAllList = allModelList.find(j => j.value == item.path)
        arr.push({
          name: modelByAllList.key,
          path: item.path,
          scale: [item.obj.scale.x, item.obj.scale.y, item.obj.scale.z],
          position: [item.obj.position.x, item.obj.position.y, item.obj.position.z],
          rotate: [item.obj.rotation.x, item.obj.rotation.y, item.obj.rotation.z],
          isShow: true,
          isVisible: item.obj.visible,
          cameraPosition,
          wholePosition
        })
      })

      // 对所有模型进行保存
      /* allModelList.forEach(item => {
        let showObj = arrNeedOutLine.find(j => j.path == item.value)
        let pos, rot, scale, visible
        if (showObj) {
          pos = showObj.obj.position
          rot = showObj.obj.rotation
          scale = showObj.obj.scale
          visible = showObj.obj.visible
        } else {
          pos = { x: 0, y: 0, z: 0 }
          rot = { x: 0, y: 0, z: 0 }
          scale = { x: 1, y: 1, z: 1 }
        }

        arr.push({
          name: item.key,
          path: item.value,
          scale: [scale.x, scale.y, scale.z],
          position: [pos.x, pos.y, pos.z],
          rotate: [rot.x, rot.y, rot.z],
          isShow: showObj ? true : false,
          isVisible: visible ? visible : false,
          cameraPosition,
          wholePosition
        })
      }) */
      return arr
    },
    // 4.3、新增按钮
    handlerAdd() {
      this.isAdd = true
      this.showDrawer = true
      this.clearData()

      this.$refs.leftTree.setNoneHeightLight() // 取消树选中状态
    },

    // 5.1、获取左侧树数据
    initTreeData() {
      this.isAdd = true
      this.clearData()
      this.treeData = []
      getList('').then(res => {
        if (res.data.code == 200) {
          let data = res.data.data
          this.handlerTreeData(data)
          this.treeData = data
        }
      })
    },
    // 5.2、处理列表数据
    handlerTreeData(arr) {
      arr.forEach(item => {
        item.label = item['compName']
        item.children.forEach(ite => {
          ite.label = ite['manufacturer']
          ite.children.forEach(it => {
            it.label = it['deviceModel']
          })
        })
      })
    },
    // 5.3、点击型号
    handlerTreeClick(data) {
      this.treeClickNode = data
      this.isAdd = false
      this.clearData()
      this.showDrawer = true

      let pathList = []
      data.propertiesView.forEach(item => {
        if (item.isShow) {
          let copyItem = JSON.parse(JSON.stringify(item))
          pathList.push(copyItem)
        }
      })

      setTimeout(() => {
        if (this.$refs['listCard'] && this.$refs['modelCard']) {
          this.$refs['listCard'].initData(data)
          this.$refs['modelCard'].initData(data, pathList)
        }
      }, 700)

      this.addModel(pathList)
    },

    // 清空右侧抽屉数据
    clearData() {
      this.setCameraPosition([10, 10, 10]) //新增初始化相机位置
      this.setWholePosition([0, 0, 0])
      turbine.children = []

      this.$refs['listCard'] && this.$refs['listCard'].clearData()
      this.$refs['modelCard'] && this.$refs['modelCard'].clearData()
    }
  }
}
</script>

<style lang="less" scoped>
.fa_container {
  color: white;
  height: 100%;
  width: 100%;
  overflow: hidden;
  .left_container {
    .left_title {
      font-size: 14px;
      font-weight: 600;
      line-height: 27px;
    }
    float: left;
    position: relative;
    display: inline-block;
    height: 98%;
    width: 220px;
    background: rgba(13, 52, 83, 0.5);
    backdrop-filter: blur(4px);
    z-index: 2;
    margin-top: 10px;
    padding: 10px;
    .drawer_btn {
      position: relative;
      left: 839%;
      top: -97%;
      height: 36px;
      padding: 0px;
      width: 110px;
    }
    .add_btn {
      margin-left: 22%;
    }
  }

  .center_container {
    display: inline-block;
    height: 100%;
    width: 100%;
    top: -114%;
    left: -4%;
    position: relative;
  }
  .right_container {
    float: right;
    z-index: 2;
    position: relative;
    height: 100%;
    width: 500px;
    top: -200%;
    left: 10px;
    background: rgba(4, 17, 33, 0.3);
    backdrop-filter: blur(4px);
    border-radius: 6px;
    // padding-right: 25px;
    ::v-deep .el-drawer {
      width: 100% !important;
      background: transparent;
      box-shadow: none;
      color: white;
      .el-drawer__header {
        color: white;
        text-align: center;
        // background: rgba(4, 17, 33, 0.5);
        margin: 0px;
        height: 55px;
        padding: 0px;
      }
      .el-header {
        height: 30px !important;
      }

      .el-tabs {
        height: 100%;
        .el-tabs__header {
          width: 60px;
          margin: 0px;
          background: rgba(4, 17, 33, 0.5);
          .el-tabs__item {
            color: white;
            padding: 0px;
            text-align: center;
          }
          .el-drawer__close-btn {
            margin-right: 15px;
          }
        }
        .el-tabs__content {
          padding: 10px;
        }
        .el-tabs__nav-wrap::after {
          background-color: transparent;
        }
      }
    }
  }
}
</style>
