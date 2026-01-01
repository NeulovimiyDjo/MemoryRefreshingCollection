function loadGame() {
  var comp = Qt.createComponent("qrc:/qml/MyImage.qml")
  for (var i=0; i<9; ++i)
    comp.createObject(grd)
}

function resetGame() {
  for (var i in grd.children) {
    grd.children[i].source = "qrc:/img/clear.png"
    grd.children[i].status = "clear"
    grd.children[i].opacity = 1
  }
  count = 0
  game_over = false
  lbl.text = "Moves made: 0"
}

function handleClick(image) {
  if (!game_over && image.status === "clear") {
    if (count%2 == 0) {
      image.source = "qrc:/img/cross.png"
      image.status = "cross"
    } else if (count%2 == 1) {
      image.source = "qrc:/img/nought.png"
      image.status = "nought"
    }
    ++count
    checkGame()
  }
}

function checkGame() {
  var i; var j; var k; var start; var found_chain

  // check rows
  for (i = 0; i < 3; ++i) {
    start = grd.children[i*3].status
    if (start !== "clear") {
      found_chain = true
      for (k = 1; k < 3; ++k) {
        if (grd.children[i*3+k].status !== start)
          found_chain = false
      }
      if (found_chain) {
        finishGame(true)
        return
      }
    }
  }

  // check columns
  for (j = 0; j < 3; ++j) {
    start = grd.children[j].status
    if (start !== "clear") {
      found_chain = true
      for (k = 1; k < 3; ++k) {
        if (grd.children[j+k*3].status !== start)
          found_chain = false
      }
      if (found_chain) {
        finishGame(true)
        return
      }
    }
  }

  // check dioganal right+down
  start = grd.children[0].status
  if (start !== "clear") {
    found_chain = true
    for (k = 1; k < 3; ++k) {
      if (grd.children[k*3+k].status !== start)
        found_chain = false
    }
    if (found_chain) {
      finishGame(true)
      return
    }
  }

  // check dioganal left+down
  start = grd.children[2].status
  if (start !== "clear") {
    found_chain = true
    for (k = 1; k < 3; ++k) {
      if (grd.children[2+k*3-k].status !== start)
        found_chain = false
    }
    if (found_chain) {
      finishGame(true)
      return
    }
  }

  if (count == 9) {
    finishGame(false)
    return
  }

  lbl.text = "Moves made: " + count
}

function finishGame(found_chain) {
  if (found_chain && count%2 == 1)
    lbl.text = "Moves made: " + count + "\nCrosses Won"
  else if (found_chain && count%2 == 0)
    lbl.text = "Moves made: " + count + "\nNoughts Won"
  else
    lbl.text = "Moves made: " + count + "\nDraw"
  game_over = true
  window.gameOver()
}

function handleGameOver() {
  for (var i in grd.children) {
    grd.children[i].opacity = 0.5
  }
}
