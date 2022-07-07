<script lang="ts">
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";

import { PageError } from "@/models/errors";

@Component
export default class PageErrorComponent extends Vue {
    @Prop(PageError) error!: PageError;
}
</script>

<template>
    <div class="col d-flex justify-content-center">
        <div class="error-template">
            <h1>{{ error.code }}</h1>
            <h2>{{ error.name }}</h2>
            <p>{{ error.message }}</p>
        </div>
    </div>
</template>

<style lang="scss" scoped>
.error-template {
    padding: 40px 15px;
}
</style>
