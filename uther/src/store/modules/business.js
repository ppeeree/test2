import { setStore, getStore } from '@/util/store'

const business = {
  state: {
    manualDiagnosisListFetch: getStore({ name: 'manualDiagnosisListFetch' }) || [],
    turbinerunDict: getStore({ name: 'turbinerunDict' }) || [],

  },
  mutations: {
    setmanualDiagnosisListFetch: (state, payload) => {
      state.manualDiagnosisListFetch = payload
      setStore({ name: 'manualDiagnosisListFetch', content: state.manualDiagnosisListFetch })
    },
    turbinerunDict: (state, payload) => {
      state.turbinerunDict = payload
      setStore({ name: 'turbinerunDict', content: state.turbinerunDict })
    },

  },
  actions: {
    setanualDiagnosisListFetch: ({ commit }, payload) => {
      commit('setanualDiagnosisListFetch', payload)
    },
    turbinerunDict: ({ commit }, payload) => {
      commit('turbinerunDict', payload)
    },
  }
}

export default business
