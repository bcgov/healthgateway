<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";

import PageErrorComponent from "@/components/PageErrorComponent.vue";
import { PageError } from "@/models/errors";

@Component({
    components: {
        PageErrorComponent,
    },
})
export default class IdirLoggedInView extends Vue {
    public errorDescription: PageError = new PageError(
        "403",
        "IDIR Login",
        "You've logged in with your IDIR. Please log out and use your BC Services Card."
    );
}
</script>

<template>
    <PageErrorComponent :error="errorDescription" />
</template>
