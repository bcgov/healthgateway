<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";

import PageErrorComponent from "@/components/PageErrorComponent.vue";
import { PageError } from "@/models/errors";

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const options: any = {
    components: {
        PageErrorComponent,
    },
};

@Component(options)
export default class IdirLoggedInView extends Vue {
    public errorDescription = new PageError(
        "403",
        "IDIR Login",
        "You've logged in with your IDIR. Please log out and use your BC Services Card."
    );
}
</script>

<template>
    <PageErrorComponent :error="errorDescription" />
</template>
