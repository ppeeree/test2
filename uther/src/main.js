import { createApp } from 'vue'
import axios from './router/axios'
import App from './App'
import router from './router/router'
import './permission'
import installErrorHandler from './error'
import cacheMixin from './cache'
import store from './store'
import { loadStyle } from './util/util'
import { validatenull } from './util/validate'
import * as urls from '@/config/env'
import VueCesium from 'vue-cesium'
import lang from 'vue-cesium/lang/zh-hans'
import utils from './util/index'
import cesuimData from './util/cesuimData'
import eventBus from './util/event-bus'
import setSession from './util/setSession'
import { iconfontUrl, iconfontVersion } from '@/config/env'
import i18n from './lang'
import basicBlock from './components/basic-block/main'
import basicContainer from './components/basic-container/main'
import thirdRegister from './components/third-register/main'
import avueUeditor from './components/avue-ueditor-compat'
import website from '@/config/website'
import crudCommon from '@/mixins/crud'
import ElementPlus from 'element-plus'
import zhCn from 'element-plus/es/locale/lang/zh-cn'
import 'element-plus/dist/index.css'
import tenantPackage from './views/system/tenantpackage'
import * as echarts from 'echarts'
import dayjs from 'dayjs'
import directives from './utils/directive'

import './styles/common.scss'
import './assets/scss/theme.scss'

const app = createApp(App)

app.use(store)
app.use(router)
app.use(i18n)
app.use(ElementPlus, { locale: zhCn })
app.use(directives)
app.use(utils)
app.use(setSession)
app.mixin(cacheMixin)
installErrorHandler(app)

if (VueCesium && VueCesium.install) {
  app.use(VueCesium, {
    cesiumPath: '/assets/Cesium/Cesium.js',
    accessToken: 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiJiNDAwMTg3Yy1kNDQwLTQyNzktOWQ4OS0yNzIyMDYyNDcyYmMiLCJpZCI6Mzg5NTEsImlhdCI6MTYzMjY1MjAwOH0.eDhFxYQUUFfbubTrNB64sA_lerzTWkczrUMa-PLtbtM',
    lang: lang
  })
}
if (window.AVUE && window.AVUE.install) {
  app.use(window.AVUE, {
    size: 'small',
    tableSize: 'small',
    calcHeight: 65,
    i18n: (key, value) => i18n.global.t(key, value)
  })
}

window.$crudCommon = crudCommon

app.config.globalProperties.$axios = axios
app.config.globalProperties.$http = axios
app.config.globalProperties.$bus = eventBus
app.config.globalProperties.$cesuimData = cesuimData
app.config.globalProperties.$echarts = echarts
app.config.globalProperties.validatenull = validatenull
app.config.globalProperties.$filters = {
  timeFormater: () => dayjs().format('HH:mm')
}
app.config.globalProperties.website = website

Object.keys(urls).forEach(key => {
  app.config.globalProperties[key] = urls[key]
})

app.component('basicContainer', basicContainer)
app.component('basicBlock', basicBlock)
app.component('thirdRegister', thirdRegister)
app.component('avueUeditor', avueUeditor)
app.component('tenantPackage', tenantPackage)

iconfontVersion.forEach(ele => {
  loadStyle(iconfontUrl.replace('$key', ele))
})

app.mount('#app')
