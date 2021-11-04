function makeNewList(amount, state, getters, type, index) {
  let oldList = []
  let itemType = ''
  switch(type) {
    case 'feats':
      oldList = state.character.feats
      itemType = 'feat'
      break
    case 'cantrips':
      oldList = state.character.classes[index].cantrips
      itemType = 'cantrip'
      break
    case 'spells':
      oldList = state.character.classes[index].spells
      itemType = 'spell'
      break
  }

  let newList = Array(amount)
  for (let i = 0; i < newList.length; i++) {
    if (i < oldList.length && getters.satisfiesCharacterConfig(oldList[i], itemType, index)) {
      newList[i] = oldList[i]
    } else {
      newList[i] = {id: - 1}
    }
  }

  return newList
}

function setNewOptions(state, commit) {
  {
    let newOptions = {}
    let oldOptions = state.character.raceOptions
  
    let changed = false
    if (addOptions(newOptions, oldOptions, 1, state.character.race.abilities))
      changed = true
    if (addOptions(newOptions, oldOptions, 1, state.character.subrace.abilities))
      changed = true

      if (oldOptions.length !== newOptions.length) {
        changed = true
      }
      else {
        Object.keys(oldOptions).forEach(abilityName => {
          if (!newOptions[abilityName]) changed = true
        })
      }

    if (changed)
      commit('setRaceOptionsObject', newOptions)
  }


  for (let i = 0; i < state.character.classes.length; i++) {
    let newOptions = {}
    let oldOptions = state.character.classes[i].options

    let classLvl = state.character.classes[i].level
    let changed = false
    if (addOptions(newOptions, oldOptions, classLvl, state.character.classes[i].class.abilities))
      changed = true
    if (addOptions(newOptions, oldOptions, classLvl, state.character.classes[i].subclass.abilities))
      changed = true

    if (oldOptions.length !== newOptions.length) {
      changed = true
    }
    else {
      Object.keys(oldOptions).forEach(abilityName => {
        if (!newOptions[abilityName]) changed = true
      })
    }

    if (changed)
      commit('setClassOptionsObject', {classIndex: i, options: newOptions})
  }
}

function optionSatisfiesCharacterConfig(option, ability, classLvl) {
  let isForCurrentAbility = ability.options.findIndex(o => o.id === option.id) !== -1
  let enoughLevel = option.level ? option.level * 2 - 1 <= classLvl : true

  return isForCurrentAbility && enoughLevel
}

function addOptions(newOptions, oldOptions, classLvl, abilities) {
  if (!abilities) return false

  let changed = false

  abilities.forEach(a => {
    let amount = a.increases.filter(lvl => lvl <= classLvl).length

    let oldList = oldOptions[a.name]
    if (typeof(oldList) === 'undefined') oldList = []

    let newList = Array(amount)
    if (oldList.length !== newList.length) changed = true
    for (let i = 0; i < newList.length; i++) {
      if (i < oldList.length && optionSatisfiesCharacterConfig(oldList[i], a, classLvl)) {
        newList[i] = oldList[i]
      } else {
        changed = true
        newList[i] = {id: - 1}
      }
    }

    newOptions[a.name] = newList
  })

  return changed
}

function getListAmount(state, getters, i, type) {
  if (state.character.classes[i].class.id === -1) return 0

  let increases
  switch(type) {
    case 'feats':
      let feats = state.character.classes[i].class.feats
      increases = feats.filter(lvl => lvl <= state.character.classes[i].level)
      break
    case 'cantrips':
      if (state.character.classes[i].subclass.cantrips
          && state.character.classes[i].subclass.cantrips.length > 0) {

        let cantrips = state.character.classes[i].subclass.cantrips
        increases = cantrips.filter(lvl => lvl <= state.character.classes[i].level)
      } else {
        let cantrips = state.character.classes[i].class.cantrips
        increases = cantrips.filter(lvl => lvl <= state.character.classes[i].level)
      }
      break
    case 'spells':
      switch (state.character.classes[i].class.name) {
        case 'Artificer':
        case 'Wizard':
         return Math.max(getters.realStatModifiers['int'] + state.character.classes[i].level, 1)
        case 'Cleric':
        case 'Druid':
         return Math.max(getters.realStatModifiers['wis'] + state.character.classes[i].level, 1)
        case 'Paladin':
         return Math.max(getters.realStatModifiers['cha'] + state.character.classes[i].level, 1)
      }
      if (state.character.classes[i].subclass.spells
          && state.character.classes[i].subclass.spells.length > 0) {
        let spells = state.character.classes[i].subclass.spells
        increases = spells.filter(lvl => lvl <= state.character.classes[i].level)
      } else {
        let spells = state.character.classes[i].class.spells
        increases = spells.filter(lvl => lvl <= state.character.classes[i].level)
      }
      break
  }

  return increases.length
}


