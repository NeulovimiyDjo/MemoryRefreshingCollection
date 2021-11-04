<template>
  <li class="item-slot">
    <div @mouseenter="enterHandler" @mouseleave="leaveHandler" class="item-slot choosable-item disabled-item">
      {{levelText}}{{item.name}}

      <div :style="{'position': 'absolute', 'top': posY + 'px'}">
        <item-tooltip v-if="mouseOver" :item="item" :moduleType="moduleType"
          :posX="posX" @enter-child="leaveHandler" :popups="popups" class="item-tooltip" />
      </div>
    </div>
  

    <div v-if="typeof(options[item.name]) !== 'undefined'">
      <choosable-items-list :moduleType="'options'" :moduleId="moduleId"
        :choosableSource="options[item.name]" :availableSource="filteredOptions(item)"
        @item-chosen="$emit('item-chosen', {pos: $event.slotId, abilityName: item.name, option: $event.item})"
        :lastModuleToClickItem="lastModuleToClickItem" @slot-clicked="$emit('slot-clicked', $event)"
        class="choosable-items-list list-options" :popups="popups"/>
    </div>
  </li>
</template>

<script>
import ChoosableItemsList from './ChoosableItemsList.vue'

import itemSlot from '../../mixins/itemSlot.js'

import {mapGetters} from 'vuex'

export default {
  name: 'StaticAbility',
  mixins: [itemSlot],
  components: {
    'choosable-items-list': ChoosableItemsList
  },

  props: {
    lastModuleToClickItem: String,
    options: Object,
    moduleId: String
  },

  computed: {
    ...mapGetters('database', [
      'filteredOptions'
    ])
  }
}
</script>

<style>
</style>
