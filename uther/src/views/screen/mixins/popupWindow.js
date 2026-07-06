import { createApp } from 'vue'

export default class PopupWindow {
  constructor(viewer, position, data, domVue, opt) {
    this.mountNode = document.createElement('div')
    this.app = createApp(domVue)
    this.vmInstance = this.app.mount(this.mountNode)
    this.vmInstance.detail = data
    this.viewer = viewer
    this.opt = opt
    this.position = position
    this.tablePopupCell = false
    this.vmInstance.closeEvent = () => {
      this.windowClose()
    }

    viewer.cesiumWidget.container.appendChild(this.vmInstance.$el)
    this.addPostRender(position)
  }

  addPostRender() {
    this.viewer.scene.postRender.addEventListener(this.postRender, this)
  }

  postRender() {
    if (!this.vmInstance.$el || !this.vmInstance.$el.style) return
    const canvasHeight = this.viewer.scene.canvas.height
    const windowPosition = new window.Cesium.Cartesian2()
    window.Cesium.SceneTransforms.wgs84ToWindowCoordinates(
      this.viewer.scene,
      this.position,
      windowPosition
    )
    this.tablePopupCell = sessionStorage.getItem('tablePopupCell') === 'true'
    this.vmInstance.$el.style.bottom = canvasHeight - windowPosition.y + (this.opt?.y || 0) + 'px'
    this.vmInstance.$el.style.left = windowPosition.x + (this.tablePopupCell ? -200 : 20) + (this.opt?.x || 0) + 'px'
    this.vmInstance.$el.style.display = this.viewer.camera.positionCartographic.height < (this.opt?.maxCameraHeight || 19000)
      ? 'block'
      : 'none'
  }

  windowClose() {
    this.viewer.selectedEntity = null
    this.vmInstance.show = false
    this.viewer.scene.postRender.removeEventListener(this.postRender, this)
    this.app.unmount()
    if (this.mountNode.parentNode) {
      this.mountNode.parentNode.removeChild(this.mountNode)
    }
  }
}