function areDifferentLists(oldList, newList) {
  if (oldList.length !== newList.length) return true

  let changed = false
  for (let i = 0; i < oldList.length; i++) {
    if (oldList[i].id !== newList[i].id) changed = true
  }

  return changed
}


function modifyBonusValuesFrom(items, bonusValues) {
  items.forEach(item => {
    if (item.bonusStats) {
      Object.keys(item.bonusStats).forEach(statName => {
        bonusValues[statName] += item.bonusStats[statName]
      })
    }
  })
}


function bonusValues(state, rootGetters) {
  let bonusValues = {
    'str': 0,
    'dex': 0,
    'con': 0,
    'wis': 0,
    'int': 0,
    'cha': 0
  }

  modifyBonusValuesFrom(state.character.feats, bonusValues)

  {
    modifyBonusValuesFrom(rootGetters['database/filteredRaceAbilities'], bonusValues)
    modifyBonusValuesFrom(rootGetters['database/filteredSubraceAbilities'], bonusValues)

    let options = state.character.raceOptions
    Object.keys(options).forEach(abilityName => {
      modifyBonusValuesFrom(options[abilityName], bonusValues)
    })
  }

  for (let i = 0; i < state.character.classes.length; i++) {
    modifyBonusValuesFrom(rootGetters['database/filteredClassAbilities'](i), bonusValues)
    modifyBonusValuesFrom(rootGetters['database/filteredSubclassAbilities'](i), bonusValues)

    let options = state.character.classes[i].options
    Object.keys(options).forEach(abilityName => {
      modifyBonusValuesFrom(options[abilityName], bonusValues)
    })
  }

  return bonusValues
}


const emptyClass = {
  class: {id: -1},
  level: 1,
  subclass: {id: -1},
  cantrips: [],
  spells: [],
  options: {}
}

const emptyChar = {
  race: {id: -1},
  subrace: {id: -1},
  raceOptions: {},
  stats: {
    'str': 8,
    'dex': 8,
    'con': 8,
    'wis': 8,
    'int': 8,
    'cha': 8
  },
  feats: [],
  classes: [JSON.parse(JSON.stringify(emptyClass))]
}

