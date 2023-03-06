<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";
import { Getter } from "vuex-class";

import type { WebClientConfiguration } from "@/models/configData";
import DependentManagementView from "@/views/DependentManagementView.vue";
import DependentsView from "@/views/DependentsView.vue";

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const options: any = {
    components: {
        DependentManagementView,
        DependentsView,
    },
};

@Component(options)
export default class DependentViewSelectorComponent extends Vue {
    @Getter("webClient", { namespace: "config" })
    private config!: WebClientConfiguration;

    private get timelineEnabled(): boolean {
        return this.config.featureToggleConfiguration.dependents
            .timelineEnabled;
    }
}
</script>
<template>
    <DependentManagementView v-if="timelineEnabled" />
    <DependentsView v-else />
</template>
