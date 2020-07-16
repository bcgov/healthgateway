<template>
    <div>
        <div v-for="year in years" :key="year">
            <button>{{ year }}</button>
        </div>
    </div>
</template>

<script lang="ts">
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";

@Component
export default class MonthYearPickerComponent extends Vue {
    private years: number[] = [
        2012,
        2013,
        2014,
        2015,
        2016,
        2017,
        2018,
        2019,
        2020,
    ];
}
</script>
