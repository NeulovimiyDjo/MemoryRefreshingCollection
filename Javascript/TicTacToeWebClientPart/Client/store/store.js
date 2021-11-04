import Vue from 'vue'
import Vuex from 'vuex'
import gameEntity from './modules/gameEntity.js'
import invites from './modules/invites.js'

Vue.use(Vuex)

const debug = process.env.NODE_ENV !== 'production'

export default new Vuex.Store({
  strict: debug,
  modules: {
    gameEntity,
    invites
  }
})