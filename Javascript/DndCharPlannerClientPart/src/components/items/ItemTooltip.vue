<template>
  <div @mouseenter="$emit('enter-child')">
    <span v-html="text" />
  </div>
</template>

<script>
export default {
  name: 'ItemTooltip',

  props: {
    moduleType: String,
    item: Object,
    posX: Number,
    popups: String
  },

  mounted() {
    if (this.text.length > 1050) {
      let max = 63
      if (this.popups === 'mid') max = 45
 
      this.$el.style.width = Math.min(this.text.length / 30, max) + 'vmin'
      this.$el.style.left = - 1.5 - Math.min(this.text.length / 30, max) + 'vmin'
    }

    if (this.posX > 0) { // tooltip on right
      let left = this.$el.getBoundingClientRect().left - this.$parent.$el.getBoundingClientRect().left
      let width = this.$el.getBoundingClientRect().width
      this.$el.style.left = left + this.posX + width + 'px'
    }  
  },

  computed: {
    text() {
      let res = this.item.name + '</br>'
      if (this.moduleType === 'cantrips') {
        res = 'Name: ' + res + 'Cantrip</br>'
      } else if (this.moduleType === 'spells' || this.moduleType === 'options' && this.item.level) {
        res = 'Name: ' + res + 'Level: ' + this.item.level + '</br>'
      }

      if (this.item.classes) {
        res = res + 'Classes: '
        for (let i = 0; i < this.item.classes.length; i++) {
          res = res + this.item.classes[i]
          if (i !== this.item.classes.length - 1) {
            res = res + ', '
          } else {
            res = res + '<br/>'
          }
        }
      }

      if (this.item.school) res = res + 'School: ' + this.item.school
      if (this.item.source) res = res + '</br>Source: ' + this.item.source + '<br/>'

      if (this.item.time) res = res + '</br>Time: ' + this.item.time
      if (this.item.range) res = res + '</br>Range: ' + this.item.range
      if (this.item.components) res = res + '</br>Components: ' + this.item.components
      if (this.item.duration) res = res + '</br>Duration: ' + this.item.duration + '</br>'


      if (this.item.requirement) {
        res = res + '</br>Prerequisite: ' + this.item.requirement + '</br>'
      }

      if (this.item.description) {
        let fontSize = Math.max(Math.min(3000 / this.item.description.length, 1.4), 1.2)
        res = res + '</br>Description:<pre style="font-size: ' + fontSize + 'vmin;">' + this.item.description + '</pre>'
      }

      if (this.item.bonusStats) {
        if (Object.keys(this.item.bonusStats).length > 0) res = res + '</br>Stats:'
        Object.keys(this.item.bonusStats).forEach(statName => {
          let sign = ''
          if (this.item.bonusStats[statName] > 0) sign = '+'

          res = res + '</br>' + sign + this.item.bonusStats[statName] + ' ' + statName
        })
      }

      return res
    }
  }
}
</script>

<style>
</style>
