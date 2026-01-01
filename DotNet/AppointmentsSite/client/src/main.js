import Vue from 'vue'
import App from './App.vue'
import router from './router'
import store from './store'

Vue.config.productionTip = false


import BootstrapVue from 'bootstrap-vue'

import 'bootstrap/dist/css/bootstrap.css'
import 'bootstrap-vue/dist/bootstrap-vue.css'

Vue.use(BootstrapVue)


import api from './api/api.js'

api.isLoggedIn()
.then(response => {
  store.commit('setLoggedIn', response)

  router.beforeEach((to, from, next) => {
    if (to.matched.some(record => record.meta.requiresAuth)) {     
      // this route requires auth, check if logged in
      // if not, redirect to login page.
      if (!store.state.loggedIn) {
        next({
          path: '/LoginForm', query: { redirect: to.fullPath }
        })
      } else {
        next()
      }
    } else if (to.matched.some(record => record.meta.requiresVisitor)) {
      // this route is only available to a visitor which means they should not be loggied in
      // if logged in, redirect to home page.
      if (store.state.loggedIn) {
        next({
          path: '/',
        })
      } else {
        next()
      }
    } else {
      next()
    }
  })

  new Vue({
    store,
    router,
    render: h => h(App)
  }).$mount('#app')  
})

