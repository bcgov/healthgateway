<script lang="ts">
import { BAlert } from "bootstrap-vue";
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const options: any = { components: { BAlert } };

@Component(options)
export default class AppErrorView extends Vue {
    @Prop({ default: false }) busy!: boolean;
}
</script>

<template>
    <div
        id="app-root"
        class="d-flex align-items-center justify-content-center h-100 p-3 p-md-4"
    >
        <b-alert v-if="busy" show variant="warning" data-testid="app-warning">
            We are unable to complete all actions because the site is too busy.
            Please try again later.
        </b-alert>
        <b-alert v-else show variant="danger" data-testid="app-error">
            Unable to load application.
        </b-alert>
    </div>
</template>
<style lang="scss" scoped>
#app-root {
    min-height: 100vh;
}
</style>
