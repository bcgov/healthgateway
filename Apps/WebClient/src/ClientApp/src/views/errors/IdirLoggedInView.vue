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
export default class IdirLoggedInView extends Vue {}
</script>

<template>
    <PageErrorComponent title="403" subtitle="IDIR Login">
        <p>
            You’ve logged in with your IDIR. Please log out and use your BC
            Services Card.
        </p>
    </PageErrorComponent>
</template>
