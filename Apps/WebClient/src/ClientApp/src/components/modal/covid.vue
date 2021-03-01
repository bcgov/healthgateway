<script lang="ts">
import Vue from "vue";
import { Component, Emit, Prop } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import UserPreferenceType from "@/constants/userPreferenceType";
import { DateWrapper } from "@/models/dateWrapper";
import { LaboratoryOrder } from "@/models/laboratory";
import { EntryType } from "@/models/timelineEntry";
import { TimelineFilterBuilder } from "@/models/timelineFilter";
import User from "@/models/user";
import type { UserPreference } from "@/models/userPreference";

@Component
export default class CovidModalComponent extends Vue {
    @Action("updateUserPreference", { namespace: "user" })
    updateUserPreference!: (params: {
        hdid: string;
        userPreference: UserPreference;
    }) => void;

    @Action("createUserPreference", { namespace: "user" })
    createUserPreference!: (params: {
        hdid: string;
        userPreference: UserPreference;
    }) => void;

    @Action("setFilter", { namespace: "timeline" })
    setFilter!: (filterBuilder: TimelineFilterBuilder) => void;

    @Getter("user", { namespace: "user" }) user!: User;

    @Getter("laboratoryOrders", { namespace: "laboratory" })
    laboratoryOrders!: LaboratoryOrder[];

    @Getter("getPreference", { namespace: "user" })
    getPreference!: (preferenceName: string) => UserPreference | undefined;

    @Prop({ default: false }) isLoading!: boolean;

    private isDismissed = false;

    private get isVisible(): boolean {
        if (this.isLoading || this.isDismissed) {
            return false;
        } else if (this.laboratoryOrders.length > 0) {
            const preference = this.getPreference(
                UserPreferenceType.ActionedCovidModalAt
            );
            if (preference != undefined) {
                const actionedCovidModalAt = new DateWrapper(preference.value);
                const mostRecentLabTime = new DateWrapper(
                    this.laboratoryOrders[0].messageDateTime
                );
                if (actionedCovidModalAt.isAfter(mostRecentLabTime)) {
                    return false;
                } else {
                    return true;
                }
            } else {
                return true;
            }
        } else {
            return false;
        }
    }

    private updateCovidPreference() {
        const preferenceName = UserPreferenceType.ActionedCovidModalAt;
        let isoNow = new DateWrapper().toISO();
        if (this.user.preferences[preferenceName] != undefined) {
            this.user.preferences[preferenceName].value = isoNow;
            this.updateUserPreference({
                hdid: this.user.hdid,
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
                hdid: this.user.hdid,
                userPreference: this.user.preferences[preferenceName],
            });
        }
    }

    private handleSubmit(modalEvt: Event) {
        // Prevent modal from closing
        modalEvt.preventDefault();

        this.updateCovidPreference();

        this.setFilter(
            TimelineFilterBuilder.create().withEntryType(EntryType.Laboratory)
        );

        // Trigger submit handler
        this.submit();

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
        @close="handleSubmit"
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
                    <b-button
                        data-testid="covidViewResultBtn"
                        variant="outline-primary"
                        @click="handleSubmit($event)"
                    >
                        View Result
                    </b-button>
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
