import 'whatwg-fetch'

const debug = process.env.NODE_ENV !== 'production'
const apiUrl = debug ? 'http://localhost:5000/api/planner' : '/api/planner'

function jsonResponsePromise(url, options) {
  return new Promise(resolve => {
    fetch(url, options)
    .then(response => {
      if (response.ok)
        return response.json()
      else
        throw new Error(response.status + ' ' + response.statusText)
    })
    .catch(error => {
      throw new Error(error.message)
    })
    .then(json => {
      resolve(json)
    })
    .catch(error => {
      console.log('Error: ' + error.message)
    })
  })  
}



export default {
  getDndDatabase() {
    return jsonResponsePromise(apiUrl + '/GetDndDatabase')
  },


  saveCharacter(char) {
    let options = {
      method: 'POST',
      headers: {
        'Accept': 'application/json',
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(char)
    }

    return jsonResponsePromise(apiUrl + '/SaveCharacter', options)
  },


  getCharacter(id) {
    return jsonResponsePromise(apiUrl + '/GetCharacter?id=' + id)
  }
}