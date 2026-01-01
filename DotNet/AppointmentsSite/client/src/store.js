import Vue from 'vue'
import Vuex from 'vuex'

Vue.use(Vuex)

export default new Vuex.Store({
  state: {
    loggedIn: null
  },
  mutations: {
    setLoggedIn(state, val) {
      state.loggedIn = val
    }
  }
})
