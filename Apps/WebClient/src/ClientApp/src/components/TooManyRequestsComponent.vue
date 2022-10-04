<script lang="ts">
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

@Component
export default class TooManyRequestsComponent extends Vue {
    @Prop({ default: "page" })
    location?: string;

    @Action("clearTooManyRequestsWarning", { namespace: "errorBanner" })
    clearTooManyRequestsWarning!: () => void;

    @Action("clearTooManyRequestsError", { namespace: "errorBanner" })
    clearTooManyRequestsError!: () => void;

    @Getter("tooManyRequestsWarning", { namespace: "errorBanner" })
    warning!: string | undefined;

    @Getter("tooManyRequestsError", { namespace: "errorBanner" })
    error!: string | undefined;

    private get isShowingWarning(): boolean {
        return this.warning === this.location;
    }

    private set isShowingWarning(value: boolean) {
        if (value === false) {
            this.clearTooManyRequestsWarning();
        }
    }

    private get isShowingError(): boolean {
        return this.error === this.location;
    }

    private set isShowingError(value: boolean) {
        if (value === false) {
            this.clearTooManyRequestsError();
        }
    }
}
</script>

<template>
    <div>
        <b-alert
            v-model="isShowingWarning"
            data-testid="too-many-requests-warning"
            variant="warning"
            dismissible
            class="no-print"
        >
            We are unable to complete all actions because the site is too busy.
            Please try again later.
        </b-alert>
        <b-alert
            v-model="isShowingError"
            data-testid="too-many-requests-error"
            variant="danger"
            dismissible
            class="no-print"
        >
            Unable to complete action as the site is too busy. Please try again
            later.
        </b-alert>
    </div>
</template>
