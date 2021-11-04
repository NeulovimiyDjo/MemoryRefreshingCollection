<template>
  <div style="display: flex; flex-direction: column;">
    <slot></slot>
    <button @click="saveHandler">Save</button>
    <br />
    Id to load:
    <input v-model="characterId"/>
    <button @click="loadHandler">Load</button>
    <br />
    <br />
    <button @click="resetHandler">Reset</button>
  </div>
</template>

<script>
import {mapState, mapGetters, mapMutations, mapActions} from 'vuex'

import api from '../api/planner.js'

function nameToItem(name, array, deepCopy) {
  if (name === '') return {id: -1}

  let item = array.find(el => el.name === name)
  if (deepCopy === 'deepCopy') return JSON.parse(JSON.stringify(item))

  return item
}

function addBonusStatsFor(items, bonusStats, prefix) {
  for (let i = 0; i < items.length; i++) {
    if (items[i].bonusStats) {
      bonusStats[prefix + ':' + items[i].name + ':' + i] = items[i].bonusStats
    }
  }
}

function getBonusStats(character) {
  let bonusStats = {}
  addBonusStatsFor(character.feats, bonusStats, 'f')

  {
    let options = character.raceOptions
    Object.keys(options).forEach(abilityName => {
      addBonusStatsFor(options[abilityName], bonusStats, 'ro')
    })
  }

  for (let i = 0; i < character.classes.length; i++) {
    let options = character.classes[i].options
    Object.keys(options).forEach(abilityName => {
      addBonusStatsFor(options[abilityName], bonusStats, 'c' + i + 'o')
    })
  }

  return bonusStats
}

function setBonusStatsFor(items, bonusStats, prefix) {
  for (let i = 0; i < items.length; i++) {
    if (items[i].id !== -1) {
      if (bonusStats[prefix + ':' + items[i].name + ':' + i]) {
        items[i].bonusStats = bonusStats[prefix + ':' + items[i].name + ':' + i]
      }
    }
  }
}

function setBonusStats(character, bonusStats) {
  setBonusStatsFor(character.feats, bonusStats, 'f')

  {
    let options = character.raceOptions
    Object.keys(options).forEach(abilityName => {
      setBonusStatsFor(options[abilityName], bonusStats, 'ro')
    })
  }

  for (let i = 0; i < character.classes.length; i++) {
    let options = character.classes[i].options
    Object.keys(options).forEach(abilityName => {
      setBonusStatsFor(options[abilityName], bonusStats, 'c' + i + 'o')
    })
  }
}

export default {
  name: 'SaverLoader',

  data() {
    return {
      characterId: ''
    }
  },

  computed: {
    ...mapState('character', [
      'changed',
      'character'
    ]),

    ...mapState('database', [
      'database'
    ]),

    ...mapGetters('database', [
      'filteredRaceAbilities',
      'filteredClassAbilities'
    ]),
  },

  methods: {
    ...mapMutations({
      setChangedFalse: 'character/setChangedFalse'
    }),

    ...mapActions({
      setCharacter: 'character/setCharacter',
      resetCharacter: 'character/resetCharacter'
    }),

    resetHandler() {
      this.resetCharacter()

      this.$router.push({name: 'planner', params: {}})
    },

    loadHandler() {
      this.loadCharacter(this.characterId)
    },

    saveHandler() {
      let raceOptions = {}
      Object.keys(this.character.raceOptions).forEach(abilityName => {
        raceOptions[abilityName] = this.character.raceOptions[abilityName].map(o => o.id === -1 ? '' : o.name)
      })

      let char = {
        race: this.character.race.id === -1 ? '' : this.character.race.name,
        subrace: this.character.subrace.id === -1 ? '' : this.character.subrace.name,
        stats: this.character.stats,
        bonusStats: getBonusStats(this.character),
        feats: this.character.feats.map(feat => feat.id === -1 ? '' : feat.name),
        raceOptions: raceOptions,

        classes: this.character.classes.map(c => {
          let options = {}
          Object.keys(c.options).forEach(abilityName => {
            options[abilityName] = c.options[abilityName].map(o => o.id === -1 ? '' : o.name)
          })

          return {
            class: c.class.id === -1 ? '' : c.class.name,
            subclass: c.subclass.id === -1 ? '' : c.subclass.name,
            level: c.level,
            cantrips: c.cantrips.map(cantrip => cantrip.id === -1 ? '' : cantrip.name),
            spells: c.spells.map(spell => spell.id === -1 ? '' : spell.name),
            options: options
          }
        })
      }


      if (this.changed) {
        api.saveCharacter(char)
        .then(id => {
          this.characterId = id
          this.$router.push({name: 'planner', params: {id: String(id)}})
        })
      }

      this.setChangedFalse()
    },

    loadCharacter(charId) {
      api.getCharacter(charId)
      .then(char => {
          let character = {
          race: nameToItem(char.race, Object.values(this.database.races)),
          stats: char.stats,
          feats: char.feats.map(name => nameToItem(name, this.database.feats, 'deepCopy')),

          classes: char.classes.map(c => {
            let cls = {
              class: nameToItem(c.class, Object.values(this.database.classes)),
              level: c.level,
              cantrips: c.cantrips.map(name => nameToItem(name, this.database.cantrips)),
              spells: c.spells.map(name => nameToItem(name, this.database.spells))
            }

            cls.subclass = cls.class.subclasses ?
              nameToItem(c.subclass, Object.values(cls.class.subclasses))
              : {id: -1}


            let options = {}
            Object.keys(c.options).forEach(abilityName => {
              let ability = nameToItem(abilityName, cls.class.abilities.concat(cls.subclass.abilities))
              options[abilityName] = c.options[abilityName].map(optionName => {
                return nameToItem(optionName, ability.options, 'deepCopy')
              })
            })

            cls.options = options


            return cls
          })
        }

        character.subrace = character.race.subraces ?
          nameToItem(char.subrace, Object.values(character.race.subraces))
          : {id: -1}


        let raceOptions = {}
        Object.keys(char.raceOptions).forEach(abilityName => {
          let ability = nameToItem(abilityName, character.race.abilities.concat(character.subrace.abilities))
          raceOptions[abilityName] = char.raceOptions[abilityName].map(optionName => {
            return nameToItem(optionName, ability.options, 'deepCopy')
          })
        })

        character.raceOptions = raceOptions

        setBonusStats(character, char.bonusStats)


        this.setCharacter(character)
        this.setChangedFalse()
        this.$router.push({name: 'planner', params: {id: String(charId)}})
      })
    }
  }
}
</script>

<style>
</style>
