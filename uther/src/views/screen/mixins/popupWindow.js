import Vue from 'vue'
export default class PopupWindow {
  constructor(viewer, position, data, domVue, opt) {
    let WindowVm = Vue.extend(domVue)
    // console.log(viewer, position, data)
    this.vmInstance = new WindowVm().$mount()
    this.vmInstance.detail = data
    this.viewer = viewer
    this.opt = opt
    this.position = position
    this.tablePopupCell = false
    //点击窗口上的关闭按钮
    this.vmInstance.closeEvent = () => {
      this.windowClose()
    }

    viewer.cesiumWidget.container.appendChild(this.vmInstance.$el)
    this.addPostRender(position)
  }

  //添加场景事件
  addPostRender() {
    this.viewer.scene.postRender.addEventListener(this.postRender, this)
  }

  //场景渲染事件 实时更新标签的位置 使其与笛卡尔坐标一致
  postRender() {
    if (!this.vmInstance.$el || !this.vmInstance.$el.style) return
    const canvasHeight = this.viewer.scene.canvas.height
    const windowPosition = new window.Cesium.Cartesian2()
    window.Cesium.SceneTransforms.wgs84ToWindowCoordinates(
      this.viewer.scene,
      this.position,
      windowPosition
    )
    if (sessionStorage.getItem('tablePopupCell') === 'true') {
      this.tablePopupCell = true
    } else {
      this.tablePopupCell = false
    }
    this.vmInstance.$el.style.bottom = canvasHeight - windowPosition.y + (this.opt?.y || 0) + 'px'
    // const elWidth = this.vmInstance.$el.offsetWidth
    this.vmInstance.$el.style.left = windowPosition.x + ( this.tablePopupCell ? -200 : 20) + (this.opt?.x || 0) + 'px'
    if (!(this.viewer.camera.positionCartographic.height < (this.opt?.maxCameraHeight || 19000))) {
      this.vmInstance.$el.style.display = 'none'
    } else {
      this.vmInstance.$el.style.display = 'block'
    }
  }

  windowClose() {
    this.viewer.selectedEntity = null
    this.vmInstance.show = false //删除dom
    this.viewer.scene.postRender.removeEventListener(this.postRender, this) //移除事件监听
  }
}
