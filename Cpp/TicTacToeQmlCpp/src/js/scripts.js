function loadGame() {
  var comp = Qt.createComponent("qrc:/qml/MyImage.qml")

  for (var k=0; k<m*n; ++k)
    comp.createObject(game_grid, {i:k/n, j:k%n})

//  for (k = 0; k<m*n; ++k)
//    game_grid.children[k].children[0].source = Qt.binding(function() {
//      var tmp = k
//      return function() {
//        return game_manager.managers[tmp].source
//      }
//    }())
}

function resetGame() {
  game_manager.resetGame()
}

function changeSize() {
  var m_tmp = parseInt(row_count.text)
  var n_tmp = parseInt(column_count.text)
  var chain = parseInt(chain_size.text)
  var valid_size = true

  if (isNaN(m_tmp) || isNaN(n_tmp)  || isNaN(chain) || m_tmp<chain || n_tmp<chain || chain<2)
    valid_size = false

  if (valid_size) {
    for (var k = 0; k<game_grid.children.length; ++k) {
      // break bindings first since destory() doesnt destroy objects immediately
      game_grid.children[k].children[0].source = ""
      game_grid.children[k].children[0].isInVictoryChain = false
      game_grid.children[k].children[0].isGameOver = false
      game_grid.children[k].destroy()
    }

    m = m_tmp
    n = n_tmp

    game_manager.changeSize(m_tmp, n_tmp, chain)

    loadGame()
  } else
    message_dialog.visible = true
}
