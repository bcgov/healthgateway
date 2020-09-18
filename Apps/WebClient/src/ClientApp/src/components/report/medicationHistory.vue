<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";
#pageTitle,
label {
    color: $primary;
    hr {
        border-top: 2px solid $primary;
    }
}

label {
    font-weight: bold;
}

.header {
    background-color: $soft_background;
    font-weight: bold;
    font-size: 0.6em;
}

.item {
    font-size: 0.6em;
    border-bottom: solid 1px $soft_background;
    page-break-inside: avoid;
}
</style>
<template>
    <div>
        <div id="pageTitle">
            <h2 id="subject">Health Gateway Medication History Report</h2>
            <hr class="mb-0" />
        </div>
        <b-row class="pt-2">
            <b-col><label>Name:</label> {{ name }}</b-col>
        </b-row>
        <b-row class="pt-2">
            <b-col><label>Date Reported:</label> {{ currentDate }}</b-col>
        </b-row>

        <b-row v-if="isEmpty" class="mt-2">
            <b-col>No records found.</b-col>
        </b-row>
        <b-row v-else class="pt-2 header">
            <b-col class="col-1">Date</b-col>
            <b-col class="col-1">DIN/PIN</b-col>
            <b-col class="col-2">Brand</b-col>
            <b-col class="col-2">Generic</b-col>
            <b-col class="col-1">Practitioner</b-col>
            <b-col class="col-1">Quantity</b-col>
            <b-col class="col-1">Strength</b-col>
            <b-col class="col-1">Form</b-col>
            <b-col class="col-1">Manufacturer</b-col>
        </b-row>
        <b-row
            v-for="item in medicationStatementHistory"
            :key="item.prescriptionIdentifier + item.dispensedDate"
            class="item pt-1"
        >
            <b-col class="col-1">{{ formatDate(item.dispensedDate) }}</b-col>
            <b-col class="col-1">{{ item.medicationSummary.din }}</b-col>
            <b-col class="col-2">{{ item.medicationSummary.brandName }}</b-col>
            <b-col class="col-2">{{
                item.medicationSummary.genericName
            }}</b-col>
            <b-col class="col-1">{{ item.practitionerSurname }}</b-col>
            <b-col class="col-1">{{ item.medicationSummary.quantity }}</b-col>
            <b-col class="col-1"
                >{{ item.medicationSummary.strength }}
                {{ item.medicationSummary.strengthUnit }}</b-col
            >
            <b-col class="col-1">{{ item.medicationSummary.form }}</b-col>
            <b-col class="col-1">{{
                item.medicationSummary.manufacturer
            }}</b-col>
        </b-row>
    </div>
</template>

<script lang="ts">
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";
import MedicationStatementHistory from "@/models/medicationStatementHistory";
import moment from "moment";

@Component
export default class MedicationHistoryReportComponent extends Vue {
    @Prop() private medicationStatementHistory!: MedicationStatementHistory[];
    @Prop() private name!: string;

    private get currentDate() {
        return moment(new Date()).format("ll");
    }

    private get isEmpty() {
        return this.medicationStatementHistory.length == 0;
    }

    private formatDate(date: Date): string {
        return moment(date).format("yyyy-MM-DD");
    }
}
</script>
