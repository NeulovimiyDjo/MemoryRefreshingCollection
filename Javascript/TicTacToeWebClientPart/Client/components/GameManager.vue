<template>
  <div id="game-manager">
    <template v-if="lookingForGame">
      <invites-control id="invites-control" @invite-click="inviteClickHandler" @accept-click="acceptClickHandler"
        @decline-click="declineClickHandler"></invites-control>
      <settings-control id="settings-control" @sizes-click="sizesClickHandler"></settings-control>
    </template>

    <template v-else>
      <game-field id="game-field" @cell-clicked="cellClickHandler" :cellIcons="resources.cellIcons"></game-field>
      <game-control id="game-control" @exit-click="exitClickHandler" @offer-replay-click="offerReplayClickHandler"
        @accept-replay-click="acceptReplayClickHandler" @decline-replay-click="declineReplayClickHandler"
        :opponentExited="opponentExited" :replayOffered="replayOffered" :replayDeclined="replayDeclined"></game-control>
    </template>
  </div>
</template>

<script>
import InvitesControl from './InvitesControl.vue'
import SettingsControl from './SettingsControl.vue'

import GameField from './GameField.vue'
import GameControl from './GameControl.vue'

import Resources from '../resources/resources.js'

import {HubConnectionBuilder, LogLevel} from '@aspnet/signalr'

const debug = process.env.NODE_ENV !== 'production'

export default {
  components: {
    'invites-control': InvitesControl,
    'settings-control': SettingsControl,

    'game-field': GameField,
    'game-control': GameControl
  },

  data() {
    return {
      resources: {
        cellIcons: null
      },

      hubConnection: null,
      myId: '',

      lookingForGame: true,

      opponentExited: false,
      replayOffered: false,
      replayDeclined: false
    }
  },


  created() {
    this.resources.cellIcons = Resources.getCellIcons()

    this.$store.dispatch('invites/initialize')

    let url = '/game'
    if (debug)
      url = 'http://localhost:45353/game'

    this.hubConnection = new HubConnectionBuilder()
      .withUrl(url)
      .configureLogging(LogLevel.Information)
      .build()

    this.hubConnection.serverTimeoutInMilliseconds = 12 * 1000

    this.hubConnection.on('MoveRecieved', index=>{
      this.$store.commit('gameEntity/makeMove', index)
    })

    this.hubConnection.on('GameEnded', conditions=>{
      this.$store.dispatch('gameEntity/finishGame', conditions)
    })

    this.hubConnection.on('GameCreated', (crossesId, noughtsId, settings) => {
      let isMyTurn
      if (crossesId === this.myId) {
        isMyTurn = true
      } else {
        isMyTurn = false
      }

      this.createGame(isMyTurn, settings)
    })


    this.hubConnection.on('MyIdAndAvaliablePlayers', (myId, players)=> {
      this.myId = myId

      this.$store.dispatch('invites/updateAvaliablePlayers', {players: players, myId: myId})
    })

    this.hubConnection.on('AvaliablePlayersUpdtated', players => {
      this.$store.dispatch('invites/updateAvaliablePlayers', {players: players, myId: this.myId})
    })

    this.hubConnection.on('InviteRequested', (opponentId, settings) => {
      this.$store.commit('invites/addInvite', {id: opponentId, settings: settings})
    })

    this.hubConnection.on('InviteRemoved', (opponentId) => {
      this.$store.dispatch('invites/removeInvite', opponentId)
    })

    
    this.hubConnection.on('OpponentExited', () => {
      this.opponentExited = true
    })

    this.hubConnection.on('ReplayRequested', () => {
      this.replayOffered = true
    })

    this.hubConnection.on('ReplayDeclined', () => {
      this.replayDeclined = true
    })

    this.hubConnection.start()
  },


  methods: {
    cellClickHandler(index) {
      this.$store.commit('gameEntity/makeMove', index)

      this.hubConnection.invoke('SendMove', index)
    },
    

    sizesClickHandler(settings) {
      this.$store.commit('invites/setMyPreferredSettings', settings)
    },


    exitClickHandler() {
      this.lookingForGame = true

      this.$store.commit('invites/reset')

      this.hubConnection.invoke('ResumeGameSearch')
    },


    inviteClickHandler(opponentId) {
      this.hubConnection.invoke('SendInviteRequest', opponentId, this.$store.state.invites.myPreferredSettings)
    },

    acceptClickHandler(opponentId) {
      this.hubConnection.invoke('SendInviteAccept', opponentId)
    },

    declineClickHandler(opponentId) {
      this.hubConnection.invoke('SendInviteDecline', opponentId)
    },


    offerReplayClickHandler() {
      this.replayDeclined = false

      this.hubConnection.invoke('SendReplayRequest')
    },

    acceptReplayClickHandler() {  
      this.hubConnection.invoke('SendReplayAccept')
    },

    declineReplayClickHandler() {
      this.replayOffered = false

      this.hubConnection.invoke('SendReplayDecline')
    },


    createGame(isMyTurn, settings) {
      this.lookingForGame = false

      this.opponentExited = false
      this.replayOffered = false
      this.replayDeclined = false

      this.$store.dispatch('gameEntity/newGame', {settings: settings, isMyTurn: isMyTurn})
    }
  }
}
</script>

<style>
  #game-manager {
    background: burlywood;
    width: 91%;
    padding-bottom: 130%;
    position: relative;
  }

  #game-field, #invites-control {
    background: burlywood;
    width: 100%;
    height: 70%;
    position: absolute;
  }

  #game-control, #settings-control {
    font-family: Verdana;
    color: forestgreen;
    background: burlywood;

    display: flex;
    flex-direction: column;
    justify-content: space-between;

    width: 100%;
    height: 30%;
    position: absolute;
    top: 70%;
  }

  @media (orientation: landscape) {
    #game-manager {
      width: 100%;
      padding-bottom: 70%;
    }

    #game-field, #invites-control {
      width: 70%;
      height: 100%;
    }

    #game-control, #settings-control {
      width: 30%;
      height: 100%;
      top: 0%;
      left: 70%;
    }
  }
</style>