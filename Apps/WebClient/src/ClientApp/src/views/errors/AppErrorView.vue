<script lang="ts">
import { BAlert } from "bootstrap-vue";
import Vue from "vue";
import { Component } from "vue-property-decorator";
import { Getter } from "vuex-class";

import { AppErrorType } from "@/constants/errorType";

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const options: any = { components: { BAlert } };

@Component(options)
export default class AppErrorView extends Vue {
    @Getter("appError")
    appError?: AppErrorType;

    private get isTooManyRequests(): boolean {
        return this.appError === AppErrorType.TooManyRequests;
    }
}
</script>

<template>
    <div
        class="flex-grow-1 d-flex align-items-center justify-content-center p-3 p-md-4"
    >
        <b-alert
            v-if="isTooManyRequests"
            show
            variant="warning"
            data-testid="app-warning"
        >
            We are unable to complete all actions because the site is too busy.
            Please try again later.
        </b-alert>
        <b-alert v-else show variant="danger" data-testid="app-error">
            Unable to load application. Please try refreshing the page or come
            back later.
        </b-alert>
    </div>
</template>
