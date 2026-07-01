/**
 * 全站权限配置 - 支持URL免密登录
 */
import router from './router/router'
import store from './store'
import { validatenull } from '@/util/validate'
import { getToken } from '@/util/auth'
import NProgress from 'nprogress'
import 'nprogress/nprogress.css'
NProgress.configure({ showSpinner: false })

const lockPage = store.getters.website.lockPage

/**
 * 从URL参数中提取登录凭证并执行自动登录
 * @param {Object} to - 目标路由
 * @returns {Promise<boolean>} - 是否成功执行了自动登录
 */
const handleAutoLogin = async (to) => {
  // 从 URL query 或 hash 中获取参数（兼容 hash 模式和 history 模式）
  const { username, password, autoLogin } = to.query

  // 参数校验：需要同时存在 username、password 且 autoLogin 为 true
  if (!username || !password || autoLogin !== 'true') {
    return false
  }

  try {
    // 显示加载进度条
    NProgress.start()

    // 调用登录接口（根据你的实际接口调整）
    const loginResult = await store.dispatch('LoginByUsername', {
      username: decodeURIComponent(username),
      password: decodeURIComponent(password),
      tenantId: '000000',
      // 标记为自动登录，后端可记录日志
      loginType: 'url_auto'
    })
    if (loginResult && loginResult.access_token) {
      // 登录成功，清除URL中的敏感参数（防止历史记录泄露）
      const { username: _, password: __, autoLogin: ___, ...cleanQuery } = to.query
      // 替换当前历史记录，移除敏感信息
      router.replace({
        path: to.path,
        query: cleanQuery,
        params: to.params
      }).catch(() => { })

      return true
    }
  } catch (error) {
    console.error('URL自动登录失败:', error)
    // 可选：显示错误提示
    // Message.error('自动登录失败，请手动登录')
  } finally {
    NProgress.done()
  }

  return false
}

router.beforeEach(async (to, from, next) => {
  const meta = to.meta || {}
  const isMenu = meta.menu === undefined ? to.query.menu : meta.menu
  store.commit('SET_IS_MENU', isMenu === undefined)

  // ========== 新增：URL免密登录逻辑 ==========
  // 未登录状态且URL携带登录参数时，尝试自动登录
  if (!getToken() && to.query.autoLogin === 'true') {
    const autoLoginSuccess = await handleAutoLogin(to)
    if (autoLoginSuccess) {
      // 自动登录成功，重新进入当前路由（此时已有token）
      return next({ ...to, replace: true })
    }
    // 自动登录失败，继续后续逻辑（跳转登录页或根据isAuth判断）
  }
  // ==========================================

  if (getToken()) {
    if (store.getters.isLock && to.path !== lockPage) {
      next({ path: lockPage })
    } else if (to.path === '/login') {
      next({ path: '/' })
    } else {
      // 修复：原代码判断逻辑有误，应为判断用户信息是否为空
      if (validatenull(store.getters.userInfo) || validatenull(store.getters.token)) {
        try {
          // 获取用户信息
          await store.dispatch('GetUserInfo')
          next({ ...to, replace: true })
        } catch (error) {
          // 获取信息失败，登出并跳转登录页
          await store.dispatch('FedLogOut')
          next({ path: '/login' })
        }
      } else {
        // 原有的标签页逻辑
        const value = to.query.src || to.fullPath
        const label = to.query.name || to.name
        const meta = to.meta || router.$avueRouter.meta || {}
        const i18n = to.query.i18n

        if (to.query.target) {
          window.open(value)
        } else if (meta.isTab !== false && !validatenull(value) && !validatenull(label)) {
          store.commit('ADD_TAG', {
            label: label,
            value: value,
            params: to.params,
            query: to.query,
            meta: (() => {
              if (!i18n) return meta
              return { i18n: i18n }
            })(),
            group: router.$avueRouter.group || []
          })
        }
        next()
      }
    }
  } else {
    // 未登录状态
    if (meta.isAuth === false) {
      next()
    } else {
      next('/login')
    }
  }
})

router.afterEach(() => {
  NProgress.done()
  let title = store.getters.tag?.label || ''
  let i18n = store.getters.tag?.meta?.i18n

  if (router.$avueRouter?.generateTitle) {
    title = router.$avueRouter.generateTitle(title, i18n)
  }

  if (router.history?.current?.fullPath === '/login') {
    title = '登录'
  }

  if (router.$avueRouter?.setTitle) {
    router.$avueRouter.setTitle(title)
  }
})