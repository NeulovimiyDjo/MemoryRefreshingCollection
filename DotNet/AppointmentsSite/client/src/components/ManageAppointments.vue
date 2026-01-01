<template>
  <div class="blue-form">
    <br />
    <b-container>
      <b-row class="my-1">
        <b-col cols="3">
          <label for="radios">Статус:</label>
        </b-col>
        <b-col cols="9">
          <b-form-radio-group id="radios"
            v-model="selected"
            :options="options"
            name="radio-inline">
          </b-form-radio-group>
        </b-col>
      </b-row>
      <b-row class="my-1">
        <b-col cols="3">
          <label for="doctor">Врач:</label>
        </b-col>
        <b-col cols="9">
          <b-form-select id="doctor" :value="currentDoctorName" @change="doctorChanged($event)">
            <option value="">
              Все
            </option>
            <option v-for="doctor in doctors" :key="doctor.name" :value="doctor.name">
              {{doctor.name}}
            </option>
          </b-form-select>
        </b-col>
      </b-row>
    </b-container>

    <b-table :items="filteredAppointments" :fields="fields" responsive class="text-nowrap">
      <template slot="status-msg" slot-scope="data">
        {{ statusMsg(data.item.status) }}
      </template>
      <template slot="date-msg" slot-scope="data">
        {{ dateMsg(data.item.date) }}
      </template>
      <template slot="actions" slot-scope="row">
        <b-button size="sm" @click="editHandler(row.item)" class="mr-2">Изменить</b-button>
        <b-button size="sm" @click="confirmHandler(row.item)" class="mr-2"
          :disabled="row.item.status !== 'Unconfirmed'">Подтвердить</b-button>
        <b-button size="sm" @click="removeHandler(row.item)">Удалить</b-button>
      </template>
    </b-table>
  </div>
</template>

<script>
import api from '../api/api.js'

function appendLeadingZeroes(n){
  if(n <= 9) {
    return "0" + n;
  }
  
  return n
}

export default {
  name: 'ManageAppointments',

  data() {
    return {
      appointments: [],
      doctors: [],
      currentDoctorName: '',

      fields: [
          { key: 'doctor', label: 'Врач' },
          { key: 'patient', label: 'ФИО пациента' },
          { key: 'date-msg', label: 'Дата' },
          { key: 'start', label: 'Начало'},
          { key: 'duration', label: 'Длительность' },
          { key: 'status-msg', label: 'Подтверждено' },
          //{ key: 'created', label: 'Создан'},
          { key: 'actions', label: 'Действия' }
        ],

      selected: 'all',
      options: [
        { text: 'Все', value: 'all' },
        { text: 'Подтвержденные', value: 'Confirmed' },
        { text: 'Неподтвержденные', value: 'Unconfirmed' }
      ]
    }
  },

  computed: {
    filteredAppointments() {
      return this.appointments.filter(a => (this.currentDoctorName === '' || this.currentDoctorName === a.doctor) &&
        (this.selected === 'all' || this.selected === a.status))
    }
  },
  
  created() {
    api.getDb()
    .then(db => {
      this.appointments = db.appointments.sort((a, b) => {
        return Number(new Date(a.date + 'T' + a.start)) - Number(new Date(b.date + 'T' + b.start))
      })
      this.doctors = db.doctors
      console.log(db)
    })
  },

  methods: {
    statusMsg(status) {
      if (status === "Confirmed") return "Да"
      else return "Нет"
    },

    dateMsg(date) {
      let d = new Date(date)
      return d.getDate() + '.' + appendLeadingZeroes(d.getMonth() + 1) + '.' + d.getFullYear()
    },

    doctorChanged(event) {
      this.currentDoctorName = event
    },

    editHandler(appointment) {
      this.$router.push({name: "EditPage", params: {id: String(appointment.id)}})
    },

    confirmHandler(appointment) {
      let info = {
        id: appointment.id,
        rowVersion: appointment.rowVersion,
        date: appointment.date,
        start: appointment.start,
        duration: appointment.duration
      }

      api.confirmAppointment(info)
      .then(response => {
        if (response.status === 'Confirmed') {
          this.appointments.find(a => a.id === appointment.id).status = 'Confirmed'
          this.appointments.find(a => a.id === appointment.id).rowVersion = response.newRowVersion
        } else if (response === 'Intersection in time') {
          alert('Этот промежуток времени пересекается с другой подтвержденной записью')
        } else if (response === 'Fail') {
          alert('Fail: Item was modified since last page load')
        }
      })
    },

    removeHandler(appointment) {
      let info = {
        id: appointment.id,
        rowVersion: appointment.rowVersion
      }

      api.removeAppointment(info)
      .then(response => {
        if (response === 'Removed') {
          this.appointments.splice(this.appointments.findIndex(a => a.id === appointment.id), 1)
        } else if (response === 'Fail') {
          alert('Fail: Item was modified since last page load')
        }
      })
    }
  }
}
</script>
