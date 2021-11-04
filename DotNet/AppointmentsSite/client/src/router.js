import Vue from 'vue'
import Router from 'vue-router'
import Home from './views/Home.vue'

Vue.use(Router)

export default new Router({
  mode: 'history',
  base: process.env.BASE_URL,
  routes: [
    {
      path: '/',
      name: 'home',
      component: Home
    },
    {
      path: '/ManageAppointments',
      name: 'ManageAppointments',
      component: () => import('./components/ManageAppointments.vue'),
      meta: {
        requiresAuth: true
      }
    },
    {
      path: '/ManageFreeTime',
      name: 'ManageFreeTime',
      component: () => import('./components/ManageFreeTime.vue'),
      meta: {
        requiresAuth: true
      }
    },
    {
      path: '/EditPage/:id',
      name: 'EditPage',
      component: () => import('./components/EditPage.vue'),
      meta: {
        requiresAuth: true
      }
    },
    {
      path: '/FreeTimePage/:id?',
      name: 'FreeTimePage',
      component: () => import('./components/FreeTimePage.vue'),
      meta: {
        requiresAuth: true
      }
    },
    {
      path: '/CalendarPage',
      name: 'CalendarPage',
      component: () => import('./components/CalendarPage.vue'),
      meta: {
        requiresAuth: true
      }
    },
    {
      path: '/SetAppointment',
      name: 'SetAppointment',
      component: () => import('./components/SetAppointment.vue')
    },
    {
      path: '/AppointmentRegistered',
      name: 'AppointmentRegistered',
      component: () => import('./views/AppointmentRegistered.vue')
    },
    {
      path: '/RegisterAccount',
      name: 'RegisterAccount',
      component: () => import('./components/RegisterAccount.vue'),
      meta: {
        requiresVisitor: true
      }
    },
    {
      path: '/LoginForm',
      name: 'LoginForm',
      component: () => import('./components/LoginForm.vue'),
      meta: {
        requiresVisitor: true
      }
    }
  ]
})
