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