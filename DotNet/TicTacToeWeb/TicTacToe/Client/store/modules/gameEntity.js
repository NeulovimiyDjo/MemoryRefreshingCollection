function _newGame(state, isMyTurn) {
  if (isMyTurn)
    state.mySide = 'crosses'
  else
    state.mySide = 'noughts'

  state.isMyTurn = isMyTurn
  state.gameOver = false
  state.endReason = ''

  state.movesCount = 0
  state.winner = ''

  state.winChainIndexes = []
  state.cells = Array(state.xDim*state.yDim).fill().map(i => 'clear')
}

function setCellNewType(state, index) {
  if (state.movesCount%2 == 0) {
    state.cells[index] = 'cross'
  } else if (state.movesCount%2 == 1) {
    state.cells[index] = 'nought'
  }
  state.movesCount++
}

function _finishGame(state, conditions) {
  state.gameOver = true
  state.winner = conditions.winner
  state.endReason = conditions.reason

  setWinChainIndexes(state, conditions)
}

// winSize cells in one of 4 directions starting from cell (i,j)
function setWinChainIndexes(state, {direction, i, j}) {
  switch(direction) {
    case 'right':
      for (let k = 0; k < state.winSize; k++) 
        state.winChainIndexes.push(i*state.xDim + (j + k))
      break
    case 'down':
      for (let k = 0; k < state.winSize; k++) 
        state.winChainIndexes.push((i+k)*state.xDim + j)
      break
    case 'right+down':
      for (let k = 0; k < state.winSize; k++) 
        state.winChainIndexes.push((i+k)*state.xDim + (j + k))
      break
    case 'left+down':
      for (let k = 0; k < state.winSize; k++) 
        state.winChainIndexes.push((i+k)*state.xDim + (j - k))
      break
  }
}

export const dimLimits = {
  minXDim: 3,
  minYDim: 3,
  minWinSize: 2,
  maxXDim: 100,
  maxYDim: 100,
  maxWinSize: 30
}

export default {
  namespaced: true,
  
  state: {
    gameOver: false,
    isMyTurn: false,
    
    mySide: '',
    movesCount: 0,
    winner: '',
    endReason: '',

    xDim: 10,
    yDim: 10,
    winSize: 4,
    timeout: 100,

    timeLeft: 0,
    startTime: 0,

    winChainIndexes: [],
    cells: []
  },

  mutations: {
    newGame(state, {settings, isMyTurn}) {
      state.xDim = settings.xDim
      state.yDim = settings.yDim
      state.winSize = settings.winSize
      state.timeout = settings.timeout
      
      _newGame(state, isMyTurn)
      state.startTime = Date.now()
    },

    makeMove(state, index) {
      setCellNewType(state, index)

      state.isMyTurn = !state.isMyTurn

      state.startTime = Date.now()
    },

    finishGame(state, conditions) {
      _finishGame(state, conditions)
    },

    calculateTimeLeft(state){
      state.timeLeft = state.timeout - Math.round((Date.now() - state.startTime)/1000)
    }
  },

  actions: {
    newGame({commit}, {settings, isMyTurn}) {
      commit('newGame', {settings, isMyTurn})

      commit('calculateTimeLeft')
      timerId = setInterval(()=>{
        commit('calculateTimeLeft')
      }, 100)
    },

    finishGame({commit}, conditions) {
      commit('finishGame', conditions)

      clearInterval(timerId)
      commit('calculateTimeLeft')
    }
  }
}

let timerId