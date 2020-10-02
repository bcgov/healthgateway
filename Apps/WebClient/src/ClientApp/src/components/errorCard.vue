<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";
import { Getter } from "vuex-class";
import BannerError from "@/models/bannerError";

@Component
export default class ErrorCardComponent extends Vue {
    @Getter("isShowing", { namespace: "errorBanner" })
    isShowing!: boolean;

    @Getter("errors", { namespace: "errorBanner" })
    errors!: BannerError[];
}
</script>

<template>
    <b-alert
        variant="danger"
        dismissible
        class="no-print mt-3"
        :show="isShowing"
    >
        <h4>Whoops, something went wrong... Try refreshing the page</h4>
        <b-btn v-b-toggle.errorDetails variant="link" class="detailsButton">
            <span class="when-opened">
                <font-awesome-icon
                    icon="chevron-up"
                    aria-hidden="true"
                ></font-awesome-icon
            ></span>
            <span class="when-closed">
                <font-awesome-icon
                    icon="chevron-down"
                    aria-hidden="true"
                ></font-awesome-icon
            ></span>
            <span class="when-closed">View Details</span>
            <span class="when-opened">Hide Details</span>
        </b-btn>
        <b-collapse id="errorDetails">
            <div v-for="(error, index) in errors" :key="index" class="py-2">
                <h6>{{ error.title }} | {{ error.errorCode }}</h6>
                <div class="pl-4">
                    <p>{{ error.description }}</p>
                    <p>{{ error.detail }}</p>
                </div>
            </div>
        </b-collapse>
    </b-alert>
</template>

<style lang="scss" scoped>
.collapsed > .when-opened,
:not(.collapsed) > .when-closed {
    display: none;
}
</style>
