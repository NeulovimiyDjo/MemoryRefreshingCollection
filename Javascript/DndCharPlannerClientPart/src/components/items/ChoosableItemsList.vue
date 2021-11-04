<template>
  <div>
    <slot></slot>
    <ul>
      <choosable-item v-for="(item, index) in choosableSource" :key="index" :slotId="index"
        :item="item" :currentlyClickedSlotId="currentlyClickedSlotId" @slot-clicked="slotClickedHandler"
        :moduleType="moduleType" class="choosable-item" :popups="popups"/>
    </ul>
    
    <div :style="{'top': avaliableItemsPosY + 'px', 'position': 'absolute'}">
      <available-items-list v-show="currentlyClickedSlotId !== -1" ref="available-items"
        :availableSource="availableSource" :slotId="currentlyClickedSlotId"
        :moduleType="moduleType" @item-chosen="$emit('item-chosen', $event)"
        class="available-items-list" :popups="popups"/>
    </div>
  </div>
</template>

<script>
import ChoosableItem from './ChoosableItem.vue'
import AvailableItemsList from './AvailableItemsList.vue'

export default {
  name: 'ChoosableItemsList',
  components: {
    'choosable-item': ChoosableItem,
    'available-items-list': AvailableItemsList
  },

  props: {
    moduleType: String,
    popups: String,

    moduleId: String,
    lastModuleToClickItem: String,
    
    choosableSource: Array,
    availableSource: Array
  },

  data() {
    return {
      lastClickedSlotId: -1,
      avaliableItemsPosY: 0
    }
  },

  computed: {
    currentlyClickedSlotId() {
      if (this.lastModuleToClickItem !== this.moduleId) return -1
        
      return this.lastClickedSlotId
    }
  },

  methods: {
    slotClickedHandler({slotId, posY}) {
      this.lastClickedSlotId = slotId
      this.avaliableItemsPosY = posY

      this.$nextTick(function() { // wait for display change back to visible
        this.$refs['available-items'].$refs['scrollable-ul'].scrollTop = 0
      })

      this.$emit('slot-clicked', this.moduleId)
    }
  }
}
</script>

<style>
</style>
