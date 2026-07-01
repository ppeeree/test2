export default class CesiumMap {
  constructor(Cesium, viewer) {
    this.Cesium = Cesium
    this.viewer = viewer
  }

  /**
   * 初始化配置
   */
  initConfig() {
    this.viewer.skyBox = false
    this.viewer.scene.backgroundColor = new this.Cesium.Color(0.0, 0.0, 0.0, 0.0)
    // viewer.skyAtmosphere.show = true

    this.viewer.scene.globe.enableLighting = false
    this.viewer.animation && (this.viewer.animation.container.style.visibility = 'hidden') // 不显示动画控件
    this.viewer.clockViewModel.multiplier = 0.34 // 速度
    this.viewer.scene.globe.depthTestAgainstTerrain = true // 深度检测  （ false: 关闭  true: 打开 ）
    this.viewer.cesiumWidget.screenSpaceEventHandler.removeInputAction(
      this.Cesium.ScreenSpaceEventType.LEFT_DOUBLE_CLICK
    ) // 取消双击定位实体的功能
    this.viewer.cesiumWidget.screenSpaceEventHandler.removeInputAction(
      this.Cesium.ScreenSpaceEventType.LEFT_CLICK
    )
    this.viewer.scene.screenSpaceCameraController.tiltEventTypes = this.Cesium.CameraEventType.RIGHT_DRAG // 改变倾斜模型，默认中键按住，改为右键按住
    this.viewer.scene.screenSpaceCameraController.zoomEventTypes = this.Cesium.CameraEventType.WHEEL // 改变绽放模型，默认滚轮和右键按住拖放，现改为滚轮缩放
    this.viewer.scene.screenSpaceCameraController.rotateEventTypes = [
      this.Cesium.CameraEventType.LEFT_DRAG,
      this.Cesium.CameraEventType.MIDDLE_DRAG
    ] // 平移 添加鼠标左键  鼠标滚轮平移
    this.viewer.scene.backgroundColor = this.Cesium.Color.TRANSPARENT //背景色变成近似白色
    this.viewer.scene.globe.baseColor = this.Cesium.Color.TRANSPARENT
    this.viewer.scene.undergroundMode = true
    // viewer.scene.requestRenderMode = true //性能优化
    this.viewer.scene.maximumRenderTimeChange = Infinity //无限

    this.Cesium.RequestScheduler.maximumRequests = 100 // 默认值50  最大请求数
    this.Cesium.RequestScheduler.maximumRequestsPerServer = 12 //默认值6 单个服务器最大请求数
  }
  initAniMat(call) {
    // 地球旋转
    // viewer.clock.multiplier = 100; //速度
    // viewer.clock.shouldAnimate = false;
    let previousTime = this.viewer.clock.currentTime.secondsOfDay
    const onTickCallback = () => {
      let spinRate = 1
      let currentTime = this.viewer.clock.currentTime.secondsOfDay
      let delta = (currentTime - previousTime) / 1000
      previousTime = currentTime
      this.viewer.scene.camera.rotate(this.Cesium.Cartesian3.UNIT_Z, -spinRate * delta)
    }
    this.viewer.clock.onTick.addEventListener(onTickCallback)

    // 玉林轮廓
    let promise = this.Cesium.GeoJsonDataSource.load('/assets/data/yulin.json', {
      stroke: this.Cesium.Color.AQUAMARINE,
      fill: this.Cesium.Color.BLUE.withAlpha(0.2),
      strokeWidth: 3,
      clampToGround: true // 开启贴地
    })
    // eslint-disable-next-line no-unused-vars
    promise.then((dataSource) => {
      // viewer.dataSources.add(dataSource);
      // let entities = dataSource.entities.values;
      // entities.forEach((entity) => {
      //   entity.polygon.material = Cesium.Color.DARKTURQUOISE.withAlpha(0.2);
      //   entity.polygon.distanceDisplayCondition =
      //     new Cesium.DistanceDisplayCondition(100000, 1000000);
      //   entity.polyline = {
      //     distanceDisplayCondition: new Cesium.DistanceDisplayCondition(
      //       100000,
      //       1000000
      //     ),
      //     positions: entity.polygon.hierarchy._value.positions,
      //     material: Cesium.Color.AQUAMARINE,
      //   };
      // });
      // setTimeout(() => {
      this.viewer.clock.onTick.removeEventListener(onTickCallback)
      // viewer.clock.shouldAnimate = false;
      this.initMapStatus()

      call && call()
      // }, 2500)
    })
  }

    /**
   * 地图初始化的状态
   */
    initMapStatus(callback = null) {
    const param = {
      destination: {
        x: -1601993.4816972143,
        y: 5073368.217629324,
        z: 3535476.6776872696
      },
      heading: 6.215844491978787,
      pitch: -0.5976850405218044,
      roll: 0.000017052693810803987
    }
    /**
     * 定位到相应视角
     */
    this.viewer.camera.flyTo({
      complete: callback,
      destination: param.destination,
      duration: 5,
      orientation: {
        heading: param.heading,
        pitch: param.pitch,
        roll: param.roll
      }
    })
  }

  Cartesian3_flyTo(cartesian3, time = 2, callback = null, options) {
    this.viewer.camera.flyTo({
      duration: time,
      complete: callback(),
      destination: cartesian3,
      orientation: { ...options }
    })
  }

  /**
   * @description 将笛卡尔坐标系转成经纬度高程
   * @param {Object} cartesianObj 笛卡尔坐标系对象 {x, y, z}
   * @returns 返回经纬度高程对象
   */
  cartesianTolngLatAlt(cartesianObj) {
    if (!this.Cesium || !window.Cesium) {
      throw new Error('非cesium地图')
    }
    if (!cartesianObj || Object.keys(cartesianObj).length !== 3) {
      throw new Error('请传入合法的cartesian对象 {x, y, z}')
    }
    const cartesian3 = new this.Cesium.Cartesian3(
      cartesianObj.x,
      cartesianObj.y,
      cartesianObj.z
    )
    const cartographic =
    this.Cesium.Ellipsoid.WGS84.cartesianToCartographic(cartesian3)
    const lat = this.Cesium.Math.toDegrees(cartographic.latitude)
    const lng = this.Cesium.Math.toDegrees(cartographic.longitude)
    const alt = cartographic.height
    return { lng, lat, alt }
  }

  //三维笛卡尔坐标转屏幕坐标
  transPosition(position) {
    return this.Cesium.SceneTransforms.wgs84ToWindowCoordinates(this.viewer.scene, position)
  }

  /**
   * 获取当前三维范围
   */
   getCurrentExtent() {
    // 范围对象
    const extent = {}
    // 得到当前三维场景
    const scene = this.viewer.scene
    // 得到当前三维场景的椭球体
    const ellipsoid = scene.globe.ellipsoid
    const canvas = scene.canvas

    // canvas左上角
    const car3_lt = this.viewer.camera.pickEllipsoid(new this.Cesium.Cartesian2(0, 0), ellipsoid)
    // canvas左下角
    const car3_lt_bottom = this.viewer.camera.pickEllipsoid(
      new this.Cesium.Cartesian2(0, canvas.height),
      ellipsoid
    )
    // canvas右下角
    const car3_rb = this.viewer.camera.pickEllipsoid(
      new this.Cesium.Cartesian2(canvas.width, canvas.height),
      ellipsoid
    )
    // canvas右上角
    const car3_rb_top = this.viewer.camera.pickEllipsoid(
      new this.Cesium.Cartesian2(canvas.width, 0),
      ellipsoid
    )

    // 当canvas左上角和右下角全部在椭球体上
    if (car3_lt && car3_lt_bottom && car3_rb && car3_rb_top) {
      const carto_lt = ellipsoid.cartesianToCartographic(car3_lt)
      const carto_lt_bottom = ellipsoid.cartesianToCartographic(car3_lt_bottom)
      const carto_rb = ellipsoid.cartesianToCartographic(car3_rb)
      const carto_rb_top = ellipsoid.cartesianToCartographic(car3_rb_top)

      extent.lt_lon = this.Cesium.Math.toDegrees(carto_lt.longitude) // 左上角 经度
      extent.lt_lat = this.Cesium.Math.toDegrees(carto_lt.latitude) // 左上角 纬度

      extent.lb_lon = this.Cesium.Math.toDegrees(carto_lt_bottom.longitude) // 左下角 经度
      extent.lb_lat = this.Cesium.Math.toDegrees(carto_lt_bottom.latitude) // 左下角 纬度

      extent.rb_lon = this.Cesium.Math.toDegrees(carto_rb.longitude) // 右下角 经度
      extent.rb_lat = this.Cesium.Math.toDegrees(carto_rb.latitude) // 右下角 纬度

      extent.rt_lon = this.Cesium.Math.toDegrees(carto_rb_top.longitude) // 右上角 经度
      extent.rt_lat = this.Cesium.Math.toDegrees(carto_rb_top.latitude) // 右上角 纬度
    } else {
      return {}
    }

    // 获取高度
    extent.height = Math.ceil(this.viewer.camera.positionCartographic.height)

    // 返回 当前三维范围 数据
    return extent
  }

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
  }
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
    let newPOl = range.map((item) => {
      return this.cartesianTolngLatAlt(item)
    })
    const langArr = []
    const latArr = []
    newPOl.forEach((item) => {
      langArr.push(item.lng)
      latArr.push(item.lat)
    })
    return Cesium.Rectangle.fromDegrees(min(langArr), min(latArr), max(langArr), max(latArr))
  }
  // 检索实体在范围内实体集合
  entitiesVision(rectangle) {
    const obj = {
      exist: [],
      notExist: []
    }
    const FJDataArr = viewer.entities.values
    for (let i = 0; i < FJDataArr.length; i++) {
      if (!(/label[\-]+?/g.test(FJDataArr[i].name)) && FJDataArr[i].name && !(/event[\-]+?/g.test(FJDataArr[i].name))) {
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
  }

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
  getInclude(all = { isAll: false, bool: true }, ids, show, FJDataArr = this.FJDataArr, label = false, opt) {
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
        const { company, color } = opt
        item.billboard.image = this.handleCanvas((item['name'].replace(/label[\-]+?/g, '') + ( company ? `\-${company}` : '')), color || '#2ED133', 120)
      }
    }
    return temp
  }

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
  }

  handleCanvas(name, color = '#2ED133', width = 101.03) {
    const canvas = document.createElement('canvas')
    const ctx = canvas.getContext('2d')
    const ratio = this.getPixelRatio(ctx)
    canvas.width = width * ratio
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

  // 添加图片
  addGraphicsEventIcon(data) {
    let { positions, img, imgSize, offset, scale, id } = data
    let entitie = viewer.entities.add({
      tempData: positions,
      position: Cesium.Cartesian3.fromDegrees(positions[0], positions[1], positions[2] || 0),
      name: 'event\-' + name,
      id: 'event\-' + id,
      billboard: {
        image: img,
        width: imgSize[0],
        height: imgSize[1],
        disableDepthTestDistance: Number.POSITIVE_INFINITY,
        verticalOrigin: window.Cesium.VerticalOrigin.BOTTOM,
        pixelOffset: new Cesium.Cartesian2(0, offset || 0),
        scale
      }
    })
    this.eventIcon.push(entitie)
    return entitie
  }

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
      // viewer.scene.postRender.addEventListener(function() {
      // 	var position = drone.position.getValue(viewer.clock.currentTime);
      // 	// 运行中则打印地理坐标位置
      // 	if(viewer.clock.shouldAnimate){
      //     console.log(position)
      // 	}
      // });
    })
  }

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
  }
}
