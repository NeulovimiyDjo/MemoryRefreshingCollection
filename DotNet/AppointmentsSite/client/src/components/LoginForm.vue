<template>
  <div>
    <h2>Sign in page</h2>
    <b-button @click="loginHandler">Войти</b-button>
  </div>
</template>

<script>
import api from '../api/api.js'

import {mapMutations} from 'vuex'

export default {
  name: 'LoginForm',

  data() {
    return {
      stub:''
    }
  },


  methods: {
    ...mapMutations([
      'setLoggedIn'
    ]),

    loginHandler() {
      let info = {
        userName: 'userName2',
        password: 'Password123!',
        remember: false
      }

      api.login(info)
      .then(response => {
        if (response === 'Success') {
          this.setLoggedIn(true)
          this.$router.push(this.$route.query.redirect || '/')
        } else if (response === 'Fail') {
          alert('Invalid login information')
        }     
      })
    }
  }
}
</script>
