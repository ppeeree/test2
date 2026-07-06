import { nextTick } from 'vue'
import store from './store'

export default function installErrorHandler(app) {
  app.config.errorHandler = function (err, vm, info) {
    nextTick(() => {
      store.commit('ADD_LOGS', {
        type: 'error',
        message: err.message,
        stack: err.stack,
        info
      })
      if (process.env.NODE_ENV === 'development') {
        window.console.group('>>>>>> error info >>>>>>')
        window.console.error(info)
        window.console.groupEnd()
        window.console.group('>>>>>> Vue instance >>>>>>')
        window.console.log(vm)
        window.console.groupEnd()
        window.console.group('>>>>>> Error >>>>>>')
        window.console.log(err)
        window.console.groupEnd()
      }
    })
  }
}
