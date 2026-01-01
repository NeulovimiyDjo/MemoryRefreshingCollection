import QtQuick 2.0
import "qrc:/js/scripts.js" as Scripts

Image {
  source: "qrc:/img/clear.png"
  property string status: "clear"
  MouseArea {
    anchors.fill: parent
    onClicked: Scripts.handleClick(parent)
  }
  width: 100*x_scale
  height: 100*y_scale
}

