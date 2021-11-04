<template>
  <div id="app">
    <b-nav tabs align="center">
      <b-nav-item to="/">Домой</b-nav-item>
      <b-nav-item to="/SetAppointment">Записаться на прием</b-nav-item>
      <template v-if="!loggedIn">
        <b-nav-item to="/LoginForm">Войти</b-nav-item>
        <!-- <b-nav-item to="/RegisterAccount" class="ml-1">Зарегистрироваться</b-nav-item> -->
      </template>
      <template v-else>
        <b-button @click="logoutHandler" variant="info">Выйти</b-button>
      </template>     
    </b-nav>

    <b-nav tabs align="center">
      <template v-if="loggedIn">
        <b-nav-item to="/ManageAppointments">Управление записями</b-nav-item>
        <b-nav-item to="/ManageFreeTime">Управление свободным временем</b-nav-item>
        <b-nav-item to="/CalendarPage">Календарь</b-nav-item>
      </template>
    </b-nav>

    <router-view />
  </div>
</template>

<script>
import api from './api/api.js'

import {mapState, mapMutations} from 'vuex'

export default {
  name: 'App',

  computed: {
    ...mapState([
      'loggedIn'
    ]),
  },

  methods: {
    ...mapMutations([
      'setLoggedIn'
    ]),

    logoutHandler() {
      api.logout()
      .then(response => {
        this.setLoggedIn(false)
        this.$router.push({path: '/'})
      })
    }
  }
}
</script>

<style lang="scss" src="./css/all.scss">
</style>
