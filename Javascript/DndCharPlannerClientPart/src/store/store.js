import Vue from 'vue'
import Vuex from 'vuex'
import character from './modules/character.js'
import database from './modules/database.js'

Vue.use(Vuex)

const debug= process.env.NODE_ENV !=='production'

export default new Vuex.Store({
  strict: debug,
  modules: {
    character,
    database
  }
})
