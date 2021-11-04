
function getStyle(selector) {
  let styles = document.styleSheets[0].cssRules

  for (let i = 0; i < styles.length; i++) {
    if (styles[i].selectorText === selector) {
      return styles[i].cssText
    }
  }
}

function getStyleAttribute(selector, attribute) {
  let style = getStyle(selector)

  let attributes = style.split(';')
  attributes[0] = attributes[0].split('{')[1]
  attributes.splice(-1, 1)

  let result
  attributes.forEach(a => {
    let s = a.split(':')

    if (s[0].trim() === attribute) result = s[1].trim()
  })

  return result
}

let top = getStyleAttribute('.available-spells-list', 'top')
//let top = parseFloat(getComputedStyle(this.$refs['available-spells'].$el).getPropertyValue('top'))
console.log(top)