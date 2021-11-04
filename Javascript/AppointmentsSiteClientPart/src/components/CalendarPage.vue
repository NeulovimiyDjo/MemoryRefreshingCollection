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
            <option v-for="doctor in db.doctors" :key="doctor.name" :value="doctor.name">
              {{doctor.name}}
            </option>
          </b-form-select>
        </b-col>
      </b-row>
    </b-container>

    <b-table striped :items="timeRanges" :fields="fields" responsive class="text-nowrap">
      <template slot="date-msg" slot-scope="data">
        {{ dateMsg(data.item.date) }}
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
  name: 'CalendarPage',

  data() {
    return {
      db: {},
      currentDoctorName: '',
      allTimeRanges: [],


      fields: [
        { key: 'date-msg', label: 'Дата' },
        { key: 'start', label: 'Начало' },
        { key: 'end', label: 'Конец' },
        { key: 'status', label: 'Статус' },
      ]  
    }
  },

  computed: {
    timeRanges() {
      let res = this.allTimeRanges.filter(tr => (this.currentDoctorName === '' || this.currentDoctorName === tr.doctor))
      
      if (res.length === 0) return []


      let daySeparators = []
      let lastDate = res[0].date
      res.forEach(t => {
        if (t.date !== lastDate) {
          daySeparators.push({
            date: '',
            start: '',
            end: '',
            doctor: '',
            status: '',
            startStamp: Number(new Date(t.date + 'T' + '04:00')),
            endStamp: Number(new Date(t.date + 'T' + '04:00')),
            _rowVariant: 'success'
          })

          lastDate = t.date
        }
      })


      res = res.concat(daySeparators).sort((a, b) => {
        return a.startStamp - b.startStamp
      })

      return res
    }
  },
  
  created(){
    api.getDb()
    .then(db => {
      this.db = db
      console.log(db)

      let res = []
      this.db.doctors.forEach(d => {
        res =  res.concat(this.appointments(d.name),this.freeTimes(d.name))
      })

      this.allTimeRanges = res.sort((a, b) => {
        return a.startStamp - b.startStamp
      })
    })
  },

  methods: {
    dateMsg(date) {
      if (date === '') return ''

      let d = new Date(date)
      return d.getDate() + '.' + appendLeadingZeroes(d.getMonth() + 1) + '.' + d.getFullYear()
    },

    doctorChanged(event) {
      this.currentDoctorName = event
    },


    appointments(doctorName) {
      if (!this.db.appointments) return []

      let aps = this.db.appointments.filter(a => a.status === "Confirmed" && a.doctor === doctorName).map(a => {
        return {
          date: a.date,
          start: a.start,
          end: a.end,
          doctor: a.doctor,
          status: a.patient,
          startStamp: Number(new Date(a.date + 'T' + a.start)),
          endStamp: Number(new Date(a.date + 'T' + a.end))
        }
      })

      return aps.sort((a, b) => {
        return a.startStamp - b.startStamp
      })
    },

    freeTimes(doctorName) {
      if (!this.db.freeTimes) return []

      let fts = this.db.freeTimes.filter(ft => ft.doctor === doctorName).map(t => {
        return {
          date: t.date,
          start: t.start,
          end: t.end,
          doctor: doctorName,
          startStamp: Number(new Date(t.date + 'T' + t.start)),
          endStamp: Number(new Date(t.date + 'T' + t.end))
        }
      }).sort((a, b) => {
        return a.startStamp - b.startStamp
      })


      let res = []

      fts.forEach(ft => {
        let sStamp = ft.startStamp
        let s = ft.start

        this.appointments(doctorName).forEach(a => {
          if (a.endStamp <= ft.startStamp || a.startStamp >= ft.endStamp) {
            return // continue
          }

          if (sStamp + 5 * 60 * 1000 < a.startStamp) {
            res.push({
              date: ft.date,
              start: s,
              end: a.start,
              doctor: doctorName,
              status: 'Свободно',
              startStamp: sStamp,
              endStamp: a.startStamp
            })
          }

          sStamp = a.endStamp
          s = a.end
        })

        if (sStamp + 5 * 60 * 1000 < ft.endStamp) {
          res.push({
            date: ft.date,
            start: s,
            end: ft.end,
            doctor: doctorName,
            status: 'Свободно',
            startStamp: sStamp,
            endStamp: ft.endStamp
          })
        }
      })

      return res
    }
  }
}
</script>