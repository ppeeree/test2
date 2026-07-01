import min from 'lodash/min'
import max from 'lodash/max'
// import { getCommonApi } from "../api/commons/commons";
// 使用方法：this.$utils.map.方法名
/* eslint-disable */

const UtilData = {
  // 多端口地址
  DYurl: 'http://192.168.0.56:{18070-18079}',
  // 高度偏差，向上是正数，向下是负数
  altitudeData: 5,
  markList: [], // 初始化地图上的风场标记点及风场名称上
  AreaMap: null, // 初始化高亮地图区域
  AreaMapBorder: null, // 初始化高亮地图边线
  FJDataArr: [],//风机实体
  FLDataArr: [],
  FJLabel: [], // 风机标签+-
  FJDataJson: {},
  eventIcon: [],
  UAVDataArr: [],
  UAVDataJson: [],
  TPDataArr: [],
  UnitDefaultCard: [],
  /**
   * 点云分类
   */
  PCD_COLOR: {
    lasClass: [
      /**
       * original: true,使用真彩色，为false时，使用分类颜色
       */
      { color: ['#999999'], name: '地面点', id: 2, isChecked: false, original: true },
      { color: ['#6DDB00'], name: '低矮植被点', id: 3, isChecked: false, original: true },
      { color: ['#6DDB00'], name: '中等植被点', id: 4, isChecked: false, original: true },
      { color: ['#6DDB00'], name: '高植被点', id: 5, isChecked: false, original: true },
      { color: ['#878787'], name: '建筑物点', id: 6, isChecked: false, original: true },
      { color: ['#d7bebe'], name: '临时建筑物', id: 9, isChecked: false, original: true },
      { color: ['#148bbc'], name: '桥梁', id: 10, isChecked: false, original: true },
      { color: ['#6e90f2'], name: '铁路', id: 11, isChecked: false, original: true },
      { color: ['#424242'], name: '公路', id: 12, isChecked: false, original: true },
      { color: ['#86dcff'], name: '湖', id: [14, 25], isChecked: false, original: true },
      { color: ['#fbc67b'], name: '地线', id: 20, isChecked: true, original: false },
      { color: ['#EFFA13'], name: '导线', id: 16, isChecked: true, original: false },
      { color: ['#ECECEC'], name: '铁塔', id: 17, isChecked: true, original: false },
      // { color: ["#ab73a1"], name: "交叉跨越上", id: 19, isChecked: true, original: false },
      // { color: ["#d9a8d0"], name: "交叉跨越下", id: 20, isChecked: true, original: false },
      // { color: ['#000000'], name: '船&车', id: 22, isChecked: true },
      { color: ['#c0c0c0'], name: '其它线', id: [/*21,*/ 23], isChecked: true, original: false }, // id:21 风机
      { color: ['#fbe35c'], name: '绝缘子', id: 27, isChecked: true, original: false },
      { color: ['#8fdbcd'], name: '引流线', id: 28, isChecked: true, original: false }
    ],

    // 测试
    modelClass: [
      { color: ['#804040'], name: '地面点', id: 2, isChecked: false },
      { color: ['#5AF774'], name: '低矮植被点', id: 3, isChecked: false },
      { color: ['#5AF774'], name: '中等植被点', id: 4, isChecked: false },
      { color: ['#5AF774'], name: '高植被点', id: 5, isChecked: false },
      { color: ['#fdf0b2'], name: '建筑物点', id: 6, isChecked: false },
      { color: ['#0403FB'], name: '低点', id: 7, isChecked: false },
      { color: ['#0403FB'], name: '模型关键点', id: 8, isChecked: false },
      { color: ['#5050FF'], name: '临时建筑物', id: 9, isChecked: false },
      { color: ['#AAAAAA'], name: '桥梁', id: 10, isChecked: false },
      { color: ['#643C64'], name: '铁路', id: 11, isChecked: false },
      { color: ['#3C3C3C'], name: '公路', id: 12, isChecked: false },
      { color: ['#8c0000'], name: '竖向绝缘子', id: 13, isChecked: true }, //竖向绝缘子 酒红色(#8c0000)
      { color: ['#c5a373'], name: '跨越架', id: 15, isChecked: true }, // 跨越架 1.fdf0b2 2.#d8a35d 3.#a47e46 4.#c5a373
      { color: ['#9097A3'], name: '地线', id: 16, isChecked: true },
      { color: ['#9097A3'], name: '导线', id: 17, isChecked: true },
      { color: ['#B8BDC0'], name: '铁塔', id: 18, isChecked: false },
      { color: ['#fdf0b2'], name: '交叉跨越上', id: 19, isChecked: false },
      { color: ['#fdf0b2'], name: '交叉跨越下', id: 20, isChecked: false },
      { color: ['#fdf0b2'], name: '跨越架', id: 21, isChecked: false },
      { color: ['#DDDDDD'], name: 'other line', id: 23, isChecked: false },
      { color: ['#dddbd8'], name: '横向绝缘子', id: 27, isChecked: true }, // 横向绝缘子 瓷白色(#dddbd8)
      { color: ['#FCC5B8'], name: '引流线', id: 28, isChecked: false },
      { color: ['#0503FC'], name: '重叠区域', id: 31, isChecked: false },
      { color: ['#0503FC'], name: '引流线', id: 0, isChecked: false }, // 未分类
      { color: ['#0503FC'], name: '引流线', id: 1, isChecked: false } // 未分类
    ]
  },
  /**
  * 销毁要回调  必须
  */
  unInit() {
    // console.log('unInit')
    window.handler && typeof window.handler.destroy == 'function' && window.handler.destroy() && (window.handler = {})
    if (!window.viewer) return
    // window.viewer.clock.onTick._listeners = []
    /* 1. 销毁所有数据源 & 实体 */
    /*   window.viewer.dataSources.removeAll(true)   // true = 销毁内存
      window.viewer.entities.removeAll() 
    window.viewer.dataSources.remove(this.AreaMap)*/
    /* 2. 销毁所有自定义图层层 */
    /*  window.viewer.imageryLayers.removeAll(true)
  */
    /* 3. 销毁所有 primitive（模型、线、面）*/
    /* window.viewer.scene.primitives.removeAll() */
    /*   this.removeEntitiesByIncludesName('flag', 'windpark')
      this.delFjModel() */
    this.markList = []
    this.AreaMap = null
    this.AreaMapBorder = null

    /* 4. 停掉时钟 & 渲染循环 */
    /*  window.viewer.clock.stop()
     window.viewer.clock.onTick.removeEventListener(viewer._onTick) // 内部循环
    window.viewer.scene.requestRenderMode = true   // 不再 requestRender*/
    /*  viewer.clock.onTick.removeEventListener(() => {
       this.rotateAll()
     }) */
    /* 5. 销毁 viewer 本体（会连带 canvas、worker、定时器）*/
    /*  window.viewer.destroy() */
    window.viewer = null
    window.cesium = null
  },

  /**
   * 初始化配置
   */
  initConfig(Cesium, viewer) {
    window.globalData = {
      tilesArr: [],
      octoberTilesArr: [], // 2021年10月14日点云数据
      shadowArr: [],
      modelArr: [], // 精细化模型数据
      designModelArr: [],
      orthoPhotoObj: []
    }
    // console.log('init')
    window.Cesium = Cesium
    window.viewer = viewer
    //console.log(viewer, 'contextOptions')

    viewer.skyBox = false
    viewer.scene.backgroundColor = new Cesium.Color(0.0, 0.0, 0.0, 0.0)
    // viewer.skyAtmosphere.show = true

    viewer._cesiumWidget._supportsImageRenderingPixelated =
      Cesium.FeatureDetection.supportsImageRenderingPixelated()
    viewer._cesiumWidget._forceResize = true
    if (Cesium.FeatureDetection.supportsImageRenderingPixelated()) {
      var vtxf_dpr = window.devicePixelRatio
      // 适度降低分辨率
      while (vtxf_dpr >= 2.0) {
        vtxf_dpr /= 2.0
      }
      //alert(dpr);
      viewer.resolutionScale = vtxf_dpr
    }

    // if (Cesium.FeatureDetection.supportsImageRenderingPixelated()) {
    //   //判断是否支持图像渲染像素化处理
    //   viewer.resolutionScale = window.devicePixelRatio
    // }
    // viewer.scene.fxaa = true
    // viewer.scene.postProcessStages.fxaa.enabled = true

    viewer.scene.globe.enableLighting = false
    viewer.animation && (viewer.animation.container.style.visibility = 'hidden') // 不显示动画控件
    viewer.clockViewModel.multiplier = 0.34 // 速度
    viewer.scene.globe.depthTestAgainstTerrain = false // 深度检测  （ false: 关闭  true: 打开 ）
    // 调试
    // viewer.scene.globe.depthTestAgainstTerrain = true;  // 地形深度测试
    // viewer.scene.debugShowFramesPerSecond = true;       // 显示帧率
    // 调试==end==
    viewer.cesiumWidget.screenSpaceEventHandler.removeInputAction(
      Cesium.ScreenSpaceEventType.LEFT_DOUBLE_CLICK
    )
    window.handler = new Cesium.ScreenSpaceEventHandler(viewer.scene.canvas)
    // 取消双击定位实体的功能
    viewer.cesiumWidget.screenSpaceEventHandler.removeInputAction(
      Cesium.ScreenSpaceEventType.LEFT_CLICK
    )
    viewer.scene.screenSpaceCameraController.tiltEventTypes = Cesium.CameraEventType.RIGHT_DRAG // 改变倾斜模型，默认中键按住，改为右键按住
    viewer.scene.screenSpaceCameraController.zoomEventTypes = Cesium.CameraEventType.WHEEL // 改变绽放模型，默认滚轮和右键按住拖放，现改为滚轮缩放
    viewer.scene.screenSpaceCameraController.rotateEventTypes = [
      Cesium.CameraEventType.LEFT_DRAG,
      Cesium.CameraEventType.MIDDLE_DRAG
    ] // 平移 添加鼠标左键  鼠标滚轮平移
    viewer.scene.backgroundColor = Cesium.Color.TRANSPARENT //背景色变成近似白色
    viewer.scene.globe.baseColor = Cesium.Color.TRANSPARENT
    viewer.scene.undergroundMode = true

    // 性能优化：关闭永远重绘
    viewer.scene.requestRenderMode = true //性能优化按需渲染
    viewer.scene.maximumRenderTimeChange = 0.03 //0.1 秒内无变化就不画 Infinity //无限
    viewer.targetFrameRate = 30   // 锁 30 FPS（笔记本省电）

    Cesium.RequestScheduler.maximumRequests = 100 // 默认值50  最大请求数
    Cesium.RequestScheduler.maximumRequestsPerServer = 12 //默认值6 单个服务器最大请求数

    // 离线地图服务
    const tifLayer = new Cesium.WebMapServiceImageryProvider({
      url: '/geoserver/jikong/wms', // geoserver服务地址
      layers: 'jikong:jikong', // 工作区名：图层名
      parameters: {
        service: 'WMS',
        format: 'image/png',
        transparent: true
      },
    })
    viewer.imageryLayers.addImageryProvider(tifLayer)
    /*   const layer = viewer.imageryLayers.addImageryProvider(tifLayer)
      layer.alpha = 0.5;
      console.log(layer, 'layer') */

    // 光照角度跟随摄像机视角，并实时刷新光照角度
    viewer.scene.light = new Cesium.DirectionalLight({
      direction: viewer.scene.camera.directionWC
    })
    viewer.scene.preRender.addEventListener(function (scene, time) {
      viewer.scene.light.direction = Cesium.Cartesian3.clone(
        viewer.scene.camera.directionWC,
        viewer.scene.light.direction
      )
    })
    /*  viewer.clock.onTick.addEventListener(() => {
       this.rotateAll()
     }) */
  },
  /**
   * 初始动画
   */
  async initAniMat({ longitude, latitude, elevation, jsonFile }, call) {
    // 地球旋转
    // viewer.clock.multiplier = 100; //速度
    viewer.clock.shouldAnimate = false;
    let previousTime = viewer.clock.currentTime.secondsOfDay
    const onTickCallback = () => {
      let spinRate = 1
      let currentTime = viewer.clock.currentTime.secondsOfDay
      let delta = (currentTime - previousTime) / 1000
      previousTime = currentTime
      viewer.scene.camera.rotate(Cesium.Cartesian3.UNIT_Z, -spinRate * delta)
    }
    viewer.clock.onTick.addEventListener(onTickCallback)

    // this.initMapStatus(null)
    // 上层地图轮廓
    this.AreaMap = await Cesium.GeoJsonDataSource.load(JSON.parse(jsonFile), {
      clampToGround: true,               // 贴地形
      fill: Cesium.Color.fromCssColorString('#113d59').withAlpha(0), // 面填充
      stroke: Cesium.Color.fromCssColorString('#02e5ed'),   // 默认边线
      strokeWidth: 3
    })
    viewer.dataSources.add(this.AreaMap);
    // ③ 用 Primitive 画超粗高亮边（可选）
    /*    const coordinates = JSON.parse(jsonFile).features[0].geometry.coordinates[0]
       const flat = coordinates.flat(2)
       this.AreaMapBorder = viewer.scene.primitives.add(
         new Cesium.Primitive({
           geometryInstances: new Cesium.GeometryInstance({
             geometry: new Cesium.PolylineGeometry({
               positions: Cesium.Cartesian3.fromDegreesArray(flat),
               width: 4  // 更粗
             })
           }),
           appearance: new Cesium.PolylineMaterialAppearance({
             material: Cesium.Material.fromType('Color', {
               color: Cesium.Color.fromCssColorString('#02e5ed') // 金色
             })
           })
         })
       ) */
    // 3. 用entities 画粗高亮边（可选）
    let entities = this.AreaMap.entities.values;
    entities.forEach((entity) => {
      if (!entity.polygon) return

      // 取外环坐标 
      const positions = entity.polygon.hierarchy.getValue().positions // Cartesian3 数组

      //新建一条独立粗线
      this.AreaMapBorder = viewer.entities.add({
        name: entity.name + '_glow',
        polyline: {
          positions,
          width: 4,                                    // 任意像素宽
          material: Cesium.Color.fromCssColorString('#02e5ed'),
          clampToGround: true,                         // 贴地形
          depthFailMaterial: Cesium.Color.YELLOW      // 地形遮挡部分也亮
        },
        // 自定义属性，方便后面一次性找到
        isGlowLine: true
      })
    });

    // 4. 飞到云南
    viewer.camera.flyTo({
      destination: Cesium.Cartesian3.fromDegrees(
        longitude,
        latitude,
        elevation + 150000,
      ),
      orientation: {
        heading: 0,
        pitch: -Cesium.Math.PI_OVER_TWO * 0.97, // 稍微俯视
        roll: 0
      }
    });
    // setTimeout(() => {
    viewer.clock.onTick.removeEventListener(onTickCallback)
    viewer.clock.shouldAnimate = false;
    // this.initMapStatus(null)

    call && call()
    // }, 2500)

  },
  /**
   * 地图初始化的状态
   */
  initMapStatus(callback = null) {
    let localUserInfo = JSON.parse(localStorage.getItem('saber-userInfo'))?.content
    let windFarmInfo = JSON.parse(sessionStorage.getItem('selectWindFarm')) || {}
    if (windFarmInfo?.latitude && windFarmInfo?.longitude) {
      localUserInfo = windFarmInfo
    }
    const param = {
      destination: Cesium.Cartesian3.fromDegrees(103, 22, 100000),// Cesium.Cartesian3.fromDegrees(+x, +y, +z),
      heading: 3.7759544975745456,
      pitch: -0.6745077231259704,
      roll: 6.283162571056469
    }
    /**
     * 定位到相应视角
     */
    viewer.camera.flyTo({
      complete: callback,
      destination: param.destination,
      orientation: {
        heading: param.heading,
        pitch: param.pitch,
        roll: param.roll
      }
    })
  },
  // 回到初始化状态
  resetMapStatus({ longitude, latitude, elevation }) {
    // 飞回初始视角
    viewer.camera.flyTo({
      destination: Cesium.Cartesian3.fromDegrees(
        longitude,     // 经度（云南中心）
        latitude,      // 纬度（云南中心）
        elevation + 150000, // 高度，单位：米 → 1.5 M 米视野最大
      ),
      orientation: {
        heading: 0,
        pitch: -Cesium.Math.PI_OVER_TWO * 0.97, // 稍微俯视
        roll: 6.283162571056469
      }
    });
    // 显示背景色和勾线
    this.AreaMap ? this.AreaMap.show = true : null
    this.AreaMapBorder ? this.AreaMapBorder.show = true : null
    // this.AreaMapBorder.forEach(e => e.show = true)
    this.markList.forEach(e => e.show = true)
  },
  // 隐藏背景色和勾线和风场图标
  hideMapStatus() {
    if (this.markList.length) {
      this.AreaMap.show = false
      //this.AreaMapBorder.forEach(e => e.show = false)
      this.AreaMapBorder.show = false
      console.log(this.AreaMapBorder.show)
      this.markList.forEach(e => e.show = false)
    }
  },
  Cartesian3_flyTo(cartesian3, time = 2, callback = null, options) {
    viewer.camera.flyTo({
      duration: time,
      complete: callback instanceof Function && callback(),
      destination: cartesian3,
      orientation: { ...options }
    })
  },

  /**
   * @description 将笛卡尔坐标系转成经纬度高程
   * @param {Object} cartesianObj 笛卡尔坐标系对象 {x, y, z}
   * @returns 返回经纬度高程对象
   */
  cartesianTolngLatAlt(cartesianObj) {
    if (!Cesium || !window.Cesium) {
      throw new Error('非cesium地图')
    }
    if (!cartesianObj || Object.keys(cartesianObj).length !== 3) {
      throw new Error('请传入合法的cartesian对象 {x, y, z}')
    }
    const cartesian3 = new Cesium.Cartesian3(cartesianObj.x, cartesianObj.y, cartesianObj.z)
    const cartographic = Cesium.Ellipsoid.WGS84.cartesianToCartographic(cartesian3)
    const lat = Cesium.Math.toDegrees(cartographic.latitude)
    const lng = Cesium.Math.toDegrees(cartographic.longitude)
    const alt = cartographic.height
    return { lng, lat, alt }
  },

  //三维笛卡尔坐标转屏幕坐标
  transPosition(position) {
    return Cesium.SceneTransforms.wgs84ToWindowCoordinates(viewer.scene, position)
  },
  /**
   * 获取视角
   */
  getCamera(flag) {
    if (viewer && viewer.camera && viewer.camera.position && viewer.camera.heading) {
      let p = this.Cartesian3_to_WGS84(viewer.camera.position)
      let heading = Cesium.Math.toDegrees(viewer.camera.heading)
      let pitch = Cesium.Math.toDegrees(viewer.camera.pitch)
      let roll = Cesium.Math.toDegrees(viewer.camera.roll)
      if (flag) {
        console.log({
          lng: p.lng,
          lat: p.lat,
          height: p.alt,
          heading: heading,
          pitch: pitch,
          roll: roll
        })
      }
      return {
        lng: p.lng,
        lat: p.lat,
        height: p.alt,
        heading: heading,
        pitch: pitch,
        roll: roll
      }
    }
  },
  /**
   * 点击获取实体属性
   */
  getPick(e, callback, error) {
    let pick = viewer.scene.pick(e.position || e.endPosition)
    pick && callback && callback(pick)
    !pick && error && error()

    // 点击打印经纬度
    const m_position = viewer.scene.pickPosition(e.position)
    let cartographic = viewer.scene.globe.ellipsoid.cartesianToCartographic(m_position)
    let tLongitude = Cesium.Math.toDegrees(cartographic.longitude)
    let tLatitude = Cesium.Math.toDegrees(cartographic.latitude)
    let tElev = cartographic.height
    // 点击打印经纬度高程
    console.log({
      lon: tLongitude,
      lat: tLatitude,
      height: tElev
    })
  },
  /**
   * 添加贴地面
   */
  addPolygonFunc(name, data) {
    // 添加贴地面
    const tempDatas = data || [
      [120.29012233841628, 32.90054693999219],
      [120.7241809458786, 32.92418139242192],
      [120.66527044480591, 32.634708653529074],
      [120.31111598565721, 32.6044859723414],
      [120.29012233841628, 32.90054693999219]
    ]
    const lonLatAnalyse = data => {
      const arr = []
      data.forEach(o => arr.push(o[0], o[1]))
      return arr
    }
    // 添加面
    const addPolygon = (name, positions, rgba) => {
      const features = turf.points([positions[0], positions[2]])
      const center = turf.center(features)
      let res = viewer.entities.values.find(item => item.id === name)
      !res &&
        viewer.entities.add({
          id: name,
          show: false,
          polygon: {
            clampToGround: true, //开启贴地
            hierarchy: new Cesium.PolygonHierarchy(
              Cesium.Cartesian3.fromDegreesArray(lonLatAnalyse(positions))
            ),
            material: Cesium.Color.BLUE.withAlpha(0.2),
            distanceDisplayCondition: new Cesium.DistanceDisplayCondition(0, 300000)
          },
          position: Cesium.Cartesian3.fromDegrees(
            center.geometry.coordinates[0],
            center.geometry.coordinates[1],
            0
          ),
          label: {
            text: name,
            font: 'normal 32px MicroSoft YaHei',
            fillColor: Cesium.Color.WHITE,
            style: Cesium.LabelStyle.FILL_AND_OUTLINE,
            verticalOrigin: Cesium.VerticalOrigin.BOTTOM,
            distanceDisplayCondition: new Cesium.DistanceDisplayCondition(0, 300000),
            pixelOffset: new Cesium.Cartesian2(0, 0)
            // disableDepthTestDistance: Number.POSITIVE_INFINITY, // 永远禁用深度测试
          }
        })
    }
    addPolygon(name || 'demo', tempDatas)
  },
  // 添加面
  addPolygon(positions, name, color) {
    const lonLatAnalyse = data => {
      const arr = []
      data.forEach(o => arr.push(o[0], o[1]))
      return arr
    }
    let color1 = null
    if (color == '1') {
      color1 = Cesium.Color.GREEN.withAlpha(0.7)
    } else {
      color1 = Cesium.Color.GRAY.withAlpha(0.7)
    }
    viewer.entities.add({
      tempName: name || '默认值',
      show: true,
      polygon: {
        clampToGround: true, //开启贴地
        hierarchy: new Cesium.PolygonHierarchy(
          Cesium.Cartesian3.fromDegreesArray(lonLatAnalyse(positions))
        ),
        material: color1
        // distanceDisplayCondition: new Cesium.DistanceDisplayCondition(0, 300000)
      }
    })
  },
  /**
   * 修改海拔高度
   */
  altitudeEdit(obj, height) {
    const boundingSphere = obj.boundingSphere // 计算obj的绑定范围

    if (!boundingSphere) {
      return
    }
    const cartographic = Cesium.Cartographic.fromCartesian(boundingSphere.center) // 计算中心点位置
    const surface = Cesium.Cartesian3.fromRadians(
      cartographic.longitude,
      cartographic.latitude,
      0.0
    ) // 计算中心点位置的地表坐标
    const offset = Cesium.Cartesian3.fromRadians(
      cartographic.longitude,
      cartographic.latitude,
      height || this.altitudeData
    ) // 偏移后的坐标
    const translation = Cesium.Cartesian3.subtract(offset, surface, new Cesium.Cartesian3())
    obj.modelMatrix = Cesium.Matrix4.fromTranslation(translation) // obj.modelMatrix转换
  },
  /**
   * 给3DTiles数据着色
   */
  cesium3DTilesSetColor(o, judge = true, linkPointCloud = null, showModel = false) {
    // 参数说明
    // o:加载点云杆塔数据之后的返回对象（o.hasClass：有无真彩色；o.tempPointSize：点云点的粗细）
    // judge:控制3DTiles显示隐藏
    // linkPointCloud: 需要显示或者隐藏的 分类信息
    if (showModel == false) {
      this.PCD_COLOR.Classification = this.PCD_COLOR.lasClass
    } else if (showModel == true) {
      this.PCD_COLOR.Classification = this.PCD_COLOR.modelClass
    }
    // 分类联动
    if (linkPointCloud) {
      const { isChecked: cloud_Checked, id: cloud_id } = linkPointCloud
      this.PCD_COLOR.Classification.forEach(item => {
        const { id } = item
        if (typeof cloud_id === 'object' && typeof id === 'object') {
          const tempJudge =
            cloud_id.length === id.length &&
            cloud_id.every(a => id.some(b => a === b)) &&
            id.every(_b => cloud_id.some(_a => _a === _b))
          tempJudge && (item.isChecked = cloud_Checked)
        } else {
          cloud_id === id && (item.isChecked = cloud_Checked)
        }
      })
    }

    // 通过分类联动点云
    let tempShow = []
    let showDataArr = []
    this.PCD_COLOR.Classification.forEach(({ id, isChecked }) => {
      if (typeof id === 'object') {
        isChecked && id.forEach(obj => showDataArr.push(obj))
      } else {
        isChecked && showDataArr.push(id)
      }
    })
    showDataArr.length != 0 &&
      showDataArr.forEach(id => tempShow.push(['${Classification}==='] + id))

    // 判断： 当judge为false的时候直接隐藏点云，当judge为true的时候按照分类控制点云的显示与隐藏。
    let tempJudge = !judge ? judge : tempShow.length != 0 && tempShow.join('||')

    // 按照分类给点云着色
    const conditions = []
    const fun = (arr, type) => {
      arr.forEach(({ color, id, original }) => {
        if (!original) {
          if (typeof id === 'object') {
            id.forEach(e => {
              conditions.push(['${' + type + '}===' + e + '', "color('" + color + "')"])
            })
          } else {
            conditions.push(['${' + type + '}===' + id + '', "color('" + color + "')"])
          }
        }
      })
    }
    fun(this.PCD_COLOR.Classification, 'Classification')
    conditions.push(['true', '${COLOR}'])

    // o.hasClass = true   // 开启真彩色
    // // 没有真彩色
    !o.hasClass &&
      (o.style = new Cesium.Cesium3DTileStyle({
        color: { conditions },
        show: tempJudge,
        pointSize: o.tempPointSize || 2
      }))

    // 有真彩色
    o.hasClass &&
      (o.style = new Cesium.Cesium3DTileStyle({
        show: judge,
        pointSize: o.tempPointSize || 2
      }))
  },
  /**
   * 加载切片数据（tileset）
   */
  add3DTiles({ url }) {
    function func(url) {
      var reg = /{([\d]*)-([\d]*)}/
      if (url && reg.test(url)) {
        let scope = reg.exec(url)
        let min = parseInt(scope[1])
        let max = parseInt(scope[2])
        let index = parseInt(Math.random() * (max - min + 1) + min)
        arguments[1] = url.replace(reg, index)
      }
      return arguments[1]
    }

    let tileset = new Cesium.Cesium3DTileset({
      url: func(UtilData.DYurl + url)
    })
    viewer.scene.primitives.add(tileset)
    tileset.pointCloudShading.eyeDomeLightingStrength = 10
    tileset.pointCloudShading.eyeDomeLightingRadius = 10
    return tileset
  },
  /**
   * 加载设计（tileset）
   */
  addDesignModel(options) {
    viewer.entities.add(options)
  },
  /**
   * 加载 点 和 文字描述
   */
  addPointAndLabel({ name, color, lonAndLat, flag }) {
    return viewer.entities.add({
      nameFlag: name,
      flag,
      position: Cesium.Cartesian3.fromDegrees(lonAndLat[0], lonAndLat[1], lonAndLat[2] || 0),
      point: {
        show: true,
        // distanceDisplayCondition: new Cesium.DistanceDisplayCondition(0, 6000),
        color: Cesium.Color.fromCssColorString(color || 'green'),
        outlineColor: Cesium.Color.WHITE,
        pixelSize: 11,
        disableDepthTestDistance: lonAndLat[2] === null ? Number.POSITIVE_INFINITY : ''
      },
      label: {
        text: name,
        font: 'normal 22px MicroSoft YaHei',
        fillColor: Cesium.Color.WHITE,
        style: Cesium.LabelStyle.FILL_AND_OUTLINE,
        outlineWidth: 1,
        verticalOrigin: Cesium.VerticalOrigin.BOTTOM,
        pixelOffset: new Cesium.Cartesian2(0, -30),
        showBackground: !0,
        backgroundColor: new Cesium.Color(0, 0, 0, 0.7),
        // disableDepthTestDistance: lonAndLat[2] === null ? Number.POSITIVE_INFINITY : '',
        disableDepthTestDistance: Number.POSITIVE_INFINITY,
        distanceDisplayCondition: new Cesium.DistanceDisplayCondition(
          0 /* 近地面距离 */,
          2000 /* 远地面距离 */
        )
      }
    })
  },
  /**
   * 获取当前三维范围
   */
  getCurrentExtent() {
    // 范围对象
    const extent = {}
    // 得到当前三维场景
    const scene = viewer.scene
    // 得到当前三维场景的椭球体
    const ellipsoid = scene.globe.ellipsoid
    const canvas = scene.canvas

    // canvas左上角
    const car3_lt = viewer.camera.pickEllipsoid(new Cesium.Cartesian2(0, 0), ellipsoid)
    // canvas左下角
    const car3_lt_bottom = viewer.camera.pickEllipsoid(
      new Cesium.Cartesian2(0, canvas.height),
      ellipsoid
    )
    // canvas右下角
    const car3_rb = viewer.camera.pickEllipsoid(
      new Cesium.Cartesian2(canvas.width, canvas.height),
      ellipsoid
    )
    // canvas右上角
    const car3_rb_top = viewer.camera.pickEllipsoid(
      new Cesium.Cartesian2(canvas.width, 0),
      ellipsoid
    )

    // 当canvas左上角和右下角全部在椭球体上
    if (car3_lt && car3_lt_bottom && car3_rb && car3_rb_top) {
      const carto_lt = ellipsoid.cartesianToCartographic(car3_lt)
      const carto_lt_bottom = ellipsoid.cartesianToCartographic(car3_lt_bottom)
      const carto_rb = ellipsoid.cartesianToCartographic(car3_rb)
      const carto_rb_top = ellipsoid.cartesianToCartographic(car3_rb_top)

      extent.lt_lon = Cesium.Math.toDegrees(carto_lt.longitude) // 左上角 经度
      extent.lt_lat = Cesium.Math.toDegrees(carto_lt.latitude) // 左上角 纬度

      extent.lb_lon = Cesium.Math.toDegrees(carto_lt_bottom.longitude) // 左下角 经度
      extent.lb_lat = Cesium.Math.toDegrees(carto_lt_bottom.latitude) // 左下角 纬度

      extent.rb_lon = Cesium.Math.toDegrees(carto_rb.longitude) // 右下角 经度
      extent.rb_lat = Cesium.Math.toDegrees(carto_rb.latitude) // 右下角 纬度

      extent.rt_lon = Cesium.Math.toDegrees(carto_rb_top.longitude) // 右上角 经度
      extent.rt_lat = Cesium.Math.toDegrees(carto_rb_top.latitude) // 右上角 纬度
    } else {
      return {}
    }

    // 获取高度
    extent.height = Math.ceil(viewer.camera.positionCartographic.height)

    // 返回 当前三维范围 数据
    return extent
  },
  /*
   * 鼠标滚轮事件
   */
  mouseWheelFun(call) {
    // const scene = viewer.scene
    // const handler = new Cesium.ScreenSpaceEventHandler(scene.canvas)
    window.handler.setInputAction(() => {
      const { height = null } = this.getCurrentExtent()
      call && call(height)
    }, Cesium.ScreenSpaceEventType.WHEEL)
  },
  /**
   * 定位
   */
  flyTo(lonAndLat, time = 2, callback = null) {
    if (!lonAndLat[0] || !lonAndLat[1]) {
      return false
    }
    viewer.camera.flyTo({
      duration: time,
      complete: callback,
      destination: Cesium.Cartesian3.fromDegrees(lonAndLat[0], lonAndLat[1], lonAndLat[2] || 10000)
    })
  },
  flyToAngle(lonAndLat, heading, pitch, roll) {
    viewer.camera.flyTo({
      destination: Cesium.Cartesian3.fromDegrees(lonAndLat[0], lonAndLat[1], lonAndLat[2]),
      orientation: {
        heading: Cesium.Math.toRadians(heading),
        pitch: Cesium.Math.toRadians(pitch),
        roll: Cesium.Math.toRadians(roll)
      }
    })
  },
  /**
   * 根据txt删除三维entities
   */
  removeEntitiesByName(attr, nameText) {
    console.log(111)
    const func = name => {
      viewer.entities.values.forEach(item => {
        if (item[attr] && item[attr] === name) {
          viewer.entities.remove(item)
          func(name)
        }
      })
    }
    func(nameText)
  },
  /**
   * 根据txt模糊查询删除三维entities
   */
  removeEntitiesByIncludesName(attr, nameText) {
    const func = name => {
      viewer.entities.values.forEach(item => {
        if (item[attr] && item[attr].split(' - ')[0].includes(name)) {
          viewer.entities.remove(item)
          func(name)
        }
      })
    }
    func(nameText)
  },
  /**
   * 根据name隐藏或展示三维entities
   */
  hideAndShowEntitiesByName(attr, nameText, flag = true) {
    const func = name => {
      viewer.entities.values.forEach(item => {
        if (item[attr] && item[attr] === name) {
          item.show = flag
        }
      })
    }
    func(nameText)
  },
  /**
   * 加载模型
   */
  update3dtilesMaxtrix(tileModel, lon, lat, scale, heading, h = 0, height) {
    let tileModelTool = {
      scale: scale,
      longitude: lon,
      latitude: lat,
      height: height ? height : 3.5,
      rx: 0, // X轴（经度）方向旋转角度（单位：度）
      ry: 0, // Y轴（纬度）方向旋转角度（单位：度）
      rz: heading, // Z轴（高程）方向旋转角度（单位：度）
      alpha: 1
    }
    var mx = Cesium.Matrix3.fromRotationX(Cesium.Math.toRadians(tileModelTool.rx))
    var my = Cesium.Matrix3.fromRotationY(Cesium.Math.toRadians(tileModelTool.ry))
    var mz = Cesium.Matrix3.fromRotationZ(Cesium.Math.toRadians(tileModelTool.rz))
    var rotationX = Cesium.Matrix4.fromRotationTranslation(mx)
    var rotationY = Cesium.Matrix4.fromRotationTranslation(my)
    var rotationZ = Cesium.Matrix4.fromRotationTranslation(mz)
    //平移 修改经纬度
    var position = Cesium.Cartesian3.fromDegrees(
      tileModelTool.longitude,
      tileModelTool.latitude,
      tileModelTool.height - h
    )
    var m = Cesium.Transforms.eastNorthUpToFixedFrame(position)
    //旋转、平移矩阵相乘
    Cesium.Matrix4.multiply(m, rotationX, m)
    Cesium.Matrix4.multiply(m, rotationY, m)
    Cesium.Matrix4.multiply(m, rotationZ, m)
    //缩放 修改缩放比例
    var scale = Cesium.Matrix4.fromUniformScale(tileModelTool.scale)
    Cesium.Matrix4.multiply(m, scale, m)
    //赋值给tileset
    tileModel._root.transform = m
    //调整地下透明度
    viewer.scene.globe.translucency.frontFaceAlphaByDistance.nearValue = Cesium.Math.clamp(
      tileModelTool.alpha,
      0.0,
      1.0
    )
  },
  /**
   *
   * @param {*} 视角移动
   */
  mapMove(cameraData) {
    const k = cameraData || this.getCamera()
    const fun = (k, call = null) => {
      viewer.camera.flyTo({
        duration: !!cameraData ? 2 : 0.1,
        complete: call,
        destination: Cesium.Cartesian3.fromDegrees(k.lng, k.lat, k.height),
        orientation: {
          heading: Cesium.Math.toRadians(k.heading),
          pitch: Cesium.Math.toRadians(k.pitch),
          roll: Cesium.Math.toRadians(k.roll)
        }
      })
    }
    k.height += 0.001
    fun(k)
  },
  // DOM节点随着地图的移动而实时移动
  domMove(o) {
    if (!o[0].element) {
      return
    }
    const scratch = new Cesium.Cartesian2()
    const scene = viewer.scene
    scene.preRender.addEventListener(function () {
      for (let i = 0; i < o.length; i++) {
        const p = Cesium.Cartesian3.fromDegrees(
          o[i].position[0],
          o[i].position[1],
          o[i].position[2] || 0
        )
        const canvasPosition = scene.cartesianToCanvasCoordinates(p, scratch)
        if (Cesium.defined(canvasPosition)) {
          o[i].element.$el.style.left =
            parseFloat(canvasPosition.x) + parseFloat(o[i].offset[0]) + 'px'
          o[i].element.$el.style.top =
            parseFloat(canvasPosition.y) + parseFloat(o[i].offset[1]) + 'px'
        }
      }
    })
  },
  // 添加线（height）
  addPolylineHeight(data) {
    let { positions, color, width, flag } = data
    return viewer.entities.add({
      flag,
      polyline: {
        positions: Cesium.Cartesian3.fromDegreesArrayHeights(positions),
        width: width || 2,
        material: Cesium.Color.fromCssColorString(color || 'blue')
      }
    })
  },
  // 添加图片实体
  addImg(lonAndLat, imgSize, img, id, offset, flag = false) {
    return viewer.entities.add({
      id,
      position: Cesium.Cartesian3.fromDegrees(lonAndLat[0], lonAndLat[1], lonAndLat[2] || 0),
      billboard: {
        image: img,
        width: imgSize[0],
        height: imgSize[1],
        pixelOffset: new Cesium.Cartesian2(0, offset || 0),
        verticalOrigin: Cesium.VerticalOrigin.BOTTOM,
        disableDepthTestDistance: flag ? Number.POSITIVE_INFINITY : ''
      }
    })
  },
  // 添加图片、文字实体
  addImgAndLabel(data) {
    let {
      baseInfo,
      positions,
      backgroundColor,
      name,
      img,
      imgSize,
      offset,
      flag,
      isDistanceDisplayCondition = false,
      disableDepthTestDistance = true
    } = data
    if (!window.viewer || window.viewer.isDestroyed()) return
    this.markList.push(window.viewer.entities.add({
      ...baseInfo,
      flag,
      tempData: positions,
      position: Cesium.Cartesian3.fromDegrees(positions[0], positions[1], positions[2] || 0),
      billboard: {
        image: img,
        width: imgSize[0],
        height: imgSize[1],
        pixelOffset: new Cesium.Cartesian2(0, offset || 0),
        verticalOrigin: Cesium.VerticalOrigin.BOTTOM,
        // 关键1：整个矩形都参与拾取（含透明像素）
        heightReference: Cesium.HeightReference.RELATIVE_TO_GROUND,
        disableDepthTestDistance: disableDepthTestDistance ? Number.POSITIVE_INFINITY : '', //永远禁用深度测试
        /*    distanceDisplayCondition:
             isDistanceDisplayCondition && new Cesium.DistanceDisplayCondition(0, 10000) */
      },
      label: {
        text: name,
        font: 'bold 12px "Microsoft YaHei", sans-serif',
        fillColor: Cesium.Color.WHITE,
        style: Cesium.LabelStyle.FILL_AND_OUTLINE,
        outlineWidth: 1,
        //verticalOrigin: Cesium.VerticalOrigin.TOP, // 文字在图标上方
        // pixelOffset: new Cesium.Cartesian2(0, -36), // 不挡图标
        disableDepthTestDistance: Number.POSITIVE_INFINITY,
        verticalOrigin: Cesium.VerticalOrigin.BOTTOM,
        pixelOffset: new Cesium.Cartesian2(0, 35),
        //  showBackground: false,
        backgroundColor: backgroundColor || new Cesium.Color(0, 0, 0, 0.7),
        /*  distanceDisplayCondition:
           isDistanceDisplayCondition && new Cesium.DistanceDisplayCondition(0, 1000000) */
      }
    }))
  },
  zoomTo3DTiles(palaceTileset) {
    palaceTileset.readyPromise.then(palaceTileset => {
      viewer.zoomTo(palaceTileset)
    })
  },
  // 隐藏点云和倾斜数据
  destoryPointCloudAndQingXie() {
    globalData.tilesArr.forEach(item => (item.show = false))
    globalData.shadowArr.forEach(item => (item.show = false))
    globalData.modelArr.forEach(item => (item.show = false))
    globalData.tilesArr = []
    globalData.shadowArr = []
    globalData.modelArr = []
  },
  destoryDesignMode() {
    let idArr = []
    globalData.designModelArr.forEach(item => {
      idArr.push(item.id)
    })
    idArr.forEach(item => {
      viewer.entities.remove(viewer.entities.getById(item))
    })
    globalData.designModelArr = []
    console.log(viewer.entities.values)
  },
  destoryOrthoPhoto() {
    globalData.orthoPhotoObj.forEach(item => {
      viewer.imageryLayers.remove(item)
    })
  },
  // 点云、倾斜、实体 抬高方法封装
  editHeight(tileset, val) {
    if (tileset.boundingSphere) {
      //高度偏差，向上是正数，向下是负数
      var heightOffset = 0
      //计算tileset的绑定范围
      var boundingSphere = tileset.boundingSphere
      //计算中心点位置
      var cartographic = Cesium.Cartographic.fromCartesian(boundingSphere.center)
      //计算中心点位置的地表坐标
      var surface = Cesium.Cartesian3.fromRadians(
        cartographic.longitude,
        cartographic.latitude,
        0.0
      )
      //偏移后的坐标
      var offset = Cesium.Cartesian3.fromRadians(
        cartographic.longitude,
        cartographic.latitude,
        heightOffset + val
      )
      var translation = Cesium.Cartesian3.subtract(offset, surface, new Cesium.Cartesian3())
      //tileset.modelMatrix转换
      tileset.modelMatrix = Cesium.Matrix4.fromTranslation(translation)
    }
  },
  getMapUUID() {
    var s = []
    var hexDigits = '0123456789abcdef'
    for (var i = 0; i < 36; i++) {
      s[i] = hexDigits.substr(Math.floor(Math.random() * 0x10), 1)
    }
    s[14] = '4'
    s[19] = hexDigits.substr((s[19] & 0x3) | 0x8, 1)
    s[8] = s[13] = s[18] = s[23] = '-'

    var uuid = s.join('')
    return uuid
  },
  // 添加文字实体
  addLabel(lonAndLat, id, name) {
    return viewer.entities.add({
      id,
      name: 'label',
      position: Cesium.Cartesian3.fromDegrees(lonAndLat[0], lonAndLat[1], lonAndLat[2] || 0),
      label: {
        text: name,
        show: true,
        font: '18px',
        fillColor: Cesium.Color.WHITE,
        style: Cesium.LabelStyle.FILL_AND_OUTLINE,
        outlineWidth: 1,
        verticalOrigin: Cesium.VerticalOrigin.BOTTOM,
        pixelOffset: new Cesium.Cartesian2(75, -50),
        showBackground: !0,
        backgroundColor: new Cesium.Color(0, 0, 0, 0.7),
        // disableDepthTestDistance: Number.POSITIVE_INFINITY,
        distanceDisplayCondition: new Cesium.DistanceDisplayCondition(0, 30000)
      }
    })
  },
  delFjModel() {
    this.removeEntitiesByIncludesName('id', 'fjModel')
    this.FJDataArr = []
    this.FJLabel = []
  },
  delUAVModel() {
    this.removeEntitiesByIncludesName('id', 'UAVModel')
    this.UAVDataArr = []
    this.UAVDataJson = {}
  },

  /***风机类型：
   *    concreteSoil---混塔
   *    steelSoil---钢塔
   */
  addFjModel(
    id,
    lon = 110.63767,
    lat = 22.59,
    h = 479,
    key = 'temp',
    defectKey = '',
    defectId = -1,
    dec = undefined,
    modelTpye
  ) {
    let JSPosition = Cesium.Cartesian3.fromDegrees(lon, lat, 0)
    let FLPosition = Cesium.Cartesian3.fromDegrees(lon, lat + 0.000038, modelTpye === 'concretesoil' ? (h + 137.1) : (h + 161.8))
    // let FLPosition = Cesium.Cartesian3.fromDegrees(lon, lat + 0.000075, 167.1)
    let roll = 0
    const modelScale = modelTpye === 'concretesoil' ? 0.04 : 0.05
    const radian = 2
    const JSName = 'entitie-'
    const FLName = 'entitie_' + 'fenglun-'
    const defectJson = {
      attention: Cesium.Color.fromCssColorString('rgba(255, 230, 4)'), // 一般缺陷为黄色
      warning: Cesium.Color.fromCssColorString('rgba(255, 107, 14)'), // 重大缺陷为深黄色
      danger: Cesium.Color.fromCssColorString('rgba(255, 15, 13)') // 紧急缺陷为红色
    }
    let defectJsonVal = defectJson[defectKey]
    function updateFanRotation() {
      roll += Cesium.Math.toRadians(radian) // 每帧旋转1度
      let hpr = new Cesium.HeadingPitchRoll(0, roll, 0)
      let orientation = Cesium.Transforms.headingPitchRollQuaternion(FLPosition, hpr)
      return orientation
    }
    function handleEntityModel(name, position, uri) {
      return {
        id: 'fjModel' + name + id,
        name: name + key,
        flag: 'fjModel',
        position,
        model: {
          uri,
          show: true,
          scale: new Cesium.CallbackProperty(() => {
            return modelScale
          }, false),
          colorBlendMode: Cesium.ColorBlendMode.HIGHLIGHT,
          shadows: Cesium.ShadowMode.DISABLE //GPU 每帧压力最低
        },
        description: dec
      }
    }
    const selectModelJS =
      modelTpye === 'concretesoil' ? '/assets/data/tatong222.glb' : '/assets/data/cabin_tower.glb'
    const selectModelFL =
      modelTpye === 'concretesoil' ? '/assets/data/fenglun222.glb' : '/assets/data/blade-semi.glb'
    const JSModel = handleEntityModel(JSName, JSPosition, selectModelJS)
    const FLModel = handleEntityModel(FLName, FLPosition, selectModelFL)
    FLModel.orientation = new Cesium.CallbackProperty(updateFanRotation, false)
    for (let i = 0; i < 2; i++) {
      const entityModelVal = i === 0 ? JSModel : FLModel
      if (defectJsonVal) {
        entityModelVal.model['color'] = new Cesium.CallbackProperty(() => {
          return defectJsonVal
        }, false)
        // modelJson['lightColor'] = new Cesium.Cartesian3(5, 5, 5)
      }
    }
    const JSModelEntity = viewer.entities.add(JSModel)
    const FLModelEntity = viewer.entities.add(FLModel)
    FLModelEntity.parent = JSModelEntity
    this.FJDataArr.push(JSModelEntity)
    this.FLDataArr.push(FLModelEntity)
    /* this.FLDataArr.push({
      entity: FLModelEntity,
      speed: 0.3 + Math.random() * 0.4, // 0.3~0.7 rad/s
      offset: Math.random() * Math.PI * 2 // 随机起始角度
    }) */
    this.FJDataJson[key] = 1
  },

  addUAVModel(
    lon = 110.63767,
    lat = 22.30259,
    h = 479,
    key = 'temp',
    defectKey = '',
    defectId = -1
  ) {
    var position = Cesium.Cartesian3.fromDegrees(lon, lat, h)

    const defectJson = {
      // 正常为白色; 正常为原皮肤
      1007001: new Cesium.Color(255, 255, 0, 1), // 一般缺陷为黄色
      1007002: Cesium.Color.DARKORANGE, // 重大缺陷为深黄色
      1007003: new Cesium.Color(1.0, 0, 0, 1) // 紧急缺陷为红色
    }
    let defectJsonVal = defectJson[defectKey]
    let modelJson = {
      show: true,
      uri: '/assets/data/CesiumDrone.gltf',
      scale: 1,
      minimumPixelSize: 60,
      maximumScale: 80
    }
    defectJsonVal && (modelJson['color'] = defectJsonVal)

    var modelXxx = viewer.entities.add({
      id: `UAVModel - ${this.getMapUUID()}`,
      name: key,
      position: position,
      defectId: defectId,
      model: modelJson
    })

    this.UAVDataArr.push(modelXxx)
    this.UAVDataJson[key] = 1
  },
  /**
   * CZML数据源创建线
   * @param {Object} polyLinePositions 经纬度坐标点数据集合（多个坐标点组成一条线）:[109.740, 19.997, 500, 109.860, 19.997, 500]
   */
  creatLine(polyLinePositions) {
    var czmlLine = this.getCzmlLine(polyLinePositions)
    var dronePromise = Cesium.CzmlDataSource.load(czmlLine)
    var drone
    dronePromise.then(function (dataSource) {
      viewer.dataSources.add(dataSource)
      drone = dataSource.entities.getById('Aircraft/Aircraft1')
      drone.orientation = new Cesium.VelocityOrientationProperty(drone.position)
      viewer.clock.shouldAnimate = true
    })
  },

  /**
   * 定义并封装CZML数据源
   * @param {Object} cartographicDegrees 时间(秒)、经度、纬度、高度坐标点数据集合（多个坐标点组成一条线）:[10,118.67247104644775,32.171616706674456,100,]
   */
  getCzmlLine(cartographicDegrees) {
    var czmlLine = [
      {
        id: 'document',
        name: 'CZML Model',
        version: '1.0',
        clock: {
          interval: '2012-08-04T10:00:00Z/2012-08-04T15:00:00Z',
          currentTime: '2012-08-04T10:00:00Z',
          range: 'LOOP_STOP',
          step: 'SYSTEM_CLOCK_MULTIPLIER'
          // multiplier: 10,
        }
      },
      {
        id: 'Aircraft/Aircraft1',
        name: 'UAVModel',
        path: {
          material: {
            polylineOutline: {
              color: {
                rgba: [255, 0, 255, 255]
              },
              outlineColor: {
                rgba: [0, 255, 255, 255]
              },
              outlineWidth: 2
            }
          },
          width: 3,
          maximumWidth: 5,
          leadTime: 5,
          trailTime: 1000,
          resolution: 5
        },
        position: {
          epoch: '2012-08-04T10:00:00Z',
          cartographicDegrees: [...cartographicDegrees]
        },
        model: {
          gltf: '/assets/data/CesiumDrone.gltf',
          minimumPixelSize: 60,
          maximumScale: 80,
          scale: 100
        }
      }
    ]
    return czmlLine
  },

  /*******
   * @description: 相机绕点飞行
   * @param {*} options 相机起始位置
   * @param {*} pitchRed 相机看点的角度
   * @param {*} distancece 相机距离点
   * @param {*} timeRed 飞行一周所需时间
   * @param {*} time 绕点时间
   * @param {Function} finshFunSurr 执行结束后方法
   * @return {*}
   */
  surroundPoint(options, pitchRed, distancece, timeRed, time, finshFunSurr) {
    let position = Cesium.Cartesian3.fromDegrees(options.lng, options.lat, options.height)
    // 相机看点的角度，如果大于0那么则是从地底往上看，所以要为负值，这里取-30度
    let pitch = Cesium.Math.toRadians(pitchRed)
    // 给定飞行一周所需时间，比如10s, 那么每秒转动度数
    let angle = 360 / timeRed
    // 给定相机距离点多少距离飞行，这里取值为5000m
    let distance = distancece
    let startTime = Cesium.JulianDate.fromDate(new Date())
    let stopTime = Cesium.JulianDate.addSeconds(startTime, time, new Cesium.JulianDate())
    viewer.clock.startTime = startTime.clone() // 开始时间
    viewer.clock.stopTime = stopTime.clone() // 结速时间
    viewer.clock.currentTime = startTime.clone() // 当前时间
    viewer.clock.clockRange = Cesium.ClockRange.CLAMPED // 行为方式
    viewer.clock.clockStep = Cesium.ClockStep.SYSTEM_CLOCK // 时钟设置为当前系统时间; 忽略所有其他设置。
    // 相机的当前heading
    let initialHeading = viewer.camera.heading
    let Exection = () => {
      // 当前已经过去的时间，单位s
      let delTime = Cesium.JulianDate.secondsDifference(
        viewer.clock.currentTime,
        viewer.clock.startTime
      )
      let heading = Cesium.Math.toRadians(delTime * angle) + initialHeading
      viewer.scene.camera.setView({
        destination: position, // 点的坐标
        orientation: {
          heading: heading,
          pitch: pitch
        }
      })
      viewer.scene.camera.moveBackward(distance)

      if (Cesium.JulianDate.compare(viewer.clock.currentTime, viewer.clock.stopTime) >= 0) {
        viewer.clock.onTick.removeEventListener(Exection, this)
        finshFunSurr()
      }
    }
    viewer.clock.onTick.addEventListener(Exection, this)
  },
  getSurfaceDistance(strat, endl) {
    if (!strat || !endl) return
    let satrt = Cesium.Cartographic.fromDegrees(strat.x1, strat.y1, strat.z1)
    let end = Cesium.Cartographic.fromDegrees(endl.x2, endl.y2, endl.z2)
    let geodesic = new Cesium.EllipsoidGeodesic()
    geodesic.setEndPoints(satrt, end)
    let distance = geodesic.surfaceDistance
    return distance
  },
  // 添加图片
  addGraphicsEventIcon(data) {
    let { positions, img, imgSize, offset, scale, id } = data
    let entitie = viewer.entities.add({
      tempData: positions,
      position: Cesium.Cartesian3.fromDegrees(positions[0], positions[1], positions[2] || 0),
      name: 'event-' + name,
      id: 'event-' + id,
      description: {
        copyImg: img
      },
      billboard: {
        image: img,
        width: imgSize[0],
        height: imgSize[1],
        disableDepthTestDistance: Number.POSITIVE_INFINITY,
        verticalOrigin: window.Cesium.VerticalOrigin.BOTTOM,
        sizeInMeters: true,
        pixelOffset: new Cesium.Cartesian2(0, offset || 0),
        scale
      }
    })
    this.eventIcon.push(entitie)
    return entitie
  },
  // 获取相机可视范围
  judgmentVision() {
    let rectangle = viewer.camera.computeViewRectangle()
    const FJDataArr = viewer.entities.values
    const obj = {
      exist: [],
      notExist: []
    }
    for (let i = 0; i < FJDataArr.length; i++) {
      if (FJDataArr[i].name !== 'label') {
        const elementValue = FJDataArr[i].position._value
        let isArea = Cesium.Rectangle.contains(
          rectangle,
          Cesium.Cartographic.fromCartesian(elementValue)
        )
        if (isArea) {
          obj.exist.push(FJDataArr[i])
        } else {
          obj.notExist.push(FJDataArr[i])
        }
      }
    }
    return obj
  },
  // 以中心点绘制矩形返回Rectangle范围
  CreateRange(position, long, wide) {
    let L2W = Cesium.Transforms.localFrameToFixedFrameGenerator('east', 'up')(position) //中心点
    const computedposition = (LW, x, y, z) => {
      let temp = Cesium.Matrix4.multiplyByPoint(
        LW,
        Cesium.Cartesian3.fromElements(x, y, z),
        new Cesium.Cartesian3()
      )
      return temp
    }
    let LT = computedposition(L2W, -wide, 0, -long)
    let LB = computedposition(L2W, -wide, 0, long)
    let RT = computedposition(L2W, wide, 0, -long)
    let RB = computedposition(L2W, wide, 0, long)
    const range = [LT, LB, RB, RT]
    let newPOl = range.map(item => {
      return this.cartesianTolngLatAlt(item)
    })
    const langArr = []
    const latArr = []
    newPOl.forEach(item => {
      langArr.push(item.lng)
      latArr.push(item.lat)
    })
    return Cesium.Rectangle.fromDegrees(min(langArr), min(latArr), max(langArr), max(latArr))
  },
  // 检索实体在范围内实体集合
  entitiesVision(rectangle) {
    const obj = {
      exist: [],
      notExist: []
    }
    const FJDataArr = viewer.entities.values
    for (let i = 0; i < FJDataArr.length; i++) {
      if (
        !/label[\-]+?/g.test(FJDataArr[i].name) &&
        FJDataArr[i].name &&
        !/event[\-]+?/g.test(FJDataArr[i].name)
      ) {
        const elementValue = FJDataArr[i].position._value
        let isArea = Cesium.Rectangle.contains(
          rectangle,
          Cesium.Cartographic.fromCartesian(elementValue)
        )
        if (isArea) {
          obj.exist.push(FJDataArr[i])
        } else {
          obj.notExist.push(FJDataArr[i])
        }
      }
    }
    return obj
  },
  /*******
   * @description: 根据id隐藏风机
   * @param {*} all isAll是否为全部,bool隐藏/显示
   * @param {*} ids id【】
   * @param {*} show 单个id隐藏/显示
   * @param {*} FJDataArr 集合
   * @param {*} label 是否为标签
   * @param {*} opt
   * @return {*}
   */
  getInclude(
    all = { isAll: false, bool: true },
    ids,
    show,
    FJDataArr = this.FJDataArr,
    label = false,
    opt
  ) {
    let temp = []
    const { isAll, bool } = all
    for (const item of FJDataArr) {
      if (!isAll && ids.includes(item.id)) {
        item.show = show
        temp.push(item)
      } else {
        item.show = isAll ? bool : !show
      }
      if (label) {
        const { company } = opt
        let str = item['name'].replace(/label[\-]+?/g, '') + (company ? `\-${company}` : '')
        item.billboard.image = this.handleCanvas(
          str,
          item.description._value.colorCopy || '#2ED133',
          120
        )
        item.billboard.width = str.length * 11 + 10
      }
    }
    return temp
  },
  getPixelRatio(context) {
    let backingStore =
      context.backingStorePixelRatio ||
      context.webkitBackingStorePixelRatio ||
      context.mozBackingStorePixelRatio ||
      context.msBackingStorePixelRatio ||
      context.oBackingStorePixelRatio ||
      context.backingStorePixelRatio ||
      1
    return (window.devicePixelRatio || 1) / backingStore
  },
  handleCanvas(name, color = '#2ED133', width = 101.03) {
    const canvas = document.createElement('canvas')
    const ctx = canvas.getContext('2d')
    const ratio = 1
    canvas.width = name.length * 11 * ratio + 10
    canvas.height = 25.89 * ratio
    ctx.fillStyle = 'rgba(255, 255, 255, 0.2)'
    ctx.fillRect(0, 0, width, 25.89)
    ctx.fillStyle = color
    ctx.fillRect(0, 0, 9.19, 25.89)
    ctx.font = '11px Arial'
    ctx.fillStyle = '#fff'
    ctx.fillText(name, 17.46, 17)
    ctx.textAlign = 'right'
    let image = new Image()
    image.src = canvas.toDataURL('image/jpg', 1)
    return image
  }
}

export default UtilData
