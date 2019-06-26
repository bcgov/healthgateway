import Vue from 'vue';
import { Component } from 'vue-property-decorator';

interface WeatherForecast {
    dateFormatted: string;
    temperatureC: number;
    temperatureF: number;
    summary: string;
}

interface PatientData {
    name: string;
    id: string;
    divStruct: string;
}

@Component
export default class FetchDataComponent extends Vue {
    //forecasts: WeatherForecast[] = [];

    patients: PatientData[] = [];

    mounted() {
        /*fetch('api/SampleData/WeatherForecasts')
            .then(response => response.json() as Promise<WeatherForecast[]>)
            .then(data => {
                this.forecasts = data;
            });*/
        fetch('api/SampleData/GetPatients')
            .then(response => response.json() as Promise<PatientData[]>)
            .then(data => {
                this.patients = data;
            });
    }
}