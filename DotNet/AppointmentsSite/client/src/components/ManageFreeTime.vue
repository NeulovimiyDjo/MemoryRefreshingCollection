<template>
  <div class="blue-form">
    <br />
    <b-container>
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
      <b-row class="my-1">
        <b-col cols="12">
          <b-button size="sm" @click="addHandler" class="mr-2">Добавить</b-button>
        </b-col>
      </b-row>
    </b-container>

    <b-table :items="filteredFreeTimes" :fields="fields" responsive class="text-nowrap">
      <template slot="date-msg" slot-scope="data">
        {{ dateMsg(data.item.date) }}
      </template>
      <template slot="actions" slot-scope="row">
        <b-button size="sm" @click="editHandler(row.item)" class="mr-2">Изменить</b-button>
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
  name: 'ManageFreeTime',

  data() {
    return {
      appointments: [],
      freeTimes: [],
      doctors: [],
      currentDoctorName: '',

      fields: [
          { key: 'doctor', label: 'Врач'},
          { key: 'date-msg', label: 'Дата'},
          { key: 'start', label: 'Начало'},
          { key: 'end', label: 'Конец' },
          { key: 'actions', label: 'Действия' }
        ],
    }
  },

  computed: {
    filteredFreeTimes() {
      return this.freeTimes.filter(a => (this.currentDoctorName === '' || this.currentDoctorName === a.doctor))
    }
  },
  
  created(){
    api.getDb()
    .then(db => {
      this.freeTimes = db.freeTimes.sort((a, b) => {
        return Number(new Date(a.date + 'T' + a.start)) - Number(new Date(b.date + 'T' + b.start))
      })

      this.appointments = db.appointments.filter(a => a.status === "Confirmed")
      this.doctors = db.doctors
      console.log(db)
    })
  },

  methods: {
    dateMsg(date) {
      let d = new Date(date)
      return d.getDate() + '.' + appendLeadingZeroes(d.getMonth() + 1) + '.' + d.getFullYear()
    },

    doctorChanged(event) {
      this.currentDoctorName = event
    },

    addHandler(freeTime) {
      this.$router.push("/FreeTimePage")
    },

    editHandler(freeTime) {
      this.$router.push({name: "FreeTimePage", params: {id: String(freeTime.id)}})
    },

    removeHandler(freeTime) {
      let intersectedAppointments = calcualteIntersectedAppointments(this.appointments, freeTime)

      if (intersectedAppointments.length > 0) {
        this.$bvModal.msgBoxConfirm('На это свободное время уже имеются подтвержденные записи.\nПродолжить?', {
          title: 'Подтвердите удаление',
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
            this.sendRemove(freeTime)
          }
        })
      } else {
        this.sendRemove(freeTime)
      }
    },

    sendRemove(freeTime) {
      let info = {
        id: freeTime.id,
        rowVersion: freeTime.rowVersion
      }

      api.removeFreeTime(info)
      .then(response => {
        if (response === 'Removed') {
          this.freeTimes.splice(this.freeTimes.findIndex(a => a.id === freeTime.id), 1)
        } else if (response === 'Fail') {
          alert('Fail: Item was modified since last page load')
        }
      })
    }
  }
}
</script>
