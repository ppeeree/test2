/**
 * 全站路由配置
 */
import { createRouter as createVueRouter, createWebHashHistory } from 'vue-router'
import PageRouter from './page/'
import ViewsRouter from './views/'
import AvueRouter from './avue-router'
import i18n from '@/lang'
import Store from '../store/'

const baseRoutes = [...PageRouter, ...ViewsRouter]

export const createRouter = () => createVueRouter({
  history: createWebHashHistory(process.env.BASE_URL),
  routes: baseRoutes
})

const Router = createRouter()

AvueRouter.install(null, {
  router: Router,
  store: Store,
  i18n: i18n,
  keepAlive: false,
})

Router.$avueRouter.formatRoutes(Store.state.user.menuAll, true)

export function resetRouter() {
  Router.getRoutes().forEach(route => {
    if (route.name && Router.hasRoute(route.name)) {
      Router.removeRoute(route.name)
    }
  })
  baseRoutes.forEach(route => Router.addRoute(route))
  AvueRouter.install(null, {
    router: Router,
    store: Store,
    i18n: i18n,
    keepAlive: false,
  })
}

export default Router
