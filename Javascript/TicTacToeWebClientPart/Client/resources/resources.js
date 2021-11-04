export default {
  getCellIcons() {
    let cellIcons = {}

    cellIcons.clearImg = new Image()
    cellIcons.clearImg.src = require('../resources/images/clear.png')

    cellIcons.crossImg = new Image()
    cellIcons.crossImg.src = require('../resources/images/cross.png')

    cellIcons.noughtImg = new Image()
    cellIcons.noughtImg.src = require('../resources/images/nought.png')

    return cellIcons
  }
}