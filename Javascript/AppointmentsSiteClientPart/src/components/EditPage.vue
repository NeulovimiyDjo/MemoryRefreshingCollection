<template>
  <div class="blue-form">
    <br />
    <b-container>
      <b-row class="my-1">
        <b-col cols="3">
          <label for="patient">ФИО пациента:</label>
        </b-col>
        <b-col cols="9">
          <b-form-input id="patient" v-model="appointment.patient" type="text" disabled></b-form-input>
        </b-col>
      </b-row>
       <b-row class="my-1">
        <b-col cols="3">
          <label for="phone">Номер телефона:</label>
        </b-col>
        <b-col cols="9">
          <b-form-input id="phone" v-model="appointment.phone" type="text" disabled></b-form-input>
        </b-col>
      </b-row>
      <b-row class="my-1">
        <b-col cols="3">
          <label for="doctor">Врач:</label>
        </b-col>
        <b-col cols="9">
          <b-form-input id="doctor" v-model="appointment.doctor" type="text" disabled></b-form-input>
        </b-col>
      </b-row>
      <b-row class="my-1">
        <b-col cols="3">
          <label for="date">Дата:</label>
        </b-col>
        <b-col cols="9">
          <b-form-input id="date" v-model="appointment.date" type="date"></b-form-input>
        </b-col>
      </b-row>
      <b-row class="my-1">
        <b-col cols="3">
          <label for="start">Начало:</label>
        </b-col>
        <b-col cols="9">
          <b-form-input id="start" v-model="appointment.start" type="time"></b-form-input>
        </b-col>
      </b-row>
      <b-row class="my-1">
        <b-col cols="3">
          <label for="duration">Длительность (в минутах):</label>
        </b-col>
        <b-col cols="9">
          <b-form-input id="duration" v-model="appointment.duration" type="number"></b-form-input>
        </b-col>
      </b-row>
      <b-row class="my-1">
        <b-col cols="3">
          <label for="info">Дополнительная информация:</label>
        </b-col>
        <b-col cols="9">
          <b-form-textarea id="info" v-model="appointment.info" type="text"></b-form-textarea>
        </b-col>
      </b-row>
      <b-row class="my-4">
        <b-col cols="12">
          <b-button variant="success" @click="confirmHandler">{{buttonName}}</b-button>
          <b-button variant="success" @click="cancelHandler" class="ml-1">Отмена</b-button>
        </b-col>
      </b-row>
    </b-container>
  </div>
</template>

<script>
import api from '../api/api.js'

export default {
  name: 'EditPage',

  data() {
    return {
      appointments: [],

      appointment: {}
    }
  },

  computed: {
    buttonName() {
      if (this.appointment.status === "Unconfirmed") return 'Подтвердить'
      else return 'Сохранить'
    }
  },
  
  created(){
    api.getDb()
    .then(db => {
      this.appointments = db.appointments
      console.log(db)
      this.appointment = JSON.parse(JSON.stringify(db.appointments.find(a => a.id === Number(this.$route.params.id))))
    })
  },

  methods: {
    confirmHandler() {
      let info = {
        id: this.appointment.id,
        rowVersion: this.appointment.rowVersion,
        date: this.appointment.date,
        start: this.appointment.start,
        duration: this.appointment.duration,
        info: this.appointment.info
      }

      api.confirmAppointment(info)
      .then(response => {
        if (response.status === 'Confirmed') {
            this.$router.push('/ManageAppointments')
        } else if (response === 'Intersection in time') {
          alert('Этот промежуток времени пересекается с другой подтвержденной записью')
        } else if (response === 'Fail') {
          alert('Fail: Item was modified since last page load')
        }
      })
    },

    cancelHandler() {
      this.$router.push('/ManageAppointments')
    }
  }
}
</script>