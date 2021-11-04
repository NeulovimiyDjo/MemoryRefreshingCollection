<template>
  <div>
    <slot></slot>
    <ul>
      <static-ability v-for="(item, itemIndex) in abilitiesSource" :key="itemIndex" :item="item"
        :moduleType="'abilities'" :options="options" :moduleId="baseModuleId + item.name"
        @item-chosen="$emit('item-chosen', $event)" :lastModuleToClickItem="lastModuleToClickItem"
        @slot-clicked="$emit('slot-clicked', $event)" :popups="popups"/>
    </ul>
  </div>
</template>

<script>
import StaticAbility from './StaticAbility.vue'

export default {
  name: 'StaticList',
  components: {
    'static-ability': StaticAbility
  },

  props: {
    popups: String,
    lastModuleToClickItem: String,
    abilitiesSource: Array,
    options: Object,
    baseModuleId: String
  }
}
</script>

<style>
</style>
