<template>
  <div>
    <div style="width: 45%;">
      <p>Avaliable opponents:</p>
      <div class="buttons-div">
        <button class="small-btn" @click="inviteButtonHandler">Invite</button>
      </div>
      <ul>
        <li v-for="opponent in avaliableOpponents" :key="opponent.key" @click="opponentSelectedHandler($event, opponent.key)">
          {{opponent.key}}
        </li>
      </ul>
    </div>
    <div style="width: 45%;">
      <p>Invites:</p>
      <div class="buttons-div">
        <button class="small-btn" @click="acceptButtonHandler">Accept</button>
        <button class="small-btn" @click="declineButtonHandler">Decline</button>
      </div>
      <ul>
        <li v-for="invite in invites" :key="invite.id" @click="inviteSelectedHandler($event, invite.id)">
          {{invite.id}} <br/> x:{{invite.settings.xDim}} y:{{invite.settings.yDim}} w:{{invite.settings.winSize}} t:{{invite.settings.timeout}}
        </li>
      </ul>
    </div>
  </div>
</template>

<script>
import {mapState} from 'vuex'

export default {
  data() {
    return {
      unsubscribeMutations: null,

      prevSelectedOpponentTarget: undefined,
      prevSelectedInviteTarget: undefined,
    }
  },

  computed: {
    ...mapState('invites', [
      'avaliableOpponents',
      'invites',

      'selectedOpponentId',
      'selectedInviteOpponentId'
    ])
  },

  
  created() {
    this.unsubscribeMutations = this.$store.subscribeAction((action,state) => {
      switch(action.type) {
        case 'invites/removeInvite':
          if (action.payload == this.selectedInviteOpponentId)
            this.clearPrevSelectedInviteTargetClassName()
          break
      }
    })
  },
    
  destroyed() {
    this.unsubscribeMutations()
  },

  methods: {
    opponentSelectedHandler(event, id) {
      this.clearPrevSelectedOpponentTargetClassName()

      this.prevSelectedOpponentTarget = event.target
      event.target.className = 'selected'

      this.$store.commit('invites/setSelectedOpponentId', id)
    },

    inviteSelectedHandler(event, id) {
      this.clearPrevSelectedInviteTargetClassName()

      this.prevSelectedInviteTarget = event.target
      event.target.className = 'selected'

      this.$store.commit('invites/setSelectedInviteOpponentId', id)
    },


    inviteButtonHandler() {
      if (this.avaliableOpponents.find(player => player.key === this.selectedOpponentId)) {
        this.$emit('invite-click', this.selectedOpponentId)
      }
    },

    acceptButtonHandler() {
      if (this.invites.find(invite => invite.id === this.selectedInviteOpponentId)) {
        this.$emit('accept-click', this.selectedInviteOpponentId)
      }
    },

    declineButtonHandler() {
      if (this.invites.find(invite => invite.id === this.selectedInviteOpponentId)) {
        this.$emit('decline-click', this.selectedInviteOpponentId)
      }
    },

    clearPrevSelectedOpponentTargetClassName(){
      if (typeof(this.prevSelectedOpponentTarget) !== 'undefined')
        this.prevSelectedOpponentTarget.className = ''
    },

    clearPrevSelectedInviteTargetClassName(){
      if (typeof(this.prevSelectedInviteTarget) !== 'undefined')
        this.prevSelectedInviteTarget.className = ''
    }
  }
}
</script>

<style>
  #invites-control {
    display: flex;
    flex-direction: row;
    justify-content: space-around;
  }

  .buttons-div {
    display: flex;
    flex-direction: row;
    justify-content: space-between;
  }

  .small-btn {
    width: 40%;
    background-color: darkolivegreen;
  }

  .selected {
    background-color: green;
  }

  button {
    outline: none;
  }

  button:active {
    background: green;
  }

</style>