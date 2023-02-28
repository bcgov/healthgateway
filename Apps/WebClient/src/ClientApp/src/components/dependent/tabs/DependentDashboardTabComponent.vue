<script lang="ts">
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";

import type { Dependent } from "@/models/dependent";

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const options: any = {
    components: {},
};

@Component(options)
export default class DependentDashboardTabComponent extends Vue {
    @Prop({ required: true })
    private dependent!: Dependent;
}
</script>
<template>
    <div>Dashboard Tab Content</div>
</template>
