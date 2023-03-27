<script lang="ts">
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";

import ReportsComponent from "@/components/report/ReportsComponent.vue";
import type { Dependent } from "@/models/dependent";

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const options: any = {
    components: {
        ReportsComponent,
    },
};

@Component(options)
export default class DependentProfileTabComponent extends Vue {
    @Prop({ required: true })
    private dependent!: Dependent;
}
</script>
<template>
    <div>
        <ReportsComponent
            :hdid="dependent.dependentInformation.hdid"
            :is-dependent="true"
        />
    </div>
</template>
