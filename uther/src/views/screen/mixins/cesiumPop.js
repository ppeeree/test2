import min from 'lodash/min'
import findIndex from 'lodash/findIndex'
import omit from 'lodash/omit'
import PopupWindow from '../mixins/popupWindow'
import popup from '../charts/popup.vue'
import dayjs from 'dayjs'

export default {
  data() {
    return {
      rightClickSiteBillboardId: '',
      popTitle: '',
      distance: [],
      infoId: null
    }
  },
  methods: {
    handleDistent(b) {
      let a = {
        x: window.viewer.canvas.clientWidth / 2,
        y: window.viewer.canvas.clientHeight / 2 + 300
      }
      let dx = Number(a.x) - Number(b.x)
      let dy = Number(a.y) - Number(b.y)
      return Math.pow(dx ** 2 + dy ** 2, .5)
    },
    cameraMoveEnd(event) {
      const rectangle = this.$utils.map.CreateRange(event.position, 500, 500)
      this.distance = []
      const cameraPositon = this.$utils.map.cartesianTolngLatAlt(event.position)
      const { exist } = this.$utils.map.entitiesVision(rectangle)
      if (exist.length === 0 && this.$cesuimData.infoDom !== null) {
        this.$cesuimData.infoDom.windowClose()
        sessionStorage.removeItem('windowPopup')
        setTimeout(() => {
          this.$cesuimData.infoDom = null
          this.rightClickSiteBillboardId = ''
        }, 10)
        return
      }
      if (exist.length === 0 || cameraPositon.alt > 8000 || (exist.length === 1 && exist[0].id === this.$cesuimData.infoDom?.opt.id)) return
      let item = null
      if (exist.length > 1) {
        let splitExistIndex = []
        for (let i = 0; i < exist.length; i++) {
          const ele = exist[i]
          if (!ele.show) {
            splitExistIndex.push(i)
            continue
          }
          let longDect = this.$utils.map.transPosition(ele.position._value)
          this.distance.push(longDect ? this.handleDistent(this.$utils.map.transPosition(ele.position._value)) : 99999)
        }
        splitExistIndex.forEach(index => exist.splice(index, 1))
        const minDist = min(this.distance)
        const index = findIndex(this.distance, o => o === minDist)
        item = exist[index]
      } else {
        item = exist[0]
      }
      // const decer2 = this.$utils.map.transPosition(item)
      const pickedLabel = item
      // eslint-disable-next-line no-useless-escape
      if (!pickedLabel?.id || this.rightClickSiteBillboardId === pickedLabel.id || !(/entitie[\-]+?/g.test(pickedLabel.id))) return
      this.infoId = pickedLabel.description?._value.infoId
      if (this.$cesuimData.infoDom !== null) {
        this.$cesuimData.infoDom.windowClose()
        this.$cesuimData.infoDom = null
        this.rightClickSiteBillboardId = ''
        sessionStorage.removeItem('windowPopup')
      }
      this.setSessionItem('windowPopup', true)
      this.rightClickSiteBillboardId = pickedLabel.id
      // eslint-disable-next-line no-useless-escape
      const entitieItem = this.$utils.map.UnitDefaultCard.find(({ spaceGuid }) => spaceGuid === pickedLabel?.id.replace(/entitie[\-]+?/g, ''))
      // eslint-disable-next-line no-useless-escape
      this.popTitle = pickedLabel['name']?.replace(/entitie[\-]+?/g, '') || ''
      const { acqTime, name, type } = entitieItem.data[0]
      this.$cesuimData.infoDom = new PopupWindow(
        window.viewer,
        pickedLabel.position._value,
        {
          name: this.popTitle,
          entitieId: this.rightClickSiteBillboardId,
          part: entitieItem.data[1].windFarmEventsList.map(item => {
            return {
              acqTime: dayjs(item.acqTime).format('YYYY-MM-DD'),
              ...omit(item, ['acqTime'])
            }
          }),
          structure: {
            infoShow: true,
            partsFailure: true,
            partsCheck: false,
            progressShow: true
          },
          deviceStatus: entitieItem?.spaceState || 'normal',
          popIconT: [
            { title: name },
            { title: type },
            { title: acqTime }
          ],
          progressEvent: entitieItem?.data[2].data || []
        },
        popup,
        {
          maxCameraHeight: 7000,
          id: this.rightClickSiteBillboardId,
          infoId: this.infoId,
          y: 70
        }
      )
    }
  }
}
