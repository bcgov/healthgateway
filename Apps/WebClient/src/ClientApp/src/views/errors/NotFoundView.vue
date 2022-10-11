<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";

import PageErrorComponent from "@/components/PageErrorComponent.vue";

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const options: any = {
    components: {
        PageErrorComponent,
    },
};

@Component(options)
export default class NotFoundView extends Vue {}
</script>

<template>
    <PageErrorComponent title="404" subtitle="Page Not Found">
        <p>The page you were looking for does not exist.</p>
    </PageErrorComponent>
</template>
