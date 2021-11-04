import QtQuick 2.0
import ImageButton 1.0

Rectangle {
  width: Math.min((main_window.width-300*Math.min(x_scale,y_scale))*m,
                  (main_window.height-50*Math.min(x_scale,y_scale))*n)
         /(m*n) - game_grid.spacing
  height: Math.min((main_window.width-300*Math.min(x_scale,y_scale))*m,
                   (main_window.height-50*Math.min(x_scale,y_scale))*n)
          /(m*n) - game_grid.spacing

  property int i
  property int j

  ImageButton {
    anchors.fill: parent
    onClicked: game_manager.makeMove(i, j)
    source: game_manager.managers[i*n+j].source
    isInVictoryChain: game_manager.managers[i*n+j].isInVictoryChain
    isGameOver: game_manager.isGameOver
  }

//  MouseArea {
//    anchors.fill: parent
//    onPressed: {game_manager.makeMove(i, j); mouse.accepted = false}
//  }
}

