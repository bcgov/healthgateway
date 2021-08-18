<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import {
    faCheckCircle,
    faEye,
    faEyeSlash,
} from "@fortawesome/free-solid-svg-icons";
import Vue from "vue";
import { Component } from "vue-property-decorator";
import { Getter } from "vuex-class";

import { VaccinationState } from "@/constants/vaccinationState";
import { DateWrapper, StringISODate } from "@/models/dateWrapper";
import VaccinationStatus from "@/models/vaccinationStatus";

library.add(faCheckCircle, faEye, faEyeSlash);

@Component
export default class VaccinationStatusResultView extends Vue {
    private showDetails = false;

    @Getter("vaccinationStatus", { namespace: "vaccinationStatus" }) status!:
        | VaccinationStatus
        | undefined;

    private get vaccinationState(): VaccinationState | undefined {
        return this.status?.state;
    }

    private get vaccinationDoses(): number | undefined {
        return this.status?.doses;
    }

    private get isFullyVaccinated(): boolean {
        return this.vaccinationState === VaccinationState.FullyVaccinated;
    }

    private get isPartiallyVaccinated(): boolean {
        return this.vaccinationState === VaccinationState.PartiallyVaccinated;
    }

    private get isVaccinationNotFound(): boolean {
        return this.vaccinationState === VaccinationState.NotFound;
    }

    private get name(): string {
        return [this.status?.firstname ?? "", this.status?.lastname ?? ""]
            .filter((name) => name?.length > 0)
            .join(" ");
    }

    private get dateOfBirth(): string {
        return this.formatDate(this.status?.birthdate ?? null);
    }

    private toggleDetails() {
        this.showDetails = !this.showDetails;
    }

    private formatDate(date: StringISODate | null): string {
        if (!date) {
            return "";
        }
        return new DateWrapper(date).format(DateWrapper.defaultFormat);
    }
}
</script>

<template>
    <div class="d-flex flex-column flex-grow-1">
        <div class="header text-white">
            <div class="container pb-3">
                <h1 class="text-center">COVID-19 Vaccination Status</h1>
                <h4 class="text-center">Government of BC</h4>
                <hr style="border-top: 2px solid #fcba19" />
                <div class="mb-2 text-center">
                    <hg-button variant="secondary" @click="toggleDetails">
                        <span v-if="showDetails">Hide Details</span>
                        <span v-else>Show Details</span>
                        <hg-icon
                            :icon="showDetails ? 'eye-slash' : 'eye'"
                            class="ml-2"
                            square
                        />
                    </hg-button>
                </div>
                <b-row>
                    <b-col>
                        <b-form-group label="Name" label-for="name">
                            <b-form-input id="name" v-model="name" disabled />
                        </b-form-group>
                    </b-col>
                    <b-col v-show="showDetails" cols="12" md="3">
                        <b-form-group
                            label="Date of Birth"
                            label-for="dateOfBirth"
                        >
                            <b-form-input
                                id="dateOfBirth"
                                v-model="dateOfBirth"
                                disabled
                            />
                        </b-form-group>
                    </b-col>
                </b-row>
                <b-row v-show="showDetails" class="justify-content-center">
                    <b-col cols="auto">
                        <img
                            v-if="isFullyVaccinated"
                            src="@/assets/images/vaccination-status/fully-vaccinated.svg"
                            alt="Fully Vaccinated"
                            class="vaccination-stage"
                        />
                        <img
                            v-else-if="
                                isPartiallyVaccinated && vaccinationDoses === 2
                            "
                            src="@/assets/images/vaccination-status/dose-2.svg"
                            alt="Two Doses"
                            class="vaccination-stage"
                        />
                        <img
                            v-else-if="
                                isPartiallyVaccinated && vaccinationDoses === 1
                            "
                            src="@/assets/images/vaccination-status/dose-1.svg"
                            alt="One Dose"
                            class="vaccination-stage"
                        />
                        <img
                            v-else
                            src="@/assets/images/vaccination-status/no-doses.svg"
                            alt="No Doses"
                            class="vaccination-stage"
                        />
                    </b-col>
                </b-row>
            </div>
        </div>
        <div
            class="
                vaccination-indicator
                p-3
                text-white
                flex-grow-1
                d-flex
                align-items-center
                justify-content-between
                flex-column
            "
            :class="{
                'fully-vaccinated': isFullyVaccinated,
                'partially-vaccinated': isPartiallyVaccinated,
                'not-found': isVaccinationNotFound,
            }"
        >
            <div class="vaccination-box m-4 p-5 d-inline-block text-center">
                <hg-icon
                    v-show="isFullyVaccinated"
                    icon="check-circle"
                    class="mb-2"
                />
                <h1 v-if="isFullyVaccinated">Vaccinated</h1>
                <h1 v-else-if="isPartiallyVaccinated">
                    <div>Partially</div>
                    <div>Vaccinated</div>
                </h1>
                <h1 v-else-if="isVaccinationNotFound">Not Found</h1>
            </div>
            <div>
                <hg-button variant="secondary" to="/">Done</hg-button>
                <hg-button
                    v-if="isPartiallyVaccinated || isFullyVaccinated"
                    variant="primary"
                    class="ml-3"
                >
                    Save for Later
                </hg-button>
            </div>
        </div>
    </div>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.header {
    background-color: $hg-brand-primary;
}

.vaccination-indicator {
    &.fully-vaccinated {
        background-color: $hg-state-success;
        .vaccination-box {
            border: 8px solid white;

            .hg-icon {
                font-size: 2.5rem;
            }
        }
    }
    &.partially-vaccinated {
        background-color: $hg-background-navigation;
        .vaccination-box {
            border: 5px dashed white;
        }
    }
    &.not-found {
        background-color: #6c757d;
        .vaccination-box {
            border: 5px dashed white;
        }
    }
}

img.vaccination-stage {
    height: 4.5em;
}
</style>
