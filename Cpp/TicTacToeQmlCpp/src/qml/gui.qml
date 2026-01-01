import QtQuick 2.0
import QtQuick.Controls 1.0
import QtQuick.Dialogs 1.1
import "qrc:/js/scripts.js" as Scripts
import GameManager 1.0
import SquareManager 1.0

ApplicationWindow {
  id: main_window
  visible: true
  width: 720; height: 480
  minimumWidth: 180; minimumHeight: 120

  property double x_scale: main_window.width/720
  property double y_scale: main_window.height/480

  property int m: 3
  property int n: 3

  GameManager {
    id: game_manager
  }

  Grid {
    id: game_grid
    x: 25*Math.min(x_scale,y_scale); y: 25*Math.min(x_scale,y_scale)
    rows: m; columns: n
    spacing: 24*Math.min(x_scale,y_scale)/Math.max(m,n)
  }

  Component.onCompleted: Scripts.loadGame()

  Label {
    id: game_info
    anchors.left: game_grid.right; anchors.top: parent.top
    anchors.leftMargin: 50*Math.min(x_scale,y_scale)
    anchors.topMargin: 40*Math.min(x_scale,y_scale)
    text: game_manager.gameInfo
    font.pixelSize: 28*Math.min(x_scale, y_scale)
  }

  Button {
    anchors.left: game_grid.right; anchors.top: parent.top
    anchors.leftMargin: 100*Math.min(x_scale,y_scale)
    anchors.topMargin: 120*Math.min(x_scale,y_scale)
    width: 100*Math.min(x_scale,y_scale); height: 40*Math.min(x_scale,y_scale)
    Text {
      anchors.centerIn: parent
      text: "reset"
      font.pixelSize: 28*Math.min(x_scale, y_scale)
    }

    MouseArea {
      anchors.fill: parent
      onClicked: Scripts.resetGame()
    }
  }

  Label {
    anchors.left: game_grid.right; anchors.top: parent.top
    anchors.leftMargin: 50*Math.min(x_scale,y_scale)
    anchors.topMargin: 230*Math.min(x_scale,y_scale)
    width: 40*Math.min(x_scale,y_scale); height: 30*Math.min(x_scale,y_scale)
    text: "Rows: "
    font.pixelSize: 28*Math.min(x_scale, y_scale)
  }

  TextField {
    id: row_count
    anchors.left: game_grid.right; anchors.top: parent.top
    anchors.leftMargin: 180*Math.min(x_scale,y_scale)
    anchors.topMargin: 235*Math.min(x_scale,y_scale)
    width: 60*Math.min(x_scale,y_scale); height: 30*Math.min(x_scale,y_scale)
    text: "3"
    font.pixelSize: 22*Math.min(x_scale, y_scale)
  }

  Label {
    anchors.left: game_grid.right; anchors.top: parent.top
    anchors.leftMargin: 50*Math.min(x_scale,y_scale)
    anchors.topMargin: 270*Math.min(x_scale,y_scale)
    width: 40*Math.min(x_scale,y_scale); height: 30*Math.min(x_scale,y_scale)
    text: "Columns: "
    font.pixelSize: 28*Math.min(x_scale, y_scale)
  }

  TextField {
    id: column_count
    anchors.left: game_grid.right; anchors.top: parent.top
    anchors.leftMargin: 180*Math.min(x_scale,y_scale)
    anchors.topMargin: 275*Math.min(x_scale,y_scale)
    width: 60*Math.min(x_scale,y_scale); height: 30*Math.min(x_scale,y_scale)
    text: "3"
    font.pixelSize: 22*Math.min(x_scale, y_scale)
  }

  Label {
    anchors.left: game_grid.right; anchors.top: parent.top
    anchors.leftMargin: 50*Math.min(x_scale,y_scale)
    anchors.topMargin: 315*Math.min(x_scale,y_scale)
    width: 40*Math.min(x_scale,y_scale); height: 30*Math.min(x_scale,y_scale)
    text: "Victory chain: "
    font.pixelSize: 20*Math.min(x_scale, y_scale)
  }

  TextField {
    id: chain_size
    anchors.left: game_grid.right; anchors.top: parent.top
    anchors.leftMargin: 180*Math.min(x_scale,y_scale)
    anchors.topMargin: 315*Math.min(x_scale,y_scale)
    width: 60*Math.min(x_scale,y_scale); height: 30*Math.min(x_scale,y_scale)
    text: "3"
    font.pixelSize: 22*Math.min(x_scale, y_scale)
  }

  Button {
    anchors.left: game_grid.right; anchors.top: parent.top
    anchors.leftMargin: 70*Math.min(x_scale,y_scale)
    anchors.topMargin: 360*Math.min(x_scale,y_scale)
    width: 160*Math.min(x_scale,y_scale); height: 40*Math.min(x_scale,y_scale)
    Text {
      anchors.centerIn: parent
      text: "change size"
      font.pixelSize: 28*Math.min(x_scale, y_scale)
    }

    MouseArea {
      anchors.fill: parent
      onClicked: Scripts.changeSize()
    }
  }

  MessageDialog {
      id: message_dialog
      text: "  Wrong game size!           "
      onAccepted: visible = false
  }
}
