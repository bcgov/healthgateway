<script lang="ts">
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";

import DependentDashboardTabComponent from "@/components/dependent/tabs/DependentDashboardTabComponent.vue";
import type { Dependent } from "@/models/dependent";
import DependentUtil from "@/utility/dependentUtil";

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const options: any = {
    components: {
        DependentDashboardTabComponent,
    },
};

@Component(options)
export default class DependentCardComponent extends Vue {
    @Prop({ required: true })
    private dependent!: Dependent;

    private get formattedName(): string {
        return DependentUtil.formatName(this.dependent.dependentInformation);
    }
}
</script>
<template>
    <div>
        <div>
            {{ formattedName }}
        </div>
        <DependentDashboardTabComponent :dependent="dependent" />
    </div>
</template>
