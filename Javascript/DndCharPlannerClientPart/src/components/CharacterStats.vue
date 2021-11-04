<template>
  <div>
    <div>
      <p>Real Stats:</p>  
      <table>
        <tr>
          <td>stat name</td>
          <td>base score</td>
          <td>real score</td>
          <td>real modifier</td>
        </tr>
        <tr v-for="(statName, index) in Object.keys(character.stats)" :key="index" class="stat">
          <td>{{statName}}</td>
          <td>
            <select :value="character.stats[statName]" @change="changeStatValue(statName, $event)">
              <option v-for="val in values" :key="val" :value="val">
                {{val}}
              </option>
            </select>
          </td>
          <td>{{realStatScores[statName]}}</td>
          <td>{{realStatModifiers[statName]}}</td>
        </tr>
      </table>
    </div>
  </div>
</template>


<script>
import {mapState, mapGetters, mapActions} from 'vuex'

export default {
  name: 'CharacterStats',

  data() {
    return {
      values: [3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18]
    }
  },

  computed: {
    ...mapState('character', [
      'character'
    ]),

    ...mapGetters('character', [
      'realStatScores',
      'realStatModifiers'
    ])
  },

   methods: {
    ...mapActions('character', [
      'setStat'
    ]),

    changeStatValue(statName, event) {
      this.setStat({statName: statName, value: Number(event.target.value)})
    }
  }
}
</script>

<style>
</style>
