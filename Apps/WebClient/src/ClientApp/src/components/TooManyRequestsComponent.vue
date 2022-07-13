<script lang="ts">
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";
import { Getter } from "vuex-class";

@Component
export default class TooManyRequestsComponent extends Vue {
    @Prop({ default: "page" })
    location?: string;

    @Getter("tooManyRequestsWarning", { namespace: "errorBanner" })
    warning!: boolean;

    @Getter("tooManyRequestsError", { namespace: "errorBanner" })
    error!: string | undefined;

    private get showWarning(): boolean {
        return this.warning === true;
    }

    private get showError(): boolean {
        return this.error === this.location;
    }
}
</script>

<template>
    <div>
        <b-alert
            data-testid="too-many-requests-warning"
            variant="warning"
            dismissible
            class="no-print"
            :show="showWarning"
        >
            We are unable to get your health records because the site is too
            busy. Please try again later.
        </b-alert>
        <b-alert
            data-testid="too-many-requests-error"
            variant="danger"
            dismissible
            class="no-print"
            :show="showError"
        >
            Unable to complete action as the site is too busy. Please try again
            later.
        </b-alert>
    </div>
</template>