export default {
  namespaced: true,

  state: {
    changed: false,
    character: JSON.parse(JSON.stringify(emptyChar))
  },

  getters: {
    realStatScores(state, getters, rootState, rootGetters) {
      let res = {}
      Object.keys(state.character.stats).forEach(statName => {
        res[statName] = Math.min(state.character.stats[statName] + bonusValues(state, rootGetters)[statName], 20)
      })
    
      return res
    },

    realStatModifiers(state, getters) {
      let res = {}
      Object.keys(state.character.stats).forEach(statName => {
        res[statName] = Math.floor((getters.realStatScores[statName] - 10) / 2)
      })

      return res
    },

    totalLevel(state) {
      let sum = 0
      state.character.classes.forEach(c => {
        sum = sum + c.level
      })

      return sum
    },

    canAddClass(state, getters, rootState) {
      return getters.totalLevel < 20
        && state.character.classes.length < Object.keys(rootState['database'].database.classes).length
    },

    canRemoveClass(state) {
      return state.character.classes.length > 1
    },

    satisfiesCharacterConfig: (state) => (item, type, index) => {
      switch(type) {
        case 'subrace': {
          let isForCurrentRace = false
          if (state.character.race.subraces) {
            isForCurrentRace = Object.values(state.character.race.subraces)
              .findIndex(sub => sub.id === item.id) !== -1
          }

          return isForCurrentRace
        }
        case 'feat': {
          return true
        }
        case 'raceAbility': {
          return true
        }
        case 'classAbility': {
          let enoughLevel = item.level <= state.character.classes[index].level

          return enoughLevel
        }
        case 'class': {
          return true
        }
        case 'subclass': {
          // check isForCurrentClass in case of class/level change
          let isForCurrentClass = false
          if (state.character.classes[index].class.subclasses) {
            isForCurrentClass = Object.values(state.character.classes[index].class.subclasses)
              .findIndex(sub => sub.id === item.id) !== -1
          }
          

          let enoughLevel = item.level <= state.character.classes[index].level

          return isForCurrentClass && enoughLevel
        }
        case 'cantrip':
        case 'spell': {
          if (item.id === -1) return false // oldList value when called from makeNewList

          let className = state.character.classes[index].class.name
          let isDivineSoul = state.character.classes[index].subclass.name === 'Divine Soul'
          let isForCurrentClass = Object.values(item.classes)
            .findIndex(c => c === className || isDivineSoul && c === 'Cleric') !== -1

          if (state.character.classes[index].subclass.name === 'Hexblade') {
            let isSpecialSpell = false
            switch(item.name) {
              case 'Shield':
              case 'Wrathful Smite':
              case 'Blur':
              case 'Branding Smite':
              case 'Blink':
              case 'Elemental Weapon':
              case 'Phantasmal Killer':
              case 'Staggering Smite':
              case 'Banishing Smite':
              case 'Cone of Cold':
                isSpecialSpell = true
                break;
            }
            isForCurrentClass =
              Object.values(item.classes).findIndex(c => c === className) !== -1
              || isSpecialSpell
          }

          if (state.character.classes[index].subclass.name === 'Arcane Trickster') {
            isForCurrentClass = Object.values(item.classes)
              .findIndex(c => c === 'Wizard') !== -1
          }
          if (state.character.classes[index].subclass.name === 'Eldritch Knight') {
            isForCurrentClass = Object.values(item.classes)
              .findIndex(c => c === 'Wizard') !== -1
          }
    
          let enoughLevel = item.level * 2 - 1 <= state.character.classes[index].level

          return isForCurrentClass && enoughLevel
        }
      }
    }
  },

  mutations: {
    setChangedFalse(state) {
      state.changed = false
    },

    resetCharacter(state) {
      state.character = JSON.parse(JSON.stringify(emptyChar))
      state.changed = false
    },

    setCharacter(state, character) {
      state.character = character
      state.changed = true
    },

    setRace(state, race) {
      state.character.race = race
      state.changed = true
    },

    setSubrace(state, subrace) {
      state.character.subrace = subrace
      state.changed = true
    },

    setStat(state, {statName, value}) {
      state.character.stats[statName] = value
      state.changed = true
    },

    setFeat(state, {pos, feat}) {
      state.character.feats.splice(pos, 1, feat)
      state.changed = true
    },

    setClass(state, {classIndex, cls}) {
      state.character.classes[classIndex].class = cls
      state.changed = true
    },

    setSubclass(state, {classIndex, subclass}) {
      state.character.classes[classIndex].subclass = subclass
      state.changed = true
    },

    setLevel(state, {classIndex, level}) {
      state.character.classes[classIndex].level = level
      state.changed = true
    },

    setCantrip(state, {classIndex, pos, cantrip}) {
      state.character.classes[classIndex].cantrips.splice(pos, 1, cantrip)
      state.changed = true
    },

    setSpell(state, {classIndex, pos, spell}) {
      state.character.classes[classIndex].spells.splice(pos, 1, spell)
      state.changed = true
    },

    setRaceOption(state, {pos, abilityName, option}) {
      state.character.raceOptions[abilityName].splice(pos, 1, option)
      state.changed = true
    },

    setClassOption(state, {classIndex, pos, abilityName, option}) {
      state.character.classes[classIndex].options[abilityName].splice(pos, 1, option)
      state.changed = true
    },

    setFeatsList(state, feats) {
      if (areDifferentLists(state.character.feats, feats)) {
        state.character.feats = feats
        state.changed = true
      }
    },

    setRaceOptionsObject(state, options) {
      state.character.raceOptions = options
      state.changed = true
    },

    setClassOptionsObject(state, {classIndex, options}) {
      state.character.classes[classIndex].options = options
      state.changed = true
    },

    setCantripsList(state, {classIndex, cantrips}) {
      if (areDifferentLists(state.character.classes[classIndex].cantrips, cantrips)) {
        state.character.classes[classIndex].cantrips = cantrips
        state.changed = true
      }
    },

    setSpellsList(state, {classIndex, spells}) {
      if (areDifferentLists(state.character.classes[classIndex].spells, spells)) {
        state.character.classes[classIndex].spells = spells
        state.changed = true
      }
    },

    addClass(state) {
      state.character.classes.push(JSON.parse(JSON.stringify(emptyClass)))
      state.changed = true
    },

    removeClass(state, classIndex) {
      state.character.classes.splice(classIndex, 1)
      state.changed = true
    }
  },

  actions: {
    resetCharacter({commit}) {
      commit('resetCharacter')
    },

    setCharacter({commit, dispatch}, character) {
      commit('setCharacter', character)
      dispatch('checkSubclass')
      dispatch('setAmounts')
    },

    setStat({commit, dispatch}, arg) {
      commit('setStat', arg)
      dispatch('setAmounts')
    },

    setRace({commit, dispatch}, arg) {
      commit('setRace', arg)
      dispatch('checkSubrace')
      dispatch('setAmounts')
    },

    setSubrace({commit, dispatch}, arg) {
      commit('setSubrace', arg)
      dispatch('setAmounts')
    },

    setFeat({commit, dispatch}, arg) {
      commit('setFeat', arg)
      dispatch('setAmounts')
    },

    setClass({commit, dispatch}, arg) {
      commit('setClass', arg)
      dispatch('checkSubclass')
      dispatch('setAmounts')
    },

    setSubclass({commit, dispatch}, arg) {
      commit('setSubclass', arg)
      dispatch('setAmounts')
    },

    setCantrip({commit}, arg) {
      commit('setCantrip', arg)
    },

    setSpell({commit}, arg) {
      commit('setSpell', arg)
    },

    setRaceOption({commit, dispatch}, arg) {
      commit('setRaceOption', arg)
      dispatch('setAmounts')
    },

    setClassOption({commit, dispatch}, arg) {
      commit('setClassOption', arg)
      dispatch('setAmounts')
    },

    setLevel({commit, dispatch}, arg) {
      commit('setLevel', arg)
      dispatch('checkSubclass')
      dispatch('setAmounts')
    },

    setAmounts({state, getters, commit}) {
      let totalFeatsAmount = 0

      for (let i = 0; i < state.character.classes.length; i++) {
        let featsAmount = getListAmount(state, null, i, 'feats')
        let cantripsAmount = getListAmount(state, null, i, 'cantrips')
        let spellsAmount = getListAmount(state, getters, i, 'spells')

        totalFeatsAmount += featsAmount

        commit('setCantripsList', {classIndex: i, cantrips: makeNewList(cantripsAmount, state, getters, 'cantrips', i)})
        commit('setSpellsList', {classIndex: i, spells: makeNewList(spellsAmount, state, getters, 'spells', i)})
      }

      commit('setFeatsList', makeNewList(totalFeatsAmount, state, getters, 'feats'))

      setNewOptions(state, commit)
    },

    checkSubrace({state, getters, commit}) {
      if (!getters.satisfiesCharacterConfig(state.character.subrace, 'subrace')) {
        commit('setSubrace', {id: -1})
      }
    },

    checkSubclass({state, getters, commit}) {
      for (let i = 0; i < state.character.classes.length; i++) {
        if (!getters.satisfiesCharacterConfig(state.character.classes[i].subclass, 'subclass', i)) {
          commit('setSubclass', {classIndex: i, subclass: {id: -1}})
        }
      }
    },

    addClass({getters, commit, dispatch}) {
      if (getters.canAddClass) {
        commit('addClass')
        dispatch('setAmounts')
      }
    },

    removeClass({getters, commit, dispatch}, classIndex) {
      if (getters.canRemoveClass){
        commit('removeClass', classIndex)
        dispatch('setAmounts')
      }
    }
  }
}