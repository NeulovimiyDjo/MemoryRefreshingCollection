<template>
  <div class="blue-form">
    <br />
    <b-container>
      <b-row class="my-1">
        <b-col cols="3">
          <label for="doctor">Врач:</label>
        </b-col>
        <b-col cols="9">
          <b-form-select id="doctor" :value="currentDoctorName" @change="doctorChanged($event)" :disabled="freeTime.id != -1">
            <option value="" disabled="true">
              Выберите врача
            </option>
            <option v-for="doctor in doctors" :key="doctor.name" :value="doctor.name">
              {{doctor.name}}
            </option>
          </b-form-select>
        </b-col>
      </b-row>

      <b-row class="my-1">
        <b-col cols="3">
          <label for="date">Дата:</label>
        </b-col>
        <b-col cols="9">
          <b-form-input id="date" v-model="freeTime.date" type="date" :disabled="currentDoctorName === ''"></b-form-input>
        </b-col>
      </b-row>
      <b-row class="my-1">
        <b-col cols="3">
          <label for="start">Время начала:</label>
        </b-col>
        <b-col cols="9">
          <b-form-input id="start" v-model="freeTime.start" type="time" :disabled="currentDoctorName === ''"></b-form-input>
        </b-col>
      </b-row>
      <b-row class="my-1">
        <b-col cols="3">
          <label for="end">Время конца:</label>
        </b-col>
        <b-col cols="9">
          <b-form-input id="end" v-model="freeTime.end" type="time" :disabled="currentDoctorName === ''"></b-form-input>
        </b-col>
      </b-row>
      <b-row class="my-4">
        <b-col cols="12">
          <b-button variant="success" @click="addHandler">{{buttonName}}</b-button>
          <b-button variant="success" @click="cancelHandler" class="ml-1">Отмена</b-button>
        </b-col>
      </b-row>
    </b-container>
  </div>
</template>

<script>
import api from '../api/api.js'

function calcualteIntersectedAppointments(appointments, ft) {
  let aps = appointments.filter(a => a.doctor === ft.doctor && a.date === ft.date).map(a => {
    return {
      date: a.date,
      start: a.start,
      end: a.end,
      status: a.patient,
      startStamp: Number(new Date(a.date + 'T' + a.start)),
      endStamp: Number(new Date(a.date + 'T' + a.end))
    }
  })
  .sort((a, b) => {
    return a.startStamp - b.startStamp
  })


  let ftStartStamp = Number(new Date(ft.date + 'T' + ft.start))
  let ftEndStamp = Number(new Date(ft.date + 'T' + ft.end))

  let intersectedAppointments = []

  aps.forEach(a => {
    if (!(a.endStamp <= ftStartStamp || a.startStamp >= ftEndStamp)) {  // a and ft intersect in time
      intersectedAppointments.push(a)
    }
  })

  return intersectedAppointments
}

export default {
  name: 'EditPage',

  data() {
    return {
      appointments: [],
      freeTimes: [],
      doctors: [],

      freeTime: {},

      currentDoctorName: ''
    }
  },

  computed: {
    buttonName() {
      if (this.freeTime.id === -1) return 'Добавить'
      else return 'Сохранить'
    }
  },
  
  created(){
    api.getDb()
    .then(db => {
      this.freeTimes = db.freeTimes
      this.doctors = db.doctors
      this.appointments = db.appointments.filter(a => a.status === "Confirmed")
      console.log(db)

      let ft = db.freeTimes.find(ft => ft.id === Number(this.$route.params.id))
      if (ft) {
        this.currentDoctorName = ft.doctor
      } else {
        ft = {id: -1}
      }

      this.freeTime = JSON.parse(JSON.stringify(ft))
    })
  },

  methods: {
    doctorChanged(event) {
      this.currentDoctorName = event
    },

    addHandler() {
      if (this.freeTime.id === -1) {
        this.sendAdd()
      } else {
        let oldFt = this.freeTimes.find(ft => ft.id === Number(this.$route.params.id))
        let newFt = this.freeTime

        let oldFtStartStamp = Number(new Date(oldFt.date + 'T' + oldFt.start))
        let oldFtEndStamp = Number(new Date(oldFt.date + 'T' + oldFt.end))

        let newFtStartStamp = Number(new Date(newFt.date + 'T' + newFt.start))
        let newFtEndStamp = Number(new Date(newFt.date + 'T' + newFt.end))



        let intersectedAppointments = []
        if (oldFtEndStamp <= newFtStartStamp || oldFtStartStamp >= newFtEndStamp) // don't intersect
        {
          intersectedAppointments = calcualteIntersectedAppointments(this.appointments, oldFt) // check oldFt which is getting deleted
        } 
        else
        {
          let intersectedLeft = []
          if (oldFtStartStamp <= newFtStartStamp) {
            let deletedLeft = { doctor: oldFt.doctor, date: oldFt.date, start: oldFt.start, end: newFt.start }
            intersectedLeft = calcualteIntersectedAppointments(this.appointments, deletedLeft)
          }

          let intersectedRight = []
          if (oldFtEndStamp >= newFtEndStamp) {
            let deletedRight = { doctor: oldFt.doctor, date: oldFt.date, start: newFt.end, end: oldFt.end }
            intersectedRight = calcualteIntersectedAppointments(this.appointments, deletedRight)
          }

          intersectedAppointments = intersectedLeft.concat(intersectedRight)
        }



        if (intersectedAppointments.length > 0) {
          this.$bvModal.msgBoxConfirm('В удаленной части старого свободного времени уже имеются подтвержденные записи.\nПродолжить?', {
            title: 'Подтвердите изменение',
            size: 'sm',
            buttonSize: 'sm',
            okVariant: 'danger',
            okTitle: 'Да',
            cancelTitle: 'Нет',
            footerClass: 'p-2',
            hideHeaderClose: false,
            centered: true,
            noCloseOnBackdrop: true,
            noCloseOnEsc: true
          })
          .then(value => {
            if (value) {
              this.sendAdd()
            }
          })
        } else {
          this.sendAdd()
        }
      }
    },

    sendAdd() {
      let info = {
        id: this.freeTime.id,
        rowVersion: this.freeTime.rowVersion,
        doctor: this.freeTime.id == -1 ? this.currentDoctorName : this.freeTime.doctor,
        date: this.freeTime.date,
        start: this.freeTime.start,
        end: this.freeTime.end
      }

      api.addFreeTime(info)
      .then(response => {
        if (response === 'Success') {
          this.$router.push('/ManageFreeTime')
        } else if (response === 'Invalid info') {
          alert('Некорректная информация')
        } else if (response === 'Fail') {
          alert('Fail: Item was modified since last page load')
        }
      })
    },

    cancelHandler() {
      this.$router.push('/ManageFreeTime')
    }
  }
}
</script>