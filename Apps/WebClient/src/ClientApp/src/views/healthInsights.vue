<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";
.column-wrapper {
    border: 1px;
}

#pageTitle {
    color: $primary;
}

#pageTitle hr {
    border-top: 2px solid $primary;
}
</style>
<template>
    <div>
        <TimelineLoadingComponent v-if="isLoading"></TimelineLoadingComponent>
        <b-row class="my-3 fluid justify-content-md-center">
            <b-col
                id="healthInsights"
                class="col-12 col-md-10 col-lg-9 column-wrapper"
            >
                <ErrorCard
                    title="Whoops!"
                    description="An error occurred."
                    :show="hasErrors"
                />
                <div id="pageTitle">
                    <h1 id="subject">Health Insights</h1>
                    <hr />
                </div>

                <!-- 
                    <HealthInsightsComponent 
                        :entries="timelineEntries"
                    />
                -->
            </b-col>
        </b-row>
        <ProtectiveWordComponent
            ref="protectiveWordModal"
            :error="protectiveWordAttempts > 1"
            :is-loading="isLoading"
            @submit="onProtectiveWordSubmit"
            @cancel="onProtectiveWordCancel"
        />
    </div>
</template>

<script lang="ts">
import Vue from "vue";
import { Component, Ref, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";
import moment from "moment";
import { Route } from "vue-router";
import { WebClientConfiguration } from "@/models/configData";
import {
    ILogger,
    IImmunizationService,
    ILaboratoryService,
    IMedicationService,
} from "@/services/interfaces";
import container from "@/plugins/inversify.config";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ResultType } from "@/constants/resulttype";
import User from "@/models/user";
import TimelineEntry from "@/models/timelineEntry";
import MedicationTimelineEntry from "@/models/medicationTimelineEntry";
import ImmunizationTimelineEntry from "@/models/immunizationTimelineEntry";
import LaboratoryTimelineEntry from "@/models/laboratoryTimelineEntry";
import MedicationStatementHistory from "../models/medicationStatementHistory";
import RequestResult from "@/models/requestResult";
import {
    LaboratoryOrder,
    LaboratoryReport,
    LaboratoryResult,
} from "@/models/laboratory";

import TimelineLoadingComponent from "@/components/timelineLoading.vue";
import ProtectiveWordComponent from "@/components/modal/protectiveWord.vue";
import ErrorCardComponent from "@/components/errorCard.vue";

const namespace: string = "user";

@Component({
    components: {
        TimelineLoadingComponent,
        ProtectiveWordComponent,
        ErrorCard: ErrorCardComponent,
    },
})
export default class HealthInsightsView extends Vue {
    private logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    @Getter("user", { namespace }) user!: User;
    @Action("getOrders", { namespace: "laboratory" })
    getLaboratoryOrders!: (params: {
        hdid: string;
    }) => Promise<RequestResult<LaboratoryOrder[]>>;
    @Getter("webClient", { namespace: "config" })
    config!: WebClientConfiguration;

    private timelineEntries: TimelineEntry[] = [];
    private isMedicationLoading: boolean = false;
    private isImmunizationLoading: boolean = false;
    private isLaboratoryLoading: boolean = false;
    private hasErrors: boolean = false;
    private protectiveWordAttempts: number = 0;

    @Ref("protectiveWordModal")
    readonly protectiveWordModal!: ProtectiveWordComponent;

    private mounted() {
        this.fetchMedicationStatements();
        this.fetchImmunizations();
        this.fetchLaboratoryResults();
    }

    private get isLoading(): boolean {
        return (
            this.isMedicationLoading ||
            this.isImmunizationLoading ||
            this.isLaboratoryLoading
        );
    }

    private get isMedicationEnabled(): boolean {
        return this.config.modules["Medication"];
    }

    private get isImmunizationEnabled(): boolean {
        return this.config.modules["Immunization"];
    }

    private get isLaboratoryEnabled(): boolean {
        return this.config.modules["Laboratory"];
    }

    private fetchMedicationStatements(protectiveWord?: string) {
        const medicationService: IMedicationService = container.get(
            SERVICE_IDENTIFIER.MedicationService
        );
        this.isMedicationLoading = true;
        let promise: Promise<RequestResult<MedicationStatementHistory[]>>;

        promise = medicationService.getPatientMedicationStatementHistory(
            this.user.hdid,
            protectiveWord
        );

        promise
            .then((results) => {
                if (results.resultStatus == ResultType.Success) {
                    this.protectiveWordAttempts = 0;
                    // Add the medication entries to the timeline list
                    for (let result of results.resourcePayload) {
                        this.timelineEntries.push(
                            new MedicationTimelineEntry(result)
                        );
                    }
                } else if (results.resultStatus == ResultType.Protected) {
                    this.protectiveWordModal.showModal();
                    this.protectiveWordAttempts++;
                } else {
                    this.logger.error(
                        "Error returned from the medication statements call: " +
                            results.resultError?.resultMessage
                    );
                    this.hasErrors = true;
                }
            })
            .catch((err) => {
                this.hasErrors = true;
                this.logger.error(err);
            })
            .finally(() => {
                this.isMedicationLoading = false;
            });
    }

    private fetchImmunizations() {
        const immunizationService: IImmunizationService = container.get(
            SERVICE_IDENTIFIER.ImmunizationService
        );
        this.isImmunizationLoading = true;
        immunizationService
            .getPatientImmunizations(this.user.hdid)
            .then((results) => {
                if (results.resultStatus == ResultType.Success) {
                    // Add the immunization entries to the timeline list
                    for (let result of results.resourcePayload) {
                        this.timelineEntries.push(
                            new ImmunizationTimelineEntry(result)
                        );
                    }
                } else {
                    this.logger.error(
                        "Error returned from the immunization call: " +
                            results.resultError?.resultMessage
                    );
                    this.hasErrors = true;
                }
            })
            .catch((err) => {
                this.hasErrors = true;
                this.logger.error(err);
            })
            .finally(() => {
                this.isImmunizationLoading = false;
            });
    }

    private fetchLaboratoryResults() {
        const laboratoryService: ILaboratoryService = container.get(
            SERVICE_IDENTIFIER.LaboratoryService
        );
        this.isLaboratoryLoading = true;
        this.getLaboratoryOrders({ hdid: this.user.hdid })
            .then((results) => {
                if (results.resultStatus == ResultType.Success) {
                    // Add the laboratory entries to the timeline list
                    for (let result of results.resourcePayload) {
                        this.timelineEntries.push(
                            new LaboratoryTimelineEntry(result)
                        );
                    }

                    if (results.resourcePayload.length > 0) {
                        this.protectiveWordModal.hideModal();
                    }
                } else {
                    this.logger.error(
                        "Error returned from the laboratory call: " +
                            results.resultError?.resultMessage
                    );
                    this.hasErrors = true;
                }
            })
            .catch((err) => {
                this.hasErrors = true;
                this.logger.error(err);
            })
            .finally(() => {
                this.isLaboratoryLoading = false;
            });
    }

    private onProtectiveWordSubmit(value: string) {
        this.fetchMedicationStatements(value);
    }

    private onProtectiveWordCancel() {
        // Does nothing as it won't be able to fetch pharmanet data.
        this.logger.debug("protective word cancelled");
    }
}
</script>
