<template>
<div>
  <h5 class="margin-bottom" id="header">{{header}}</h5>

  <p>Preferred Settings:</p>
  <div id="game-settings">
    <div>
      <label>x-dim:</label>
      <input v-model.trim.number="xDim" type="number"
            :min="limits.minXDim" :max="limits.maxXDim" required>
    </div>
    <div>
      <label>y-dim:</label>
      <input v-model.trim.number="yDim" type="number"
            :min="limits.minYDim" :max="limits.maxYDim" required/>
    </div>
    <div>
      <label>win-size:</label>
      <input v-model.trim.number="winSize" type="number"
            :min="limits.minWinSize" :max="limits.maxWinSize" required/>
    </div>
    <button id="apply-btn" class="game-btn" @click="applyButtonHandler">Apply</button>
  </div>
  <div id="curr-settings">
    <p class="margin-bottom">My Current Preferred Settings: </p>
    <p>{{$store.state.invites.myPreferredSettings.xDim}} y:{{$store.state.invites.myPreferredSettings.yDim}} w:{{$store.state.invites.myPreferredSettings.winSize}}</p>
  </div>

</div>
</template>

<script>
import {dimLimits} from '../store/modules/gameEntity.js'

function settingsAreValid({xDim, yDim, winSize}) {
  if (typeof(xDim) !== 'number' || typeof(yDim) !== 'number' || typeof(winSize) !== 'number'||
      xDim < dimLimits.minXDim || yDim < dimLimits.minYDim || winSize < dimLimits.minWinSize ||
      xDim > dimLimits.maxXDim || yDim > dimLimits.maxYDim || winSize > dimLimits.maxWinSize ||
      winSize > Math.min(xDim,yDim)
      )
    return false
  else
    return true
}

function preferredSettingsChanged(state, {xDim, yDim, winSize}) {
  if (xDim === state.myPreferredSettings.xDim && yDim === state.myPreferredSettings.yDim &&
      winSize === state.myPreferredSettings.winSize)
    return false
  else
    return true
}

export default {
  data() {
    return {
      xDim: this.$store.state.invites.myPreferredSettings.xDim,
      yDim: this.$store.state.invites.myPreferredSettings.yDim,
      winSize: this.$store.state.invites.myPreferredSettings.winSize,

      limits: {
        minXDim: dimLimits.minXDim,
        minYDim: dimLimits.minYDim,
        minWinSize: dimLimits.minWinSize,

        maxXDim: dimLimits.maxXDim,
        maxYDim: dimLimits.maxYDim,
        maxWinSize: dimLimits.maxWinSize
      },

      header: 'Search games.'
    }
  },

  watch: {
    xDim(newVal, oldVal) {
      if (typeof(newVal) !== 'number') {
        this.xDim = oldVal-1 // force input update
        this.xDim = oldVal
      }
    },

    yDim(newVal, oldVal) {
      if (typeof(newVal) !== 'number') {
        this.yDim = oldVal-1 // force input update
        this.yDim = oldVal
      }
    },

    winSize(newVal, oldVal) {
      if (typeof(newVal) !== 'number') {
        this.winSize = oldVal-1 // force input update
        this.winSize = oldVal
      }
    }
  },

  methods: {
    applyButtonHandler() {
      let settings = {xDim: this.xDim, yDim: this.yDim, winSize: this.winSize, timeout: this.$store.state.gameEntity.timeout}

      if (!settingsAreValid(settings) || !preferredSettingsChanged(this.$store.state.invites, settings))
        return

      this.$emit('sizes-click', settings)
    }
  }
}
</script>

<style>
  .game-btn {
    background: darkolivegreen;
    outline: none;
    align-self: center;
  }

  .game-btn:active {
    background: green;
  }

  #game-settings {
    display: flex;
    flex-direction: row;
  }

  #game-settings > div {
    display: flex;
    flex-direction: row;
    justify-content: space-between;
    margin: 0 0 5% 0;
  }

  input {
    box-sizing: border-box;
    width: 75%;
    margin: 0 5% 0 0;
    border-color: green;
  }

  input:invalid {
    border-color: red;
  }

  #apply-btn {
    margin: 0 0 5% 0;
  }

  #curr-settings {
    display: flex;
    flex-direction: row;
  }

  .margin-bottom {
    margin: 0 0 5% 0;
  }

  @media (orientation: landscape) {
    #game-settings {
      flex-direction: column;
    }

    #header {
      padding: 0 0 0 10%;
    }

    label {
      padding: 0 0 0 10%;
    }

    input {
      width: 25%;
      margin: 0 10% 0 0;
    }

    #apply-btn {
      margin: 10% 0 10% 0;
    }

    #curr-settings {
      flex-direction: column;
    }

    .margin-bottom {
      margin: 0 0 15% 0;
    }
  }
</style>
