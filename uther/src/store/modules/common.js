import {
  setStore,
  getStore,
  removeStore
} from '@/util/store'
import website from '@/config/website'

const common = {

  state: {
    language: 'zh',//getStore({name: 'language'}) || 'zh',
    isCollapse: true, // 默认左侧菜单折叠收起
    isFullScren: false,
    isMenu: true,
    isShade: false,
    screen: -1,
    isLock: getStore({ name: 'isLock' }) || false,
    showTag: false,
    showDebug: true,
    showCollapse: true,
    showSearch: true,
    showLock: true,
    showFullScren: true,
    showTheme: true,
    showMenu: true,
    showColor: true,
    colorName: getStore({ name: 'colorName' }) || '#409EFF',
    themeName: getStore({ name: 'themeName' }) || 'theme-screen',
    lockPasswd: getStore({ name: 'lockPasswd' }) || '',
    website: website,
    timeOutObj: null
  },
  mutations: {
    SET_LANGUAGE: (state, language) => {
      state.language = language
      setStore({
        name: 'language',
        content: state.language
      })
    },
    SET_SHADE: (state, active) => {
      state.isShade = active
    },
    SET_COLLAPSE: (state) => {
      state.isCollapse = !state.isCollapse
    },
    SET_FULLSCREN: (state) => {
      state.isFullScren = !state.isFullScren
    },
    SET_IS_MENU: (state, menu) => {
      state.isMenu = menu
    },
    SET_LOCK: (state) => {
      state.isLock = true
      setStore({
        name: 'isLock',
        content: state.isLock,
        type: 'session'
      })
    },
    SET_SCREEN: (state, screen) => {
      state.screen = screen
    },
    SET_COLOR_NAME: (state, colorName) => {
      state.colorName = colorName
      setStore({
        name: 'colorName',
        content: state.colorName
      })
    },
    SET_THEME_NAME: (state, themeName) => {
      state.themeName = themeName
      setStore({
        name: 'themeName',
        content: state.themeName
      })
    },
    SET_LOCK_PASSWD: (state, lockPasswd) => {
      state.lockPasswd = lockPasswd
      setStore({
        name: 'lockPasswd',
        content: state.lockPasswd,
        type: 'session'
      })
    },
    CLEAR_LOCK: (state) => {
      state.isLock = false
      state.lockPasswd = ''
      removeStore({
        name: 'lockPasswd',
        type: 'session'
      })
      removeStore({
        name: 'isLock',
        type: 'session'
      })
    },
    SET_TIME: (state, data) => {
      state.timeOutObj = data
    }
  }
}
export default common
