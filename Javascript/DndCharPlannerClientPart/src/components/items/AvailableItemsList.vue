<template>
  <div @mouseenter="$emit('enter-child')">
    <ul class="scrollable-list" ref='scrollable-ul'>
      <li @click="$emit('item-chosen', {slotId: slotId, item: {id: -1}})" class="remove-option">Remove</li>
      <available-item v-for="item in availableSource" :key="item.name" :item="item" :moduleType="moduleType"
        @item-clicked="$emit('item-chosen', {slotId: slotId, item: $event})" class="available-item"
        :popups="popups"/>
    </ul>
  </div> 
</template>

<script>
import AvailableItem from './AvailableItem.vue'

export default {
  name: 'AvailableItemsList',
  components: {
    'available-item': AvailableItem,
  },

  props: {
    moduleType: String,
    popups: String,

    slotId: Number,
    
    availableSource: Array
  }
}
</script>

<style>
</style>
