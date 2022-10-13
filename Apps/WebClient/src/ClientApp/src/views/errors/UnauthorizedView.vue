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
export default class UnauthorizedView extends Vue {}
</script>

<template>
    <PageErrorComponent title="401" subtitle="Unauthorized">
        <p>You do not have permission to view this page.</p>
    </PageErrorComponent>
</template>
