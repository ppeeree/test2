export default {
  // 风机实体集合
  FJDataArr: [],
  // 风机实体标签集合
  FJLabel: [],
  // 事件图标集合
  eventIcon: [],
  // 弹框标签
  popupList: [],
  // 弹窗dom
  infoDom: null,
  // 清除弹框标签
  handleClearPopupList() {
    this.popupList.length > 0 &&
      this.popupList.forEach(item => {
        // item.domRove()
        item.windowClose()
      })
    this.popupList = []
  },
  // 清空缓存
  handleClearAllData() {
    this.FJDataArr = []
    this.FJLabel = []
    this.eventIcon = []
    this.popupList = []
    this.infoDom = null
  }
}
