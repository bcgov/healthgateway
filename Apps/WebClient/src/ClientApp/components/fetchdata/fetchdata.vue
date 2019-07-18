<template>
    <div>
        <!--
        <h1>Weather forecast</h1>

        <p>This component demonstrates fetching data from the server.</p>

        <table v-if="forecasts.length" class="table">
            <thead>
                <tr>
                    <th>Date</th>
                    <th>Temp. (C)</th>
                    <th>Temp. (F)</th>
                    <th>Summary</th>
                </tr>
            </thead>
            <tbody>
                <tr v-for="item in forecasts">
                    <td>{{ item.dateFormatted }}</td>
                    <td>{{ item.temperatureC }}</td>
                    <td>{{ item.temperatureF }}</td>
                    <td>{{ item.summary }}</td>
                </tr>
            </tbody>
        </table>
        -->

        <h1>Patient Search</h1>

        <p>This component demonstrates fetching data from the server.</p>

        <table v-if="patients.length" class="table">
            <thead>
                <tr>
                    <th>Id</th>
                    <th>Name</th>
                    <th>Narrative contents</th>
                </tr>
            </thead>
            <tbody>
                <tr v-for="item in patients">
                    <td>{{ item.id }}</td>
                    <td>{{ item.name }}</td>
                    <td v-html= "item.divStruct">{{ test }}</td>
                </tr>
            </tbody>
        </table>

        <p v-else><em>Loading...</em></p>
    </div>
</template>

<script lang="ts">
import { Vue, Component} from 'vue-property-decorator';

interface PatientData {
    name: string;
    id: string;
    divStruct: string;
}

@Component
export default class FetchDataComponent extends Vue {
    patients: PatientData[] = [];

    mounted() {
        fetch('api/SampleData/GetPatients')
            .then(response => response.json() as Promise<PatientData[]>)
            .then(data => {
                this.patients = data;
            });
    }
}
</script>

