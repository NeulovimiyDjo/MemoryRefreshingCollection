import 'whatwg-fetch'

import store from '../store'
import router from '../router'


const debug = process.env.NODE_ENV !== 'production'
const apiUrl = debug ? 'http://localhost:5000/api' : '/api'


function getCookie(cname) {
  let name = cname + "="
  let decodedCookie = decodeURIComponent(document.cookie)
  let cookiesArray = decodedCookie.split(';')
  for(let i = 0; i <cookiesArray.length; i++) {
      let c = cookiesArray[i]
      while (c.charAt(0) == ' ') {
          c = c.substring(1)
      }
      if (c.indexOf(name) == 0) {
          return c.substring(name.length, c.length)
      }
  }
  return ""
}

function jsonResponsePromise(url, options) {
  if (options && options.method === 'POST') {
    let xsrfToken = getCookie("XSRF-TOKEN")

    if (!options.headers)
      options.headers = {}

    options.headers['X-CSRF-TOKEN'] = xsrfToken
  }

  return new Promise(resolve => {
    fetch(url, options)
    .then(response => {
      if (response.ok) {
        return response.json()
      }
      else {
        if (response.status === 401) {
          store.commit('setLoggedIn', false)
          router.push({
            path: '/LoginForm', query: { redirect: router.history.current.fullPath }
          })
        } else {
          throw new Error(response.status + ' ' + response.statusText)
        }
      }
    })
    .catch(error => {
      throw new Error(error.message)
    })
    .then(json => {
      resolve(json)
    })
    .catch(error => {
      console.log(error.message)
    })
  })  
}


export default {
  isLoggedIn() {
    let options = {
      credentials: 'include'
    }

    return jsonResponsePromise(apiUrl + '/account/isLoggedIn', options)
  },

  register(info) {
    let options = {
      method: 'POST',
      credentials: 'include',
      headers: {
        'Accept': 'application/json',
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(info)
    }

    return jsonResponsePromise(apiUrl + '/account/register', options)
  },

  login(info) {
    let options = {
      method: 'POST',
      credentials: 'include',
      headers: {
        'Accept': 'application/json',
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(info)
    }

    return jsonResponsePromise(apiUrl + '/account/login', options)
  },

  logout() {
    let options = {
      method: 'POST',
      credentials: 'include'
    }

    return jsonResponsePromise(apiUrl + '/account/logout', options)
  },

  confirmAppointment(info) {
    let options = {
      method: 'POST',
      credentials: 'include',
      headers: {
        'Accept': 'application/json',
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(info)
    }

    return jsonResponsePromise(apiUrl + '/appointments/confirmAppointment', options)
  },

  removeAppointment(info) {
    let options = {
      method: 'POST',
      credentials: 'include',
      headers: {
        'Accept': 'application/json',
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(info)
    }

    return jsonResponsePromise(apiUrl + '/appointments/removeAppointment', options)
  },

  addFreeTime(info) {
    let options = {
      method: 'POST',
      credentials: 'include',
      headers: {
        'Accept': 'application/json',
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(info)
    }

    return jsonResponsePromise(apiUrl + '/appointments/addFreeTime', options)
  },

  removeFreeTime(info) {
    let options = {
      method: 'POST',
      credentials: 'include',
      headers: {
        'Accept': 'application/json',
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(info)
    }

    return jsonResponsePromise(apiUrl + '/appointments/removeFreeTime', options)
  },

  getDb() {
    let options = {
      credentials: 'include'
    }

    return jsonResponsePromise(apiUrl + '/appointments/getDb', options)
  },



  getAvailableDateTimes() {
    return jsonResponsePromise(apiUrl + '/appointments/getAvailableDateTimes')
  },

  setAppointment(info) {
    let options = {
      method: 'POST',
      credentials: 'include',
      headers: {
        'Accept': 'application/json',
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(info)
    }

    return jsonResponsePromise(apiUrl + '/appointments/setAppointment', options)
  }
}