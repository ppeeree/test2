import { getFanModelData, getUnitDefaultCard } from '@/api/screen/index'
import { levelColor } from '@/util/constant'
import CesuimCanvas from '@/util/cesuimCanvas'
// import { mockListUAVData } from '../mocks/index'
import { upObjVal } from '@/util/exp'

export default {
  methods: {
    async initData(id) {
      id || (id = this.WindFarm.id)
      const { data: res } = await getFanModelData(id)
      if (!res?.data) return
      // upObjVal(this.WindFarm, res.data[0])
      // this.getUnitDefaultCard(res) 机组故障卡片
      /*  res?.data.forEach(item => {
         this.getDrawLineDept(item.childNode)
       }) */
      if (this.$utils.map.FJDataArr?.length) {
        this.$utils.map.delFjModel() // 先清空
      }
      this.getDrawLineDept(res.data)
    },
    getDrawLineDept(arr) {
      arr.forEach((item, index) => {
        const {
          elevation,
          latitude,
          longitude,
          deviceModel,
          deviceVendor,
          operationalDate,
          poweringRate,
          deviceID,
          deviceName,
          deviceStatus,
          deviceModelType
        } = item
        let image = this.handleCanvas(deviceName, levelColor[deviceStatus] || '#2ED133')
        let lon = +longitude
        let lat = +latitude
        let height = parseInt(elevation) || +elevation === 0 ? +elevation : 479
        let dec = {
          deviceVendor,
          operationalDate,
          poweringRate,
          deviceModel,
          position: {
            elevation,
            latitude,
            longitude
          },
          infoId: index,
          deviceStatus
        }
        if (Cesium) {
          this.$utils.map.addFjModel(
            deviceID,
            lon,
            lat,
            height,
            deviceName,
            deviceStatus,
            -1,
            dec,
            deviceModelType.toLowerCase()
            // (modelTpye = 'concreteSoil')
          )
        }
        const params = {
          positions: [lon, lat, height],
          img: image,
          id: deviceID,
          name: deviceName
        }
        const labelDec = {
          colorCopy: levelColor[deviceStatus]
        }
        // 添加label
        this.addGraphicsl(params, labelDec)
      })
    },
    // 添加lable
    handleCanvas(name, color = '#2ED133') {
      const canvas = document.createElement('canvas')
      const ctx = canvas.getContext('2d')
      canvas.width = name.length * 20
      canvas.height = 25.89
      ctx.fillStyle = 'rgba(255, 255, 255, 0.2)'
      ctx.fillRect(0, 0, 101.03, 25.89)
      ctx.fillStyle = color
      ctx.fillRect(0, 0, 9.19, 25.89)
      ctx.font = '11px Arial'
      ctx.fillStyle = '#fff'
      ctx.fillText(name, 17.46, 17)
      ctx.textAlign = 'right'
      let image = new Image()
      image.src = canvas.toDataURL('image/jpg', 1)
      return image
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

    // 添加图片
    addGraphicsl(data, dec) {
      // eslint-disable-next-line no-unused-vars
      let { positions, img, imgSize, scale, id, name } = data
      const entitie = window.viewer.entities.add({
        tempData: positions,
        position: window.Cesium.Cartesian3.fromDegrees(
          positions[0],
          positions[1],
          positions[2] || 0
        ),
        show: true,
        // eslint-disable-next-line no-useless-escape
        name: 'fjModellabel-' + name,
        // eslint-disable-next-line no-useless-escape
        id: 'fjModellabel-' + id,
        description: dec,
        billboard: {
          image: img,
          // width: imgSize[0],
          // height: imgSize[1],
          // scale,
          pixelOffset: new window.Cesium.Cartesian2(65, -5),
          verticalOrigin: window.Cesium.VerticalOrigin.BOTTOM,
          disableDepthTestDistance: Number.POSITIVE_INFINITY,
          distanceDisplayCondition: new window.Cesium.DistanceDisplayCondition(0, 30000)
          // sizeInMeters: true
        }
      })
      this.$utils.map.FJLabel.push(entitie)
    },
    addGraphcs(res) {
      let cesuimCanvas = new CesuimCanvas(window.Cesium, window.viewer)
      res.forEach(
        ({ type, eventGrade, unreadEventSize, longitude, latitude, elevation, deviceID }) => {
          type &&
            eventGrade &&
            unreadEventSize &&
            cesuimCanvas.handleEventIconImg(type, eventGrade, unreadEventSize).then(res => {
              const params = {
                positions: [+longitude, +latitude, +elevation + 150],
                img: res,
                imgSize: [35, 47],
                scale: 0.3,
                id: deviceID
              }
              this.$utils.map.addGraphicsEventIcon(params)
            })
        }
      )
    },
    async getUnitDefaultCard(res) {
      const localValue =
        JSON.parse(sessionStorage.getItem('selectWindFarm'))?.id ||
        JSON.parse(localStorage.getItem('saber-userInfo'))?.content.dept_id ||
        null
      const { data } = await getUnitDefaultCard(localValue)
      this.$utils.map.UnitDefaultCard =
        data?.data && data?.data.length !== 0
          ? res.data[0].childNode.reduce((acc, cur) => {
            const target = acc.find(e => e.deviceID === cur.deviceID)
            if (target) {
              Object.assign(target, cur)
            } else {
              acc.push(cur)
            }
            return acc
          }, data.data[0].turbineList)
          : []
      this.addGraphcs(this.$utils.map.UnitDefaultCard)
    },
    // 无人机Model
    setUAVModel() {
      // this.$utils.map.addUAVModel('107.520349', '33.984809', 4600, 'WRJ', '')
      // let pointsArr = mockListUAVData.call(this)
      this.$utils.map.creatLine(pointsArr.map(Number))
    }
  }
}
