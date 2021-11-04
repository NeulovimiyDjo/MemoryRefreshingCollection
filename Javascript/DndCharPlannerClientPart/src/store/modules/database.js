import api from '../../api/planner.js'

const scoreImprovement = [
  {id: 110, name: 'stats+1x2'},
  {id: 160, name: 'stat+2'}
]

const bonusStatsAllOne = {
  'str': 1,
  'dex': 1,
  'con': 1,
  'wis': 1,
  'int': 1,
  'cha': 1
}

const scoreIncreaseVariant = [{id: 5800, name: 'stats+1x2'}]


function canHaveMultiple(name) {
  switch(name) {
    case 'stats+1x2':
    case 'stat+2':
    case 'Elemental Adept':
      return true
  }

  return false
}

export default {
  namespaced: true,

  state: {
    database: {
      races: [],
      feats: [],
  
      cantrips: [],
      spells: [],
      classes: []
    }
  },

  getters: {
    filteredRaces: (state, getters, rootState, rootGetters) => {
      return Object.values(state.database.races).filter(race => {
        let alreadyChosen = rootState['character'].character.race.id === race.id

        return !alreadyChosen
      })
    },

    filteredSubraces: (state, getters, rootState, rootGetters) => {
      let subraces = rootState['character'].character.race.subraces
      if (!subraces) return []

      return Object.values(subraces).filter(subrace => {
        let alreadyChosen = rootState['character'].character.subrace.id === subrace.id

        return !alreadyChosen
      })
    },

    filteredFeats: (state, getters, rootState, rootGetters) => {
      return state.database.feats.filter(feat => {
        let alreadyChosen = rootState['character'].character.feats.findIndex(f => f.id === feat.id) !== -1

        let options = rootState['character'].character.raceOptions
        Object.keys(options).forEach(abilityName => {
          let alreadyChosenByRaceOption = options[abilityName].findIndex(o => o.name === feat.name) !== -1

          if (alreadyChosenByRaceOption) alreadyChosen = true
        })

        let satisfiesCharacterConfig = rootGetters['character/satisfiesCharacterConfig'](feat, 'feat')

        return satisfiesCharacterConfig && (!alreadyChosen || canHaveMultiple(feat.name))
      })
    },

    filteredRaceAbilities: (state, getters, rootState, rootGetters) => {
      let raceAbilities = rootState['character'].character.race.abilities
      if (!raceAbilities) return []

      return raceAbilities.filter(ability => {
        let satisfiesCharacterConfig = rootGetters['character/satisfiesCharacterConfig'](ability, 'raceAbility')

        return satisfiesCharacterConfig
      })
    },

    filteredSubraceAbilities: (state, getters, rootState, rootGetters) => {
      let subraceAbilities = rootState['character'].character.subrace.abilities
      if (!subraceAbilities) return []

      return subraceAbilities.filter(ability => {
        let satisfiesCharacterConfig = rootGetters['character/satisfiesCharacterConfig'](ability, 'raceAbility')

        return satisfiesCharacterConfig
      })
    },

    filteredClassAbilities: (state, getters, rootState, rootGetters) => (index) => {
      let classAbilities = rootState['character'].character.classes[index].class.abilities
      if (!classAbilities) return []

      return classAbilities.filter(ability => {
        let satisfiesCharacterConfig = rootGetters['character/satisfiesCharacterConfig'](ability, 'classAbility', index)

        return satisfiesCharacterConfig
      })
    },

    filteredSubclassAbilities: (state, getters, rootState, rootGetters) => (index) => {
      let subclassAbilities = rootState['character'].character.classes[index].subclass.abilities
      if (!subclassAbilities) return []

      return subclassAbilities.filter(ability => {
        let satisfiesCharacterConfig = rootGetters['character/satisfiesCharacterConfig'](ability, 'classAbility', index)

        return satisfiesCharacterConfig
      })
    },

    filteredOptions: (state, getters, rootState, rootGetters) => (ability) => {
      if (ability.name === 'Divine Magic') {
        let index = rootState['character'].character.classes.findIndex(c => c.subclass.name === 'Divine Soul')
        return ability.options.filter(option => {
          let alreadyChosen = false
          for (let i = 0; i < rootState['character'].character.classes.length; i++) {
            let alreadyChosenByClass = rootState['character'].character.classes[i].spells.findIndex(s => s.id === option.id) !== -1

            if (alreadyChosenByClass) alreadyChosen = true
          }

          let satisfiesCharacterConfig = rootGetters['character/satisfiesCharacterConfig'](option, 'spell', index)

          return satisfiesCharacterConfig && !alreadyChosen
        })
      } else {
        return ability.options.filter(option => {
          let alreadyChosen = false
          for (let i = 0; i < rootState['character'].character.classes.length; i++) {
            let options = rootState['character'].character.classes[i].options
            Object.keys(options).forEach(abilityName => {
              let alreadyChosenByClass = options[abilityName].findIndex(o => o.name === option.name) !== -1
  
              if (alreadyChosenByClass) alreadyChosen = true
            })
          }

          let options = rootState['character'].character.raceOptions
          Object.keys(options).forEach(abilityName => {
            let alreadyChosenByRace = options[abilityName].findIndex(o => o.name === option.name) !== -1

            if (alreadyChosenByRace) alreadyChosen = true
          })

          let alreadyChosenByFeat = rootState['character'].character.feats
          .findIndex(f => f.name === option.name) !== -1

          if (alreadyChosenByFeat) alreadyChosen = true

  
          return !alreadyChosen || canHaveMultiple(option.name)
        })
      }
    },

    filteredClasses: (state, getters, rootState, rootGetters) => {
      return Object.values(state.database.classes).filter(cls => {
        let alreadyChosen = rootState['character'].character.classes.findIndex(c => c.class.id === cls.id) !== -1
        let satisfiesCharacterConfig = rootGetters['character/satisfiesCharacterConfig'](cls, 'class')
        
        return satisfiesCharacterConfig && !alreadyChosen
      })
    },

    filteredSubclasses: (state, getters, rootState, rootGetters) => (index) => {
      let subclasses = rootState['character'].character.classes[index].class.subclasses
      if (!subclasses) return []

      return Object.values(subclasses).filter(subclass => {
        let alreadyChosen = rootState['character'].character.classes[index].subclass.id === subclass.id
        let satisfiesCharacterConfig = rootGetters['character/satisfiesCharacterConfig'](subclass, 'subclass', index)

        return satisfiesCharacterConfig && !alreadyChosen
      })
    },

    filteredCantrips: (state, getters, rootState, rootGetters) => (index) => {
      return state.database.cantrips.filter(cantrip => {
        let alreadyChosen = false
        for (let i = 0; i < rootState['character'].character.classes.length; i++) {
          let alreadyChosenByClass = rootState['character'].character.classes[i].cantrips.findIndex(c => c.id === cantrip.id) !== -1

          if (alreadyChosenByClass) alreadyChosen = true
        }

        let satisfiesCharacterConfig = rootGetters['character/satisfiesCharacterConfig'](cantrip, 'cantrip', index)

        return satisfiesCharacterConfig && !alreadyChosen
      })
    },

    filteredSpells: (state, getters, rootState, rootGetters) => (index) => {
      return state.database.spells.filter(spell => {
        let alreadyChosen = false
        for (let i = 0; i < rootState['character'].character.classes.length; i++) {
          let chosenInSpells = rootState['character'].character.classes[i].spells.findIndex(s => s.id === spell.id) !== -1
          
          let options = rootState['character'].character.classes[i].options
          let chosenInOptions = Object.keys(options)
            .findIndex(oName => options[oName].findIndex(o => o.id === spell.id) !== -1) !== -1

          if (chosenInSpells || chosenInOptions) alreadyChosen = true
        }

        let satisfiesCharacterConfig = rootGetters['character/satisfiesCharacterConfig'](spell, 'spell', index)

        return satisfiesCharacterConfig && !alreadyChosen
      })
    }
  },

  mutations: {
    setDatabase(state, database) {
      state.database = database
    }
  },

  actions: {
    load({commit}) {
      let loadedPromise = new Promise(resolve => {
        api.getDndDatabase()
        .then(db => {
          let database = {}
          database.cantrips = db.cantrips
          database.spells = db.spells
          database.classes = db.classes
          database.feats = scoreImprovement.concat(db.feats)
          database.races = db.races
  
          
          database.races['Human'].subraces['Standard Human'].abilities
            .find(a => a.name === 'Ability Score Increase').bonusStats = bonusStatsAllOne
  
          database.races['Half-Elf'].abilities
            .find(a => a.name === 'Ability Score Increase').options = scoreIncreaseVariant
  
          database.races['Human'].subraces['Variant Human'].abilities
            .find(a => a.name === 'Ability Score Increase').options = scoreIncreaseVariant
          database.races['Human'].subraces['Variant Human'].abilities
            .find(a => a.name === 'Feat').options = db.feats
  
          let dm = database.classes['Sorcerer'].subclasses['Divine Soul'].abilities
            .find(a => a.name === 'Divine Magic')
          dm.options = database.spells.filter(s => s.classes.findIndex(c => c === 'Cleric') !== -1)
  
          let afs = database.classes['Fighter'].subclasses['Champion'].abilities
            .find(a => a.name === 'Additional Fighting Style')
          afs.options = database.classes['Fighter'].abilities
            .find(a => a.name === 'Fighting Style').options
  
            
          commit('setDatabase', database)

          resolve()
        })
      })

      return loadedPromise
    }
  }
}