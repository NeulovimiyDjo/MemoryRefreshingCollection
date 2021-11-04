import QtQuick 2.0
import QtQuick.Controls 1.0
import "qrc:/js/scripts.js" as Scripts

ApplicationWindow {
  id: window
  visible: true
  width: 640; height: 480
  minimumWidth: 160; minimumHeight: 120

  property double x_scale: window.width/640
  property double y_scale: window.height/480

  signal gameOver()
  property int count: 0
  property bool game_over: false
  Component.onCompleted: gameOver.connect(Scripts.handleGameOver)

  Grid {
    id: grd
    x: 25*x_scale; y: 25*y_scale
    columns: 3; rows: 3
    spacing: 10*Math.min(x_scale,y_scale)
    Component.onCompleted: Scripts.loadGame()
  }

  Label {
    id: lbl
    anchors.left: grd.right; anchors.top: parent.top
    anchors.leftMargin: 50*x_scale
    anchors.topMargin: 50*y_scale
    text: "Moves made: 0"
    font.pixelSize: 28*Math.min(x_scale,y_scale)
  }

  Button {
    anchors.left: grd.right; anchors.top: parent.top
    anchors.leftMargin: 100*x_scale
    anchors.topMargin: 150*y_scale
    width: 100*x_scale; height: 40*y_scale
    Text {
      anchors.centerIn: parent
      text: "reset"
      font.pixelSize: 28*Math.min(x_scale,y_scale)
    }

    //text: "reSet"
    MouseArea {
      anchors.fill: parent
      onClicked: Scripts.resetGame()
    }
  }
}
