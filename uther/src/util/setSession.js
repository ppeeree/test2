import Vue from 'vue'

Vue.prototype.setSessionItem = function (key, newVal) {
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

Vue.prototype.setLocalstorageItem = function (key, newVal) {
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
