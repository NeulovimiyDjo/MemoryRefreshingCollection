<template>
  <li @mouseenter="enterHandler" @mouseleave="leaveHandler" @click.stop="$emit('item-clicked', realItem)" class="item-slot">
    {{levelText}}{{item.name}}

    <stat-chooser :item="item" @item-changed="itemChangedHandler" />

    <div :style="{'position': 'absolute', 'top': posY + 'px'}">
      <item-tooltip v-if="mouseOver" :item="item" :moduleType="moduleType"
        :posX="posX" @enter-child="leaveHandler" :popups="popups" class="item-tooltip" />
    </div>
  </li>
</template>

<script>
import StatChooser from './StatChooser.vue'

import itemSlot from '../../mixins/itemSlot.js'

export default {
  name: 'AvailableItem',
  mixins: [itemSlot],
  components: {
    'stat-chooser': StatChooser
  },

  data() {
    return {
      realItem: this.item
    }
  },

  methods: {
    itemChangedHandler(item) {
      this.realItem = item
    }
  }
}
</script>

<style>
</style>
