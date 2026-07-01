export default {
  data() {
    return {
      billboard: {},
      scale: 0.7,
      show: true,
      distanceDisplayCondition: { near: 0, far: 20000000 },
      horizontalOrigin: 0
    }
  },
  methods: {
    handlePngPosition(item) {
      return {
        lng: +item.longitude,
        lat: +item.latitude,
        height: +item.elevation + 1200
      }
    },
    handlePngUrl(item) {
      console.log(`/img/screen/billboardIcon/Inspection/${item.eventLevel}.png`)
      return `/img/screen/billboardIcon/Inspection/${item.eventLevel}.png`
    }
  }
}
