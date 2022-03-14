<script lang="ts">
import Vue from "vue";
import { Component, Emit, Prop } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import { EntryType } from "@/constants/entryType";
import UserPreferenceType from "@/constants/userPreferenceType";
import { DateWrapper } from "@/models/dateWrapper";
import { Covid19LaboratoryOrder } from "@/models/laboratory";
import { TimelineFilterBuilder } from "@/models/timelineFilter";
import User from "@/models/user";
import type { UserPreference } from "@/models/userPreference";

@Component
export default class CovidTestModalComponent extends Vue {
    @Action("updateUserPreference", { namespace: "user" })
    updateUserPreference!: (params: { userPreference: UserPreference }) => void;

    @Action("createUserPreference", { namespace: "user" })
    createUserPreference!: (params: { userPreference: UserPreference }) => void;

    @Action("setFilter", { namespace: "timeline" })
    setFilter!: (filterBuilder: TimelineFilterBuilder) => void;

    @Getter("user", { namespace: "user" }) user!: User;

    @Getter("covid19LaboratoryOrders", { namespace: "laboratory" })
    covid19LaboratoryOrders!: Covid19LaboratoryOrder[];

    @Getter("getPreference", { namespace: "user" })
    getPreference!: (preferenceName: string) => UserPreference | undefined;

    @Prop({ default: false }) isLoading!: boolean;

    private isDismissed = false;

    private get isVisible(): boolean {
        if (this.isLoading || this.isDismissed) {
            return false;
        }
        if (this.covid19LaboratoryOrders.length > 0) {
            const preference = this.getPreference(
                UserPreferenceType.ActionedCovidModalAt
            );
            if (preference != undefined) {
                const actionedCovidModalAt = new DateWrapper(preference.value);
                const mostRecentLabTime = new DateWrapper(
                    this.covid19LaboratoryOrders[0].messageDateTime
                );
                return !actionedCovidModalAt.isAfter(mostRecentLabTime);
            } else {
                return true;
            }
        }

        return false;
    }

    private updateCovidPreference() {
        const preferenceName = UserPreferenceType.ActionedCovidModalAt;
        let isoNow = new DateWrapper().toISO();
        if (this.user.preferences[preferenceName] != undefined) {
            this.user.preferences[preferenceName].value = isoNow;
            this.updateUserPreference({
                userPreference: this.user.preferences[preferenceName],
            });
        } else {
            this.user.preferences[preferenceName] = {
                hdId: this.user.hdid,
                preference: preferenceName,
                value: isoNow,
                version: 0,
                createdDateTime: new DateWrapper().toISO(),
            };
            this.createUserPreference({
                userPreference: this.user.preferences[preferenceName],
            });
        }
    }

    private handleSubmit(modalEvt: Event) {
        // Prevent modal from closing
        modalEvt.preventDefault();

        this.updateCovidPreference();

        this.setFilter(
            TimelineFilterBuilder.create().withEntryType(
                EntryType.Covid19LaboratoryOrder
            )
        );

        // Trigger submit handler
        this.submit();

        // Hide the modal manually
        this.$nextTick(() => {
            this.isDismissed = true;
        });
    }

    private handleCancel(modalEvt: Event) {
        // Prevent modal from closing
        modalEvt.preventDefault();

        this.updateCovidPreference();

        // Trigger cancel handler
        this.cancel();

        // Hide the modal manually
        this.$nextTick(() => {
            this.isDismissed = true;
        });
    }

    @Emit()
    private submit() {
        return;
    }

    @Emit()
    private cancel() {
        return;
    }
}
</script>

<template>
    <b-modal
        id="covid-modal"
        v-model="isVisible"
        data-testid="covidModal"
        title="COVID-19"
        header-bg-variant="danger"
        header-text-variant="light"
        footer-class="modal-footer"
        :no-close-on-backdrop="true"
        centered
        @close="handleCancel"
    >
        <form @submit.stop.prevent="handleSubmit">
            <b-row data-testid="covidModalText">
                <b-col>
                    <span
                        >Check the status of your COVID-19 test and view your
                        result when it is available</span
                    >
                </b-col>
            </b-row>
        </form>
        <template #modal-footer>
            <b-row>
                <b-col>
                    <hg-button
                        data-testid="covidViewResultBtn"
                        variant="secondary"
                        @click="handleSubmit($event)"
                    >
                        View Result
                    </hg-button>
                </b-col>
            </b-row>
        </template>
    </b-modal>
</template>

<style lang="scss" scoped>
.modal-footer {
    justify-content: flex-end;
}
</style>
