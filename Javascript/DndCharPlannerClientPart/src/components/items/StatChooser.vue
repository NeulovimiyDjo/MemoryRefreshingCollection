<template>
  <div class="stat-chooser">
    <div v-if="statsPlus1x2Items.includes(item.name)">
      <div style="display:flex;direction:row;">
        <p>+1</p>
        <select @click.stop :value="selected1" @change="selected1Changed($event)">
          <option v-for="stat in statTypesMinusSelected2" :key="stat" :value="stat">
            {{stat}}
          </option>
        </select>
      </div>
      <div style="display:flex;direction:row;">
        <p>+1</p>
        <select @click.stop :value="selected2" @change="selected2Changed($event)">
          <option v-for="stat in statTypesMinusSelected1" :key="stat" :value="stat">
            {{stat}}
          </option>
        </select>
      </div>
    </div>

    <div v-if="statsPlus2x1Items.includes(item.name)">
      <div style="display:flex;direction:row;">
        <p>+2</p>
        <select @click.stop :value="selected1" @change="selected1Changed($event)">
          <option v-for="stat in statTypesFiltered" :key="stat" :value="stat">
            {{stat}}
          </option>
        </select>
        <br />
      </div>
    </div>

    <div v-if="statsPlus1x1Items.includes(item.name)">
      <div style="display:flex;direction:row;">
        <p>+1</p>
        <select @click.stop :value="selected1" @change="selected1Changed($event)">
          <option v-for="stat in statTypesFiltered" :key="stat" :value="stat">
            {{stat}}
          </option>
        </select>
        <br />
      </div>
    </div>
  </div>
</template>

<script>
import {mapState} from 'vuex'

const statsPlus1x2Items = ['stats+1x2']
const statsPlus2x1Items = ['stat+2']
const statsPlus1x1Items = ['Resilient','Athlete','Lightly Armored',
'Moderately Armored','Observant','Tavern Brawler','Weapon Master']


function makeNewItem(item, s1, s2) {
  let newItem = JSON.parse(JSON.stringify(item))

  if (statsPlus1x2Items.includes(item.name)) {
    newItem.bonusStats = {[s1]:1,[s2]:1}
  } else if (statsPlus2x1Items.includes(item.name)) {
    newItem.bonusStats = {[s1]:2}
  } else if (statsPlus1x1Items.includes(item.name)) {
    newItem.bonusStats = {[s1]:1}
  }

  return newItem
}

export default {
  name: 'StatChooser',

  props: {
    item: Object
  },

  data() {
    return {
      statTypes: ['str', 'dex', 'con', 'wis', 'int', 'cha'],
      selected1: 'str',
      selected2: 'dex',

      statsPlus1x2Items: statsPlus1x2Items,
      statsPlus2x1Items: statsPlus2x1Items,
      statsPlus1x1Items: statsPlus1x1Items
    }
  },

  computed: {
    ...mapState('database', [
      'database'
    ]),

    statTypesMinusSelected1() {
      return this.statTypes.filter(t => t !== this.selected1)
    },

    statTypesMinusSelected2() {
      return this.statTypes.filter(t => t !== this.selected2)
    },

    statTypesFiltered() {
      switch(this.item.name) {
        case 'Athlete':
        case 'Lightly Armored':
        case 'Moderately Armored':
        case 'Weapon Master':
          return ['str', 'dex']
        case 'Observant':
          this.selected1 = 'int'
          return ['int', 'wis']
        case 'Tavern Brawler':
          return ['str', 'con']
        default:
          return this.statTypes
      }
    }
  },

  created() {
    this.changeItem()
  },

  methods: {
    selected1Changed(event) {
      this.selected1 = event.target.value

      this.changeItem()
    },

    selected2Changed(event) {
      this.selected2 = event.target.value

      this.changeItem()
    },

    changeItem() {
      if (statsPlus1x2Items.concat(statsPlus2x1Items, statsPlus1x1Items).includes(this.item.name)) {
        let newItem = makeNewItem(this.item, this.selected1, this.selected2)

        this.$emit('item-changed', newItem)
      }
    }
  }
}
</script>

<style>
</style>
