<script lang="ts">
import { BAlert } from "bootstrap-vue";
import Vue from "vue";
import { Component } from "vue-property-decorator";

@Component({ components: { BAlert } })
export default class AppErrorView extends Vue {}
</script>

<template>
    <div
        id="app-root"
        class="d-flex align-items-center justify-content-center h-100 p-3 p-md-4"
    >
        <b-alert show variant="danger" data-testid="app-error">
            Unable to load application.
        </b-alert>
    </div>
</template>
<style lang="scss" scoped>
#app-root {
    min-height: 100vh;
}
</style>
