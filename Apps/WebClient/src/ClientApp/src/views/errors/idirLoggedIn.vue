<template>
    <div>
        <ErrorComponent :error="errorDescription" />
    </div>
</template>

<script lang="ts">
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";
import PageErrorComponent from "@/components/error.vue";
import PageError from "@/models/pageError";

@Component({
    components: {
        ErrorComponent: PageErrorComponent,
    },
})
export default class IdirLoggedInView extends Vue {
    public errorDescription: PageError = new PageError(
        "403",
        "Not BC Services Card login",
        "You've logged in with your IDIR. Please log out and use your BC Services Card."
    );
}
</script>
