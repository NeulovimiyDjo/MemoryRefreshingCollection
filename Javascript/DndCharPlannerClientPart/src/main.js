import Vue from 'vue'
import App from './components/App.vue'
import store from './store/store.js'
import router from './router'

Vue.config.productionTip = false

new Vue({
  store,
  router,
  render: h => h(App)
}).$mount('#app')
