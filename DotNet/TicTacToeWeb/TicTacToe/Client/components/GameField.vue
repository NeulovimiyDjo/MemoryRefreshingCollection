<template>
<div>
  <canvas ref="canvas"
          @mousedown.prevent="mouseDownHandler($event)"
          @mouseup="mouseUpHandler($event)"
          @mouseleave="mouseLeaveHandler()"
          @touchstart.prevent="touchStartHandler($event)"
          @touchend="touchEndHandler($event)">
  </canvas>
</div>
</template>

<script>
function getCellIndex(event, context, state) {
  let {xDim,yDim,cellBorderWidth,imgSize,imgStep} = getSizeAttributes(context, state)

  // get mouse coordinates relative to canvas
  let rect = context.canvas.getBoundingClientRect()
  let x = event.clientX - rect.left
  let y = event.clientY - rect.top

  let i = Math.floor(y / imgStep)
  let j = Math.floor(x / imgStep)

  if (i > yDim-1 || j > xDim-1)
    return -1

  // if coords got between the cells
  if ((imgStep - x%imgStep) < cellBorderWidth ||
      (imgStep - y%imgStep) < cellBorderWidth)
    return -1

  return i*xDim + j
}

function getSizeAttributes(context,state) {
  let xDim = state.xDim
  let yDim = state.yDim
  let maxDim = Math.max(xDim,yDim)

  let cellBorderWidth = Math.floor(context.canvas.width / 12 / maxDim)
  if (cellBorderWidth === 0) {
    cellBorderWidth = 1
  }

  // (width - ((2 borders for each cell) + (1 space between them - 1))) / maxDim
  let imgSize = Math.floor((context.canvas.width - (maxDim*2 + maxDim-1)*cellBorderWidth) / maxDim)
  let imgStep = (imgSize + 3*cellBorderWidth)

  return {xDim: xDim, yDim: yDim, cellBorderWidth: cellBorderWidth, imgSize: imgSize, imgStep: imgStep}
}

function getCellBorderStyle(look) {
  switch(look) {
    case 'pressed':
      return '#08F'
    case 'disabled':
      return '#444'
    case 'chain-member':
      return '#080'
    case 'normal':
      return '#048'
    default:
      throw "Invalid cellLook"
  }
}

function getCellImg(type, cellIcons) {
  switch(type) {
    case 'clear':
      return cellIcons.clearImg
    case 'cross':
      return cellIcons.crossImg
    case 'nought':
      return cellIcons.noughtImg
    default:
      throw "Invalid cellType"
  }
}

function setCanvasSizes(canvas) {
  canvas.width = canvas.parentElement.clientWidth
  canvas.height = canvas.parentElement.clientHeight
}

