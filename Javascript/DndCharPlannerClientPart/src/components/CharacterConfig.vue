<template>
  <div>
    <div class="container-single">
      <p>Race</p>
      <choosable-items-list :moduleType="'race'" :moduleId="'race'"
        :choosableSource="[character.race]" :availableSource="filteredRaces"
        @item-chosen="setRace($event.item)"
        :lastModuleToClickItem="lastModuleToClickItem" @slot-clicked="$emit('slot-clicked', $event)"
        class="choosable-items-list list-single" :popups="'right'"/>
    </div>

    <div v-show="filteredSubraces.length > 0" class="container-single">
      <p>Subrace</p>
      <choosable-items-list :moduleType="'subrace'" :moduleId="'subrace'"
        :choosableSource="[character.subrace]" :availableSource="filteredSubraces"
        @item-chosen="setSubrace($event.item)"
        :lastModuleToClickItem="lastModuleToClickItem" @slot-clicked="$emit('slot-clicked', $event)"
        class="choosable-items-list list-single" :popups="'right'"/>
    </div>

    <character-stats class="character-stats" />
    
    <div class="container-single">
      <p>Character Level:</p><p>{{totalLevel}}</p>
    </div>

    <div style="background-color:olive;margin: 1vmin 0 1vmin 0;" v-for="(cls, classIndex) in character.classes" :key="classIndex">
      <div class="container-single">
        <p>Class</p>
        <choosable-items-list :moduleType="'class'" :moduleId="'class'+classIndex"
          :choosableSource="[character.classes[classIndex].class]" :availableSource="filteredClasses"
          @item-chosen="setClass({classIndex: classIndex, cls: $event.item})"
          :lastModuleToClickItem="lastModuleToClickItem" @slot-clicked="$emit('slot-clicked', $event)"
          class="choosable-items-list list-single" :popups="'right'"/>
      </div>

      <div v-show="filteredSubclasses(classIndex).length > 0" class="container-single">
        <p>Subclass</p>
        <choosable-items-list :moduleType="'subclass'" :moduleId="'subclass'+classIndex"
          :choosableSource="[character.classes[classIndex].subclass]"
          :availableSource="filteredSubclasses(classIndex)"
          @item-chosen="setSubclass({classIndex: classIndex, subclass: $event.item})"
          :lastModuleToClickItem="lastModuleToClickItem" @slot-clicked="$emit('slot-clicked', $event)"
          class="choosable-items-list list-single" :popups="'right'"/>
      </div>

      <div class="container-single" style="margin: 1vmin 0 1vmin 0;">
        <p>Class Level</p>
        <select :value="character.classes[classIndex].level" @change="levelChangedHandler(classIndex, $event)" style="margin: 0.5vmin">
          <option v-for="lvl in possibleLevels(classIndex)" :key="lvl" :value="lvl">
            {{lvl}}
          </option>
        </select>
      </div>

      <button v-show="canRemoveClass" @click="removeClass(classIndex)" style="background-color:red;width:100%">Remove Class</button>
    </div>

    <button v-show="canAddClass" @click="addClass" style="background-color:green;">Add New Class</button>
  </div>
</template>

<script>
import ChoosableItemsList from './items/ChoosableItemsList.vue'
import CharacterStats from './CharacterStats.vue'

import {mapState, mapGetters, mapActions} from 'vuex'

export default {
  name: 'CharacterConfig',
  components: {
    'choosable-items-list': ChoosableItemsList,
    'character-stats': CharacterStats
  },

  props: {
    lastModuleToClickItem: String
  },

  data() {
    return {
      levelsList: [1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20]
    }
  },

  computed: {
    ...mapState('character', [
      'character'
    ]),

    ...mapGetters('character', [
      'totalLevel',
      'canAddClass',
      'canRemoveClass'
    ]),

    ...mapGetters('database', [
      'filteredRaces',
      'filteredSubraces',
      'filteredClasses',
      'filteredSubclasses',
    ])
  },

  methods: {
    ...mapActions('character', [
      'setRace',
      'setSubrace',
      'setClass',
      'setSubclass',
      'setLevel',
      'addClass',
      'removeClass'
    ]),

    levelChangedHandler(index, event) {
      this.setLevel({classIndex: index, level: Number(event.target.value)})
    },

    possibleLevels(index) {
      return this.levelsList.filter(lvl => this.totalLevel + lvl - this.character.classes[index].level <= 20)
    }
  }
}
</script>

<style>
</style>
