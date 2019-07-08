import Vue from 'vue';
import { Component } from 'vue-property-decorator';

interface Immunization {
    date: string;
    vaccine: string;
    lot: string;
    dose: string;
    booster: string;
    site: string;
}

@Component
export default class ImmunizationsComponent extends Vue {

    immunizations: Immunization[] = [];

    /** 
    mounted() {

        fetch('api//GetPatients')
            .then(response => response.json() as Promise<Immunization[]>)
            .then(data => {
                this.patients = data;
            });
    } */
}