<template>
  <div class="blue-form">
    <br />
    <b-container>
      <b-row class="my-1">
        <b-col cols="3">
          <label for="patient">ФИО пациента:</label>
        </b-col>
        <b-col cols="9">
          <b-form-input id="patient" v-model="patientName" type="text"></b-form-input>
        </b-col>
      </b-row>
      <b-row class="my-1">
        <b-col cols="3">
          <label for="phone">Номер телефона:</label>
        </b-col>
        <b-col cols="9">
          <b-form-input id="phone" v-model="patientPhone" type="tel"></b-form-input>
        </b-col>
      </b-row>
      <b-row class="my-1">
        <b-col cols="3">
          <label for="doctor">Врач:</label>
        </b-col>
        <b-col cols="9">
          <b-form-select id="doctor" :value="doctorName" @change="doctorChanged($event)">
            <option value="" disabled="true">
              Выберите врача
            </option>
            <option v-for="d in doctorsNamesList" :key="d" :value="d">
              {{d}}
            </option>
          </b-form-select>
        </b-col>
      </b-row>
      <b-row class="my-1">
        <b-col cols="3">
          <label for="date">Дата:</label>
        </b-col>
        <b-col cols="9">
          <b-form-select id="date" :disabled="doctorName === ''" :value="date" @change="dateChanged($event)">
            <option value="" disabled="true">
              Выберите дату
            </option>
            <option v-for="date in datesList" :key="date" :value="date">
              {{dateMsg(date)}}
            </option>
          </b-form-select>
        </b-col>
      </b-row>
      <b-row class="my-1">
        <b-col cols="3">
          <label for="time">Время:</label>
        </b-col>
        <b-col cols="9">
          <b-form-select id="time" :disabled="date === ''" :value="time" @change="timeChanged($event)">
            <option value="" disabled="true">
              Выберите время
            </option>
            <option v-for="timeObj in timeObjectsList" :key="timeObj.time" :value="timeObj.time">
              {{timeObj.time}}
            </option>
          </b-form-select>
        </b-col>
      </b-row>
      <b-row class="my-4">
        <b-col cols="12">
          <b-button id="send" variant="success" @click="btn1ClickHandler">Отправить</b-button>
        </b-col>
      </b-row>

      <b-popover target="teste" :show.sync="popoverShow" placement="topright">
        <template slot="title">
          <b-button @click="onClose" class="close" aria-label="Close">
            <span class="d-inline-block" aria-hidden="true">&times;</span>
          </b-button>
          Notification title
        </template>

        <div>
          Обработка информации Обработка информации Обработка информации Обработка информации Обработка информации Обработка информации Обработка информации
          <br />
          <b-button @click="onOk" size="sm" variant="primary" class="float-right">Ok</b-button>
        </div>
      </b-popover>

    </b-container>
    <div id ="teste" class="popover-anchor fixed-bottom"></div>
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
  name: 'SetAppointment',

  data() {
    return {
      popoverShow: false,


      patientName: '',
      patientPhone: '',

      doctorName: '',
      date: '',
      time: '',

      dateTimes: {}
    }
  },

  computed: {
    doctorsNamesList() {
      return Object.keys(this.dateTimes)
    },

    datesList() {
      if (this.doctorName === '') return []

      return [...new Set(this.dateTimes[this.doctorName].map(dt => dt.date))]
    },

    timeObjectsList() {
      if (this.doctorName === '') return []

      return this.dateTimes[this.doctorName].filter(dt => dt.date === this.date).map(dt => {
        return {
          time: dt.time,
          label: dt.time + ' - ' + dt.endTime
        }
      })
    }
  },

  created() {
    api.getAvailableDateTimes()
    .then(dateTimes => {
      this.dateTimes = dateTimes
      console.log(dateTimes)
    })
  },

  mounted() {
    setTimeout(() => {
      this.popoverShow = true
    }, 1000)
  },

  methods: {
    dateMsg(date) {
      let d = new Date(date)
      return d.getDate() + '.' + appendLeadingZeroes(d.getMonth() + 1) + '.' + d.getFullYear()
    },

    onClose() {
      this.popoverShow = false
    },

    onOk() {
      this.popoverShow = false
    },


    doctorChanged(event) {
      this.doctorName = event
      this.date = ''
      this.time = ''
    },

    dateChanged(event) {
      this.date = event
      this.time = ''
    },

    timeChanged(event) {
      this.time = event
    },

    btn1ClickHandler() {
      let info = {
        patient: this.patientName,
        phone: this.patientPhone,
        doctor: this.doctorName,
        date: this.date,
        time: this.time
      }

      api.setAppointment(info).then(response => {
        if (response === 'Created') {
          this.$router.push('/AppointmentRegistered')
        }
        else if (response === 'Invalid info') {
          alert('Некорректная информация')
        }
      })
    }
  }
}
</script>
