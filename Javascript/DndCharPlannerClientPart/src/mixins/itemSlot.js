import ItemTooltip from '../components/items/ItemTooltip.vue'

export default {
  components: {
    'item-tooltip': ItemTooltip
  },

  props: {
    moduleType: String,
    popups: String,
    item: Object
  },

  data() {
    return {
      mouseOver: false,
      posX: 0,
      posY: 0,
    }
  },

  computed: {
    levelText() {
      if (this.item.level && (this.moduleType === 'spells' || this.moduleType === 'options'))
        return this.item.level + ':'
      else
        return ''
    }
  },

  methods: {
    enterHandler() {
      this.mouseOver = true
      this.posY = this.$el.getBoundingClientRect().top - this.$parent.$el.getBoundingClientRect().top
      
      if (this.popups === 'right') {
        this.posX = this.$el.getBoundingClientRect().width
      } else {
        this.posX = 0
      }
    },

    leaveHandler() {
      this.mouseOver = false
    }
  }
}