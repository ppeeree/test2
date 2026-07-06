import { createStore } from 'vuex'
import user from './modules/user'
import common from './modules/common'
import tags from './modules/tags'
import logs from './modules/logs'
import dict from './modules/dict'
import business from './modules/business'
import getters from './getters'

const store = createStore({
  modules: {
    user,
    common,
    logs,
    tags,
    dict,
    business
  },
  getters,
})

export default store
