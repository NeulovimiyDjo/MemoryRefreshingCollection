<template>
<div @click="lastModuleToClickItem=''" @mouseleave="lastModuleToClickItem=''">
  <div class="character-planner">
    <character-config class="character-config" :lastModuleToClickItem="lastModuleToClickItem" @slot-clicked="lastModuleToClickItem=$event" />

    <div class="spell-lists-area">
      <div>
        <div v-for="(cls, classIndex) in character.classes" :key="classIndex">
          <static-list :abilitiesSource="filteredClassAbilities(classIndex)"
            :lastModuleToClickItem="lastModuleToClickItem"
            :options="character.classes[classIndex].options" :baseModuleId="'options'+classIndex"
            @item-chosen="setClassOption({classIndex: classIndex, pos: $event.pos, abilityName: $event.abilityName, option: $event.option})"
            @slot-clicked="lastModuleToClickItem=$event" class="choosable-items-list"
            :popups="'right'">
            <p>{{smartClassName(cls, classIndex)}} Abilities</p>
          </static-list>

          <static-list v-show="filteredSubclassAbilities(classIndex).length > 0"
            :abilitiesSource="filteredSubclassAbilities(classIndex)" :lastModuleToClickItem="lastModuleToClickItem"
            :options="character.classes[classIndex].options" :baseModuleId="'options'+classIndex"
            @item-chosen="setClassOption({classIndex: classIndex, pos: $event.pos, abilityName: $event.abilityName, option: $event.option})"
            @slot-clicked="lastModuleToClickItem=$event" class="choosable-items-list"
            :popups="'right'">
            <p>{{smartClassName(cls, classIndex)}} Subclass Abilities</p>
          </static-list>
        </div>
      </div>

      <div>
        <static-list :abilitiesSource="filteredRaceAbilities"
          :lastModuleToClickItem="lastModuleToClickItem"
          :options="character.raceOptions" :baseModuleId="'options'+'race'"
          @item-chosen="setRaceOption($event)"
          @slot-clicked="lastModuleToClickItem=$event" class="choosable-items-list"
          :popups="'mid'">
          <p>Race Features</p>
        </static-list>

        <static-list :abilitiesSource="filteredSubraceAbilities"
          :lastModuleToClickItem="lastModuleToClickItem"
          :options="character.raceOptions" :baseModuleId="'options'+'subrace'"
          @item-chosen="setRaceOption($event)"
          @slot-clicked="lastModuleToClickItem=$event" class="choosable-items-list"
          :popups="'mid'">
          <p>Subrace Features</p>
        </static-list>

        <choosable-items-list v-show="character.feats.length > 0"
          :moduleType="'feats'" :moduleId="'feats'"
          :choosableSource="character.feats" :availableSource="filteredFeats"
          @item-chosen="setFeat({pos: $event.slotId, feat: $event.item})"
          :lastModuleToClickItem="lastModuleToClickItem"
          @slot-clicked="lastModuleToClickItem=$event" class="choosable-items-list"
          :popups="'mid'">
          <p>Feats</p>
        </choosable-items-list>
      </div>

      <div>
        <div>
          <choosable-items-list v-for="(cls, classIndex) in character.classes" :key="classIndex"
            v-show="character.classes[classIndex].cantrips.length > 0"
            :moduleType="'cantrips'" :moduleId="'cantrips' + classIndex"
            :choosableSource="character.classes[classIndex].cantrips" :availableSource="filteredCantrips(classIndex)"
            @item-chosen="setCantrip({classIndex: classIndex, pos: $event.slotId, cantrip: $event.item})"
            :lastModuleToClickItem="lastModuleToClickItem" @slot-clicked="lastModuleToClickItem=$event"
            class="choosable-items-list" :popups="'left'">
            <p>{{smartClassName(cls, classIndex)}} Cantrips:</p>
          </choosable-items-list>
        </div>
          
        <div>
          <choosable-items-list v-for="(cls, classIndex) in character.classes" :key="classIndex"
            v-show="character.classes[classIndex].spells.length > 0"
            :moduleType="'spells'" :moduleId="'spells' + classIndex"
            :choosableSource="character.classes[classIndex].spells" :availableSource="filteredSpells(classIndex)"
            @item-chosen="setSpell({classIndex: classIndex, pos: $event.slotId, spell: $event.item})"
            :lastModuleToClickItem="lastModuleToClickItem" @slot-clicked="lastModuleToClickItem=$event"
            class="choosable-items-list" :popups="'left'">
            <p>{{smartClassName(cls, classIndex)}} Spells:</p>
          </choosable-items-list>
        </div>
      </div>
    </div>

    <saver-loader ref="saver-loader" class="saver-loader">Saving/Loading</saver-loader>
  </div>

  <div id="height-balancer" />
</div>
</template>

<script>
import ChoosableItemsList from './items/ChoosableItemsList.vue'
import StaticList from './items/StaticList.vue'
import CharacterConfig from './CharacterConfig.vue'
import SaverLoader from './SaverLoader.vue'

import {mapState, mapGetters, mapActions} from 'vuex'

import api from '../api/planner.js'


export default {
  name: 'CharacterPlanner',
  components: {
    'saver-loader': SaverLoader,
    'character-config': CharacterConfig,
    'choosable-items-list': ChoosableItemsList,
    'static-list': StaticList
  },

  data() {
    return {
      lastModuleToClickItem: ''
    }
  },

  computed: {
    ...mapState('character', [
      'character'
    ]),

    ...mapGetters('database', [
      'filteredRaceAbilities',
      'filteredSubraceAbilities',
      'filteredClassAbilities',
      'filteredSubclassAbilities',
      'filteredFeats',
      'filteredCantrips',
      'filteredSpells'
    ])
  },

  methods: {
    ...mapActions('character', [
      'setFeat',
      'setCantrip',
      'setSpell',
      'setRaceOption',
      'setClassOption'
    ]),

    smartClassName(cls, index) {
      return cls.class.id === -1 ? 'Class' + (index + 1) : cls.class.name
    }
  },

  created() {
    this.$store.dispatch('database/load')
    .then(() => {
      if (this.$route.params.id)
        this.$refs['saver-loader'].loadCharacter(this.$route.params.id)
    })
  },

  watch: {
    '$route' (to, from) {
      if (to.params.id)
        this.$refs['saver-loader'].loadCharacter(to.params.id)
      else
        this.$refs['saver-loader'].resetCharacter()
    }
  }
}
</script>

<style>
</style>
