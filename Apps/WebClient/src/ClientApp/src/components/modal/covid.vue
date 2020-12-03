<script lang="ts">
import Vue from "vue";
import { Component, Emit, Prop, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";
import User from "@/models/user";
import container from "@/plugins/inversify.config";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";
import { DateWrapper } from "@/models/dateWrapper";
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
    @Getter("user", { namespace: "user" }) user!: User;

    @Prop() error!: boolean;
    @Prop({ default: false }) isLoading!: boolean;

    private logger!: ILogger;

    private mounted() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    }

    public isVisible = false;
    public show = false;

    private actionCovidModal() {
        this.logger.debug("Actioning Covid Modal...");
        let isoNow = new DateWrapper().toISO();
        if (this.user.preferences.actionedCovidModalAt != undefined) {
            this.user.preferences.actionedCovidModalAt.value = isoNow;
            this.updateUserPreference({
                hdid: this.user.hdid,
                userPreference: this.user.preferences.actionedCovidModalAt,
            });
        } else {
            this.user.preferences.actionedCovidModalAt = {
                hdId: this.user.hdid,
                preference: "actionedCovidModalAt",
                value: isoNow,
                version: 0,
                createdDateTime: new DateWrapper().toISO(),
            };
            this.createUserPreference({
                hdid: this.user.hdid,
                userPreference: this.user.preferences.actionedCovidModalAt,
            });
        }
    }

    public showModal(): void {
        this.show = true;
        if (!this.isLoading) {
            this.isVisible = true;
        }
    }

    public hideModal(): void {
        this.show = false;
        this.isVisible = false;
    }

    @Watch("isLoading")
    private onIsLoading() {
        if (!this.isLoading && this.show) {
            this.isVisible = true;
        }
    }

    @Emit()
    private submit() {
        this.show = false;
        this.isVisible = false;
        return;
    }

    @Emit()
    private cancel() {
        this.actionCovidModal();
        this.hideModal();
        return;
    }

    private handleSubmit(bvModalEvt: Event) {
        // Prevent modal from closing
        bvModalEvt.preventDefault();

        this.actionCovidModal();

        // Trigger submit handler
        this.submit();

        // Hide the modal manually
        this.$nextTick(() => {
            this.hideModal();
        });
    }

    private handleCancel(bvModalEvt: Event) {
        // Prevent modal from closing
        bvModalEvt.preventDefault();

        // Trigger cancel handler
        this.cancel();

        // Hide the modal manually
        this.$nextTick(() => {
            this.hideModal();
        });
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
        @close="cancel"
    >
        <b-row>
            <b-col>
                <form @submit.stop.prevent="handleSubmit">
                    <b-row data-testid="covidModalText">
                        <b-col>
                            <span
                                >Check the status of your COVID-19 test and view
                                your result when it is available</span
                            >
                        </b-col>
                    </b-row>
                </form>
            </b-col>
        </b-row>
        <template #modal-footer>
            <b-row>
                <b-col>
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
