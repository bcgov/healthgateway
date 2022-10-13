<script lang="ts">
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";

@Component
export default class PageErrorComponent extends Vue {
    @Prop({ required: true })
    title!: string;

    @Prop({ required: false })
    subtitle?: string;
}
</script>

<template>
    <div class="d-flex justify-content-center">
        <div class="px-3 py-5">
            <h1>{{ title }}</h1>
            <h2 v-if="subtitle">{{ subtitle }}</h2>
            <slot />
        </div>
    </div>
</template>
