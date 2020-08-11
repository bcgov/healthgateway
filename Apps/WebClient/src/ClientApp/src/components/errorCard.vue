<style lang="scss" scoped>
.collapsed > .when-opened,
:not(.collapsed) > .when-closed {
    display: none;
}
</style>

<template>
    <b-alert
        variant="danger"
        dismissible
        class="no-print mt-3"
        :show="isShowing"
    >
        <h4>Whoops, something went wrong</h4>
        <div v-for="(error, index) in errors" :key="index" class="py-2">
            <h6>{{ error.title }}</h6>
            <span>Error: {{ error.errorCode }}</span>
            <b-btn
                v-b-toggle="'errorDetail-' + index"
                variant="link"
                class="detailsButton"
            >
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
                <span class="when-opened">Hide</span>
            </b-btn>
            <b-collapse :id="'errorDetail-' + index" class="pl-4">
                <p>{{ error.description }}</p>
                <p>{{ error.detail }}</p>
            </b-collapse>
        </div>
    </b-alert>
</template>

<script lang="ts">
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";
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
