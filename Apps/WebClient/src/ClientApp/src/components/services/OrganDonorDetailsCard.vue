<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faCircleInfo, faDownload } from "@fortawesome/free-solid-svg-icons";
import Vue from "vue";
import { Component } from "vue-property-decorator";

import { OrganDonorRegistrationData } from "@/models/patientData";

library.add(faCircleInfo, faDownload);

//eslint-disable-next-line @typescript-eslint/no-explicit-any
const options: any = {};
@Component(options)
export default class OrganDonorDetailsCard extends Vue {
    private registrationData: OrganDonorRegistrationData = {
        registrationFileId: "undefined",
        status: "Registered",
        statusMessage: "You're registered",
        type: "OrganDonorRegistration",
    };
}
</script>

<template>
    <b-card
        container
        class="hg-card-button h-100 w-100 d-flex flex-column align-content-start text-left rounded shadow border-0"
        header-class="border-0 bg-transparent d-flex"
        v-bind="$attrs"
    >
        <template #header>
            <img
                class="organ-donor-registry-logo align-self-center float-left mr-2 valign-baseline"
                src="@/assets/images/services/odr-logo.svg"
                alt="Organ Donor Registry Logo"
            />
            <h5 class="align-self-end">Organ Donor Registration</h5>
        </template>
        <b-container>
            <b-row class="align-items-center mb-5">
                <span
                    class="label-color mr-2 font-weight-lighter mb-0 fs-donor-label"
                >
                    Status:
                </span>
                <span class="mr-2 font-weight-bolder mb-0 fs-donor-label">{{
                    registrationData.status
                }}</span>
                <hg-icon
                    :id="`organ-donor-registration-info-${registrationData.registrationFileId}`"
                    class="ml-2 info-color"
                    icon="circle-info"
                ></hg-icon>
                <b-popover
                    :target="`organ-donor-registration-info-${registrationData.registrationFileId}`"
                    triggers="hover"
                    placement="top"
                    boundary="viewport"
                    data-testid="organ-donor-registration-info-popover"
                >
                    {{ registrationData.statusMessage }}
                </b-popover>
            </b-row>
            <b-row class="align-items-center mb-5">
                <span
                    class="label-color mr-2 font-weight-lighter mb-0 fs-donor-label"
                >
                    Decision:
                </span>
                <hg-button
                    v-if="registrationData.registrationFileId"
                    variant="secondary"
                    ><hg-icon
                        icon="download"
                        size="medium"
                        square
                        aria-hidden="true"
                    />
                    Download
                </hg-button>
                <span v-else class="mr-2 font-weight-bolder mb-0 fs-donor-label"
                    >Not Available</span
                >
            </b-row>
            <b-row>
                <a
                    href="http://www.transplant.bc.ca/Pages/Register-your-Decision.aspx"
                    target="_blank"
                >
                    Register or update your decision
                </a>
            </b-row>
        </b-container>
    </b-card>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.fs-donor-label {
    font-size: 0.9em;
}

.info-color {
    color: $primary !important;
}

.label-color {
    color: grey;
}
</style>
