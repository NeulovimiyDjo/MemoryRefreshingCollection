<template>
<div id="app">
  <h3>{{this.message}}</h3>
  <div id="game-row">
    <div id="game-column">
      <game-manager></game-manager>
    </div>
  </div>
</div>
</template>

<script>
import GameManager from './GameManager.vue'

export default {
  components: {
    'game-manager': GameManager
  },

  data() {
    return {
      message: 'TicTacToe',
    }
  }
}
</script>

<style>
  #app {
    background: burlywood;
    font-family: Verdana;
    color: navy;
    display: flex;
    flex-direction: column;
  }

  #game-row {
    background:burlywood;
    display: flex;
    flex-direction: row;
    flex: 0 0 88vmin;
  }

  #game-column {
    background: burlywood;
    flex: 0 0 66vmin;
  }

  @media (orientation: landscape) {
    #game-row {
      flex: 0 0 80vmin;
    }

    #game-column {
      flex: 0 0 95vmin;
    }
  }

  h3 {
    font-size: 5vmin;
  }

  h5 {
    font-size: 2vmin;
    height: 2em;
  }

  p {
    font-size: 2vmin;
    height: 2em;
    margin: 0;
  }

  li {
    font-size: 2vmin;
  }

  button {
    width: 20vmin;
    font-size: 3vmin;
  }

  input {
    font-size: 3vmin;
  }

  label {
    font-size: 3vmin;
  }

  h3, h5, button, ul, p {
    padding: 0;
    border: 0;
    margin: 0;
  }
</style>