export default {
  props: {
    cellIcons: {
      type: Object,
      default: null
    }
  },

  data() {
    return {
      context: null,

      unsubscribeMutations: null,
      
      lastPressedCellIndex: -1,
      lastTouch: null
    }
  },

  computed: {
    gameState() {
      return this.$store.state.gameEntity
    },

    gameOver() {
      return this.gameState.gameOver
    }
  },

  watch: {
    gameOver() {
      this.drawAllCells()
    }
  },

  created() {
    window.addEventListener("resize", this.resizeHandler)

    this.unsubscribeMutations = this.$store.subscribe((mutation,state) => {
      switch(mutation.type) {
        case 'gameEntity/makeMove':
          this.drawCell(mutation.payload, 'normal') // payload is cellIndex
          break
        case 'gameEntity/newGame':
        case 'gameEntity/changeSizes':
          this.drawAllCells()
          break
      }
    })
  },
  
  destroyed() {
    this.unsubscribeMutations()

    window.removeEventListener("resize", this.resizeHandler)
  },

  mounted() {
    setCanvasSizes(this.$refs['canvas'])
    this.context = this.$refs['canvas'].getContext('2d')

    this.drawAllCells()
  },

  methods: { 
    resizeHandler() {
      setCanvasSizes(this.$refs['canvas'])
      this.drawAllCells()
    },

    mouseDownHandler(event) {
      if (!this.gameState.isMyTurn) return

      let index = getCellIndex(event, this.context, this.gameState)
      this.cellMouseDown(index)
    },

    mouseUpHandler(event) {
      if (!this.gameState.isMyTurn) return

      let index = getCellIndex(event, this.context, this.gameState)
      this.cellMouseUp(index)
    },

    mouseLeaveHandler() {
      if (!this.gameState.isMyTurn) return

      this.cellMouseUp(-1)
    },

    touchStartHandler(e) {
      if (!this.gameState.isMyTurn) return

      // deactivate prevoius mouse-down if more than one touch happens simultaneously
      if (e.touches.length > 1) {
        this.cellMouseUp(-1)
      }

      // handle new one
      this.cellMouseDown(getCellIndex(e.touches[e.touches.length-1], this.context, this.gameState))
      this.lastTouch = e.touches[e.touches.length-1]
    },

    touchEndHandler(e) {
      if (!this.gameState.isMyTurn) return

      if (this.lastTouch === null) return

      // check if lastTouch got removed from touches list (meaning that's the event that's handling lastTouch)
      let removed = true
      for (let i = 0; i < e.touches.length; i++) {
        if (e.touches[i].identifier === this.lastTouch.identifier) removed = false
      }

      if (removed) {
        this.cellMouseUp(getCellIndex(this.lastTouch, this.context, this.gameState))
        this.lastTouch = null
      }
    },


    cellMouseDown(index) {
      if (index > -1 && !this.gameState.gameOver && this.gameState.cells[index] === 'clear') {
        // set pressed cell look
        this.lastPressedCellIndex = index
        this.drawCell(index, 'pressed')
      } else {
        this.lastPressedCellIndex = -1
      }
    },

    cellMouseUp(index) {
      if (index > -1 && index === this.lastPressedCellIndex &&
          !this.gameState.gameOver && this.gameState.cells[index] === 'clear'
      ) {
        // make the actual move
        this.$emit('cell-clicked', index)
      } else if (this.lastPressedCellIndex > -1) {
        // restore normal cell look
        this.drawCell(this.lastPressedCellIndex, 'normal')
      }

      this.lastPressedCellIndex = -1
    },


    drawAllCells() {
      this.context.clearRect(0,0,this.context.canvas.width,this.context.canvas.height)

      let look = ''
      if (this.gameState.gameOver) {
        look = 'disabled'
      } else {
        look = 'normal'
      }

      // draw all cells with 'disabled' or 'normal' look
      for (let index = 0; index < this.gameState.cells.length; index++) {
        this.drawCell(index, look)
      }

      if (this.gameState.gameOver) {
        // redraw win chain cells with 'chain-member' look
        this.gameState.winChainIndexes.forEach( index => {
          this.drawCell(index, 'chain-member')
        })
      }
    },

    drawCell(index, cellLook) {
      if (!this.context) return
      let context = this.context

      let {xDim,yDim,cellBorderWidth,imgSize,imgStep} = getSizeAttributes(context, this.gameState)

      context.strokeStyle = getCellBorderStyle(cellLook)
      context.lineWidth = cellBorderWidth

      let i = Math.floor(index / xDim)
      let j = index % xDim

      // draw cell border
      context.strokeRect(cellBorderWidth/2+imgStep*j, cellBorderWidth/2+imgStep*i,
                        imgSize+cellBorderWidth, imgSize+cellBorderWidth)

      // draw cell icon
      let cellImg = getCellImg(this.gameState.cells[index], this.cellIcons) 
      context.drawImage(cellImg, cellBorderWidth+imgStep*j, cellBorderWidth+imgStep*i, imgSize, imgSize)
    }
  }
}
</script>
