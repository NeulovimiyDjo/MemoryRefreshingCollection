<template>
  <li @mouseenter="enterHandler" @mouseleave="leaveHandler" @click.stop="$emit('slot-clicked', {slotId: slotId, posY: posY})"
    class="item-slot" :class="{'selected-item': currentlyClickedSlotId === slotId}">
    {{levelText}}{{item.name}}

    <div :style="{'position': 'absolute', 'top': posY + 'px'}">
      <item-tooltip v-if="mouseOver && item.id !== -1" :item="item" :moduleType="moduleType"
        :posX="2.1*posX" @enter-child="leaveHandler" :popups="popups" class="item-tooltip" />
    </div>
  </li>
</template>

<script>
import itemSlot from '../../mixins/itemSlot.js'

export default {
  name: 'ChoosableItem',
  mixins: [itemSlot],

  props: {
    slotId: Number,
    currentlyClickedSlotId: Number
  }
}
</script>

<style>
</style>
