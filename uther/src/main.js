import Vue from 'vue'
import axios from './router/axios'
import VueAxios from 'vue-axios'
import App from './App'
import router from './router/router'
import './permission' // 权限
import './error' // 日志
import './cache'//页面缓存
import store from './store'
import { loadStyle } from './util/util'
import * as urls from '@/config/env'
// import Element from 'element-ui'
import VueCesium from 'vue-cesium'
import lang from 'vue-cesium/lang/zh-hans'
import utils from './util/index'
import cesuimData from './util/cesuimData'
import { UxGrid, UxTableColumn } from 'umy-ui'
import 'umy-ui/lib/theme-chalk/index.css'

Vue.use(UxGrid)
Vue.use(UxTableColumn)

Vue.use(VueCesium, {
  cesiumPath: '/assets/Cesium/Cesium.js',
  accessToken: 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiJiNDAwMTg3Yy1kNDQwLTQyNzktOWQ4OS0yNzIyMDYyNDcyYmMiLCJpZCI6Mzg5NTEsImlhdCI6MTYzMjY1MjAwOH0.eDhFxYQUUFfbubTrNB64sA_lerzTWkczrUMa-PLtbtM',
  lang: lang
})

Vue.use(utils)

import {
  iconfontUrl,
  iconfontVersion
} from '@/config/env'
import i18n from './lang' // Internationalization
import basicBlock from './components/basic-block/main'
import basicContainer from './components/basic-container/main'
import thirdRegister from './components/third-register/main'
import avueUeditor from 'avue-plugin-ueditor'
import website from '@/config/website'
import crudCommon from '@/mixins/crud'
// 业务组件
import ElementUI from 'element-ui'
import tenantPackage from './views/system/tenantpackage'
import 'element-ui/lib/theme-chalk/index.css'
import locale from 'element-ui/lib/locale/lang/zh-CN'
import * as echarts from 'echarts'
// import 'echarts-gl'
import dayjs from 'dayjs'
import './util/setSession'
import directives from './utils/directive'

// 全局样式
import './styles/common.scss'
import './assets/scss/theme.scss'
// import './styles/load'

// 自定义指令
// 全局可拖拽指令
import '../src/utils/directive.js'

Vue.prototype.$bus = new Vue()
Vue.prototype.$cesuimData = cesuimData

// Vue.use(Echarts)
Vue.prototype.$echarts = echarts

Vue.filter('timeFormater', () => {
  return dayjs().format('HH:mm')
})

Vue.use(ElementUI)
Vue.use(directives)
// 注册全局crud驱动
window.$crudCommon = crudCommon
// 加载Vue拓展
Vue.use(router)
Vue.use(VueAxios, axios)
/* Vue.use(Element, {
  i18n: (key, value) => i18n.t(key, value)
}) */
Vue.use(window.AVUE, {
  size: 'small',
  tableSize: 'small',
  calcHeight: 65,
  i18n: (key, value) => i18n.t(key, value)
})
// 注册全局容器
Vue.component('basicContainer', basicContainer)
Vue.component('basicBlock', basicBlock)
Vue.component('thirdRegister', thirdRegister)
Vue.component('avueUeditor', avueUeditor)
Vue.component('tenantPackage', tenantPackage)
// Vue.component('v-chart',ECharts)
// 加载相关url地址
Object.keys(urls).forEach(key => {
  Vue.prototype[key] = urls[key]
})
// 加载NutFlow
// Vue.use(window.WfDesignBase)
// 加载website
Vue.prototype.website = website
// 动态加载阿里云字体库
iconfontVersion.forEach(ele => {
  loadStyle(iconfontUrl.replace('$key', ele))
})

Vue.config.productionTip = false

new Vue({
  router,
  store,
  i18n,
  render: h => h(App)
}).$mount('#app')
