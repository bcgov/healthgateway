<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faChevronDown, faChevronUp } from "@fortawesome/free-solid-svg-icons";
import Vue from "vue";
import { Component } from "vue-property-decorator";
import { Getter } from "vuex-class";

import BannerError from "@/models/bannerError";

library.add(faChevronDown, faChevronUp);

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
        data-testid="errorBanner"
        variant="danger"
        dismissible
        class="no-print mt-3 mx-2"
        :show="isShowing"
    >
        <h4>Whoops, something went wrong... Try refreshing the page</h4>
        <b-btn
            v-b-toggle.errorDetails
            data-testid="errorDetailsBtn"
            variant="link"
            class="detailsButton"
        >
            <hg-icon
                icon="chevron-down"
                size="medium"
                aria-hidden="true"
                class="when-closed mr-2"
            />
            <span class="when-closed">View Details</span>
            <hg-icon
                icon="chevron-up"
                size="medium"
                aria-hidden="true"
                class="when-opened mr-2"
            />
            <span class="when-opened">Hide Details</span>
        </b-btn>
        <b-collapse id="errorDetails">
            <div v-for="(error, index) in errors" :key="index" class="py-2">
                <h6>{{ error.title }} | {{ error.errorCode }}</h6>
                <div class="pl-4">
                    <p data-testid="errorTextDescription">
                        {{ error.description }}
                    </p>
                    <p data-testid="errorTextDetails">{{ error.detail }}</p>
                    <p v-if="error.traceId" data-testid="errorSupportDetails">
                        If this issue persists, contact HealthGateway@gov.bc.ca
                        and provide {{ error.traceId }}
                    </p>
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
