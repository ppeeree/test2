export function setSessionItem(key, newVal) {
  let newStorageEvent = document.createEvent('StorageEvent')
  const storage = {
    setItem: function (k, val) {
      sessionStorage.setItem(k, val)
      newStorageEvent.initStorageEvent(
        'setItem', // 事件别名
        false,
        false,
        k,
        null,
        val,
        null,
        null
      )
      // 派发事件
      window.dispatchEvent(newStorageEvent)
    }
  }
  return storage.setItem(key, newVal)
}

export function setLocalstorageItem(key, newVal) {
  // 创建一个StorageEvent事件
  let newStorageEvent = document.createEvent('StorageEvent')
  const storage = {
    setItem: function (k, val) {
      localStorage.setItem(k, val)
      // 初始化创建的事件
      newStorageEvent.initStorageEvent('setLocalItem', false, false, k, null, val, null, null)
      // 派发对象
      window.dispatchEvent(newStorageEvent)
    }
  }
  return storage.setItem(key, newVal)
}

export default {
  install(app) {
    app.config.globalProperties.setSessionItem = setSessionItem
    app.config.globalProperties.setLocalstorageItem = setLocalstorageItem
  }
}
